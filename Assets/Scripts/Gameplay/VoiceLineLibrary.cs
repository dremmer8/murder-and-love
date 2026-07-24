using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalogue of dialogue VO clips under <c>Assets/Content/voice/{Character}/p_N/p_N_l_M</c>.
/// Rebuild via <c>Tools → Voice → Rebuild Voice Line Library</c>.
/// </summary>
[CreateAssetMenu(fileName = "VoiceLineLibrary", menuName = "Audio/Voice Line Library", order = 1)]
public class VoiceLineLibrary : ScriptableObject
{
    public enum VoiceCharacter
    {
        MC = 0,
        Mandy = 1,
        Lau = 2
    }

    [Serializable]
    public class Entry
    {
        public VoiceCharacter Character;
        public int Phase;
        public int Line;
        public AudioClip Clip;
        public TextAsset PhonemeXml;
    }

    [SerializeField] List<Entry> entries = new();

    Dictionary<long, Entry> _lookup;
    bool _built;

    public IReadOnlyList<Entry> Entries => entries;

    public void SetEntries(List<Entry> newEntries)
    {
        entries = newEntries ?? new List<Entry>();
        _built = false;
        _lookup = null;
    }

    public bool TryGet(VoiceCharacter character, int phase, int line, out Entry entry)
    {
        entry = null;
        EnsureLookup();
        return _lookup.TryGetValue(MakeKey(character, phase, line), out entry) && entry != null && entry.Clip != null;
    }

    void OnEnable()
    {
        _built = false;
        _lookup = null;
    }

    void EnsureLookup()
    {
        if (_built && _lookup != null)
            return;

        _lookup = new Dictionary<long, Entry>();
        for (int i = 0; i < entries.Count; i++)
        {
            Entry e = entries[i];
            if (e == null || e.Clip == null || e.Phase <= 0 || e.Line <= 0)
                continue;

            long key = MakeKey(e.Character, e.Phase, e.Line);
            _lookup[key] = e;
        }

        _built = true;
    }

    static long MakeKey(VoiceCharacter character, int phase, int line)
    {
        return ((long)(int)character << 40) | ((long)phase << 20) | (uint)line;
    }
}
