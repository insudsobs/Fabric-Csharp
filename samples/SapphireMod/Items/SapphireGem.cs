using FabricCsharp.Api;

namespace SapphireMod.Items;

/// <summary>
/// 蓝宝石 — 基础材料物品
/// 从蓝宝石矿石中掉落，用于合成蓝宝石装备和蓝宝石块。
///
/// Java 映射: net.minecraft.item.Item
/// </summary>
public class SapphireGemItem : Item
{
    public SapphireGemItem(Settings settings) : base(settings)
    {
    }
}
