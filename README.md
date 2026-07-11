# ⛏️ FabricCsharp

<div align="center">

**Write Minecraft Fabric mods in C# — transpiled to JVM bytecode, no Java knowledge required.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Minecraft](https://img.shields.io/badge/Minecraft-1.21.x-62B47A?logo=minecraft)](https://fabricmc.net/)

</div>

---

## What is FabricCsharp?

**FabricCsharp** is a complete SDK and toolchain that lets you write Minecraft Fabric mods in C#. Your C# code is transpiled to Java, then compiled into a standard Fabric `.jar` — the same output as a vanilla Java mod. No embedded runtimes, no native libraries, no CLR hacks. Just a clean Fabric JAR.

```csharp
// Your mod in C# — looks and feels native
[ModInfo(Id = "my-mod", Name = "My Mod", Version = "1.0.0")]
public class MyMod : IModInitializer
{
    var key = RegistryKey<Item>.Of("my_mod", "cool_sword");
    var sword = Registries.Register(key,
        () => new CoolSword(new Item.Settings { MaxCount = 1 }));

    public void OnInitialize() { }
}
```

```bash
# One command — produces my-mod.jar ready for Minecraft
dotnet build
```

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   YOUR C# MOD CODE                      │
│               [ModInfo], IModInitializer,                │
│            Registries.Register(), Events, etc.           │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              FabricCsharp.Transpiler                     │
│          Roslyn CSharpSyntaxWalker                       │
│          C# AST → Java source code                      │
│          Type mapping, property→getter/setter,           │
│          event→callback, nameof→string, etc.             │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              FabricCsharp.CodeGen                        │
│          fabric.mod.json, mixin configs,                 │
│          block states, item/block models,                │
│          language files — all auto-generated             │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              Gradle + Fabric Loom                        │
│          Java compile, remapping (Yarn/Mojang),          │
│          mixin annotation processing                     │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              my-mod.jar                                  │
│          Standard Fabric JAR, ready to use               │
└─────────────────────────────────────────────────────────┘
```

## Key Features

| Feature | Description |
|---------|-------------|
| **Native C# Experience** | Write mods in C# with full IDE support (Rider, VS Code, Visual Studio) |
| **Standard JAR Output** | Final artifact is a regular Fabric JAR — no weird dependencies |
| **No Java Required** | You never touch Java code, Gradle, or Loom config manually |
| **Roslyn Analyzers** | Real-time checks that catch unsupported C# patterns before build |
| **`dotnet build`** | The entire pipeline runs from a single MSBuild command |
| **Type mapping** | 60+ C#→Java type rules: primitives, collections, delegates, Minecraft types |
| **Syntax translation** | if/else, for/foreach/while, try/catch, switch, lambda, string interpolation |
| **Property mapping** | C# properties → Java getter/setter methods |
| **Event mapping** | C# `event` + `+=` → Fabric Java `.register()` callbacks |
| **Auto code generation** | `fabric.mod.json`, mixin configs, block states, models, lang files |
| **Source maps** | Errors point to your C# line, not generated Java |
| **`dotnet new` templates** | `dotnet new fabric-mod -n MyMod` scaffolds a complete project |

## Quick Start

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [JDK 21+](https://adoptium.net/) (auto-detected by the build pipeline)

### Install

```bash
# Install the project template (coming to NuGet soon)
dotnet new install FabricCsharp.Templates

# Create your first mod
dotnet new fabric-mod -n MyFirstMod --ModId my-first-mod

# Build → produces MyFirstMod.jar
dotnet build
```

### Manual Setup

```xml
<!-- MyMod.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ModId>my-mod</ModId>
    <MinecraftVersion>1.21.4</MinecraftVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FabricCsharp.Sdk" Version="0.1.0-*" />
    <PackageReference Include="FabricCsharp.Api" Version="0.1.0-*" />
  </ItemGroup>
</Project>
```

## Example: Sapphire Equipment Mod

A complete mod built with FabricCsharp — sapshire ore, tools, and special effects:

| Component | ID | Details |
|-----------|-----|---------|
| Sapphire Gem | `sapphire_mod:sapphire` | Crafting material, Uncommon rarity |
| Sapphire Ore | `sapphire_mod:sapphire_ore` | Generates underground, drops gems |
| Sapphire Block | `sapphire_mod:sapphire_block` | Storage block, beacon compatible |
| Sapphire Sword | `sapphire_mod:sapphire_sword` | 4 attack damage, 1.6 speed |
| Sapphire Pickaxe | `sapphire_mod:sapphire_pickaxe` | Diamond-tier, 1561 durability |
| **Special effect** | Haste I (2s) | Triggers when mining with any Sapphire tool |

```csharp
// Event handlers — C# idiomatic
public static class EventHandlers
{
    public static void Register()
    {
        Events.BlockBreakAfter += OnBlockBroken;
        Events.ServerStarting  += OnServerStarting;
        Events.PlayerJoin      += OnPlayerJoin;
    }
}
```

→ See the full source in [`samples/SapphireMod/`](samples/SapphireMod/)

## Project Layout

```
FabricCsharp/
├── src/
│   ├── FabricCsharp.Api/           # C# bindings for Minecraft/Fabric types
│   │   ├── Identifier.cs           #   minecraft:path resource identifier
│   │   ├── Item.cs, Block.cs       #   Item/Block base classes + Settings
│   │   ├── Registries.cs           #   Registry system (transpiler intrinsic)
│   │   ├── ModEntrypoints.cs       #   IModInitializer, IClientModInitializer
│   │   ├── Events.cs               #   Fabric event bindings + stub types
│   │   └── ModInfoAttribute.cs     #   [ModInfo] metadata attribute
│   │
│   ├── FabricCsharp.Transpiler/    # Core C# → Java translator
│   │   ├── CsParser/               #   Source collection, metadata extraction
│   │   ├── JavaGenerator/          #   TypeMapper, StatementTranslator, etc.
│   │   ├── Mapping/                #   Yarn/Intermediary/Mojang resolver
│   │   └── Pipeline/               #   Build orchestration, Gradle runner
│   │
│   ├── FabricCsharp.CodeGen/       # Resource file generator
│   │   └── Generators/             #   fabric.mod.json, mixin configs, assets
│   │
│   ├── FabricCsharp.Analyzers/     # Roslyn analyzers (10 diagnostic rules)
│   ├── FabricCsharp.Sdk/           # MSBuild SDK (props, targets, tasks)
│   └── FabricCsharp.Templates/     # dotnet new project templates
│
├── test/                           # Unit & integration tests
└── samples/
    ├── SimpleItemMod/              # Minimal example: one custom item
    └── SapphireMod/                # Full-featured mod example
```

## Supported C# → Java Mappings

### Types

| C# | Java | | C# | Java |
|----|------|-|----|------|
| `string` | `String` | | `object` | `Object` |
| `int` | `int` | | `bool` | `boolean` |
| `float` | `float` | | `double` | `double` |
| `List<T>` | `ArrayList<T>` | | `Dictionary<K,V>` | `HashMap<K,V>` |
| `Action<T>` | `Consumer<T>` | | `Func<T,R>` | `Function<T,R>` |

### Syntax

| C# | Java |
|----|------|
| `namespace X.Y { }` | `package X.Y;` |
| `using X;` | `import X;` |
| `class Foo : Bar, IBaz` | `class Foo extends Bar implements IBaz` |
| `base.Foo()` | `super.Foo()` |
| `nameof(X)` | `"X"` |
| `typeof(T)` | `T.class` |
| `x is T` | `x instanceof T` |
| `Prop { get; set; }` | `getProp()` / `setProp(v)` |
| `$"Hello {name}"` | `"Hello " + name` |
| `foreach (var x in items)` | `for (var x : items)` |

### Intentionally Unsupported

| Feature | Reason |
|---------|--------|
| `dynamic` | No JVM equivalent |
| `unsafe` code | JVM has no pointer arithmetic |
| `Span<T>` / `Memory<T>` | Stack-only ref structs |
| `async` / `await` | Minecraft is single-threaded game loop |
| LINQ query expressions | Use method chains or foreach instead |
| `ref` / `out` parameters | Java is pass-by-value only |

---

> 💡 All unsupported features are flagged by Roslyn analyzers **in real time** inside your IDE — no surprises at build time.

## Comparison with Alternatives

| | FabricCsharp | CSCraft.Sdk | Fabric Language Kotlin |
|---|---|---|---|
| **Language** | C# (transpiled) | C# (transpiled) | Kotlin (native JVM) |
| **JAR output** | ✅ Standard Fabric JAR | ✅ Standard Fabric JAR | ✅ Standard Fabric JAR |
| **API coverage** | Full SDK (planned) | Partial (moderate) | Full (official FabricMC) |
| **IDE support** | ✅ Analyzers + source gen | ❌ None | ✅ IntelliJ plugin |
| **Mixin support** | C# DSL → Java mixins | ❌ Java only | ❌ Java only |
| **Open source** | ✅ MIT | ✅ MIT | ✅ Apache 2.0 |
| **Build system** | `dotnet build` | `dotnet build` | Gradle |
| **License** | MIT | MIT | Apache 2.0 |

## Roadmap

- [x] Core transpiler (C# → Java source)
- [x] Type mapping (60+ rules)
- [x] Resource generation (fabric.mod.json, models, lang)
- [x] Roslyn analyzers (10 rules)
- [x] MSBuild SDK integration
- [x] `dotnet new` templates
- [x] Sample mod (Sapphire Equipment)
- [ ] Complete Fabric API binding (items, blocks, entities, networking, rendering)
- [ ] Mixin DSL (C# attributes → Java mixin classes)
- [ ] Rider / VS Code extension with source-level debugging
- [ ] Multi-version compatibility (1.21.x + 26.x)
- [ ] NuGet package distribution

## Contributing

Contributions are welcome! The project is at an early stage and there's plenty to work on:

- **Transpiler**: More C# syntax support, edge cases
- **API bindings**: Expand the Fabric API surface in `FabricCsharp.Api`
- **Analyzers**: More diagnostic rules for common pitfalls
- **Testing**: Unit tests for transpiler, integration tests for mod output
- **Docs**: Tutorials, migration guides, API reference

```bash
git clone https://github.com/cscraft/cscraft.git
cd FabricCsharp
dotnet build        # Build everything
dotnet test         # Run tests
```

## License

MIT © FabricCsharp Contributors

---

<div align="center">

**Write Minecraft mods in the language you love.**

`dotnet build` → play. That's it.

</div>
