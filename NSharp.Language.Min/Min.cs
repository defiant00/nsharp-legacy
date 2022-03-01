using NSharp.Core;
using NSharp.Core.Ast;
using NSharp.Language.Min.Compiler;

namespace NSharp.Language.Min;

public class Min : ILanguage
{
    public LoadResult Load(string fileName)
    {
        var result = new LoadResult
        {
            FileName = fileName,
        };

        var parser = new Parser();
        result.Ast = parser.Parse(fileName).Result;

        return result;
    }

    public SaveResult Save(string fileName, AstItem ast)
    {
        var result = new SaveResult
        {
            FileName = fileName,
            Success = true,
        };
        string code = Decompiler.Decompile(ast);
        System.IO.File.WriteAllText(fileName, code);
        return result;
    }
}