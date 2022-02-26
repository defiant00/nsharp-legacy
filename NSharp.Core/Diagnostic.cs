namespace NSharp.Core;

public enum Severity
{
    Message,
    Warning,
    Error,
}

public class Diagnostic
{
    public Severity Severity { get; set; }

    public string Message { get; set; } = string.Empty;
}