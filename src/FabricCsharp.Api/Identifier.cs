using System.Diagnostics.CodeAnalysis;

namespace FabricCsharp.Api;

/// <summary>
/// A Minecraft resource identifier consisting of a namespace and a path.
/// Equivalent to net.minecraft.util.Identifier in Minecraft.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier>
{
    public string Namespace { get; }
    public string Path { get; }

    public Identifier(string namespaceName, string path)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            throw new ArgumentException("Namespace cannot be null or whitespace.", nameof(namespaceName));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

        Namespace = namespaceName;
        Path = path;
    }

    /// <summary>
    /// Creates an identifier with the "minecraft" namespace.
    /// </summary>
    public static Identifier Minecraft(string path) => new("minecraft", path);

    /// <summary>
    /// Parses an identifier string in the format "namespace:path".
    /// If no colon is present, the namespace defaults to "minecraft".
    /// </summary>
    public static Identifier Of(string id)
    {
        var colonIndex = id.IndexOf(':');
        return colonIndex >= 0
            ? new Identifier(id[..colonIndex], id[(colonIndex + 1)..])
            : new Identifier("minecraft", id);
    }

    public override string ToString() => $"{Namespace}:{Path}";

    public bool Equals(Identifier other) =>
        string.Equals(Namespace, other.Namespace, StringComparison.Ordinal) &&
        string.Equals(Path, other.Path, StringComparison.Ordinal);

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Identifier other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Namespace, Path);

    public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
    public static bool operator !=(Identifier left, Identifier right) => !left.Equals(right);
}
