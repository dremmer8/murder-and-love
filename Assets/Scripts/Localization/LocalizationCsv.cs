using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Minimal CSV helper for localization tables: header <c>key,text</c>, quoted fields, newlines in quotes.
/// </summary>
public static class LocalizationCsv
{
    public static Dictionary<string, string> Parse(string csv)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrEmpty(csv))
            return map;

        List<string> lines = SplitRecords(csv);
        int start = 0;
        if (lines.Count > 0 && IsHeader(lines[0]))
            start = 1;

        for (int i = start; i < lines.Count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!TrySplitColumns(line, out string key, out string text))
                continue;

            key = key.Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            map[key] = Unescape(text);
        }

        return map;
    }

    public static string Write(IEnumerable<KeyValuePair<string, string>> entries)
    {
        var sb = new StringBuilder();
        sb.Append("key,text\n");
        foreach (KeyValuePair<string, string> pair in entries)
        {
            if (string.IsNullOrEmpty(pair.Key))
                continue;

            sb.Append(Escape(pair.Key));
            sb.Append(',');
            sb.Append(Escape(pair.Value ?? ""));
            sb.Append('\n');
        }

        return sb.ToString();
    }

    static bool IsHeader(string line)
    {
        if (!TrySplitColumns(line, out string a, out string b))
            return false;

        return string.Equals(a.Trim(), "key", StringComparison.OrdinalIgnoreCase)
            && string.Equals(b.Trim(), "text", StringComparison.OrdinalIgnoreCase);
    }

    static List<string> SplitRecords(string csv)
    {
        var records = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
                continue;
            }

            if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    i++;

                records.Add(current.ToString());
                current.Length = 0;
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            records.Add(current.ToString());

        return records;
    }

    static bool TrySplitColumns(string line, out string key, out string text)
    {
        key = "";
        text = "";
        if (string.IsNullOrEmpty(line))
            return false;

        var cols = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                cols.Add(current.ToString());
                current.Length = 0;
                continue;
            }

            current.Append(c);
        }

        cols.Add(current.ToString());
        if (cols.Count < 2)
            return false;

        key = cols[0];
        // Join remaining columns in case text accidentally contained unquoted commas.
        if (cols.Count == 2)
            text = cols[1];
        else
        {
            var rest = new StringBuilder(cols[1]);
            for (int i = 2; i < cols.Count; i++)
            {
                rest.Append(',');
                rest.Append(cols[i]);
            }

            text = rest.ToString();
        }

        return true;
    }

    static string Escape(string value)
    {
        if (value == null)
            return "";

        bool needsQuotes = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        if (!needsQuotes)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    static string Unescape(string value)
    {
        return value ?? "";
    }
}
