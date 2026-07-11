namespace FabricCsharp.Api;

/// <summary>
/// Represents a status effect instance applied to an entity.
/// Maps to net.minecraft.entity.effect.StatusEffectInstance in Minecraft.
/// </summary>
public abstract class StatusEffectInstance
{
    public required StatusEffect Effect { get; init; }
    public int Duration { get; init; }
    public int Amplifier { get; init; }
    public bool Ambient { get; init; }
    public bool ShowParticles { get; init; }
    public bool ShowIcon { get; init; }
}
