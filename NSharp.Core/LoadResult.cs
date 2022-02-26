using NSharp.Core.Ast;

namespace NSharp.Core;

public class LoadResult : Result
{
    public AstItem? Ast { get; set; }
}