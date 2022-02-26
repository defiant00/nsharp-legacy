using System.Text;
using NSharp.Core.Ast;

namespace NSharp.Language.Neutral;

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
            case Core.Ast.File file:
                ProcessFile(sb, file);
                break;
        }
    }

    private static void ProcessFile(StringBuilder sb, Core.Ast.File file)
    {
        AstItem? priorItem = null;
        for (int i = 0; i < file.Statements.Count; i++)
        {
            Process(sb, i, file.Statements[i], priorItem);
            priorItem = file.Statements[i];
        }
    }
}