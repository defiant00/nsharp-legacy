using System.Text;
using NSharp.Language.Neutral.Compiler;

namespace NSharp.Language.Neutral;

public static class Helpers
{
    public static void Indent(this StringBuilder sb, int indent) => sb.Append(' ', indent * 4);

    public static void AppendIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.Append(content);
    }

    public static void AppendLineIndented(this StringBuilder sb, int indent, string content)
    {
        sb.Indent(indent);
        sb.AppendLine(content);
    }

    public static readonly Dictionary<string, TokenType> KeywordTokens = new()
    {
        ["ns"] = TokenType.Namespace,
        ["use"] = TokenType.Use,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["return"] = TokenType.Return,
        ["try"] = TokenType.Try,
        ["catch"] = TokenType.Catch,
        ["finally"] = TokenType.Finally,
        ["throw"] = TokenType.Throw,
        ["for"] = TokenType.For,
        ["break"] = TokenType.Break,
        ["continue"] = TokenType.Continue,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["null"] = TokenType.Null,
        ["="] = TokenType.Equal,
        ["!="] = TokenType.NotEqual,
        ["<"] = TokenType.LessThan,
        [">"] = TokenType.GreaterThan,
        ["<="] = TokenType.LessThanOrEqual,
        [">="] = TokenType.GreaterThanOrEqual,
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["&"] = TokenType.BitwiseAnd,
        ["|"] = TokenType.BitwiseOr,
        ["^"] = TokenType.BitwiseXor,
        ["."] = TokenType.Dot,
        [","] = TokenType.Comma,
        ["("] = TokenType.LeftParenthesis,
        [")"] = TokenType.RightParenthesis,
        ["["] = TokenType.LeftBracket,
        ["]"] = TokenType.RightBracket,
        [":"] = TokenType.Assign,
        ["+:"] = TokenType.AddAssign,
        ["-:"] = TokenType.SubtractAssign,
        ["*:"] = TokenType.MultiplyAssign,
        ["/:"] = TokenType.DivideAssign,
        ["%:"] = TokenType.ModuloAssign,
        ["&:"] = TokenType.BitwiseAndAssign,
        ["|:"] = TokenType.BitwiseOrAssign,
        ["^:"] = TokenType.BitwiseXorAssign,
        ["<<:"] = TokenType.LeftShiftAssign,
        [">>:"] = TokenType.RightShiftAssign,
        ["*"] = TokenType.Multiply,
        ["/"] = TokenType.Divide,
        ["%"] = TokenType.Modulo,
        ["+"] = TokenType.Add,
        ["-"] = TokenType.Subtract,
        ["!"] = TokenType.Not,
        ["~"] = TokenType.BitwiseNot,
        ["++"] = TokenType.Increment,
        ["--"] = TokenType.Decrement,
    };
}