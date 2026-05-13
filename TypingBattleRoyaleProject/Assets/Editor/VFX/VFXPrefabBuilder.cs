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
    const string AOEPrefabPath = PrefabDir + "/VFX_AOE.prefab";
    const string AuraPrefabPath = PrefabDir + "/VFX_Aura.prefab";
    const string BeamPrefabPath = PrefabDir + "/VFX_Beam.prefab";
    const string SummonPrefabPath = PrefabDir + "/VFX_Summon.prefab";
    const string BuffDebuffPrefabPath = PrefabDir + "/VFX_BuffDebuff.prefab";
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

    [MenuItem("Tools/TBR/Build VFX_AOE Prefab")]
    public static void BuildAOE()
    {
        EnsureFolder(PrefabDir);
        var prefab = CreateOrLoadAOEPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_AOE prefab listo.");
    }

    [MenuItem("Tools/TBR/Build VFX_Aura Prefab")]
    public static void BuildAura()
    {
        EnsureFolder(PrefabDir);
        var prefab = CreateOrLoadAuraPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_Aura prefab listo.");
    }

    [MenuItem("Tools/TBR/Build VFX_Beam Prefab")]
    public static void BuildBeam()
    {
        EnsureFolder(PrefabDir);
        var prefab = CreateOrLoadBeamPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_Beam prefab listo.");
    }

    [MenuItem("Tools/TBR/Build VFX_Summon Prefab")]
    public static void BuildSummon()
    {
        EnsureFolder(PrefabDir);
        var prefab = CreateOrLoadSummonPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_Summon prefab listo.");
    }

    [MenuItem("Tools/TBR/Build VFX_BuffDebuff Prefab")]
    public static void BuildBuffDebuff()
    {
        EnsureFolder(PrefabDir);
        var prefab = CreateOrLoadBuffDebuffPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[TBR] VFX_BuffDebuff prefab listo.");
    }

    [MenuItem("Tools/TBR/Build All Archetype Prefabs")]
    public static void BuildAllArchetypes()
    {
        EnsureFolder(PrefabDir);
        EnsureFolder(MaterialDir);
        EnsureFolder(ResourcesDir);

        CreateOrLoadFireT1Material();
        CreateOrLoadProjectilePrefab();
        CreateOrLoadAOEPrefab();
        CreateOrLoadAuraPrefab();
        CreateOrLoadBeamPrefab();
        CreateOrLoadSummonPrefab();
        CreateOrLoadBuffDebuffPrefab();
        BuildOrUpdateSpellCatalog();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[TBR] 6 arquetipos VFX construidos (Projectile, AOE, Aura, Beam, Summon, BuffDebuff).");
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

    static GameObject CreateOrLoadAOEPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(AOEPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_AOE");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 4f;
        main.startSize = 0.4f;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.maxParticles = 400;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 200) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        shape.arc = 360f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = new ParticleSystem.MinMaxCurve(3f);

        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<AOEVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, AOEPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateOrLoadAuraPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(AuraPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_Aura");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 3f;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = 0.5f;
        main.startSize = 0.3f;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.rateOverTime = 40f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 1f;
        shape.donutRadius = 0.1f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(0.5f);

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<AuraVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, AuraPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateOrLoadBeamPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BeamPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_Beam");

        var line = go.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.25f;
        line.endWidth = 0.25f;
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");
        if (shader != null) line.material = new Material(shader);

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = 0.4f;
        main.startSpeed = 1f;
        main.startSize = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.rateOverTime = 80f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<BeamVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, BeamPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateOrLoadSummonPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SummonPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_Summon");

        var placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        placeholder.name = "PlaceholderMesh";
        Object.DestroyImmediate(placeholder.GetComponent<Collider>());
        placeholder.transform.SetParent(go.transform, worldPositionStays: false);
        placeholder.transform.localPosition = new Vector3(0f, 1f, 0f);
        placeholder.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 1f;
        main.startSize = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.rateOverTime = 30f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.6f;

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<SummonVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, SummonPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateOrLoadBuffDebuffPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BuffDebuffPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("VFX_BuffDebuff");

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 3f;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 0.3f;
        main.startSize = 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.maxParticles = 100;

        var emission = ps.emission;
        emission.rateOverTime = 25f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(0.4f);

        go.AddComponent<SpellVFXBinder>();
        go.AddComponent<BuffDebuffVFX>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, BuffDebuffPrefabPath);
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
