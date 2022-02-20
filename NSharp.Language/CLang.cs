namespace NSharp.Language;

using System.Text;
using NSharp.Core.Ast;

public static class CLang
{
    public static string Process(File file)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// {file.Name}");
        sb.AppendLine();
        if (file.Statements != null)
            foreach (var st in file.Statements)
                Process(sb, 0, st);

        return sb.ToString();
    }

    private static void Process(StringBuilder sb, int indent, AstItem ast)
    {
        switch (ast)
        {
            case Class c:
                Process(sb, indent, c);
                break;
            case Comment c:
                Process(sb, indent, c);
                break;
            case CurrentObjectInstance c:
                Process(sb, indent, c);
                break;
            case ExpressionStatement es:
                Process(sb, indent, es);
                break;
            case FunctionDefinition fd:
                Process(sb, indent, fd);
                break;
            case Space s:
                Process(sb, indent, s);
                break;
        }
    }

    private static void Process(StringBuilder sb, int indent, Class cl)
    {
        sb.Append('\t', indent);
        sb.AppendLine($"class {cl.Name}");
        sb.Append('\t', indent);
        sb.AppendLine("{");
        if (cl.Statements != null)
            foreach (var st in cl.Statements)
                Process(sb, indent + 1, st);
        sb.Append('\t', indent);
        sb.AppendLine("}");
    }

    private static void Process(StringBuilder sb, int indent, Comment comment)
    {
        sb.Append('\t', indent);
        sb.AppendLine($"// {comment.Content}");
    }

    private static void Process(StringBuilder sb, int indent, CurrentObjectInstance co) => sb.Append("this");

    private static void Process(StringBuilder sb, int indent, ExpressionStatement es)
    {
        sb.Append('\t', indent);
        if (es.Expression != null)
            Process(sb, indent, es.Expression);
        sb.AppendLine(";");
    }

    private static void Process(StringBuilder sb, int indent, FunctionDefinition fd)
    {
        sb.Append('\t', indent);
        sb.AppendLine($"public static void {fd.Name}()");
        sb.Append('\t', indent);
        sb.AppendLine("{");
        if (fd.Statements != null)
            foreach (var st in fd.Statements)
                Process(sb, indent + 1, st);
        sb.Append('\t', indent);
        sb.AppendLine("}");
    }

    private static void Process(StringBuilder sb, int indent, Space space)
    {
        for (int i = 0; i < indent; i++)
            sb.AppendLine();
    }
}
