namespace NSharp.Core;

public struct Position
{
    public int Line { get; set; }
    public int Column { get; set; }

    public override string ToString() => $"({Line}, {Column})";
}