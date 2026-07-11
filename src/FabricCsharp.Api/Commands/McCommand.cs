namespace FabricCsharp.Api.Commands;

/// <summary>
/// Provides a C#-idiomatic interface for registering Minecraft commands.
/// Transpiler intrinsic: calls are translated to CommandRegistrationCallback.EVENT.register() in Java.
/// </summary>
public static class McCommand
{
    /// <summary>
    /// Registers a simple command with no arguments.
    /// Java: dispatcher.register(CommandManager.literal(name).executes(ctx -> { ... }));
    /// </summary>
    public static void Register(string name, Action<CommandContext> handler)
    {
        throw new NotSupportedException(
            "McCommand.Register is a transpiler intrinsic and should not be called at runtime. " +
            "It is converted to CommandRegistrationCallback.EVENT.register() during the C# to Java translation.");
    }

    /// <summary>
    /// Registers a command with one integer argument.
    /// Java: dispatcher.register(CommandManager.literal(name).then(CommandManager.argument("value", IntegerArgumentType.integer()).executes(ctx -> { ... })));
    /// </summary>
    public static void RegisterWithInt(string name, Action<CommandContext, int> handler)
    {
        throw new NotSupportedException(
            "McCommand.RegisterWithInt is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>
    /// Registers a command with one string argument.
    /// </summary>
    public static void RegisterWithString(string name, Action<CommandContext, string> handler)
    {
        throw new NotSupportedException(
            "McCommand.RegisterWithString is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>
    /// Registers a command with one player argument (ops only).
    /// Java: dispatcher.register(CommandManager.literal(name).requires(src -> src.hasPermissionLevel(2)).then(CommandManager.argument("target", EntityArgumentType.player()).executes(ctx -> { ... })));
    /// </summary>
    public static void RegisterOpWithPlayer(string name, Action<CommandContext, Player> handler)
    {
        throw new NotSupportedException(
            "McCommand.RegisterOpWithPlayer is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>
    /// Registers a command with a player argument (no op requirement).
    /// </summary>
    public static void RegisterWithPlayer(string name, Action<CommandContext, Player> handler)
    {
        throw new NotSupportedException(
            "McCommand.RegisterWithPlayer is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>
    /// Registers a command with a BlockPos argument.
    /// </summary>
    public static void RegisterWithBlockPos(string name, Action<CommandContext, BlockPos> handler)
    {
        throw new NotSupportedException(
            "McCommand.RegisterWithBlockPos is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>
    /// Registers a command with subcommands.
    /// Use a builder pattern for complex commands.
    /// </summary>
    public static CommandBuilder Build(string name)
    {
        throw new NotSupportedException(
            "McCommand.Build is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Fluent builder for complex commands with subcommands and multiple arguments.
/// </summary>
public class CommandBuilder
{
    /// <summary>Require operator permission level 2+ to execute.</summary>
    public CommandBuilder RequiresOp()
    {
        throw new NotSupportedException(
            "CommandBuilder.RequiresOp is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Require a specific permission level.</summary>
    public CommandBuilder RequiresPermission(int level)
    {
        throw new NotSupportedException(
            "CommandBuilder.RequiresPermission is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Add a literal subcommand.</summary>
    public CommandBuilder ThenLiteral(string name)
    {
        throw new NotSupportedException(
            "CommandBuilder.ThenLiteral is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Add an integer argument subcommand.</summary>
    public CommandBuilder ThenInt(string name, Action<CommandContext, int> handler)
    {
        throw new NotSupportedException(
            "CommandBuilder.ThenInt is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Add a string argument subcommand.</summary>
    public CommandBuilder ThenString(string name, Action<CommandContext, string> handler)
    {
        throw new NotSupportedException(
            "CommandBuilder.ThenString is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Add a player argument subcommand.</summary>
    public CommandBuilder ThenPlayer(string name, Action<CommandContext, Player> handler)
    {
        throw new NotSupportedException(
            "CommandBuilder.ThenPlayer is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Add a BlockPos argument subcommand.</summary>
    public CommandBuilder ThenBlockPos(string name, Action<CommandContext, BlockPos> handler)
    {
        throw new NotSupportedException(
            "CommandBuilder.ThenBlockPos is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Set the handler for the current node.</summary>
    public CommandBuilder Executes(Action<CommandContext> handler)
    {
        throw new NotSupportedException(
            "CommandBuilder.Executes is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Finalize and register the command.</summary>
    public void Register()
    {
        throw new NotSupportedException(
            "CommandBuilder.Register is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Stub type for command execution context.
/// Maps to: com.mojang.brigadier.context.CommandContext&lt;net.minecraft.server.command.ServerCommandSource&gt;
/// </summary>
public abstract class CommandContext
{
    /// <summary>Get the command source (server, player, etc).</summary>
    public abstract CommandSource GetSource();
}

/// <summary>
/// Stub type for command source.
/// Maps to: net.minecraft.server.command.ServerCommandSource
/// </summary>
public abstract class CommandSource
{
    /// <summary>Send a feedback message to the command executor.</summary>
    public abstract void SendFeedback(Text text, bool broadcastToOps);

    /// <summary>Send an error message.</summary>
    public abstract void SendError(Text text);

    /// <summary>Get the player who executed this command, or null.</summary>
    public abstract Player? GetPlayer();

    /// <summary>Get the server instance.</summary>
    public abstract Server GetServer();

    /// <summary>Get the world the command was executed in.</summary>
    public abstract World GetWorld();
}

/// <summary>
/// Stub for Minecraft text components.
/// Maps to: net.minecraft.text.Text
/// </summary>
public abstract class Text
{
    /// <summary>Create a literal text component.</summary>
    public static Text Of(string content)
    {
        throw new NotSupportedException(
            "Text.Of is a transpiler intrinsic and should not be called at runtime. " +
            "It is converted to Text.of()/Text.literal() during the C# to Java translation.");
    }

    /// <summary>Create a translatable text component.</summary>
    public static Text Translatable(string key, params object[] args)
    {
        throw new NotSupportedException(
            "Text.Translatable is a transpiler intrinsic and should not be called at runtime.");
    }

    /// <summary>Create an empty text component.</summary>
    public static Text Empty()
    {
        throw new NotSupportedException(
            "Text.Empty is a transpiler intrinsic and should not be called at runtime.");
    }
}
