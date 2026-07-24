using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scans <c>Assets/Content/voice</c> and writes a <see cref="VoiceLineLibrary"/> asset.
/// </summary>
public static class VoiceLineLibraryBuilder
{
    const string VoiceRoot = "Assets/Content/voice";
    const string DefaultAssetPath = "Assets/Content/voice/VoiceLineLibrary.asset";

    static readonly Regex FileNameRegex = new(
        @"^p_(\d+)_l_(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [MenuItem("Tools/Voice/Rebuild Voice Line Library")]
    public static void Rebuild()
    {
        if (!AssetDatabase.IsValidFolder(VoiceRoot))
        {
            Debug.LogError($"[VoiceLineLibrary] Folder missing: {VoiceRoot}");
            return;
        }

        var entries = new List<VoiceLineLibrary.Entry>();
        int skipped = 0;

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { VoiceRoot });
        for (int i = 0; i < guids.Length; i++)
        {
            string clipPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!TryParseClipPath(clipPath, out VoiceLineLibrary.VoiceCharacter character, out int phase, out int line))
            {
                skipped++;
                continue;
            }

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null)
            {
                skipped++;
                continue;
            }

            string xmlPath = Path.ChangeExtension(clipPath, ".xml");
            var phoneme = AssetDatabase.LoadAssetAtPath<TextAsset>(xmlPath);

            entries.Add(new VoiceLineLibrary.Entry
            {
                Character = character,
                Phase = phase,
                Line = line,
                Clip = clip,
                PhonemeXml = phoneme
            });
        }

        entries.Sort((a, b) =>
        {
            int c = a.Character.CompareTo(b.Character);
            if (c != 0) return c;
            c = a.Phase.CompareTo(b.Phase);
            if (c != 0) return c;
            return a.Line.CompareTo(b.Line);
        });

        VoiceLineLibrary library = AssetDatabase.LoadAssetAtPath<VoiceLineLibrary>(DefaultAssetPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<VoiceLineLibrary>();
            AssetDatabase.CreateAsset(library, DefaultAssetPath);
        }

        library.SetEntries(entries);
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int withPhoneme = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].PhonemeXml != null)
                withPhoneme++;
        }

        Debug.Log(
            $"[VoiceLineLibrary] Rebuilt {DefaultAssetPath}: {entries.Count} clips " +
            $"({withPhoneme} with phoneme XML, {skipped} skipped).");
        Selection.activeObject = library;
        EditorGUIUtility.PingObject(library);
    }

    static bool TryParseClipPath(
        string assetPath,
        out VoiceLineLibrary.VoiceCharacter character,
        out int phase,
        out int line)
    {
        character = default;
        phase = 0;
        line = 0;

        if (string.IsNullOrEmpty(assetPath))
            return false;

        string normalized = assetPath.Replace('\\', '/');
        string[] parts = normalized.Split('/');
        // .../voice/{Character}/p_N/p_N_l_M.wav
        if (parts.Length < 4)
            return false;

        string fileName = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);
        string folderPhase = parts[parts.Length - 2];
        string characterFolder = parts[parts.Length - 3];

        if (!Enum.TryParse(characterFolder, ignoreCase: true, out character))
            return false;

        Match match = FileNameRegex.Match(fileName);
        if (!match.Success)
            return false;

        phase = int.Parse(match.Groups[1].Value);
        line = int.Parse(match.Groups[2].Value);

        if (!folderPhase.Equals($"p_{phase}", StringComparison.OrdinalIgnoreCase))
            return false;

        return phase > 0 && line > 0;
    }
}
