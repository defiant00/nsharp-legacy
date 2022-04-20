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

    private ParseResult<Expression> ParseArray()
    {
        var start = Peek;
        var res = Accept(TokenType.LeftBracket, TokenType.RightBracket);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in array type declaration", res);

        var typeResult = ParseType();
        if (typeResult.Error)
            return typeResult;

        return new ParseResult<Expression>(new Core.Ast.Array(start.Position, typeResult.Result));
    }

    private ParseResult<Statement> ParseAssignment(Expression left)
    {
        var op = Next().Type.ToAssignmentOperator();
        var right = ParseExpression();
        if (right.Error && right.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);
        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in assignment", res);
        return new ParseResult<Statement>(new Assignment(left.Position, op, left, right.Result));
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

        if (Accept(TokenType.Is).Success)
        {
            var interfaceIdent = ParseIdentifier();
            if (!interfaceIdent.Error && interfaceIdent.Result is Identifier iId)
                classResult.Interfaces.Add(iId);

            while (Accept(TokenType.Comma).Success)
            {
                interfaceIdent = ParseIdentifier();
                if (!interfaceIdent.Error && interfaceIdent.Result is Identifier id)
                    classResult.Interfaces.Add(id);
            }
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

        switch (Peek.Type)
        {
            case TokenType.Class:
                return ParseClass(modifiers);
            case TokenType.Value:
                return ParseConstant(modifiers);
            case TokenType.Function:
                return ParseMethodOrProperty(modifiers);
            case TokenType.Variable:
                return ParseVariable(modifiers);
        }

        var token = Next();
        return ErrorStatement($"Invalid token in class: {token}", token.Position);
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

    private ParseResult<Statement> ParseConstant(List<Modifier> modifiers)
    {
        // [modifiers] val [name] [type]
        // [modifiers] val [name] [type] = [expr]

        var start = Next();     // accept val

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in constant", res);

        string name = GetToken(res).Value;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);

        var constant = new Constant(start.Position, modifiers, name, typeResult.Result);

        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp.Value, exp.Position);
            constant.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in constant", res);

        return new ParseResult<Statement>(constant);
    }

    private ParseResult<Statement> ParseConstructor(List<Modifier> modifiers, Token start)
    {
        // constructor
        // [modifiers] fn new([params]) is [statement]
        // [modifiers] fn new([params])
        //     [statements]

        var res = Accept(TokenType.New, TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        var ctorDef = new ConstructorDefinition(start.Position, modifiers);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (!paramType.Error)
                ctorDef.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        // [modifiers] fn new([params]) is [statement]
        if (Accept(TokenType.Is).Success)
        {
            var stmtResult = ParseMethodStatement();
            if (stmtResult.Error)
                return stmtResult;
            ctorDef.Statements.Add(stmtResult.Result);
            return new ParseResult<Statement>(ctorDef);
        }

        // [modifiers] fn new([params])
        //     [statements]
        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        while (Peek.Type != TokenType.Dedent)
            ctorDef.Statements.Add(ParseMethodStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        return new ParseResult<Statement>(ctorDef);
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
        return ParseBinaryOperatorRightSide(0, left.Result);
    }

    private ParseResult<Statement> ParseExpressionStatement()
    {
        var left = ParsePrimaryExpression();
        if (left.Error && left.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);

        if (Peek.Type.IsAssignment())
            return ParseAssignment(left.Result);

        var expr = ParseBinaryOperatorRightSide(0, left.Result);
        if (expr.Error && expr.Result is ErrorExpression exp)
            return ErrorStatement(exp.Value, exp.Position);

        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in expression statement", res);

        return new ParseResult<Statement>(new ExpressionStatement(expr.Result.Position, expr.Result));
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

    // Returns the error parse result, or null on success.
    private ParseResult<Statement>? ParseGetSet(Property propDef)
    {
        AcceptResult res;
        if (Peek.Type == TokenType.Get)
        {
            if (Accept(TokenType.Get, TokenType.Is).Success)
            {
                // get is [expr]

                var exprResult = ParseExpression();
                if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                    return ErrorStatement(exp.Value, exp.Position);
                propDef.GetStatements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);
            }
            else
            {
                // get
                //     [statements]

                res = Accept(TokenType.Get, TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);

                while (Peek.Type != TokenType.Dedent)
                    propDef.GetStatements.Add(ParseMethodStatement().Result);

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);
            }
        }
        else
        {
            res = Accept(TokenType.Set, TokenType.LeftParenthesis, TokenType.Literal, TokenType.RightParenthesis);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in set", res);
            propDef.SetParameterName = GetToken(res, 2).Value;

            if (Accept(TokenType.Is).Success)
            {
                // set(v) is [statement]

                var stmtResult = ParseMethodStatement();
                if (stmtResult.Error)
                    return stmtResult;
                propDef.SetStatements.Add(stmtResult.Result);

                // res = Accept(TokenType.EOL);
                // if (res.Failure)
                //     return InvalidTokenErrorStatement("Invalid token in set", res);
            }
            else
            {
                // set(v)
                //     [statements]

                res = Accept(TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in set", res);

                while (Peek.Type != TokenType.Dedent)
                    propDef.SetStatements.Add(ParseMethodStatement().Result);

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in set", res);
            }
        }
        return null;
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

    private ParseResult<Statement> ParseMethod(List<Modifier> modifiers, Token nameToken)
    {
        // method
        // [modifiers] fn [name]([params]) is [statement]
        // [modifiers] fn [name]([params])
        //     [statements]
        // [modifiers] fn [name]([params]) [type] is [expr]
        // [modifiers] fn [name]([params]) [type]
        //     [statements]

        var res = Accept(TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        var methodDef = new MethodDefinition(nameToken.Position, modifiers, nameToken.Value);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (!paramType.Error)
                methodDef.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        // [modifiers] fn [name]([params]) is [statement]
        if (Accept(TokenType.Is).Success)
        {
            var stmtResult = ParseMethodStatement();
            if (stmtResult.Error)
                return stmtResult;
            methodDef.Statements.Add(stmtResult.Result);
            return new ParseResult<Statement>(methodDef);
        }

        // return type
        if (Peek.Type != TokenType.EOL)
        {
            var typeResult = ParseType();
            if (typeResult.Error && typeResult.Result is ErrorExpression ex)
                return ErrorStatement(ex.Value, ex.Position);
            methodDef.ReturnType = typeResult.Result;

            // [modifiers] fn [name]([params]) [type] is [expr]
            if (Accept(TokenType.Is).Success)
            {
                var exprResult = ParseExpression();
                if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                    return ErrorStatement(exp.Value, exp.Position);
                methodDef.Statements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in method", res);
                return new ParseResult<Statement>(methodDef);
            }
        }

        // [modifiers] fn [name]([params])
        //     [statements]
        // [modifiers] fn [name]([params]) [type]
        //     [statements]
        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        while (Peek.Type != TokenType.Dedent)
            methodDef.Statements.Add(ParseMethodStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        return new ParseResult<Statement>(methodDef);
    }

    private ParseResult<Statement> ParseMethodOrProperty(List<Modifier> modifiers)
    {
        var start = Next();     // accept fn

        if (Peek.Type == TokenType.New)
            return ParseConstructor(modifiers, start);

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method or property", res);

        var nameToken = GetToken(res);

        if (Peek.Type == TokenType.LeftParenthesis)
            return ParseMethod(modifiers, nameToken);

        return ParseProperty(modifiers, nameToken);
    }

    private ParseResult<Statement> ParseMethodStatement() =>
        Peek.Type switch
        {
            TokenType.Break => ParseBreak(),
            TokenType.Comment => ParseComment(),
            TokenType.Continue => ParseContinue(),
            TokenType.EOL => ParseSpace(),
            TokenType.If => ParseIf(),
            TokenType.Return => ParseReturn(),
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

    private ParseResult<Statement> ParseProperty(List<Modifier> modifiers, Token nameToken)
    {
        // property
        // [modifiers] fn [name] [type] [= [expr]]
        // [modifiers] fn [name] [type] [get and/or set] [= [expr]]
        // [modifiers] fn [name] [type] is [expr]
        // [modifiers] fn [name] [type]
        //     [statements]
        // [modifiers] fn [name] [type] [= [expr]]
        //     get is [expr]
        //     set is [statement]
        // [modifiers] fn [name] [type] [= [expr]]
        //     get
        //         [statements]
        //     set(v)
        //         [statements]

        AcceptResult res;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);

        var propDef = new Property(nameToken.Position, modifiers, nameToken.Value, typeResult.Result);

        // [modifiers] fn [name] [type] [get and/or set]
        if (Peek.Type == TokenType.Get || Peek.Type == TokenType.Set)
        {
            var getSet = Next();
            var other = TokenType.Set;
            if (getSet.Type == TokenType.Get)
                propDef.Set = false;
            else
            {
                propDef.Get = false;
                other = TokenType.Get;
            }

            res = Accept(TokenType.Comma, other);
            if (res.Success)
            {
                if (GetToken(res, 1).Type == TokenType.Get)
                    propDef.Get = true;
                else
                    propDef.Set = true;
            }
        }
        else if (Accept(TokenType.Is).Success)
        {
            // [is [expr]]

            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp.Value, exp.Position);
            propDef.GetStatements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
            res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in property", res);
            return new ParseResult<Statement>(propDef);
        }

        // [= [expr]]
        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp.Value, exp.Position);
            propDef.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in property", res);

        if (Accept(TokenType.Indent).Success)
        {
            if (Peek.Type == TokenType.Get || Peek.Type == TokenType.Set)
            {
                // [modifiers] fn [name] [type]
                //     get is [expr]
                //     set(v) is [statement]
                // [modifiers] fn [name] [type]
                //     get
                //         [statements]
                //     set(v)
                //         [statements]

                var other = Peek.Type == TokenType.Get ? TokenType.Set : TokenType.Get;

                var getSetResult = ParseGetSet(propDef);
                if (getSetResult != null)
                    return getSetResult;

                if (Peek.Type == other)
                {
                    getSetResult = ParseGetSet(propDef);
                    if (getSetResult != null)
                        return getSetResult;
                }

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in property", res);
            }
            else
            {
                // [modifiers] fn [name] [type]
                //     [statements]

                while (Peek.Type != TokenType.Dedent)
                    propDef.GetStatements.Add(ParseMethodStatement().Result);

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in property", res);
            }
        }

        return new ParseResult<Statement>(propDef);
    }

    private ParseResult<Statement> ParseReturn()
    {
        var start = Next();     // accept return
        var ret = new Return(start.Position);
        if (Peek.Type != TokenType.EOL)
        {
            var expr = ParseExpression();
            if (expr.Error && expr.Result is ErrorExpression ex)
                return ErrorStatement(ex.Value, ex.Position);
            ret.Value = expr.Result;
        }
        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in return", res);
        return new ParseResult<Statement>(ret);
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
            case TokenType.LeftBracket:
                return ParseArray();
            case TokenType.Literal:
                return ParseIdentifier();
        }

        // TODO - split this off properly so things that just take dotted text like namespaces
        // use a separate parse from the more general one that supports generics.
        // During those changes also make it so this continues the parsing if a match was
        // found above so you can have string.Join resolve to System.String.Join for example

        var token = Next();
        return ErrorExpression("Invalid token in type parsing: " + token, token.Position);
    }

    private ParseResult<Statement> ParseVariable(List<Modifier> modifiers)
    {
        // field
        // [modifiers] var [name] [type]
        // [modifiers] var [name] [type] = [expr]

        var start = Next();     // accept var

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in variable", res);

        string name = GetToken(res).Value;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex.Value, ex.Position);

        var variable = new Variable(start.Position, modifiers, name, typeResult.Result);

        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp.Value, exp.Position);
            variable.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in variable", res);

        return new ParseResult<Statement>(variable);
    }
}