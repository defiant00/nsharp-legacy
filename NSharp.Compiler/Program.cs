using System.Diagnostics;
using NSharp.Core;
using NSharp.Language.Neutral;

namespace NSharp.Compiler;

public static class Program
{
    private const string EDIT_SETTINGS = ".nsedit";

    private const string SAVE_SETTINGS = ".nssave";

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
            Console.WriteLine("    compile [files]       - compile files");
            Console.WriteLine("    edit [files]          - create a file.ns.edit file per input file for editing per the .nsedit settings");
            Console.WriteLine("    format [files]        - format the specified files per the .nsedit and .nssave settings");
            Console.WriteLine("    save [files]          - save the specified files per the .nssave settings, deleting the .edit file on success");
            Console.WriteLine("    validate [files]      - validate the specified files' syntax");
            return;
        }

        switch (args[0].ToLower())
        {
            case "build":
                Console.WriteLine("Build not yet supported");
                break;
            case "compile":
                Console.WriteLine("Compile not yet supported");
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

    private static void Edit(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");
        if (!file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"File '{file}' does not end with .ns");

        ILanguage loadLang = FindSpecifiedLanguage(SAVE_SETTINGS, file);
        ILanguage saveLang = FindSpecifiedLanguage(EDIT_SETTINGS, file);

        Core.Ast.AstItem ast = loadLang.Load(file);
        File.WriteAllText(file + ".edit", saveLang.Save(ast));
    }

    private static void Format(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");

        ILanguage language;

        if (file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            language = FindSpecifiedLanguage(SAVE_SETTINGS, file);
        else if (file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            language = FindSpecifiedLanguage(EDIT_SETTINGS, file);
        else
            throw new Exception($"File '{file}' does not end with .ns or .ns.edit");

        Core.Ast.AstItem ast = language.Load(file);
        File.WriteAllText(file, language.Save(ast));
    }

    private static void Save(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");
        if (!file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"File '{file}' does not end with .ns.edit");

        ILanguage loadLang = FindSpecifiedLanguage(EDIT_SETTINGS, file);
        ILanguage saveLang = FindSpecifiedLanguage(SAVE_SETTINGS, file);

        Core.Ast.AstItem ast = loadLang.Load(file);
        File.WriteAllText(file[..^5], saveLang.Save(ast));
        // TODO - delete the .ns.edit file on success
    }

    private static void Validate(string file)
    {
        if (!File.Exists(file))
            throw new Exception($"File '{file}' does not exist.");

        ILanguage language;

        if (file.EndsWith(".ns", StringComparison.OrdinalIgnoreCase))
            language = FindSpecifiedLanguage(SAVE_SETTINGS, file);
        else if (file.EndsWith(".ns.edit", StringComparison.OrdinalIgnoreCase))
            language = FindSpecifiedLanguage(EDIT_SETTINGS, file);
        else
            throw new Exception($"File '{file}' does not end with .ns or .ns.edit");

        Core.Ast.AstItem ast = language.Load(file);
    }

    private static ILanguage DefaultLanguage() => new Neutral();

    private static ILanguage GetLanguage(string settingsPath)
    {
        //  TODO - load the language and config options from the settings file
        return DefaultLanguage();
    }

    private static ILanguage FindSpecifiedLanguage(string settingsFileName, string startingPath)
    {
        string? dirName = Path.GetDirectoryName(startingPath);
        if (dirName == null)
            return DefaultLanguage();
        DirectoryInfo? dir = new DirectoryInfo(dirName);
        while (dir != null)
        {
            string settingsPath = Path.Combine(dir.FullName, settingsFileName);
            if (File.Exists(settingsPath))
                return GetLanguage(settingsPath);
            dir = dir.Parent;
        }
        Console.WriteLine($"'{settingsFileName}' not found, using the default language.");
        return DefaultLanguage();
    }
}