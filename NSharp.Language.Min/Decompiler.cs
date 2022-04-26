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

    public string Decompile(ISyntaxTreeItem item)
    {
        Buffer.Clear();
        Process(0, item, null);
        return Buffer.ToString();
    }

    private void Indent(int indent) => Buffer.Indent(Settings.Indentation, indent);
    private void AppendIndented(int indent, string content) => Buffer.AppendIndented(Settings.Indentation, indent, content);
    private void AppendLineIndented(int indent, string content) => Buffer.AppendLineIndented(Settings.Indentation, indent, content);
    private void AppendModifiersIndented(int indent, List<Modifier> modifiers) => Buffer.AppendModifiersIndented(Settings.Indentation, indent, modifiers);

    private void Process(int indent, ISyntaxTreeItem? currentItem, ISyntaxTreeItem? priorItem)
    {
        switch (currentItem)
        {
            case null:
                return;
            case Assignment assignment:
                ProcessAssignment(indent, assignment);
                break;
            case Break:
                ProcessBreak(indent);
                break;
            case Class cl:
                ProcessClass(indent, cl);
                break;
            case Comment comment:
                ProcessComment(indent, comment);
                break;
            case Constant constant:
                ProcessConstant(indent, constant);
                break;
            case ConstructorDefinition ctorDef:
                ProcessConstructorDefinition(indent, ctorDef);
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
            case If ifStatement:
                ProcessIf(indent, ifStatement);
                break;
            case Import import:
                ProcessImport(indent, import);
                break;
            case MethodDefinition methodDef:
                ProcessMethodDefinition(indent, methodDef);
                break;
            case Namespace ns:
                ProcessNamespace(indent, ns);
                break;
            case Property property:
                ProcessProperty(indent, property);
                break;
            case Return ret:
                ProcessReturn(indent, ret);
                break;
            case Space space:
                ProcessSpace(space);
                break;
            case Field variable:
                ProcessField(indent, variable);
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
            case Indexer accessor:
                ProcessAccessor(indent, accessor);
                break;
            case Core.Ast.Array array:
                ProcessArray(indent, array);
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
            case Generic generic:
                ProcessGeneric(indent, generic);
                break;
            case Identifier identifier:
                ProcessIdentifier(identifier);
                break;
            case LiteralToken literalToken:
                ProcessLiteralToken(literalToken);
                break;
            case MethodCall methodCall:
                ProcessMethodCall(indent, methodCall);
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

    private void ProcessAccessor(int indent, Indexer accessor)
    {
        ProcessExpression(indent, accessor.Expr);
        Buffer.Append("[");
        ProcessExpression(indent, accessor.Arguments[0]);
        for (int i = 1; i < accessor.Arguments.Count; i++)
        {
            Buffer.Append(", ");
            ProcessExpression(indent, accessor.Arguments[i]);
        }
        Buffer.Append("]");
    }

    private void ProcessArray(int indent, Core.Ast.Array array)
    {
        Buffer.Append("[]");
        ProcessExpression(indent, array.Type);
    }

    private void ProcessAssignment(int indent, Assignment assignment)
    {
        Indent(indent);
        ProcessExpression(indent, assignment.Left);
        Buffer.Append(" ");
        Buffer.Append(assignment.Operator.StringVal());
        Buffer.Append(" ");
        ProcessExpression(indent, assignment.Right);
        Buffer.AppendLine();
    }

    private void ProcessBinaryOperator(int indent, BinaryOperator binaryOperator, int parentPrecedence)
    {
        bool parentheses = Settings.AllParens || binaryOperator.Operator.Precedence() < parentPrecedence;
        bool space = binaryOperator.Operator != BinaryOperatorType.Dot && binaryOperator.Operator != BinaryOperatorType.NullDot;
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
        Buffer.Append(cl.Name.GetLiteral());
        if (cl.Parent != null)
        {
            Buffer.Append(" from ");
            ProcessExpression(indent, cl.Parent);
        }
        if (cl.Interfaces.Any())
        {
            Buffer.Append(" is ");
            ProcessExpression(indent, cl.Interfaces[0]);
            for (int i = 1; i < cl.Interfaces.Count; i++)
            {
                Buffer.Append(", ");
                ProcessExpression(indent, cl.Interfaces[i]);
            }
        }
        Buffer.AppendLine();
        ProcessStatements(indent + 1, cl.Statements);
    }

    private void ProcessComment(int indent, Comment comment)
    {
        AppendIndented(indent, ";");
        if (comment.IsDocumentation)
            Buffer.Append(";");
        Buffer.AppendLine(comment.Value);
    }

    private void ProcessConstant(int indent, Constant constant)
    {
        AppendModifiersIndented(indent, constant.Modifiers);
        Buffer.Append("val ");
        Buffer.Append(constant.Name);
        Buffer.Append(" ");
        ProcessExpression(indent, constant.Type);
        if (constant.Value != null)
        {
            Buffer.Append(" = ");
            ProcessExpression(indent, constant.Value);
        }
        Buffer.AppendLine();
    }

    private void ProcessConstructorDefinition(int indent, ConstructorDefinition ctorDef)
    {
        AppendModifiersIndented(indent, ctorDef.Modifiers);
        Buffer.Append("fn new(");
        bool paramMultiline = Settings.ParamMultiline && ctorDef.Parameters.Count > 1;
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent + 1);
        }
        if (ctorDef.Parameters.Any())
        {
            ProcessParameter(indent, ctorDef.Parameters[0]);
            for (int i = 1; i < ctorDef.Parameters.Count; i++)
            {
                Buffer.Append(",");
                if (paramMultiline)
                {
                    Buffer.AppendLine();
                    Indent(indent + 1);
                }
                else
                    Buffer.Append(" ");
                ProcessParameter(indent, ctorDef.Parameters[i]);
            }
        }
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent);
        }
        Buffer.Append(")");
        if (ctorDef.Statements.Count() == 1)
        {
            Buffer.Append(" is ");
            Process(0, ctorDef.Statements[0], null);
        }
        else
        {
            Buffer.AppendLine();
            ProcessStatements(indent + 1, ctorDef.Statements);
        }
    }

    private void ProcessContinue(int indent) => AppendLineIndented(indent, "continue");

    private void ProcessExpressionStatement(int indent, ExpressionStatement expressionStatement)
    {
        Indent(indent);
        ProcessExpression(indent, expressionStatement.Expression);
        Buffer.AppendLine();
    }

    private void ProcessFile(int indent, Core.Ast.File file) => ProcessStatements(indent, file.Statements);

    private void ProcessGeneric(int indent, Generic generic)
    {
        ProcessExpression(indent, generic.Expr);
        Buffer.Append("{");
        ProcessExpression(indent, generic.Arguments[0]);
        for (int i = 1; i < generic.Arguments.Count; i++)
        {
            Buffer.Append(", ");
            ProcessExpression(indent, generic.Arguments[i]);
        }
        Buffer.Append("}");
    }

    private void ProcessIdentifier(Identifier identifier)
    {
        Buffer.Append(identifier.Value);

        // if (identifier.Parts.Count == 2 && identifier.Parts[0].Value == "System" && identifier.Parts[0].Types == null && identifier.Parts[1].Types == null)
        // {
        //     switch (identifier.Parts[1].Value)
        //     {
        //         case "String":
        //             Buffer.Append("str");
        //             return;
        //         case "Char":
        //             Buffer.Append("char");
        //             return;
        //         case "Boolean":
        //             Buffer.Append("bool");
        //             return;
        //         case "SByte":
        //             Buffer.Append("i8");
        //             return;
        //         case "Int16":
        //             Buffer.Append(Settings.CTypes ? "short" : "i16");
        //             return;
        //         case "Int32":
        //             Buffer.Append(Settings.CTypes ? "int" : "i32");
        //             return;
        //         case "Int64":
        //             Buffer.Append(Settings.CTypes ? "long" : "i64");
        //             return;
        //         case "Byte":
        //             Buffer.Append(Settings.CTypes ? "byte" : "u8");
        //             return;
        //         case "UInt16":
        //             Buffer.Append(Settings.CTypes ? "ushort" : "u16");
        //             return;
        //         case "UInt32":
        //             Buffer.Append(Settings.CTypes ? "uint" : "u32");
        //             return;
        //         case "UInt64":
        //             Buffer.Append(Settings.CTypes ? "ulong" : "u64");
        //             return;
        //         case "Single":
        //             Buffer.Append(Settings.CTypes ? "float" : "f32");
        //             return;
        //         case "Double":
        //             Buffer.Append(Settings.CTypes ? "double" : "f64");
        //             return;
        //         case "Decimal":
        //             Buffer.Append("decimal");
        //             return;
        //     }
        // }

        // ProcessIdentifierPart(identifier.Parts[0]);
        // for (int i = 1; i < identifier.Parts.Count; i++)
        // {
        //     Buffer.Append(".");
        //     ProcessIdentifierPart(identifier.Parts[i]);
        // }
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
        Buffer.AppendJoin(".", import.Value);
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

    private void ProcessMethodCall(int indent, MethodCall methodCall)
    {
        ProcessExpression(indent, methodCall.Expr);
        Buffer.Append("(");
        if (methodCall.Arguments.Any())
        {
            ProcessExpression(indent, methodCall.Arguments[0]);
            for (int i = 1; i < methodCall.Arguments.Count; i++)
            {
                Buffer.Append(", ");
                ProcessExpression(indent, methodCall.Arguments[i]);
            }
        }
        Buffer.Append(")");
    }

    private void ProcessMethodDefinition(int indent, MethodDefinition methodDef)
    {
        AppendModifiersIndented(indent, methodDef.Modifiers);
        Buffer.Append("fn ");
        Buffer.Append(methodDef.Name.GetLiteral());
        Buffer.Append("(");
        bool paramMultiline = Settings.ParamMultiline && methodDef.Parameters.Count > 1;
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent + 1);
        }
        if (methodDef.Parameters.Any())
        {
            ProcessParameter(indent, methodDef.Parameters[0]);
            for (int i = 1; i < methodDef.Parameters.Count; i++)
            {
                Buffer.Append(",");
                if (paramMultiline)
                {
                    Buffer.AppendLine();
                    Indent(indent + 1);
                }
                else
                    Buffer.Append(" ");
                ProcessParameter(indent, methodDef.Parameters[i]);
            }
        }
        if (paramMultiline)
        {
            Buffer.AppendLine();
            Indent(indent);
        }
        Buffer.Append(")");
        if (methodDef.ReturnType != null)
        {
            Buffer.Append(" ");
            ProcessExpression(indent, methodDef.ReturnType);
            if (methodDef.Statements.Count() == 1 && methodDef.Statements[0] is Return ret)
            {
                Buffer.Append(" is ");
                ProcessExpression(indent, ret.Value);
                Buffer.AppendLine();
                return;
            }
        }
        else if (methodDef.Statements.Count() == 1)
        {
            Buffer.Append(" is ");
            Process(0, methodDef.Statements[0], null);
            return;
        }

        Buffer.AppendLine();
        ProcessStatements(indent + 1, methodDef.Statements);
    }

    private void ProcessNamespace(int indent, Namespace ns)
    {
        AppendIndented(indent, "ns ");
        Buffer.AppendJoin(".", ns.Value);
        Buffer.AppendLine();
    }

    private void ProcessParameter(int indent, Parameter parameter)
    {
        Buffer.Append(parameter.Name.GetLiteral());
        Buffer.Append(" ");
        ProcessExpression(indent, parameter.Type);
    }

    private void ProcessProperty(int indent, Property property)
    {
        AppendModifiersIndented(indent, property.Modifiers);
        Buffer.Append("fn ");
        Buffer.Append(property.Name.GetLiteral());
        Buffer.Append(" ");
        ProcessExpression(indent, property.Type);
        if (property.GetStatements.Count == 0 && property.SetStatements.Count == 0 && property.Get != property.Set)
            Buffer.Append(property.Get ? " get" : " set");
        else if (property.SetStatements.Count == 0 && property.GetStatements.Count == 1 && property.GetStatements[0] is Return ret)
        {
            Buffer.Append(" is ");
            ProcessExpression(indent, ret.Value);
        }
        if (property.Value != null)
        {
            Buffer.Append(" = ");
            ProcessExpression(indent, property.Value);
        }
        Buffer.AppendLine();
        if (property.SetStatements.Count == 0 && property.GetStatements.Count > 1)
        {
            foreach (var statement in property.GetStatements)
                Process(indent + 1, statement, null);
            return;
        }
        else if (property.SetStatements.Any())
        {
            if (property.GetStatements.Any())
            {
                AppendIndented(indent + 1, "get");

                if (property.GetStatements.Count == 1 && property.GetStatements[0] is Return ret)
                {
                    Buffer.Append(" is ");
                    ProcessExpression(indent, ret.Value);
                    Buffer.AppendLine();
                }
                else
                {
                    Buffer.AppendLine();
                    foreach (var statement in property.GetStatements)
                        Process(indent + 2, statement, null);
                }
            }
        }
        if (property.SetStatements.Any())
        {
            AppendIndented(indent + 1, "set(");
            Buffer.Append(property.SetParameterName);
            Buffer.Append(")");
            if (property.SetStatements.Count == 1)
            {
                Buffer.Append(" is ");
                Process(0, property.SetStatements[0], null);
            }
            else
            {
                Buffer.AppendLine();
                foreach (var statement in property.SetStatements)
                    Process(indent + 2, statement, null);
            }
        }
    }

    private void ProcessReturn(int indent, Return ret)
    {
        AppendIndented(indent, "ret");
        if (ret.Value != null)
        {
            Buffer.Append(" ");
            ProcessExpression(indent, ret.Value);
        }
        Buffer.AppendLine();
    }

    private void ProcessSpace(Space space)
    {
        for (int i = 0; i < space.Size; i++)
            Buffer.AppendLine();
    }

    private void ProcessStatements(int indent, List<Statement> statements)
    {
        ISyntaxTreeItem? priorStatement = null;
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

    private void ProcessField(int indent, Field variable)
    {
        AppendModifiersIndented(indent, variable.Modifiers);
        Buffer.Append("var ");
        Buffer.Append(variable.Name);
        Buffer.Append(" ");
        ProcessExpression(indent, variable.Type);
        if (variable.Value != null)
        {
            Buffer.Append(" = ");
            ProcessExpression(indent, variable.Value);
        }
        Buffer.AppendLine();
    }
}