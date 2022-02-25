using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.Neutral;

public class Neutral : ILanguage
{
    public AstItem Load(string fileName)
    {
        return new Core.Ast.File { Name = Path.GetFileName(fileName) };
    }

    public string Save(AstItem ast)
    {
        var sb = new StringBuilder();
        Process(sb, 0, 0, ast);
        return sb.ToString();
    }

    private static void Process(StringBuilder sb, int index, int indent, AstItem? ast)
    {
        switch (ast)
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
        for (int i = 0; i < file.Statements.Count; i++)
            Process(sb, i, 0, file.Statements[i]);
    }
}