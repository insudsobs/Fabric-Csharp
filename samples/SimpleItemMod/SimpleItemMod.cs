using FabricCsharp.Api;

namespace SimpleItemMod;

[ModInfo(
    Id = "simple-item-mod",
    Name = "Simple Item Mod",
    Version = "1.0.0",
    Description = "Adds a simple custom item to Minecraft",
    Authors = new[] { "FabricCsharp User" },
    License = "MIT"
)]
public class SimpleItemMod : IModInitializer
{
    // Define a custom item
    public static readonly RegistryKey<Item> CustomIngotKey =
        RegistryKey<Item>.Of("simple_item_mod", "custom_ingot");

    public static readonly Item CustomIngot = Registries.Register(
        CustomIngotKey,
        () => new CustomItem(new Item.Settings
        {
            MaxCount = 64
        }));

    public void OnInitialize()
    {
        // Item registration happens automatically via static initialization
        // The transpiler converts the Registries.Register call above
        // into Java Registry.register() code.
        Console.WriteLine("SimpleItemMod initialized!");
    }
}

/// <summary>
/// A custom item implementation.
/// Maps to a Java class extending net.minecraft.item.Item.
/// </summary>
public class CustomItem : Item
{
    public CustomItem(Settings settings) : base(settings)
    {
    }
}
