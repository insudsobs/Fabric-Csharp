using FabricCsharp.Api;

namespace SapphireMod;

/// <summary>
/// 客户端初始化入口。
/// 仅在客户端启动时运行，用于注册渲染器、颜色提供器等。
/// 注意：不需要单独的 [ModInfo] — 它与主 mod 共享同一个 mod ID。
/// </summary>
public class SapphireModClient : IClientModInitializer
{
    public void OnInitializeClient()
    {
        // 客户端初始化逻辑
        // 例如：注册方块颜色提供器
        // 蓝宝石矿石使用自定义蓝色色调
        //
        // Java 侧会生成:
        // ColorProviderRegistry.BLOCK.register(
        //     (state, world, pos, tintIndex) -> 0xFF_4488FF,
        //     SapphireMod.SAPPHIRE_ORE_BLOCK);
    }
}
