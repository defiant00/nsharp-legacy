using NSharp.Core;
using NSharp.Core.Ast;
using NSharp.Language.Min.Compiler;

namespace NSharp.Language.Min;

public class Min : ILanguage
{
    private Settings Settings { get; set; }

    public Min(Dictionary<string, string> settings)
    {
        Settings = new Settings(settings);
    }

    public LoadResult Load(string fileName)
    {
        var result = new LoadResult(fileName);

        var parser = new Parser();
        result.Ast = parser.Parse(fileName).Result as Core.Ast.File;

        return result;
    }

    public SaveResult Save(string fileName, AstItem ast)
    {
        var result = new SaveResult(fileName);
        string code = new Decompiler(Settings).Decompile(ast);
        System.IO.File.WriteAllText(fileName, code);
        return result;
    }
}