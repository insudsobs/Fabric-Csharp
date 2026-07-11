namespace FabricCsharp.Api;

/// <summary>
/// Represents an in-game Item. All C# items should inherit from this class.
/// Maps to net.minecraft.item.Item in Minecraft.
/// </summary>
public abstract class Item
{
    /// <summary>
    /// Settings for configuring an item's properties.
    /// </summary>
    public class Settings
    {
        public int MaxCount { get; set; } = 64;
        public int MaxDamage { get; set; }
        public bool Fireproof { get; set; }
        public Rarity Rarity { get; set; } = Rarity.Common;
        public FoodComponent? Food { get; set; }
        public int Enchantability { get; set; }
        public Item? RecipeRemainder { get; set; }
        public EquipmentSlot EquipmentSlot { get; set; }
    }

    /// <summary>
    /// Creates a new Item with the specified settings.
    /// </summary>
    protected Item(Settings settings)
    {
        ItemSettings = settings;
    }

    public Settings ItemSettings { get; }
}
