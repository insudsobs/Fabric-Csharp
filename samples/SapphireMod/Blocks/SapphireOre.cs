using FabricCsharp.Api;

namespace SapphireMod.Blocks;

/// <summary>
/// 蓝宝石矿石 — 在地下生成（Y=-64 到 Y=16）
/// 挖掘后掉落 1-2 颗蓝宝石（受时运附魔影响）
/// 只能用铁镐或更高级镐挖掘
///
/// Java 映射: net.minecraft.block.Block
/// </summary>
public class SapphireOreBlock : Block
{
    public SapphireOreBlock(Settings settings) : base(settings)
    {
    }
}
