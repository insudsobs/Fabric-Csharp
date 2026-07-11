# FabricCsharp API Reference

完整的 C# Fabric API 绑定文档。以下是 `FabricCsharp.Api` 命名空间中所有类型的详细说明。

> **重要概念：Transpiler Intrinsic**
>
> 标记为 "Transpiler Intrinsic" 的方法/类**不会在 .NET 运行时执行**，它们在编译时被 Transpiler 拦截并翻译为对应的 Java 代码。在 C# 中调用这些方法只是为了通过编译——真正的逻辑由生成的 Java 代码实现。

---

## 目录

1. [核心类型](#核心类型)
2. [注册系统](#注册系统)
3. [入口点接口](#入口点接口)
4. [属性](#属性)
5. [事件系统](#事件系统)
6. [Stub 类型](#stub-类型)
7. [枚举](#枚举)
8. [类型映射参考表](#类型映射参考表)
9. [支持的 C# 语法](#支持的-c-语法)
10. [不支持的 C# 特性](#不支持的-c-特性)

---

## 核心类型

### `Identifier`

Minecraft 资源标识符，格式为 `namespace:path`。

```csharp
public readonly struct Identifier : IEquatable<Identifier>
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 构造函数 | `Identifier(string namespace, string path)` | 用指定的命名空间和路径创建标识符 |
| 属性 | `string Namespace` | 命名空间部分 |
| 属性 | `string Path` | 路径部分 |
| 静态方法 | `Identifier Minecraft(string path)` | 创建 `minecraft:xxx` 标识符的快捷方式 |
| 静态方法 | `Identifier Of(string id)` | 从 `"namespace:path"` 字符串解析。无冒号时默认 namespace 为 `"minecraft"` |
| 方法 | `string ToString()` | 返回 `"namespace:path"` 格式 |

**使用示例：**

```csharp
// 三种创建方式
var id1 = new Identifier("my_mod", "cool_sword");      // 显式构造
var id2 = Identifier.Of("my_mod:cool_sword");           // 字符串解析
var id3 = Identifier.Minecraft("diamond");               // minecraft:diamond

// 比较
if (id1 == id2) { /* true */ }

// 用作注册键
var key = RegistryKey<Item>.Of("my_mod", "cool_sword");
```

**Java 映射**: `net.minecraft.util.Identifier`

---

### `RegistryKey<T>`

类型化的注册键，用于 Minecraft 1.21.2+ 的 key-aware 注册系统。

```csharp
public readonly record struct RegistryKey<T>(Identifier Id)
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 构造函数 | `RegistryKey<T>(Identifier id)` | 从 Identifier 创建注册键 |
| 属性 | `Identifier Id` | 底层标识符 |
| 静态方法 | `RegistryKey<T> Of(string ns, string path)` | 快捷创建方式 |

**使用示例：**

```csharp
// 为物品创建注册键
var swordKey = RegistryKey<Item>.Of("my_mod", "cool_sword");

// 为方块创建注册键
var oreKey = RegistryKey<Block>.Of("my_mod", "sapphire_ore");

// 类型参数确保你只能用它注册对应类型
// RegistryKey<Item> 只能传给 Registries.Register<Item>()
```

**Java 映射**: `net.minecraft.registry.RegistryKey`

---

### `Item`

游戏内物品的抽象基类。你的所有自定义物品都应继承此类。

```csharp
public abstract class Item
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 构造函数 | `protected Item(Settings settings)` | 创建带配置的物品实例 |
| 属性 | `Settings ItemSettings` | 物品的配置参数 |

#### `Item.Settings`

物品配置类（Builder 模式）。

```csharp
public class Settings
```

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `MaxCount` | `int` | `64` | 最大堆叠数量 |
| `MaxDamage` | `int` | `0` | 最大耐久值（0 表示不可损坏） |
| `Fireproof` | `bool` | `false` | 是否防火（不掉入岩浆） |
| `Rarity` | `Rarity` | `Common` | 物品稀有度（影响名称颜色） |

**使用示例：**

```csharp
// 基础物品 — 64 堆叠
new Item.Settings { MaxCount = 64 }

// 工具 — 不可堆叠，有耐久
new Item.Settings { MaxCount = 1, MaxDamage = 1561 }

// 稀有掉落物 — 蓝色名称
new Item.Settings { Rarity = Rarity.Epic }

// 防火物品 — 不会在岩浆中销毁
new Item.Settings { Fireproof = true }
```

**Java 映射**: `net.minecraft.item.Item` / `net.minecraft.item.Item.Settings`

---

### `Block`

游戏内方块的抽象基类。

```csharp
public abstract class Block
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 构造函数 | `protected Block(Settings settings)` | 创建带配置的方块实例 |
| 属性 | `Settings BlockSettings` | 方块的配置参数 |

#### `Block.Settings`

方块配置类（Fluent Builder 模式）。

```csharp
public class Settings
```

| 成员 | 类型/签名 | 默认值 | 说明 |
|------|-----------|--------|------|
| `Hardness` | `float` | `1.0` | 硬度（挖掘时间） |
| `Resistance` | `float` | `1.0` | 爆炸抗性 |
| `RequiresTool` | `bool` | `false` | 是否需要用工具才能掉落 |
| `SoundGroup` | `SoundGroup?` | `null` | 方块音效组 |
| 方法 | `Settings HardnessAndResistance(float h, float r)` | — | 同时设置硬度和抗性，返回自身 |
| 方法 | `Settings RequiresCorrectTool()` | — | 设置为需要正确工具，返回自身 |

**使用示例：**

```csharp
// 矿石 — 用 Fluent API
new Block.Settings
{
    SoundGroup = SoundGroup.Stone
}.HardnessAndResistance(3.0f, 3.0f).RequiresCorrectTool()

// 金属块
new Block.Settings
{
    Hardness = 5.0f,
    Resistance = 6.0f,
    RequiresTool = true,
    SoundGroup = SoundGroup.Metal
}
```

**Java 映射**: `net.minecraft.block.Block` / `net.minecraft.block.AbstractBlock.Settings`

---

### `BlockItem`

方块的物品形态，继承自 `Item`。

```csharp
public abstract class BlockItem : Item
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 构造函数 | `protected BlockItem(Block block, Item.Settings settings)` | 绑定方块和物品配置 |
| 属性 | `Block Block` | 关联的方块 |

**使用示例：**

```csharp
// 注册方块时通常自动创建 BlockItem
// FabricCsharp SDK 会为每个 Register<Block>() 调用
// 自动生成对应的 BlockItem 注册
var ore = Registries.Register(oreKey, () => new MyOreBlock(settings));
// 等价于 Java: Items.register(block);
```

**Java 映射**: `net.minecraft.item.BlockItem`

---

## 注册系统

### `Registries`

**Transpiler Intrinsic** — 在运行时调用会抛出 `NotSupportedException`。

```csharp
public static class Registries
```

| 成员 | 签名 | 说明 |
|------|------|------|
| 静态方法 | `T Register<T>(RegistryKey<T> key, Func<T> factory)` | 向 Minecraft 注册表注册对象 |

**工作原理：**

1. C# 中写 `Registries.Register(key, () => new X())`
2. Transpiler 检测到该调用，生成 Java 代码：
   ```java
   X item = Items.register(key, X::new, new Item.Settings().maxCount(64));
   ```

**使用示例：**

```csharp
// 注册物品 — 最常见模式
var key = RegistryKey<Item>.Of("my_mod", "custom_item");
var item = Registries.Register(key, () => new CustomItem(
    new Item.Settings { MaxCount = 64 }));

// 注册方块
var blockKey = RegistryKey<Block>.Of("my_mod", "custom_block");
var block = Registries.Register(blockKey, () => new CustomBlock(
    new Block.Settings { Hardness = 3.0f }));
```

**注意事项：**
- 必须在静态字段初始化器或 `OnInitialize()` 中调用
- `Func<T>` 参数必须是 lambda 表达式（`() => new X()`），不能是已存在的变量
- 类型参数 `T` 决定了注册到哪个注册表（`Item` → `Registries.ITEM`，`Block` → `Registries.BLOCK`）

---

## 入口点接口

### `IModInitializer`

主 mod 入口点，在客户端和服务端都会执行。

```csharp
public interface IModInitializer
{
    void OnInitialize();
}
```

**执行时机：** 最先触发（`preLaunch` 之后，`client`/`server` 之前）

**使用示例：**

```csharp
[ModInfo(Id = "my-mod", Name = "My Mod", Version = "1.0.0")]
public class MyMod : IModInitializer
{
    public void OnInitialize()
    {
        // 在这里注册事件监听器
        // 物品/方块的静态注册在静态初始化中完成
        EventHandlers.Register();
    }
}
```

**Java 映射**: `net.fabricmc.api.ModInitializer`

---

### `IClientModInitializer`

客户端专用的 mod 入口点。

```csharp
public interface IClientModInitializer
{
    void OnInitializeClient();
}
```

**执行时机：** `IModInitializer.OnInitialize()` 之后，仅在客户端

**使用示例：**

```csharp
public class MyModClient : IClientModInitializer
{
    public void OnInitializeClient()
    {
        // 注册实体渲染器
        // 注册颜色提供器
        // 注册 HUD 覆盖层
    }
}
```

**Java 映射**: `net.fabricmc.api.ClientModInitializer`

---

### `IDedicatedServerModInitializer`

专用服务器入口点。

```csharp
public interface IDedicatedServerModInitializer
{
    void OnInitializeServer();
}
```

**执行时机：** `IModInitializer.OnInitialize()` 之后，仅在专用服务器

**Java 映射**: `net.fabricmc.api.DedicatedServerModInitializer`

---

## 属性

### `[ModInfo]`

标记 mod 主类并提供元数据。**必须**应用在实现了 `IModInitializer` 的类上。

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModInfoAttribute : Attribute
```

| 属性 | 类型 | 必需 | 说明 |
|------|------|------|------|
| `Id` | `string` | ✅ | 唯一 mod ID，格式：`^[a-z][a-z0-9-_]{1,63}$` |
| `Name` | `string` | ✅ | 人类可读的 mod 名称（显示在 Mod Menu 中） |
| `Version` | `string` | ✅ | 语义化版本号（SemVer 2.0.0） |
| `Description` | `string?` | ❌ | mod 简介 |
| `Authors` | `string[]?` | ❌ | 作者列表 |
| `Contributors` | `string[]?` | ❌ | 贡献者列表 |
| `Contact` | `ModContact?` | ❌ | 联系信息 |
| `License` | `string?` | ❌ | SPDX 许可证标识符（如 `"MIT"`） |
| `Icon` | `string?` | ❌ | 图标 PNG 路径（如 `"assets/modid/icon.png"`） |
| `Environment` | `string` | ❌ | 环境限制：`"*"`、`"client"`、`"server"`（默认 `"*"`） |

**使用示例：**

```csharp
[ModInfo(
    Id = "sapphire-mod",
    Name = "Sapphire Equipment",
    Version = "1.0.0",
    Description = "Adds sapphire ore, tools, and equipment to Minecraft.",
    Authors = new[] { "YourName" },
    License = "MIT",
    Environment = "*"
)]
public class SapphireMod : IModInitializer { ... }
```

**生成的 `fabric.mod.json` 片段：**

```json
{
  "schemaVersion": 1,
  "id": "sapphire-mod",
  "version": "${version}",
  "name": "Sapphire Equipment",
  "description": "Adds sapphire ore, tools, and equipment to Minecraft.",
  "authors": ["YourName"],
  "license": "MIT",
  "environment": "*",
  "entrypoints": {
    "main": ["sapphire-mod.SapphireMod"]
  },
  "depends": {
    "fabricloader": ">=0.16.10",
    "minecraft": "~1.21.4",
    "java": ">=21",
    "fabric-api": "*"
  }
}
```

---

### `ModContact`

联系信息，作为 `[ModInfo]` 的可选子属性。

```csharp
public class ModContact
{
    public string? Homepage { get; init; }
    public string? Sources { get; init; }
    public string? Issues { get; init; }
}
```

**使用示例：**

```csharp
[ModInfo(
    Id = "my-mod",
    Name = "My Mod",
    Version = "1.0.0",
    Contact = new ModContact
    {
        Homepage = "https://example.com",
        Sources = "https://github.com/user/my-mod",
        Issues = "https://github.com/user/my-mod/issues"
    }
)]
```

---

## 事件系统

### `Events`

**Transpiler Intrinsic** — 所有事件成员在运行时不会触发。

```csharp
public static class Events
```

C# 中通过 `+=` / `-=` 注册/注销事件。Transpiler 将其翻译为 Java Fabric API 的 `.register()` 调用。

#### 可用事件列表

| 事件 | 签名 | Fabric API 来源 | 环境 |
|------|------|----------------|------|
| `BlockBreakBefore` | `Action<Player, World, BlockPos, BlockState, BlockEntity?>` | `PlayerBlockBreakEvents.BEFORE` | 双端 |
| `BlockBreakAfter` | `Action<Player, World, BlockPos, BlockState, BlockEntity?>` | `PlayerBlockBreakEvents.AFTER` | 双端 |
| `ServerStarting` | `Action<Server>` | `ServerLifecycleEvents.SERVER_STARTING` | 双端 |
| `ServerStarted` | `Action<Server>` | `ServerLifecycleEvents.SERVER_STARTED` | 双端 |
| `ServerStopping` | `Action<Server>` | `ServerLifecycleEvents.SERVER_STOPPING` | 双端 |
| `PlayerJoin` | `Action<Player>` | `ServerPlayConnectionEvents.JOIN` | 双端 |

**C# 写法：**

```csharp
Events.BlockBreakAfter += OnBlockBroken;
Events.ServerStarting += OnServerStarting;
Events.PlayerJoin += OnPlayerJoin;
```

**生成的 Java 代码：**

```java
PlayerBlockBreakEvents.AFTER.register((world, player, pos, state, entity) -> {
    onBlockBroken(world, player, pos, state, entity);
});

ServerLifecycleEvents.SERVER_STARTING.register(server -> {
    onServerStarting(server);
});

ServerPlayConnectionEvents.JOIN.register((handler, sender, server) -> {
    onPlayerJoin(handler.getPlayer());
});
```

---

## Stub 类型

这些抽象类是**占位符**，标记 Java 类型映射关系但没有任何 .NET 运行时实现。

| C# 类型 | Java 映射 | 说明 |
|----------|-----------|------|
| `Player` | `net.minecraft.entity.player.PlayerEntity` | 玩家实体 |
| `World` | `net.minecraft.world.World` | 世界/维度 |
| `Server` | `net.minecraft.server.MinecraftServer` | 服务器实例 |
| `BlockPos` | `net.minecraft.util.math.BlockPos` | 方块坐标 |
| `BlockState` | `net.minecraft.block.BlockState` | 方块状态 |
| `BlockEntity` | `net.minecraft.block.entity.BlockEntity` | 方块实体 |

**使用示例：**

```csharp
// Stub 类型只用在事件处理器签名中
// 你不能在 C# 中 new Player() —— 这没有意义
// 它们只是让 Transpiler 知道要映射到哪个 Java 类型

private static void OnBlockBroken(
    Player player, World world, BlockPos pos, BlockState state, BlockEntity? blockEntity)
{
    // 方法体是 Transpiler Intrinsic
    // 实际操作由生成的 Java 代码执行
}
```

---

## 枚举

### `Rarity`

物品稀有度，决定名称颜色。

```csharp
public enum Rarity
{
    Common,    // 白色   — 普通物品
    Uncommon,  // 黄色   — 附魔书
    Rare,      // 青色   — 下界合金
    Epic       // 品红色 — 龙蛋
}
```

**Java 映射**: `net.minecraft.util.Rarity`

### `SoundGroup`

方块音效组，决定脚步声、放置声、破坏声。

```csharp
public enum SoundGroup
{
    Wood, Stone, Metal, Glass, Wool,
    Sand, Snow, Ladder, Anvil, Slime,
    Honey, Chain, Lantern
}
```

**Java 映射**: `net.minecraft.sound.BlockSoundGroup`

---

## 类型映射参考表

### 原始类型

| C# | Java |
|----|------|
| `bool` | `boolean` |
| `byte` / `sbyte` | `byte` |
| `short` / `ushort` | `short` |
| `int` / `uint` | `int` |
| `long` / `ulong` | `long` |
| `float` | `float` |
| `double` | `double` |
| `decimal` | `double` |
| `char` | `char` |
| `string` | `String` |
| `object` | `Object` |

### 集合类型

| C# | Java |
|----|------|
| `List<T>` | `ArrayList<T>` |
| `Dictionary<K,V>` | `HashMap<K,V>` |
| `HashSet<T>` | `HashSet<T>` |
| `IEnumerable<T>` | `Iterable<T>` |
| `ICollection<T>` | `Collection<T>` |
| `IList<T>` | `List<T>` |
| `IDictionary<K,V>` | `Map<K,V>` |
| `ISet<T>` | `Set<T>` |

### 委托类型

| C# | Java |
|----|------|
| `Action` | `Runnable` |
| `Action<T>` | `Consumer<T>` |
| `Func<R>` | `Supplier<R>` |
| `Func<T,R>` | `Function<T,R>` |
| `Predicate<T>` | `Predicate<T>` |

### FabricCsharp API → Minecraft

| C# | Java |
|----|------|
| `FabricCsharp.Api.Identifier` | `net.minecraft.util.Identifier` |
| `FabricCsharp.Api.RegistryKey<T>` | `net.minecraft.registry.RegistryKey` |
| `FabricCsharp.Api.Item` | `net.minecraft.item.Item` |
| `FabricCsharp.Api.Item.Settings` | `net.minecraft.item.Item.Settings` |
| `FabricCsharp.Api.Block` | `net.minecraft.block.Block` |
| `FabricCsharp.Api.Block.Settings` | `net.minecraft.block.AbstractBlock.Settings` |
| `FabricCsharp.Api.BlockItem` | `net.minecraft.item.BlockItem` |
| `FabricCsharp.Api.Rarity` | `net.minecraft.util.Rarity` |
| `FabricCsharp.Api.Player` | `net.minecraft.entity.player.PlayerEntity` |
| `FabricCsharp.Api.World` | `net.minecraft.world.World` |
| `FabricCsharp.Api.Server` | `net.minecraft.server.MinecraftServer` |
| `FabricCsharp.Api.BlockPos` | `net.minecraft.util.math.BlockPos` |
| `FabricCsharp.Api.BlockState` | `net.minecraft.block.BlockState` |
| `FabricCsharp.Api.BlockEntity` | `net.minecraft.block.entity.BlockEntity` |

---

## 支持的 C# 语法

| C# 语法 | Java 翻译 | 状态 |
|---------|-----------|------|
| `namespace X.Y { }` | `package X.Y;` | ✅ |
| `using X;` | `import X;` | ✅ |
| `class Foo : Bar, IBaz` | `class Foo extends Bar implements IBaz` | ✅ |
| `base.Xxx()` | `super.Xxx()` | ✅ |
| `this.Xxx()` | `this.Xxx()` | ✅ |
| `nameof(X)` | `"X"` (字符串字面量) | ✅ |
| `typeof(T)` | `T.class` | ✅ |
| `x is T` | `x instanceof T` | ✅ |
| `(T)x` | `(T)x` | ✅ |
| `new T[5]` | `new T[5]` | ✅ |
| `new List<T>() { a, b }` | `Arrays.asList(a, b)` | ✅ |
| `foreach (var x in items)` | `for (var x : items)` | ✅ |
| `$"Hello {name}"` 插值字符串 | `"Hello " + name` | ✅ |
| `obj?.Prop` | `obj != null ? obj.getProp() : null` | ✅ |
| Property `T Prop { get; set; }` | `T getProp()` + `void setProp(T v)` | ✅ |
| Lambda `(x) => expr` | `(x) -> expr` | ✅ |
| `switch` 语句 | `switch` 语句 | ✅ |
| `try/catch/finally` | `try/catch/finally` | ✅ |
| `new Obj() { A = 1, B = 2 }` | `new Obj().setA(1).setB(2)` | ✅ |

---

## 不支持的 C# 特性

这些特性被 Roslyn Analyzer 标记为编译错误或警告。

| ID | 特性 | 严重性 | 原因 |
|----|------|--------|------|
| **FC001** | 属性链 `a.b.c` | ❌ Error | 无法可靠映射到 Java getter 链 |
| **FC002** | 空条件运算符 `?.` `?[]` | ❌ Error | Java 无等价语法（但 Transpiler 展开为 null check） |
| **FC003** | `dynamic` 关键字 | ❌ Error | JVM 无动态类型系统 |
| **FC004** | `unsafe` 代码 | ❌ Error | JVM 无指针 |
| **FC005** | `Span<T>` / `Memory<T>` | ❌ Error | 栈引用类型，JVM 不支持 |
| **FC006** | `async` / `await` | ⚠️ Warning | Minecraft 是单线程游戏循环 |
| **FC007** | `struct` 值类型 | ⚠️ Warning | Java 只有 class，struct→class 会丢失值语义 |
| **FC008** | LINQ 查询表达式 | ⚠️ Warning | 翻译可能失败；用方法链或 foreach 替代 |
| **FC009** | Recipe key 不为 char | ❌ Error | 配方键必须是 `char` 字面量 |
| **FC010** | 缺少 `[ModInfo]` 属性 | ❌ Error | Entrypoint 类必须标记 `[ModInfo]` |

---

## 协作模型

```
┌──────────────────────┐
│  你的 C# 代码         │  ← 你写的
│  [ModInfo], Item      │
│  IModInitializer      │
├──────────────────────┤
│  FabricCsharp.Api     │  ← SDK 提供（stub 类型）
│  编译时引用            │
├──────────────────────┤
│  FabricCsharp.Transpiler│  ← 自动运行，你不需要管
│  C# → Java 源码       │
├──────────────────────┤
│  FabricCsharp.CodeGen  │  ← 自动运行，生成 JSON
│  fabric.mod.json 等   │
├──────────────────────┤
│  Gradle + Fabric Loom │  ← 自动运行
│  JVM 字节码编译       │
├──────────────────────┤
│  my-mod.jar           │  ← 最终产物
│  标准 Fabric JAR      │
└──────────────────────┘
```

## Analyzer 错误速查

当你看到以下错误时：

| 错误 | 解决方案 |
|------|----------|
| `FC001 Property chain detected` | 将属性链提取为局部变量：`McPlayer p = src.Player; p.Heal(10);` |
| `FC002 Null conditional not supported` | 先用 `if (obj != null)` 检查，再访问属性 |
| `FC006 async/await not recommended` | 使用 `McScheduler.RunLater()` 替代异步操作 |
| `FC008 LINQ may fail` | 改用 `foreach` 循环或 `.Where().Select()` 方法链（而非查询表达式） |
| `FC010 Missing [ModInfo]` | 给实现了 `IModInitializer` 的类加上 `[ModInfo]` 属性 |
