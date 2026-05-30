using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Monta la cara de cada personaje (Berry, Wander, Ixia, Klug) sobre su modelo:
/// por cada SkinnedMeshRenderer crea una copia "FaceOverlay" (misma malla, huesos y UVs) con el
/// shader TBR/FaceOverlay y el componente CharacterFace, y rellena las emociones
/// con los PNG de Assets/Artist/Animations/Faces (por convención de nombres).
///
/// El shader reubica el PNG de cara (un dibujo suelto) dentro de un rectángulo de UV (_FaceRect)
/// que corresponde a la isla UV de la cabeza. Como el overlay usa la malla skinneada, la cara se
/// DEFORMA igual que el modelo durante las animaciones.
///
/// Trabaja sobre los prefabs YA generados:
///   Assets/Prefabs/Characters/&lt;Personaje&gt;_Gameplay.prefab  (modelo en el hijo "Model")
///   Assets/Prefabs/Characters/&lt;Personaje&gt;_Preview.prefab   (el modelo es la raíz)
///
/// Menú: Tools > Characters > Setup Character Faces
///
/// Es re-ejecutable (idempotente): borra los FaceOverlay anteriores y NO pisa el _FaceRect ya
/// ajustado de los materiales existentes.
///
/// AJUSTE: tras correrlo, abre el material SM_(Personaje)_Face en FaceMaterials y mueve el
/// "Rect UV de la cara" (x, y, ancho, alto) hasta que el rostro quede en su lugar. Se ve en vivo.
/// </summary>
public static class CharacterFaceSetupTool
{
    private const string OUT_DIR = "Assets/Prefabs/Characters";
    private const string FACES_DIR = "Assets/Artist/Animations/Faces";
    private const string MAT_DIR = "Assets/Prefabs/Characters/FaceMaterials";

    private static readonly string[] CharacterNames = { "Berry", "Wander", "Ixia", "Klug" };

    // La cara solo se monta en la malla del cuerpo (la que tiene la cabeza), no en accesorios
    // (sombrero, pelo, lentes...). Se filtra por nombre de la malla / del GameObject.
    private static readonly string[] BodyMeshKeywords = { "cuerpo", "body" };

    // Pistas de nombre (sin acentos, en minúsculas) -> emoción.
    private static readonly (FaceState state, string[] keys)[] Keywords =
    {
        (FaceState.Neutral,   new[] { "neutra" }),
        (FaceState.Casting,   new[] { "casteando", "concentrac" }),
        (FaceState.Hurt,      new[] { "dano", "recibiendo", "enojo" }),
        (FaceState.Death,     new[] { "muerte" }),
        (FaceState.Jump,      new[] { "saltando", "sorpresa" }),
        (FaceState.SpellFail, new[] { "fallando", "tristeza" }),
    };

    [MenuItem("Tools/Characters/Setup Character Faces")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog(
                "Setup Character Faces",
                "Agregará la cara (overlay de malla + CharacterFace) a los prefabs de Gameplay y Preview de " +
                "Berry, Wander, Ixia y Klug, usando los PNG de la carpeta Faces.\n\n" +
                "Luego hay que abrir el material SM_<Personaje>_Face y ajustar el 'Rect UV de la cara' para " +
                "que el rostro quede en su lugar (se ve en vivo).\n\n" +
                "¿Continuar?",
                "Sí, montar caras", "Cancelar"))
            return;

        var log = new StringBuilder();
        SetupAllFaces(log);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CharacterFaceSetup] Resultado:\n" + log);
        EditorUtility.DisplayDialog("Setup Character Faces",
            "Listo. Revisa la consola.\n\nAhora, por cada personaje, abre el material\n" +
            "Assets/Prefabs/Characters/FaceMaterials/SM_<Personaje>_Face\n" +
            "y mueve el 'Rect UV de la cara' (x, y, ancho, alto) hasta que el rostro quede en su lugar. " +
            "Se ve en vivo en el prefab y la cara cambia sola según la animación.",
            "OK");
    }

    /// <summary>Aplica las caras a todos los personajes en sus prefabs de Gameplay y Preview.</summary>
    public static void SetupAllFaces(StringBuilder log)
    {
        EnsureFolder(MAT_DIR);

        foreach (var name in CharacterNames)
        {
            log.AppendLine($"\n=== {name} ===");

            var faces = LoadFaces(name, log);
            if (faces.Count == 0)
            {
                log.AppendLine($"  ERROR: no se encontraron PNG en {FACES_DIR}/{name}. Salto este personaje.");
                continue;
            }

            Material mat = GetOrCreateFaceMaterial(name, faces, log, out bool matIsNew);
            if (mat == null) continue;

            SetupPrefab($"{OUT_DIR}/{name}_Gameplay.prefab", isGameplay: true, name, faces, mat, matIsNew, log);
            SetupPrefab($"{OUT_DIR}/{name}_Preview.prefab", isGameplay: false, name, faces, mat, matIsNew, log);
        }
    }

    // ---------------- Por prefab ----------------

    private static void SetupPrefab(string path, bool isGameplay, string charName,
        Dictionary<FaceState, Texture2D[]> faces, Material mat, bool matIsNew, StringBuilder log)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            log.AppendLine($"  AVISO: no existe {path}; lo salto.");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            // El modelo: en gameplay es el hijo "Model"; en preview, la propia raíz.
            Transform model = isGameplay ? (root.transform.Find("Model") ?? root.transform) : root.transform;

            BuildFace(root, model, charName, faces, mat, matIsNew, log);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            log.AppendLine($"  {(isGameplay ? "gameplay" : "preview")} -> {path}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void BuildFace(GameObject root, Transform model, string charName,
        Dictionary<FaceState, Texture2D[]> faces, Material mat, bool matIsNew, StringBuilder log)
    {
        // Idempotencia: quitamos overlays/anchors de ejecuciones previas.
        RemoveOld(model);

        var all = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (all.Length == 0)
        {
            log.AppendLine("  AVISO: el modelo no tiene SkinnedMeshRenderer; no puedo montar la cara.");
            return;
        }

        // Elegimos UNA sola malla: la del cuerpo (la que tiene la cabeza/cara). Preferimos por nombre
        // (cuerpo/body); si los nombres no ayudan (el FBX las exporta como "Mesh.001"...), tomamos la
        // de más vértices, que es el cuerpo (los accesorios -sombrero, pelo, lentes, capa- son más chicos).
        SkinnedMeshRenderer body = null;
        foreach (var s in all)
            if (s != null && s.sharedMesh != null && IsBodyMesh(s)) { body = s; break; }
        if (body == null)
        {
            int best = -1;
            foreach (var s in all)
            {
                if (s == null || s.sharedMesh == null) continue;
                int vc = s.sharedMesh.vertexCount;
                if (vc > best) { best = vc; body = s; }
            }
        }
        if (body == null)
        {
            log.AppendLine("  AVISO: no encontré una malla válida para la cara.");
            return;
        }

        var targets = new List<SkinnedMeshRenderer> { body };
        var skipped = new List<string>();
        foreach (var s in all)
            if (s != null && s != body)
                skipped.Add($"{s.name}({(s.sharedMesh != null ? s.sharedMesh.vertexCount : 0)}v)");
        log.AppendLine($"  malla de cara: {body.name} ({body.sharedMesh.vertexCount}v)");
        if (skipped.Count > 0) log.AppendLine("  omitidas: " + string.Join(", ", skipped));

        Texture2D neutral = NeutralTexture(faces);
        var overlays = new List<Renderer>();

        // Por cada malla de cuerpo creamos una copia "overlay" que comparte malla, huesos y UVs,
        // con el material de cara. La cara solo aparece dentro del rect de UV de la cabeza y se
        // deforma igual que el modelo.
        foreach (var src in targets)
        {
            if (src == null || src.sharedMesh == null) continue;

            var go = new GameObject("FaceOverlay_" + src.name);
            go.transform.SetParent(src.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.layer = src.gameObject.layer;

            var ov = go.AddComponent<SkinnedMeshRenderer>();
            ov.sharedMesh = src.sharedMesh;
            ov.bones = src.bones;
            ov.rootBone = src.rootBone;
            ov.localBounds = src.localBounds;
            ov.quality = src.quality;
            ov.updateWhenOffscreen = src.updateWhenOffscreen;
            ov.skinnedMotionVectors = false;
            ov.shadowCastingMode = ShadowCastingMode.Off;
            ov.receiveShadows = false;
            ov.lightProbeUsage = LightProbeUsage.Off;
            ov.reflectionProbeUsage = ReflectionProbeUsage.Off;

            int sub = Mathf.Max(1, src.sharedMesh.subMeshCount);
            var mats = new Material[sub];
            for (int i = 0; i < sub; i++) mats[i] = mat;
            ov.sharedMaterials = mats;

            // Textura neutral por defecto para verla en el editor (en runtime la cambia CharacterFace).
            if (neutral != null)
            {
                var mpb = new MaterialPropertyBlock();
                ov.GetPropertyBlock(mpb);
                mpb.SetTexture("_BaseMap", neutral);
                mpb.SetTexture("_MainTex", neutral);
                ov.SetPropertyBlock(mpb);
            }

            overlays.Add(ov);
        }

        // Rect UV automático: bbox de los vértices pesados al hueso de la cabeza. Solo en materiales
        // nuevos, para no pisar un ajuste manual previo.
        if (matIsNew && mat.HasProperty("_FaceRect"))
        {
            if (TryComputeHeadUVRect(targets, out Rect r))
            {
                mat.SetVector("_FaceRect", new Vector4(r.x, r.y, r.width, r.height));
                EditorUtility.SetDirty(mat);
                log.AppendLine($"  _FaceRect auto = ({r.x:0.000}, {r.y:0.000}, {r.width:0.000}, {r.height:0.000})");
            }
            else
            {
                log.AppendLine("  _FaceRect auto: no detecté la cabeza (¿malla no legible o hueso sin 'head'?); ajustá el rect a mano.");
            }
        }

        var face = root.GetComponent<CharacterFace>();
        if (face == null) face = root.AddComponent<CharacterFace>();
        face.animator = model.GetComponent<Animator>() ?? model.GetComponentInChildren<Animator>(true);
        face.faceRenderers = overlays.ToArray();
        face.clips = BuildClips(faces);

        log.AppendLine($"  overlays={overlays.Count} (malla cuerpo) | emociones={face.clips.Count}");
    }

    private static bool IsBodyMesh(SkinnedMeshRenderer r)
    {
        string n = Normalize(r.name);
        string m = r.sharedMesh != null ? Normalize(r.sharedMesh.name) : string.Empty;
        foreach (var k in BodyMeshKeywords)
            if (n.Contains(k) || m.Contains(k)) return true;
        return false;
    }

    /// <summary>
    /// Calcula el rectángulo UV que ocupan los vértices pesados al hueso de la cabeza (huesos cuyo
    /// nombre contiene "head"/"cabeza"). Sirve como _FaceRect inicial para que la cara caiga sobre
    /// la cabeza sin ajuste manual.
    /// </summary>
    private static bool TryComputeHeadUVRect(List<SkinnedMeshRenderer> targets, out Rect rect)
    {
        rect = default;
        float minU = float.MaxValue, minV = float.MaxValue, maxU = float.MinValue, maxV = float.MinValue;
        bool any = false;

        foreach (var smr in targets)
        {
            var mesh = smr.sharedMesh;
            if (mesh == null) continue;

            Vector2[] uvs = mesh.uv;
            BoneWeight[] bw = mesh.boneWeights;
            var bones = smr.bones;
            if (uvs == null || uvs.Length == 0 || bw == null || bw.Length == 0 || bones == null) continue;

            var headIdx = new HashSet<int>();
            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i];
                if (b == null) continue;
                string bn = Normalize(b.name);
                if (bn.Contains("head") || bn.Contains("cabeza")) headIdx.Add(i);
            }
            if (headIdx.Count == 0) continue;

            int n = Mathf.Min(uvs.Length, bw.Length);
            for (int i = 0; i < n; i++)
            {
                var w = bw[i];
                float hw = 0f;
                if (headIdx.Contains(w.boneIndex0)) hw += w.weight0;
                if (headIdx.Contains(w.boneIndex1)) hw += w.weight1;
                if (headIdx.Contains(w.boneIndex2)) hw += w.weight2;
                if (headIdx.Contains(w.boneIndex3)) hw += w.weight3;
                if (hw < 0.5f) continue;

                Vector2 uv = uvs[i];
                if (uv.x < minU) minU = uv.x;
                if (uv.x > maxU) maxU = uv.x;
                if (uv.y < minV) minV = uv.y;
                if (uv.y > maxV) maxV = uv.y;
                any = true;
            }
        }

        if (!any) return false;
        rect = new Rect(minU, minV, Mathf.Max(1e-4f, maxU - minU), Mathf.Max(1e-4f, maxV - minV));
        return true;
    }

    private static void RemoveOld(Transform model)
    {
        var toRemove = new List<GameObject>();
        foreach (var t in model.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t == model) continue;
            if (t.name.StartsWith("FaceOverlay") || t.name == "FaceAnchor")
                toRemove.Add(t.gameObject);
        }
        foreach (var go in toRemove) if (go != null) Object.DestroyImmediate(go);
    }

    private static Texture2D NeutralTexture(Dictionary<FaceState, Texture2D[]> faces)
    {
        if (faces.TryGetValue(FaceState.Neutral, out var n) && n.Length > 0) return n[0];
        foreach (var f in faces.Values) if (f.Length > 0) return f[0];
        return null;
    }

    // ---------------- Carga de PNG de caras ----------------

    private static Dictionary<FaceState, Texture2D[]> LoadFaces(string charName, StringBuilder log)
    {
        var result = new Dictionary<FaceState, Texture2D[]>();
        string folder = $"{FACES_DIR}/{charName}";
        if (!AssetDatabase.IsValidFolder(folder)) return result;

        var byState = new Dictionary<FaceState, List<(string path, Texture2D tex)>>();

        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;

            if (!TryClassify(path, out FaceState state))
            {
                log.AppendLine($"  AVISO: no pude clasificar la cara '{System.IO.Path.GetFileName(path)}'.");
                continue;
            }

            if (!byState.TryGetValue(state, out var list)) { list = new List<(string, Texture2D)>(); byState[state] = list; }
            list.Add((path, tex));
        }

        foreach (var kv in byState)
        {
            var frames = kv.Value.OrderBy(t => t.path, System.StringComparer.OrdinalIgnoreCase)
                                 .Select(t => t.tex).ToArray();
            result[kv.Key] = frames;
        }
        return result;
    }

    private static bool TryClassify(string path, out FaceState state)
    {
        string n = Normalize(System.IO.Path.GetFileNameWithoutExtension(path));
        foreach (var (s, keys) in Keywords)
            foreach (var k in keys)
                if (n.Contains(k)) { state = s; return true; }
        state = FaceState.Neutral;
        return false;
    }

    private static List<FaceClip> BuildClips(Dictionary<FaceState, Texture2D[]> faces)
    {
        var clips = new List<FaceClip>();
        // Orden estable y legible en el Inspector.
        foreach (FaceState s in System.Enum.GetValues(typeof(FaceState)))
            if (faces.TryGetValue(s, out var frames) && frames != null && frames.Length > 0)
                clips.Add(new FaceClip { state = s, frames = frames });
        return clips;
    }

    // ---------------- Material (shader TBR/FaceOverlay) ----------------

    private static Material GetOrCreateFaceMaterial(string charName, Dictionary<FaceState, Texture2D[]> faces, StringBuilder log, out bool isNew)
    {
        isNew = false;
        string path = $"{MAT_DIR}/SM_{charName}_Face.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        Shader shader = Shader.Find("TBR/FaceOverlay");
        if (shader == null)
        {
            log.AppendLine("  ERROR: no encuentro el shader 'TBR/FaceOverlay' (¿compiló FaceOverlay.shader?). Salto el material.");
            return null;
        }

        isNew = mat == null;
        if (isNew)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        // El rect inicial solo se pone en materiales nuevos, para no pisar tu ajuste manual al re-correr.
        if (isNew && mat.HasProperty("_FaceRect"))
            mat.SetVector("_FaceRect", new Vector4(0.6f, 0.4f, 0.3f, 0.3f));

        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", (float)CullMode.Back);
        if (mat.HasProperty("_AlphaCutoff")) mat.SetFloat("_AlphaCutoff", 0.02f);

        // Textura neutral por defecto para verla en el editor (en runtime la cambia CharacterFace por MPB).
        Texture2D def = NeutralTexture(faces);
        if (def != null && mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", def);
            mat.mainTexture = def;
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ---------------- Utilidades ----------------

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.ToLowerInvariant();
        var decomposed = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (char c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
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
