using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.Min;

public class Decompiler
{
    private Settings Settings { get; set; }
    private StringBuilder Buffer { get; set; }

    public Decompiler(Settings settings)
    {
        Settings = settings;
        Buffer = new StringBuilder();
    }

    public string Decompile(AstItem ast)
    {
        Buffer.Clear();
        Process(0, ast, null);
        return Buffer.ToString();
    }

    private void Indent(int indent) => Buffer.Indent(Settings.Indentation, indent);
    private void AppendIndented(int indent, string content) => Buffer.AppendIndented(Settings.Indentation, indent, content);
    private void AppendLineIndented(int indent, string content) => Buffer.AppendLineIndented(Settings.Indentation, indent, content);
    private void AppendModifiersIndented(int indent, List<Modifier> modifiers) => Buffer.AppendModifiersIndented(Settings.Indentation, indent, modifiers);

    private void Process(int indent, AstItem? currentItem, AstItem? priorItem)
    {
        switch (currentItem)
        {
            case null:
                return;
            case Break:
                ProcessBreak(indent);
                break;
            case Class cl:
                ProcessClass(indent, cl);
                break;
            case Comment comment:
                ProcessComment(indent, comment);
                break;
            case Continue:
                ProcessContinue(indent);
                break;
            case ExpressionStatement expressionStatement:
                ProcessExpressionStatement(indent, expressionStatement);
                break;
            case Core.Ast.File file:
                ProcessFile(indent, file);
                break;
            case FunctionDefinition functionDef:
                ProcessFunctionDefinition(indent, functionDef);
                break;
            case If ifStatement:
                ProcessIf(indent, ifStatement);
                break;
            case Import import:
                ProcessImport(indent, import);
                break;
            case Namespace ns:
                ProcessNamespace(indent, ns);
                break;
            case Property property:
                ProcessProperty(indent, property);
                break;
            case Space space:
                ProcessSpace(space);
                break;
            default:
                AppendLineIndented(indent, $"[{currentItem}]");
                break;
        }
    }

    private void ProcessExpression(int indent, Expression? expression, int parentPrecedence = -1)
    {
        switch (expression)
        {
            case null:
                Buffer.Append("void");
                break;
            case Assignment assignment:
                ProcessAssignment(indent, assignment);
                break;
            case BinaryOperator binaryOperator:
                ProcessBinaryOperator(indent, binaryOperator, parentPrecedence);
                break;
            case Character c:
                Buffer.Append("'");
                Buffer.Append(c.Value);
                Buffer.Append("'");
                break;
            case CurrentObjectInstance:
                Buffer.Append("this");
                break;
            case FunctionCall functionCall:
                ProcessFunctionCall(indent, functionCall);
                break;
            case Identifier identifier:
                ProcessIdentifier(identifier);
                break;
            case LiteralToken literalToken:
                ProcessLiteralToken(literalToken);
                break;
            case Number n:
                Buffer.Append(n.Value);
                break;
            case Core.Ast.String str:
                ProcessString(indent, str);
                break;
            case StringLiteral stringLiteral:
                Buffer.Append(stringLiteral.Value);
                break;
            default:
                Buffer.Append($"[{expression}]");
                break;
        }
    }

    private void ProcessAssignment(int indent, Assignment assignment)
    {
        ProcessExpression(indent, assignment.Left);
        Buffer.Append(" ");
        Buffer.Append(assignment.Operator.StringVal());
        Buffer.Append(" ");
        ProcessExpression(indent, assignment.Right);
    }

    private void ProcessBinaryOperator(int indent, BinaryOperator binaryOperator, int parentPrecedence)
    {
        bool parentheses = Settings.AllParens || binaryOperator.Operator.Precedence() < parentPrecedence;
        bool space = binaryOperator.Operator != OperatorType.Dot;
        if (parentheses)
            Buffer.Append("(");
        ProcessExpression(indent, binaryOperator.Left, binaryOperator.Operator.Precedence());
        if (space)
            Buffer.Append(" ");
        Buffer.Append(binaryOperator.Operator.StringVal());
        if (space)
            Buffer.Append(" ");
        ProcessExpression(indent, binaryOperator.Right, binaryOperator.Operator.Precedence() + 1);
        if (parentheses)
            Buffer.Append(")");
    }

    private void ProcessBreak(int indent) => AppendLineIndented(indent, "break");

    private void ProcessClass(int indent, Class cl)
    {
        AppendModifiersIndented(indent, cl.Modifiers);
        Buffer.Append($"class ");
        Buffer.AppendLine(cl.Name.GetLiteral());
        ProcessStatements(indent + 1, cl.Statements);
    }

    private void ProcessComment(int indent, Comment comment)
    {
        AppendIndented(indent, ";");
        if (comment.IsDocumentation)
            Buffer.Append(";");
        Buffer.AppendLine(comment.Value);
    }

    private void ProcessContinue(int indent) => AppendLineIndented(indent, "continue");

    private void ProcessExpressionStatement(int indent, ExpressionStatement expressionStatement)
    {
        Indent(indent);
        ProcessExpression(indent, expressionStatement.Expression);
        Buffer.AppendLine();
    }

    private void ProcessFile(int indent, Core.Ast.File file) => ProcessStatements(indent, file.Statements);

    private void ProcessFunctionCall(int indent, FunctionCall functionCall)
    {
        ProcessExpression(indent, functionCall.Target);
        Buffer.Append("(");
        if (functionCall.Parameters.Count > 0)
        {
            ProcessExpression(indent, functionCall.Parameters[0]);
            for (int i = 1; i < functionCall.Parameters.Count; i++)
            {
                Buffer.Append(", ");
                ProcessExpression(indent, functionCall.Parameters[i]);
            }
        }
        Buffer.Append(")");
    }

    private void ProcessFunctionDefinition(int indent, FunctionDefinition functionDef)
    {
        AppendModifiersIndented(indent, functionDef.Modifiers);
        ProcessExpression(indent, functionDef.ReturnType);
        Buffer.Append(" ");
        Buffer.Append(functionDef.Name.GetLiteral());
        Buffer.Append("(");
        bool paramMultiline = Settings.ParamMultiline && functionDef.Parameters.Count > 1;
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent + 1);
        }
        if (functionDef.Parameters.Count > 0)
        {
            ProcessParameter(indent, functionDef.Parameters[0]);
            for (int i = 1; i < functionDef.Parameters.Count; i++)
            {
                Buffer.Append(",");
                if (paramMultiline)
                {
                    Buffer.AppendLine();
                    Indent(indent + 1);
                }
                else
                    Buffer.Append(" ");
                ProcessParameter(indent, functionDef.Parameters[i]);
            }
        }
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent);
        }
        Buffer.AppendLine(")");
        ProcessStatements(indent + 1, functionDef.Statements);
    }

    private void ProcessIdentifier(Identifier identifier)
    {
        ProcessIdentifierPart(identifier.Parts[0]);
        for (int i = 1; i < identifier.Parts.Count; i++)
        {
            Buffer.Append(".");
            ProcessIdentifierPart(identifier.Parts[i]);
        }
    }

    private void ProcessIdentifierPart(IdentifierPart identifierPart)
    {
        Buffer.Append(identifierPart.Value);
        if (identifierPart.Types != null && identifierPart.Types.Count > 0)
        {
            Buffer.Append(":");
            bool parens = Settings.AllParenGenerics || identifierPart.Types.Count > 1;
            if (parens)
                Buffer.Append("(");
            ProcessIdentifier(identifierPart.Types[0]);
            for (int i = 1; i < identifierPart.Types.Count; i++)
            {
                Buffer.Append(", ");
                ProcessIdentifier(identifierPart.Types[i]);
            }
            if (parens)
                Buffer.Append(")");
        }
    }

    private void ProcessIf(int indent, If ifStatement, bool indentFirstLine = true)
    {
        if (indentFirstLine)
            Indent(indent);
        Buffer.Append("if ");
        ProcessExpression(indent, ifStatement.Condition);
        Buffer.AppendLine();
        ProcessStatements(indent + 1, ifStatement.Statements);
        if (ifStatement.Else != null)
        {
            AppendIndented(indent, "else");
            if (ifStatement.Else.Count == 1 && ifStatement.Else[0] is If ifSt)
            {
                Buffer.Append(" ");
                ProcessIf(indent, ifSt, false);
            }
            else
            {
                Buffer.AppendLine();
                ProcessStatements(indent + 1, ifStatement.Else);
            }
        }
    }

    private void ProcessImport(int indent, Import import)
    {
        AppendIndented(indent, "use ");
        ProcessExpression(indent, import.Value);
        Buffer.AppendLine();
    }

    private void ProcessLiteralToken(LiteralToken literalToken)
    {
        Buffer.Append(literalToken.Token switch
        {
            Literal.False => "false",
            Literal.Null => "null",
            Literal.True => "true",
            _ => $"[{literalToken.Token}]",
        });
    }

    private void ProcessNamespace(int indent, Namespace ns)
    {
        AppendIndented(indent, "ns ");
        ProcessExpression(indent, ns.Name);
        Buffer.AppendLine();
    }

    private void ProcessParameter(int indent, Parameter parameter)
    {
        ProcessExpression(indent, parameter.Type);
        Buffer.Append(" ");
        Buffer.Append(parameter.Name.GetLiteral());
    }

    private void ProcessProperty(int indent, Property property)
    {
        AppendModifiersIndented(indent, property.Modifiers);
        ProcessExpression(indent, property.Type);
        Buffer.Append(" ");
        Buffer.AppendLine(property.Name.GetLiteral());
    }

    private void ProcessSpace(Space space)
    {
        for (int i = 0; i < space.Size; i++)
            Buffer.AppendLine();
    }

    private void ProcessStatements(int indent, List<Statement> statements)
    {
        AstItem? priorStatement = null;
        foreach (var statement in statements)
        {
            Process(indent, statement, priorStatement);
            priorStatement = statement;
        }
    }

    private void ProcessString(int indent, Core.Ast.String str)
    {
        ProcessStringLine(indent, str.Lines[0]);
        for (int i = 1; i < str.Lines.Count; i++)
        {
            Buffer.AppendLine(" ..");
            Indent(Settings.NoIndentMultiline ? indent : indent + 1);
            ProcessStringLine(indent, str.Lines[i]);
        }
    }

    private void ProcessStringLine(int indent, List<Expression> line)
    {
        Buffer.Append("\"");
        foreach (var expr in line)
        {
            bool curlies = !(expr is StringLiteral);
            if (curlies)
                Buffer.Append("{");
            ProcessExpression(indent, expr);
            if (curlies)
                Buffer.Append("}");
        }
        Buffer.Append("\"");
    }
}