using FabricCsharp.Api;

namespace SapphireMod.Items;

/// <summary>
/// 蓝宝石镐 — 采矿速度 8.0，攻击伤害 +3，攻速 1.2，耐久 1561
/// 等效于钻石镐的性能，但外观为蓝宝石色
///
/// Java 映射: net.minecraft.item.PickaxeItem
/// </summary>
public class SapphirePickaxeItem : Item
{
    public SapphirePickaxeItem(Settings settings) : base(settings)
    {
    }
}
