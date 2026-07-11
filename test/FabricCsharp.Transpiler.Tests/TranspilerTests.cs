using System.Text;
using FabricCsharp.Transpiler.CsParser;
using FabricCsharp.Transpiler.JavaGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FabricCsharp.Transpiler.Tests;

/// <summary>
/// Tests for the core C# → Java transpiler logic.
/// </summary>
public class TranspilerTests
{
    private readonly TypeMapper _typeMapper;
    private readonly SyntaxTranslator _translator;
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp13);

    public TranspilerTests()
    {
        _typeMapper = new TypeMapper();
        _translator = new SyntaxTranslator(_typeMapper);
    }

    [Fact]
    public void TypeMapper_Primitives_MapCorrectly()
    {
        Assert.Equal("String", _typeMapper.MapType("string"));
        Assert.Equal("int", _typeMapper.MapType("int"));
        Assert.Equal("boolean", _typeMapper.MapType("bool"));
        Assert.Equal("float", _typeMapper.MapType("float"));
        Assert.Equal("double", _typeMapper.MapType("double"));
        Assert.Equal("void", _typeMapper.MapType("void"));
        Assert.Equal("boolean", _typeMapper.MapType("bool"));
    }

    [Fact]
    public void TypeMapper_FabricCsharpApiTypes_MapToMinecraft()
    {
        Assert.Equal("net.minecraft.util.Identifier", _typeMapper.MapType("FabricCsharp.Api.Identifier"));
        Assert.Equal("net.minecraft.item.Item", _typeMapper.MapType("FabricCsharp.Api.Item"));
        Assert.Equal("net.minecraft.block.Block", _typeMapper.MapType("FabricCsharp.Api.Block"));
        Assert.Equal("net.minecraft.entity.player.PlayerEntity", _typeMapper.MapType("FabricCsharp.Api.Player"));
    }

    [Fact]
    public void TypeMapper_Collections_MapCorrectly()
    {
        Assert.Equal("java.util.ArrayList<{0}>",
            _typeMapper.MapGenericTypeDefinition("System.Collections.Generic.List`1"));
        Assert.Equal("java.util.HashMap<{0}, {1}>",
            _typeMapper.MapGenericTypeDefinition("System.Collections.Generic.Dictionary`2"));
    }

    [Fact]
    public void Translate_SimpleClass_GeneratesJavaClass()
    {
        var csCode = @"
namespace com.example;

public class HelloMod : IModInitializer
{
    public void OnInitialize()
    {
        Console.WriteLine(""Hello from C#!"");
    }
}
";

        var javaCode = TranslateCode(csCode);

        Assert.Contains("package com.example;", javaCode);
        Assert.Contains("public class HelloMod", javaCode);
        Assert.Contains("void OnInitialize()", javaCode);
        Assert.Contains("Hello from C#!", javaCode);
    }

    [Fact]
    public void Translate_ModInfoAttribute_GeneratesCorrectCode()
    {
        var csCode = @"
using FabricCsharp.Api;

[ModInfo(Id = ""test"", Name = ""Test"", Version = ""1.0.0"")]
public class TestMod : IModInitializer
{
    public void OnInitialize() { }
}
";

        var javaCode = TranslateCode(csCode);
        Assert.Contains("public class TestMod", javaCode);
        Assert.Contains("OnInitialize", javaCode);
    }

    [Fact]
    public void Translate_ForEach_GeneratesJavaForEach()
    {
        var csCode = @"
public class Example
{
    public void Loop(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}
";

        var javaCode = TranslateCode(csCode);
        Assert.Contains("for (var item :", javaCode);
    }

    [Fact]
    public void Translate_IfStatement_GeneratesJavaIf()
    {
        var csCode = @"
public class Example
{
    public void Check(int x)
    {
        if (x > 5)
        {
            Console.WriteLine(""big"");
        }
        else
        {
            Console.WriteLine(""small"");
        }
    }
}
";

        var javaCode = TranslateCode(csCode);
        Assert.Contains("if (x > 5)", javaCode);
        Assert.Contains("else", javaCode);
    }

    [Fact]
    public void Translate_StringInterpolation_GeneratesConcatenation()
    {
        var csCode = @"
public class Example
{
    public void SayHello(string name)
    {
        var msg = $""Hello {name}!"";
        Console.WriteLine(msg);
    }
}
";

        var javaCode = TranslateCode(csCode);
        // Should contain the string concatenation
        Assert.Contains("Hello ", javaCode);
        Assert.Contains("name", javaCode);
    }

    [Fact]
    public void Translate_NullConditional_ExpandsToNullCheck()
    {
        var csCode = @"
public class Example
{
    public void SafeCall(object obj)
    {
        var result = obj?.ToString();
    }
}
";

        var javaCode = TranslateCode(csCode);
        // null conditional becomes ternary null check
        Assert.Contains("!= null", javaCode, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Translate_NameOf_BecomesStringLiteral()
    {
        var csCode = @"
public class Example
{
    public void LogName()
    {
        var n = nameof(Example);
    }
}
";

        var javaCode = TranslateCode(csCode);
        Assert.Contains("\"Example\"", javaCode);
    }

    [Fact]
    public void ModMetadataExtractor_FindsModInfoAttribute()
    {
        var csCode = @"
using FabricCsharp.Api;

[ModInfo(Id = ""my-mod"", Name = ""My Mod"", Version = ""2.0.0"", Description = ""A test mod"", Authors = new[] { ""Alice"", ""Bob"" })]
public class MyMod : IModInitializer
{
    public void OnInitialize() { }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(csCode, ParseOptions);
        var compilation = CreateCompilation(syntaxTree);
        var (classDecl, metadata) = ModMetadataExtractor.FindModClass(syntaxTree, compilation)
            .GetValueOrDefault();

        Assert.NotNull(classDecl);
        Assert.NotNull(metadata);
        Assert.Equal("my-mod", metadata.Id);
        Assert.Equal("My Mod", metadata.Name);
        Assert.Equal("2.0.0", metadata.Version);
        Assert.Equal("A test mod", metadata.Description);
        Assert.Equal(new[] { "Alice", "Bob" }, metadata.Authors);
    }

    [Fact]
    public void JavaWriter_GeneratesValidJava()
    {
        using var writer = new JavaWriter();
        writer.WritePackage("com.example");
        writer.AddImport("java.util.List");
        writer.AddImport("net.minecraft.item.Item");
        writer.WriteImports();

        writer.OpenBrace("public class HelloMod implements ModInitializer");
        writer.WriteLine("@Override");
        writer.OpenBrace("public void onInitialize()");
        writer.WriteLine("System.out.println(\"Hello!\");");
        writer.CloseBrace();
        writer.CloseBrace();

        var output = writer.ToString();

        Assert.Contains("package com.example;", output);
        Assert.Contains("import java.util.List;", output);
        Assert.Contains("import net.minecraft.item.Item;", output);
        Assert.Contains("public class HelloMod", output);
        Assert.Contains("@Override", output);
        Assert.Contains("System.out.println", output);
    }

    /// <summary>
    /// Helper: translates a C# code string to Java.
    /// </summary>
    private string TranslateCode(string csCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(csCode, ParseOptions);
        var compilation = CreateCompilation(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return _translator.TranslateFile(syntaxTree, semanticModel);
    }

    private static CSharpCompilation CreateCompilation(params SyntaxTree[] syntaxTrees)
    {
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FabricCsharp.Api.Item).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "TestMod",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
