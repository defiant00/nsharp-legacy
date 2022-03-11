namespace NSharp.Core;

public enum OperatorType
{
    None,
    Dot,
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
            OperatorType.Dot => 10,

            OperatorType.Multiply => 9,
            OperatorType.Divide => 9,
            OperatorType.Modulo => 9,

            OperatorType.Add => 8,
            OperatorType.Subtract => 8,

            OperatorType.LeftShift => 7,
            OperatorType.RightShift => 7,

            OperatorType.BitwiseAnd => 6,

            OperatorType.BitwiseOr => 5,
            OperatorType.BitwiseXor => 5,

            OperatorType.LessThan => 4,
            OperatorType.GreaterThan => 4,
            OperatorType.LessThanOrEqual => 4,
            OperatorType.GreaterThanOrEqual => 4,

            OperatorType.Equal => 3,
            OperatorType.NotEqual => 3,

            OperatorType.And => 2,

            OperatorType.Or => 1,

            _ => -1,
        };
}