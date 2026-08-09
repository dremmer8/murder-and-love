using System;
using System.Collections.Generic;

/// <summary>
/// Speaker-name handling shared by dialogue VO, speaker colouring and thought-line detection.
/// <para>
/// Each locale's Ink script writes its own speaker names and punctuation (Chinese uses the
/// full-width colon <c>：</c>), while <see cref="VoiceLineLibrary"/> and the Inspector speaker
/// colour list stay keyed on the English names. Callers split on <see cref="IndexOfSeparator"/>
/// and pass the result through <see cref="Canonicalize"/> so every locale resolves to the same
/// English key.
/// </para>
/// </summary>
public static class LocalizedSpeakerNames
{
    public const string Thoughts = "Thoughts";

    static readonly char[] Separators = { ':', '：' };

    static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "你", "You" },
        { "王太太", "Mrs Wong" },
        { "曼婷", "Mandy" },
        { "醉汉", "Drunk Man" },
        { "醉警", "Drunk Cop" },
        { "警官", "Police Officer" },
        { "杰", "J" },
        { "宇杰", "Jason" },
        { "思绪", Thoughts },
    };

    /// <summary>Index of the speaker/body separator, or -1 when the line has none.</summary>
    public static int IndexOfSeparator(string line)
    {
        return string.IsNullOrEmpty(line) ? -1 : line.IndexOfAny(Separators);
    }

    /// <summary>
    /// Trims a speaker name, drops a trailing separator and maps it to its English equivalent.
    /// Unknown names are returned trimmed so English scripts pass through untouched.
    /// </summary>
    public static string Canonicalize(string speaker)
    {
        if (string.IsNullOrWhiteSpace(speaker))
            return string.Empty;

        string trimmed = speaker.Trim().TrimEnd(Separators).TrimEnd();
        return Aliases.TryGetValue(trimmed, out string english) ? english : trimmed;
    }

    public static bool IsThoughts(string speaker)
    {
        return string.Equals(Canonicalize(speaker), Thoughts, StringComparison.OrdinalIgnoreCase);
    }
}
