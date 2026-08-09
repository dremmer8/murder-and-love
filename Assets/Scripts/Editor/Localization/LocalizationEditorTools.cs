using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tools: export UI CSV, create English catalog, register alt locales, bind LocalizedTmpText.
/// </summary>
public static class LocalizationEditorTools
{
    const string LocalizationFolder = "Assets/Localization";
    const string ResourcesFolder = "Assets/Resources";
    const string CatalogAssetPath = ResourcesFolder + "/LocalizationCatalog.asset";
    const string EnglishCsvPath = LocalizationFolder + "/ui_en.csv";
    const string DefaultInkPath = "Assets/Dialogues/Final_dialogues/Final_eng.json";
    const string GameScenePath = "Assets/Scenes/Game.unity";
    const string ChineseInkPath = "Assets/Dialogues/Final_dialogues/Final_zh.ink";
    const string ChineseCsvPath = LocalizationFolder + "/ui_zh.csv";
    const string ChineseFontCharsPath = "Assets/Content/Prototype/Font/HuiwenZH/used_chars.txt";

    [MenuItem("Tools/Localization/Export UI Strings CSV")]
    public static void ExportUiStringsCsv()
    {
        EnsureFolders();
        EnsureGameSceneLoadedForHarvest();

        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        HarvestDefaults(map);
        HarvestFromOpenScenes(map);

        string csv = LocalizationCsv.Write(map);
        string path = EditorUtility.SaveFilePanel(
            "Export UI Strings CSV",
            Path.GetFullPath(LocalizationFolder),
            "ui_en.csv",
            "csv");

        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllText(path, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        if (path.Replace('\\', '/').Contains("/Assets/"))
        {
            string assetPath = "Assets" + LastSplitSegment(path.Replace('\\', '/'), "/Assets");
            AssetDatabase.ImportAsset(assetPath);
        }

        // Always refresh the project English CSV when exporting to the default location.
        if (string.Equals(
                Path.GetFullPath(path),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), EnglishCsvPath)),
                StringComparison.OrdinalIgnoreCase))
        {
            AssetDatabase.ImportAsset(EnglishCsvPath);
        }
        else
        {
            // Also write/update the project seed path for convenience.
            File.WriteAllText(EnglishCsvPath, csv, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(EnglishCsvPath);
        }

        Debug.Log($"[Localization] Exported {map.Count} strings to {path}");
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Localization/Create Or Update English Catalog")]
    public static void CreateOrUpdateEnglishCatalog()
    {
        EnsureFolders();

        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), EnglishCsvPath)))
        {
            // Build CSV into the project path without a save dialog.
            EnsureGameSceneLoadedForHarvest();
            var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
            HarvestDefaults(map);
            HarvestFromOpenScenes(map);
            File.WriteAllText(EnglishCsvPath, LocalizationCsv.Write(map), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(EnglishCsvPath);
        }

        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(EnglishCsvPath);
        TextAsset ink = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultInkPath);
        if (csv == null)
        {
            Debug.LogError($"[Localization] Missing CSV at {EnglishCsvPath}");
            return;
        }

        if (ink == null)
            Debug.LogWarning($"[Localization] Missing Ink JSON at {DefaultInkPath} — assign later on the catalog.");

        LocalizationCatalog catalog = AssetDatabase.LoadAssetAtPath<LocalizationCatalog>(CatalogAssetPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<LocalizationCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
        }

        LocalizationLocale existingEn = catalog.FindById("en");
        TMP_FontAsset existingFont = existingEn?.font;
        float existingSizeOffset = existingEn?.fontSizeOffset ?? 0f;
        VoiceLineLibrary existingVoice = existingEn?.voiceLineLibrary;
        catalog.EditorUpsertLocale(new LocalizationLocale
        {
            id = "en",
            displayName = "English",
            uiCsv = csv,
            inkStory = ink,
            font = existingFont,
            fontSizeOffset = existingSizeOffset,
            voiceLineLibrary = existingVoice
        });

        // Ensure default id is English after upsert.
        var list = new List<LocalizationLocale>();
        for (int i = 0; i < catalog.Locales.Count; i++)
            list.Add(catalog.Locales[i]);
        catalog.EditorSetLocales(list, defaultId: "en");

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Localization] English catalog ready at {CatalogAssetPath}");
        Selection.activeObject = catalog;
    }

    [MenuItem("Tools/Localization/Register Locale From CSV…")]
    public static void RegisterLocaleFromCsv()
    {
        EnsureFolders();
        LocalizationCatalog catalog = AssetDatabase.LoadAssetAtPath<LocalizationCatalog>(CatalogAssetPath);
        if (catalog == null)
        {
            CreateOrUpdateEnglishCatalog();
            catalog = AssetDatabase.LoadAssetAtPath<LocalizationCatalog>(CatalogAssetPath);
        }

        string csvPath = EditorUtility.OpenFilePanel("Select translated UI CSV", Path.GetFullPath(LocalizationFolder), "csv");
        if (string.IsNullOrEmpty(csvPath))
            return;

        string inkPath = EditorUtility.OpenFilePanel("Select compiled Ink JSON (optional — Cancel to skip)", Application.dataPath, "json");

        string id = Path.GetFileNameWithoutExtension(csvPath);
        if (id.StartsWith("ui_", StringComparison.OrdinalIgnoreCase))
            id = id.Substring(3);
        if (string.IsNullOrWhiteSpace(id))
            id = "alt";

        string displayName = id.Length > 0
            ? char.ToUpperInvariant(id[0]) + id.Substring(1)
            : id;

        if (!EditorUtility.DisplayDialog(
                "Register Locale",
                $"Register locale?\n\nId: {id}\nDisplay name: {displayName}\nCSV: {destCsvPreview(id)}\n\nYou can rename displayName on the LocalizationCatalog afterward.",
                "Register",
                "Cancel"))
            return;

        string destCsv = $"{LocalizationFolder}/ui_{id}.csv";
        File.Copy(csvPath, Path.Combine(Directory.GetCurrentDirectory(), destCsv), overwrite: true);
        AssetDatabase.ImportAsset(destCsv);
        TextAsset csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(destCsv);

        TextAsset inkAsset = null;
        if (!string.IsNullOrEmpty(inkPath))
        {
            string destInk = $"{LocalizationFolder}/Final_{id}.json";
            if (inkPath.Replace('\\', '/').Contains("/Assets/"))
            {
                string assetPath = "Assets" + LastSplitSegment(inkPath.Replace('\\', '/'), "/Assets");
                inkAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            }

            if (inkAsset == null)
            {
                File.Copy(inkPath, Path.Combine(Directory.GetCurrentDirectory(), destInk), overwrite: true);
                AssetDatabase.ImportAsset(destInk);
                inkAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(destInk);
            }
        }

        LocalizationLocale existing = catalog.FindById(id);
        TMP_FontAsset existingFont = existing?.font;
        float existingSizeOffset = existing?.fontSizeOffset ?? 0f;
        VoiceLineLibrary existingVoice = existing?.voiceLineLibrary;
        catalog.EditorUpsertLocale(new LocalizationLocale
        {
            id = id,
            displayName = displayName,
            uiCsv = csvAsset,
            inkStory = inkAsset != null ? inkAsset : catalog.FindById("en")?.inkStory,
            font = existingFont,
            fontSizeOffset = existingSizeOffset,
            voiceLineLibrary = existingVoice
        });

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Localization] Registered locale '{id}' ({displayName}) on catalog.");
        Selection.activeObject = catalog;

        static string destCsvPreview(string localeId) => $"{LocalizationFolder}/ui_{localeId}.csv";
    }

    [MenuItem("Tools/Localization/Export Chinese Font Character List")]
    public static void ExportChineseFontCharacterList()
    {
        var chars = new SortedSet<char>();
        AddAsciiFontChars(chars);

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ChineseInkPath)))
            AddFileChars(chars, ChineseInkPath);
        else
            Debug.LogWarning($"[Localization] Missing Ink at {ChineseInkPath}");

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ChineseCsvPath)))
            AddFileChars(chars, ChineseCsvPath);
        else
            Debug.LogWarning($"[Localization] Missing CSV at {ChineseCsvPath}");

        string directory = Path.GetDirectoryName(ChineseFontCharsPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), directory)))
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), directory));

        var builder = new StringBuilder(chars.Count);
        foreach (char c in chars)
            builder.Append(c);

        File.WriteAllText(
            Path.Combine(Directory.GetCurrentDirectory(), ChineseFontCharsPath),
            builder.ToString(),
            new UTF8Encoding(false));
        AssetDatabase.ImportAsset(ChineseFontCharsPath);

        Debug.Log($"[Localization] Exported {chars.Count} unique characters to {ChineseFontCharsPath}. Use Font Asset Creator → Characters from File.");
        EditorUtility.RevealInFinder(Path.Combine(Directory.GetCurrentDirectory(), ChineseFontCharsPath));
    }

    static void AddAsciiFontChars(ISet<char> chars)
    {
        for (char c = ' '; c <= '~'; c++)
            chars.Add(c);
    }

    static void AddFileChars(ISet<char> chars, string assetPath)
    {
        string text = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), assetPath), Encoding.UTF8);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!char.IsControl(c) || c == '\n' || c == '\r')
                chars.Add(c);
        }
    }

    [MenuItem("Tools/Localization/Add LocalizedTmpText To Selection")]
    public static void AddLocalizedTmpTextToSelection()
    {
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            TMP_Text label = go.GetComponent<TMP_Text>();
            if (label == null)
                label = go.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
                continue;

            LocalizedTmpText loc = label.GetComponent<LocalizedTmpText>();
            if (loc == null)
                loc = Undo.AddComponent<LocalizedTmpText>(label.gameObject);

            string key = SuggestMenuKey(label);
            loc.EditorConfigure(key, label.text);
            EditorUtility.SetDirty(loc);
            count++;
        }

        Debug.Log($"[Localization] Configured LocalizedTmpText on {count} object(s).");
    }

    [MenuItem("Tools/Localization/Add Menu Localizer To Open Scenes")]
    public static void AddMenuLocalizerToOpenScenes()
    {
        int added = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
                for (int c = 0; c < canvases.Length; c++)
                {
                    Canvas canvas = canvases[c];
                    if (canvas == null)
                        continue;

                    string n = canvas.gameObject.name;
                    if (n.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) < 0
                        && n.IndexOf("Pause", StringComparison.OrdinalIgnoreCase) < 0
                        && n != "CanvasMain"
                        && n != "CanvasMainMenu"
                        && n != "CanvasPauseMenu")
                        continue;

                    if (canvas.GetComponent<UiMenuLabelLocalizer>() != null)
                        continue;

                    Undo.AddComponent<UiMenuLabelLocalizer>(canvas.gameObject);
                    added++;
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }

        Debug.Log($"[Localization] Added UiMenuLabelLocalizer to {added} canvas(es).");
    }

    static string LastSplitSegment(string value, string separator)
    {
        string[] parts = value.Split(new[] { separator }, StringSplitOptions.None);
        return parts.Length > 0 ? parts[parts.Length - 1] : "";
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Localization"))
            AssetDatabase.CreateFolder("Assets", "Localization");
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }

    static void EnsureGameSceneLoadedForHarvest()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == GameScenePath)
                return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);
    }

    static void HarvestDefaults(SortedDictionary<string, string> map)
    {
        void Put(string key, string text)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (!map.ContainsKey(key))
                map[key] = text ?? "";
        }

        Put(LocalizationKeys.HintInteract, "[E] Interact");
        Put(LocalizationKeys.HintTalk, "[E] Talk");
        Put(LocalizationKeys.HintCheckPager, "[TAB] Check pager");
        Put(LocalizationKeys.HintMinigameFallback, "A / D — turn dial\nMouse — grab & place\n[ESC] Leave");
        Put(LocalizationKeys.HintPagerOpen, "A / D — scroll\n[D] at end — next message\n[TAB] Put down pager");
        Put(LocalizationKeys.HintPagerTutorialScroll, "[A] / [D] Scroll left & right");
        Put(LocalizationKeys.HintPagerTutorialAdvance, "[D] at end — next message");
        Put(LocalizationKeys.HintPagerRespondReading, "A / D — scroll\n[D] at end — continue / reply\n[TAB] Put down pager");
        Put(LocalizationKeys.HintPagerRespondTyping, "Type on any key to reply\n[TAB] Put down pager");
        Put(LocalizationKeys.HintPagerRespondTutorialScroll, "[A] / [D] Scroll left & right");
        Put(LocalizationKeys.HintPagerRespondTutorialAdvance, "[D] at end — continue / reply");
        Put(LocalizationKeys.HintDialogueProgress, "[SPACE] Continue\nMouse — choose options");
        Put(LocalizationKeys.HintDialogueChoice, "Mouse — choose options");

        Put(LocalizationKeys.PagerEmpty, "no messages");
        Put(LocalizationKeys.PagerNewMessage, "new message");
        Put(LocalizationKeys.PagerStartTyping, "start typing");
        Put(LocalizationKeys.PagerRespondReply, "I'm done. The wash cycles should be finished in a few minutes.");

        Put(LocalizationKeys.MenuPlay, "Play");
        Put(LocalizationKeys.MenuResume, "Resume");
        Put(LocalizationKeys.MenuOptions, "Options");
        Put(LocalizationKeys.MenuSound, "Sound");
        Put(LocalizationKeys.MenuMusic, "Music");
        Put(LocalizationKeys.MenuLanguage, "Language");
        Put(LocalizationKeys.MenuBack, "Back");
        Put(LocalizationKeys.MenuExit, "Exit");
        Put(LocalizationKeys.MenuExitToMenu, "Exit to menu");
    }

    static void HarvestFromOpenScenes(SortedDictionary<string, string> map)
    {
        void Put(string key, string text)
        {
            if (string.IsNullOrEmpty(key) || text == null)
                return;
            map[key] = text;
        }

        foreach (ControlHintsPresenter hints in UnityEngine.Object.FindObjectsByType<ControlHintsPresenter>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SerializedObject so = new SerializedObject(hints);
            Put(LocalizationKeys.HintInteract, so.FindProperty("interactableHintText")?.stringValue);
            Put(LocalizationKeys.HintTalk, so.FindProperty("dialogueHintText")?.stringValue);
            Put(LocalizationKeys.HintCheckPager, so.FindProperty("checkPagerHint")?.stringValue);
            Put(LocalizationKeys.HintMinigameFallback, so.FindProperty("minigameFallbackHint")?.stringValue);
            Put(LocalizationKeys.HintPagerOpen, so.FindProperty("pagerOpenHint")?.stringValue);
            Put(LocalizationKeys.HintPagerTutorialScroll, so.FindProperty("pagerTutorialScrollHint")?.stringValue);
            Put(LocalizationKeys.HintPagerTutorialAdvance, so.FindProperty("pagerTutorialAdvanceHint")?.stringValue);
            Put(LocalizationKeys.HintPagerRespondReading, so.FindProperty("pagerRespondReadingHint")?.stringValue);
            Put(LocalizationKeys.HintPagerRespondTyping, so.FindProperty("pagerRespondTypingHint")?.stringValue);
            Put(LocalizationKeys.HintPagerRespondTutorialScroll, so.FindProperty("pagerRespondTutorialScrollHint")?.stringValue);
            Put(LocalizationKeys.HintPagerRespondTutorialAdvance, so.FindProperty("pagerRespondTutorialAdvanceHint")?.stringValue);
            Put(LocalizationKeys.HintDialogueProgress, so.FindProperty("dialogueProgressHint")?.stringValue);
            Put(LocalizationKeys.HintDialogueChoice, so.FindProperty("dialogueChoiceHint")?.stringValue);
        }

        foreach (PagerTextController pager in UnityEngine.Object.FindObjectsByType<PagerTextController>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SerializedObject so = new SerializedObject(pager);
            Put(LocalizationKeys.PagerEmpty, so.FindProperty("emptyInboxText")?.stringValue);
            Put(LocalizationKeys.PagerNewMessage, so.FindProperty("unreadPropText")?.stringValue);
            Put(LocalizationKeys.PagerStartTyping, so.FindProperty("startTypingText")?.stringValue);
            Put(LocalizationKeys.PagerRespondReply, so.FindProperty("respondSupportReply")?.stringValue);
        }

        foreach (TaskManager tasks in UnityEngine.Object.FindObjectsByType<TaskManager>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SerializedObject so = new SerializedObject(tasks);
            SerializedProperty list = so.FindProperty("tasks");
            if (list == null || !list.isArray)
                continue;

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                string text = entry.FindPropertyRelative("taskText")?.stringValue;
                if (!string.IsNullOrEmpty(text))
                    Put(LocalizationKeys.Task(i), text);
            }

            string fallback = so.FindProperty("fallbackText")?.stringValue;
            if (!string.IsNullOrEmpty(fallback))
                Put("task.fallback", fallback);
        }

        foreach (MinigameActivator mini in UnityEngine.Object.FindObjectsByType<MinigameActivator>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string controls = mini.ControlHintsFallback;
            if (!string.IsNullOrEmpty(controls))
                Put(LocalizationKeys.MinigameControls(mini.gameObject.name), controls);

            IReadOnlyList<MinigameStepHintEntry> steps = mini.StepHints;
            if (steps == null)
                continue;

            for (int i = 0; i < steps.Count; i++)
            {
                MinigameStepHintEntry step = steps[i];
                if (step == null || string.IsNullOrEmpty(step.stepId) || string.IsNullOrEmpty(step.hintText))
                    continue;

                Put(LocalizationKeys.MinigameStep(mini.gameObject.name, step.stepId), step.hintText);
            }
        }

        foreach (LocalizedTmpText loc in UnityEngine.Object.FindObjectsByType<LocalizedTmpText>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(loc.Key))
                continue;

            TMP_Text label = loc.GetComponent<TMP_Text>();
            string text = !string.IsNullOrEmpty(loc.Fallback)
                ? loc.Fallback
                : label != null ? label.text : "";
            Put(loc.Key, text);
        }

        foreach (UiMenuLabelLocalizer menu in UnityEngine.Object.FindObjectsByType<UiMenuLabelLocalizer>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach ((string key, string text) in menu.EditorHarvest())
                Put(key, text);
        }
    }

    static string SuggestMenuKey(TMP_Text label)
    {
        string name = label.gameObject.name;
        return name switch
        {
            "Play" => LocalizationKeys.MenuPlay,
            "Resume" => LocalizationKeys.MenuResume,
            "Options" => LocalizationKeys.MenuOptions,
            "Sound" or "Sounds" => LocalizationKeys.MenuSound,
            "Music" => LocalizationKeys.MenuMusic,
            "Language" => LocalizationKeys.MenuLanguage,
            "Back" => LocalizationKeys.MenuBack,
            "Exit" => LocalizationKeys.MenuExit,
            "Exit to menu" => LocalizationKeys.MenuExitToMenu,
            _ => "ui." + name.Replace(' ', '_').ToLowerInvariant()
        };
    }
}
