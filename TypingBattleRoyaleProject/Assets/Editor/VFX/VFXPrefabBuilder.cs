#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class VFXPrefabBuilder
{
    const string PrefabDir = "Assets/Prefabs/VFX";
    const string MaterialDir = "Assets/Materials/VFX";
    const string ResourcesDir = "Assets/Resources";
    const string ProjectilePrefabPath = PrefabDir + "/VFX_Projectile.prefab";
    const string FireT1MaterialPath = MaterialDir + "/M_Fire_T1.mat";
    const string SpellCatalogPath = ResourcesDir + "/SpellCatalog.asset";
    const string SpellsRoot = "Assets/ScriptableObjects/Objects/Spells";
    const string BolaDeFuegoPath = "Assets/ScriptableObjects/Objects/Spells/Fire/Tier 1/Bola de Fuego.asset";

    [MenuItem("Tools/TBR/Build VFX_Projectile Prefab")]
    public static void Build()
    {
        EnsureFolder(PrefabDir);
        EnsureFolder(MaterialDir);
        EnsureFolder(ResourcesDir);

        var material = CreateOrLoadFireT1Material();
        var prefab = CreateOrLoadProjectilePrefab();
        AssignToBolaDeFuego(material);
        BuildOrUpdateSpellCatalog();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_Projectile prefab + M_Fire_T1 material + SpellCatalog listos.");
    }

    static void BuildOrUpdateSpellCatalog()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<SpellCatalog>(SpellCatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<SpellCatalog>();
            AssetDatabase.CreateAsset(catalog, SpellCatalogPath);
        }
        var guids = AssetDatabase.FindAssets("t:Spell", new[] { SpellsRoot });
        var list = new System.Collections.Generic.List<Spell>(guids.Length);
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var s = AssetDatabase.LoadAssetAtPath<Spell>(path);
            if (s != null) list.Add(s);
        }
        var so = new SerializedObject(catalog);
        var arr = so.FindProperty("spells");
        arr.arraySize = list.Count;
        for (int i = 0; i < list.Count; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        Debug.Log($"[TBR] SpellCatalog poblado con {list.Count} spells.");
    }

    static Material CreateOrLoadFireT1Material()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(FireT1MaterialPath);
        if (mat != null) return mat;

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");
        mat = new Material(shader) { name = "M_Fire_T1" };
        var fire = new Color(1f, 0.45f, 0.1f, 1f);
        mat.color = fire;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", fire);
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", fire * 2f);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 1f);
        if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.EnableKeyword("_EMISSION");
        mat.renderQueue = 3000;
        AssetDatabase.CreateAsset(mat, FireT1MaterialPath);
        return mat;
    }

    static GameObject CreateOrLoadProjectilePrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_Projectile");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.startSize = 1f;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 50f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.3f;

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<ProjectileVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void AssignToBolaDeFuego(Material mat)
    {
        var spell = AssetDatabase.LoadAssetAtPath<Spell>(BolaDeFuegoPath);
        if (spell == null)
        {
            Debug.LogWarning("[TBR] No se encontro " + BolaDeFuegoPath);
            return;
        }
        var so = new SerializedObject(spell);
        var tierProp = so.FindProperty("tier");
        if (tierProp != null) tierProp.enumValueIndex = (int)SpellTiers.TierOne;
        var archProp = so.FindProperty("archetype");
        if (archProp != null) archProp.enumValueIndex = (int)SpellTypes.Projectile;
        var matProp = so.FindProperty("materialVFX");
        if (matProp != null) matProp.objectReferenceValue = mat;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(spell);
    }

    static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath)) return;
        var parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
        var folder = Path.GetFileName(assetPath);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
