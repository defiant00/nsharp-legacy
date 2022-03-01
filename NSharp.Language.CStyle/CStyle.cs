using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.CStyle;

public class CStyle
{
    public static string Process(Core.Ast.File file)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"// {file.Name}");
        sb.AppendLine();
        for (int i = 0; i < file.Statements.Count; i++)
            Process(sb, i, 0, file.Statements[i]);

        return sb.ToString();
    }

    private static void Process(StringBuilder sb, int index, int indent, AstItem? ast)
    {
        switch (ast)
        {
            case null:
                return;
            case Break b:
                Process(sb, indent, b);
                break;
            case Class c:
                Process(sb, index, indent, c);
                break;
            case Comment c:
                Process(sb, indent, c);
                break;
            case Continue c:
                Process(sb, indent, c);
                break;
            case CurrentObjectInstance c:
                Process(sb, c);
                break;
            case ExpressionStatement es:
                Process(sb, indent, es);
                break;
            case FunctionDefinition fd:
                Process(sb, index, indent, fd);
                break;
            case Identifier i:
                Process(sb, i);
                break;
            case If i:
                Process(sb, indent, i);
                break;
            case LiteralToken lt:
                Process(sb, lt);
                break;
            case Loop l:
                Process(sb, indent, l);
                break;
            case Space s:
                Process(sb, s);
                break;
        }
    }

    private static void Process(StringBuilder sb, int indent, Break br) => sb.AppendLineIndented(indent, "break;");

    private static void Process(StringBuilder sb, int index, int indent, Class cl)
    {
        if (index > 0)
            sb.AppendLine();

        sb.AppendIndented(indent, $"class ");
        Process(sb, cl.Name);
        sb.AppendLine();
        Process(sb, indent, cl.Statements);
    }

    private static void Process(StringBuilder sb, int indent, Comment comment) => sb.AppendLineIndented(indent, $"// {comment.Value}");

    private static void Process(StringBuilder sb, int indent, Continue cont) => sb.AppendLineIndented(indent, "continue;");

    private static void Process(StringBuilder sb, CurrentObjectInstance co) => sb.Append("this");

    private static void Process(StringBuilder sb, int indent, ExpressionStatement es)
    {
        sb.Indent(indent);
        Process(sb, 0, indent, es.Expression);
        sb.AppendLine(";");
    }

    private static void Process(StringBuilder sb, int index, int indent, FunctionDefinition fd)
    {
        if (index > 0)
            sb.AppendLine();

        sb.AppendIndented(indent, $"public static void ");
        Process(sb, fd.Name);
        sb.AppendLine("()");
        Process(sb, indent, fd.Statements);
    }

    private static void Process(StringBuilder sb, Identifier id)
    {
        if (Helpers.Keywords.Contains(id.Value))
            sb.Append("`");
        sb.Append(id.Value);
    }

    private static void Process(StringBuilder sb, int indent, If ifSt)
    {
        sb.AppendIndented(indent, "if (");
        Process(sb, 0, indent, ifSt.Condition);
        sb.AppendLine(")");
        Process(sb, indent, ifSt.Statements);
        if (ifSt.Else != null)
        {
            sb.AppendLineIndented(indent, "else");
            Process(sb, indent, ifSt.Else);
        }
    }

    private static void Process(StringBuilder sb, LiteralToken lit)
    {
        switch (lit.Token)
        {
            case Token.True:
                sb.Append("true");
                break;
            case Token.False:
                sb.Append("false");
                break;
            case Token.Null:
                sb.Append("null");
                break;
        }
    }

    private static void Process(StringBuilder sb, int indent, Loop loop)
    {
        if (loop.ConditionAtEnd)
            sb.AppendLineIndented(indent, "do");
        else
        {
            sb.AppendIndented(indent, "while (");
            Process(sb, 0, indent, loop.Condition);
            sb.AppendLine(")");
        }
        sb.AppendLineIndented(indent, "{");
        for (int i = 0; i < loop.Statements.Count; i++)
            Process(sb, i, indent + 1, loop.Statements[i]);
        if (loop.ConditionAtEnd)
        {
            sb.AppendIndented(indent, "} while (");
            Process(sb, 0, indent, loop.Condition);
            sb.AppendLine(");");
        }
        else
            sb.AppendLineIndented(indent, "}");
    }

    private static void Process(StringBuilder sb, Space space)
    {
        for (int i = 0; i < space.Size; i++)
            sb.AppendLine();
    }

    private static void Process(StringBuilder sb, int indent, List<Statement> statements)
    {
        bool curlies = !(statements.Count == 1 && statements[0].IsCode);
        if (curlies)
            sb.AppendLineIndented(indent, "{");
        for (int i = 0; i < statements.Count; i++)
            Process(sb, i, indent + 1, statements[i]);
        if (curlies)
            sb.AppendLineIndented(indent, "}");
    }
}
