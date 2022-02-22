namespace NSharp.Language;

using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

public static class CLang
{
    private static void Indent(this StringBuilder sb, int indent) => sb.Append('\t', indent);

    private static void AppendIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.Append(content);
    }

    private static void AppendLineIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.AppendLine(content);
    }

    public static string Process(File file)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"// {file.Name}");
        sb.AppendLine();
        foreach (var s in file.Statements)
            Process(sb, 0, s);

        return sb.ToString();
    }

    private static void Process(StringBuilder sb, int indent, AstItem? ast)
    {
        switch (ast)
        {
            case null:
                return;
            case Break b:
                Process(sb, indent, b);
                break;
            case Class c:
                Process(sb, indent, c);
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
                Process(sb, indent, fd);
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

    private static void Process(StringBuilder sb, int indent, Class cl)
    {
        sb.AppendLineIndented(indent, $"class {cl.Name}");
        Process(sb, indent, cl.Statements);
    }

    private static void Process(StringBuilder sb, int indent, Comment comment) => sb.AppendLineIndented(indent, $"// {comment.Content}");

    private static void Process(StringBuilder sb, int indent, Continue cont) => sb.AppendLineIndented(indent, "continue;");

    private static void Process(StringBuilder sb, CurrentObjectInstance co) => sb.Append("this");

    private static void Process(StringBuilder sb, int indent, ExpressionStatement es)
    {
        sb.Indent(indent);
        Process(sb, indent, es.Expression);
        sb.AppendLine(";");
    }

    private static void Process(StringBuilder sb, int indent, FunctionDefinition fd)
    {
        sb.AppendLineIndented(indent, $"public static void {fd.Name}()");
        Process(sb, indent, fd.Statements);
    }

    private static void Process(StringBuilder sb, int indent, If i)
    {
        sb.AppendIndented(indent, "if (");
        Process(sb, indent, i.Condition);
        sb.AppendLine(")");
        Process(sb, indent, i.Statements);
        if (i.Else != null)
        {
            sb.AppendLineIndented(indent, "else");
            Process(sb, indent, i.Else);
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
            Process(sb, indent, loop.Condition);
            sb.AppendLine(")");
        }
        sb.AppendLineIndented(indent, "{");
        foreach (var statement in loop.Statements)
            Process(sb, indent + 1, statement);
        if (loop.ConditionAtEnd)
        {
            sb.AppendIndented(indent, "} while (");
            Process(sb, indent, loop.Condition);
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
        foreach (var s in statements)
            Process(sb, indent + 1, s);
        if (curlies)
            sb.AppendLineIndented(indent, "}");
    }
}
