namespace FabricCsharp.Api;

/// <summary>
/// Marks a class as the main mod entry point and provides metadata
/// that gets written to fabric.mod.json during the build.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModInfoAttribute : Attribute
{
    /// <summary>
    /// Unique mod identifier. Must match the pattern: ^[a-z][a-z0-9-_]{1,63}$
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable mod name. Displayed in Mod Menu and other UIs.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Semantic version string (SemVer 2.0.0 format).
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Brief description of what the mod does.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// List of author names.
    /// </summary>
    public string[]? Authors { get; init; }

    /// <summary>
    /// List of contributor names.
    /// </summary>
    public string[]? Contributors { get; init; }

    /// <summary>
    /// Contact information for the mod.
    /// </summary>
    public ModContact? Contact { get; init; }

    /// <summary>
    /// SPDX license identifier (e.g., "MIT", "GPL-3.0").
    /// </summary>
    public string? License { get; init; }

    /// <summary>
    /// Path to mod icon PNG, relative to the assets folder.
    /// Example: "assets/modid/icon.png"
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Environment restriction: "client", "server", or "*" for both.
    /// </summary>
    public string Environment { get; init; } = "*";
}

/// <summary>
/// Contact information for a mod's metadata.
/// </summary>
public class ModContact
{
    public string? Homepage { get; init; }
    public string? Sources { get; init; }
    public string? Issues { get; init; }
}
