namespace FabricCsharp.Api;

/// <summary>
/// Provides access to Fabric API events through a C#-idiomatic interface.
/// Event handlers registered here are translated to Java event callbacks by the transpiler.
/// </summary>
public static class Events
{
#pragma warning disable CS0067 // Events are transpiler intrinsics, invoked from generated Java code

    /// <summary>
    /// Fired after a block is broken by a player (but before the block is removed).
    /// Maps to PlayerBlockBreakEvents.BEFORE in Fabric.
    /// </summary>
    public static event Action<Player, World, BlockPos, BlockState, BlockEntity?>? BlockBreakBefore;

    /// <summary>
    /// Fired after a block is broken by a player (after removal).
    /// Maps to PlayerBlockBreakEvents.AFTER in Fabric.
    /// </summary>
    public static event Action<Player, World, BlockPos, BlockState, BlockEntity?>? BlockBreakAfter;

    /// <summary>
    /// Fired when the server is starting.
    /// Maps to ServerLifecycleEvents.SERVER_STARTING in Fabric.
    /// </summary>
    public static event Action<Server>? ServerStarting;

    /// <summary>
    /// Fired after the server has started.
    /// Maps to ServerLifecycleEvents.SERVER_STARTED in Fabric.
    /// </summary>
    public static event Action<Server>? ServerStarted;

    /// <summary>
    /// Fired when the server is stopping.
    /// Maps to ServerLifecycleEvents.SERVER_STOPPING in Fabric.
    /// </summary>
    public static event Action<Server>? ServerStopping;

    /// <summary>
    /// Fired when a player joins the server.
    /// Maps to ServerPlayConnectionEvents.JOIN in Fabric.
    /// </summary>
    public static event Action<Player>? PlayerJoin;
#pragma warning restore CS0067
}

// === Stub types for event parameters ===
// These are placeholders that the transpiler maps to real Minecraft types.

/// <summary>Stub for net.minecraft.entity.player.PlayerEntity</summary>
public abstract class Player { }
/// <summary>Stub for net.minecraft.world.World</summary>
public abstract class World { }
/// <summary>Stub for net.minecraft.server.MinecraftServer</summary>
public abstract class Server { }
/// <summary>Stub for net.minecraft.util.math.BlockPos</summary>
public abstract class BlockPos { }
/// <summary>Stub for net.minecraft.block.BlockState</summary>
public abstract class BlockState { }
/// <summary>Stub for net.minecraft.block.entity.BlockEntity</summary>
public abstract class BlockEntity { }
