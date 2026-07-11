# FabricCsharp 使用教程

从零开始，用 C# 构建一个完整的 Minecraft Fabric mod。

> **前置条件：**
> - [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
> - [JDK 21+](https://adoptium.net/)
> - Minecraft 1.21.x + Fabric Loader 已安装

---

## 目录

1. [快速上手：5 分钟构建你的第一个 mod](#1-快速上手5-分钟构建你的第一个-mod)
2. [添加自定义物品](#2-添加自定义物品)
3. [添加自定义方块](#3-添加自定义方块)
4. [注册事件监听](#4-注册事件监听)
5. [添加 Mixin（字节码注入）](#5-添加-mixin字节码注入)
6. [添加网络通信](#6-添加网络通信)
7. [添加命令](#7-添加命令)
8. [处理物品资源](#8-处理物品资源)
9. [调试技巧](#9-调试技巧)
10. [常见问题与解决方案](#10-常见问题与解决方案)
11. [项目结构最佳实践](#11-项目结构最佳实践)

---

## 1. 快速上手：5 分钟构建你的第一个 mod

### 1.1 创建项目

```bash
# 安装项目模板（即将在 NuGet 上发布）
dotnet new install FabricCsharp.Templates

# 创建新 mod 项目
dotnet new fabric-mod -n MyFirstMod --ModId my-first-mod --Author "YourName"

# 进入项目目录
cd MyFirstMod
```

如果模板尚未安装，手动创建：

```bash
dotnet new classlib -n MyFirstMod
cd MyFirstMod
```

编辑 `MyFirstMod.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ModId>my-first-mod</ModId>
    <MinecraftVersion>1.21.4</MinecraftVersion>
    <FabricLoaderVersion>0.16.10</FabricLoaderVersion>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="path/to/FabricCsharp.Api.csproj" />
  </ItemGroup>
</Project>
```

### 1.2 写你的第一个 Mod 类

替换 `Class1.cs`：

```csharp
using FabricCsharp.Api;

namespace MyFirstMod;

[ModInfo(
    Id = "my-first-mod",
    Name = "My First Mod",
    Version = "1.0.0",
    Description = "My first C# Fabric mod!",
    Authors = new[] { "YourName" },
    License = "MIT"
)]
public class MyFirstMod : IModInitializer
{
    public void OnInitialize()
    {
        // mod 初始化逻辑
    }
}
```

### 1.3 构建

```bash
dotnet build
```

构建成功后，JAR 文件在 `build/libs/my-first-mod-1.0.0.jar`。

### 1.4 测试

```bash
# 复制到 Minecraft mods 文件夹
cp build/libs/my-first-mod-1.0.0.jar ~/Library/Application\ Support/minecraft/mods/

# 启动 Minecraft + Fabric
# 在 Mod Menu 中应该能看到 "My First Mod"
```

---

## 2. 添加自定义物品

### 2.1 定义物品类

创建 `Items/RubyItem.cs`：

```csharp
using FabricCsharp.Api;

namespace MyFirstMod.Items;

/// <summary>
/// 红宝石 — 一种稀有材料物品
/// 从红宝石矿石掉落，可用于合成高级装备
/// </summary>
public class RubyItem : Item
{
    public RubyItem(Settings settings) : base(settings)
    {
    }
}
```

### 2.2 在 Mod 入口中注册

```csharp
// 在 MyFirstMod.cs 中添加：

// 1. 定义注册键
public static readonly RegistryKey<Item> RubyKey =
    RegistryKey<Item>.Of("my_first_mod", "ruby");

// 2. 注册物品
public static readonly Item Ruby = Registries.Register(
    RubyKey,
    () => new RubyItem(new Item.Settings
    {
        MaxCount = 64,
        Rarity = Rarity.Uncommon  // 黄色名称
    }));
```

### 2.3 物品 Setting 常见配置

```csharp
// 普通材料 — 64 堆叠
new Item.Settings { MaxCount = 64 }

// 工具 — 不可堆叠，有耐久
new Item.Settings { MaxCount = 1, MaxDamage = 250 }

// 食物 — 通常由 FoodComponent 控制，这里仅示意
new Item.Settings { MaxCount = 16 }

// 稀有物品 — 蓝色名称，防火
new Item.Settings { MaxCount = 64, Rarity = Rarity.Epic, Fireproof = true }

// 无法破坏的物品
new Item.Settings { MaxCount = 1, MaxDamage = 0 }
```

### 2.4 物品类型变体

```csharp
// 剑
public class RubySword : Item
{
    public RubySword(Settings settings) : base(settings) { }
}

// 镐
public class RubyPickaxe : Item
{
    public RubyPickaxe(Settings settings) : base(settings) { }
}

// 注册方式完全相同
var swordKey = RegistryKey<Item>.Of("my_mod", "ruby_sword");
var sword = Registries.Register(swordKey,
    () => new RubySword(new Item.Settings { MaxCount = 1 }));
```

---

## 3. 添加自定义方块

### 3.1 定义方块类

创建 `Blocks/RubyOreBlock.cs`：

```csharp
using FabricCsharp.Api;

namespace MyFirstMod.Blocks;

public class RubyOreBlock : Block
{
    public RubyOreBlock(Settings settings) : base(settings)
    {
    }
}
```

### 3.2 注册方块

```csharp
// 在 MyFirstMod.cs 中添加：

public static readonly RegistryKey<Block> RubyOreKey =
    RegistryKey<Block>.Of("my_first_mod", "ruby_ore");

public static readonly Block RubyOre = Registries.Register(
    RubyOreKey,
    () => new RubyOreBlock(new Block.Settings
    {
        Hardness = 3.0f,
        Resistance = 3.0f,
        RequiresTool = true,
        SoundGroup = SoundGroup.Stone
    }));

// 注册方块后，BlockItem 会被自动生成
// 等价于 Java: Items.register(block);
```

### 3.3 Block.Settings 常见配置

```csharp
// 矿石
new Block.Settings
{
    SoundGroup = SoundGroup.Stone
}.HardnessAndResistance(3.0f, 3.0f).RequiresCorrectTool()

// 泥土类
new Block.Settings
{
    SoundGroup = SoundGroup.Sand
}.HardnessAndResistance(0.5f, 2.5f)

// 金属块
new Block.Settings
{
    SoundGroup = SoundGroup.Metal
}.HardnessAndResistance(5.0f, 6.0f).RequiresCorrectTool()

// 玻璃类
new Block.Settings
{
    SoundGroup = SoundGroup.Glass
}.HardnessAndResistance(0.3f, 0.3f)
```

---

## 4. 注册事件监听

### 4.1 创建事件处理器

创建 `EventHandlers.cs`：

```csharp
using FabricCsharp.Api;

namespace MyFirstMod;

public static class EventHandlers
{
    private static bool _registered;

    /// <summary>
    /// 所有事件监听器的注册入口
    /// 防止重复注册
    /// </summary>
    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        // ═══ 方块事件 ═══
        Events.BlockBreakAfter += OnBlockBroken;

        // ═══ 服务器生命周期 ═══
        Events.ServerStarting += OnServerStarting;
        Events.ServerStarted += OnServerStarted;
        Events.ServerStopping += OnServerStopping;

        // ═══ 玩家事件 ═══
        Events.PlayerJoin += OnPlayerJoin;
    }

    // ──── 事件处理器实现 ────

    /// <summary>
    /// 方块被破坏后触发（After = 方块已移除）。
    /// 用于在方块被破坏后执行额外逻辑。
    /// </summary>
    private static void OnBlockBroken(
        Player player, World world, BlockPos pos,
        BlockState state, BlockEntity? blockEntity)
    {
        // Transpiler Intrinsic: 方法体由 Java 侧代码实现
        // 此处写占位逻辑
    }

    /// <summary>
    /// 服务器启动中 — 在加载世界之前
    /// 适合在这里注册自定义配方的序列化器
    /// </summary>
    private static void OnServerStarting(Server server)
    {
        // Transpiler Intrinsic
    }

    /// <summary>
    /// 服务器已完全启动 — 所有世界已加载
    /// </summary>
    private static void OnServerStarted(Server server)
    {
        // Transpiler Intrinsic
    }

    /// <summary>
    /// 服务器停止中
    /// 适合在这里保存自定义数据
    /// </summary>
    private static void OnServerStopping(Server server)
    {
        // Transpiler Intrinsic
    }

    /// <summary>
    /// 玩家加入服务器
    /// </summary>
    private static void OnPlayerJoin(Player player)
    {
        // Transpiler Intrinsic
    }
}
```

### 4.2 在 Mod 入口中调用

```csharp
public class MyFirstMod : IModInitializer
{
    public void OnInitialize()
    {
        // 必须在 OnInitialize() 中调用
        EventHandlers.Register();
    }
}
```

### 4.3 事件执行顺序

```
JVM 启动
  → preLaunch (PreLaunchEntrypoint)
  → main (IModInitializer.OnInitialize)        ← 你的主入口
  → client/server (IClientModInitializer 或 IDedicatedServerModInitializer)
  → 游戏循环开始
      → 事件自由触发
```

---

## 5. 添加 Mixin（字节码注入）

> **注意：** Mixin 是最复杂的 Fabric 特性，目前 FabricCsharp 支持通过 C# 属性声明简单的 Mixin。

### 5.1 创建一个标题界面 Mixin

```csharp
using FabricCsharp.Api;

namespace MyFirstMod.Mixins;

/// <summary>
/// 在 Minecraft 标题界面添加自定义文字。
/// 使用 @Inject 在 init() 方法的尾部注入代码。
/// </summary>
[Mixin(typeof(TitleScreen))]
public abstract class TitleScreenMixin
{
    /// <summary>
    /// 在 TitleScreen.init() 方法尾部注入。
    /// CallbackInfo 是 Mixin 的回调控制对象。
    /// </summary>
    [Inject(Method = "init", At = AtType.Tail)]
    private void OnInit(CallbackInfo info)
    {
        // 添加自定义逻辑
        // Transpiler 生成 Java mixin:
        //
        // @Inject(method = "init", at = @At("TAIL"))
        // private void onInit(CallbackInfo info) {
        //     ...
        // }
    }
}
```

### 5.2 Mixin 注解映射

| C# 属性 | Java 注解 | 说明 |
|--------|-----------|------|
| `[Mixin(typeof(T))]` | `@Mixin(T.class)` | 目标类 |
| `[Inject(Method = "m", At = AtType.Head)]` | `@Inject(method = "m", at = @At("HEAD"))` | 方法开始处注入 |
| `[Inject(Method = "m", At = AtType.Tail)]` | `@Inject(method = "m", at = @At("TAIL"))` | 方法尾部注入 |
| `[Inject(Method = "m", At = AtType.Return)]` | `@Inject(method = "m", at = @At("RETURN"))` | return 前注入 |
| `[Overwrite]` | `@Overwrite` | 完全覆盖目标方法 |

### 5.3 限制

- ❌ `@Redirect` / `@ModifyVariable` / `@ModifyConstant` 暂不支持
- ❌ 需要访问局部变量的 injection point（`@At("INVOKE")` 等）暂不支持
- ✅ 对简单场景（Tail / Head / Overwrite + CallbackInfo）完全支持
- 🔧 复杂 Mixin 可以写原生 Java 类放在项目中（混合项目）

---

## 6. 添加网络通信

### 6.1 定义 Payload

```csharp
using FabricCsharp.Api;

namespace MyFirstMod.Network;

/// <summary>
/// 自定义网络数据包。
/// 用于在客户端和服务器之间传输自定义数据。
/// </summary>
[PacketPayload("my_first_mod:ruby_count")]
public record RubyCountPayload(int count) : ICustomPacketPayload
{
    // Transpiler 会自动生成对应的 Java CustomPacketPayload record
    // 包含 StreamCodec 和注册逻辑
}
```

### 6.2 发送数据（预计 Phase 2 完整支持）

```csharp
// C# 写法（Transpiler 翻译到 Java）:
//
// Server → Client:
//   ServerPlayNetworking.send(player, new RubyCountPayload(5));
//
// Client → Server:
//   ClientPlayNetworking.send(new RubyCountPayload(5));
```

---

## 7. 添加命令

```csharp
// 在 OnInitialize() 中注册命令

public void OnInitialize()
{
    // 注册简单命令
    McCommand.Register("mycommand", context =>
    {
        // context.Source → CommandSourceStack
        // 发送反馈消息给执行者
    });
}
```

> 📅 完整的命令系统将在 Phase 2 中实现。

---

## 8. 处理物品资源

### 8.1 纹理放置

```
Assets/
  textures/
    item/
      ruby.png           ← 物品纹理（PNG，建议 16×16）
    block/
      ruby_ore.png       ← 方块纹理
```

### 8.2 音效

```
Assets/
  sounds/
    ruby_pickup.ogg      ← 音效文件
```

### 8.3 自动生成的资源

构建时 FabricCsharp 会自动生成：

| 文件 | 路径 |
|------|------|
| `fabric.mod.json` | `build/resources/` |
| `ruby_ore.json` (blockstate) | `build/resources/assets/.../blockstates/` |
| `ruby_ore.json` (model) | `build/resources/assets/.../models/block/` |
| `ruby.json` (item model) | `build/resources/assets/.../models/item/` |
| `ruby.json` (item definition, 1.21.4+) | `build/resources/assets/.../items/` |
| `en_us.json` (lang) | `build/resources/assets/.../lang/` |
| `modid.mixins.json` | `build/resources/` |

---

## 9. 调试技巧

### 9.1 查看生成的 Java 代码

构建后查看 `build/src/main/java/` 目录：

```bash
ls -la build/src/main/java/MyFirstMod/
# MyFirstMod.java
# Items/RubyItem.java
# EventHandlers.java
```

### 9.2 使用 Source Map 定位错误

如果 Gradle 构建报 Java 编译错误，搜索生成的 `.java` 文件中的注释：

```java
// C# source: RubyItem.cs:12   ← 这是你 C# 代码的行号
public class RubyItem extends Item {
```

### 9.3 查看生成的 fabric.mod.json

```bash
cat build/src/main/resources/fabric.mod.json | python3 -m json.tool
```

### 9.4 验证 JAR 内容

```bash
jar tf build/libs/my-first-mod-1.0.0.jar | head -20
```

---

## 10. 常见问题与解决方案

### ❌ `FC001 Property chain detected`

```csharp
// ❌ 错误
src.Player.Heal(10);

// ✅ 正确 — 提取为局部变量
McPlayer p = src.Player;
p.Heal(10);
```

### ❌ `FC002 Null conditional not supported`

```csharp
// ❌ 错误
obj?.DoSomething();

// ✅ 正确
if (obj != null)
{
    obj.DoSomething();
}
```

### ❌ `FC010 Missing [ModInfo]`

```csharp
// ❌ 错误 — 缺少 [ModInfo]
public class MyMod : IModInitializer { }

// ✅ 正确
[ModInfo(Id = "my-mod", Name = "My Mod", Version = "1.0.0")]
public class MyMod : IModInitializer { }
```

### ❌ `FC006 async/await not recommended`

```csharp
// ❌ 不推荐
async void OnBlockBroken(...)
{
    await Task.Delay(1000);
    // do stuff
}

// ✅ 使用 Fabric 调度器（Phase 2 支持）
// McScheduler.RunLater(() => { ... }, 20); // 20 ticks = 1 秒
```

### ❌ Transpiler 说找不到 [ModInfo]

检查：
1. `[ModInfo]` 是否在**实现了 `IModInitializer` 的类**上
2. `Id`、`Name`、`Version` 是否**全部赋值**
3. 类是否是 `public`

### ❌ Gradle 报 "JAVA_HOME not set"

设置 JDK 21 路径：

```xml
<!-- 在 .csproj 中添加 -->
<PropertyGroup>
  <JavaHome>/path/to/jdk-21</JavaHome>
</PropertyGroup>
```

### ❌ 构建后 JAR 未生成

检查：
1. `dotnet restore` 是否成功
2. JDK 21 是否安装且有 `gradle` 在 PATH 上
3. 构建日志中 Gradle 步骤是否成功

---

## 11. 项目结构最佳实践

### 推荐布局

```
MyMod/
├── MyMod.csproj               # 项目文件
├── MyMod.cs                   # 主入口：[ModInfo] + IModInitializer
├── MyModClient.cs             # 客户端入口（可选）
├── Items/                     # 所有自定义物品
│   ├── RubyItem.cs
│   ├── RubySword.cs
│   └── RubyPickaxe.cs
├── Blocks/                    # 所有自定义方块
│   ├── RubyOreBlock.cs
│   └── RubyBlock.cs
├── Entities/                  # 自定义实体（可选）
│   └── RubyGolem.cs
├── Mixins/                    # Mixin 类（可选）
│   └── TitleScreenMixin.cs
├── Network/                   # 网络包定义（可选）
│   └── RubyCountPayload.cs
├── EventHandlers.cs           # 事件处理器（可选但推荐）
├── Assets/                    # 纹理、音效、模型
│   ├── textures/
│   │   ├── item/
│   │   └── block/
│   ├── sounds/
│   └── models/
└── README.md                  # mod 说明文档
```

### 命名规范

| 项目 | 规范 | 示例 |
|------|------|------|
| ModId | `lower-kebab-case` | `sapphire-mod` |
| C# 类名 | `PascalCase` | `RubySwordItem` |
| 物品 ID | `snake_case` | `ruby_sword` |
| 注册键 | `PascalCase` + `Key` 后缀 | `RubySwordKey` |
| 事件处理器 | `On` + 事件名 | `OnBlockBroken` |
| 文件 | 一个类 = 一个文件 | `Items/RubySword.cs` |

### 依赖管理

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- 你的 mod 用到的其他 Fabric mod 作为版本号 -->
    <SapphireModVersion>1.0.0</SapphireModVersion>
  </PropertyGroup>
  <ItemGroup>
    <!-- 如果 SapphireMod 也发布了 C# NuGet 包，可以直接引用 -->
    <!-- 你的 mod 的 fabric.mod.json 中自动添加依赖声明 -->
  </ItemGroup>
</Project>
```

---

## 进阶指南

### 单个 mod 注册多种类型的完整示例

```csharp
[ModInfo(Id = "complete-mod", Name = "Complete Mod", Version = "1.0.0")]
public class CompleteMod : IModInitializer
{
    // ═══ 方块 ═══
    public static readonly RegistryKey<Block> DemoOreKey =
        RegistryKey<Block>.Of("complete_mod", "demo_ore");

    public static readonly Block DemoOre = Registries.Register(
        DemoOreKey,
        () => new DemoOreBlock(new Block.Settings
        {
            SoundGroup = SoundGroup.Stone
        }.HardnessAndResistance(3.0f, 3.0f).RequiresCorrectTool()));

    // ═══ 物品（材料） ═══
    public static readonly RegistryKey<Item> DemoGemKey =
        RegistryKey<Item>.Of("complete_mod", "demo_gem");

    public static readonly Item DemoGem = Registries.Register(
        DemoGemKey,
        () => new DemoGemItem(new Item.Settings { Rarity = Rarity.Rare }));

    // ═══ 物品（剑） ═══
    public static readonly RegistryKey<Item> DemoSwordKey =
        RegistryKey<Item>.Of("complete_mod", "demo_sword");

    public static readonly Item DemoSword = Registries.Register(
        DemoSwordKey,
        () => new DemoSwordItem(new Item.Settings { MaxCount = 1 }));

    // ═══ 初始化 ═══
    public void OnInitialize()
    {
        EventHandlers.Register();
    }
}
```

### 多文件 mod 的组织

```
CompleteMod/
├── CompleteMod.cs              # 只放入口和注册声明
├── CompleteModClient.cs        # 客户端初始化
├── Items/
│   ├── DemoGemItem.cs          # 每个物品一个文件
│   ├── DemoSwordItem.cs
│   ├── DemoPickaxeItem.cs
│   └── DemoAxeItem.cs
├── Blocks/
│   ├── DemoOreBlock.cs
│   └── DemoStorageBlock.cs
├── EventHandlers.cs            # 所有事件集中管理
├── Mixins/
│   ├── EntityRenderMixin.cs
│   └── ItemTooltipMixin.cs
├── Network/
│   ├── DemoSyncPayload.cs
│   └── DemoConfigPayload.cs
└── Assets/
    └── textures/
```

---

## 下一步

1. ✅ 已掌握：创建一个含物品和方块的简单 mod
2. 📖 学习 [API 参考文档](API-REFERENCE.md) 了解所有可用类型
3. 🔗 参考 [SapphireMod 示例](../samples/SapphireMod/) 查看完整 mod 实现
4. 🤝 查看 [README](../README.md) 了解项目架构和贡献指南
5. 📅 Phase 2 将支持：完整物品/方块 API、实体系统、网络 API、命令系统
