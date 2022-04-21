namespace NSharp.Core;

public enum OperatorType
{
    None,
    Dot,
    NullDot,
    NullCoalesce,
    Is,
    As,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Not,
    BitwiseNot,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    And,
    Or,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift,
}

public static class OperatorHelpers
{
    public static int Precedence(this OperatorType operatorType) =>
        operatorType switch
        {
            OperatorType.Dot => 12,
            OperatorType.NullDot => 12,

            OperatorType.As => 11,

            OperatorType.Multiply => 10,
            OperatorType.Divide => 10,
            OperatorType.Modulo => 10,

            OperatorType.Add => 9,
            OperatorType.Subtract => 9,

            OperatorType.LeftShift => 8,
            OperatorType.RightShift => 8,

            OperatorType.BitwiseAnd => 7,

            OperatorType.BitwiseOr => 6,
            OperatorType.BitwiseXor => 6,

            OperatorType.Is => 5,
            OperatorType.LessThan => 5,
            OperatorType.GreaterThan => 5,
            OperatorType.LessThanOrEqual => 5,
            OperatorType.GreaterThanOrEqual => 5,

            OperatorType.Equal => 4,
            OperatorType.NotEqual => 4,

            OperatorType.And => 3,

            OperatorType.Or => 2,

            OperatorType.NullCoalesce => 1,

            _ => -1,
        };
}