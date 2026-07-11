using FabricCsharp.Api;
using SapphireMod.Blocks;
using SapphireMod.Items;

namespace SapphireMod;

/// <summary>
/// 主 mod 入口点 — 蓝宝石装备 mod
///
/// 功能：
///   - 蓝宝石矿石（地下生成，挖掘掉落蓝宝石）
///   - 蓝宝石块（9 颗蓝宝石合成，用于存储/装饰）
///   - 蓝宝石（材料，从矿石掉落）
///   - 蓝宝石剑（4 攻击力，1.6 攻速）
///   - 蓝宝石镐（3 攻击力，1.2 攻速，耐久 1561）
///   - 特殊效果：用蓝宝石工具挖掘时获得 2 秒急迫效果
/// </summary>
[ModInfo(
    Id = "sapphire-mod",
    Name = "Sapphire Equipment",
    Version = "1.0.0",
    Description = "Adds sapphire ore, tools, and equipment to Minecraft. Sapphire tools grant Haste when mining blocks.",
    Authors = new[] { "FabricCsharp Dev" },
    License = "MIT",
    Environment = "*"
)]
public class SapphireMod : IModInitializer
{
    // ═══════════════════════════════════════════════════
    // 方块注册
    // ═══════════════════════════════════════════════════

    public static readonly RegistryKey<Block> SapphireOreKey =
        RegistryKey<Block>.Of("sapphire_mod", "sapphire_ore");

    public static readonly Block SapphireOreBlock = Registries.Register(
        SapphireOreKey,
        () => new SapphireOreBlock(new Block.Settings
        {
            Hardness = 3.0f,
            Resistance = 3.0f,
            RequiresTool = true,
            SoundGroup = SoundGroup.Stone
        }));

    public static readonly RegistryKey<Block> SapphireBlockKey =
        RegistryKey<Block>.Of("sapphire_mod", "sapphire_block");

    public static readonly Block SapphirePureBlock = Registries.Register(
        SapphireBlockKey,
        () => new SapphireStorageBlock(new Block.Settings
        {
            Hardness = 5.0f,
            Resistance = 6.0f,
            RequiresTool = true,
            SoundGroup = SoundGroup.Metal
        }));

    // ═══════════════════════════════════════════════════
    // 物品注册
    // ═══════════════════════════════════════════════════

    public static readonly RegistryKey<Item> SapphireGemKey =
        RegistryKey<Item>.Of("sapphire_mod", "sapphire");

    public static readonly Item SapphireGemItem = Registries.Register(
        SapphireGemKey,
        () => new SapphireGemItem(new Item.Settings
        {
            MaxCount = 64,
            Rarity = Rarity.Uncommon
        }));

    public static readonly RegistryKey<Item> SapphireSwordKey =
        RegistryKey<Item>.Of("sapphire_mod", "sapphire_sword");

    public static readonly Item SapphireSwordItem = Registries.Register(
        SapphireSwordKey,
        () => new SapphireSwordItem(new Item.Settings
        {
            MaxCount = 1,
            MaxDamage = 0
        }));

    public static readonly RegistryKey<Item> SapphirePickaxeKey =
        RegistryKey<Item>.Of("sapphire_mod", "sapphire_pickaxe");

    public static readonly Item SapphirePickaxeItem = Registries.Register(
        SapphirePickaxeKey,
        () => new SapphirePickaxeItem(new Item.Settings
        {
            MaxCount = 1,
            MaxDamage = 0
        }));

    // ═══════════════════════════════════════════════════
    // 初始化入口
    // ═══════════════════════════════════════════════════

    public void OnInitialize()
    {
        // 所有物品和方块的注册已经在静态初始化中完成
        // Transpiler 会将上面的 Registries.Register 调用
        // 转换为 Java 侧的 Registry.register() 代码

        // 注册事件监听
        EventHandlers.Register();
    }
}
