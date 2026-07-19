using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Captures baked lightmaps, light probes, and reflection probes into a
/// <see cref="BakedLightingScenario"/>. Textures are duplicated so the next
/// Generate Lighting pass cannot destroy the snapshot.
/// </summary>
public static class BakedLightingScenarioCapture
{
    const string DefaultFolder = "Assets/LightingScenarios";
    const string AutoBackupFolder = "Assets/LightingScenarios/_AutoBackup";

    [InitializeOnLoadMethod]
    static void RegisterBakeGuard()
    {
        Lightmapping.bakeStarted -= OnBakeStarted;
        Lightmapping.bakeStarted += OnBakeStarted;
    }

    static void OnBakeStarted()
    {
        if (LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0)
            return;

        if (!EditorPrefs.GetBool("MAL_AutoBackupLightmapsBeforeBake", true))
            return;

        try
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName))
                sceneName = "Scene";

            EnsureFolder(AutoBackupFolder);
            string path = $"{AutoBackupFolder}/{sceneName}_{stamp}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var scenario = ScriptableObject.CreateInstance<BakedLightingScenario>();
            AssetDatabase.CreateAsset(scenario, path);
            CaptureInto(scenario, path, clearPreviousCopies: false);
            EditorUtility.SetDirty(scenario);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[BakedLighting] Auto-backed up current bake to '{path}' before Generate Lighting. " +
                "Disable via MurderAndLove/Lighting/Auto-Backup Before Bake.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BakedLighting] Auto-backup before bake failed: {e.Message}");
        }
    }

    [MenuItem("MurderAndLove/Lighting/Reset Reflection Probes To Baked Mode")]
    static void ResetReflectionProbesToBaked()
    {
        ReflectionProbe[] probes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int changed = 0;
        for (int i = 0; i < probes.Length; i++)
        {
            ReflectionProbe probe = probes[i];
            if (probe == null || probe.mode == ReflectionProbeMode.Baked)
                continue;

            Undo.RecordObject(probe, "Reset Reflection Probe To Baked");
            probe.mode = ReflectionProbeMode.Baked;
            probe.customBakedTexture = null;
            EditorUtility.SetDirty(probe);
            changed++;
        }

        Debug.Log($"[BakedLighting] Reset {changed} ReflectionProbe(s) to Baked mode. Rebake probes if needed.");
    }

    [MenuItem("MurderAndLove/Lighting/Auto-Backup Before Bake", false, 100)]
    static void ToggleAutoBackup()
    {
        bool next = !EditorPrefs.GetBool("MAL_AutoBackupLightmapsBeforeBake", true);
        EditorPrefs.SetBool("MAL_AutoBackupLightmapsBeforeBake", next);
        Debug.Log($"[BakedLighting] Auto-backup before bake: {(next ? "ON" : "OFF")}");
    }

    [MenuItem("MurderAndLove/Lighting/Auto-Backup Before Bake", true)]
    static bool ToggleAutoBackupValidate()
    {
        Menu.SetChecked(
            "MurderAndLove/Lighting/Auto-Backup Before Bake",
            EditorPrefs.GetBool("MAL_AutoBackupLightmapsBeforeBake", true));
        return true;
    }

    [MenuItem("MurderAndLove/Lighting/Capture Scene Bake Into New Scenario")]
    public static void CaptureIntoNewScenario()
    {
        if (!HasCapturableData())
        {
            EditorUtility.DisplayDialog(
                "Nothing To Capture",
                "Bake the scene first (Window → Rendering → Lighting → Generate Lighting), then capture.",
                "OK");
            return;
        }

        EnsureFolder(DefaultFolder);
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Baked Lighting Scenario",
            "BakedLightingScenario",
            "asset",
            "Choose where to save the captured lighting scenario.",
            DefaultFolder);

        if (string.IsNullOrEmpty(path))
            return;

        var scenario = ScriptableObject.CreateInstance<BakedLightingScenario>();
        AssetDatabase.CreateAsset(scenario, path);
        CaptureInto(scenario, path, clearPreviousCopies: true);
        EditorUtility.SetDirty(scenario);
        FinishCapture(scenario, path);
    }

    public static void CaptureIntoExisting(BakedLightingScenario scenario)
    {
        if (scenario == null)
        {
            Debug.LogError("BakedLightingScenarioCapture: scenario is null.");
            return;
        }

        if (!HasCapturableData())
        {
            EditorUtility.DisplayDialog(
                "Nothing To Capture",
                "Bake the scene first, then capture into this scenario.",
                "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(scenario);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("BakedLightingScenarioCapture: scenario must be a saved asset.");
            return;
        }

        CaptureInto(scenario, path, clearPreviousCopies: true);
        EditorUtility.SetDirty(scenario);
        FinishCapture(scenario, path);
    }

    /// <summary>
    /// Updates only <see cref="BakedLightingScenario.bakedProbes"/> from the current scene bake.
    /// Leaves lightmaps / LightingData / environment on the asset untouched.
    /// Use after a blackout (or lit) rebake when lightmaps are already correct.
    /// </summary>
    public static void CaptureLightProbesOnly(BakedLightingScenario scenario)
    {
        if (scenario == null)
        {
            Debug.LogError("BakedLightingScenarioCapture: scenario is null.");
            return;
        }

        LightProbes probes = LightmapSettings.lightProbes;
        if (probes == null || probes.bakedProbes == null || probes.bakedProbes.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No Light Probes",
                "Scene has no baked light probe data.\nBake lighting first (Generate Lighting), then capture probes.",
                "OK");
            return;
        }

        Undo.RecordObject(scenario, "Capture Light Probes Only");
        CaptureLightProbes(scenario);
        EditorUtility.SetDirty(scenario);
        AssetDatabase.SaveAssets();

        float energy = AverageProbeEnergy(scenario.bakedProbes);
        EditorUtility.DisplayDialog(
            "Light Probes Captured",
            $"Wrote {scenario.LightProbeCount} probes into '{scenario.name}'.\n" +
            $"Average probe energy: {energy:F4}\n\n" +
            "Lightmaps / LightingData on this asset were not changed.",
            "OK");
    }

    /// <summary>
    /// Logs a quick energy comparison so you can tell if two scenarios actually differ.
    /// </summary>
    public static void CompareProbeEnergy(BakedLightingScenario a, BakedLightingScenario b)
    {
        if (a == null || b == null)
        {
            Debug.LogWarning("[BakedLighting] Assign both scenarios before comparing probes.");
            return;
        }

        float ea = AverageProbeEnergy(a.bakedProbes);
        float eb = AverageProbeEnergy(b.bakedProbes);
        int ca = a.LightProbeCount;
        int cb = b.LightProbeCount;
        bool identical = ca == cb && ca > 0 && ProbesApproximatelyEqual(a.bakedProbes, b.bakedProbes);

        Debug.Log(
            $"[BakedLighting] Probe compare:\n" +
            $"  '{a.name}': count={ca}, avg energy={ea:F4}\n" +
            $"  '{b.name}': count={cb}, avg energy={eb:F4}\n" +
            $"  Approximately identical: {identical}");

        EditorUtility.DisplayDialog(
            "Probe Compare",
            $"'{a.name}': {ca} probes, avg energy {ea:F4}\n" +
            $"'{b.name}': {cb} probes, avg energy {eb:F4}\n\n" +
            (identical
                ? "These look IDENTICAL — rebake blackout with lights off, then Capture Probes Only."
                : "These differ — probe swap should be visible on Blend Probes meshes."),
            "OK");
    }

    static float AverageProbeEnergy(SphericalHarmonicsL2[] probes)
    {
        if (probes == null || probes.Length == 0)
            return 0f;

        double sum = 0;
        for (int i = 0; i < probes.Length; i++)
        {
            // DC term of SH ≈ ambient intensity contribution.
            Color c = new(
                probes[i][0, 0],
                probes[i][1, 0],
                probes[i][2, 0],
                1f);
            sum += c.r + c.g + c.b;
        }

        return (float)(sum / probes.Length);
    }

    static bool ProbesApproximatelyEqual(SphericalHarmonicsL2[] a, SphericalHarmonicsL2[] b, float epsilon = 1e-4f)
    {
        if (a == null || b == null || a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            for (int rgb = 0; rgb < 3; rgb++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (Mathf.Abs(a[i][rgb, c] - b[i][rgb, c]) > epsilon)
                        return false;
                }
            }
        }

        return true;
    }

    static bool HasCapturableData()
    {
        if (Lightmapping.lightingDataAsset != null)
            return true;

        if (HasSceneLightmaps())
            return true;

        LightProbes probes = LightmapSettings.lightProbes;
        if (probes != null && probes.count > 0)
            return true;

        return UnityEngine.Object.FindObjectsByType<ReflectionProbe>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None).Length > 0;
    }

    static bool HasSceneLightmaps()
    {
        LightmapData[] maps = LightmapSettings.lightmaps;
        if (maps == null || maps.Length == 0)
            return false;

        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] != null && maps[i].lightmapColor != null)
                return true;
        }

        return false;
    }

    static void CaptureInto(BakedLightingScenario scenario, string scenarioAssetPath, bool clearPreviousCopies)
    {
        string textureFolder = GetTextureFolder(scenarioAssetPath);
        EnsureFolder(textureFolder);

        if (clearPreviousCopies)
        {
            scenario.lightingDataAsset = null;
            ClearFolderAssets(textureFolder);
        }

        string scenarioName = Path.GetFileNameWithoutExtension(scenarioAssetPath);
        int copied = 0;
        int failed = 0;
        var remaps = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

        CaptureLightmaps(scenario, textureFolder, scenarioName, remaps, ref copied, ref failed);
        CaptureLightProbes(scenario);
        // Reflection probes are not captured/swapped — bake separate probes and toggle via object lists.
        scenario.applyReflectionProbes = false;
        scenario.reflectionProbes = Array.Empty<BakedLightingScenario.ReflectionProbeEntry>();
        CaptureEnvironment(scenario, textureFolder, scenarioName, remaps, ref copied, ref failed);
        CaptureLightingDataAsset(scenario, textureFolder, scenarioName, remaps);

        if (failed > 0)
        {
            Debug.LogError(
                $"[BakedLighting] Capture finished with {failed} failed texture copies. " +
                "Do NOT rebake until this is fixed.");
        }
        else if (scenario.HasLightmaps && !ValidateLightmapsCopied(scenario, textureFolder))
        {
            Debug.LogError(
                "[BakedLighting] Scenario still references scene lightmap files. Capture failed. Do not rebake yet.");
        }
        else
        {
            Debug.Log(
                $"[BakedLighting] Captured {copied} texture(s), " +
                $"{scenario.LightProbeCount} light probes, " +
                $"LightingData={(scenario.lightingDataAsset != null ? "yes" : "no")} → '{textureFolder}'.");
        }
    }

    static void CaptureLightmaps(
        BakedLightingScenario scenario,
        string textureFolder,
        string scenarioName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps,
        ref int copied,
        ref int failed)
    {
        LightmapData[] source = LightmapSettings.lightmaps;
        scenario.lightmapsMode = LightmapSettings.lightmapsMode;
        scenario.lightmaps = new BakedLightingScenario.LightmapEntry[source != null ? source.Length : 0];

        for (int i = 0; i < scenario.lightmaps.Length; i++)
        {
            LightmapData data = source[i];
            scenario.lightmaps[i] = new BakedLightingScenario.LightmapEntry
            {
                lightmapColor = DuplicateTexture2D(data.lightmapColor, textureFolder, $"{scenarioName}_LM{i}_color", remaps, ref copied, ref failed),
                lightmapDir = DuplicateTexture2D(data.lightmapDir, textureFolder, $"{scenarioName}_LM{i}_dir", remaps, ref copied, ref failed),
                shadowMask = DuplicateTexture2D(data.shadowMask, textureFolder, $"{scenarioName}_LM{i}_shadowmask", remaps, ref copied, ref failed)
            };
        }
    }

    static void CaptureLightProbes(BakedLightingScenario scenario)
    {
        LightProbes probes = LightmapSettings.lightProbes;
        if (probes != null && probes.bakedProbes != null && probes.bakedProbes.Length > 0)
        {
            scenario.bakedProbes = (SphericalHarmonicsL2[])probes.bakedProbes.Clone();
            Debug.Log($"[BakedLighting] Captured {scenario.bakedProbes.Length} light probe SH coefficients.");
        }
        else
        {
            scenario.bakedProbes = Array.Empty<SphericalHarmonicsL2>();
            Debug.LogWarning("[BakedLighting] No light probe data in the scene to capture.");
        }
    }

    static void CaptureEnvironment(
        BakedLightingScenario scenario,
        string textureFolder,
        string scenarioName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps,
        ref int copied,
        ref int failed)
    {
        scenario.CopyEnvironmentFromScene();
        scenario.applyAmbient = true;

        // CopyEnvironmentFromScene already read this safely; duplicate only if it's a real cubemap.
        Cubemap envReflection = scenario.customReflection as Cubemap;
        if (envReflection == null)
            envReflection = BakedLightingScenario.TryGetCustomReflection();

        if (envReflection != null)
        {
            scenario.customReflection = DuplicateTextureAsset(
                envReflection,
                textureFolder,
                $"{scenarioName}_EnvReflection",
                remaps,
                ref copied,
                ref failed);
        }
        else
        {
            scenario.customReflection = null;
        }
    }

    /// <summary>
    /// Copies LightingData.asset and rewrites its texture references to the duplicated copies
    /// so a later scene rebake cannot destroy this snapshot.
    /// </summary>
    static void CaptureLightingDataAsset(
        BakedLightingScenario scenario,
        string textureFolder,
        string scenarioName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps)
    {
        scenario.lightingDataAsset = null;

        LightingDataAsset source = Lightmapping.lightingDataAsset;
        if (source == null)
        {
            Debug.LogWarning("[BakedLighting] No LightingData.asset assigned on the Lighting window.");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.LogError("[BakedLighting] LightingData.asset has no asset path.");
            return;
        }

        string destPath = $"{textureFolder}/{scenarioName}_LightingData.asset";
        destPath = AssetDatabase.GenerateUniqueAssetPath(destPath);

        if (!AssetDatabase.CopyAsset(sourcePath, destPath))
        {
            Debug.LogError($"[BakedLighting] Failed to copy LightingData.asset from '{sourcePath}'.");
            return;
        }

        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
        LightingDataAsset copy = AssetDatabase.LoadAssetAtPath<LightingDataAsset>(destPath);
        if (copy == null)
        {
            Debug.LogError($"[BakedLighting] Could not load copied LightingData at '{destPath}'.");
            return;
        }

        int remapped = RemapObjectReferences(copy, remaps);
        // Also remap references on any sub-assets (e.g. embedded LightProbes).
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(destPath);
        for (int i = 0; i < subAssets.Length; i++)
        {
            if (subAssets[i] == null || subAssets[i] == copy)
                continue;
            remapped += RemapObjectReferences(subAssets[i], remaps);
        }

        EditorUtility.SetDirty(copy);
        AssetDatabase.SaveAssets();

        scenario.lightingDataAsset = copy;
        Debug.Log(
            $"[BakedLighting] Captured LightingData.asset → '{destPath}' " +
            $"(remapped {remapped} object reference(s)).");
    }

    static int RemapObjectReferences(
        UnityEngine.Object asset,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps)
    {
        if (asset == null || remaps == null || remaps.Count == 0)
            return 0;

        var so = new SerializedObject(asset);
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;
        int count = 0;

        while (prop.Next(enterChildren))
        {
            enterChildren = true;
            if (prop.propertyType != SerializedPropertyType.ObjectReference)
                continue;

            UnityEngine.Object current = prop.objectReferenceValue;
            if (current == null)
                continue;

            if (!remaps.TryGetValue(current, out UnityEngine.Object replacement) || replacement == null)
                continue;

            prop.objectReferenceValue = replacement;
            count++;
        }

        if (count > 0)
            so.ApplyModifiedPropertiesWithoutUndo();

        return count;
    }

    static string SanitizeFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "probe";

        char[] invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(path.Length);
        for (int i = 0; i < path.Length; i++)
        {
            char c = path[i];
            if (c == '/' || c == '\\')
                sb.Append('_');
            else if (Array.IndexOf(invalid, c) >= 0)
                sb.Append('_');
            else
                sb.Append(c);
        }

        string result = sb.ToString();
        if (result.Length > 80)
            result = result.Substring(result.Length - 80);
        return result;
    }

    static Texture2D DuplicateTexture2D(
        Texture2D source,
        string folder,
        string fileName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps,
        ref int copied,
        ref int failed)
    {
        return DuplicateTextureAsset(source, folder, fileName, remaps, ref copied, ref failed) as Texture2D;
    }

    /// <summary>
    /// Returns a NEW texture asset under <paramref name="folder"/>. Never returns the source.
    /// </summary>
    static Texture DuplicateTextureAsset(
        Texture source,
        string folder,
        string fileName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps,
        ref int copied,
        ref int failed)
    {
        if (source == null)
            return null;

        if (remaps != null && remaps.TryGetValue(source, out UnityEngine.Object existing) && existing is Texture existingTex)
            return existingTex;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.LogError($"[BakedLighting] No asset path for '{source.name}'. Cannot copy.");
            failed++;
            return null;
        }

        Texture result = null;

        if (source is Texture2D tex2D && (AssetDatabase.IsSubAsset(source) || !File.Exists(GetFullPath(sourcePath))))
        {
            Texture2D extracted = ExtractTexture2DAsset(tex2D, folder, fileName);
            if (extracted != null)
            {
                copied++;
                result = extracted;
            }
            else
            {
                failed++;
                return null;
            }
        }
        else
        {
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension))
                extension = ".exr";

            string destPath = $"{folder}/{fileName}{extension}";
            destPath = AssetDatabase.GenerateUniqueAssetPath(destPath);

            string fullSource = GetFullPath(sourcePath);
            string fullDest = GetFullPath(destPath);

            try
            {
                if (File.Exists(fullSource))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullDest) ?? string.Empty);
                    if (File.Exists(fullDest))
                        File.Delete(fullDest);
                    if (File.Exists(fullDest + ".meta"))
                        File.Delete(fullDest + ".meta");

                    File.Copy(fullSource, fullDest, overwrite: false);
                    AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
                    CopyTextureImporterSettings(sourcePath, destPath);

                    Texture copy = LoadTextureAtPath(destPath);
                    if (copy == null)
                    {
                        Debug.LogError($"[BakedLighting] Import failed for '{destPath}'.");
                        failed++;
                        return null;
                    }

                    if (AssetDatabase.AssetPathToGUID(destPath) == AssetDatabase.AssetPathToGUID(sourcePath))
                    {
                        Debug.LogError($"[BakedLighting] Copy still shares GUID with '{sourcePath}'.");
                        failed++;
                        return null;
                    }

                    copied++;
                    result = copy;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BakedLighting] File copy failed ({e.Message}), trying AssetDatabase.CopyAsset…");
            }

            if (result == null && AssetDatabase.CopyAsset(sourcePath, destPath))
            {
                Texture copy = LoadTextureAtPath(destPath);
                if (copy != null && AssetDatabase.AssetPathToGUID(destPath) != AssetDatabase.AssetPathToGUID(sourcePath))
                {
                    copied++;
                    result = copy;
                }
            }

            if (result == null)
            {
                Debug.LogError($"[BakedLighting] Failed to duplicate '{sourcePath}'.");
                failed++;
                return null;
            }
        }

        if (remaps != null && result != null)
            remaps[source] = result;

        return result;
    }

    static Texture LoadTextureAtPath(string assetPath)
    {
        Texture tex = AssetDatabase.LoadAssetAtPath<Cubemap>(assetPath);
        if (tex != null)
            return tex;

        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex != null)
            return tex;

        return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
    }

    static Texture2D ExtractTexture2DAsset(Texture2D source, string folder, string fileName)
    {
        RenderTexture rt = null;
        Texture2D readable = null;
        try
        {
            int w = source.width;
            int h = source.height;
            bool hdr = IsHdrLightmap(source);

            rt = RenderTexture.GetTemporary(w, h, 0, hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            readable = new Texture2D(w, h, hdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, false, true);
            readable.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            readable.Apply(false, false);
            RenderTexture.active = prev;

            string destPath = $"{folder}/{fileName}{(hdr ? ".exr" : ".png")}";
            destPath = AssetDatabase.GenerateUniqueAssetPath(destPath);
            string fullDest = GetFullPath(destPath);

            if (hdr)
                File.WriteAllBytes(fullDest, readable.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
            else
                File.WriteAllBytes(fullDest, readable.EncodeToPNG());

            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
            ApplyLightmapImporterSettings(destPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[BakedLighting] ExtractTexture2DAsset failed: {e.Message}");
            return null;
        }
        finally
        {
            if (rt != null)
                RenderTexture.ReleaseTemporary(rt);
            if (readable != null)
                UnityEngine.Object.DestroyImmediate(readable);
        }
    }

    static bool IsHdrLightmap(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (!string.IsNullOrEmpty(path) && path.EndsWith(".exr", StringComparison.OrdinalIgnoreCase))
            return true;

        return tex.format == TextureFormat.RGBAHalf
               || tex.format == TextureFormat.RGBAFloat
               || tex.format == TextureFormat.BC6H;
    }

    static void ApplyLightmapImporterSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Lightmap;
        importer.sRGBTexture = false;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    static void CopyTextureImporterSettings(string sourcePath, string destPath)
    {
        var sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        var destImporter = AssetImporter.GetAtPath(destPath) as TextureImporter;
        if (destImporter == null)
            return;

        if (sourceImporter != null)
        {
            destImporter.textureType = sourceImporter.textureType;
            destImporter.textureShape = sourceImporter.textureShape;
            destImporter.sRGBTexture = sourceImporter.sRGBTexture;
            destImporter.mipmapEnabled = sourceImporter.mipmapEnabled;
            destImporter.wrapMode = sourceImporter.wrapMode;
            destImporter.filterMode = sourceImporter.filterMode;
            destImporter.anisoLevel = sourceImporter.anisoLevel;
            destImporter.maxTextureSize = sourceImporter.maxTextureSize;
            destImporter.textureCompression = sourceImporter.textureCompression;
        }
        else
        {
            destImporter.textureType = TextureImporterType.Default;
            destImporter.sRGBTexture = false;
            destImporter.mipmapEnabled = false;
        }

        destImporter.SaveAndReimport();
    }

    static bool ValidateLightmapsCopied(BakedLightingScenario scenario, string textureFolder)
    {
        if (scenario.lightmaps == null)
            return false;

        textureFolder = textureFolder.Replace('\\', '/');

        for (int i = 0; i < scenario.lightmaps.Length; i++)
        {
            BakedLightingScenario.LightmapEntry entry = scenario.lightmaps[i];
            if (entry == null)
                continue;

            if (!IsUnderFolder(entry.lightmapColor, textureFolder))
                return false;
            if (entry.lightmapDir != null && !IsUnderFolder(entry.lightmapDir, textureFolder))
                return false;
            if (entry.shadowMask != null && !IsUnderFolder(entry.shadowMask, textureFolder))
                return false;
        }

        return scenario.HasLightmaps;
    }

    static bool IsUnderFolder(Texture tex, string folder)
    {
        if (tex == null)
            return true;

        string path = AssetDatabase.GetAssetPath(tex)?.Replace('\\', '/');
        return !string.IsNullOrEmpty(path) && path.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase);
    }

    static void ClearFolderAssets(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!string.IsNullOrEmpty(path) && path != folder)
                AssetDatabase.DeleteAsset(path);
        }
    }

    static string GetTextureFolder(string scenarioAssetPath)
    {
        string dir = Path.GetDirectoryName(scenarioAssetPath)?.Replace('\\', '/');
        string name = Path.GetFileNameWithoutExtension(scenarioAssetPath);
        return $"{dir}/{name}_Lightmaps";
    }

    static string GetFullPath(string assetPath)
    {
        assetPath = assetPath.Replace('\\', '/');
        if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        return Path.GetFullPath(assetPath);
    }

    static void EnsureFolder(string folder)
    {
        folder = folder.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void FinishCapture(BakedLightingScenario scenario, string path)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(scenario);
        Debug.Log(
            $"[BakedLighting] Captured bake into '{path}'\n" +
            $"  LightingData: {(scenario.lightingDataAsset != null ? scenario.lightingDataAsset.name : "none")}\n" +
            $"  Lightmaps: {scenario.LightmapCount}\n" +
            $"  Light probes: {scenario.LightProbeCount}\n" +
            "Safe to Generate Lighting again — this scenario keeps its own copies.\n" +
            "Reflection probes are not swapped; bake separate probes and toggle objects on the controller.");
    }
}

[CustomEditor(typeof(BakedLightingScenario))]
public class BakedLightingScenarioEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        var scenario = (BakedLightingScenario)target;

        if (GUILayout.Button("Capture Current Scene Bake Into This Asset"))
        {
            if (EditorUtility.DisplayDialog(
                    "Overwrite Scenario?",
                    "Copies lightmaps, light probes, and LightingData into this asset.\n" +
                    "After this succeeds, you can rebake safely.",
                    "Capture",
                    "Cancel"))
            {
                BakedLightingScenarioCapture.CaptureIntoExisting(scenario);
            }
        }

        if (GUILayout.Button("Capture Light Probes Only (keep lightmaps)"))
        {
            BakedLightingScenarioCapture.CaptureLightProbesOnly(scenario);
        }

        EditorGUILayout.HelpBox(
            "Full capture: LightingData + lightmaps + light probes.\n" +
            "Probes-only: refreshes bakedProbes after a rebake without touching stored lightmaps.\n" +
            "Unity cannot bake probes alone — Generate Lighting is required, then use Probes Only here.\n\n" +
            "Reflection probes are NOT swapped — toggle separate baked probes via BakedLightingController lists.",
            MessageType.Info);
    }
}

[CustomEditor(typeof(BakedLightingController))]
public class BakedLightingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var controller = (BakedLightingController)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Capture / Preview", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Capture stores LightingData + lightmaps + light probes (not reflection probes).\n" +
            "Toggle separate baked reflection probe objects via Realtime Lights / Objects lists.",
            MessageType.Warning);

        using (new EditorGUI.DisabledScope(controller.LightsOnScenario == null))
        {
            if (GUILayout.Button("Capture Bake → Lights On Scenario"))
                BakedLightingScenarioCapture.CaptureIntoExisting(controller.LightsOnScenario);
        }

        using (new EditorGUI.DisabledScope(controller.BlackoutScenario == null))
        {
            if (GUILayout.Button("Capture Bake → Blackout Scenario"))
                BakedLightingScenarioCapture.CaptureIntoExisting(controller.BlackoutScenario);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Light Probes Only", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(controller.LightsOnScenario == null))
            {
                if (GUILayout.Button("Capture Probes → Lights On"))
                    BakedLightingScenarioCapture.CaptureLightProbesOnly(controller.LightsOnScenario);
            }

            using (new EditorGUI.DisabledScope(controller.BlackoutScenario == null))
            {
                if (GUILayout.Button("Capture Probes → Blackout"))
                    BakedLightingScenarioCapture.CaptureLightProbesOnly(controller.BlackoutScenario);
            }
        }

        using (new EditorGUI.DisabledScope(
                   controller.LightsOnScenario == null || controller.BlackoutScenario == null))
        {
            if (GUILayout.Button("Compare Probe Energy (Lights On vs Blackout)"))
                BakedLightingScenarioCapture.CompareProbeEnergy(
                    controller.LightsOnScenario,
                    controller.BlackoutScenario);
        }

        EditorGUILayout.HelpBox(
            "Unity cannot bake light probes alone — Generate Lighting always rebakes lightmaps too.\n" +
            "Your scenario assets keep their copied lightmaps; use Capture Probes Only after a blackout bake.",
            MessageType.Info);

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Lights On"))
            {
                Undo.RegisterCompleteObjectUndo(controller.gameObject, "Preview Lights On");
                controller.ApplyState(BakedLightingController.LightingState.LightsOn, immediate: true);
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Preview Blackout"))
            {
                Undo.RegisterCompleteObjectUndo(controller.gameObject, "Preview Blackout");
                controller.ApplyState(BakedLightingController.LightingState.Blackout, immediate: true);
                EditorUtility.SetDirty(controller);
            }
        }

        EditorGUILayout.HelpBox(
            "Preview applies the full state: LightingData / lightmaps / probes AND the Realtime Lights / Objects lists.",
            MessageType.Info);
    }
}
