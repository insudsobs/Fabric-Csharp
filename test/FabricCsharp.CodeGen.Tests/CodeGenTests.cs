using System.Text.Json;
using FabricCsharp.Api;
using FabricCsharp.CodeGen.Generators;
using Xunit;

namespace FabricCsharp.CodeGen.Tests;

/// <summary>
/// Tests for the CodeGen generators: ModJsonGenerator, AssetGenerator,
/// MixinConfigGenerator, and LangGenerator.
/// </summary>
public class CodeGenTests
{
    // -----------------------------------------------------------------------
    // ModJsonGenerator tests
    // -----------------------------------------------------------------------

    [Fact]
    public void ModJsonGenerator_GeneratesValidJson()
    {
        var generator = new ModJsonGenerator();
        var metadata = new ModMetadata
        {
            Id = "test-mod",
            Name = "Test Mod",
            Version = "1.0.0",
            MainClass = "TestMod",
        };

        var json = generator.Generate(metadata, "1.21.4", "0.16.10");

        Assert.NotNull(json);
        Assert.NotEmpty(json);

        // Verify it is valid JSON
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Required fields should be present
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.Equal("test-mod", id.GetString());

        Assert.True(root.TryGetProperty("name", out var name));
        Assert.Equal("Test Mod", name.GetString());

        Assert.True(root.TryGetProperty("version", out var version));

        // Should have depends with fabricloader and minecraft entries
        Assert.True(root.TryGetProperty("depends", out var depends));
    }

    [Fact]
    public void ModJsonGenerator_IncludesOptionalFields()
    {
        var generator = new ModJsonGenerator();
        var metadata = new ModMetadata
        {
            Id = "optional-mod",
            Name = "Optional Mod",
            Version = "2.0.0",
            Description = "A mod with optional fields",
            Authors = new[] { "Alice", "Bob" },
            MainClass = "OptionalMod",
        };

        var json = generator.Generate(metadata, "1.21.4", "0.16.10");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("description", out var desc));
        Assert.Equal("A mod with optional fields", desc.GetString());

        Assert.True(root.TryGetProperty("authors", out var authors));
        Assert.Equal(2, authors.GetArrayLength());
    }

    // -----------------------------------------------------------------------
    // AssetGenerator tests
    // -----------------------------------------------------------------------

    [Fact]
    public void AssetGenerator_ItemModel_HasCorrectStructure()
    {
        var generator = new AssetGenerator();
        var json = generator.GenerateItemModel("mymod", "ruby");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("parent", out var parent));
        Assert.Equal("item/generated", parent.GetString());

        Assert.True(root.TryGetProperty("textures", out var textures));
        Assert.True(textures.TryGetProperty("layer0", out var layer0));
        Assert.Equal("mymod:item/ruby", layer0.GetString());
    }

    [Fact]
    public void AssetGenerator_BlockState_HasVariants()
    {
        var generator = new AssetGenerator();
        var json = generator.GenerateBlockState("mymod", "ruby_block");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("variants", out var variants));
        Assert.Equal(JsonValueKind.Object, variants.ValueKind);
    }

    // -----------------------------------------------------------------------
    // MixinConfigGenerator tests
    // -----------------------------------------------------------------------

    [Fact]
    public void MixinConfigGenerator_CreatesValidJson()
    {
        var generator = new MixinConfigGenerator();
        var json = generator.Generate(
            "testmod",
            "testmod.mixin",
            new[] { "MixinA", "MixinB" },
            new[] { "ClientMixin" },
            null);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("required", out var required));
        Assert.True(required.GetBoolean());

        Assert.True(root.TryGetProperty("package", out var package));
        Assert.Equal("testmod.mixin", package.GetString());

        Assert.True(root.TryGetProperty("mixins", out var mixins));
        Assert.Equal(2, mixins.GetArrayLength());

        Assert.True(root.TryGetProperty("client", out var client));
        Assert.Equal(1, client.GetArrayLength());

        Assert.True(root.TryGetProperty("injectors", out var injectors));
    }

    // -----------------------------------------------------------------------
    // LangGenerator tests
    // -----------------------------------------------------------------------

    [Fact]
    public void LangGenerator_ItemKey_FormatsCorrectly()
    {
        var key = LangGenerator.ItemKey("mymod", "ruby");

        Assert.Equal("item.mymod.ruby", key);
    }
}
