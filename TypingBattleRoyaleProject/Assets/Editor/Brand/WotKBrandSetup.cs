#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using BrandPalette = WotK.Brand.WotKBrand;

namespace WotK.Brand.EditorTools
{
    public static class WotKBrandSetup
    {
        const string FontsFolder = "Assets/Artist/Brand/Fonts";
        const string BoldTtfPath = "Assets/Fonts/gontserrat/Gontserrat-Bold.ttf";
        const string RegularTtfPath = "Assets/Fonts/gontserrat/Gontserrat-Regular.ttf";
        const string BoldAssetPath = FontsFolder + "/Gontserrat-Bold SDF.asset";
        const string RegularAssetPath = FontsFolder + "/Gontserrat-Regular SDF.asset";

        const string LiberationSansGuid = "8f586378b4e144a9851e7b34d9b748ee";

        static readonly string[] BrandScenePaths =
        {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/LobbyScene.unity",
            "Assets/Scenes/CharacterSelect.unity",
            "Assets/Scenes/GameplayScene.unity",
        };

        [MenuItem("Tools/WotK/Setup Brand Fonts")]
        public static void SetupBrandFonts()
        {
            if (!Directory.Exists(FontsFolder))
            {
                Directory.CreateDirectory(FontsFolder);
                AssetDatabase.Refresh();
            }

            var boldFont = LoadOrCreateFontAsset(BoldTtfPath, BoldAssetPath, "Gontserrat-Bold SDF");
            var regularFont = LoadOrCreateFontAsset(RegularTtfPath, RegularAssetPath, "Gontserrat-Regular SDF");

            if (boldFont == null || regularFont == null)
            {
                EditorUtility.DisplayDialog("WotK Brand Setup",
                    "Could not create Gontserrat font assets. Verify that the TTFs exist at:\n" +
                    BoldTtfPath + "\n" + RegularTtfPath, "OK");
                return;
            }

            int swapped = SwapFontsInScenes(boldFont, regularFont);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("WotK Brand Setup",
                $"Brand fonts ready.\n\n" +
                $"Bold: {BoldAssetPath}\n" +
                $"Regular: {RegularAssetPath}\n\n" +
                $"Swapped {swapped} TMP text components in brand scenes.",
                "Done");
        }

        [MenuItem("Tools/WotK/Generate Font Assets Only")]
        public static void GenerateFontAssetsOnly()
        {
            if (!Directory.Exists(FontsFolder))
            {
                Directory.CreateDirectory(FontsFolder);
                AssetDatabase.Refresh();
            }

            LoadOrCreateFontAsset(BoldTtfPath, BoldAssetPath, "Gontserrat-Bold SDF");
            LoadOrCreateFontAsset(RegularTtfPath, RegularAssetPath, "Gontserrat-Regular SDF");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static TMP_FontAsset LoadOrCreateFontAsset(string ttfPath, string assetPath, string assetName)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var ttf = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (ttf == null)
            {
                Debug.LogError($"[WotKBrandSetup] TTF not found at {ttfPath}");
                return null;
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                ttf,
                samplingPointSize: 90,
                atlasPadding: 9,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            fontAsset.name = assetName;
            AssetDatabase.CreateAsset(fontAsset, assetPath);
            return fontAsset;
        }

        static int SwapFontsInScenes(TMP_FontAsset bold, TMP_FontAsset regular)
        {
            int totalSwapped = 0;
            var originalScene = EditorSceneManager.GetActiveScene().path;

            foreach (var scenePath in BrandScenePaths)
            {
                if (!File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int swapped = SwapFontsInOpenScene(bold, regular);
                totalSwapped += swapped;

                if (swapped > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrEmpty(originalScene))
            {
                EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            }

            return totalSwapped;
        }

        static int SwapFontsInOpenScene(TMP_FontAsset bold, TMP_FontAsset regular)
        {
            int swapped = 0;
            var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp in texts)
            {
                if (tmp.font == null) continue;

                var sourceGuid = GetFontAssetGuid(tmp.font);
                bool isLiberationSans = sourceGuid == LiberationSansGuid;
                bool isAlreadyBrand = tmp.font == bold || tmp.font == regular;
                bool retarget = isLiberationSans || isAlreadyBrand || ShouldRetarget(tmp.font);
                if (!retarget) continue;

                var target = ChooseVariant(tmp, bold, regular);
                if (tmp.font != target)
                {
                    tmp.font = target;
                    EditorUtility.SetDirty(tmp);
                    swapped++;
                }

                AlignTextColor(tmp);
            }
            return swapped;
        }

        static void AlignTextColor(TMP_Text tmp)
        {
            if (IsPlaceholder(tmp)) return;

            Color current = tmp.color;
            // Preserve custom colors set by scripts (red status, green success, etc.)
            if (!IsApproximately(current, Color.white) && !IsApproximately(current, Color.black)) return;

            Color target = ParentHasLightBackground(tmp) ? BrandPalette.Black : BrandPalette.White;
            if (!IsApproximately(current, target))
            {
                tmp.color = target;
                EditorUtility.SetDirty(tmp);
            }
        }

        static bool IsPlaceholder(TMP_Text tmp)
        {
            var name = tmp.gameObject.name;
            return name != null && name.IndexOf("Placeholder", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool ParentHasLightBackground(TMP_Text tmp)
        {
            var t = tmp.transform.parent;
            while (t != null)
            {
                var img = t.GetComponent<UnityEngine.UI.Image>();
                if (img != null && img.enabled && img.color.a > 0.5f)
                {
                    Color c = img.color;
                    float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
                    return lum > 0.6f;
                }
                t = t.parent;
            }
            return false;
        }

        static bool IsApproximately(Color a, Color b, float tol = 0.02f)
        {
            return Mathf.Abs(a.r - b.r) < tol && Mathf.Abs(a.g - b.g) < tol && Mathf.Abs(a.b - b.b) < tol;
        }

        static bool ShouldRetarget(TMP_FontAsset font)
        {
            return font != null && font.name.IndexOf("LiberationSans", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static TMP_FontAsset ChooseVariant(TMP_Text tmp, TMP_FontAsset bold, TMP_FontAsset regular)
        {
            bool wantsBold = (tmp.fontStyle & FontStyles.Bold) != 0 || tmp.fontSize >= 30f;
            return wantsBold ? bold : regular;
        }

        static string GetFontAssetGuid(TMP_FontAsset font)
        {
            var path = AssetDatabase.GetAssetPath(font);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.AssetPathToGUID(path);
        }
    }
}
#endif
