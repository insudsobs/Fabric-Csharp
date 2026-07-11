using FabricCsharp.Api;

namespace FabricModTemplate;

[ModInfo(
    Id = "fabric-mod-id",
    Name = "My Fabric Mod",
    Version = "1.0.0",
    Description = "A Minecraft Fabric mod written in C#",
    Authors = new[] { "YourName" },
    License = "MIT"
)]
public class ModEntry : IModInitializer
{
    public void OnInitialize()
    {
        // This is where your mod initialization code goes.
        // Register items, blocks, events, commands, etc.
    }
}
