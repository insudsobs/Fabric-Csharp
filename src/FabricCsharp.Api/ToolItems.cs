namespace FabricCsharp.Api;

/// <summary>
/// A sword item with attack damage and speed properties.
/// Maps to net.minecraft.item.SwordItem in Minecraft.
/// </summary>
public abstract class SwordItem : Item
{
    public ToolMaterial Material { get; }
    public float AttackDamage { get; }
    public float AttackSpeed { get; }

    protected SwordItem(ToolMaterial material, float attackDamage, float attackSpeed, Settings settings)
        : base(settings)
    {
        Material = material;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
    }
}

/// <summary>
/// A pickaxe item with attack damage and speed properties.
/// Maps to net.minecraft.item.PickaxeItem in Minecraft.
/// </summary>
public abstract class PickaxeItem : Item
{
    public ToolMaterial Material { get; }
    public float AttackDamage { get; }
    public float AttackSpeed { get; }

    protected PickaxeItem(ToolMaterial material, float attackDamage, float attackSpeed, Settings settings)
        : base(settings)
    {
        Material = material;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
    }
}

/// <summary>
/// An axe item with attack damage and speed properties.
/// Maps to net.minecraft.item.AxeItem in Minecraft.
/// </summary>
public abstract class AxeItem : Item
{
    public ToolMaterial Material { get; }
    public float AttackDamage { get; }
    public float AttackSpeed { get; }

    protected AxeItem(ToolMaterial material, float attackDamage, float attackSpeed, Settings settings)
        : base(settings)
    {
        Material = material;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
    }
}

/// <summary>
/// A shovel item with attack damage and speed properties.
/// Maps to net.minecraft.item.ShovelItem in Minecraft.
/// </summary>
public abstract class ShovelItem : Item
{
    public ToolMaterial Material { get; }
    public float AttackDamage { get; }
    public float AttackSpeed { get; }

    protected ShovelItem(ToolMaterial material, float attackDamage, float attackSpeed, Settings settings)
        : base(settings)
    {
        Material = material;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
    }
}

/// <summary>
/// A hoe item with attack damage and speed properties.
/// Maps to net.minecraft.item.HoeItem in Minecraft.
/// </summary>
public abstract class HoeItem : Item
{
    public ToolMaterial Material { get; }
    public float AttackDamage { get; }
    public float AttackSpeed { get; }

    protected HoeItem(ToolMaterial material, float attackDamage, float attackSpeed, Settings settings)
        : base(settings)
    {
        Material = material;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
    }
}
