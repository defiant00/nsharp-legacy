namespace NSharp.Language.Min.Compiler;

public struct AcceptResult
{
    public bool Failure { get; set; } = false;
    public int StartingIndex { get; set; }
    public int Count { get; set; } = 0;

    public AcceptResult(int startingIndex) => StartingIndex = startingIndex;
}