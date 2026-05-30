using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emociones de la cara. Coinciden con los assets que dieron los artistas
/// (Assets/Artist/Animations/Faces/&lt;Personaje&gt;) y con los estados del Animator:
///   Neutral  -> Idle / Running / Interacción   (Neutra)
///   Casting  -> Ataque / Ataque_001              (Concentración_Casteando)
///   Hurt     -> RecibirDaño                       (Enojo_Recibiendo Daño)
///   Death    -> Muerte                            (Muerte)
///   Jump     -> Jump_001                          (Sorpresa_Saltando)
///   SpellFail-> (sin estado de animación)          (Tristeza_Fallando Hechizo)
/// </summary>
public enum FaceState { Neutral, Casting, Hurt, Death, Jump, SpellFail }

/// <summary>Una emoción con sus frames (1..n). Berry/Wander = 1, Ixia = 2, Klug = 3.</summary>
[System.Serializable]
public class FaceClip
{
    public FaceState state;
    public Texture2D[] frames;
}

/// <summary>
/// Dibuja la cara del personaje y la cambia según el estado del Animator, para que el modelo
/// parezca "vivo".
///
/// La cara se renderiza con mallas "overlay": copias de las SkinnedMeshRenderer del modelo que
/// comparten la MISMA malla, huesos y UVs, con un material transparente que muestra el PNG de la
/// cara. Como los PNG son overlays de UV completos (la cara está en su isla de UV y el resto es
/// transparente), la cara aparece exactamente donde está mapeada en la cabeza y se DEFORMA igual
/// que el modelo durante las animaciones. Aquí solo intercambiamos la textura por estado.
///
/// Lee el estado directamente del Animator (que el NetworkAnimator ya replica), así que las caras
/// se ven bien en el jugador local y en los remotos. El cambio de frames dentro de una emoción
/// (Ixia/Klug) da el efecto de parpadeo/idle.
///
/// La textura se aplica con un MaterialPropertyBlock, así que no crea instancias de material ni
/// interfiere con el cambio de skin (PlayerSkin ignora estos renderers).
///
/// Lo monta la herramienta de Editor "Tools/Characters/Setup Character Faces" (CharacterFaceSetupTool).
/// </summary>
[DisallowMultipleComponent]
public class CharacterFace : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Animator del modelo. Si se deja vacío se busca en los hijos.")]
    public Animator animator;

    [Tooltip("Renderers overlay de la cara (copias de la malla con el material de cara). Los crea la herramienta de Editor.")]
    public Renderer[] faceRenderers;

    [Header("Caras por emoción")]
    public List<FaceClip> clips = new List<FaceClip>();

    [Header("Animación de frames")]
    [Tooltip("Segundos entre frames cuando una emoción tiene varios (efecto 'vivo').")]
    [Min(0.01f)] public float frameInterval = 0.18f;

    [Tooltip("Reproduce los frames en ida y vuelta (1-2-3-2-1) en vez de en bucle (1-2-3-1).")]
    public bool pingPong = true;

    [Header("Gestos puntuales (sin estado de animación)")]
    [Tooltip("Cuánto dura la cara de 'fallo de hechizo' (Tristeza) cuando se dispara por código.")]
    public float spellFailDuration = 1.2f;

    [Header("Nombres de estado del Animator")]
    public string[] deathStates = { "Muerte" };
    public string[] hurtStates = { "RecibirDaño" };
    public string[] jumpStates = { "Jump_001" };
    public string[] castStates = { "Ataque", "Ataque_001" };

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    private readonly Dictionary<FaceState, Texture2D[]> _byState = new Dictionary<FaceState, Texture2D[]>();
    private int[] _deathHashes, _hurtHashes, _jumpHashes, _castHashes;

    private MaterialPropertyBlock _mpb;
    private FaceState _current = FaceState.Neutral;
    private int _frame;
    private int _dir = 1;
    private float _timer;

    private FaceState _oneShotState;
    private float _oneShotUntil = -1f;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        _mpb = new MaterialPropertyBlock();

        _deathHashes = ToHashes(deathStates);
        _hurtHashes = ToHashes(hurtStates);
        _jumpHashes = ToHashes(jumpStates);
        _castHashes = ToHashes(castStates);

        _byState.Clear();
        foreach (var clip in clips)
        {
            if (clip == null || clip.frames == null || clip.frames.Length == 0) continue;
            _byState[clip.state] = clip.frames;
        }
    }

    private void OnEnable()
    {
        _current = FaceState.Neutral;
        _frame = 0;
        _dir = 1;
        _timer = 0f;
        ApplyFrame();
    }

    private void Update()
    {
        if (!HasRenderers()) return;

        FaceState target = ResolveTarget();

        if (target != _current)
        {
            _current = target;
            _frame = 0;
            _dir = 1;
            _timer = 0f;
            ApplyFrame();
            return;
        }

        AdvanceFrames();
    }

    private FaceState ResolveTarget()
    {
        int hash = animator != null ? animator.GetCurrentAnimatorStateInfo(0).shortNameHash : 0;

        // La muerte manda siempre, por encima de cualquier gesto puntual.
        if (Contains(_deathHashes, hash)) return FaceState.Death;

        // Gesto puntual disparado por código (p.ej. fallo de hechizo).
        if (_oneShotUntil > Time.time) return _oneShotState;

        if (Contains(_hurtHashes, hash)) return FaceState.Hurt;
        if (Contains(_jumpHashes, hash)) return FaceState.Jump;
        if (Contains(_castHashes, hash)) return FaceState.Casting;
        return FaceState.Neutral;
    }

    private void AdvanceFrames()
    {
        var frames = GetFrames(_current);
        if (frames == null || frames.Length <= 1) return;

        _timer += Time.deltaTime;
        if (_timer < frameInterval) return;
        _timer -= frameInterval;

        if (pingPong)
        {
            _frame += _dir;
            if (_frame >= frames.Length - 1) { _frame = frames.Length - 1; _dir = -1; }
            else if (_frame <= 0) { _frame = 0; _dir = 1; }
        }
        else
        {
            _frame = (_frame + 1) % frames.Length;
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        if (!HasRenderers()) return;

        var frames = GetFrames(_current);
        if (frames == null || frames.Length == 0) return;

        int idx = Mathf.Clamp(_frame, 0, frames.Length - 1);
        Texture tex = frames[idx];
        if (tex == null) return;

        for (int i = 0; i < faceRenderers.Length; i++)
        {
            var r = faceRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetTexture(BaseMapId, tex);
            _mpb.SetTexture(MainTexId, tex);
            r.SetPropertyBlock(_mpb);
        }
    }

    private bool HasRenderers() => faceRenderers != null && faceRenderers.Length > 0;

    /// <summary>Devuelve los frames de un estado, con respaldo a Neutral si no existen.</summary>
    private Texture2D[] GetFrames(FaceState state)
    {
        if (_byState.TryGetValue(state, out var f) && f != null && f.Length > 0) return f;
        if (_byState.TryGetValue(FaceState.Neutral, out var n) && n != null && n.Length > 0) return n;
        return null;
    }

    // ---------------- API pública para gestos sin estado de animación ----------------

    /// <summary>Muestra una emoción durante 'duration' segundos por encima del estado del Animator (salvo Muerte).</summary>
    public void PlayOneShot(FaceState state, float duration)
    {
        _oneShotState = state;
        _oneShotUntil = Time.time + Mathf.Max(0f, duration);
    }

    /// <summary>Cara de "fallo de hechizo" (Tristeza). Llámalo cuando el jugador falla el casteo/typing.</summary>
    public void PlaySpellFail() => PlayOneShot(FaceState.SpellFail, spellFailDuration);

    /// <summary>True si el renderer es uno de los overlays de cara (lo usa PlayerSkin para no pisarlo).</summary>
    public bool IsFaceRenderer(Renderer r)
    {
        if (r == null || faceRenderers == null) return false;
        for (int i = 0; i < faceRenderers.Length; i++) if (faceRenderers[i] == r) return true;
        return false;
    }

    private static int[] ToHashes(string[] names)
    {
        if (names == null) return System.Array.Empty<int>();
        var hashes = new int[names.Length];
        for (int i = 0; i < names.Length; i++) hashes[i] = Animator.StringToHash(names[i]);
        return hashes;
    }

    private static bool Contains(int[] set, int value)
    {
        if (set == null) return false;
        for (int i = 0; i < set.Length; i++) if (set[i] == value) return true;
        return false;
    }
}
