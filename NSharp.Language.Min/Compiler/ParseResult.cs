namespace NSharp.Language.Min.Compiler;

public class ParseResult<T>
{
    public T Result { get; set; }
    public bool Error { get; set; } = false;

    public ParseResult(T result) => Result = result;

    public ParseResult(T result, bool error)
    {
        Result = result;
        Error = error;
    }
}