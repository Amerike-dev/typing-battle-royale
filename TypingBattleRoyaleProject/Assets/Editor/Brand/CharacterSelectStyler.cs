#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterSelectStyler
{
    const string ScenePath = "Assets/Scenes/CharacterSelect.unity";
    const string BoldFontPath = "Assets/Artist/Brand/Fonts/Gontserrat-Bold SDF.asset";
    const string RegularFontPath = "Assets/Artist/Brand/Fonts/Gontserrat-Regular SDF.asset";

    static readonly Color BgDark = new Color32(0x07, 0x05, 0x04, 0xFF);
    static readonly Color CardColor = new Color32(0x18, 0x16, 0x16, 0xFF);
    static readonly Color CardAccent = new Color32(0x48, 0x5C, 0xC7, 0xFF);
    static readonly Color CTA = new Color32(0xFA, 0x46, 0x16, 0xFF);
    static readonly Color TextPrimary = Color.white;
    static readonly Color TextMuted = new Color32(0xB0, 0xB0, 0xB0, 0xFF);

    [MenuItem("Tools/TBR/Style CharacterSelect")]
    public static void Apply()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (!EditorUtility.DisplayDialog("CharacterSelect Styler",
                "La escena actual tiene cambios sin guardar. Se descartaran al abrir CharacterSelect.unity. Continuar?",
                "Continuar", "Cancelar")) return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        StyleBackground(roots);
        StyleLogo(roots);
        StyleHeader(roots);
        StylePlayerSlots(roots);
        StyleContainer(roots);
        StyleArrowsContainer(roots);
        StyleConfirmButton(roots);
        HideClutter(roots);
        ApplyFonts(roots);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TBR] CharacterSelect estilizado.");
    }

    static void StyleBackground(GameObject[] roots)
    {
        var go = FindByName(roots, "Background");
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt != null) Stretch(rt);
        var img = go.GetComponent<Image>();
        if (img != null) img.color = BgDark;
    }

    static void StyleLogo(GameObject[] roots)
    {
        var go = FindByName(roots, "WotKLogo");
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -20f);
            rt.sizeDelta = new Vector2(220f, 70f);
        }
        var img = go.GetComponent<Image>();
        if (img != null) img.preserveAspect = true;
    }

    static void StyleHeader(GameObject[] roots)
    {
        var go = FindByName(roots, "Text (TMP)");
        if (go == null) return;
        go.SetActive(true);
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -110f);
            rt.sizeDelta = new Vector2(0f, 60f);
        }
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = "ELIGE TU PERSONAJE";
            tmp.fontSize = 42f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TextPrimary;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;
        }
    }

    static void StylePlayerSlots(GameObject[] roots)
    {
        string[] names = { "Player1", "Player2", "Player3", "Player4" };
        const float widthPct = 0.20f;
        const float gap = (1f - 4f * widthPct) / 5f;

        for (int i = 0; i < names.Length; i++)
        {
            var slot = FindByName(roots, names[i]);
            if (slot == null) continue;

            var rt = slot.GetComponent<RectTransform>();
            if (rt != null)
            {
                float xMin = gap + i * (widthPct + gap);
                rt.anchorMin = new Vector2(xMin, 0.18f);
                rt.anchorMax = new Vector2(xMin + widthPct, 0.78f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            var img = slot.GetComponent<Image>();
            if (img == null) img = slot.AddComponent<Image>();
            img.color = CardColor;
            img.raycastTarget = false;

            var outlineCol = i == 0 ? CTA : CardAccent;
            var outline = slot.GetComponent<Outline>();
            if (outline == null) outline = slot.AddComponent<Outline>();
            outline.effectColor = outlineCol;
            outline.effectDistance = new Vector2(2f, -2f);

            int idx = 0;
            foreach (Transform child in slot.transform)
            {
                var tmp = child.GetComponent<TMP_Text>();
                if (tmp == null) continue;
                var crt = child.GetComponent<RectTransform>();
                bool isEstado = child.name.ToLowerInvariant().Contains("estado");

                if (crt != null)
                {
                    crt.anchorMin = new Vector2(0f, 1f);
                    crt.anchorMax = new Vector2(1f, 1f);
                    crt.pivot = new Vector2(0.5f, 1f);
                    if (isEstado)
                    {
                        crt.anchoredPosition = new Vector2(0f, -68f);
                        crt.sizeDelta = new Vector2(-20f, 34f);
                    }
                    else
                    {
                        crt.anchoredPosition = new Vector2(0f, -16f);
                        crt.sizeDelta = new Vector2(-20f, 48f);
                    }
                }
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TextOverflowModes.Truncate;
                if (isEstado)
                {
                    tmp.fontSize = 20f;
                    tmp.color = TextMuted;
                    tmp.fontStyle = FontStyles.Normal;
                }
                else
                {
                    tmp.text = $"PLAYER {i + 1}";
                    tmp.fontSize = 28f;
                    tmp.color = TextPrimary;
                    tmp.fontStyle = FontStyles.Bold;
                }
                idx++;
            }
        }
    }

    static void StyleContainer(GameObject[] roots)
    {
        var go = FindByName(roots, "Container");
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.05f, 0.22f);
            rt.anchorMax = new Vector2(0.95f, 0.62f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
        var hlg = go.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.padding = new RectOffset(30, 30, 0, 0);
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childScaleWidth = false;
            hlg.childScaleHeight = false;
        }
    }

    static void StyleArrowsContainer(GameObject[] roots)
    {
        var go = FindByName(roots, "ArrowsContainer");
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(360f, 360f);
        }

        string[] labels = { "LeftButton", "RightButton", "UpButton", "DownButton" };
        Vector2[] positions = {
            new Vector2(-130f, 0f),
            new Vector2(130f, 0f),
            new Vector2(0f, 130f),
            new Vector2(0f, -130f),
        };
        string[] glyphs = { "<", ">", "^", "v" };
        for (int i = 0; i < labels.Length; i++)
        {
            var btn = FindByName(roots, labels[i]);
            if (btn == null) continue;
            var brt = btn.GetComponent<RectTransform>();
            if (brt != null)
            {
                brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
                brt.anchoredPosition = positions[i];
                brt.sizeDelta = new Vector2(60f, 60f);
            }
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = CardAccent;
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = glyphs[i];
                label.fontSize = 32f;
                label.color = TextPrimary;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        var finish = FindByName(roots, "FinishButton");
        if (finish != null)
        {
            var frt = finish.GetComponent<RectTransform>();
            if (frt != null)
            {
                frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
                frt.anchoredPosition = new Vector2(0f, -200f);
                frt.sizeDelta = new Vector2(220f, 56f);
            }
            var img = finish.GetComponent<Image>();
            if (img != null) img.color = CTA;
            var label = finish.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "LISTO";
                label.fontSize = 24f;
                label.color = TextPrimary;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    static void StyleConfirmButton(GameObject[] roots)
    {
        var go = FindByName(roots, "ConfirmButton");
        if (go == null) return;
        go.SetActive(true);
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 40f);
            rt.sizeDelta = new Vector2(320f, 72f);
        }
        var img = go.GetComponent<Image>();
        if (img != null) img.color = CTA;
        var label = go.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "EMPEZAR PARTIDA";
            label.fontSize = 28f;
            label.color = TextPrimary;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    static void HideClutter(GameObject[] roots)
    {
        string[] toHide = { "Button", "Controller" };
        foreach (var name in toHide)
        {
            var go = FindByName(roots, name);
            if (go != null) go.SetActive(false);
        }
    }

    static void ApplyFonts(GameObject[] roots)
    {
        var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
        var regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);
        if (bold == null && regular == null) return;

        foreach (var root in roots)
        {
            var tmps = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var tmp in tmps)
            {
                bool wantsBold = (tmp.fontStyle & FontStyles.Bold) != 0 || tmp.fontSize >= 24f;
                var target = wantsBold ? bold : regular;
                if (target == null) target = bold != null ? bold : regular;
                if (target != null) tmp.font = target;
            }
        }
    }

    static GameObject FindByName(GameObject[] roots, string name)
    {
        foreach (var r in roots)
        {
            var t = FindRecursive(r.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static Transform FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var r = FindRecursive(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
