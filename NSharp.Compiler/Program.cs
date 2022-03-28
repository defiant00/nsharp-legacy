using System.Diagnostics;
using NSharp.Core;
using NSharp.Language.CStyle;
using NSharp.Language.Min;
using NSharp.Language.PyStyle;

namespace NSharp.Compiler;

public static class Program
{
    private const string EDIT_SETTINGS = ".nsedit";
    private const string SAVE_SETTINGS = ".nssave";
    private const string LANGUAGE_KEY = "language";

    public static void Main(string[] args)
    {
        Console.WriteLine("N# Compiler v0.1.0");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (args.Length < 2 || args[0] == "?" || args[0] == "help")
        {
            Console.WriteLine();
            Console.WriteLine("Usage: nsc [command] [file(s)]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("    build [project paths] - tbd, build projects");
            Console.WriteLine("    compile [files]       - tbd, compile files");
            Console.WriteLine("    edit [files]          - create a file.ns.edit file per input file for editing per the .nsedit settings");
            Console.WriteLine("    format [files]        - format the specified files per the .nsedit and .nssave settings");
            Console.WriteLine("    save [files]          - save the specified files per the .nssave settings");
            Console.WriteLine("    validate [files]      - validate the specified files' syntax");
            return;
        }

        switch (args[0].ToLower())
        {
            case "build":
                Console.WriteLine("Build not yet supported");
                break;
            case "compile":
                Compile(args.Skip(1));
                break;
            case "edit":
                for (int i = 1; i < args.Length; i++)
                    Edit(args[i]);
                break;
            case "format":
                for (int i = 1; i < args.Length; i++)
                    Format(args[i]);
                break;
            case "save":
                for (int i = 1; i < args.Length; i++)
                    Save(args[i]);
                break;
            case "validate":
                for (int i = 1; i < args.Length; i++)
                    Validate(args[i]);
                break;
            default:
                Console.WriteLine($"Unknown command '{args[0]}'");
                return;
        }

        Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
    }

    private static void Compile(IEnumerable<string> files)
    {
        var compiler = new Compiler();
        foreach (string file in files)
        {
            if (!File.Exists(file))
                throw new Exception($"File '{file}' does not exist.");

            ILanguage language;

            if (file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
                language = GetLanguage(SAVE_SETTINGS, file);
            else if (file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
                language = GetLanguage(EDIT_SETTINGS, file);
            else
                throw new Exception($"File '{file}' does not end with .ns or .ns.edit");

            LoadResult loadResult = language.Load(file);
            HandleResult(loadResult);
            if (loadResult.Ast != null)
                compiler.Add(loadResult.Ast);
        }
        HandleCompileResult(compiler.Compile());
        compiler.Save();
    }

    private static void Edit(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");
        if (!file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"File '{file}' does not end with .ns");

        ILanguage loadLang = GetLanguage(SAVE_SETTINGS, file);
        ILanguage saveLang = GetLanguage(EDIT_SETTINGS, file);

        LoadResult loadResult = loadLang.Load(file);
        HandleResult(loadResult);
        if (loadResult.Ast != null)
            HandleResult(saveLang.Save(file + ".edit", loadResult.Ast));
    }

    private static void Format(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");

        ILanguage language;

        if (file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            language = GetLanguage(SAVE_SETTINGS, file);
        else if (file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            language = GetLanguage(EDIT_SETTINGS, file);
        else
            throw new Exception($"File '{file}' does not end with .ns or .ns.edit");

        LoadResult loadResult = language.Load(file);
        HandleResult(loadResult);
        if (loadResult.Ast != null)
            HandleResult(language.Save(file, loadResult.Ast));
    }

    private static void Save(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");
        if (!file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"File '{file}' does not end with .ns.edit");

        ILanguage loadLang = GetLanguage(EDIT_SETTINGS, file);
        ILanguage saveLang = GetLanguage(SAVE_SETTINGS, file);

        LoadResult loadResult = loadLang.Load(file);
        HandleResult(loadResult);
        if (loadResult.Ast != null)
            HandleResult(saveLang.Save(file[..^5], loadResult.Ast));
    }

    private static void Validate(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");

        ILanguage language;

        if (file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            language = GetLanguage(SAVE_SETTINGS, file);
        else if (file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            language = GetLanguage(EDIT_SETTINGS, file);
        else
            throw new Exception($"File '{file}' does not end with .ns or .ns.edit");

        LoadResult loadResult = language.Load(file);
        HandleResult(loadResult);
    }

    private static void HandleResult(Result result)
    {
        if (result.Diagnostics.Count > 0)
        {
            Console.WriteLine(result.FileName);
            foreach (var diagnostic in result.Diagnostics)
                Console.WriteLine($"  {diagnostic.Severity}: {diagnostic.Message}");
        }
    }

    private static void HandleCompileResult(List<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            Console.WriteLine($"{diagnostic.Severity}: {diagnostic.Message}");
        if (diagnostics.All(d => d.Severity != Severity.Error))
            Console.WriteLine("Compile successful");
    }

    private static ILanguage GetLanguage(string settingsFileName, string startingPath)
    {
        Dictionary<string, string>? settings = null;

        string? dirName = Path.GetDirectoryName(startingPath);
        if (dirName != null)
        {
            DirectoryInfo? dir = new DirectoryInfo(dirName);
            while (settings == null && dir != null)
            {
                string settingsPath = Path.Combine(dir.FullName, settingsFileName);
                if (File.Exists(settingsPath))
                    settings = Configuration.Load(settingsPath);
                dir = dir.Parent;
            }
        }

        if (settings == null)
        {
            Console.WriteLine($"'{settingsFileName}' not found, using default settings.");
            settings = new();
        }

        ILanguage? lang = null;
        if (settings.ContainsKey(LANGUAGE_KEY))
        {
            switch (settings[LANGUAGE_KEY])
            {
                case "CStyle":
                    lang = new CStyle(settings);
                    break;
                case "Min":
                    lang = new Min(settings);
                    break;
                case "PyStyle":
                    lang = new PyStyle(settings);
                    break;
            }
        }
        if (lang == null)
            lang = new Min(settings);

        return lang;
    }
}