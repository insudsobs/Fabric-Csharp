namespace FabricCsharp.Api;

/// <summary>
/// Structured representation of a mod's metadata extracted from [ModInfo].
/// This is a shared data transfer object used by both the transpiler and code generator.
/// </summary>
public class ModMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? License { get; set; }
    public string? Icon { get; set; }
    public string Environment { get; set; } = "*";
    public string[]? Authors { get; set; }
    public string[]? Contributors { get; set; }

    public bool IsValid => !string.IsNullOrEmpty(Id) &&
                            !string.IsNullOrEmpty(Name) &&
                            !string.IsNullOrEmpty(Version);

    public string MainClass { get; set; } = string.Empty;
    public string? ClientClass { get; set; }
    public string? ServerClass { get; set; }
}
