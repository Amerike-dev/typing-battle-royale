using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cablea el panel de Instrucciones en la escena CharacterSelect:
/// - Agrega InstructionsPanelController al Canvas que contiene el panel "Instrucciones".
/// - Crea (si no existe) un TimerText dentro del panel con la fuente Gontserrat-Bold.
/// - Enlaza las referencias del controller.
///
/// Menú: Tools > Characters > Setup Instructions Panel
///
/// Requiere tener CharacterSelect como escena activa (abierta). Marca la escena como modificada;
/// guardá con Ctrl+S. Es re-ejecutable (no duplica).
/// </summary>
public static class InstructionsPanelSetupTool
{
    private const string FONT_PATH = "Assets/Fonts/gontserrat/Gontserrat-Bold SDF.asset";
    private const string PANEL_NAME = "Instrucciones";
    private const string TIMER_NAME = "TimerText";

    [MenuItem("Tools/Characters/Setup Instructions Panel")]
    public static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[InstructionsSetup] No hay escena activa.");
            return;
        }

        Transform panel = FindInSceneByName(scene, PANEL_NAME);
        if (panel == null)
        {
            Debug.LogError($"[InstructionsSetup] No se encontró un objeto llamado '{PANEL_NAME}'. Abrí la escena CharacterSelect y asegurate de que el panel exista (Canvas/Instrucciones).");
            return;
        }

        // El controller debe vivir en un objeto activo: usamos el Canvas padre del panel.
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        GameObject host = canvas != null ? canvas.gameObject : panel.gameObject;

        var controller = host.GetComponent<InstructionsPanelController>();
        if (controller == null)
        {
            controller = host.AddComponent<InstructionsPanelController>();
            Debug.Log($"[InstructionsSetup] InstructionsPanelController agregado a '{host.name}'.");
        }

        // TimerText dentro del panel.
        TMP_Text timer = null;
        Transform existing = panel.Find(TIMER_NAME);
        if (existing != null) timer = existing.GetComponent<TMP_Text>();

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        if (font == null)
            Debug.LogWarning($"[InstructionsSetup] No se encontró la fuente en {FONT_PATH}; el TimerText quedará con la fuente por defecto.");

        if (timer == null)
        {
            var go = new GameObject(TIMER_NAME, typeof(RectTransform));
            go.transform.SetParent(panel, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 60f);
            rt.sizeDelta = new Vector2(300f, 140f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "20";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 96f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;

            timer = tmp;
            Debug.Log("[InstructionsSetup] TimerText creado dentro del panel.");
        }
        else if (font != null)
        {
            timer.font = font;
            timer.fontStyle = FontStyles.Bold;
        }

        // Enlazar referencias del controller.
        var so = new SerializedObject(controller);
        SetRef(so, "panel", panel.gameObject);
        SetRef(so, "timerText", timer);
        if (font != null) SetRef(so, "timerFont", font);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[InstructionsSetup] Listo. Guardá la escena (Ctrl+S).");
    }

    private static void SetRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.objectReferenceValue = value;
        else Debug.LogWarning($"[InstructionsSetup] No se encontró la propiedad '{propName}'.");
    }

    private static Transform FindInSceneByName(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform found = FindRecursive(t.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
