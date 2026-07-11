using System.Text;
using System.Text.Json;

namespace FabricCsharp.CodeGen.Generators;

/// <summary>
/// Generates language (localization) files for the mod.
/// </summary>
public class LangGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Generates an English (en_us.json) language file with auto-generated names.
    /// </summary>
    public string GenerateEnglish(
        string modId,
        IReadOnlyDictionary<string, string>? customTranslations = null)
    {
        var translations = new Dictionary<string, string>();

        if (customTranslations != null)
        {
            foreach (var (key, value) in customTranslations)
            {
                translations[key] = value;
            }
        }

        return JsonSerializer.Serialize(translations, JsonOptions);
    }

    /// <summary>
    /// Adds an item translation entry.
    /// </summary>
    public static string ItemKey(string modId, string itemId) =>
        $"item.{modId}.{itemId}";

    /// <summary>
    /// Adds a block translation entry.
    /// </summary>
    public static string BlockKey(string modId, string blockId) =>
        $"block.{modId}.{blockId}";

    /// <summary>
    /// Writes the language file to the assets directory.
    /// </summary>
    public async Task WriteAsync(
        string modId,
        string outputDir,
        IReadOnlyDictionary<string, string>? translations = null,
        CancellationToken ct = default)
    {
        var langDir = Path.Combine(outputDir, "src", "main", "resources",
            "assets", modId, "lang");
        Directory.CreateDirectory(langDir);

        var json = GenerateEnglish(modId, translations);
        await File.WriteAllTextAsync(
            Path.Combine(langDir, "en_us.json"), json, ct);
    }
}
