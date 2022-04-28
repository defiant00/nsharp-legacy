using System.Text;
using NSharp.Core;
using NSharp.Language.Min.Compiler;

namespace NSharp.Language.Min;

public static class Helpers
{
    public static void Indent(this StringBuilder sb, string indentStr, int indent)
    {
        for (int i = 0; i < indent; i++)
            sb.Append(indentStr);
    }

    public static void AppendIndented(this StringBuilder sb, string indentStr, int indent, string content)
    {
        sb.Indent(indentStr, indent);
        sb.Append(content);
    }

    public static void AppendLineIndented(this StringBuilder sb, string indentStr, int indent, string content)
    {
        sb.Indent(indentStr, indent);
        sb.AppendLine(content);
    }

    public static void AppendModifiersIndented(this StringBuilder sb, string indentStr, int indent, List<Modifier> modifiers)
    {
        sb.Indent(indentStr, indent);
        foreach (var modifier in modifiers)
        {
            sb.Append(modifier.StringVal());
            sb.Append(" ");
        }
    }

    public static string GetLiteral(this string val) => KeywordTokens.ContainsKey(val) ? Constants.LITERAL_INDICATOR + val : val;

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

    public static string StringVal(this BinaryOperatorType op) =>
        op switch
        {
            BinaryOperatorType.Dot => ".",
            BinaryOperatorType.NullDot => "?.",

            BinaryOperatorType.As => "as",

            BinaryOperatorType.Multiply => "*",
            BinaryOperatorType.Divide => "/",
            BinaryOperatorType.Modulus => "%",

            BinaryOperatorType.Add => "+",
            BinaryOperatorType.Subtract => "-",

            BinaryOperatorType.LeftShift => "<<",
            BinaryOperatorType.RightShift => ">>",

            BinaryOperatorType.BitwiseAnd => "&",

            BinaryOperatorType.BitwiseOr => "|",
            BinaryOperatorType.BitwiseXor => "^",

            BinaryOperatorType.LessThan => "<",
            BinaryOperatorType.GreaterThan => ">",
            BinaryOperatorType.LessThanOrEqual => "<=",
            BinaryOperatorType.GreaterThanOrEqual => ">=",

            BinaryOperatorType.Equal => "==",
            BinaryOperatorType.NotEqual => "!=",

            BinaryOperatorType.And => "and",

            BinaryOperatorType.Or => "or",

            BinaryOperatorType.NullCoalesce => "??",

            _ => $"[{op}]",
        };

    public static string StringVal(this AssignmentOperator op) =>
        op switch
        {
            AssignmentOperator.Assign => "=",
            AssignmentOperator.Add => "+=",
            AssignmentOperator.Subtract => "-=",
            AssignmentOperator.Multiply => "*=",
            AssignmentOperator.Divide => "/=",
            AssignmentOperator.Modulus => "%=",
            AssignmentOperator.BitwiseAnd => "&=",
            AssignmentOperator.BitwiseOr => "|=",
            AssignmentOperator.BitwiseXor => "^=",
            AssignmentOperator.LeftShift => "<<=",
            AssignmentOperator.RightShift => ">>=",
            AssignmentOperator.NullCoalesce => "??=",
            _ => $"[{op}]",
        };

    public static readonly Dictionary<string, TokenType> KeywordTokens = new()
    {
        ["ns"] = TokenType.Namespace,
        ["use"] = TokenType.Use,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["_"] = TokenType.Discard,
        ["public"] = TokenType.Public,
        ["protected"] = TokenType.Protected,
        ["internal"] = TokenType.Internal,
        ["private"] = TokenType.Private,
        ["static"] = TokenType.Static,
        ["this"] = TokenType.This,
        ["str"] = TokenType.String,
        ["char"] = TokenType.Character,
        ["bool"] = TokenType.Boolean,
        ["i8"] = TokenType.I8,
        ["i16"] = TokenType.I16,
        ["short"] = TokenType.I16,
        ["i32"] = TokenType.I32,
        ["int"] = TokenType.I32,
        ["i64"] = TokenType.I64,
        ["long"] = TokenType.I64,
        ["u8"] = TokenType.U8,
        ["byte"] = TokenType.U8,
        ["u16"] = TokenType.U16,
        ["ushort"] = TokenType.U16,
        ["u32"] = TokenType.U32,
        ["uint"] = TokenType.U32,
        ["u64"] = TokenType.U64,
        ["ulong"] = TokenType.U64,
        ["f32"] = TokenType.F32,
        ["float"] = TokenType.F32,
        ["f64"] = TokenType.F64,
        ["double"] = TokenType.F64,
        ["decimal"] = TokenType.Decimal,
        ["class"] = TokenType.Class,
        ["struct"] = TokenType.Struct,
        ["enum"] = TokenType.Enum,
        ["interface"] = TokenType.Interface,
        ["fn"] = TokenType.Function,
        ["del"] = TokenType.Delegate,
        ["var"] = TokenType.Variable,
        ["val"] = TokenType.Value,
        ["get"] = TokenType.Get,
        ["set"] = TokenType.Set,
        ["from"] = TokenType.From,
        ["is"] = TokenType.Is,
        ["as"] = TokenType.As,
        ["new"] = TokenType.New,
        ["base"] = TokenType.Base,
        ["ret"] = TokenType.Return,
        ["try"] = TokenType.Try,
        ["catch"] = TokenType.Catch,
        ["fin"] = TokenType.Finally,
        ["throw"] = TokenType.Throw,
        ["for"] = TokenType.For,
        ["in"] = TokenType.In,
        ["bet"] = TokenType.Between,
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
        [".."] = TokenType.DoubleDot,
        ["?."] = TokenType.NullDot,
        ["??"] = TokenType.NullCoalesce,
        [","] = TokenType.Comma,
        ["?"] = TokenType.Conditional,
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
        ["%="] = TokenType.ModulusAssign,
        ["&="] = TokenType.BitwiseAndAssign,
        ["|="] = TokenType.BitwiseOrAssign,
        ["^="] = TokenType.BitwiseXorAssign,
        ["<<="] = TokenType.LeftShiftAssign,
        [">>="] = TokenType.RightShiftAssign,
        ["??="] = TokenType.NullCoalesceAssign,
        ["*"] = TokenType.Multiply,
        ["/"] = TokenType.Divide,
        ["%"] = TokenType.Modulus,
        ["+"] = TokenType.Add,
        ["-"] = TokenType.Subtract,
        ["!"] = TokenType.Not,
        ["~"] = TokenType.BitwiseNot,
    };
}