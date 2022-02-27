using NSharp.Core;

namespace NSharp.Language.Neutral.Compiler;

public enum TokenType
{
    Error,
    Indent,
    Dedent,
    EOL,
    EOF,
    Comment,
    Character,
    String,
    Number,
    Literal,
    Namespace,
    Use,
    If,
    Else,
    // modifiers
    // types
    // enum
    // class
    Return,
    Try,
    Catch,
    Finally,
    Throw,
    For,
    // while/loop
    Break,
    Continue,
    Literal_Expression_Start,
    True,
    False,
    Null,
    Literal_Expression_End,
    Comparison_Operators_Start,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Comparison_Operators_End,
    And,
    Or,
    // in
    // instanceof
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift,
    Dot,
    Comma,
    Colon,
    LeftParenthesis,
    RightParenthesis,
    LeftBracket,
    RightBracket,
    LeftCurly,
    RightCurly,
    Assignment_Start,
    Assign,
    AddAssign,
    SubtractAssign,
    MultiplyAssign,
    DivideAssign,
    ModuloAssign,
    BitwiseAndAssign,
    BitwiseOrAssign,
    BitwiseXorAssign,
    LeftShiftAssign,
    RightShiftAssign,
    Assignment_End,
    Multiply,
    Divide,
    Modulo,
    Unary_Operator_Start,
    Add,
    Subtract,
    Not,
    BitwiseNot,
    Post_Operator_Start,
    Increment,
    Decrement,
    Post_Operator_End,
    Unary_Operator_End,
}

public class Token
{
    public TokenType Type { get; set; }
    public Position Position { get; set; }
    public string? Value { get; set; }

    public override string ToString() => $"{Type} {Position} '{Value}'";
}