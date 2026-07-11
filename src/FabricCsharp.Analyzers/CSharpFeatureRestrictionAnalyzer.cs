using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FabricCsharp.Analyzers;

/// <summary>
/// A Roslyn analyzer that detects C# features not supported by the FabricCsharp transpiler.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSharpFeatureRestrictionAnalyzer : DiagnosticAnalyzer
{
    // Diagnostic IDs
    public const string PropertyChainId = "FC001";
    public const string NullConditionalId = "FC002";
    public const string DynamicId = "FC003";
    public const string UnsafeId = "FC004";
    public const string SpanId = "FC005";
    public const string AsyncId = "FC006";
    public const string StructId = "FC007";
    public const string LinqId = "FC008";
    public const string MissingModInfoId = "FC010";

    private static readonly DiagnosticDescriptor PropertyChainRule = new(
        PropertyChainId,
        "Property chain detected — extract to local variable",
        "Property chain '{0}' should be extracted to a local variable to ensure correct Java translation",
        "FabricCsharp",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The transpiler requires property chains to be extracted to local variables.");

    private static readonly DiagnosticDescriptor NullConditionalRule = new(
        NullConditionalId,
        "Null conditional operator (?. or ?[]) not supported",
        "The null conditional operator is not supported in FabricCsharp mod code",
        "FabricCsharp",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DynamicRule = new(
        DynamicId,
        "dynamic type not supported",
        "The 'dynamic' keyword is not supported in FabricCsharp mod code",
        "FabricCsharp",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsafeRule = new(
        UnsafeId,
        "unsafe code not supported",
        "Unsafe code blocks are not supported in FabricCsharp mod code",
        "FabricCsharp",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AsyncRule = new(
        AsyncId,
        "async/await not recommended",
        "async/await is not well supported in Minecraft's single-threaded game loop model. Consider using synchronous APIs or the mod's scheduler instead.",
        "FabricCsharp",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor LinqRule = new(
        LinqId,
        "LINQ query expression may not translate correctly",
        "LINQ query expressions may not translate correctly to Java. Consider using method chains or foreach loops.",
        "FabricCsharp",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingModInfoRule = new(
        MissingModInfoId,
        "Missing [ModInfo] attribute",
        "The mod entry point class must have a [ModInfo] attribute with Id, Name, and Version specified",
        "FabricCsharp",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        PropertyChainRule,
        NullConditionalRule,
        DynamicRule,
        UnsafeRule,
        AsyncRule,
        LinqRule,
        MissingModInfoRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for syntax node analysis
        context.RegisterSyntaxNodeAction(AnalyzePropertyChain, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeConditionalAccess, SyntaxKind.ConditionalAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDynamicUsage, SyntaxKind.IdentifierName);
        context.RegisterSyntaxNodeAction(AnalyzeUnsafeCode, SyntaxKind.UnsafeStatement);
        context.RegisterSyntaxNodeAction(AnalyzeAsyncMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLinqQuery, SyntaxKind.QueryExpression);
        context.RegisterSyntaxNodeAction(AnalyzeMissingModInfo, SyntaxKind.ClassDeclaration);
    }

    /// <summary>
    /// Detects property chains like obj.Prop1.Prop2 which don't map well to Java getters.
    /// </summary>
    private static void AnalyzePropertyChain(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)context.Node;

        // Check if this is a property chain (a.b.c where a.b is also a member access)
        if (memberAccess.Expression is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax inner)
        {
            // If the inner access looks like a property (starts with lowercase or is a method call chain)
            var fullChain = memberAccess.ToString();

            // Don't flag simple method calls or namespace access patterns
            if (fullChain.Contains('.') && !fullChain.EndsWith("()") &&
                !IsNamespaceAccess(fullChain) && !IsStaticMethodCall(fullChain))
            {
                var diagnostic = Diagnostic.Create(PropertyChainRule, memberAccess.GetLocation(), fullChain);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeConditionalAccess(SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(NullConditionalRule, context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeDynamicUsage(SyntaxNodeAnalysisContext context)
    {
        var identifier = (Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)context.Node;
        if (identifier.Identifier.Text == "dynamic")
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type?.SpecialType == SpecialType.None)
            {
                // It's a keyword usage, not a type name
                var diagnostic = Diagnostic.Create(DynamicRule, identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeUnsafeCode(SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(UnsafeRule, context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeAsyncMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)context.Node;
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
        {
            var diagnostic = Diagnostic.Create(AsyncRule, method.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeLinqQuery(SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(LinqRule, context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeMissingModInfo(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)context.Node;

        // Check if class implements IModInitializer
        var baseList = classDecl.BaseList;
        if (baseList == null) return;

        var hasModInit = baseList.Types.Any(t =>
            t.Type.ToString().Contains("IModInitializer"));

        if (hasModInit)
        {
            // Check if it has [ModInfo]
            var hasModInfo = classDecl.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("ModInfo"));

            if (!hasModInfo)
            {
                var diagnostic = Diagnostic.Create(MissingModInfoRule, classDecl.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsNamespaceAccess(string expr) =>
        expr.Split('.')[0] is string first &&
        (first == "System" || first == "FabricCsharp" || first == "Microsoft" ||
         first == "net" || first == "com" || char.IsUpper(first[0]));

    private static bool IsStaticMethodCall(string expr) =>
        expr.EndsWith("()") || expr.Contains('(');
}
