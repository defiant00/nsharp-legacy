using NSharp.Core.Ast;

namespace NSharp.Core;

public abstract class Result
{
    public string FileName { get; set; }
    public List<Diagnostic> Diagnostics = new();

    public Result(string fileName)
    {
        FileName = fileName;
    }
}

public class LoadResult : Result
{
    public Ast.File? Ast { get; set; }

    public LoadResult(string fileName) : base(fileName) { }
}

public class SaveResult : Result
{
    public bool Success { get; set; } = true;

    public SaveResult(string fileName) : base(fileName) { }
}