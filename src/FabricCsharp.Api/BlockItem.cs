namespace FabricCsharp.Api;

/// <summary>
/// A BlockItem is the inventory form of a Block.
/// Maps to net.minecraft.item.BlockItem in Minecraft.
/// </summary>
public abstract class BlockItem : Item
{
    public Block Block { get; }

    protected BlockItem(Block block, Settings settings) : base(settings)
    {
        Block = block;
    }
}
