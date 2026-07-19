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
        CaptureReflectionProbes(scenario, textureFolder, scenarioName, remaps, ref copied, ref failed);
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
                $"{scenario.ReflectionProbeCount} reflection probes, " +
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

    static void CaptureReflectionProbes(
        BakedLightingScenario scenario,
        string textureFolder,
        string scenarioName,
        Dictionary<UnityEngine.Object, UnityEngine.Object> remaps,
        ref int copied,
        ref int failed)
    {
        ReflectionProbe[] sceneProbes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        scenario.applyReflectionProbes = true;
        scenario.reflectionProbes = new BakedLightingScenario.ReflectionProbeEntry[sceneProbes.Length];

        for (int i = 0; i < sceneProbes.Length; i++)
        {
            ReflectionProbe probe = sceneProbes[i];
            if (probe == null)
                continue;

            string hierarchyPath = BakedLightingScenario.GetHierarchyPath(probe.transform);
            Texture sourceTex = GetReflectionProbeTexture(probe);
            string safeName = SanitizeFileName(hierarchyPath);

            scenario.reflectionProbes[i] = new BakedLightingScenario.ReflectionProbeEntry
            {
                hierarchyPath = hierarchyPath,
                cubemap = DuplicateTextureAsset(sourceTex, textureFolder, $"{scenarioName}_RP{i}_{safeName}", remaps, ref copied, ref failed),
                intensity = probe.intensity,
                boxProjection = probe.boxProjection,
                size = probe.size,
                center = probe.center,
                blendDistance = probe.blendDistance,
                importance = probe.importance
            };
        }

        if (sceneProbes.Length == 0)
            Debug.LogWarning("[BakedLighting] No ReflectionProbes found in the scene.");
        else
            Debug.Log($"[BakedLighting] Captured {sceneProbes.Length} reflection probe(s).");
    }

    static Texture GetReflectionProbeTexture(ReflectionProbe probe)
    {
        if (probe.mode == ReflectionProbeMode.Custom && probe.customBakedTexture != null)
            return probe.customBakedTexture;
        if (probe.bakedTexture != null)
            return probe.bakedTexture;
        return probe.texture;
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
            $"  Reflection probes: {scenario.ReflectionProbeCount}\n" +
            "Safe to Generate Lighting again — this scenario keeps its own copies.");
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
                    "Copies lightmaps, light probes, and reflection probes into this asset.\n" +
                    "After this succeeds, you can rebake safely.",
                    "Capture",
                    "Cancel"))
            {
                BakedLightingScenarioCapture.CaptureIntoExisting(scenario);
            }
        }

        EditorGUILayout.HelpBox(
            "Captures:\n" +
            "• LightingData.asset (copied + remapped to snapshot textures)\n" +
            "• Lightmaps (copied textures)\n" +
            "• Light probe SH coefficients\n" +
            "• Reflection probe cubemaps\n" +
            "• Ambient / default reflection\n\n" +
            "Bake → Capture → change lights → Bake → Capture.",
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
            "Capture stores LightingData.asset + lightmaps + probes. Do this after each bake, before the next.",
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

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Lights On"))
            {
                if (controller.LightsOnScenario != null)
                    BakedLightingController.ApplyScenario(controller.LightsOnScenario);
                else
                    Debug.LogWarning("No Lights On scenario assigned.");
            }

            if (GUILayout.Button("Preview Blackout"))
            {
                if (controller.BlackoutScenario != null)
                    BakedLightingController.ApplyScenario(controller.BlackoutScenario);
                else
                    Debug.LogWarning("No Blackout scenario assigned.");
            }
        }
    }
}
