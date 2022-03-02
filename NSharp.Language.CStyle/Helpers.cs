using System.Text;

namespace NSharp.Language.CStyle;

public static class Helpers
{
    public static void Indent(this StringBuilder sb, int indent) => sb.Append('\t', indent);

    public static void AppendIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.Append(content);
    }

    public static void AppendLineIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.AppendLine(content);
    }

    public static HashSet<string> Keywords = new HashSet<string> {
        "true",
        "false",
        "null",
        "this",
    };
}