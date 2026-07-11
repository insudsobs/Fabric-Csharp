namespace FabricCsharp.Api;

/// <summary>
/// Defines the food properties for an item.
/// Maps to net.minecraft.component.type.FoodComponent in Minecraft.
/// </summary>
public class FoodComponent
{
    public int Hunger { get; set; } = 4;
    public float SaturationModifier { get; set; } = 0.3f;
    public bool IsMeat { get; set; }
    public bool AlwaysEdible { get; set; }
    public bool Snack { get; set; }
    public StatusEffectInstance[]? StatusEffects { get; set; }
}
