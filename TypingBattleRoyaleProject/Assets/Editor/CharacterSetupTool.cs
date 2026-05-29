using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Herramienta one-click para integrar los modelos reales de personaje (Berry, Wander, Ixia, Klug)
/// en todo el flujo: genera los prefabs de preview (Character Select) y de gameplay (networked),
/// asigna materiales/animator, crea los SkinInfo, los registra en DefaultNetworkPrefabs y los
/// cablea en IDController (preview) y GameplayManager (spawn).
///
/// Menú: Tools > Characters > Setup Real Characters
///
/// Es re-ejecutable (idempotente): sobrescribe los prefabs/assets generados.
/// </summary>
public static class CharacterSetupTool
{
    private const string ART_DIR = "Assets/Artist/Animations";
    private const string OUT_DIR = "Assets/Prefabs/Characters";
    private const string TEMPLATE_GAMEPLAY = "Assets/Prefabs/PrefabList/SkinsInfo/Botas/Gameplay/BotasGameplay_01.prefab";
    private const string NETWORK_PREFABS = "Assets/DefaultNetworkPrefabs.asset";
    private const string GAMEPLAY_SCENE = "Assets/Scenes/GameplayScene.unity";

    private class CharDef
    {
        public string name;
        public string fbx;
        public string animator;
        public string[] skins;   // 3 materiales (colorIndex 0-2)
    }

    private static readonly CharDef[] Characters =
    {
        new CharDef {
            name = "Berry",
            fbx = ART_DIR + "/Berry_Model.fbx",
            animator = ART_DIR + "/BerryAnimatorController.controller",
            skins = new[] { ART_DIR + "/SM_Berry_Skin_1.mat", ART_DIR + "/SM_Berry_Skin_2.mat", ART_DIR + "/SM_Berry_Skin_3.mat" }
        },
        new CharDef {
            name = "Wander",
            fbx = ART_DIR + "/Wander_Model.fbx",
            animator = ART_DIR + "/WanderAnimatorController.controller",
            skins = new[] { ART_DIR + "/SM_Wander_Skin_1.mat", ART_DIR + "/SM_Wander_Skin_2.mat", ART_DIR + "/SM_Wander_Skin_3.mat" }
        },
        new CharDef {
            // OJO: Ixia no tiene SM_Ixia_Skin_3.mat; usamos el duplicado "SM_Ixia_Skin_2 1.mat" como 3er color.
            name = "Ixia",
            fbx = ART_DIR + "/Ixia_Model.fbx",
            animator = ART_DIR + "/IxiaAnimatorController.controller",
            skins = new[] { ART_DIR + "/SM_Ixia_Skin_1.mat", ART_DIR + "/SM_Ixia_Skin_2.mat", ART_DIR + "/SM_Ixia_Skin_2 1.mat" }
        },
        new CharDef {
            name = "Klug",
            fbx = ART_DIR + "/Klug_Model.fbx",
            animator = ART_DIR + "/KlugAnimatorController.controller",
            skins = new[] { ART_DIR + "/SM_Klug_Skin_1.mat", ART_DIR + "/SM_Klug_Skin_2.mat", ART_DIR + "/SM_Klug_Skin_3.mat" }
        },
    };

    [MenuItem("Tools/Characters/Setup Real Characters")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog(
                "Setup Real Characters",
                "Generará prefabs de preview y gameplay para Berry, Wander, Ixia y Klug, los registrará en la red y los cableará en IDController y GameplayManager.\n\nGuarda tu escena actual antes de continuar.\n\n¿Continuar?",
                "Sí, generar", "Cancelar"))
            return;

        var log = new StringBuilder();
        var skinInfos = new List<SkinInfo>();

        EnsureFolder(OUT_DIR);

        var template = AssetDatabase.LoadAssetAtPath<GameObject>(TEMPLATE_GAMEPLAY);
        if (template == null)
        {
            Debug.LogError($"[CharacterSetup] No se encontró el prefab plantilla de gameplay en {TEMPLATE_GAMEPLAY}. Aborto.");
            return;
        }

        foreach (var c in Characters)
        {
            log.AppendLine($"\n=== {c.name} ===");

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(c.fbx);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(c.animator);
            var mats = c.skins.Select(p => AssetDatabase.LoadAssetAtPath<Material>(p)).ToArray();

            if (fbx == null) { log.AppendLine($"  ERROR: FBX no encontrado: {c.fbx}"); continue; }
            if (controller == null) log.AppendLine($"  AVISO: Animator no encontrado: {c.animator}");
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == null) log.AppendLine($"  AVISO: material no encontrado: {c.skins[i]}");

            var preview = BuildPreview(c, fbx, controller, mats, log);
            var gameplay = BuildGameplay(c, template, fbx, controller, mats, log);

            if (gameplay != null) RegisterNetworkPrefab(gameplay, log);

            var si = WriteSkinInfo(c, preview, gameplay, mats, controller, log);
            if (si != null) skinInfos.Add(si);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Cablear consumidores
        WireIntoIDControllerPrefabs(skinInfos, log);
        WireIntoGameplayManager(skinInfos, log);

        AssetDatabase.SaveAssets();

        Debug.Log("[CharacterSetup] Resultado:\n" + log);
        EditorUtility.DisplayDialog("Setup Real Characters",
            $"Listo. Se generaron {skinInfos.Count} personajes.\n\nRevisa la consola para el detalle y los avisos de ajuste manual (escala/posición del modelo).",
            "OK");
    }

    // ---- Preview prefab (Character Select) ----
    private static GameObject BuildPreview(CharDef c, GameObject fbx, RuntimeAnimatorController controller, Material[] mats, StringBuilder log)
    {
        var temp = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        if (temp == null) { log.AppendLine("  ERROR: no se pudo instanciar el FBX para preview."); return null; }

        var anim = temp.GetComponent<Animator>();
        if (anim == null) anim = temp.AddComponent<Animator>();
        if (controller != null) anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false;

        if (mats.Length > 0 && mats[0] != null) PlayerSkin.ApplyTo(temp, mats[0]);

        string path = $"{OUT_DIR}/{c.name}_Preview.prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(temp, path);
        Object.DestroyImmediate(temp);

        log.AppendLine(saved != null ? $"  preview -> {path}" : "  ERROR: no se pudo guardar el preview.");
        return saved;
    }

    // ---- Gameplay prefab (networked) ----
    private static GameObject BuildGameplay(CharDef c, GameObject template, GameObject fbx, RuntimeAnimatorController controller, Material[] mats, StringBuilder log)
    {
        // Clonamos la plantilla (conserva NetworkObject, PlayerController, UI, CharacterController, etc.)
        GameObject root = PrefabUtility.LoadPrefabContents(TEMPLATE_GAMEPLAY);
        if (root == null) { log.AppendLine("  ERROR: no se pudo cargar la plantilla de gameplay."); return null; }

        // Quitamos el modelo placeholder primitivo del root.
        var mf = root.GetComponent<MeshFilter>(); if (mf != null) Object.DestroyImmediate(mf);
        var mr = root.GetComponent<MeshRenderer>(); if (mr != null) Object.DestroyImmediate(mr);

        // El Animator del root quedará sin uso (PlayerAnimatorView se repunta al del modelo);
        // le quitamos el controller para que no intente animar bones inexistentes.
        var rootAnim = root.GetComponent<Animator>();
        if (rootAnim != null) rootAnim.runtimeAnimatorController = null;

        // Quitamos el modelo placeholder que viene como HIJO en la plantilla (p.ej. "PersonajesLowPoly").
        RemovePlaceholderModels(root, log);

        // Si quedó un "Model" de una ejecución previa, lo eliminamos para ser idempotentes.
        var oldModel = root.transform.Find("Model");
        if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

        // Instanciamos el FBX como hijo "Model" (en la misma escena de edición del prefab).
        var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.scene);
        model.transform.SetParent(root.transform, false);
        model.name = "Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // Animator en el modelo (conserva el Avatar generado por el FBX) + controller.
        var modelAnim = model.GetComponent<Animator>();
        if (modelAnim == null) modelAnim = model.AddComponent<Animator>();
        if (controller != null) modelAnim.runtimeAnimatorController = controller;
        modelAnim.applyRootMotion = false;

        // Repuntamos PlayerAnimatorView al Animator del modelo (es defensivo ante params faltantes).
        var pav = root.GetComponentInChildren<PlayerAnimatorView>(true);
        if (pav != null) pav.playerAnimator = modelAnim;
        else log.AppendLine("  AVISO: no se encontró PlayerAnimatorView en la plantilla.");

        // PlayerSkin: renderers del modelo + materiales.
        var ps = root.GetComponent<PlayerSkin>();
        if (ps == null) ps = root.AddComponent<PlayerSkin>();
        ps.renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Cast<Renderer>().ToArray();
        ps.skins = mats;
        if (ps.renderers.Length == 0)
            log.AppendLine("  AVISO: el modelo no tiene SkinnedMeshRenderer (¿el FBX no está skinneado?).");

        // Skin por defecto para que se vea bien en el editor.
        if (mats.Length > 0 && mats[0] != null) PlayerSkin.ApplyTo(model, mats[0]);

        string path = $"{OUT_DIR}/{c.name}_Gameplay.prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        log.AppendLine(saved != null ? $"  gameplay -> {path}  (renderers: {ps.renderers.Length})" : "  ERROR: no se pudo guardar el gameplay.");
        return saved;
    }

    /// <summary>
    /// Elimina los hijos directos del root que sean modelos placeholder (tienen malla y no son UI/efectos).
    /// No toca el hijo "Model" (nuestro modelo nuevo), ni UI/Camera/Particle/CastOrigin/SpellUIController.
    /// </summary>
    private static void RemovePlaceholderModels(GameObject root, StringBuilder log)
    {
        var toRemove = new List<GameObject>();
        foreach (Transform child in root.transform)
        {
            if (child.name == "Model") continue;

            bool isUiOrFx =
                child.GetComponentInChildren<Canvas>(true) != null ||
                child.GetComponent<ParticleSystem>() != null ||
                child.name == "Camera" || child.name == "CastOrigin" ||
                child.name == "SpellUIController" || child.name == "Particle" || child.name == "UI";

            bool hasMesh =
                child.GetComponentInChildren<MeshRenderer>(true) != null ||
                child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;

            if (!isUiOrFx && hasMesh) toRemove.Add(child.gameObject);
        }

        foreach (var go in toRemove)
        {
            log.AppendLine($"  placeholder eliminado: {go.name}");
            Object.DestroyImmediate(go);
        }
    }

    private static void RegisterNetworkPrefab(GameObject prefab, StringBuilder log)
    {
        var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NETWORK_PREFABS);
        if (list == null) { log.AppendLine($"  AVISO: no se encontró {NETWORK_PREFABS}; registra el prefab manualmente."); return; }

        // Vía SerializedObject (robusto ante cambios de API de Netcode).
        var so = new SerializedObject(list);
        var listProp = so.FindProperty("List");
        if (listProp == null || !listProp.isArray)
        {
            log.AppendLine("  AVISO: no se pudo leer la lista de NetworkPrefabs; registra el prefab manualmente.");
            return;
        }

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var existing = listProp.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab");
            if (existing != null && existing.objectReferenceValue == prefab)
            {
                log.AppendLine("  ya estaba en DefaultNetworkPrefabs");
                return;
            }
        }

        listProp.arraySize++;
        var el = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
        var overrideProp = el.FindPropertyRelative("Override");
        if (overrideProp != null) overrideProp.enumValueIndex = 0;
        el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        var src = el.FindPropertyRelative("SourcePrefabToOverride"); if (src != null) src.objectReferenceValue = null;
        var ovr = el.FindPropertyRelative("OverridingTargetPrefab"); if (ovr != null) ovr.objectReferenceValue = null;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(list);
        log.AppendLine("  registrado en DefaultNetworkPrefabs");
    }

    private static SkinInfo WriteSkinInfo(CharDef c, GameObject preview, GameObject gameplay, Material[] mats, RuntimeAnimatorController controller, StringBuilder log)
    {
        string path = $"{OUT_DIR}/{c.name}.asset";
        var si = AssetDatabase.LoadAssetAtPath<SkinInfo>(path);
        if (si == null)
        {
            si = ScriptableObject.CreateInstance<SkinInfo>();
            AssetDatabase.CreateAsset(si, path);
        }
        si.skinName = c.name;
        si.previewModel = preview;
        si.gameplayPrefab = gameplay;
        si.skins = mats;
        si.animator = controller;
        EditorUtility.SetDirty(si);
        log.AppendLine($"  SkinInfo -> {path}");
        return si;
    }

    // ---- Cablear arraySkins en los prefabs que tienen IDController ----
    private static void WireIntoIDControllerPrefabs(List<SkinInfo> skinInfos, StringBuilder log)
    {
        log.AppendLine("\n=== Cableado IDController (preview) ===");
        int wired = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null || go.GetComponentInChildren<IDController>(true) == null) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            var idc = root.GetComponentInChildren<IDController>(true);
            if (idc != null && SetSkinArray(idc, "arraySkins", skinInfos))
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                log.AppendLine($"  arraySkins seteado en {path}");
                wired++;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        if (wired == 0) log.AppendLine("  AVISO: no se encontró ningún prefab con IDController. Asigna arraySkins manualmente.");
    }

    // ---- Cablear arraySkins en GameplayManager (escena de gameplay) ----
    private static void WireIntoGameplayManager(List<SkinInfo> skinInfos, StringBuilder log)
    {
        log.AppendLine("\n=== Cableado GameplayManager (spawn) ===");

        Scene scene = default;
        bool alreadyOpen = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.path == GAMEPLAY_SCENE) { scene = s; alreadyOpen = true; break; }
        }
        if (!alreadyOpen)
        {
            if (!System.IO.File.Exists(GAMEPLAY_SCENE)) { log.AppendLine($"  AVISO: no existe {GAMEPLAY_SCENE}."); return; }
            scene = EditorSceneManager.OpenScene(GAMEPLAY_SCENE, OpenSceneMode.Additive);
        }

        GameplayManager gm = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            gm = go.GetComponentInChildren<GameplayManager>(true);
            if (gm != null) break;
        }

        if (gm != null && SetSkinArray(gm, "arraySkins", skinInfos))
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.AppendLine("  arraySkins seteado en GameplayManager");
        }
        else
        {
            log.AppendLine("  AVISO: no se encontró GameplayManager. Asigna arraySkins manualmente.");
        }

        if (!alreadyOpen) EditorSceneManager.CloseScene(scene, true);
    }

    private static bool SetSkinArray(Object component, string fieldName, List<SkinInfo> skinInfos)
    {
        var so = new SerializedObject(component);
        var arr = so.FindProperty(fieldName);
        if (arr == null || !arr.isArray) return false;

        arr.arraySize = skinInfos.Count;
        for (int i = 0; i < skinInfos.Count; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = skinInfos[i];

        so.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
