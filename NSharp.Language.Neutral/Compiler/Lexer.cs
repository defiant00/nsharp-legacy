using NSharp.Core;

namespace NSharp.Language.Neutral.Compiler;

public class Lexer
{
    const char EOF = '\0';
    const string OPERATOR_CHARACTERS = "()[]<>!=+-*/%,.:&|^~";

    private delegate StateFunction? StateFunction();

    private string Input { get; set; }
    private int InputStart { get; set; }
    private int InputPosition { get; set; }
    private StateFunction? State { get; set; }
    private Stack<int> IndentationLevels { get; set; }
    private Stack<int> Widths { get; set; }
    private bool InStatement { get; set; }

    public List<Token> Tokens { get; set; }

    public Lexer(string input)
    {
        Input = input;
        InputStart = 0;
        InputPosition = 0;
        State = LexIndent;
        IndentationLevels = new Stack<int>();
        IndentationLevels.Push(0);
        Widths = new Stack<int>();
        InStatement = false;
        Tokens = new List<Token>();

        while (State != null)
            State = State();
    }

    // Literals can start with a letter, _, or ` to indicate an identifier that matches a keyword
    private bool IsValidLiteralStart(char c) => char.IsLetter(c) || c == '_' || c == '`';

    // Literals can contain letters, numbers, and _
    private bool IsValidLiteral(char c) => char.IsLetter(c) || c == '_' || char.IsDigit(c);

    private Position CurrentPosition
    {
        get
        {
            var pos = new Position { Line = 1, Column = 1 };
            bool currentLine = true;
            for (int i = InputStart - 1; i >= 0; i--)
            {
                if (Input[i] == '\n')
                {
                    pos.Line++;
                    currentLine = false;
                }
                else if (currentLine)
                    pos.Column++;
            }

            return pos;
        }
    }

    private string CurrentValue => Input[InputStart..InputPosition];

    private char Next()
    {
        if (InputPosition >= Input.Length)
        {
            Widths.Push(0);
            return EOF;
        }
        Widths.Push(1);
        return Input[InputPosition++];
    }

    private char Peek
    {
        get
        {
            char c = Next();
            Backup();
            return c;
        }
    }

    private void Backup() => InputPosition -= Widths.Pop();

    private void Discard(int increment = 0)
    {
        InputPosition += increment;
        InputStart = InputPosition;
        Widths.Clear();
    }

    private bool Accept(string valid)
    {
        if (valid.IndexOf(Next()) >= 0)
            return true;
        Backup();
        return false;
    }

    private void AcceptRun(string valid)
    {
        while (valid.IndexOf(Next()) >= 0) { }
        Backup();
    }

    private StateFunction? Error(string error)
    {
        Tokens.Add(new Token { Type = TokenType.Error, Position = CurrentPosition, Value = error });
        return null;
    }

    private void Emit(TokenType token)
    {
        Tokens.Add(new Token { Type = token, Position = CurrentPosition, Value = CurrentValue });
        InputStart = InputPosition;
        Widths.Clear();
    }

    private void EmitIndent(int indent)
    {
        int currentIndent = IndentationLevels.Peek();
        if (indent > currentIndent)
        {
            Emit(TokenType.Indent);
            IndentationLevels.Push(indent);
        }
        else
        {
            while (IndentationLevels.Count > 0 && indent < currentIndent)
            {
                Emit(TokenType.Dedent);
                IndentationLevels.Pop();
                currentIndent = IndentationLevels.Peek();
            }
            if (IndentationLevels.Count == 0 || currentIndent != indent)
                Error("Mismatched indentation level encountered");
        }
    }

    private StateFunction? LexIndent()
    {
        InStatement = false;
        int indent = 0;
        while (true)
        {
            switch (Next())
            {
                case EOF:
                    Discard();
                    EmitIndent(0);
                    Emit(TokenType.EOF);
                    return null;
                case '\r':
                case '\n':
                    indent = 0;
                    Discard();
                    break;
                case ' ':
                    indent++;
                    break;
                case '\t':
                    indent += 4;
                    break;
                default:
                    Backup();
                    Discard();
                    EmitIndent(indent);
                    return LexStatement;
            }
        }
    }

    private StateFunction? LexStatement()
    {
        while (true)
        {
            char peekVal = Peek;
            switch (peekVal)
            {
                case EOF:
                    if (InStatement)
                        Emit(TokenType.EOL);
                    EmitIndent(0);
                    Emit(TokenType.EOF);
                    return null;
                case ' ':
                case '\t':
                case '\r':
                    Discard(1);
                    break;
                case '\n':
                    Next();
                    if (InStatement)
                        Emit(TokenType.EOL);
                    return LexIndent;
                case ';':
                    return LexComment;
                case '\'':
                    InStatement = true;
                    return LexCharacter;
                case '"':
                    InStatement = true;
                    return LexString;
                default:
                    InStatement = true;
                    if (IsValidLiteralStart(peekVal))
                        return LexLiteral;
                    else if (char.IsDigit(peekVal))
                        return LexNumber;
                    else if (Accept(OPERATOR_CHARACTERS))
                        return LexOperator;
                    return Error($"Invalid character '{peekVal}' encountered.");

            }
        }
    }

    private StateFunction? LexCharacter()
    {
        // discard the '
        Discard(1);
        char c = Next();
        if (c == EOF || c == '\r' || c == '\n')
            return Error("Unclosed '");
        else if (c == '\\')
        {
            c = Next();
            if (c == EOF || c == '\r' || c == '\n')
                return Error("Unclosed '");
        }
        if (Peek != '\'')
            return Error("Unclosed '");
        Emit(TokenType.Character);
        Discard(1);
        return LexStatement;
    }

    private StateFunction? LexComment()
    {
        // discard the ;
        Discard(1);
        for (char c = Peek; c != EOF && c != '\r' && c != '\n'; c = Peek)
            Next();
        Emit(TokenType.Comment);
        Emit(TokenType.EOL);
        return LexIndent;
    }

    private StateFunction? LexLiteral()
    {
        // accept the starting literal character
        Next();
        for (char c = Peek; IsValidLiteral(c); c = Peek)
            Next();
        Emit(Helpers.ToTokenType(CurrentValue));
        return LexStatement;
    }

    private StateFunction? LexNumber()
    {
        for (char c = Peek; char.IsDigit(c); c = Peek)
            Next();
        if (Accept("."))
        {
            bool foundDigitsAfterDecimal = false;
            for (char c = Peek; char.IsDigit(c); c = Peek)
            {
                Next();
                foundDigitsAfterDecimal = true;
            }
            // Put the decimal back if there are no numbers after it
            if (!foundDigitsAfterDecimal)
                Backup();
        }
        Emit(TokenType.Number);
        return LexStatement;
    }

    private StateFunction? LexOperator()
    {
        AcceptRun(OPERATOR_CHARACTERS);
        int initialPos = InputPosition;
        TokenType tokenType = Helpers.ToTokenType(CurrentValue);
        while (tokenType == TokenType.Literal && InputPosition > InputStart)
        {
            Backup();
            tokenType = Helpers.ToTokenType(CurrentValue);
        }
        if (InputPosition > InputStart)
        {
            Emit(tokenType);
            return LexStatement;
        }
        return Error($"Invalid operator '{Input[InputStart..initialPos]}'");
    }

    private StateFunction? LexString()
    {
        return null;
    }
}