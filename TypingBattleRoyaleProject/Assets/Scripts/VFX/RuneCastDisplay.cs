using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Muestra las runas (pentagramas) frente a la mano del personaje mientras se invoca un hechizo.
///
/// - El sprite de runa se elige por <see cref="Spell.elementType"/> (vía <see cref="RuneLibrary"/>).
/// - La cantidad y disposición de runas depende de <see cref="Spell.tier"/>:
///     · T1: una sola runa pequeña con glow.
///     · T2: tres runas a la misma altura pero distinta profundidad; las dos que rodean a la del
///           medio tienen mayor opacidad y la del medio es la que lleva el glow.
///     · T3: cinco runas concéntricas que crecen de tamaño (creciente).
/// - Todas las runas giran mientras dura la invocación.
/// - El glow es HDR (color > 1) y lo recoge el Bloom del Volume Profile.
///
/// El estado (activa / elemento / tier) se sincroniza por NetworkVariable con permiso de escritura
/// del Owner, de modo que TODOS los clientes ven las runas del personaje que castea (igual que la
/// animación de Cast). El Owner escribe al empezar/terminar el casteo; todos reconstruyen las runas
/// en OnValueChanged. También se reconstruye en OnNetworkSpawn para late-joiners.
/// </summary>
public class RuneCastDisplay : NetworkBehaviour
{
    [Header("Referencias")]
    [Tooltip("Punto donde nacen las runas. Suele ser un hijo de CastOrigin (la mano). Las runas se " +
             "crean en el plano XY local de este anchor y giran sobre su eje Z local.")]
    public Transform runeAnchor;

    [Tooltip("Biblioteca que mapea cada elemento con su sprite de runa.")]
    public RuneLibrary runeLibrary;

    [Tooltip("Material plantilla con el shader TBR/RuneGlow (HDR). Cada runa recibe una instancia " +
             "propia de este material para fijar su color HDR (glow) y opacidad.")]
    public Material runeMaterial;

    [Header("Tamaño")]
    [Tooltip("Escala base (uniforme) de cada runa, en unidades locales del anchor.")]
    public float baseRuneSize = 0.02f;

    [Header("Glow (HDR) y opacidad")]
    [Tooltip("Tinte base del glow. Normalmente blanco para respetar el color del sprite; se multiplica por la intensidad.")]
    [ColorUsage(false, false)] public Color baseTint = Color.white;
    [Tooltip("Multiplicador HDR para la runa que brilla (>1 hace que el Bloom genere glow).")]
    public float glowIntensity = 3.5f;
    [Tooltip("Multiplicador HDR para las runas que NO brillan (1 = sin glow).")]
    public float dimIntensity = 1f;
    [Range(0f, 1f)] [Tooltip("Opacidad de la runa que brilla.")]
    public float glowOpacity = 1f;
    [Range(0f, 1f)] [Tooltip("Opacidad de la runa central del T2 (la que brilla).")]
    public float t2MidOpacity = 0.85f;
    [Range(0f, 1f)] [Tooltip("Opacidad de las dos runas que rodean a la central en T2 (mayor opacidad).")]
    public float t2FlankOpacity = 1f;

    [Header("Disposición")]
    [Tooltip("Separación en profundidad (eje Z local) entre las runas que rodean a la central en T2.")]
    public float t2DepthOffset = 0.12f;
    [Tooltip("Escalas crecientes de las 5 runas del T3 (creciente). Multiplican a baseRuneSize.")]
    public float[] t3Scales = { 0.5f, 0.7f, 0.9f, 1.1f, 1.35f };
    [Tooltip("Separación en profundidad entre capas concéntricas del T3 (evita z-fighting).")]
    public float t3DepthStep = 0.05f;

    [Header("Rotación")]
    [Tooltip("Velocidad de giro de las runas, en grados por segundo.")]
    public float rotationSpeed = 60f;
    [Tooltip("Si está activo, las runas alternan el sentido de giro (capas pares vs impares).")]
    public bool alternateDirection = true;

    // ---- Estado de red ----
    private struct RuneState : INetworkSerializable, System.IEquatable<RuneState>
    {
        public bool active;
        public byte element; // (byte)Elements
        public byte tier;    // (byte)SpellTiers

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref active);
            s.SerializeValue(ref element);
            s.SerializeValue(ref tier);
        }

        public bool Equals(RuneState other) =>
            active == other.active && element == other.element && tier == other.tier;
    }

    private readonly NetworkVariable<RuneState> _state =
        new NetworkVariable<RuneState>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly List<Transform> _runes = new List<Transform>();
    private readonly List<float> _spins = new List<float>();
    private readonly List<Material> _matInstances = new List<Material>();
    private CastInputController _caster;

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += OnStateChanged;

        // Late-join / reconstrucción del estado actual.
        Apply(_state.Value);

        if (IsOwner)
        {
            _caster = GetComponent<CastInputController>();
            if (_caster == null) _caster = GetComponentInChildren<CastInputController>(true);
            if (_caster != null)
            {
                _caster.OnCastStarted += HandleCastStarted;
                _caster.OnCastEnded += HandleCastEnded;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        _state.OnValueChanged -= OnStateChanged;
        if (IsOwner && _caster != null)
        {
            _caster.OnCastStarted -= HandleCastStarted;
            _caster.OnCastEnded -= HandleCastEnded;
        }
        ClearRunes();
    }

    // ---- Owner: traduce el ciclo de casteo a estado de red ----
    private void HandleCastStarted(Spell spell)
    {
        if (spell == null || spell.elementType == Elements.None)
        {
            HandleCastEnded();
            return;
        }
        _state.Value = new RuneState
        {
            active = true,
            element = (byte)spell.elementType,
            tier = (byte)spell.tier
        };
    }

    private void HandleCastEnded()
    {
        _state.Value = new RuneState { active = false, element = 0, tier = 0 };
    }

    private void OnStateChanged(RuneState _, RuneState current) => Apply(current);

    // ---- Construcción visual (corre en todos los clientes) ----
    private void Apply(RuneState state)
    {
        ClearRunes();

        if (!state.active) return;
        if (runeAnchor == null || runeLibrary == null) return;

        Elements element = (Elements)state.element;
        Sprite sprite = runeLibrary.GetSprite(element);
        if (sprite == null) return;

        SpellTiers tier = (SpellTiers)state.tier;
        switch (tier)
        {
            case SpellTiers.T1: BuildTier1(sprite); break;
            case SpellTiers.T2: BuildTier2(sprite); break;
            case SpellTiers.T3: BuildTier3(sprite); break;
        }
    }

    private void BuildTier1(Sprite sprite)
    {
        // Una sola runa pequeña con glow.
        CreateRune(sprite, Vector3.zero, baseRuneSize, glowIntensity, glowOpacity, 0);
    }

    private void BuildTier2(Sprite sprite)
    {
        // Tres runas a la misma altura, distinta profundidad (eje Z local).
        // Las dos que rodean a la central: mayor opacidad, sin glow.
        // La central: glow.
        CreateRune(sprite, new Vector3(0f, 0f, +t2DepthOffset), baseRuneSize, dimIntensity, t2FlankOpacity, 0);
        CreateRune(sprite, Vector3.zero, baseRuneSize, glowIntensity, t2MidOpacity, 1);
        CreateRune(sprite, new Vector3(0f, 0f, -t2DepthOffset), baseRuneSize, dimIntensity, t2FlankOpacity, 2);
    }

    private void BuildTier3(Sprite sprite)
    {
        // Cinco runas concéntricas que crecen de tamaño (creciente). Cada una gira.
        int count = t3Scales != null && t3Scales.Length > 0 ? t3Scales.Length : 5;
        for (int i = 0; i < count; i++)
        {
            float scale = (t3Scales != null && i < t3Scales.Length ? t3Scales[i] : 1f) * baseRuneSize;
            float depth = i * t3DepthStep;
            CreateRune(sprite, new Vector3(0f, 0f, depth), scale, glowIntensity, glowOpacity, i);
        }
    }

    /// <param name="index">Índice de la capa; controla el sentido de giro cuando alternateDirection está activo.</param>
    private void CreateRune(Sprite sprite, Vector3 localPos, float scale, float intensity, float opacity, int index)
    {
        var go = new GameObject("Rune");
        var t = go.transform;
        t.SetParent(runeAnchor, false);
        t.localPosition = localPos;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        // Orden de dibujo estable: capas más "profundas" detrás.
        sr.sortingOrder = index;

        // Instancia de material por runa: el color HDR (glow + opacidad) se setea directo en el
        // material para que funcione con el SRP Batcher (un MaterialPropertyBlock sobre una
        // propiedad del CBUFFER UnityPerMaterial puede ser ignorado por el batcher).
        if (runeMaterial != null)
        {
            var mat = new Material(runeMaterial);
            Color c = baseTint * intensity; // HDR: rgb pueden superar 1 -> Bloom
            c.a = opacity;
            mat.SetColor(ColorId, c);
            sr.sharedMaterial = mat;
            _matInstances.Add(mat);
        }

        _runes.Add(t);
        // Sentido de giro: alterna por capa si está activado.
        _spins.Add(alternateDirection && (index % 2 == 1) ? -rotationSpeed : rotationSpeed);
    }

    private void ClearRunes()
    {
        for (int i = 0; i < _runes.Count; i++)
        {
            if (_runes[i] != null) Destroy(_runes[i].gameObject);
        }
        for (int i = 0; i < _matInstances.Count; i++)
        {
            if (_matInstances[i] != null) Destroy(_matInstances[i]);
        }
        _runes.Clear();
        _spins.Clear();
        _matInstances.Clear();
    }

    private void Update()
    {
        if (_runes.Count == 0) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < _runes.Count; i++)
        {
            var t = _runes[i];
            if (t == null) continue;
            // Gira en el plano del sprite (eje Z local) -> efecto de círculo mágico.
            t.Rotate(0f, 0f, _spins[i] * dt, Space.Self);
        }
    }
}
