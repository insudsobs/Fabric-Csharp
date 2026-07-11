using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Translates a C# class marked with [Mixin] to a Java Mixin class with
/// SpongePowered Mixin annotations (@Mixin, @Inject, @Overwrite, etc.).
/// </summary>
public class MixinTranslator
{
    private readonly TypeMapper _typeMapper;
    private readonly StatementTranslator _statementTranslator;
    private readonly ExpressionTranslator _expressionTranslator;

    public MixinTranslator(
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

        // Add SpongePowered Mixin imports
        writer.AddImport("org.spongepowered.asm.mixin.Mixin");
        writer.AddImport("org.spongepowered.asm.mixin.injection.At");
        writer.AddImport("org.spongepowered.asm.mixin.injection.Inject");
        writer.AddImport("org.spongepowered.asm.mixin.Overwrite");
        writer.AddImport("org.spongepowered.asm.mixin.gen.Accessor");
        writer.AddImport("org.spongepowered.asm.mixin.gen.Invoker");

        // Detect [Mixin(typeof(T))] and extract the target class
        var mixinAttr = GetMixinAttribute(classDecl);
        var targetClass = mixinAttr != null
            ? GetTargetClass(mixinAttr)
            : "Object";

        // Detect @Mixin priority and remap options
        var priority = mixinAttr != null
            ? GetPriority(mixinAttr)
            : 1000;

        var remap = mixinAttr != null
            ? GetRemap(mixinAttr)
            : true;

        var modifiers = "public abstract";
        var className = classDecl.Identifier.Text;
        var extendsClause = TranslateExtends(classDecl, writer);
        var implementsClause = TranslateImplements(classDecl, writer);

        // Write @Mixin annotation
        writer.WriteLine($"@Mixin(value = {targetClass}.class, priority = {priority}, remap = {remap})");

        var declaration = $"{modifiers} class {className}";
        if (extendsClause != null)
            declaration += $" extends {extendsClause}";
        if (implementsClause != null)
            declaration += $" implements {implementsClause}";

        writer.OpenBrace(declaration);

        foreach (var member in classDecl.Members)
        {
            if (member is MethodDeclarationSyntax method)
                TranslateMixinMethod(method, semanticModel, writer);
            else if (member is FieldDeclarationSyntax field)
                TranslateMixinField(field, writer);
            else if (member is PropertyDeclarationSyntax property)
                TranslateProperty(property, writer);
        }

        writer.CloseBrace();
    }

    private void TranslateMixinMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, JavaWriter writer)
    {
        // Detect which mixin attribute this method has
        var injectAttr = GetAttribute<InjectAttributeData>(method, "InjectAttribute", "Inject");
        var overwriteAttr = GetAttribute<OverwriteAttributeData>(method, "OverwriteAttribute", "Overwrite");
        var accessorAttr = GetAttribute<AccessorAttributeData>(method, "AccessorAttribute", "Accessor");
        var invokerAttr = GetAttribute<InvokerAttributeData>(method, "InvokerAttribute", "Invoker");
        var modifyArgAttr = GetAttribute<ModifyArgAttributeData>(method, "ModifyArgAttribute", "ModifyArg");
        var modifyVariableAttr = GetAttribute<ModifyVariableAttributeData>(method, "ModifyVariableAttribute", "ModifyVariable");
        var redirectAttr = GetAttribute<RedirectAttributeData>(method, "RedirectAttribute", "Redirect");

        if (injectAttr != null)
            TranslateInjectMethod(method, injectAttr, writer);
        else if (overwriteAttr != null)
            TranslateOverwriteMethod(method, overwriteAttr, writer);
        else if (accessorAttr != null)
            TranslateAccessorMethod(method, accessorAttr, writer);
        else if (invokerAttr != null)
            TranslateInvokerMethod(method, invokerAttr, writer);
        else if (modifyArgAttr != null)
            TranslateModifyArgMethod(method, modifyArgAttr, writer);
        else if (modifyVariableAttr != null)
            TranslateModifyVariableMethod(method, modifyVariableAttr, writer);
        else if (redirectAttr != null)
            TranslateRedirectMethod(method, redirectAttr, writer);
        else
            TranslateMethodFallback(method, writer);
    }

    private void TranslateInjectMethod(MethodDeclarationSyntax method, InjectAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.injection.Inject");
        writer.AddImport("org.spongepowered.asm.mixin.injection.At");

        var sb = new System.Text.StringBuilder("@Inject(method = \"");
        sb.Append(attr.Method);
        sb.Append("\", at = @At(\"");
        sb.Append(MapAtType(attr.At));
        sb.Append('"');

        if (attr.Args is { Length: > 0 })
        {
            sb.Append(", args = {");
            sb.Append(string.Join(", ", attr.Args.Select(a => $"\"{a}\"")));
            sb.Append('}');
        }

        if (attr.Ordinal >= 0)
        {
            sb.Append(", ordinal = ");
            sb.Append(attr.Ordinal);
        }

        if (attr.Cancellable)
        {
            sb.Append(", cancellable = true");
        }

        sb.Append("), remap = false)");

        WriteMethodWithAnnotation(method, writer, sb.ToString());
    }

    private void TranslateOverwriteMethod(MethodDeclarationSyntax method, OverwriteAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.Overwrite");

        if (attr.Method != null)
        {
            writer.WriteLine($"@Overwrite(method = \"{attr.Method}\", remap = false)");
        }
        else
        {
            writer.WriteLine("@Overwrite(remap = false)");
        }

        WriteMethodWithAnnotation(method, writer, null);
    }

    private void TranslateAccessorMethod(MethodDeclarationSyntax method, AccessorAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.gen.Accessor");

        writer.WriteLine($"@Accessor(\"{attr.Field}\")");
        writer.WriteLine("@org.spongepowered.asm.mixin.gen.Accessor(\"test\")");

        // Accessor methods are abstract — just write the signature
        var modifiers = "public abstract";
        var returnType = MapReturnType(method.ReturnType, writer);
        var paramList = string.Join(", ", method.ParameterList.Parameters
            .Select(p => TranslateParameter(p, writer)));

        writer.WriteLine($"{modifiers} {returnType} {method.Identifier.Text}({paramList});");
    }

    private void TranslateInvokerMethod(MethodDeclarationSyntax method, InvokerAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.gen.Invoker");

        writer.WriteLine($"@Invoker(\"{attr.Method}\")");

        // Invoker methods are abstract — just write the signature
        var modifiers = "public abstract";
        var returnType = MapReturnType(method.ReturnType, writer);
        var paramList = string.Join(", ", method.ParameterList.Parameters
            .Select(p => TranslateParameter(p, writer)));

        writer.WriteLine($"{modifiers} {returnType} {method.Identifier.Text}({paramList});");
    }

    private void TranslateModifyArgMethod(MethodDeclarationSyntax method, ModifyArgAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.injection.ModifyArg");
        writer.AddImport("org.spongepowered.asm.mixin.injection.At");

        writer.WriteLine($"@ModifyArg(method = \"{attr.Method}\", at = @At(\"{MapAtType(attr.At)}\"), index = {attr.Index}, remap = false)");

        WriteMethodWithAnnotation(method, writer, null);
    }

    private void TranslateModifyVariableMethod(MethodDeclarationSyntax method, ModifyVariableAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.injection.ModifyVariable");
        writer.AddImport("org.spongepowered.asm.mixin.injection.At");

        var sb = new System.Text.StringBuilder("@ModifyVariable(method = \"");
        sb.Append(attr.Method);
        sb.Append("\", at = @At(\"");
        sb.Append(MapAtType(attr.At));
        sb.Append('"');

        if (attr.Ordinal >= 0)
        {
            sb.Append(", ordinal = ");
            sb.Append(attr.Ordinal);
        }

        sb.Append(", remap = false");

        // If there's args, add them
        if (attr.Args is { Length: > 0 })
        {
            sb.Append(", args = {");
            sb.Append(string.Join(", ", attr.Args.Select(a => $"\"{a}\"")));
            sb.Append('}');
        }

        sb.Append(')');

        WriteMethodWithAnnotation(method, writer, sb.ToString());
    }

    private void TranslateRedirectMethod(MethodDeclarationSyntax method, RedirectAttributeData attr, JavaWriter writer)
    {
        writer.AddImport("org.spongepowered.asm.mixin.injection.Redirect");
        writer.AddImport("org.spongepowered.asm.mixin.injection.At");

        var sb = new System.Text.StringBuilder("@Redirect(method = \"");
        sb.Append(attr.Method);
        sb.Append("\", at = @At(\"");
        sb.Append(MapAtType(attr.At));
        sb.Append('"');

        if (attr.Args is { Length: > 0 })
        {
            sb.Append(", args = {");
            sb.Append(string.Join(", ", attr.Args.Select(a => $"\"{a}\"")));
            sb.Append('}');
        }

        sb.Append(", remap = false)");

        WriteMethodWithAnnotation(method, writer, sb.ToString());
    }

    private void TranslateMethodFallback(MethodDeclarationSyntax method, JavaWriter writer)
    {
        WriteMethodWithAnnotation(method, writer, null);
    }

    private void WriteMethodWithAnnotation(MethodDeclarationSyntax method, JavaWriter writer, string? annotation)
    {
        if (annotation != null)
            writer.WriteLine(annotation);

        var modifiers = "private";

        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            modifiers = "private static";
        else if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            modifiers = "private";
        else if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
            modifiers = "protected";
        else if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            modifiers = "private"; // Mixin handlers are often private despite C# public

        var returnType = MapReturnType(method.ReturnType, writer);
        var paramList = string.Join(", ", method.ParameterList.Parameters
            .Select(p => TranslateParameter(p, writer)));

        writer.OpenBrace($"{modifiers} {returnType} {method.Identifier.Text}({paramList})");

        if (method.Body != null)
        {
            foreach (var statement in method.Body.Statements)
                _statementTranslator.Translate(statement, writer);
        }

        writer.CloseBrace();
    }

    private void TranslateMixinField(FieldDeclarationSyntax field, JavaWriter writer)
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

    private string MapReturnType(TypeSyntax returnType, JavaWriter writer)
    {
        var type = returnType.ToString();
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = SimplifyJavaType(mapped);
        var import = _typeMapper.GetImportForType(type);
        if (import != null) writer.AddImport(import);
        return mapped;
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

    private static string SimplifyJavaType(string javaType)
    {
        var lastDot = javaType.LastIndexOf('.');
        return lastDot >= 0 ? javaType[(lastDot + 1)..] : javaType;
    }

    private static string MapAtType(string atType) => atType switch
    {
        "Head" => "HEAD",
        "Tail" => "TAIL",
        "Return" => "RETURN",
        "Invoke" => "INVOKE",
        "Field" => "FIELD",
        "New" => "NEW",
        "Constant" => "CONSTANT",
        "Load" => "LOAD",
        "Store" => "STORE",
        _ => atType.ToUpperInvariant()
    };

    #region Attribute Parsing from C# Syntax

    private AttributeSyntax? GetMixinAttribute(ClassDeclarationSyntax classDecl)
    {
        return classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString() is "Mixin" or "MixinAttribute");
    }

    private string GetTargetClass(AttributeSyntax attr)
    {
        // Should be typeof(TargetClass) in the attribute argument list
        var typeOfArg = attr.ArgumentList?.Arguments
            .Select(a => a.Expression)
            .OfType<TypeOfExpressionSyntax>()
            .FirstOrDefault();

        if (typeOfArg != null)
        {
            var typeName = typeOfArg.Type.ToString();
            var mapped = _typeMapper.MapType(typeName);
            if (mapped != null)
                return SimplifyJavaType(mapped);
            return typeName;
        }

        return "Object";
    }

    private int GetPriority(AttributeSyntax attr)
    {
        var priorityArg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a =>
                a.NameEquals?.Name.Identifier.Text is "Priority" or "priority");

        if (priorityArg?.Expression is LiteralExpressionSyntax literal &&
            literal.Kind() is SyntaxKind.NumericLiteralExpression)
        {
            if (int.TryParse(literal.Token.Text, out var priority))
                return priority;
        }

        return 1000;
    }

    private bool GetRemap(AttributeSyntax attr)
    {
        var remapArg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a =>
                a.NameEquals?.Name.Identifier.Text is "Remap" or "remap");

        if (remapArg?.Expression is LiteralExpressionSyntax literal)
        {
            if (literal.Kind() is SyntaxKind.TrueLiteralExpression)
                return true;
            if (literal.Kind() is SyntaxKind.FalseLiteralExpression)
                return false;
        }

        return true;
    }

    private T? GetAttribute<T>(MethodDeclarationSyntax method, string attrName, string shortName)
        where T : class, new()
    {
        var attr = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString() is var n && (n == attrName || n == shortName));

        if (attr == null) return default;

        var result = new T();

        // Map named arguments to properties using reflection-compatible approach
        if (attr.ArgumentList is { } argList)
        {
            foreach (var arg in argList.Arguments)
            {
                if (arg.NameEquals != null)
                {
                    var propName = arg.NameEquals.Name.Identifier.Text;
                    SetProperty(result, propName, arg.Expression);
                }
                else if (arg.Expression is TypeOfExpressionSyntax typeOf)
                {
                    SetProperty(result, "At", typeOf); // Positional arg — might be At type
                }
                else
                {
                    // First positional arg
                    SetPositionalArg(result, arg.Expression, parsedPositionals: 0);
                }
            }
        }

        return result;
    }

    private void SetProperty<T>(T target, string propName, ExpressionSyntax value) where T : class
    {
        switch (target)
        {
            case InjectAttributeData inject:
                InjectSetProperty(inject, propName, value);
                break;
            case OverwriteAttributeData overwrite:
                OverwriteSetProperty(overwrite, propName, value);
                break;
            case AccessorAttributeData accessor:
                AccessorSetProperty(accessor, propName, value);
                break;
            case InvokerAttributeData invoker:
                InvokerSetProperty(invoker, propName, value);
                break;
            case ModifyArgAttributeData modifyArg:
                ModifyArgSetProperty(modifyArg, propName, value);
                break;
            case ModifyVariableAttributeData modifyVariable:
                ModifyVariableSetProperty(modifyVariable, propName, value);
                break;
            case RedirectAttributeData redirect:
                RedirectSetProperty(redirect, propName, value);
                break;
        }
    }

    private void SetPositionalArg<T>(T target, ExpressionSyntax value, int parsedPositionals) where T : class
    {
        // Positional args: first is Method, second could be At
        switch (target)
        {
            case InjectAttributeData inject:
                if (parsedPositionals == 0 && value is LiteralExpressionSyntax lit)
                    inject.Method = lit.Token.ValueText;
                break;
            case ModifyArgAttributeData modifyArg:
                if (parsedPositionals == 0 && value is LiteralExpressionSyntax lit2)
                    modifyArg.Method = lit2.Token.ValueText;
                break;
            case RedirectAttributeData redirect:
                if (parsedPositionals == 0 && value is LiteralExpressionSyntax lit3)
                    redirect.Method = lit3.Token.ValueText;
                break;
        }
    }

    private static void InjectSetProperty(InjectAttributeData target, string propName, ExpressionSyntax value)
    {
        switch (propName)
        {
            case "Method" or "method":
                if (value is LiteralExpressionSyntax lit)
                    target.Method = lit.Token.ValueText;
                break;
            case "At" or "at":
                if (value is MemberAccessExpressionSyntax member &&
                    member.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AtType")
                    target.At = member.Name.Identifier.Text;
                break;
            case "Args" or "args":
                if (value is ImplicitArrayCreationExpressionSyntax array)
                    target.Args = array.Initializer.Expressions
                        .OfType<LiteralExpressionSyntax>()
                        .Select(e => e.Token.ValueText)
                        .ToArray();
                else if (value is ArrayCreationExpressionSyntax array2)
                    target.Args = array2.Initializer!.Expressions
                        .OfType<LiteralExpressionSyntax>()
                        .Select(e => e.Token.ValueText)
                        .ToArray();
                break;
            case "Ordinal" or "ordinal":
                if (value is LiteralExpressionSyntax ordLit && int.TryParse(ordLit.Token.Text, out var ord))
                    target.Ordinal = ord;
                break;
            case "Cancellable" or "cancellable":
                if (value is LiteralExpressionSyntax cancelLit)
                    target.Cancellable = cancelLit.Kind() is SyntaxKind.TrueLiteralExpression;
                break;
        }
    }

    private static void OverwriteSetProperty(OverwriteAttributeData target, string propName, ExpressionSyntax value)
    {
        if (propName is "Method" or "method" && value is LiteralExpressionSyntax lit)
            target.Method = lit.Token.ValueText;
    }

    private static void AccessorSetProperty(AccessorAttributeData target, string propName, ExpressionSyntax value)
    {
        if (propName is "Field" or "field" && value is LiteralExpressionSyntax lit)
            target.Field = lit.Token.ValueText;
    }

    private static void InvokerSetProperty(InvokerAttributeData target, string propName, ExpressionSyntax value)
    {
        if (propName is "Method" or "method" && value is LiteralExpressionSyntax lit)
            target.Method = lit.Token.ValueText;
    }

    private static void ModifyArgSetProperty(ModifyArgAttributeData target, string propName, ExpressionSyntax value)
    {
        switch (propName)
        {
            case "Method" or "method":
                if (value is LiteralExpressionSyntax lit)
                    target.Method = lit.Token.ValueText;
                break;
            case "Index" or "index":
                if (value is LiteralExpressionSyntax idxLit && int.TryParse(idxLit.Token.Text, out var idx))
                    target.Index = idx;
                break;
            case "At" or "at":
                if (value is MemberAccessExpressionSyntax member &&
                    member.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AtType")
                    target.At = member.Name.Identifier.Text;
                break;
            case "Args" or "args":
                if (value is ImplicitArrayCreationExpressionSyntax array)
                    target.Args = array.Initializer.Expressions
                        .OfType<LiteralExpressionSyntax>()
                        .Select(e => e.Token.ValueText)
                        .ToArray();
                break;
        }
    }

    private static void ModifyVariableSetProperty(ModifyVariableAttributeData target, string propName, ExpressionSyntax value)
    {
        switch (propName)
        {
            case "Method" or "method":
                if (value is LiteralExpressionSyntax lit)
                    target.Method = lit.Token.ValueText;
                break;
            case "At" or "at":
                if (value is MemberAccessExpressionSyntax member &&
                    member.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AtType")
                    target.At = member.Name.Identifier.Text;
                break;
            case "Ordinal" or "ordinal":
                if (value is LiteralExpressionSyntax ordLit && int.TryParse(ordLit.Token.Text, out var ord))
                    target.Ordinal = ord;
                break;
            case "Args" or "args":
                if (value is ImplicitArrayCreationExpressionSyntax array)
                    target.Args = array.Initializer.Expressions
                        .OfType<LiteralExpressionSyntax>()
                        .Select(e => e.Token.ValueText)
                        .ToArray();
                break;
        }
    }

    private static void RedirectSetProperty(RedirectAttributeData target, string propName, ExpressionSyntax value)
    {
        switch (propName)
        {
            case "Method" or "method":
                if (value is LiteralExpressionSyntax lit)
                    target.Method = lit.Token.ValueText;
                break;
            case "At" or "at":
                if (value is MemberAccessExpressionSyntax member &&
                    member.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AtType")
                    target.At = member.Name.Identifier.Text;
                break;
            case "Args" or "args":
                if (value is ImplicitArrayCreationExpressionSyntax array)
                    target.Args = array.Initializer.Expressions
                        .OfType<LiteralExpressionSyntax>()
                        .Select(e => e.Token.ValueText)
                        .ToArray();
                break;
        }
    }

    #endregion

    #region Attribute Data Transfer Objects

    private class InjectAttributeData
    {
        public string Method { get; set; } = "";
        public string At { get; set; } = "HEAD";
        public string[]? Args { get; set; }
        public int Ordinal { get; set; } = -1;
        public bool Cancellable { get; set; }
    }

    private class OverwriteAttributeData
    {
        public string? Method { get; set; }
    }

    private class AccessorAttributeData
    {
        public string Field { get; set; } = "";
    }

    private class InvokerAttributeData
    {
        public string Method { get; set; } = "";
    }

    private class ModifyArgAttributeData
    {
        public string Method { get; set; } = "";
        public int Index { get; set; }
        public string At { get; set; } = "HEAD";
        public string[]? Args { get; set; }
    }

    private class ModifyVariableAttributeData
    {
        public string Method { get; set; } = "";
        public string At { get; set; } = "HEAD";
        public int Ordinal { get; set; } = -1;
        public string[]? Args { get; set; }
    }

    private class RedirectAttributeData
    {
        public string Method { get; set; } = "";
        public string At { get; set; } = "HEAD";
        public string[]? Args { get; set; }
    }

    #endregion
}
