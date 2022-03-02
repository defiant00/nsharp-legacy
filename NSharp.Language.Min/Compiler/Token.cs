using NSharp.Core;

namespace NSharp.Language.Min.Compiler;

public class Token
{
    public TokenType Type { get; set; }
    public Position Position { get; set; }
    public string Value { get; set; }

    public Token(TokenType type, Position position, string value)
    {
        Type = type;
        Position = position;
        Value = value;
    }

    public override string ToString() => $"{Type} {Position} '{Value}'";
}