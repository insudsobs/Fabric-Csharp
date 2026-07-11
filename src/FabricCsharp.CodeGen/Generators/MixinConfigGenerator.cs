using System.Text.Json;
using FabricCsharp.CodeGen.Models;

namespace FabricCsharp.CodeGen.Generators;

/// <summary>
/// Generates Mixin configuration JSON files from C# mixin class definitions.
/// </summary>
public class MixinConfigGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Generates a mixin config JSON for the given mod.
    /// </summary>
    public string Generate(
        string modId,
        string mixinPackage,
        string[]? commonMixins,
        string[]? clientMixins,
        string[]? serverMixins)
    {
        var model = new MixinConfigModel
        {
            Required = true,
            MinVersion = "0.8",
            Package = $"{modId}.mixin",
            CompatibilityLevel = "JAVA_21",
            Mixins = commonMixins,
            Client = clientMixins,
            Server = serverMixins,
            Injectors = new Dictionary<string, int>
            {
                ["defaultRequire"] = 1
            }
        };

        return JsonSerializer.Serialize(model, JsonOptions);
    }

    /// <summary>
    /// Writes the mixin config JSON to the resources directory.
    /// </summary>
    public async Task WriteAsync(
        string modId,
        string outputDirectory,
        string[]? commonMixins = null,
        string[]? clientMixins = null,
        string[]? serverMixins = null,
        CancellationToken ct = default)
    {
        var resourcesDir = Path.Combine(outputDirectory, "src", "main", "resources");
        Directory.CreateDirectory(resourcesDir);

        var json = Generate(modId, $"{modId}.mixin", commonMixins, clientMixins, serverMixins);
        var outputPath = Path.Combine(resourcesDir, $"{modId}.mixins.json");
        await File.WriteAllTextAsync(outputPath, json, ct);
    }
}
