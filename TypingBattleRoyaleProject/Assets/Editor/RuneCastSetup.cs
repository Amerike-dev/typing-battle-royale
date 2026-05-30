using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de Editor que deja listo el sistema de runas de casteo:
///   1) Crea el material RuneGlow (shader TBR/RuneGlow) si no existe.
///   2) Crea y autopuebla el RuneLibrary (Elements -> sprite de runa).
///   3) En cada prefab de personaje (_Gameplay): crea el anchor "RuneAnchor" bajo "CastOrigin",
///      agrega el componente RuneCastDisplay en la raíz y cablea sus referencias.
///
/// Es idempotente: se puede correr varias veces sin duplicar nada.
/// Menú: Tools > TBR > Setup Rune Cast Displays.
/// </summary>
public static class RuneCastSetup
{
    private const string ShaderName = "TBR/RuneGlow";
    private const string ShaderPath = "Assets/Shaders/RuneGlow.shader";
    private const string MaterialPath = "Assets/Shaders/RuneGlow.mat";
    private const string LibraryPath = "Assets/ScriptableObjects/RuneLibrary.asset";
    private const string RunesFolder = "Assets/Artist/AssetsIcons/Runes/";
    private const string AnchorName = "RuneAnchor";
    private const string CastOriginName = "CastOrigin";

    private static readonly string[] PrefabPaths =
    {
        "Assets/Prefabs/Characters/Berry_Gameplay.prefab",
        "Assets/Prefabs/Characters/Ixia_Gameplay.prefab",
        "Assets/Prefabs/Characters/Klug_Gameplay.prefab",
        "Assets/Prefabs/Characters/Wander_Gameplay.prefab",
    };

    // Mapeo elemento -> nombre de archivo de runa (sin extensión).
    private static readonly (Elements element, string fileName)[] RuneMap =
    {
        (Elements.Fire,    "Pentagrama Fuego"),
        (Elements.Water,   "Pentagrama Agua"),
        (Elements.Earth,   "Pentagrama Tierra"),
        (Elements.Wind,    "Pentagrama Aire"),
        (Elements.Nature,  "Pentagrama Naturaleza"),
        (Elements.Thunder, "Pentagrama Rayo"),
        (Elements.Dark,    "Pentagrama Oscuridad"),
        (Elements.Light,   "Pentagrama Luz"),
        (Elements.Ice,     "Pentagrama Hielo"),
        (Elements.Lava,    "Pentagrama Lava"),
    };

    [MenuItem("Tools/TBR/Setup Rune Cast Displays")]
    public static void Run()
    {
        Material material = EnsureMaterial();
        RuneLibrary library = EnsureLibrary();

        int wired = 0;
        foreach (string path in PrefabPaths)
        {
            if (SetupPrefab(path, material, library)) wired++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RuneCastSetup] Listo. Material='{material?.name}', Library='{library?.name}', prefabs cableados={wired}/{PrefabPaths.Length}.");
    }

    private static Material EnsureMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null) return material;

        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null) shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogError($"[RuneCastSetup] No se encontró el shader '{ShaderName}'. ¿Compiló {ShaderPath}?");
            return null;
        }

        material = new Material(shader) { name = "RuneGlow" };
        material.SetColor("_Color", Color.white);
        AssetDatabase.CreateAsset(material, MaterialPath);
        Debug.Log($"[RuneCastSetup] Material creado en {MaterialPath}.");
        return material;
    }

    private static RuneLibrary EnsureLibrary()
    {
        var library = AssetDatabase.LoadAssetAtPath<RuneLibrary>(LibraryPath);
        bool created = false;
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<RuneLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            created = true;
        }

        var entries = new RuneLibrary.RuneEntry[RuneMap.Length];
        int missing = 0;
        for (int i = 0; i < RuneMap.Length; i++)
        {
            string spritePath = RunesFolder + RuneMap[i].fileName + ".PNG";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                missing++;
                Debug.LogWarning($"[RuneCastSetup] No se encontró el sprite '{spritePath}' para {RuneMap[i].element}.");
            }
            entries[i] = new RuneLibrary.RuneEntry { element = RuneMap[i].element, sprite = sprite };
        }

        library.runes = entries;
        EditorUtility.SetDirty(library);
        Debug.Log($"[RuneCastSetup] RuneLibrary {(created ? "creada" : "actualizada")} ({RuneMap.Length - missing}/{RuneMap.Length} sprites).");
        return library;
    }

    private static bool SetupPrefab(string path, Material material, RuneLibrary library)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null)
        {
            Debug.LogError($"[RuneCastSetup] No se pudo abrir el prefab {path}.");
            return false;
        }

        try
        {
            Transform castOrigin = FindByName(root.transform, CastOriginName);
            if (castOrigin == null)
            {
                Debug.LogWarning($"[RuneCastSetup] {path}: no se encontró '{CastOriginName}'. Se omite.");
                return false;
            }

            // Anchor bajo CastOrigin (placeholder; ajustar posición/rotación a gusto en el Editor).
            Transform anchor = FindChildByName(castOrigin, AnchorName);
            if (anchor == null)
            {
                var anchorGo = new GameObject(AnchorName);
                anchor = anchorGo.transform;
                anchor.SetParent(castOrigin, false);
                anchor.localPosition = new Vector3(0f, 0f, 0.2f); // un poco enfrente de la mano
                anchor.localRotation = Quaternion.identity;
                anchor.localScale = Vector3.one;
            }

            var display = root.GetComponent<RuneCastDisplay>();
            if (display == null) display = root.AddComponent<RuneCastDisplay>();

            display.runeAnchor = anchor;
            display.runeMaterial = material;
            display.runeLibrary = library;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"[RuneCastSetup] {path}: RuneCastDisplay + '{AnchorName}' cableados.");
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform FindChildByName(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        }
        return null;
    }

    private static Transform FindByName(Transform root, string name)
    {
        if (root.name == name) return root;
        var all = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t.name == name) return t;
        }
        return null;
    }
}
