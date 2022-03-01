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
        return new ParseResult<Expression>(new ErrorExpression { Position = position, Value = error }, true);
    }

    private ParseResult<Statement> ErrorStatement(string error, Position position)
    {
        // todo - log the error
        return new ParseResult<Statement>(new ErrorStatement { Position = position, Value = error }, true);
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

        var file = ParseFile(fileName);

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

    private ParseResult<Statement> ParseComment()
    {
        var commentResult = new ParseResult<Statement>(new Comment { Value = Next().Value }, false);

        var res = Accept(TokenType.EOL);
        if (!res.Success)
            return InvalidTokenErrorStatement("Invalid token in comment", res);

        return commentResult;
    }

    private ParseResult<Statement> ParseFile(string fileName)
    {
        var file = new Core.Ast.File { Name = Path.GetFileName(fileName) };
        bool error = false;
        while (CurrentIndex < Tokens.Count)
        {
            switch (Peek.Type)
            {
                case TokenType.Comment:
                    file.Statements.Add(ParseComment().Result);
                    break;
                case TokenType.EOF:
                    // call next so that we break out of the while loop
                    Next();
                    break;
                case TokenType.EOL:
                    Next();
                    file.Statements.Add(new Space { Size = 1 });
                    break;
                case TokenType.Namespace:
                    break;
                default:
                    file.Statements.Add(ErrorStatement($"Invalid token: {Peek}", Peek.Position).Result);
                    Next();
                    error = true;
                    break;
            }
        }

        return new ParseResult<Statement>(file, error);
    }
}