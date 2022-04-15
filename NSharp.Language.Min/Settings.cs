namespace NSharp.Language.Min;

public class Settings
{
    private const string ALL_PARENS = "allparens";
    private const string ALL_PAREN_GENERICS = "allparengenerics";
    private const string INDENTATION = "indentation";
    private const string NO_INDENT_MULTILINE = "no_indentmultiline";
    private const string PARAM_MULTILINE = "parammultiline";
    private const string CTYPES = "ctypes";

    public bool AllParens { get; set; }
    public bool AllParenGenerics { get; set; }
    public string Indentation { get; set; }
    public bool NoIndentMultiline { get; set; }
    public bool ParamMultiline { get; set; }
    public bool CTypes { get; set; }

    public Settings(Dictionary<string, string> settings)
    {
        AllParens = settings.ContainsKey(ALL_PARENS);
        AllParenGenerics = settings.ContainsKey(ALL_PAREN_GENERICS);
        Indentation = settings.ContainsKey(INDENTATION) ? settings[INDENTATION] : "    ";
        NoIndentMultiline = settings.ContainsKey(NO_INDENT_MULTILINE);
        ParamMultiline = settings.ContainsKey(PARAM_MULTILINE);
        CTypes = settings.ContainsKey(CTYPES);
    }
}