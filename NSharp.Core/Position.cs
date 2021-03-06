namespace NSharp.Core;

public struct Position
{
    public int Line { get; set; }
    public int Column { get; set; }

    public Position(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public override string ToString() => $"({Line}, {Column})";
}