using System.Text.Json;
using FabricCsharp.Api;
using FabricCsharp.CodeGen.Models;

namespace FabricCsharp.CodeGen.Generators;

/// <summary>
/// Generates the fabric.mod.json file from extracted mod metadata.
/// </summary>
public class ModJsonGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Generates fabric.mod.json content from mod metadata.
    /// </summary>
    public string Generate(ModMetadata metadata, string minecraftVersion, string fabricLoaderVersion)
    {
        var model = new ModJsonModel
        {
            Id = metadata.Id,
            Name = metadata.Name,
            Version = "${version}", // Uses Gradle property expansion
            Description = metadata.Description,
            Authors = metadata.Authors,
            Contributors = metadata.Contributors,
            License = metadata.License,
            Icon = metadata.Icon,
            Environment = metadata.Environment,
        };

        // Build entrypoints
        var entrypoints = new Dictionary<string, string[]>();

        if (!string.IsNullOrEmpty(metadata.MainClass))
        {
            entrypoints["main"] = new[] { $"{metadata.Id}.{metadata.MainClass}" };
        }

        if (!string.IsNullOrEmpty(metadata.ClientClass))
        {
            entrypoints["client"] = new[] { $"{metadata.Id}.{metadata.ClientClass}" };
        }

        if (!string.IsNullOrEmpty(metadata.ServerClass))
        {
            entrypoints["server"] = new[] { $"{metadata.Id}.{metadata.ServerClass}" };
        }

        if (entrypoints.Count > 0)
            model.Entrypoints = entrypoints;

        // Mixins (placeholder — will be populated by MixinConfigGenerator)
        model.Mixins = new[] { $"{metadata.Id}.mixins.json" };

        // Dependencies
        model.Depends = new Dictionary<string, string>
        {
            ["fabricloader"] = $">={fabricLoaderVersion}",
            ["minecraft"] = $"~{minecraftVersion}",
            ["java"] = ">=21",
            ["fabric-api"] = "*"
        };

        return JsonSerializer.Serialize(model, JsonOptions);
    }

    /// <summary>
    /// Writes fabric.mod.json to the resources directory.
    /// </summary>
    public async Task WriteAsync(
        ModMetadata metadata,
        string outputDirectory,
        string minecraftVersion,
        string fabricLoaderVersion,
        CancellationToken ct = default)
    {
        var resourcesDir = Path.Combine(outputDirectory, "src", "main", "resources");
        Directory.CreateDirectory(resourcesDir);

        var json = Generate(metadata, minecraftVersion, fabricLoaderVersion);
        var outputPath = Path.Combine(resourcesDir, "fabric.mod.json");
        await File.WriteAllTextAsync(outputPath, json, ct);
    }
}
