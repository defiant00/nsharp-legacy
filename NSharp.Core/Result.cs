using NSharp.Core.Ast;

namespace NSharp.Core;

public abstract class Result
{
    public string FileName { get; set; } = string.Empty;

    public List<Diagnostic> Diagnostics = new();
}

public class LoadResult : Result
{
    public AstItem? Ast { get; set; }
}

public class SaveResult : Result
{
    public bool Success { get; set; }
}