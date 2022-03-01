namespace NSharp.Language.Neutral.Compiler;

public class ParseResult<T>
{
    public T Result { get; set; }
    public bool Error { get; set; }

    public ParseResult(T result, bool error)
    {
        Result = result;
        Error = error;
    }
}