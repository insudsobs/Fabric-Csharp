using System.Text.Json;

namespace FabricCsharp.CodeGen.Generators;

/// <summary>
/// Generates Minecraft resource pack files: block states, item/block models, etc.
/// </summary>
public class AssetGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Generates a basic item model JSON for a handheld item.
    /// </summary>
    public string GenerateItemModel(string modId, string itemId)
    {
        var model = new
        {
            parent = "item/generated",
            textures = new
            {
                layer0 = $"{modId}:item/{itemId}"
            }
        };

        return JsonSerializer.Serialize(model, JsonOptions);
    }

    /// <summary>
    /// Generates a basic block model JSON.
    /// </summary>
    public string GenerateBlockModel(string modId, string blockId, string? parent = null)
    {
        var model = new
        {
            parent = parent ?? "block/cube_all",
            textures = new
            {
                all = $"{modId}:block/{blockId}"
            }
        };

        return JsonSerializer.Serialize(model, JsonOptions);
    }

    /// <summary>
    /// Generates a basic block state JSON.
    /// </summary>
    public string GenerateBlockState(string modId, string blockId)
    {
        var blockState = new
        {
            variants = new
            {
                _ = new
                {
                    model = $"{modId}:block/{blockId}"
                }
            }
        };

        return JsonSerializer.Serialize(blockState, JsonOptions);
    }

    /// <summary>
    /// Generates an item model definition file (required since Minecraft 1.21.4).
    /// </summary>
    public string GenerateItemModelDefinition(string modId, string itemId)
    {
        var definition = new
        {
            model = new
            {
                type = "model",
                model = $"{modId}:item/{itemId}"
            }
        };

        return JsonSerializer.Serialize(definition, JsonOptions);
    }

    /// <summary>
    /// Writes default assets for a registered item.
    /// </summary>
    public async Task WriteItemAssetsAsync(
        string modId, string itemId, string outputDir, CancellationToken ct = default)
    {
        var assetsDir = Path.Combine(outputDir, "src", "main", "resources", "assets", modId);

        // Item model
        var modelDir = Path.Combine(assetsDir, "models", "item");
        Directory.CreateDirectory(modelDir);
        await File.WriteAllTextAsync(
            Path.Combine(modelDir, $"{itemId}.json"),
            GenerateItemModel(modId, itemId), ct);

        // Item model definition (1.21.4+)
        var itemDefDir = Path.Combine(assetsDir, "items");
        Directory.CreateDirectory(itemDefDir);
        await File.WriteAllTextAsync(
            Path.Combine(itemDefDir, $"{itemId}.json"),
            GenerateItemModelDefinition(modId, itemId), ct);
    }

    /// <summary>
    /// Writes default assets for a registered block.
    /// </summary>
    public async Task WriteBlockAssetsAsync(
        string modId, string blockId, string outputDir, CancellationToken ct = default)
    {
        var assetsDir = Path.Combine(outputDir, "src", "main", "resources", "assets", modId);

        // Block model
        var modelDir = Path.Combine(assetsDir, "models", "block");
        Directory.CreateDirectory(modelDir);
        await File.WriteAllTextAsync(
            Path.Combine(modelDir, $"{blockId}.json"),
            GenerateBlockModel(modId, blockId), ct);

        // Block state
        var blockStateDir = Path.Combine(assetsDir, "blockstates");
        Directory.CreateDirectory(blockStateDir);
        await File.WriteAllTextAsync(
            Path.Combine(blockStateDir, $"{blockId}.json"),
            GenerateBlockState(modId, blockId), ct);

        // Block item model
        var itemModelDir = Path.Combine(assetsDir, "models", "item");
        Directory.CreateDirectory(itemModelDir);
        await File.WriteAllTextAsync(
            Path.Combine(itemModelDir, $"{blockId}.json"),
            GenerateItemModel(modId, blockId), ct);
    }
}
