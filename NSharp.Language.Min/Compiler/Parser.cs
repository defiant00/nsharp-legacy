using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.Min.Compiler;

public class Parser
{
    private List<Token> Tokens { get; set; } = new();
    private int CurrentIndex { get; set; } = 0;

    private Token Peek => Tokens[CurrentIndex];

    private Token Next() => Tokens[CurrentIndex++];

    private void Backup() => CurrentIndex--;

    private ParseResult<Expression> ErrorExpression(string error, Position position)
    {
        // todo - log the error
        return new ParseResult<Expression>(new ErrorExpression(position, error), true);
    }

    private ParseResult<Statement> ErrorStatement(string error, Position position)
    {
        // todo - log the error
        return new ParseResult<Statement>(new ErrorStatement(position, error), true);
    }

    private ParseResult<Expression> InvalidTokenErrorExpression(string error, AcceptResult result)
    {
        var token = ErrorToken(result);
        return ErrorExpression($"{error}: {token}", token.Position);
    }

    private ParseResult<Statement> InvalidTokenErrorStatement(string error, AcceptResult result)
    {
        var token = ErrorToken(result);
        return ErrorStatement($"{error}: {token}", token.Position);
    }

    public ParseResult<Statement> Parse(string fileName)
    {
        var lexer = new Lexer();

        using var reader = new StreamReader(fileName);
        string? line;
        while ((line = reader.ReadLine()) != null)
            lexer.Lex(line);
        lexer.EndOfFile();

        Tokens = lexer.Tokens;

        foreach (var token in lexer.Tokens)
        {
            Console.Write(token);
            if (token.Type == TokenType.EOL || token.Type == TokenType.EOF)
                Console.WriteLine();
            else
                Console.Write(" | ");
        }
        Console.WriteLine();

        var file = ParseFileStatement(fileName);

        file.Result.Print(0);

        return file;
    }

    private AcceptResult Accept(params TokenType[] tokens)
    {
        var result = new AcceptResult(CurrentIndex);
        foreach (var token in tokens)
        {
            var current = Next();
            if (current.Type != token)
            {
                CurrentIndex = result.StartingIndex;
                result.Success = false;
                return result;
            }
            result.Count++;
        }
        return result;
    }

    private Token GetToken(AcceptResult result, int index = 0) => Tokens[result.StartingIndex + index];

    private Token ErrorToken(AcceptResult result) => Tokens[result.StartingIndex + result.Count];

    private ParseResult<Expression> ParseAssignment(Expression left)
    {
        var op = Next().Type.ToAssignmentOperator();
        var right = ParseExpression();
        if (right.Error)
            return right;
        return new ParseResult<Expression>(new Assignment(left.Position, op, left, right.Result));
    }

    private ParseResult<Expression> ParseBinaryOperatorRightSide(int leftPrecedence, Expression left)
    {
        while (true)
        {
            int tokenPrecedence = Peek.Precedence();

            // If this is a binary operator that binds at least as tightly as the
            // current operator then consume it, otherwise we're done.
            if (tokenPrecedence < leftPrecedence)
                return new ParseResult<Expression>(left);

            var op = Next();
            var right = ParsePrimaryExpression();
            if (right.Error)
                return right;

            // If the binary operator binds less tightly with the right than the operator
            // after the right, let the pending operator take the right as its left.
            if (tokenPrecedence < Peek.Precedence())
            {
                right = ParseBinaryOperatorRightSide(tokenPrecedence + 1, right.Result);
                if (right.Error)
                    return right;
            }

            // Merge left and right.
            left = new BinaryOperator(left.Position, op.Type.ToOperator(), left, right.Result);
        }
    }

    private ParseResult<Statement> ParseBreak()
    {
        var res = Accept(TokenType.Break, TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in break", res);
        return new ParseResult<Statement>(new Break(GetToken(res).Position));
    }

    private ParseResult<Statement> ParseClass(List<Modifier> modifiers)
    {
        var res = Accept(TokenType.Class, TokenType.Literal);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        var classResult = new Class(GetToken(res, 1).Position, modifiers, GetToken(res, 1).Value);

        if (Accept(TokenType.From).Success)
        {
            var parentIdent = ParseIdentifier();
            if (!parentIdent.Error && parentIdent.Result is Identifier pId)
                classResult.Parent = pId;
        }

        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        while (Peek.Type != TokenType.Dedent)
            classResult.Statements.Add(ParseClassStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement($"Invalid token in method {classResult.Name}", res);

        return new ParseResult<Statement>(classResult);
    }

    private ParseResult<Statement> ParseClassStatement()
    {
        var startToken = Peek;
        int size = 0;
        while (Peek.Type == TokenType.EOL)
        {
            Next();
            size++;
        }
        if (size > 0)
            return new ParseResult<Statement>(new Space(startToken.Position, size));

        if (Peek.Type == TokenType.Comment)
            return ParseComment();

        var modifiers = new List<Modifier>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToModifier());

        if (Peek.Type == TokenType.Class)
            return ParseClass(modifiers);

        ParseResult<Expression>? type = null;
        if (Peek.Type != TokenType.EOL)
        {
            type = ParseType();
            if (type.Error)
                return ErrorStatement("Invalid type", type.Result.Position);
        }
        else
            Next();

        var nameToken = Next();
        if (nameToken.Type != TokenType.Literal)
            return ErrorStatement("Invalid token in class: " + nameToken, nameToken.Position);

        if (Peek.Type == TokenType.EOL && type?.Result is Identifier typeIdent)
        {
            Next();
            return new ParseResult<Statement>(new Property(nameToken.Position, modifiers, typeIdent, nameToken.Value));
        }

        var res = Accept(TokenType.LeftParenthesis);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        var methodDef = new MethodDefinition(nameToken.Position, modifiers, nameToken.Value) { ReturnType = type?.Result };

        while (Peek.Type != TokenType.RightParenthesis)
        {
            var paramType = ParseType();
            if (Peek.Type != TokenType.Literal)
            {
                var errorToken = Next();
                return ErrorStatement($"Invalid token in parameters: {errorToken}", errorToken.Position);
            }
            if (!paramType.Error)
                methodDef.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, Next().Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis, TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        while (Peek.Type != TokenType.Dedent)
            methodDef.Statements.Add(ParseMethodStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement($"Invalid token in method {methodDef.Name}", res);

        return new ParseResult<Statement>(methodDef);
    }

    private ParseResult<Statement> ParseComment()
    {
        var comment = new Comment(Peek.Position, Peek.Value);
        Next();

        if (comment.Value.StartsWith(";"))
        {
            comment.Value = comment.Value[1..];
            comment.IsDocumentation = true;
        }

        var commentResult = new ParseResult<Statement>(comment);

        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in comment", res);

        return commentResult;
    }

    private ParseResult<Statement> ParseContinue()
    {
        var res = Accept(TokenType.Continue, TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in continue", res);
        return new ParseResult<Statement>(new Continue(GetToken(res).Position));
    }

    private ParseResult<Statement> ParseEnum(List<Modifier> modifiers)
    {
        Next();
        return ErrorStatement("Enum parsing not supported yet", new Position());
    }

    private ParseResult<Expression> ParseExpression()
    {
        var left = ParsePrimaryExpression();
        if (left.Error)
            return left;
        if (Peek.Type.IsAssignment())
            return ParseAssignment(left.Result);
        return ParseBinaryOperatorRightSide(0, left.Result);
    }

    private ParseResult<Statement> ParseExpressionStatement()
    {
        var token = Peek;
        var exprStatement = new ExpressionStatement(token.Position, ParseExpression().Result);

        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in expression statement", res);

        return new ParseResult<Statement>(exprStatement);
    }

    private ParseResult<Statement> ParseFileModifiableStatement()
    {
        var modifiers = new List<Modifier>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToModifier());

        return Peek.Type switch
        {
            TokenType.Class => ParseClass(modifiers),
            TokenType.Enum => ParseEnum(modifiers),
            TokenType.Interface => ParseInterface(modifiers),
            TokenType.Struct => ParseStruct(modifiers),
            _ => ErrorStatement($"Invalid token: {Peek}", Peek.Position),
        };
    }

    private ParseResult<Statement> ParseFileStatement(string fileName)
    {
        var file = new Core.Ast.File(Path.GetFileName(fileName));
        bool error = false;
        while (CurrentIndex < Tokens.Count)
        {
            switch (Peek.Type)
            {
                case TokenType.Class:
                case TokenType.Enum:
                case TokenType.Interface:
                case TokenType.Struct:
                    file.Statements.Add(ParseFileModifiableStatement().Result);
                    break;
                case TokenType.Comment:
                    file.Statements.Add(ParseComment().Result);
                    break;
                case TokenType.EOF:
                    // call next so that we break out of the while loop
                    Next();
                    break;
                case TokenType.EOL:
                    file.Statements.Add(ParseSpace().Result);
                    break;
                case TokenType.Namespace:
                    file.Statements.Add(ParseNamespace().Result);
                    break;
                case TokenType.Use:
                    file.Statements.Add(ParseImport().Result);
                    break;
                default:
                    if (Peek.Type.IsModifier())
                        file.Statements.Add(ParseFileModifiableStatement().Result);
                    else
                    {
                        file.Statements.Add(ErrorStatement($"Invalid token: {Peek}", Peek.Position).Result);
                        Next();
                        error = true;
                    }
                    break;
            }
        }

        return new ParseResult<Statement>(file, error);
    }

    private ParseResult<Expression> ParseIdentifier()
    {
        var start = Peek.Position;

        var firstPartResult = ParseIdentifierPart();

        if (!firstPartResult.Error && firstPartResult.Result is IdentifierPart firstPart)
        {
            var identifier = new Identifier(start, firstPart);

            var res = Accept(TokenType.Dot);
            while (res.Success)
            {
                var nextPartResult = ParseIdentifierPart();
                if (!nextPartResult.Error && nextPartResult.Result is IdentifierPart nextPart)
                {
                    identifier.Parts.Add(nextPart);
                    res = Accept(TokenType.Dot);
                }
                else
                    return nextPartResult;
            }

            return new ParseResult<Expression>(identifier);
        }
        return firstPartResult;
    }

    private ParseResult<Expression> ParseIdentifierPart()
    {
        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in identifier", res);
        var identifierPart = new IdentifierPart(GetToken(res).Position, GetToken(res).Value);

        // generics
        if (Accept(TokenType.LeftCurly).Success)
        {
            var typeIdentRes = ParseIdentifier();
            if (!typeIdentRes.Error && typeIdentRes.Result is Identifier typeIdentifier)
            {
                identifierPart.Types = new();
                identifierPart.Types.Add(typeIdentifier);

                // check for multiple types
                while (Accept(TokenType.Comma).Success)
                {
                    typeIdentRes = ParseIdentifier();
                    if (!typeIdentRes.Error && typeIdentRes.Result is Identifier nextTypeIdent)
                        identifierPart.Types.Add(nextTypeIdent);
                    else
                        return typeIdentRes;
                }

                res = Accept(TokenType.RightCurly);
                if (res.Failure)
                    return InvalidTokenErrorExpression("Invalid token in identifier", res);
            }
            else
                return typeIdentRes;
        }

        return new ParseResult<Expression>(identifierPart);
    }

    private ParseResult<Statement> ParseIf(bool inElseIf = false)
    {
        // accept if
        var start = Next();
        var condition = ParseExpression();
        if (condition.Error && condition.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);

        var res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in if", res);

        var ifStatement = new If(start.Position, condition.Result);

        while (Peek.Type != TokenType.Dedent)
        {
            var statement = ParseMethodStatement();
            if (statement.Error)
                return statement;
            ifStatement.Statements.Add(statement.Result);
        }

        res = Accept(TokenType.Dedent, TokenType.Else);
        if (!res.Failure)
        {
            ifStatement.Else = new();

            if (Peek.Type == TokenType.If)
            {
                var elseIf = ParseIf(true);
                if (elseIf.Error)
                    return elseIf;
                ifStatement.Else.Add(elseIf.Result);
            }
            else
            {
                res = Accept(TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in if", res);

                while (Peek.Type != TokenType.Dedent)
                {
                    var statement = ParseMethodStatement();
                    if (statement.Error)
                        return statement;
                    ifStatement.Else.Add(statement.Result);
                }
            }
        }

        if (!inElseIf)
        {
            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in if", res);
        }

        return new ParseResult<Statement>(ifStatement);
    }

    private ParseResult<Statement> ParseImport()
    {
        var useToken = Next();
        var identifier = ParseIdentifier();
        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in use", res);

        if (!identifier.Error && identifier.Result is Identifier ident)
            return new ParseResult<Statement>(new Import(useToken.Position, ident));

        return ErrorStatement("Couldn't parse use", useToken.Position);
    }

    private ParseResult<Statement> ParseInterface(List<Modifier> modifiers)
    {
        Next();
        return ErrorStatement("Interface parsing not supported yet", new Position());
    }

    private ParseResult<Expression> ParseMethodCall(Expression target)
    {
        var methodCall = new MethodCall(target.Position, target);
        var res = Accept(TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in method call", res);

        if (Peek.Type != TokenType.RightParenthesis)
        {
            var paramExpr = ParseExpression();
            if (paramExpr.Error)
                return paramExpr;
            methodCall.Parameters.Add(paramExpr.Result);
            while (Accept(TokenType.Comma).Success)
            {
                paramExpr = ParseExpression();
                if (paramExpr.Error)
                    return paramExpr;
                methodCall.Parameters.Add(paramExpr.Result);
            }
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in method call", res);

        return new ParseResult<Expression>(methodCall);
    }

    private ParseResult<Statement> ParseMethodStatement() =>
        Peek.Type switch
        {
            TokenType.Break => ParseBreak(),
            TokenType.Comment => ParseComment(),
            TokenType.Continue => ParseContinue(),
            TokenType.EOL => ParseSpace(),
            TokenType.If => ParseIf(),
            _ => ParseExpressionStatement(),
        };

    private ParseResult<Statement> ParseNamespace()
    {
        var nsToken = Next();
        var identifier = ParseIdentifier();
        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in namespace", res);

        if (!identifier.Error && identifier.Result is Identifier ident)
            return new ParseResult<Statement>(new Namespace(nsToken.Position, ident));

        return ErrorStatement("Couldn't parse namespace", nsToken.Position);
    }

    private ParseResult<Expression> ParseParenthesizedExpression()
    {
        // Discard the opening parenthesis
        Next();
        var expression = ParseExpression();
        var res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in parenthesized expression", res);
        return expression;
    }

    private ParseResult<Expression> ParsePrimaryExpression()
    {
        ParseResult<Expression> leftResult;
        if (Peek.Type == TokenType.LeftParenthesis)
            leftResult = ParseParenthesizedExpression();
        else if (Peek.Type == TokenType.Literal)
            leftResult = ParseIdentifier();
        else if (Peek.Type == TokenType.StringStart)
            leftResult = ParseString();
        else
        {
            var token = Next();
            leftResult = token.Type switch
            {
                TokenType.CharacterLiteral => new ParseResult<Expression>(new Character(token.Position, token.Value)),
                TokenType.False => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.False)),
                TokenType.Null => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.Null)),
                TokenType.NumberLiteral => new ParseResult<Expression>(new Number(token.Position, token.Value)),
                TokenType.This => new ParseResult<Expression>(new CurrentObjectInstance(token.Position)),
                TokenType.True => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.True)),
                _ => ErrorExpression("Invalid token in expression: " + token, token.Position)
            };
        }
        if (leftResult.Error)
            return leftResult;

        if (Peek.Type == TokenType.LeftParenthesis)
            leftResult = ParseMethodCall(leftResult.Result);

        return leftResult;
    }

    private ParseResult<Statement> ParseSpace()
    {
        var position = Peek.Position;
        int size = 0;
        while (Peek.Type == TokenType.EOL)
        {
            Next();
            size++;
        }
        return new ParseResult<Statement>(new Space(position, size));
    }

    private ParseResult<Expression> ParseString()
    {
        var stringResult = new Core.Ast.String(Peek.Position);
        AcceptResult res;

        do
        {
            var currentLine = new List<Expression>();
            stringResult.Lines.Add(currentLine);

            res = Accept(TokenType.StringStart);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in string", res);

            while (Peek.Type != TokenType.StringEnd)
            {
                if (Peek.Type == TokenType.StringLiteral)
                {
                    currentLine.Add(new StringLiteral(Peek.Position, Peek.Value));
                    Next();
                }
                else if (Accept(TokenType.LeftCurly).Success)
                {
                    var nestedExpression = ParseExpression();
                    if (nestedExpression.Error)
                        return nestedExpression;
                    currentLine.Add(nestedExpression.Result);
                    res = Accept(TokenType.RightCurly);
                    if (res.Failure)
                        return InvalidTokenErrorExpression("Invalid token in string", res);
                }
                else
                    return ErrorExpression("Invalid token in string parsing: " + Peek, Peek.Position);
            }
            res = Accept(TokenType.StringEnd);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in string", res);
        } while (Accept(TokenType.DoubleDot).Success);

        return new ParseResult<Expression>(stringResult);
    }

    private ParseResult<Statement> ParseStruct(List<Modifier> modifiers)
    {
        Next();
        return ErrorStatement("Struct parsing not supported yet", new Position());
    }

    private ParseResult<Expression> ParseType()
    {
        switch (Peek.Type)
        {
            case TokenType.String:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "String"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.Character:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Char"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.Boolean:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Boolean"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.I8:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "SByte"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.I16:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Int16"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.I32:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Int32"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.I64:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Int64"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.U8:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Byte"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.U16:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "UInt16"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.U32:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "UInt32"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.U64:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "UInt64"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.F32:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Single"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.F64:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Double"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
            case TokenType.Decimal:
                {
                    var type = new Identifier(Peek.Position, new IdentifierPart(Peek.Position, "System"));
                    type.Parts.Add(new IdentifierPart(Peek.Position, "Decimal"));
                    Next();
                    return new ParseResult<Expression>(type);
                }
        }

        // TODO - split this off properly so things that just take dotted text like namespaces
        // use a separate parse from the more general one that supports generics.
        // During those changes also make it so this continues the parsing if a match was
        // found above so you can have string.Join resolve to System.String.Join for example
        if (Peek.Type == TokenType.Literal)
        {
            var typeResult = ParseIdentifier();
            if (typeResult.Error)
                return typeResult;

            var type = typeResult.Result;
            while (Accept(TokenType.LeftBracket, TokenType.RightBracket).Success)
                type = new Core.Ast.Array(type.Position, type);

            return new ParseResult<Expression>(type);
        }

        var token = Next();
        return ErrorExpression("Invalid token in type parsing: " + token, token.Position);
    }
}