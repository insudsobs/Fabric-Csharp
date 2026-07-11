namespace FabricCsharp.Api;

/// <summary>
/// Represents an in-game Block. All C# blocks should inherit from this class.
/// Maps to net.minecraft.block.Block in Minecraft.
/// </summary>
public abstract class Block
{
    /// <summary>
    /// Settings for configuring a block's properties.
    /// </summary>
    public class Settings
    {
        public float Hardness { get; set; } = 1.0f;
        public float Resistance { get; set; } = 1.0f;
        public bool RequiresTool { get; set; }
        public SoundGroup? SoundGroup { get; set; }
        public float Slipperiness { get; set; } = 0.6f;
        public int Luminance { get; set; }
        public bool ToolRequired { get; set; }
        public MapColor MapColor { get; set; } = MapColor.Stone;
        public bool TicksRandomly { get; set; }
        public bool Collidable { get; set; } = true;

        public Settings HardnessAndResistance(float hardness, float resistance)
        {
            Hardness = hardness;
            Resistance = resistance;
            return this;
        }

        public Settings RequiresCorrectTool()
        {
            RequiresTool = true;
            return this;
        }
    }

    /// <summary>
    /// Creates a new Block with the specified settings.
    /// </summary>
    protected Block(Settings settings)
    {
        BlockSettings = settings;
    }

    public Settings BlockSettings { get; }
}

/// <summary>
/// Represents the sound group for a block (wood, stone, metal, etc.).
/// Maps to net.minecraft.sound.BlockSoundGroup in Minecraft.
/// </summary>
public enum SoundGroup
{
    Wood,
    Stone,
    Metal,
    Glass,
    Wool,
    Sand,
    Snow,
    Ladder,
    Anvil,
    Slime,
    Honey,
    Chain,
    Lantern
}
