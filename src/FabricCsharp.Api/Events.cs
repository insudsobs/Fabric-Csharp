namespace FabricCsharp.Api;

/// <summary>
/// Provides access to Fabric API events through a C#-idiomatic interface.
/// Event handlers registered here are translated to Java event callbacks by the transpiler.
/// </summary>
public static class Events
{
#pragma warning disable CS0067 // Events are transpiler intrinsics, invoked from generated Java code

    // === Block Events ===

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
    /// Fired when a player places a block.
    /// Maps to PlayerBlockPlaceEvents.BEFORE in Fabric.
    /// </summary>
    public static event Action<Player, World, BlockPos, BlockState, ItemStack>? BlockPlace;

    /// <summary>
    /// Fired when a player attacks (starts breaking) a block.
    /// Maps to AttackBlockCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Player, World, Hand, BlockPos, Direction>? AttackBlock;

    // === Server Lifecycle Events ===

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
    /// Fired after the server has stopped.
    /// Maps to ServerLifecycleEvents.SERVER_STOPPED in Fabric.
    /// </summary>
    public static event Action<Server>? ServerStopped;

    /// <summary>
    /// Fired when a world is loaded on the server.
    /// Maps to ServerWorldEvents.LOAD in Fabric.
    /// </summary>
    public static event Action<Server, ServerWorld>? ServerWorldLoad;

    // === Player Events ===

    /// <summary>
    /// Fired when a player joins the server.
    /// Maps to ServerPlayConnectionEvents.JOIN in Fabric.
    /// </summary>
    public static event Action<Player>? PlayerJoin;

    /// <summary>
    /// Fired when a player leaves the server.
    /// Maps to ServerPlayConnectionEvents.DISCONNECT in Fabric.
    /// </summary>
    public static event Action<Player>? PlayerLeave;

    /// <summary>
    /// Fired when a player dies.
    /// Maps to ServerPlayerEvents.AFTER_DEATH in Fabric.
    /// </summary>
    public static event Action<Player, DamageSource>? PlayerDeath;

    /// <summary>
    /// Fired when a player respawns.
    /// Maps to ServerPlayerEvents.AFTER_RESPAWN in Fabric.
    /// </summary>
    public static event Action<Player>? PlayerRespawn;

    /// <summary>
    /// Fired each server tick for every online player.
    /// Maps to ServerTickEvents.START_PLAYER_TICK in Fabric.
    /// </summary>
    public static event Action<Player>? PlayerTick;

    /// <summary>
    /// Fired when a player sends a chat message.
    /// Maps to ServerMessageEvents.CHAT_MESSAGE in Fabric.
    /// </summary>
    public static event Action<Player, string>? PlayerChat;

    /// <summary>
    /// Fired when a player earns an advancement.
    /// Maps to PlayerAdvancementCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Player, Identifier>? PlayerAdvancement;

    // === Item/Inventory Events ===

    /// <summary>
    /// Fired when a player uses an item.
    /// Maps to UseItemCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Player, World, Hand>? UseItem;

    /// <summary>
    /// Fired when a player picks up an item entity.
    /// Maps to ItemPickupEvents.PLAYER_PICKUP in Fabric.
    /// </summary>
    public static event Action<Player, ItemEntity>? ItemPickup;

    /// <summary>
    /// Fired when a player crafts an item.
    /// Maps to CraftItemCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Player, ItemStack, ScreenHandler>? CraftItem;

    /// <summary>
    /// Fired when a player smelts (takes result from furnace) an item.
    /// Maps to SmeltItemCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Player, ItemStack>? SmeltItem;

    // === World Events ===

    /// <summary>
    /// Fired each tick for every loaded world.
    /// Maps to WorldTickCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<World>? WorldTick;

    /// <summary>
    /// Fired when an entity is spawned in a world.
    /// Maps to EntitySpawnCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<Entity, World>? EntitySpawn;

    /// <summary>
    /// Fired when lightning strikes a position in a world.
    /// Maps to LightningStrikeCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<World, BlockPos>? LightningStrike;

    // === Client Events ===

    /// <summary>
    /// Fired each client tick.
    /// Maps to ClientTickEvents.START_CLIENT_TICK in Fabric.
    /// </summary>
    public static event Action<MinecraftClient>? ClientTick;

    /// <summary>
    /// Fired when the HUD is rendered.
    /// Maps to HudRenderCallback.EVENT in Fabric.
    /// </summary>
    public static event Action<DrawContext, float>? RenderHud;

    // === Network Events ===

    /// <summary>
    /// Fired when the server receives a packet from a player.
    /// Maps to ServerPacketEvents.RECEIVED in Fabric.
    /// </summary>
    public static event Action<Server, Player, Identifier>? ServerPacketReceived;

    /// <summary>
    /// Fired when the client receives a packet.
    /// Maps to ClientPacketEvents.RECEIVED in Fabric.
    /// </summary>
    public static event Action<Player, Identifier>? ClientPacketReceived;

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
/// <summary>Stub for net.minecraft.item.ItemStack</summary>
public abstract class ItemStack { }
/// <summary>Stub for net.minecraft.util.Hand</summary>
public abstract class Hand { }
/// <summary>Stub for net.minecraft.util.math.Direction</summary>
public abstract class Direction { }
/// <summary>Stub for net.minecraft.server.world.ServerWorld</summary>
public abstract class ServerWorld { }
/// <summary>Stub for net.minecraft.entity.damage.DamageSource</summary>
public abstract class DamageSource { }
/// <summary>Stub for net.minecraft.entity.ItemEntity</summary>
public abstract class ItemEntity { }
/// <summary>Stub for net.minecraft.screen.ScreenHandler</summary>
public abstract class ScreenHandler { }
/// <summary>Stub for net.minecraft.entity.Entity</summary>
public abstract class Entity { }
/// <summary>Stub for net.minecraft.client.MinecraftClient</summary>
public abstract class MinecraftClient { }
/// <summary>Stub for net.minecraft.client.gui.DrawContext</summary>
public abstract class DrawContext { }
