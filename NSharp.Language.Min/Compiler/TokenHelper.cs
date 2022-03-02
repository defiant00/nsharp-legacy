namespace NSharp.Language.Min.Compiler;

public static class TokenHelper
{
    public static bool IsAssignment(this TokenType type) => type > TokenType.Assignment_Start && type < TokenType.Assignment_End;
    public static bool IsComparisonOperator(this TokenType type) => type > TokenType.Comparison_Operator_Start && type < TokenType.Comparison_Operator_End;
    public static bool IsLiteralExpression(this TokenType type) => type > TokenType.Literal_Expression_Start && type < TokenType.Literal_Expression_End;
    public static bool IsModifier(this TokenType type) => type > TokenType.Modifier_Start && type < TokenType.Modifier_End;
    public static bool IsPostOperator(this TokenType type) => type > TokenType.Post_Operator_Start && type < TokenType.Post_Operator_End;
    public static bool IsUnaryOperator(this TokenType type) => type > TokenType.Unary_Operator_Start && type < TokenType.Unary_Operator_End;

    public static Core.Token ToCoreToken(this TokenType type) =>
        type switch
        {
            TokenType.Public => Core.Token.Public,
            TokenType.Protected => Core.Token.Protected,
            TokenType.Internal => Core.Token.Internal,
            TokenType.Private => Core.Token.Private,
            TokenType.Static => Core.Token.Static,
            _ => throw new Exception($"Could not match token type {type} to a core token."),
        };
}