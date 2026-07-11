# Changelog

## [0.1.0] - 2026-07-11

### Added
- Core transpiler: Roslyn-powered C# → Java source translation
- Type mapping: 60+ C# to Java type conversion rules
- Syntax translation: classes, methods, properties, fields, if/else, for/foreach/while, try/catch, switch, lambda, string interpolation
- Resource generation: fabric.mod.json, mixin configs, block states, item/block models, language files (en_us.json)
- Roslyn analyzers: 10 diagnostic rules for unsupported C# features (FC001-FC010)
- MSBuild SDK integration: `dotnet build` end-to-end pipeline
- `dotnet new fabric-mod` project template
- Gradle + Fabric Loom build system integration
- Source map annotations in generated Java code
- API bindings: Identifier, RegistryKey, Item, Block, BlockItem, Rarity, SoundGroup, ModInfoAttribute
- Entrypoint interfaces: IModInitializer, IClientModInitializer, IDedicatedServerModInitializer
- Event system stubs: BlockBreak, ServerLifecycle, PlayerJoin
- Sample mod: Sapphire Equipment (ore, tools, special effects)
- Unit tests: 12 transpiler tests
- Documentation: README, API Reference, Tutorial

### Planned for 0.2.0
- Full Fabric API binding (tools, entities, networking, rendering, commands)
- Mixin DSL (C# attributes → Java mixin classes)
- GitHub Actions CI/CD
- Expanded event bindings (30+ events)
- Full Item/Block Settings properties
- NuGet package publication
