namespace NSharp.Core;

public enum BinaryOperatorType
{
    None,
    Dot,
    NullDot,
    NullCoalesce,
    As,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulus,
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

public static class BinaryOperatorHelpers
{
    public static int Precedence(this BinaryOperatorType operatorType) =>
        operatorType switch
        {
            BinaryOperatorType.Dot => 12,
            BinaryOperatorType.NullDot => 12,

            BinaryOperatorType.As => 11,

            BinaryOperatorType.Multiply => 10,
            BinaryOperatorType.Divide => 10,
            BinaryOperatorType.Modulus => 10,

            BinaryOperatorType.Add => 9,
            BinaryOperatorType.Subtract => 9,

            BinaryOperatorType.LeftShift => 8,
            BinaryOperatorType.RightShift => 8,

            BinaryOperatorType.BitwiseAnd => 7,

            BinaryOperatorType.BitwiseOr => 6,
            BinaryOperatorType.BitwiseXor => 6,

            BinaryOperatorType.LessThan => 5,
            BinaryOperatorType.GreaterThan => 5,
            BinaryOperatorType.LessThanOrEqual => 5,
            BinaryOperatorType.GreaterThanOrEqual => 5,

            BinaryOperatorType.Equal => 4,
            BinaryOperatorType.NotEqual => 4,

            BinaryOperatorType.And => 3,

            BinaryOperatorType.Or => 2,

            BinaryOperatorType.NullCoalesce => 1,

            _ => -1,
        };
}