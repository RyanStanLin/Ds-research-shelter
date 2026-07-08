using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace DhhResearch;

internal static class Csv
{
    public static List<Dictionary<string, string>> Read(string path)
    {
        using var parser = new TextFieldParser(path);
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;
        var header = parser.ReadFields() ?? throw new InvalidDataException($"Missing header: {path}");
        if (header.Distinct(StringComparer.Ordinal).Count() != header.Length)
            throw new InvalidDataException($"Duplicate column: {path}");
        var rows = new List<Dictionary<string, string>>();
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields()!;
            if (fields.Length != header.Length)
                throw new InvalidDataException($"Column count mismatch at line {parser.LineNumber}: {path}");
            rows.Add(header.Zip(fields).ToDictionary(x => x.First, x => x.Second));
        }
        return rows;
    }

    public static void Write(string path, IEnumerable<string> header, IEnumerable<IEnumerable<object?>> rows)
    {
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(false));
        writer.WriteLine(string.Join(",", header.Select(Quote)));
        foreach (var row in rows) writer.WriteLine(string.Join(",", row.Select(Quote)));
    }

    private static string Quote(object? value)
    {
        string text = value switch
        {
            null => "NA",
            double d when !double.IsFinite(d) => throw new InvalidDataException("Nonfinite output"),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()!
        };
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }

    public static double? Number(string value)
    {
        if (value is "NA" or "") return null;
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || !double.IsFinite(result))
            throw new InvalidDataException($"Invalid numerical value: {value}");
        return result;
    }
}
