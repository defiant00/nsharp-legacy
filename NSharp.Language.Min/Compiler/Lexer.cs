using NSharp.Core;

namespace NSharp.Language.Min.Compiler;

public class Lexer
{
    const char EOL = '\0';
    const string OPERATOR_CHARACTERS = "()[]<>!=+-*/%,.:&|^~";
    const char LITERAL_INDICATOR = '`';

    private delegate StateFunction? StateFunction();

    private string Line { get; set; }
    private int LineNumber { get; set; }
    private int LineStartIndex { get; set; }
    private int LineCurrentIndex { get; set; }
    private StateFunction? State { get; set; }
    private Stack<int> IndentationLevels { get; set; }

    public List<Token> Tokens { get; set; }

    public Lexer()
    {
        IndentationLevels = new Stack<int>();
        IndentationLevels.Push(0);
        Tokens = new List<Token>();
        Line = string.Empty;
        LineNumber = 0;
    }

    public void Lex(string line)
    {
        LineNumber++;
        Line = line;
        LineStartIndex = 0;
        LineCurrentIndex = 0;
        State = LexIndent;

        while (State != null)
            State = State();
    }

    public void EndOfFile()
    {
        EmitIndent(0);
        Emit(TokenType.EOF);
    }

    // Literals can start with a letter, _, or ` to indicate an identifier that matches a keyword
    private bool IsValidLiteralStart(char c) => char.IsLetter(c) || c == '_' || c == LITERAL_INDICATOR;

    // Literals can contain letters, numbers, and _
    private bool IsValidLiteral(char c) => char.IsLetter(c) || c == '_' || char.IsDigit(c);

    private bool IsLineEnd(char c) => c == EOL || c == '\r' || c == '\n';

    private Position CurrentPosition => new Position { Line = LineNumber, Column = LineStartIndex + 1 };

    private string CurrentValue => Line[LineStartIndex..LineCurrentIndex];

    private char Next()
    {
        char next = Peek;
        LineCurrentIndex++;
        return next;
    }

    private char Peek => (LineCurrentIndex >= Line.Length) ? EOL : Line[LineCurrentIndex];

    private void Backup() => LineCurrentIndex--;

    private void Discard(int increment = 0)
    {
        LineCurrentIndex += increment;
        LineStartIndex = LineCurrentIndex;
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
        Tokens.Add(new Token(TokenType.Error, CurrentPosition, error));
        return null;
    }

    private void Emit(TokenType token)
    {
        Tokens.Add(new Token(token, CurrentPosition, CurrentValue));
        LineStartIndex = LineCurrentIndex;
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
                Error("Mismatched indentation level");
        }
    }

    private StateFunction? LexIndent()
    {
        int indent = 0;
        while (true)
        {
            switch (Next())
            {
                case EOL:
                case '\r':
                case '\n':
                    Backup();
                    Discard();
                    Emit(TokenType.EOL);
                    return null;
                case ' ':
                    indent++;
                    break;
                case '\t':
                    indent += 4;
                    break;
                case ';':
                    Backup();
                    Discard();
                    EmitIndent(indent);
                    return LexComment;
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
                case ' ':
                case '\t':
                    Discard(1);
                    break;
                case EOL:
                case '\r':
                case '\n':
                    Discard();
                    Emit(TokenType.EOL);
                    return null;
                case ';':
                    return Error("Comments must be on their own line");
                case '\'':
                    return LexCharacter;
                case '"':
                    return LexString;
                default:
                    if (IsValidLiteralStart(peekVal))
                        return LexLiteral;
                    else if (char.IsDigit(peekVal))
                        return LexNumber;
                    else if (Accept(OPERATOR_CHARACTERS))
                        return LexOperator;
                    return Error($"Invalid character '{peekVal}'");

            }
        }
    }

    private StateFunction? LexCharacter()
    {
        // discard the '
        Discard(1);
        char c = Next();
        if (IsLineEnd(c))
        {
            Error("Unclosed '");
            return LexStatement;
        }
        else if (c == '\\')
        {
            c = Next();
            if (IsLineEnd(c))
            {
                Error("Unclosed '");
                return LexStatement;
            }
        }
        if (Peek != '\'')
            Error("Unclosed '");
        else
        {
            Emit(TokenType.Character);
            Discard(1);
        }
        return LexStatement;
    }

    private StateFunction? LexComment()
    {
        // discard the ;
        Discard(1);
        for (char c = Peek; !IsLineEnd(c); c = Peek)
            Next();
        Emit(TokenType.Comment);
        Emit(TokenType.EOL);
        return null;
    }

    private StateFunction? LexLiteral()
    {
        // accept the starting literal character
        bool isLiteral = Next() == LITERAL_INDICATOR;
        // if it starts with `, discard it
        if (isLiteral)
            Discard();

        for (char c = Peek; IsValidLiteral(c); c = Peek)
            Next();

        if (CurrentValue.Length == 0)
            Error("A literal must have a value");
        else if (isLiteral)
            Emit(TokenType.Literal);
        else
        {
            bool found = Helpers.KeywordTokens.TryGetValue(CurrentValue, out TokenType tokenType);
            Emit(found ? tokenType : TokenType.Literal);
        }
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
        int initialIndex = LineCurrentIndex;
        bool found = Helpers.KeywordTokens.TryGetValue(CurrentValue, out TokenType tokenType);
        while (!found && LineCurrentIndex > LineStartIndex)
        {
            Backup();
            found = Helpers.KeywordTokens.TryGetValue(CurrentValue, out tokenType);
        }
        if (LineCurrentIndex > LineStartIndex)
        {
            Emit(tokenType);
            return LexStatement;
        }
        return Error($"Invalid operator '{Line[LineStartIndex..initialIndex]}'");
    }

    private StateFunction? LexString()
    {
        // discard the "
        Discard(1);

        while (true)
        {
            char c = Peek;
            if (c == '"')
            {
                Emit(TokenType.String);
                Discard(1);
                break;
            }
            else if (c == '\\')
            {
                Next();
                if (IsLineEnd(Peek))
                {
                    Error("Unclosed \"");
                    break;
                }
            }
            else if (IsLineEnd(c))
            {
                Error("Unclosed \"");
                break;
            }
            Next();
        }

        return LexStatement;
    }
}