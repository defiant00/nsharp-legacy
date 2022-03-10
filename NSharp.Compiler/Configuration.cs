namespace NSharp.Compiler;

public static class Configuration
{
    private static readonly char[] Delimiters = { ' ', '\t' };

    public static Dictionary<string, string> Load(string fileName)
    {
        var configuration = new Dictionary<string, string>();
        using var reader = new StreamReader(fileName);
        string? line;
        while ((line = reader.ReadLine()?.Trim()) != null)
        {
            int index = line.IndexOfAny(Delimiters);
            string key = line;
            string value = string.Empty;
            if (index > -1)
            {
                key = line[0..index];
                value = line[(index + 1)..].TrimStart();
                if (value.Length > 1 && value.StartsWith('"') && value.EndsWith('"'))
                    value = value[1..^1];
            }
            if (key.Length > 0)
                configuration[key] = value;
        }
        return configuration;
    }
}