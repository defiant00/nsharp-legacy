using NSharp.Core.Ast;

namespace NSharp.Core;

public interface ILanguage
{
    public AstItem Load(string fileName);
    public string Save(AstItem ast);
}