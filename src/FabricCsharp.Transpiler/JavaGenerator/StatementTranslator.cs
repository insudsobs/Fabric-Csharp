using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FabricCsharp.Transpiler.JavaGenerator;

/// <summary>
/// Translates C# statements to Java statements.
/// </summary>
public class StatementTranslator
{
    private readonly TypeMapper _typeMapper;
    private readonly ExpressionTranslator _expressionTranslator;

    public StatementTranslator(TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
        _expressionTranslator = new ExpressionTranslator(typeMapper);
    }

    public void Translate(StatementSyntax statement, JavaWriter writer)
    {
        switch (statement)
        {
            case ExpressionStatementSyntax exprStmt:
                TranslateExpressionStatement(exprStmt, writer);
                break;
            case LocalDeclarationStatementSyntax localDecl:
                TranslateLocalDeclaration(localDecl, writer);
                break;
            case BlockSyntax block:
                TranslateBlock(block, writer);
                break;
            case IfStatementSyntax ifStmt:
                TranslateIfStatement(ifStmt, writer);
                break;
            case ForEachStatementSyntax forEach:
                TranslateForEach(forEach, writer);
                break;
            case ForStatementSyntax forStmt:
                TranslateFor(forStmt, writer);
                break;
            case WhileStatementSyntax whileStmt:
                TranslateWhile(whileStmt, writer);
                break;
            case ReturnStatementSyntax returnStmt:
                TranslateReturn(returnStmt, writer);
                break;
            case ThrowStatementSyntax throwStmt:
                TranslateThrow(throwStmt, writer);
                break;
            case TryStatementSyntax tryStmt:
                TranslateTry(tryStmt, writer);
                break;
            case SwitchStatementSyntax switchStmt:
                TranslateSwitch(switchStmt, writer);
                break;
            default:
                writer.WriteLine($"// TODO: Unsupported statement type: {statement.Kind()}");
                break;
        }
    }

    private void TranslateExpressionStatement(ExpressionStatementSyntax stmt, JavaWriter writer)
    {
        var expr = _expressionTranslator.Translate(stmt.Expression);
        writer.WriteLine($"{expr};");
    }

    private void TranslateLocalDeclaration(LocalDeclarationStatementSyntax stmt, JavaWriter writer)
    {
        var type = stmt.Declaration.Type.ToString();
        var mapped = _typeMapper.MapType(type);

        // If it's a var declaration, we need to infer the type from the initializer
        // For simplicity, we use "var" and let Java 10+ infer it
        var javaType = stmt.Declaration.Type.IsVar
            ? "var"
            : (mapped != null ? JavaNameOnly(mapped) : type);

        if (mapped != null)
        {
            var import = _typeMapper.GetImportForType(type);
            if (import != null) writer.AddImport(import);
        }

        foreach (var variable in stmt.Declaration.Variables)
        {
            var initializer = variable.Initializer != null
                ? $" = {_expressionTranslator.Translate(variable.Initializer.Value)}"
                : "";

            writer.WriteLine($"{javaType} {variable.Identifier.Text}{initializer};");
        }
    }

    private void TranslateBlock(BlockSyntax block, JavaWriter writer)
    {
        foreach (var stmt in block.Statements)
        {
            Translate(stmt, writer);
        }
    }

    private void TranslateIfStatement(IfStatementSyntax stmt, JavaWriter writer)
    {
        var condition = _expressionTranslator.Translate(stmt.Condition);
        writer.OpenBrace($"if ({condition})");
        Translate(stmt.Statement, writer);
        writer.CloseBrace();

        if (stmt.Else != null)
        {
            writer.OpenBrace("else");
            Translate(stmt.Else.Statement, writer);
            writer.CloseBrace();
        }
    }

    private void TranslateForEach(ForEachStatementSyntax stmt, JavaWriter writer)
    {
        var varType = stmt.Type.ToString();
        var mapped = _typeMapper.MapType(varType);
        var javaType = stmt.Type.IsVar ? "var" : (mapped != null ? JavaNameOnly(mapped) : varType);
        var collection = _expressionTranslator.Translate(stmt.Expression);

        writer.OpenBrace($"for ({javaType} {stmt.Identifier.Text} : {collection})");
        Translate(stmt.Statement, writer);
        writer.CloseBrace();
    }

    private void TranslateFor(ForStatementSyntax stmt, JavaWriter writer)
    {
        var init = stmt.Declaration != null
            ? TranslateForInit(stmt.Declaration)
            : stmt.Initializers.Any()
                ? string.Join(", ", stmt.Initializers.Select(i => _expressionTranslator.Translate(i)))
                : "";

        var condition = stmt.Condition != null
            ? _expressionTranslator.Translate(stmt.Condition)
            : "";

        var increment = stmt.Incrementors.Any()
            ? string.Join(", ", stmt.Incrementors.Select(i => _expressionTranslator.Translate(i)))
            : "";

        writer.OpenBrace($"for ({init}; {condition}; {increment})");
        Translate(stmt.Statement, writer);
        writer.CloseBrace();
    }

    private string TranslateForInit(VariableDeclarationSyntax decl)
    {
        var type = decl.Type.ToString();
        var mapped = _typeMapper.MapType(type);
        var javaType = mapped != null ? JavaNameOnly(mapped) : type;

        var vars = string.Join(", ", decl.Variables.Select(v =>
        {
            var init = v.Initializer != null
                ? $" = {_expressionTranslator.Translate(v.Initializer.Value)}"
                : "";
            return $"{javaType} {v.Identifier.Text}{init}";
        }));

        return vars;
    }

    private void TranslateWhile(WhileStatementSyntax stmt, JavaWriter writer)
    {
        var condition = _expressionTranslator.Translate(stmt.Condition);
        writer.OpenBrace($"while ({condition})");
        Translate(stmt.Statement, writer);
        writer.CloseBrace();
    }

    private void TranslateReturn(ReturnStatementSyntax stmt, JavaWriter writer)
    {
        if (stmt.Expression != null)
        {
            var expr = _expressionTranslator.Translate(stmt.Expression);
            writer.WriteLine($"return {expr};");
        }
        else
        {
            writer.WriteLine("return;");
        }
    }

    private void TranslateThrow(ThrowStatementSyntax stmt, JavaWriter writer)
    {
        var expr = _expressionTranslator.Translate(stmt.Expression!);
        writer.WriteLine($"throw {expr};");
    }

    private void TranslateTry(TryStatementSyntax stmt, JavaWriter writer)
    {
        writer.OpenBrace("try");
        foreach (var s in stmt.Block.Statements)
            Translate(s, writer);
        writer.CloseBrace();

        foreach (var catchClause in stmt.Catches)
        {
            var type = catchClause.Declaration?.Type.ToString() ?? "Exception";
            writer.OpenBrace($"catch ({type} {catchClause.Declaration?.Identifier.Text ?? "e"})");
            foreach (var s in catchClause.Block.Statements)
                Translate(s, writer);
            writer.CloseBrace();
        }

        if (stmt.Finally != null)
        {
            writer.OpenBrace("finally");
            foreach (var s in stmt.Finally.Block.Statements)
                Translate(s, writer);
            writer.CloseBrace();
        }
    }

    private void TranslateSwitch(SwitchStatementSyntax stmt, JavaWriter writer)
    {
        var expr = _expressionTranslator.Translate(stmt.Expression);
        writer.OpenBrace($"switch ({expr})");

        foreach (var section in stmt.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax caseLabel)
                {
                    var caseExpr = _expressionTranslator.Translate(caseLabel.Value);
                    writer.WriteLine($"case {caseExpr}:");
                }
                else if (label is DefaultSwitchLabelSyntax)
                {
                    writer.WriteLine("default:");
                }
            }

            writer.Indent();
            foreach (var s in section.Statements)
                Translate(s, writer);

            // Add break if there isn't one
            var hasBreak = section.Statements
                .OfType<BreakStatementSyntax>()
                .Any();
            if (!hasBreak)
                writer.WriteLine("break;");

            writer.Outdent();
        }

        writer.CloseBrace();
    }

    private static string JavaNameOnly(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
    }
}
