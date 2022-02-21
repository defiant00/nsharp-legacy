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
            case Block b:
                Process(sb, indent, b);
                break;
            case Break b:
                Process(sb, indent, b);
                break;
            case Class c:
                Process(sb, indent, c);
                break;
            case Comment c:
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

    private static void Process(StringBuilder sb, int indent, Block b)
    {
        // while loop
        if (b.Statements.Count >= 2
            && b.Statements[0] is If ifSt && ifSt.Block.Statements.Count == 1
            && ifSt.Block.Statements[0] is Break
            && b.Statements[^1] is Loop)
        {
            sb.AppendIndented(indent, "while (!(");
            Process(sb, indent, ifSt.Condition);
            sb.AppendLine("))");
            ProcessBlockRange(sb, indent, b, 1, b.Statements.Count - 1);
            sb.AppendLine();
            return;
        }

        // do while loop
        if (b.Statements.Count >= 1
            && b.Statements[^1] is If ifSt2 && ifSt2.Block.Statements.Count == 1
            && ifSt2.Block.Statements[0] is Loop)
        {
            sb.AppendLineIndented(indent, "do");
            ProcessBlockRange(sb, indent, b, 0, b.Statements.Count - 1);
            sb.Append(" while (");
            Process(sb, indent, ifSt2.Condition);
            sb.AppendLine(");");
            return;
        }

        ProcessBlockRange(sb, indent, b, 0, b.Statements.Count);
        sb.AppendLine();
    }

    private static void ProcessBlockRange(StringBuilder sb, int indent, Block block, int start, int max)
    {
        sb.AppendLineIndented(indent, "{");
        for (int i = start; i < max; i++)
            Process(sb, indent + 1, block.Statements[i]);
        sb.AppendIndented(indent, "}");
    }

    private static void Process(StringBuilder sb, int indent, Break br) => sb.AppendLineIndented(indent, "break;");

    private static void Process(StringBuilder sb, int indent, Class cl)
    {
        sb.AppendLineIndented(indent, $"class {cl.Name}");
        Process(sb, indent, cl.Block);
    }

    private static void Process(StringBuilder sb, int indent, Comment comment) => sb.AppendLineIndented(indent, $"// {comment.Content}");

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
        Process(sb, indent, fd.Block);
    }

    private static void Process(StringBuilder sb, int indent, If i)
    {
        sb.AppendIndented(indent, "if (");
        Process(sb, indent, i.Condition);
        sb.AppendLine(")");
        Process(sb, indent, i.Block);
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

    private static void Process(StringBuilder sb, int indent, Loop loop) => sb.AppendLineIndented(indent, "loop;");

    private static void Process(StringBuilder sb, Space space)
    {
        for (int i = 0; i < space.Size; i++)
            sb.AppendLine();
    }
}
