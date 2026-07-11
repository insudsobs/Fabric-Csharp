using System.Collections.Immutable;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Provides bidirectional mapping between C# types and their Java equivalents.
/// This is the foundation of the transpiler — knows how every C# type maps to Java.
/// </summary>
public class TypeMapper
{
    private static readonly ImmutableDictionary<string, string> PrimitiveMappings = new Dictionary<string, string>
    {
        ["void"] = "void",
        ["bool"] = "boolean",
        ["byte"] = "byte",
        ["sbyte"] = "byte",
        ["short"] = "short",
        ["ushort"] = "short",
        ["int"] = "int",
        ["uint"] = "int",
        ["long"] = "long",
        ["ulong"] = "long",
        ["float"] = "float",
        ["double"] = "double",
        ["char"] = "char",
        ["decimal"] = "double",
        ["string"] = "String",
        ["object"] = "Object",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> SystemTypeMappings = new Dictionary<string, string>
    {
        ["System.String"] = "java.lang.String",
        ["System.Object"] = "java.lang.Object",
        ["System.Int32"] = "int",
        ["System.Int64"] = "long",
        ["System.Single"] = "float",
        ["System.Double"] = "double",
        ["System.Boolean"] = "boolean",
        ["System.Byte"] = "byte",
        ["System.Char"] = "char",
        ["System.Void"] = "void",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> CollectionMappings = new Dictionary<string, string>
    {
        ["System.Collections.Generic.List`1"] = "java.util.ArrayList<{0}>",
        ["System.Collections.Generic.Dictionary`2"] = "java.util.HashMap<{0}, {1}>",
        ["System.Collections.Generic.HashSet`1"] = "java.util.HashSet<{0}>",
        ["System.Collections.Generic.IEnumerable`1"] = "java.lang.Iterable<{0}>",
        ["System.Collections.Generic.ICollection`1"] = "java.util.Collection<{0}>",
        ["System.Collections.Generic.IList`1"] = "java.util.List<{0}>",
        ["System.Collections.Generic.IDictionary`2"] = "java.util.Map<{0}, {1}>",
        ["System.Collections.Generic.ISet`1"] = "java.util.Set<{0}>",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> DelegateMappings = new Dictionary<string, string>
    {
        ["System.Action"] = "java.lang.Runnable",
        ["System.Action`1"] = "java.util.function.Consumer<{0}>",
        ["System.Func`1"] = "java.util.function.Supplier<{0}>",
        ["System.Func`2"] = "java.util.function.Function<{0}, {1}>",
        ["System.Predicate`1"] = "java.util.function.Predicate<{0}>",
    }.ToImmutableDictionary();

    private static readonly ImmutableHashSet<string> FabricCsharpApiTypes = new HashSet<string>
    {
        "FabricCsharp.Api.Identifier",
        "FabricCsharp.Api.RegistryKey`1",
        "FabricCsharp.Api.Item",
        "FabricCsharp.Api.Item.Settings",
        "FabricCsharp.Api.Block",
        "FabricCsharp.Api.Block.Settings",
        "FabricCsharp.Api.BlockItem",
        "FabricCsharp.Api.Rarity",
        "FabricCsharp.Api.Player",
        "FabricCsharp.Api.World",
        "FabricCsharp.Api.Server",
        "FabricCsharp.Api.BlockPos",
        "FabricCsharp.Api.BlockState",
        "FabricCsharp.Api.BlockEntity",
        "FabricCsharp.Api.Registries",
        "FabricCsharp.Api.Events",
        "FabricCsharp.Api.FoodComponent",
        "FabricCsharp.Api.EquipmentSlot",
        "FabricCsharp.Api.StatusEffectInstance",
        "FabricCsharp.Api.StatusEffect",
        "FabricCsharp.Api.ToolMaterial",
        "FabricCsharp.Api.SwordItem",
        "FabricCsharp.Api.PickaxeItem",
        "FabricCsharp.Api.AxeItem",
        "FabricCsharp.Api.ShovelItem",
        "FabricCsharp.Api.HoeItem",
        "FabricCsharp.Api.MapColor",
    }.ToImmutableHashSet();

    /// <summary>
    /// Maps a FabricCsharp.Api type to its actual Minecraft/Fabric Java type.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> ApiToMinecraftMappings = new Dictionary<string, string>
    {
        ["FabricCsharp.Api.Identifier"] = "net.minecraft.util.Identifier",
        ["FabricCsharp.Api.RegistryKey`1"] = "net.minecraft.registry.RegistryKey",
        ["FabricCsharp.Api.Item"] = "net.minecraft.item.Item",
        ["FabricCsharp.Api.Item.Settings"] = "net.minecraft.item.Item.Settings",
        ["FabricCsharp.Api.Block"] = "net.minecraft.block.Block",
        ["FabricCsharp.Api.Block.Settings"] = "net.minecraft.block.AbstractBlock.Settings",
        ["FabricCsharp.Api.BlockItem"] = "net.minecraft.item.BlockItem",
        ["FabricCsharp.Api.Rarity"] = "net.minecraft.util.Rarity",
        ["FabricCsharp.Api.Player"] = "net.minecraft.entity.player.PlayerEntity",
        ["FabricCsharp.Api.World"] = "net.minecraft.world.World",
        ["FabricCsharp.Api.Server"] = "net.minecraft.server.MinecraftServer",
        ["FabricCsharp.Api.BlockPos"] = "net.minecraft.util.math.BlockPos",
        ["FabricCsharp.Api.BlockState"] = "net.minecraft.block.BlockState",
        ["FabricCsharp.Api.BlockEntity"] = "net.minecraft.block.entity.BlockEntity",
        ["FabricCsharp.Api.FoodComponent"] = "net.minecraft.component.type.FoodComponent",
        ["FabricCsharp.Api.EquipmentSlot"] = "net.minecraft.entity.EquipmentSlot",
        ["FabricCsharp.Api.StatusEffectInstance"] = "net.minecraft.entity.effect.StatusEffectInstance",
        ["FabricCsharp.Api.StatusEffect"] = "net.minecraft.entity.effect.StatusEffect",
        ["FabricCsharp.Api.ToolMaterial"] = "net.minecraft.item.ToolMaterial",
        ["FabricCsharp.Api.SwordItem"] = "net.minecraft.item.SwordItem",
        ["FabricCsharp.Api.PickaxeItem"] = "net.minecraft.item.PickaxeItem",
        ["FabricCsharp.Api.AxeItem"] = "net.minecraft.item.AxeItem",
        ["FabricCsharp.Api.ShovelItem"] = "net.minecraft.item.ShovelItem",
        ["FabricCsharp.Api.HoeItem"] = "net.minecraft.item.HoeItem",
        ["FabricCsharp.Api.MapColor"] = "net.minecraft.block.MapColor",
        ["FabricCsharp.Api.CallbackInfo"] = "org.spongepowered.asm.mixin.injection.callback.CallbackInfo",
        ["FabricCsharp.Api.CallbackInfoReturnable`1"] = "org.spongepowered.asm.mixin.injection.callback.CallbackInfoReturnable",
        ["FabricCsharp.Api.Direction"] = "net.minecraft.util.math.Direction",
    }.ToImmutableDictionary();

    /// <summary>
    /// Maps a C# type name (primitive, system, or full CLR name) to its Java type name.
    /// Returns null if no known mapping exists.
    /// </summary>
    public string? MapType(string csTypeName)
    {
        // Check FabricCsharp API mappings first (they override everything)
        if (ApiToMinecraftMappings.TryGetValue(csTypeName, out var mcType))
            return mcType;

        // Check primitives
        if (PrimitiveMappings.TryGetValue(csTypeName, out var primitive))
            return primitive;

        // Check system types
        if (SystemTypeMappings.TryGetValue(csTypeName, out var systemType))
            return systemType;

        // Check delegates
        if (DelegateMappings.TryGetValue(csTypeName, out var delegateType))
            return delegateType;

        return null;
    }

    /// <summary>
    /// Maps a generic C# type definition (e.g., System.Collections.Generic.List`1) to its Java equivalent.
    /// </summary>
    public string? MapGenericTypeDefinition(string genericTypeDefinition)
    {
        if (CollectionMappings.TryGetValue(genericTypeDefinition, out var javaType))
            return javaType;
        if (DelegateMappings.TryGetValue(genericTypeDefinition, out var delegateType))
            return delegateType;
        return null;
    }

    /// <summary>
    /// Checks if a type name is a FabricCsharp API stub type.
    /// </summary>
    public bool IsFabricCsharpApiType(string typeName) =>
        FabricCsharpApiTypes.Contains(typeName);

    /// <summary>
    /// Checks if a type name is a C# primitive type.
    /// </summary>
    public bool IsPrimitive(string typeName) =>
        PrimitiveMappings.ContainsKey(typeName);

    /// <summary>
    /// Gets the Java import statement for a fully qualified type name.
    /// </summary>
    public string? GetImportForType(string csTypeName)
    {
        var mapped = MapType(csTypeName);
        return mapped != null && mapped.Contains('.') ? mapped : null;
    }

    /// <summary>
    /// Converts C# access modifiers to Java equivalents.
    /// </summary>
    public static string MapAccessModifier(string csModifier) => csModifier switch
    {
        "public" => "public",
        "private" => "private",
        "protected" => "protected",
        "internal" => "public",  // Java has no internal; map to public
        "protected internal" => "protected",
        _ => "public"
    };

    /// <summary>
    /// Converts a C# operator name to Java equivalent.
    /// </summary>
    public static string MapOperator(string csOperator) => csOperator switch
    {
        "op_Addition" => "+",
        "op_Subtraction" => "-",
        "op_Multiply" => "*",
        "op_Division" => "/",
        "op_Modulus" => "%",
        "op_Equality" => "equals",
        "op_Inequality" => "!equals",
        _ => csOperator
    };
}
