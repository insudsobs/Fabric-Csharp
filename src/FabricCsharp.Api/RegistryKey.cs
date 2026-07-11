namespace FabricCsharp.Api;

/// <summary>
/// A typed registry key used for key-aware registration in Minecraft 1.21.2+.
/// Equivalent to net.minecraft.registry.RegistryKey in Minecraft.
/// </summary>
/// <typeparam name="T">The type of object this key identifies in the registry.</typeparam>
public readonly record struct RegistryKey<T>(Identifier Id)
{
    public static RegistryKey<T> Of(string namespaceName, string path) =>
        new(new Identifier(namespaceName, path));

    public override string ToString() => Id.ToString();
}
