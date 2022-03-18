namespace NSharp.Language.Min.Compiler;

public struct AcceptResult
{
    public bool Success { get; set; } = true;
    public bool Failure => !Success;
    public int StartingIndex { get; set; }
    public int Count { get; set; } = 0;

    public AcceptResult(int startingIndex) => StartingIndex = startingIndex;
}