using NSharp.Core;

namespace NSharp.Language.Min.Compiler;

public static class TokenHelper
{
    public static bool IsAssignment(this TokenType type) => type > TokenType.Assignment_Start && type < TokenType.Assignment_End;
    public static bool IsComparisonOperator(this TokenType type) => type > TokenType.Comparison_Operator_Start && type < TokenType.Comparison_Operator_End;
    public static bool IsLiteralExpression(this TokenType type) => type > TokenType.Literal_Expression_Start && type < TokenType.Literal_Expression_End;
    public static bool IsModifier(this TokenType type) => type > TokenType.Modifier_Start && type < TokenType.Modifier_End;
    public static bool IsPostOperator(this TokenType type) => type > TokenType.Post_Operator_Start && type < TokenType.Post_Operator_End;
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

    public static OperatorType ToOperator(this TokenType type) =>
        type switch
        {
            TokenType.Dot => OperatorType.Dot,

            TokenType.Multiply => OperatorType.Multiply,
            TokenType.Divide => OperatorType.Divide,
            TokenType.Modulo => OperatorType.Modulo,

            TokenType.Add => OperatorType.Add,
            TokenType.Subtract => OperatorType.Subtract,

            TokenType.LeftShift => OperatorType.LeftShift,
            TokenType.RightShift => OperatorType.RightShift,

            TokenType.BitwiseAnd => OperatorType.BitwiseAnd,

            TokenType.BitwiseOr => OperatorType.BitwiseOr,
            TokenType.BitwiseXor => OperatorType.BitwiseXor,

            TokenType.LessThan => OperatorType.LessThan,
            TokenType.GreaterThan => OperatorType.GreaterThan,
            TokenType.LessThanOrEqual => OperatorType.LessThanOrEqual,
            TokenType.GreaterThanOrEqual => OperatorType.GreaterThanOrEqual,

            TokenType.Equal => OperatorType.Equal,
            TokenType.NotEqual => OperatorType.NotEqual,

            TokenType.And => OperatorType.And,

            TokenType.Or => OperatorType.Or,

            _ => OperatorType.None,
        };

    public static int Precedence(this Token token) => token.Type.ToOperator().Precedence();
}