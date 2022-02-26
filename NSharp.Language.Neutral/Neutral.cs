using NSharp.Core;
using NSharp.Core.Ast;
using NSharp.Language.Neutral.Compiler;

namespace NSharp.Language.Neutral;

public class Neutral : ILanguage
{
    public LoadResult Load(string fileName)
    {
        var result = new LoadResult
        {
            FileName = fileName,
            Ast = new Core.Ast.File { Name = Path.GetFileName(fileName) },
        };

        var lexer = new Lexer(System.IO.File.ReadAllText(fileName));
        
        foreach (var token in lexer.Tokens)
            Console.WriteLine(token);

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