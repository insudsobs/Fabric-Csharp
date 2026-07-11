using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Translates C# expressions to Java expressions.
/// Handles method calls, property access, operators, literals, object creation, etc.
/// </summary>
public class ExpressionTranslator
{
    private readonly TypeMapper _typeMapper;

    public ExpressionTranslator(TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public string Translate(ExpressionSyntax expr) => expr switch
    {
        LiteralExpressionSyntax literal => TranslateLiteral(literal),
        InvocationExpressionSyntax invocation => TranslateInvocation(invocation),
        MemberAccessExpressionSyntax memberAccess => TranslateMemberAccess(memberAccess),
        ObjectCreationExpressionSyntax objectCreation => TranslateObjectCreation(objectCreation),
        IdentifierNameSyntax identifier => identifier.Identifier.Text,
        BinaryExpressionSyntax binary => TranslateBinary(binary),
        ConditionalExpressionSyntax conditional => TranslateConditional(conditional),
        SimpleLambdaExpressionSyntax lambda => TranslateLambda(lambda),
        ParenthesizedLambdaExpressionSyntax parenLambda => TranslateLambda(parenLambda),
        ParenthesizedExpressionSyntax paren => $"({Translate(paren.Expression)})",
        ElementAccessExpressionSyntax elementAccess => TranslateElementAccess(elementAccess),
        ArrayCreationExpressionSyntax arrayCreation => TranslateArrayCreation(arrayCreation),
        CastExpressionSyntax cast => TranslateCast(cast),
        ConditionalAccessExpressionSyntax condAccess => TranslateConditionalAccess(condAccess),
        ThisExpressionSyntax => "this",
        BaseExpressionSyntax => "super",
        DefaultExpressionSyntax => "null",
        TypeOfExpressionSyntax typeOf => TranslateTypeOf(typeOf),
        PostfixUnaryExpressionSyntax postfix => TranslatePostfix(postfix),
        PrefixUnaryExpressionSyntax prefix => TranslatePrefix(prefix),
        AssignmentExpressionSyntax assignment => TranslateAssignment(assignment),
        InterpolatedStringExpressionSyntax interpolated => TranslateInterpolatedString(interpolated),
        CheckedExpressionSyntax checkedExpr => Translate((ExpressionSyntax)checkedExpr.Expression),
        AwaitExpressionSyntax awaitExpr => Translate(awaitExpr.Expression), // strip await
        _ => expr.ToString()
    };

    private string TranslateLiteral(LiteralExpressionSyntax literal) => literal.Kind() switch
    {
        SyntaxKind.StringLiteralExpression => $"\"{EscapeJavaString(literal.Token.ValueText)}\"",
        SyntaxKind.CharacterLiteralExpression => $"'{EscapeJavaChar(literal.Token.ValueText[0])}'",
        SyntaxKind.NumericLiteralExpression => literal.Token.Text,
        SyntaxKind.TrueLiteralExpression => "true",
        SyntaxKind.FalseLiteralExpression => "false",
        SyntaxKind.NullLiteralExpression => "null",
        SyntaxKind.DefaultLiteralExpression => "null",
        _ => literal.Token.Text
    };

    private string TranslateInvocation(InvocationExpressionSyntax invocation)
    {
        var methodExpr = invocation.Expression;

        // Handle nameof() — convert to string literal
        if (methodExpr is IdentifierNameSyntax nameExpr &&
            nameExpr.Identifier.Text == "nameof" &&
            invocation.ArgumentList.Arguments.Count == 1)
        {
            var nameofArg = invocation.ArgumentList.Arguments[0].Expression;
            // Extract just the simple name from a potentially dotted expression
            var name = nameofArg is MemberAccessExpressionSyntax memberAcc
                ? memberAcc.Name.Identifier.Text
                : nameofArg is IdentifierNameSyntax id
                    ? id.Identifier.Text
                    : nameofArg.ToString();
            return $"\"{name}\"";
        }

        // Handle special transpiler intrinsics
        if (methodExpr is MemberAccessExpressionSyntax ma)
        {
            var target = ma.Expression.ToString();
            var method = ma.Name.Identifier.Text;

            // Registries.Register<T>(key, factory) → Java registration code
            if (target == "Registries" && method == "Register")
            {
                return TranslateRegisterCall(invocation);
            }
        }

            // Standard method invocation translation
        var methodName = TranslateMethodName(methodExpr);
        var args = string.Join(", ", invocation.ArgumentList.Arguments
            .Select(a => Translate(a.Expression)));

        return $"{methodName}({args})";
    }

    /// <summary>
    /// Translates Registries.Register&lt;T&gt;(key, factory) → Items.register(key, Type::new, settings) for Items
    /// or Blocks.register(key, Type::new, settings) for Blocks.
    /// </summary>
    private string TranslateRegisterCall(InvocationExpressionSyntax invocation)
    {
        var typeArg = ExtractRegisterTypeArg(invocation);
        var regClass = typeArg == "Block" ? "Blocks" : "Items";
        var args = invocation.ArgumentList.Arguments;

        var keyExpr = Translate(args[0].Expression);

        if (args.Count < 2)
            return $"{regClass}.register({keyExpr})";

        var factoryExpr = args[1].Expression;

        if (factoryExpr is SimpleLambdaExpressionSyntax sl &&
            sl.Body is ObjectCreationExpressionSyntax creation)
        {
            var className = creation.Type.ToString();
            if (creation.ArgumentList is { Arguments.Count: > 0 })
            {
                var settingsArg = creation.ArgumentList.Arguments[0];
                var settings = settingsArg.Expression is ObjectCreationExpressionSyntax settingsCreation
                    ? TranslateSettingsObjectInit(settingsCreation)
                    : Translate(settingsArg.Expression);
                return $"{regClass}.register({keyExpr}, {className}::new, {settings})";
            }
            return $"{regClass}.register({keyExpr}, {className}::new)";
        }

        var factory = Translate(factoryExpr);
        return $"{regClass}.register({keyExpr}, {factory})";
    }

    /// <summary>
    /// Translates new Item.Settings { MaxCount = 64, Rarity = Rarity.Uncommon }
    /// → new Item.Settings().maxCount(64).rarity(Rarity.UNCOMMON)
    /// </summary>
    private static string TranslateSettingsObjectInit(ObjectCreationExpressionSyntax creation)
    {
        var typeName = creation.Type.ToString();
        var result = $"new {typeName}()";
        if (creation.Initializer == null) return result;

        foreach (var expr in creation.Initializer.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax assignment) continue;
            var prop = assignment.Left.ToString();
            var value = assignment.Right.ToString();
            var setter = char.ToLower(prop[0]) + prop[1..];
            value = value switch
            {
                "Rarity.Common" => "Rarity.COMMON",
                "Rarity.Uncommon" => "Rarity.UNCOMMON",
                "Rarity.Rare" => "Rarity.RARE",
                "Rarity.Epic" => "Rarity.EPIC",
                _ => value
            };
            result += $".{setter}({value})";
        }
        return result;
    }

    /// <summary>
    /// Extracts the generic type argument from Register&lt;Item&gt; or Register&lt;Block&gt;.
    /// </summary>
    private static string ExtractRegisterTypeArg(InvocationExpressionSyntax invocation)
    {
        // Method 1: Check explicit generic type argument on Register<T>
        if (invocation.Expression is MemberAccessExpressionSyntax ma &&
            ma.Name is GenericNameSyntax generic)
        {
            var typeName = generic.TypeArgumentList.Arguments.FirstOrDefault()?.ToString();
            if (typeName is "Block" or "Item") return typeName;
        }

        // Method 2: Climb to VariableDeclarator to check declared type
        var parent = invocation.Parent;
        while (parent != null)
        {
            if (parent is EqualsValueClauseSyntax equals &&
                equals.Parent is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax varDecl })
            {
                var declType = varDecl.Type.ToString();
                if (declType is "Block" or "var") return "Block";
                if (declType is "Item") return "Item";
                break;
            }
            if (parent is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax varDecl2 })
            {
                var declType = varDecl2.Type.ToString();
                if (declType is "Block") return "Block";
                if (declType is "Item") return "Item";
                break;
            }
            parent = parent.Parent;
        }

        // Method 3: Look for Block/Item hint in argument text
        var argText = invocation.ArgumentList.ToString();
        if (argText.Contains("<Block>") || argText.Contains("BlockKey") || argText.Contains("OreKey"))
            return "Block";

        return "Item";
    }

    private string TranslateMethodName(ExpressionSyntax expr)
    {
        if (expr is MemberAccessExpressionSyntax ma)
        {
            var obj = Translate(ma.Expression);
            var method = ma.Name.Identifier.Text;

            // Handle special methods
            method = method switch
            {
                "Equals" => "equals",
                "GetHashCode" => "hashCode",
                "ToString" => "toString",
                "GetType" => "getClass",
                _ => PascalToCamel(method)
            };

            return $"{obj}.{method}";
        }

        if (expr is IdentifierNameSyntax id)
        {
            return id.Identifier.Text;
        }

        return expr.ToString();
    }

    private string TranslateMemberAccess(MemberAccessExpressionSyntax memberAccess)
    {
        var obj = Translate(memberAccess.Expression);
        var member = memberAccess.Name.Identifier.Text;

        // Property access → getter call in Java
        // Check if this looks like a property access (not a static method or class access)
        if (memberAccess.Expression is not IdentifierNameSyntax ||
            !char.IsUpper(memberAccess.Expression.ToString()[0]))
        {
            // This might be a property; convert to getter
            var getter = $"get{PascalCase(member)}()";
            return $"{obj}.{getter}";
        }

        return $"{obj}.{member}";
    }

    private string TranslateObjectCreation(ObjectCreationExpressionSyntax creation)
    {
        var type = creation.Type.ToString();
        var mapped = _typeMapper.MapType(type);
        var javaType = mapped != null ? JavaNameOnly(mapped) : type;

        var args = string.Join(", ", creation.ArgumentList?.Arguments
            .Select(a => Translate(a.Expression)) ?? []);

        return $"new {javaType}({args})";
    }

    private string TranslateBinary(BinaryExpressionSyntax binary)
    {
        var left = Translate(binary.Left);
        var right = Translate(binary.Right);

        var op = binary.OperatorToken.Text switch
        {
            "==" => "==",
            "!=" => "!=",
            "<" => "<",
            ">" => ">",
            "<=" => "<=",
            ">=" => ">=",
            "&&" => "&&",
            "||" => "||",
            "+" => "+",
            "-" => "-",
            "*" => "*",
            "/" => "/",
            "%" => "%",
            "??" => null, // null coalescing — cannot translate naively
            _ => binary.OperatorToken.Text
        };

        if (op == null)
        {
            // ?? operator: left != null ? left : right
            return $"{left} != null ? {left} : {right}";
        }

        // String concatenation safety: in Java, + on strings works the same
        return $"{left} {op} {right}";
    }

    private string TranslateConditional(ConditionalExpressionSyntax conditional)
    {
        var condition = Translate(conditional.Condition);
        var whenTrue = Translate(conditional.WhenTrue);
        var whenFalse = Translate(conditional.WhenFalse);
        return $"{condition} ? {whenTrue} : {whenFalse}";
    }

    private string TranslateLambda(SimpleLambdaExpressionSyntax lambda)
    {
        var param = lambda.Parameter.Identifier.Text;
        var bodyExpr = lambda.Body as ExpressionSyntax;
        var body = bodyExpr != null ? Translate(bodyExpr) : lambda.Body.ToString();
        return $"({param}) -> {body}";
    }

    private string TranslateLambda(ParenthesizedLambdaExpressionSyntax lambda)
    {
        var parameters = string.Join(", ", lambda.ParameterList.Parameters
            .Select(p => p.Identifier.Text));
        var body = lambda.Body is BlockSyntax block
            ? $"{{ /* block lambda */ }}"
            : Translate((lambda.Body as ExpressionSyntax)!);
        return $"({parameters}) -> {body}";
    }

    private string TranslateElementAccess(ElementAccessExpressionSyntax access)
    {
        var obj = Translate(access.Expression);
        var indices = string.Join("][", access.ArgumentList.Arguments
            .Select(a => Translate(a.Expression)));
        // Array access: obj[index] → obj[index]
        // List.Get() → list.get(index)
        return $"{obj}[{indices}]"; // Simplification: assume it's an array
    }

    private string TranslateArrayCreation(ArrayCreationExpressionSyntax creation)
    {
        var type = creation.Type.ElementType.ToString();
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = JavaNameOnly(mapped);

        if (creation.Initializer != null)
        {
            var elements = string.Join(", ", creation.Initializer.Expressions
                .Select(e => Translate(e)));
            return $"new {mapped}[] {{ {elements} }}";
        }

        var sizes = string.Join("][", creation.Type.RankSpecifiers
            .SelectMany(r => r.Sizes)
            .Select(s => Translate(s)));
        return $"new {mapped}[{sizes}]";
    }

    private string TranslateCast(CastExpressionSyntax cast)
    {
        var type = cast.Type.ToString();
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = JavaNameOnly(mapped);
        var expr = Translate(cast.Expression);
        return $"({mapped})({expr})";
    }

    private string TranslateConditionalAccess(ConditionalAccessExpressionSyntax condAccess)
    {
        // C# ?. operator → Java null check pattern
        var target = Translate(condAccess.Expression);

        if (condAccess.WhenNotNull is MemberBindingExpressionSyntax member)
        {
            return $"{target} != null ? {target}.{member.Name.Identifier.Text} : null";
        }

        if (condAccess.WhenNotNull is InvocationExpressionSyntax invocation)
        {
            var methodName = TranslateMethodName(invocation.Expression);
            var args = string.Join(", ", invocation.ArgumentList.Arguments
                .Select(a => Translate(a.Expression)));
            return $"{target} != null ? {methodName}({args}) : null";
        }

        return $"{target} != null ? {Translate(condAccess.WhenNotNull)} : null";
    }

    private string TranslateTypeOf(TypeOfExpressionSyntax typeOf)
    {
        var type = typeOf.Type.ToString();
        var mapped = _typeMapper.MapType(type) ?? type;
        mapped = JavaNameOnly(mapped);
        return $"{mapped}.class";
    }

    private string TranslatePostfix(PostfixUnaryExpressionSyntax postfix)
    {
        var operand = Translate(postfix.Operand);
        return postfix.OperatorToken.Text switch
        {
            "++" => $"{operand}++",
            "--" => $"{operand}--",
            _ => $"{operand}{postfix.OperatorToken.Text}"
        };
    }

    private string TranslatePrefix(PrefixUnaryExpressionSyntax prefix)
    {
        var operand = Translate(prefix.Operand);
        return prefix.OperatorToken.Text switch
        {
            "++" => $"++{operand}",
            "--" => $"--{operand}",
            _ => $"{prefix.OperatorToken.Text}{operand}"
        };
    }

    private string TranslateAssignment(AssignmentExpressionSyntax assignment)
    {
        var left = Translate(assignment.Left);
        var right = Translate(assignment.Right);
        return $"{left} = {right}";
    }

    private string TranslateInterpolatedString(InterpolatedStringExpressionSyntax interpolated)
    {
        var parts = new List<string>();
        foreach (var content in interpolated.Contents)
        {
            if (content is InterpolationSyntax interpolation)
            {
                parts.Add($"\" + {Translate(interpolation.Expression)} + \"");
            }
            else if (content is InterpolatedStringTextSyntax text)
            {
                parts.Add(text.TextToken.Text);
            }
        }

        return string.Concat(parts);
    }

    #region Helpers

    private static string PascalToCamel(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLower(name[0]) + name[1..];
    }

    private static string PascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpper(name[0]) + name[1..];
    }

    private static string JavaNameOnly(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
    }

    private static string EscapeJavaString(string value) =>
        value.Replace("\\", "\\\\")
             .Replace("\"", "\\\"")
             .Replace("\n", "\\n")
             .Replace("\r", "\\r")
             .Replace("\t", "\\t");

    private static string EscapeJavaChar(char c) => c switch
    {
        '\\' => "\\\\",
        '\'' => "\\'",
        '\n' => "\\n",
        '\r' => "\\r",
        '\t' => "\\t",
        _ => c.ToString()
    };

    #endregion
}
