using NSharp.Core;

namespace NSharp.Language.Min.Compiler;

public static class TokenHelper
{
    public static bool IsAssignment(this TokenType type) => type > TokenType.Assignment_Start && type < TokenType.Assignment_End;
    public static bool IsLineContinuationHeld(this TokenType type) => type > TokenType.Line_Continuation_Held_Start && type < TokenType.Line_Continuation_Held_End;
    public static bool IsLineContinuationPostfix(this TokenType type) => type > TokenType.Line_Continuation_Postfix_Start && type < TokenType.Line_Continuation_Postfix_End;
    public static bool IsLineContinuationPrefix(this TokenType type) => type > TokenType.Line_Continuation_Prefix_Start && type < TokenType.Line_Continuation_Prefix_End;
    public static bool IsModifier(this TokenType type) => type > TokenType.Modifier_Start && type < TokenType.Modifier_End;
    public static bool IsType(this TokenType type) => type > TokenType.Type_Start && type < TokenType.Type_End;
    public static bool IsUnaryOperator(this TokenType type) => type > TokenType.Unary_Operator_Start && type < TokenType.Unary_Operator_End;

    public static Modifier ToModifier(this TokenType type) =>
        type switch
        {
            TokenType.Public => Modifier.Public,
            TokenType.Protected => Modifier.Protected,
            TokenType.Internal => Modifier.Internal,
            TokenType.Private => Modifier.Private,
            TokenType.Static => Modifier.Static,
            _ => throw new Exception($"Could not match token type {type} to a modifier."),
        };

    public static UnaryOperatorType ToUnaryOperator(this TokenType type) =>
        type switch
        {
            TokenType.Subtract => UnaryOperatorType.Negate,
            TokenType.Not => UnaryOperatorType.Not,
            TokenType.BitwiseNot => UnaryOperatorType.BitwiseNot,
            _ => UnaryOperatorType.None,
        };

    public static BinaryOperatorType ToBinaryOperator(this TokenType type) =>
        type switch
        {
            TokenType.Dot => BinaryOperatorType.Dot,
            TokenType.NullDot => BinaryOperatorType.NullDot,

            TokenType.As => BinaryOperatorType.As,

            TokenType.Multiply => BinaryOperatorType.Multiply,
            TokenType.Divide => BinaryOperatorType.Divide,
            TokenType.Modulo => BinaryOperatorType.Modulo,

            TokenType.Add => BinaryOperatorType.Add,
            TokenType.Subtract => BinaryOperatorType.Subtract,

            TokenType.LeftShift => BinaryOperatorType.LeftShift,
            TokenType.RightShift => BinaryOperatorType.RightShift,

            TokenType.BitwiseAnd => BinaryOperatorType.BitwiseAnd,

            TokenType.BitwiseOr => BinaryOperatorType.BitwiseOr,
            TokenType.BitwiseXor => BinaryOperatorType.BitwiseXor,

            TokenType.LessThan => BinaryOperatorType.LessThan,
            TokenType.GreaterThan => BinaryOperatorType.GreaterThan,
            TokenType.LessThanOrEqual => BinaryOperatorType.LessThanOrEqual,
            TokenType.GreaterThanOrEqual => BinaryOperatorType.GreaterThanOrEqual,

            TokenType.Equal => BinaryOperatorType.Equal,
            TokenType.NotEqual => BinaryOperatorType.NotEqual,

            TokenType.And => BinaryOperatorType.And,

            TokenType.Or => BinaryOperatorType.Or,

            TokenType.NullCoalesce => BinaryOperatorType.NullCoalesce,

            _ => BinaryOperatorType.None,
        };

    public static int Precedence(this Token token) => token.Type.ToBinaryOperator().Precedence();

    public static AssignmentOperator ToAssignmentOperator(this TokenType type) =>
        type switch
        {
            TokenType.Assign => AssignmentOperator.Assign,
            TokenType.AddAssign => AssignmentOperator.Add,
            TokenType.SubtractAssign => AssignmentOperator.Subtract,
            TokenType.MultiplyAssign => AssignmentOperator.Multiply,
            TokenType.DivideAssign => AssignmentOperator.Divide,
            TokenType.ModuloAssign => AssignmentOperator.Modulo,
            TokenType.BitwiseAndAssign => AssignmentOperator.BitwiseAnd,
            TokenType.BitwiseOrAssign => AssignmentOperator.BitwiseOr,
            TokenType.BitwiseXorAssign => AssignmentOperator.BitwiseXor,
            TokenType.LeftShiftAssign => AssignmentOperator.LeftShift,
            TokenType.RightShiftAssign => AssignmentOperator.RightShift,
            TokenType.NullCoalesceAssign => AssignmentOperator.NullCoalesce,
            _ => AssignmentOperator.None,
        };
}