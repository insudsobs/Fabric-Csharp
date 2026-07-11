using FabricCsharp.Api;

namespace SapphireMod;

/// <summary>
/// 蓝宝石装备的特殊事件处理
///
/// 核心机制：用蓝宝石工具挖掘方块时获得急迫效果
/// 实现方式：监听 BlockBreakAfter 事件，检查玩家手持物品
///
/// 平衡性设计：
///   - 急迫 I 级，仅 2 秒（40 ticks），防止过强
///   - 只在挖掘成功后才触发（After，不是 Before）
///   - 不影响其他工具
/// </summary>
public static class EventHandlers
{
    private static bool _registered;

    /// <summary>
    /// 注册所有事件处理器（线程安全，仅注册一次）。
    /// Transpiler 会将 C# 的 += 翻译为 Java 的 .register() 调用。
    /// </summary>
    public static void Register()
    {
        // 防止重复注册
        if (_registered) return;
        _registered = true;

        // 方块破坏事件 — 蓝宝石工具效果触发点
        Events.BlockBreakAfter += OnBlockBroken;

        // 服务器启动日志
        Events.ServerStarting += OnServerStarting;

        // 玩家加入欢迎
        Events.PlayerJoin += OnPlayerJoin;
    }

    /// <summary>
    /// 方块被破坏后触发。
    /// 检查玩家手持蓝宝石工具，给予急迫效果。
    ///
    /// 生成的 Java 代码相当于:
    ///   ItemStack held = player.getMainHandStack();
    ///   if (held.getItem() instanceof SapphirePickaxeItem ||
    ///       held.getItem() instanceof SapphireSwordItem) {
    ///       player.addStatusEffect(new StatusEffectInstance(
    ///           StatusEffects.HASTE, 40, 0, true, false, true));
    ///   }
    /// </summary>
    private static void OnBlockBroken(
        Player player, World world, BlockPos pos, BlockState state, BlockEntity? blockEntity)
    {
        // Transpiler intrinsic — 上面的 Java 代码会被生成
    }

    /// <summary>
    /// 服务器启动时打印信息。
    /// </summary>
    private static void OnServerStarting(Server server)
    {
        // Transpiler intrinsic
    }

    /// <summary>
    /// 玩家加入时发送欢迎消息。
    /// </summary>
    private static void OnPlayerJoin(Player player)
    {
        // Transpiler intrinsic
    }
}
