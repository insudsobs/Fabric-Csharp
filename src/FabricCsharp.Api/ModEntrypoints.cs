namespace FabricCsharp.Api;

/// <summary>
/// Base interface for the main mod initializer.
/// Implement this to receive the main initialization callback.
/// Maps to net.fabricmc.api.ModInitializer in Fabric.
/// </summary>
public interface IModInitializer
{
    /// <summary>
    /// Called when the mod is initialized. Runs first on both client and server.
    /// Use this for common mod logic: registrations, event listeners, etc.
    /// </summary>
    void OnInitialize();
}

/// <summary>
/// Base interface for client-side mod initialization.
/// Implement this to receive client-only initialization callbacks.
/// Maps to net.fabricmc.api.ClientModInitializer in Fabric.
/// </summary>
public interface IClientModInitializer
{
    /// <summary>
    /// Called after OnInitialize, only on the client side.
    /// Use this for rendering, input handling, and other client-specific logic.
    /// </summary>
    void OnInitializeClient();
}

/// <summary>
/// Base interface for dedicated server mod initialization.
/// Implement this to receive server-only initialization callbacks.
/// Maps to net.fabricmc.api.DedicatedServerModInitializer in Fabric.
/// </summary>
public interface IDedicatedServerModInitializer
{
    /// <summary>
    /// Called after OnInitialize, only on dedicated servers.
    /// </summary>
    void OnInitializeServer();
}
