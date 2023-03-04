using System.Text.RegularExpressions;

namespace Kapok.DataPort.Csv;

public class CsvHelper
{
    public static Dictionary<LineSeparator, Func<string?, string[]>> DictionaryOfLineSeparatorAndItsFunc = new();

    static CsvHelper()
    {
        DictionaryOfLineSeparatorAndItsFunc[LineSeparator.Unknown] = ParseLineNotSeparated;
        DictionaryOfLineSeparatorAndItsFunc[LineSeparator.Tab] = ParseLineTabSeparated;
        DictionaryOfLineSeparatorAndItsFunc[LineSeparator.Semicolon] = ParseLineSemicolonSeparated;
        DictionaryOfLineSeparatorAndItsFunc[LineSeparator.Comma] = ParseLineCommaSeparated;
    }

    // ******************************************************************
    public enum LineSeparator
    {
        Unknown = 0,
        Tab,
        Semicolon,
        Comma
    }

    // ******************************************************************
    public static LineSeparator GuessCsvSeparator(string? oneLine)
    {
        List<Tuple<LineSeparator, int>> listOfLineSeparatorAndThereFirstLineSeparatedValueCount = new List<Tuple<LineSeparator, int>>();

        listOfLineSeparatorAndThereFirstLineSeparatedValueCount.Add(new Tuple<LineSeparator, int>(LineSeparator.Tab, ParseLineTabSeparated(oneLine).Length));
        listOfLineSeparatorAndThereFirstLineSeparatedValueCount.Add(new Tuple<LineSeparator, int>(LineSeparator.Semicolon, ParseLineSemicolonSeparated(oneLine).Length));
        listOfLineSeparatorAndThereFirstLineSeparatedValueCount.Add(new Tuple<LineSeparator, int>(LineSeparator.Comma, ParseLineCommaSeparated(oneLine).Length));

        Tuple<LineSeparator, int>? bestBet =
            // listOfLineSeparatorAndThereFirstLineSeparatedValueCount.MaxBy((n) => n.Item2).First()
            (from p in listOfLineSeparatorAndThereFirstLineSeparatedValueCount
                orderby p.Item2 descending
                select p).FirstOrDefault();

        if (bestBet != null && bestBet.Item2 > 1)
        {
            return bestBet.Item1;
        }

        return LineSeparator.Unknown;
    }

    // ******************************************************************
    public static string[] ParseLineCommaSeparated(string? line)
    {
        if (line == null) return Array.Empty<string>();

        // CSV line parsing : From "jgr4" in http://www.kimgentes.com/worshiptech-web-tools-page/2008/10/14/regex-pattern-for-parsing-csv-files-with-embedded-commas-dou.html
        var matches = Regex.Matches(line, @"\s?((?<x>(?=[,]+))|""(?<x>([^""]|"""")+)""|""(?<x>)""|(?<x>[^,]+)),?",
            RegexOptions.ExplicitCapture);

        string[] values = (from Match m in matches
            select m.Groups["x"].Value.Trim().Replace("\"\"", "\"")).ToArray();

        return values;
    }

    // ******************************************************************
    public static string[] ParseLineTabSeparated(string? line)
    {
        if (line == null) return Array.Empty<string>();

        var matchesTab = Regex.Matches(line, @"\s?((?<x>(?=[\t]+))|""(?<x>([^""]|"""")+)""|""(?<x>)""|(?<x>[^\t]+))\t?",
            RegexOptions.ExplicitCapture);

        string[] values = (from Match m in matchesTab
            select m.Groups["x"].Value.Trim().Replace("\"\"", "\"")).ToArray();

        return values;
    }

    // ******************************************************************
    public static string[] ParseLineSemicolonSeparated(string? line)
    {
        if (line == null) return Array.Empty<string>();

        // CSV line parsing : From "jgr4" in http://www.kimgentes.com/worshiptech-web-tools-page/2008/10/14/regex-pattern-for-parsing-csv-files-with-embedded-commas-dou.html
        var matches = Regex.Matches(line, @"\s?((?<x>(?=[;]+))|""(?<x>([^""]|"""")+)""|""(?<x>)""|(?<x>[^;]+));?",
            RegexOptions.ExplicitCapture);

        string[] values = (from Match m in matches
            select m.Groups["x"].Value.Trim().Replace("\"\"", "\"")).ToArray();

        return values;
    }

    // ******************************************************************
    public static string[] ParseLineNotSeparated(string? line)
    {
        if (line == null) return Array.Empty<string>();

        string[] lineValues = new string[1];
        lineValues[0] = line;
        return lineValues;
    }

    // ******************************************************************
    public static List<string[]> ParseText(string text)
    {
        // detect line ending
        int pos = 0;
        bool carriageReturnDetected = false;
        bool lineFeedDetected = false;

        while (pos < text.Length) // do lexware run until line ending is detected.
        {
            var currentChar = text[pos++];

            if (currentChar == '\n')
            {
                lineFeedDetected = true;
                break;
            }

            if (carriageReturnDetected)
                break;

            if (currentChar == '\r')
            {
                carriageReturnDetected = true;
            }
        }

        string? lineSeparator;
        if (carriageReturnDetected)
        {
            lineSeparator = lineFeedDetected
                ? "\r\n"
                : "\r";
        }
        else if (lineFeedDetected)
        {
            lineSeparator = "\n";
        }
        else
        {
            // no line detected, text just has one line
            lineSeparator = null;
        }

        var lines = lineSeparator != null
            ? text.Split(new[] { lineSeparator }, StringSplitOptions.None)
            : new[] {text};

        return ParseString(lines);
    }

    // ******************************************************************
    public static List<string[]> ParseString(string[] lines)
    {
        List<string[]> result = new List<string[]>();

        LineSeparator lineSeparator = LineSeparator.Unknown;
        if (lines.Any())
        {
            lineSeparator = GuessCsvSeparator(lines[0]);
        }

        Func<string, string[]> funcParse = DictionaryOfLineSeparatorAndItsFunc[lineSeparator];

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            result.Add(funcParse(line));
        }

        return result;
    }

    // ******************************************************************
}