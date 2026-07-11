using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Main entry point for translating C# syntax trees to Java source code.
/// Coordinates walking the syntax tree and delegating to specialized translators.
/// </summary>
public class SyntaxTranslator
{
    private readonly TypeMapper _typeMapper;
    private readonly StatementTranslator _statementTranslator;
    private readonly ExpressionTranslator _expressionTranslator;
    private readonly ClassTranslator _classTranslator;

    public SyntaxTranslator(TypeMapper? typeMapper = null)
    {
        _typeMapper = typeMapper ?? new TypeMapper();
        _statementTranslator = new StatementTranslator(_typeMapper);
        _expressionTranslator = new ExpressionTranslator(_typeMapper);
        _classTranslator = new ClassTranslator(_typeMapper, _statementTranslator, _expressionTranslator);
    }

    /// <summary>
    /// Translates a single C# source file to Java.
    /// </summary>
    public string TranslateFile(SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        var writer = new JavaWriter();
        var root = syntaxTree.GetCompilationUnitRoot();

        // Translate namespace → package
        var namespaceDecl = root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault()
                            ?? root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault() as MemberDeclarationSyntax;

        var packageName = "mod"; // default package
        if (namespaceDecl is NamespaceDeclarationSyntax ns)
        {
            packageName = ns.Name.ToString();
        }
        else if (namespaceDecl is FileScopedNamespaceDeclarationSyntax fns)
        {
            packageName = fns.Name.ToString();
        }

        writer.WritePackage(packageName);

        // Collect and translate all class declarations
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classDecl in classes)
        {
            _classTranslator.Translate(classDecl, semanticModel, writer);
        }

        return writer.ToString();
    }

    /// <summary>
    /// Translates a single C# source file and returns the suggested Java filename.
    /// </summary>
    public (string JavaCode, string JavaFileName) TranslateFileWithName(
        SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        var javaCode = TranslateFile(syntaxTree, semanticModel);

        var root = syntaxTree.GetCompilationUnitRoot();
        var mainClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var className = mainClass?.Identifier.Text ?? "Unknown";

        // Determine the package directory
        var ns = root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault()
                 ?? root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault() as MemberDeclarationSyntax;

        string relativeDir;
        if (ns is NamespaceDeclarationSyntax namespaceDecl)
        {
            relativeDir = namespaceDecl.Name.ToString().Replace('.', '/');
        }
        else if (ns is FileScopedNamespaceDeclarationSyntax fns)
        {
            relativeDir = fns.Name.ToString().Replace('.', '/');
        }
        else
        {
            relativeDir = "";
        }

        var javaFileName = Path.Combine(relativeDir, $"{className}.java");
        return (javaCode, javaFileName);
    }
}
