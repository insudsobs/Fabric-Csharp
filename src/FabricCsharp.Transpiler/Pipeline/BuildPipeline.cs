using FabricCsharp.Api;
using FabricCsharp.Transpiler.CsParser;
using FabricCsharp.Transpiler.JavaGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.Pipeline;

/// <summary>
/// Orchestrates the complete C# → Java transpilation pipeline.
/// </summary>
public class BuildPipeline
{
    private readonly ModFileCollector _collector;
    private readonly SyntaxTranslator _translator;
    private readonly TypeMapper _typeMapper;

    public string ModSourcesDirectory { get; set; } = "src";
    public string OutputDirectory { get; set; } = "build/java";
    public string AssetsDirectory { get; set; } = "Assets";
    public string MinecraftVersion { get; set; } = "1.21.4";
    public string FabricLoaderVersion { get; set; } = "0.16.10";
    public string GradleDirectory { get; set; } = "build";

    public BuildPipeline(
        ModFileCollector? collector = null,
        SyntaxTranslator? translator = null,
        TypeMapper? typeMapper = null)
    {
        _collector = collector ?? new ModFileCollector();
        _translator = translator ?? new SyntaxTranslator();
        _typeMapper = typeMapper ?? new TypeMapper();
    }

    /// <summary>
    /// Executes the full transpilation pipeline:
    /// 1. Collect C# source files
    /// 2. Build Roslyn compilation
    /// 3. Extract mod metadata
    /// 4. Translate each .cs file to .java
    /// 5. Return translation results
    /// </summary>
    public async Task<TranspilationResult> ExecuteAsync(CancellationToken ct = default)
    {
        var result = new TranspilationResult();

        // Step 1: Collect sources
        _collector.Clear().AddFromDirectory(ModSourcesDirectory);
        var sourceFiles = _collector.SourceFiles;

        if (sourceFiles.Count == 0)
        {
            result.Errors.Add("No C# source files found.");
            return result;
        }

        // Step 2: Build compilation
        var compilation = _collector.BuildCompilation();
        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (diagnostics.Count > 0)
        {
            result.Errors.AddRange(diagnostics.Select(d => d.ToString()));
            // Continue anyway for partial translation
        }

        // Step 3: Find mod entry point and extract metadata
        ModMetadata? modMetadata = null;
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var (classDecl, metadata) = ModMetadataExtractor.FindModClass(syntaxTree, compilation) ?? default;
            if (classDecl != null && metadata != null)
            {
                modMetadata = metadata;
                modMetadata.MainClass = classDecl.Identifier.Text;

                // Also find client/server entry points
                FindClientServerClasses(compilation, metadata);
                break;
            }
        }

        if (modMetadata == null)
        {
            result.Errors.Add("No class with [ModInfo] attribute found. Add [ModInfo] to your mod's main class.");
            return result;
        }

        if (!modMetadata.IsValid)
        {
            result.Errors.Add("[ModInfo] must have at least Id, Name, and Version specified.");
            return result;
        }

        result.ModMetadata = modMetadata;

        // Step 4: Translate each C# file to Java
        var outputDir = Path.Combine(OutputDirectory, "src", "main", "java");
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var (javaCode, javaFileName) = _translator.TranslateFileWithName(syntaxTree, semanticModel);

            var outputPath = Path.Combine(outputDir, javaFileName);
            var outputFileDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputFileDir))
                Directory.CreateDirectory(outputFileDir);

            await File.WriteAllTextAsync(outputPath, javaCode, ct);
            result.GeneratedJavaFiles.Add((outputPath, javaFileName));
        }

        result.Success = true;
        return result;
    }

    private void FindClientServerClasses(Compilation compilation, ModMetadata metadata)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var root = syntaxTree.GetCompilationUnitRoot();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var baseList = classDecl.BaseList;
                if (baseList == null) continue;

                foreach (var baseType in baseList.Types)
                {
                    var typeName = baseType.Type.ToString();
                    if (typeName == "IClientModInitializer" ||
                        typeName == "FabricCsharp.Api.IClientModInitializer")
                    {
                        metadata.ClientClass = classDecl.Identifier.Text;
                    }
                    if (typeName == "IDedicatedServerModInitializer" ||
                        typeName == "FabricCsharp.Api.IDedicatedServerModInitializer")
                    {
                        metadata.ServerClass = classDecl.Identifier.Text;
                    }
                }
            }
        }
    }
}

/// <summary>
/// Result of a transpilation run.
/// </summary>
public class TranspilationResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public ModMetadata? ModMetadata { get; set; }
    public List<(string OutputPath, string JavaFileName)> GeneratedJavaFiles { get; set; } = new();
}
