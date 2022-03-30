namespace NSharp.Compiler;

public class CompilerState
{
    public string Namespace { get; set; } = string.Empty;
    public int FieldDefinitionRow { get; set; } = 1;
    public int MethodDefinitionRow { get; set; } = 1;
}