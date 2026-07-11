using System.Text.Json;

namespace FabricCsharp.Transpiler.Mapping;

/// <summary>
/// Resolves Minecraft class/method/field mappings between development names
/// (Yarn or Mojang) and intermediary (ABI-stable) names.
/// </summary>
public class MappingsResolver
{
    /// <summary>
    /// Mapping source: "yarn", "mojang", or "intermediary".
    /// </summary>
    public string MappingType { get; set; } = "yarn";

    /// <summary>
    /// Minecraft version for the mappings.
    /// </summary>
    public string MinecraftVersion { get; set; } = "1.21.4";

    private readonly Dictionary<string, string> _classMappings = new();
    private readonly Dictionary<string, string> _methodMappings = new();
    private readonly Dictionary<string, string> _fieldMappings = new();

    /// <summary>
    /// Loads mappings from a Yarn tiny mappings file.
    /// </summary>
    public async Task LoadYarnMappingsAsync(string mappingsFilePath, CancellationToken ct = default)
    {
        var lines = await File.ReadAllLinesAsync(mappingsFilePath, ct);

        string? currentClass = null;

        foreach (var line in lines)
        {
            if (line.StartsWith('\t')) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('\t');
            if (parts.Length < 2) continue;

            switch (parts[0])
            {
                case "CLASS" when parts.Length >= 3:
                    // CLASS obf_name intermediary_name yarn_name
                    currentClass = parts[2]; // yarn name
                    _classMappings[currentClass] = parts[1]; // → intermediary
                    break;

                case "METHOD" when currentClass != null && parts.Length >= 4:
                    // METHOD obf_name obf_desc intermediary_name yarn_name
                    var methodKey = $"{currentClass}#{parts[3]}";
                    _methodMappings[methodKey] = parts[2]; // → intermediary
                    break;

                case "FIELD" when currentClass != null && parts.Length >= 5:
                    // FIELD obf_name obf_desc intermediary_name yarn_name
                    var fieldKey = $"{currentClass}#{parts[3]}";
                    _fieldMappings[fieldKey] = parts[2]; // → intermediary
                    break;
            }
        }
    }

    /// <summary>
    /// Resolves a Yarn class name to its intermediary equivalent.
    /// </summary>
    public string? ResolveClass(string yarnClassName) =>
        _classMappings.TryGetValue(yarnClassName, out var intermediary)
            ? intermediary
            : null;

    /// <summary>
    /// Resolves a Yarn method name to its intermediary equivalent.
    /// </summary>
    public string? ResolveMethod(string yarnClassName, string yarnMethodName)
    {
        var key = $"{yarnClassName}#{yarnMethodName}";
        return _methodMappings.TryGetValue(key, out var intermediary)
            ? intermediary
            : null;
    }

    /// <summary>
    /// Resolves a Yarn field name to its intermediary equivalent.
    /// </summary>
    public string? ResolveField(string yarnClassName, string yarnFieldName)
    {
        var key = $"{yarnClassName}#{yarnFieldName}";
        return _fieldMappings.TryGetValue(key, out var intermediary)
            ? intermediary
            : null;
    }

    /// <summary>
    /// For Mojang mappings (official names), no remapping is needed —
    /// Fabric Loader uses Mojang names directly starting with Minecraft 26.1+.
    /// </summary>
    public bool IsMojangMapping => MappingType == "mojang";

    /// <summary>
    /// Returns the appropriate Java class name for a mapped class.
    /// For Yarn-based projects, this is the yarn name used directly in Java code
    /// (Loom handles the intermediary remap at build time).
    /// For Mojang mappings, this is the official Mojang name.
    /// </summary>
    public string GetJavaClassName(string mappedName) => mappedName;
}
