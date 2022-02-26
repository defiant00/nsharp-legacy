namespace NSharp.Core;

public abstract class Result
{
    public string FileName { get; set; } = string.Empty;

    public List<Diagnostic> Diagnostics = new();
}