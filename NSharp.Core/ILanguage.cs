using NSharp.Core.Ast;

namespace NSharp.Core;

public interface ILanguage
{
    public LoadResult Load(string fileName);
    public SaveResult Save(string fileName, AstItem ast);
}