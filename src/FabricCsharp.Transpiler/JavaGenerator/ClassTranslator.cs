using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Translates C# class/interface declarations to Java class/interface declarations.
/// </summary>
public class ClassTranslator
{
    private readonly TypeMapper _typeMapper;
    private readonly StatementTranslator _statementTranslator;
    private readonly ExpressionTranslator _expressionTranslator;

    public ClassTranslator(
        TypeMapper typeMapper,
        StatementTranslator statementTranslator,
        ExpressionTranslator expressionTranslator)
    {
        _typeMapper = typeMapper;
        _statementTranslator = statementTranslator;
        _expressionTranslator = expressionTranslator;
    }

    public void Translate(ClassDeclarationSyntax classDecl, SemanticModel semanticModel, JavaWriter writer)
    {
        var syntaxTree = classDecl.SyntaxTree;
        var location = classDecl.GetLocation();
        var sourceLine = location.GetLineSpan().StartLinePosition.Line + 1;
        writer.WriteSourceMap(syntaxTree.FilePath, sourceLine);

        var modifiers = TranslateModifiers(classDecl);
        var className = classDecl.Identifier.Text;
        var extendsClause = TranslateExtends(classDecl, writer);
        var implementsClause = TranslateImplements(classDecl, writer);

        var declaration = $"{modifiers} class {className}";
        if (extendsClause != null)
            declaration += $" extends {extendsClause}";
        if (implementsClause != null)
            declaration += $" implements {implementsClause}";

        writer.OpenBrace(declaration);

        foreach (var member in classDecl.Members)
        {
            if (member is MethodDeclarationSyntax method)
                TranslateMethod(method, semanticModel, writer);
            else if (member is FieldDeclarationSyntax field)
                TranslateField(field, writer);
            else if (member is PropertyDeclarationSyntax property)
                TranslateProperty(property, writer);
        }

        writer.CloseBrace();
    }

    private static string TranslateModifiers(ClassDeclarationSyntax classDecl)
    {
        var mods = "public";
        if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            mods += " abstract";
        if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            mods += " static";
        return mods;
    }

    private string? TranslateExtends(ClassDeclarationSyntax classDecl, JavaWriter writer)
    {
        var baseList = classDecl.BaseList;
        if (baseList == null) return null;

        foreach (var baseType in baseList.Types)
        {
            var typeName = baseType.Type.ToString();
            var mapped = _typeMapper.MapType(typeName);
            if (mapped != null)
            {
                mapped = SimplifyJavaType(mapped);
                var import = _typeMapper.GetImportForType(typeName);
                if (import != null) writer.AddImport(import);
                return mapped;
            }

            if (!typeName.StartsWith("I"))
                return typeName;
        }

        return null;
    }

    private string? TranslateImplements(ClassDeclarationSyntax classDecl, JavaWriter writer)
    {
        var baseList = classDecl.BaseList;
        if (baseList == null) return null;

        var interfaces = new List<string>();

        foreach (var baseType in baseList.Types)
        {
            var typeName = baseType.Type.ToString();
            var mapped = _typeMapper.MapType(typeName);

            if (mapped != null)
            {
                mapped = SimplifyJavaType(mapped);
                var import = _typeMapper.GetImportForType(typeName);
                if (import != null) writer.AddImport(import);

                if (typeName is "FabricCsharp.Api.IModInitializer" or
                    "FabricCsharp.Api.IClientModInitializer" or
                    "FabricCsharp.Api.IDedicatedServerModInitializer")
                {
                    continue;
                }
            }

            if (typeName.StartsWith("I") || mapped != null)
            {
                interfaces.Add(mapped ?? typeName);
            }
        }

        return interfaces.Count > 0 ? string.Join(", ", interfaces) : null;
    }

    private void TranslateMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, JavaWriter writer)
    {
        var modifiers = TranslateMethodModifiers(method);

        var returnType = method.ReturnType.ToString();
        var mappedReturn = _typeMapper.MapType(returnType) ?? returnType;
        mappedReturn = SimplifyJavaType(mappedReturn);
        var import = _typeMapper.GetImportForType(returnType);
        if (import != null) writer.AddImport(import);

        var parameters = method.ParameterList.Parameters
            .Select(p => TranslateParameter(p, writer))
            .ToList();

        if (method.Identifier.Text is "OnInitialize" or "OnInitializeClient" or "OnInitializeServer")
            writer.WriteLine("@Override");

        var paramList = string.Join(", ", parameters);
        writer.OpenBrace($"{modifiers} {mappedReturn} {method.Identifier.Text}({paramList})");

        if (method.Body != null)
        {
            foreach (var statement in method.Body.Statements)
                _statementTranslator.Translate(statement, writer);
        }

        writer.CloseBrace();
    }

    private static string TranslateMethodModifiers(MethodDeclarationSyntax method)
    {
        var mods = "public";

        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            mods = "public static";
        else if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            mods = "private";
        else if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
            mods = "protected";

        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            mods += " abstract";

        return mods;
    }

    private string TranslateParameter(ParameterSyntax parameter, JavaWriter writer)
    {
        var type = parameter.Type?.ToString() ?? "Object";
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = SimplifyJavaType(mapped);
        var import = _typeMapper.GetImportForType(type);
        if (import != null) writer.AddImport(import);

        return $"{mapped} {parameter.Identifier.Text}";
    }

    private void TranslateField(FieldDeclarationSyntax field, JavaWriter writer)
    {
        var modifiers = "private";

        if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            modifiers = "public";
        else if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
            modifiers = "protected";

        if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            modifiers += " static";
        if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
            modifiers += " final";

        foreach (var variable in field.Declaration.Variables)
        {
            var type = field.Declaration.Type.ToString();
            var mapped = _typeMapper.MapType(type) ?? type;
            mapped = SimplifyJavaType(mapped);
            var import = _typeMapper.GetImportForType(type);
            if (import != null) writer.AddImport(import);

            var initializer = variable.Initializer != null
                ? $" = {_expressionTranslator.Translate(variable.Initializer.Value)}"
                : "";

            writer.WriteLine($"{modifiers} {mapped} {variable.Identifier.Text}{initializer};");
        }
    }

    private void TranslateProperty(PropertyDeclarationSyntax property, JavaWriter writer)
    {
        var type = property.Type.ToString();
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = SimplifyJavaType(mapped);
        var import = _typeMapper.GetImportForType(type);
        if (import != null) writer.AddImport(import);

        var name = property.Identifier.Text;
        var pascalName = char.ToUpper(name[0]) + name[1..];
        var backingField = $"_{char.ToLower(name[0])}{name[1..]}";

        writer.WriteLine($"private {mapped} {backingField};");
        writer.WriteLine();

        if (property.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)) == true)
        {
            writer.WriteLine($"public {mapped} get{pascalName}() {{");
            writer.WriteLine($"    return {backingField};");
            writer.WriteLine("}");
            writer.WriteLine();
        }

        if (property.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.SetKeyword)) == true)
        {
            writer.WriteLine($"public void set{pascalName}({mapped} value) {{");
            writer.WriteLine($"    this.{backingField} = value;");
            writer.WriteLine("}");
            writer.WriteLine();
        }
    }

    private static string SimplifyJavaType(string javaType)
    {
        var lastDot = javaType.LastIndexOf('.');
        return lastDot >= 0 ? javaType[(lastDot + 1)..] : javaType;
    }
}
