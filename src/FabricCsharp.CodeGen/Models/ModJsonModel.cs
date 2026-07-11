using System.Text.Json.Serialization;

namespace FabricCsharp.CodeGen.Models;

/// <summary>
/// Represents the structure of fabric.mod.json.
/// </summary>
public class ModJsonModel
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "${version}";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("authors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Authors { get; set; }

    [JsonPropertyName("contributors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Contributors { get; set; }

    [JsonPropertyName("contact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModContactModel? Contact { get; set; }

    [JsonPropertyName("license")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? License { get; set; }

    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Icon { get; set; }

    [JsonPropertyName("environment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Environment { get; set; }

    [JsonPropertyName("entrypoints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Entrypoints { get; set; }

    [JsonPropertyName("mixins")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Mixins { get; set; }

    [JsonPropertyName("depends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Depends { get; set; }

    [JsonPropertyName("recommends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Recommends { get; set; }
}

public class ModContactModel
{
    [JsonPropertyName("homepage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Homepage { get; set; }

    [JsonPropertyName("sources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sources { get; set; }

    [JsonPropertyName("issues")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Issues { get; set; }
}

/// <summary>
/// Represents a Mixin configuration JSON file.
/// </summary>
public class MixinConfigModel
{
    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [JsonPropertyName("minVersion")]
    public string MinVersion { get; set; } = "0.8";

    [JsonPropertyName("package")]
    public string Package { get; set; } = string.Empty;

    [JsonPropertyName("compatibilityLevel")]
    public string CompatibilityLevel { get; set; } = "JAVA_21";

    [JsonPropertyName("mixins")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Mixins { get; set; }

    [JsonPropertyName("client")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Client { get; set; }

    [JsonPropertyName("server")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Server { get; set; }

    [JsonPropertyName("injectors")]
    public Dictionary<string, int> Injectors { get; set; } = new()
    {
        ["defaultRequire"] = 1
    };
}
