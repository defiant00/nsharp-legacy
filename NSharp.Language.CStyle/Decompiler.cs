using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.CStyle;

public static class Decompiler
{
    public static string Decompile(AstItem ast)
    {
        var sb = new StringBuilder();
        Process(sb, 0, ast, null);
        return sb.ToString();
    }

    private static void Process(StringBuilder sb, int indent, AstItem? currentItem, AstItem? priorItem)
    {
        switch (currentItem)
        {
            case null:
                return;
            case Break:
                ProcessBreak(sb, indent);
                break;
            case Class cl:
                ProcessClass(sb, indent, cl);
                break;
            case Comment comment:
                ProcessComment(sb, indent, comment);
                break;
            case Continue:
                ProcessContinue(sb, indent);
                break;
            case ExpressionStatement expressionStatement:
                ProcessExpressionStatement(sb, indent, expressionStatement);
                break;
            case Core.Ast.File file:
                ProcessFile(sb, indent, file);
                break;
            case If ifStatement:
                ProcessIf(sb, indent, ifStatement);
                break;
            case MethodDefinition methodDef:
                ProcessMethodDefinition(sb, indent, methodDef);
                break;
            case Namespace ns:
                ProcessNamespace(sb, indent, ns);
                break;
            case Property property:
                ProcessProperty(sb, indent, property);
                break;
            case Space space:
                ProcessSpace(sb, space);
                break;
            default:
                sb.AppendLineIndented(indent, $"[{currentItem}]");
                break;
        }
    }

    private static void ProcessExpression(StringBuilder sb, Expression? expression, int parentPrecedence = -1)
    {
        switch (expression)
        {
            case null:
                sb.Append("void");
                break;
            case BinaryOperator binaryOperator:
                ProcessBinaryOperator(sb, binaryOperator, parentPrecedence);
                break;
            case Character c:
                sb.Append("'");
                sb.Append(c.Value);
                sb.Append("'");
                break;
            case CurrentObjectInstance:
                sb.Append("this");
                break;
            case Identifier identifier:
                ProcessIdentifier(sb, identifier);
                break;
            case LiteralToken literalToken:
                ProcessLiteralToken(sb, literalToken);
                break;
            case Number n:
                sb.Append(n.Value);
                break;
            case Core.Ast.String s:
                sb.Append('"');
                sb.Append(string.Join(Environment.NewLine, s.Lines));
                sb.Append('"');
                break;
            default:
                sb.Append($"[{expression}]");
                break;
        }
    }

    private static void ProcessBinaryOperator(StringBuilder sb, BinaryOperator binaryOperator, int parentPrecedence)
    {
        bool parentheses = binaryOperator.Operator.Precedence() < parentPrecedence;
        if (parentheses)
            sb.Append("(");
        ProcessExpression(sb, binaryOperator.Left, binaryOperator.Operator.Precedence());
        sb.Append(" ");
        sb.Append(binaryOperator.Operator.StringVal());
        sb.Append(" ");
        ProcessExpression(sb, binaryOperator.Right, binaryOperator.Operator.Precedence() + 1);
        if (parentheses)
            sb.Append(")");
    }

    private static void ProcessBreak(StringBuilder sb, int indent) => sb.AppendLineIndented(indent, "break");

    private static void ProcessClass(StringBuilder sb, int indent, Class cl)
    {
        sb.AppendModifiersIndented(indent, cl.Modifiers);
        sb.AppendLine($"class {cl.Name.GetLiteral()}");
        sb.AppendLineIndented(indent, "{");
        ProcessStatements(sb, indent + 1, cl.Statements);
        sb.AppendLineIndented(indent, "}");
    }

    private static void ProcessComment(StringBuilder sb, int indent, Comment comment)
    {
        sb.AppendLineIndented(indent, $"//{comment.Value}");
    }

    private static void ProcessContinue(StringBuilder sb, int indent) => sb.AppendLineIndented(indent, "continue");

    private static void ProcessExpressionStatement(StringBuilder sb, int indent, ExpressionStatement expressionStatement)
    {
        sb.Indent(indent);
        ProcessExpression(sb, expressionStatement.Expression);
        sb.AppendLine(";");
    }

    private static void ProcessFile(StringBuilder sb, int indent, Core.Ast.File file)
    {
        ProcessStatements(sb, indent, file.Statements);
    }

    private static void ProcessIdentifier(StringBuilder sb, Identifier identifier)
    {
        ProcessIdentifierPart(sb, identifier.Parts[0]);
        for (int i = 1; i < identifier.Parts.Count; i++)
        {
            sb.Append(".");
            ProcessIdentifierPart(sb, identifier.Parts[i]);
        }
    }

    private static void ProcessIdentifierPart(StringBuilder sb, IdentifierPart identifierPart)
    {
        sb.Append(identifierPart.Value);
    }

    private static void ProcessIf(StringBuilder sb, int indent, If ifStatement)
    {
        sb.AppendIndented(indent, "if (");
        ProcessExpression(sb, ifStatement.Condition);
        sb.AppendLine(")");
        sb.AppendLineIndented(indent, "{");
        ProcessStatements(sb, indent + 1, ifStatement.Statements);
        sb.AppendLineIndented(indent, "}");
        if (ifStatement.Else != null)
        {
            sb.AppendLineIndented(indent, "else");
            sb.AppendLineIndented(indent, "{");
            ProcessStatements(sb, indent + 1, ifStatement.Else);
            sb.AppendLineIndented(indent, "}");
        }
    }

    private static void ProcessLiteralToken(StringBuilder sb, LiteralToken literalToken)
    {
        sb.Append(literalToken.Token switch
        {
            Literal.False => "false",
            Literal.Null => "null",
            Literal.True => "true",
            _ => $"[{literalToken.Token}]",
        });
    }

    private static void ProcessMethodDefinition(StringBuilder sb, int indent, MethodDefinition methodDef)
    {
        sb.AppendModifiersIndented(indent, methodDef.Modifiers);
        ProcessExpression(sb, methodDef.ReturnType);
        sb.Append($" {methodDef.Name.GetLiteral()}(");
        if (methodDef.Parameters.Count > 0)
        {
            ProcessParameter(sb, methodDef.Parameters[0]);
            for (int i = 1; i < methodDef.Parameters.Count; i++)
            {
                sb.Append(", ");
                ProcessParameter(sb, methodDef.Parameters[i]);
            }
        }
        sb.AppendLine(")");
        sb.AppendLineIndented(indent, "{");
        ProcessStatements(sb, indent + 1, methodDef.Statements);
        sb.AppendLineIndented(indent, "}");
    }

    private static void ProcessNamespace(StringBuilder sb, int indent, Namespace ns)
    {
        sb.AppendIndented(indent, "namespace ");
        ProcessExpression(sb, ns.Name);
        sb.AppendLine(";");
    }

    private static void ProcessParameter(StringBuilder sb, Parameter parameter)
    {
        ProcessExpression(sb, parameter.Type);
        sb.Append(" ");
        sb.Append(parameter.Name.GetLiteral());
    }

    private static void ProcessProperty(StringBuilder sb, int indent, Property property)
    {
        sb.AppendModifiersIndented(indent, property.Modifiers);
        ProcessExpression(sb, property.Type);
        sb.Append(" ");
        sb.Append(property.Name.GetLiteral());
        sb.AppendLine(";");
    }

    private static void ProcessSpace(StringBuilder sb, Space space)
    {
        for (int i = 0; i < space.Size; i++)
            sb.AppendLine();
    }

    private static void ProcessStatements(StringBuilder sb, int indent, List<Statement> statements)
    {
        AstItem? priorStatement = null;
        foreach (var statement in statements)
        {
            Process(sb, indent, statement, priorStatement);
            priorStatement = statement;
        }
    }
}