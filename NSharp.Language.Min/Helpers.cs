using System.Text;
using NSharp.Core;
using NSharp.Language.Min.Compiler;

namespace NSharp.Language.Min;

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

    public static void AppendModifiersIndented(this StringBuilder sb, int indent, List<Modifier> modifiers)
    {
        sb.Indent(indent);
        foreach (var mod in modifiers)
        {
            sb.Append(mod.StringVal());
            sb.Append(" ");
        }
    }

    public static string GetLiteral(this string val) => KeywordTokens.ContainsKey(val) ? "`" + val : val;

    public static string StringVal(this Modifier modifier) =>
        modifier switch
        {
            Modifier.Public => "public",
            Modifier.Protected => "protected",
            Modifier.Internal => "internal",
            Modifier.Private => "private",
            Modifier.Static => "static",
            _ => $"[{modifier}]",
        };

    public static readonly Dictionary<string, TokenType> KeywordTokens = new()
    {
        ["ns"] = TokenType.Namespace,
        ["use"] = TokenType.Use,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["public"] = TokenType.Public,
        ["protected"] = TokenType.Protected,
        ["internal"] = TokenType.Internal,
        ["private"] = TokenType.Private,
        ["static"] = TokenType.Static,
        ["this"] = TokenType.This,
        ["void"] = TokenType.Void,
        // types
        ["class"] = TokenType.Class,
        ["struct"] = TokenType.Struct,
        ["enum"] = TokenType.Enum,
        ["interface"] = TokenType.Interface,
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
        ["=="] = TokenType.Equal,
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
        ["<<"] = TokenType.LeftShift,
        [">>"] = TokenType.RightShift,
        ["."] = TokenType.Dot,
        [","] = TokenType.Comma,
        [":"] = TokenType.Colon,
        ["("] = TokenType.LeftParenthesis,
        [")"] = TokenType.RightParenthesis,
        ["["] = TokenType.LeftBracket,
        ["]"] = TokenType.RightBracket,
        ["{"] = TokenType.LeftCurly,
        ["}"] = TokenType.RightCurly,
        ["="] = TokenType.Assign,
        ["+="] = TokenType.AddAssign,
        ["-="] = TokenType.SubtractAssign,
        ["*="] = TokenType.MultiplyAssign,
        ["/="] = TokenType.DivideAssign,
        ["%="] = TokenType.ModuloAssign,
        ["&="] = TokenType.BitwiseAndAssign,
        ["|="] = TokenType.BitwiseOrAssign,
        ["^="] = TokenType.BitwiseXorAssign,
        ["<<="] = TokenType.LeftShiftAssign,
        [">>="] = TokenType.RightShiftAssign,
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