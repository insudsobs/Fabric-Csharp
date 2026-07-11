using FabricCsharp.Api;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.CsParser;

/// <summary>
/// Extracts mod metadata from [ModInfo] attributes on C# classes.
/// </summary>
public class ModMetadataExtractor
{
    /// <summary>
    /// Parses a ModInfoAttribute from a class declaration's attributes.
    /// Returns null if no [ModInfo] attribute is found.
    /// </summary>
    public static ModMetadata? ExtractMetadata(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        foreach (var attributeList in classDecl.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var typeInfo = semanticModel.GetTypeInfo(attribute);
                if (typeInfo.Type?.Name == nameof(ModInfoAttribute) ||
                    typeInfo.Type?.ToDisplayString() == "FabricCsharp.Api.ModInfoAttribute")
                {
                    return ParseModInfoAttribute(attribute, semanticModel);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the class with [ModInfo] attribute in a syntax tree.
    /// </summary>
    public static (ClassDeclarationSyntax Class, ModMetadata Metadata)? FindModClass(
        SyntaxTree syntaxTree, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetCompilationUnitRoot();

        foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var metadata = ExtractMetadata(classDecl, semanticModel);
            if (metadata != null)
            {
                return (classDecl, metadata);
            }
        }

        return null;
    }

    private static ModMetadata ParseModInfoAttribute(AttributeSyntax attribute, SemanticModel semanticModel)
    {
        var metadata = new ModMetadata();

        if (attribute.ArgumentList == null)
            return metadata;

        foreach (var arg in attribute.ArgumentList.Arguments)
        {
            var name = arg.NameEquals?.Name.Identifier.Text;
            var value = ExtractStringValue(arg.Expression);

            switch (name)
            {
                case "Id":
                    metadata.Id = value ?? string.Empty;
                    break;
                case "Name":
                    metadata.Name = value ?? string.Empty;
                    break;
                case "Version":
                    metadata.Version = value ?? string.Empty;
                    break;
                case "Description":
                    metadata.Description = value;
                    break;
                case "License":
                    metadata.License = value;
                    break;
                case "Icon":
                    metadata.Icon = value;
                    break;
                case "Environment":
                    metadata.Environment = value ?? "*";
                    break;
                case "Authors":
                    metadata.Authors = ExtractStringArray(arg.Expression);
                    break;
                case "Contributors":
                    metadata.Contributors = ExtractStringArray(arg.Expression);
                    break;
            }
        }

        return metadata;
    }

    private static string? ExtractStringValue(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal &&
            literal.Kind() == SyntaxKind.StringLiteralExpression)
        {
            var text = literal.Token.ValueText;
            return text;
        }

        if (expression is ConditionalExpressionSyntax)
        {
            // Not supported — return null
            return null;
        }

        return expression.ToString().Trim('"');
    }

    private static string[]? ExtractStringArray(ExpressionSyntax expression)
    {
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            return implicitArray.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.Kind() == SyntaxKind.StringLiteralExpression)
                .Select(l => l.Token.ValueText)
                .ToArray();
        }

        if (expression is ArrayCreationExpressionSyntax arrayExpr)
        {
            return arrayExpr.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.Kind() == SyntaxKind.StringLiteralExpression)
                .Select(l => l.Token.ValueText)
                .ToArray();
        }

        if (expression is CollectionExpressionSyntax collectionExpr)
        {
            return collectionExpr.Elements
                .OfType<ExpressionElementSyntax>()
                .Select(e => e.Expression)
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.Kind() == SyntaxKind.StringLiteralExpression)
                .Select(l => l.Token.ValueText)
                .ToArray();
        }

        return null;
    }
}

