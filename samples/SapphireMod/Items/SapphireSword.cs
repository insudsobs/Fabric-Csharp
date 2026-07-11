using FabricCsharp.Api;

namespace SapphireMod.Items;

/// <summary>
/// 蓝宝石剑 — 攻击伤害 +4，攻速 1.6
/// 蓝宝石工具的特殊能力：挖掘方块后给予 2 秒急迫效果
///
/// Java 映射: net.minecraft.item.SwordItem
/// </summary>
public class SapphireSwordItem : Item
{
    public SapphireSwordItem(Settings settings) : base(settings)
    {
    }
}
