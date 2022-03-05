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
            Console.WriteLine(token);
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
                result.Failure = true;
                return result;
            }
            result.Count++;
        }
        return result;
    }

    private Token GetToken(AcceptResult result, int index = 0) => Tokens[result.StartingIndex + index];

    private Token ErrorToken(AcceptResult result) => Tokens[result.StartingIndex + result.Count];

    private ParseResult<Statement> ParseBreak()
    {
        var res = Accept(TokenType.Break, TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in break", res);
        return new ParseResult<Statement>(new Break(GetToken(res).Position));
    }

    private ParseResult<Statement> ParseClass(List<Core.Token> modifiers)
    {
        var res = Accept(TokenType.Class, TokenType.Literal);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        var classResult = new Class(GetToken(res, 1).Position, modifiers, GetToken(res, 1).Value);

        res = Accept(TokenType.EOL, TokenType.Indent);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        while (Peek.Type != TokenType.Dedent)
            classResult.Statements.Add(ParseClassStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement($"Invalid token in function {classResult.Name}", res);

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

        var modifiers = new List<Core.Token>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToCoreToken());

        if (Peek.Type == TokenType.Class)
            return ParseClass(modifiers);

        var type = ParseType();
        if (type.Error)
            return ErrorStatement("Invalid type", type.Result.Position);

        var nameToken = Next();
        if (nameToken.Type != TokenType.Literal)
            return ErrorStatement("Invalid token in class: " + nameToken, nameToken.Position);

        if (Peek.Type == TokenType.EOL && type.Result is Identifier typeIdent)
        {
            Next();
            return new ParseResult<Statement>(new Property(nameToken.Position, modifiers, typeIdent, nameToken.Value));
        }

        var res = Accept(TokenType.LeftParenthesis);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        var functionDef = new FunctionDefinition(nameToken.Position, modifiers, type.Result, nameToken.Value);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            var paramType = ParseType();
            if (Peek.Type != TokenType.Literal)
            {
                var errorToken = Next();
                return ErrorStatement($"Invalid token in parameters: {errorToken}", errorToken.Position);
            }
            if (!paramType.Error && paramType.Result is Identifier paramIdent)
                functionDef.Parameters.Add(new Parameter(paramIdent.Position, paramIdent, Next().Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis, TokenType.EOL, TokenType.Indent);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);


        while (Peek.Type != TokenType.Dedent)
            functionDef.Statements.Add(ParseFunctionStatement().Result);

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement($"Invalid token in function {functionDef.Name}", res);

        return new ParseResult<Statement>(functionDef);
    }

    private ParseResult<Statement> ParseComment()
    {
        var commentResult = new ParseResult<Statement>(new Comment(Peek.Position, Peek.Value));
        Next();

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

    private ParseResult<Statement> ParseEnum(List<Core.Token> modifiers)
    {
        Next();
        return ErrorStatement("Enum parsing not supported yet", new Position());
    }

    private ParseResult<Statement> ParseFileModifiableStatement()
    {
        var modifiers = new List<Core.Token>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToCoreToken());

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

    private ParseResult<Statement> ParseFunctionStatement() =>
        Peek.Type switch
        {
            TokenType.Break => ParseBreak(),
            TokenType.Continue => ParseContinue(),
            TokenType.EOL => ParseSpace(),
            _ => ErrorStatement($"Invalid token: {Peek}", Peek.Position),
        };

    private ParseResult<Expression> ParseIdentifier()
    {
        var identifier = new Identifier(Peek.Position, Peek.Value);
        Next();

        var res = Accept(TokenType.Dot, TokenType.Literal);
        while (!res.Failure)
        {
            identifier.Parts.Add(new IdentifierPart(GetToken(res, 1).Value));
            res = Accept(TokenType.Dot, TokenType.Literal);
        }

        return new ParseResult<Expression>(identifier);
    }

    private ParseResult<Statement> ParseInterface(List<Core.Token> modifiers)
    {
        Next();
        return ErrorStatement("Interface parsing not supported yet", new Position());
    }

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

    private ParseResult<Statement> ParseStruct(List<Core.Token> modifiers)
    {
        Next();
        return ErrorStatement("Struct parsing not supported yet", new Position());
    }

    private ParseResult<Expression> ParseType()
    {
        if (Peek.Type == TokenType.Literal)
            return ParseIdentifier();

        var token = Next();
        return token.Type switch
        {
            TokenType.Void => new ParseResult<Expression>(new Core.Ast.Void(token.Position)),
            _ => ErrorExpression("Invalid token in type parsing: " + token, new Position()),
        };
    }
}