namespace FabricCsharp.Api;

/// <summary>
/// Static registry helpers for registering items, blocks, and other game objects.
/// These calls are special-cased by the transpiler to generate proper Java registration code.
/// </summary>
public static class Registries
{
    /// <summary>
    /// Registers an object in the appropriate registry.
    /// The transpiler converts this to the correct Java Registry.register() call with key-aware registration.
    /// </summary>
    /// <typeparam name="T">The type of object to register (Item, Block, etc.).</typeparam>
    /// <param name="key">The registry key identifying this object.</param>
    /// <param name="factory">A factory function that creates the object.</param>
    /// <returns>The registered object.</returns>
    public static T Register<T>(RegistryKey<T> key, Func<T> factory)
    {
        // This method is never actually called at runtime.
        // It is intercepted by the transpiler which generates the appropriate Java registration code.
        // The implementation below is just a stub for compilation purposes.
        throw new NotSupportedException(
            "Registries.Register is a transpiler intrinsic and should not be called at runtime. " +
            "It is converted to Java Registry.register() calls during the C# to Java translation.");
    }
}
