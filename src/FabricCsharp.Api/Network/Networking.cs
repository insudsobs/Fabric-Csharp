namespace FabricCsharp.Api.Network;

/// <summary>
/// Interface for custom packet payloads.
/// Implement this in your record/class to define a custom network packet.
/// Maps to: net.fabricmc.fabric.api.networking.v1.CustomPacketPayload
/// </summary>
public interface ICustomPacketPayload
{
    /// <summary>The packet type identifier.</summary>
    PacketType GetType();
}

/// <summary>
/// Packet type identifier.
/// Maps to: net.fabricmc.fabric.api.networking.v1.CustomPacketPayload.Type
/// </summary>
public abstract class PacketType
{
    /// <summary>Create a packet type from an identifier.</summary>
    public static PacketType Of(Identifier id)
    {
        throw new NotSupportedException(
            "PacketType.Of is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Attribute to mark a record/class as a custom packet payload.
/// The transpiler generates the Java CustomPacketPayload record + StreamCodec + registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PacketPayloadAttribute : Attribute
{
    /// <summary>The packet identifier string (e.g., "modid:packet_name").</summary>
    public required string Id { get; init; }

    /// <summary>Direction: "s2c" (server-to-client), "c2s" (client-to-server), or "both".</summary>
    public string Direction { get; init; } = "both";
}

/// <summary>
/// Server-side networking helpers. Transpiler intrinsic.
/// </summary>
public static class ServerNetworking
{
    /// <summary>Send a packet to a specific player.</summary>
    public static void Send(Player player, ICustomPacketPayload payload)
    {
        throw new NotSupportedException(
            "ServerNetworking.Send is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Register a receiver for a packet type.</summary>
    public static void RegisterReceiver(PacketType type, Action<ICustomPacketPayload, Player> handler)
    {
        throw new NotSupportedException(
            "ServerNetworking.RegisterReceiver is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Client-side networking helpers. Transpiler intrinsic.
/// </summary>
public static class ClientNetworking
{
    /// <summary>Send a packet to the server.</summary>
    public static void Send(ICustomPacketPayload payload)
    {
        throw new NotSupportedException(
            "ClientNetworking.Send is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Register a receiver for a packet type.</summary>
    public static void RegisterReceiver(PacketType type, Action<ICustomPacketPayload> handler)
    {
        throw new NotSupportedException(
            "ClientNetworking.RegisterReceiver is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Server-to-client packet sender. Call from server-side code.
/// </summary>
public static class ServerPlayNetworking
{
    /// <summary>Send a packet to a specific player.</summary>
    public static void Send(Player player, ICustomPacketPayload payload)
    {
        throw new NotSupportedException(
            "ServerPlayNetworking.Send is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Client-to-server packet sender. Call from client-side code.
/// </summary>
public static class ClientPlayNetworking
{
    /// <summary>Send a packet to the server.</summary>
    public static void Send(ICustomPacketPayload payload)
    {
        throw new NotSupportedException(
            "ClientPlayNetworking.Send is a transpiler intrinsic and should not be called at runtime.");
    }
}
