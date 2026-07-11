using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.CsParser;

/// <summary>
/// Discovers and collects all C# source files relevant to mod transpilation.
/// </summary>
public class ModFileCollector
{
    private readonly List<string> _sourceFiles = new();

    /// <summary>
    /// Collects all .cs files from a project directory, recursively.
    /// </summary>
    public ModFileCollector AddFromDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            _sourceFiles.AddRange(Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories));
        }
        return this;
    }

    /// <summary>
    /// Adds specific source files.
    /// </summary>
    public ModFileCollector AddFiles(IEnumerable<string> files)
    {
        _sourceFiles.AddRange(files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)));
        return this;
    }

    /// <summary>
    /// Gets the collected source files.
    /// </summary>
    public IReadOnlyList<string> SourceFiles => _sourceFiles;

    /// <summary>
    /// Builds a Roslyn Compilation from the collected source files.
    /// </summary>
    public CSharpCompilation BuildCompilation()
    {
        var syntaxTrees = _sourceFiles
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToList();

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        return CSharpCompilation.Create(
            "FabricCsharpMod",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Clears the collector for reuse.
    /// </summary>
    public ModFileCollector Clear()
    {
        _sourceFiles.Clear();
        return this;
    }
}
