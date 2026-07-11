using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FabricCsharp.Transpiler.Pipeline;

/// <summary>
/// Manages the Gradle build system for the generated Java project.
/// Generates Gradle wrapper, build.gradle, and settings.gradle, then invokes the build.
/// </summary>
public class GradleRunner
{
    public string ProjectDirectory { get; set; } = "build";
    public string JavaHome { get; set; } = string.Empty;
    public string MinecraftVersion { get; set; } = "1.21.4";
    public string FabricLoaderVersion { get; set; } = "0.16.10";
    public string FabricApiVersion { get; set; } = "0.114.3+1.21.5";
    public string MappingsType { get; set; } = "yarn"; // "yarn" or "mojang"
    public string YarnBuild { get; set; } = "1.21.4+build.1";
    public string ModId { get; set; } = "unknown";
    public string ModVersion { get; set; } = "0.1.0";

    /// <summary>
    /// Generates all Gradle build files for the mod project.
    /// </summary>
    public async Task GenerateBuildFilesAsync(CancellationToken ct = default)
    {
        Directory.CreateDirectory(ProjectDirectory);

        await GenerateBuildGradleAsync(ct);
        await GenerateSettingsGradleAsync(ct);
        await GenerateGradlePropertiesAsync(ct);
    }

    /// <summary>
    /// Runs the Gradle build to produce the final JAR file.
    /// </summary>
    public async Task<GradleResult> BuildAsync(CancellationToken ct = default)
    {
        var result = new GradleResult();

        // First ensure Gradle wrapper exists
        await EnsureGradleWrapperAsync(ct);

        // Run gradlew build
        var processInfo = new ProcessStartInfo
        {
            FileName = GetGradlewPath(),
            Arguments = "build --no-daemon",
            WorkingDirectory = ProjectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(JavaHome))
            processInfo.Environment["JAVA_HOME"] = JavaHome;

        using var process = new Process { StartInfo = processInfo };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null)
                outputBuilder.AppendLine(args.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null)
                errorBuilder.AppendLine(args.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(ct);

        result.Success = process.ExitCode == 0;
        result.ExitCode = process.ExitCode;
        result.StandardOutput = outputBuilder.ToString();
        result.StandardError = errorBuilder.ToString();

        if (result.Success)
        {
            // Find the output JAR
            var jarDir = Path.Combine(ProjectDirectory, "build", "libs");
            if (Directory.Exists(jarDir))
            {
                var jars = Directory.GetFiles(jarDir, "*.jar")
                    .Where(f => !f.EndsWith("-sources.jar") && !f.EndsWith("-dev.jar"))
                    .ToList();
                if (jars.Count > 0)
                    result.OutputJarPath = jars.First();
            }
        }

        return result;
    }

    private async Task GenerateBuildGradleAsync(CancellationToken ct)
    {
        var mappingsContent = MappingsType switch
        {
            "mojang" => $"mappings \"net.fabricmc:yarn:{YarnBuild}:v2\"\n\tmappings loom.officialMojangMappings()",
            _ => $"mappings \"net.fabricmc:yarn:{YarnBuild}:v2\""
        };

        var buildGradle = $@"plugins {{
    id 'fabric-loom' version '1.10-SNAPSHOT'
    id 'maven-publish'
}}

version = project.mod_version
group = project.maven_group

base {{
    archivesName = project.archives_base_name
}}

repositories {{
    // FabricCsharp runtime helper library would go here
}}

loom {{
    splitEnvironmentSourceSets()

    mods {{
        ""{ModId}"" {{
            sourceSet sourceSets.main
            sourceSet sourceSets.client
        }}
    }}
}}

dependencies {{
    minecraft ""com.mojang:minecraft:{MinecraftVersion}""
    {mappingsContent}
    modImplementation ""net.fabricmc:fabric-loader:{FabricLoaderVersion}""
    modImplementation ""net.fabricmc.fabric-api:fabric-api:{FabricApiVersion}""
}}

processResources {{
    inputs.property ""version"", project.version

    filesMatching(""fabric.mod.json"") {{
        expand ""version"": project.version
    }}
}}

tasks.withType(JavaCompile).configureEach {{
    options.encoding = ""UTF-8""
    options.release = 21
}}

java {{
    withSourcesJar()
    sourceCompatibility = JavaVersion.VERSION_21
    targetCompatibility = JavaVersion.VERSION_21
}}

jar {{
    from(""LICENSE"") {{
        rename {{ ""${{it}}_{{project.base.archivesName.get()}}"" }}
    }}
}}
";

        await File.WriteAllTextAsync(
            Path.Combine(ProjectDirectory, "build.gradle"),
            buildGradle, ct);
    }

    private async Task GenerateSettingsGradleAsync(CancellationToken ct)
    {
        var settings = $@"pluginManagement {{
    repositories {{
        maven {{
            name = 'Fabric'
            url = 'https://maven.fabricmc.net/'
        }}
        mavenCentral()
        gradlePluginPortal()
    }}
}}

rootProject.name = '{ModId}'
";

        await File.WriteAllTextAsync(
            Path.Combine(ProjectDirectory, "settings.gradle"),
            settings, ct);
    }

    private async Task GenerateGradlePropertiesAsync(CancellationToken ct)
    {
        var props = $@"org.gradle.jvmargs=-Xmx2G -XX:+UseZGC
org.gradle.parallel=true
org.gradle.caching=true

minecraft_version={MinecraftVersion}
yarn_mappings={YarnBuild}
loader_version={FabricLoaderVersion}
fabric_version={FabricApiVersion}

mod_version={ModVersion}
maven_group=com.{ModId}
archives_base_name={ModId}
";

        await File.WriteAllTextAsync(
            Path.Combine(ProjectDirectory, "gradle.properties"),
            props, ct);
    }

    private string GetGradlewPath()
    {
        var isWindows = OperatingSystem.IsWindows();
        var scriptName = isWindows ? "gradlew.bat" : "gradlew";
        return Path.Combine(ProjectDirectory, scriptName);
    }

    private async Task EnsureGradleWrapperAsync(CancellationToken ct)
    {
        var gradlewPath = GetGradlewPath();
        if (File.Exists(gradlewPath))
            return;

        // Generate a minimal Gradle wrapper by running gradle wrapper
        // We create a temporary settings.gradle to make this work
        var tempSettings = Path.Combine(ProjectDirectory, "settings.gradle");
        if (!File.Exists(tempSettings))
        {
            await File.WriteAllTextAsync(tempSettings, "rootProject.name = 'temp'", ct);
        }

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "gradle",
                Arguments = "wrapper --gradle-version 8.13",
                WorkingDirectory = ProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!string.IsNullOrEmpty(JavaHome))
                processInfo.Environment["JAVA_HOME"] = JavaHome;

            using var process = Process.Start(processInfo);
            if (process != null)
                await process.WaitForExitAsync(ct);
        }
        catch (FileNotFoundException)
        {
            // Gradle not installed — the user needs to have Gradle available
            throw new InvalidOperationException(
                "Gradle is not installed or not on PATH. " +
                "Please install Gradle 8.0+ and ensure 'gradle' is on your PATH.");
        }
    }
}

/// <summary>
/// Result of a Gradle build operation.
/// </summary>
public class GradleResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public string? OutputJarPath { get; set; }
}
