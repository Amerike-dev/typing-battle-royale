using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Aplica un glow emisivo al monolito y genera por código un sistema de partículas
/// (aura de polvo místico que sube). Ambos se colorean según el elemento del monolito,
/// leído desde <see cref="MonolithController.NetworkElement"/> para que sea idéntico en todos los clientes.
///
/// Se agrega como componente al prefab del Monolith. Requiere un Renderer con el shader
/// Custom/SimpleToonShader (que expone _EmissionColor / _EmissionStrength).
/// </summary>
[RequireComponent(typeof(MonolithController))]
public class MonolithAura : NetworkBehaviour
{
    [System.Serializable]
    public struct ElementColor
    {
        public Elements element;
        [ColorUsage(true, true)] public Color color;
    }

    [Header("Glow")]
    [Tooltip("Fuerza de la emisión del monolito (alimenta el Bloom de URP). 0 = sin glow.")]
    public float glowStrength = 2.5f;
    [Tooltip("Intensidad del brillo de bordes (fresnel/rim) del shader.")]
    public float fresnelStrength = 4f;

    [Header("Partículas (aura de polvo)")]
    public bool generateParticles = true;
    [Tooltip("Partículas emitidas por segundo.")]
    public float emissionRate = 14f;
    [Tooltip("Radio del disco de emisión en la base, en unidades de mundo.")]
    public float emitRadius = 10f;
    [Tooltip("Velocidad de subida en unidades de mundo por segundo.")]
    public float riseSpeed = 8f;
    [Tooltip("Tamaño de cada partícula en unidades de mundo.")]
    public float particleSize = 3f;
    [Tooltip("Segundos que vive cada partícula (define cuánto sube).")]
    public float particleLifetime = 4f;
    [Tooltip("Desplazamiento vertical del emisor respecto al pivote del monolito (mundo).")]
    public float baseHeightOffset = 0f;

    [Header("Paleta por elemento (opcional)")]
    [Tooltip("Sobrescribe los colores por defecto. Si un elemento no está aquí, se usa la paleta interna.")]
    public List<ElementColor> palette = new List<ElementColor>();

    private MonolithController _controller;
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private ParticleSystem _aura;
    private ParticleSystemRenderer _auraRenderer;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");
    private static readonly int FresnelStrengthId = Shader.PropertyToID("_FresnelStrength");

    void Awake()
    {
        _controller = GetComponent<MonolithController>();
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public override void OnNetworkSpawn()
    {
        // En un servidor dedicado (sin cliente) no hace falta nada visual.
        bool dedicatedServer = IsServer && !IsClient;
        if (dedicatedServer) return;

        if (generateParticles) BuildAura();

        if (_controller != null)
        {
            _controller.NetworkElement.OnValueChanged += OnElementChanged;
            Apply(_controller.NetworkElement.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_controller != null)
            _controller.NetworkElement.OnValueChanged -= OnElementChanged;
    }

    private void OnElementChanged(Elements previous, Elements current) => Apply(current);

    private void Apply(Elements element)
    {
        Color color = GetColor(element);

        // --- Glow del monolito (por instancia, sin tocar el material compartido) ---
        if (_renderer != null)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorId, color);
            _mpb.SetFloat(EmissionStrengthId, glowStrength);
            _mpb.SetFloat(FresnelStrengthId, fresnelStrength);
            _renderer.SetPropertyBlock(_mpb);
        }

        // --- Color del aura ---
        if (_aura != null)
        {
            var main = _aura.main;
            Color particleColor = color;
            particleColor.a = 0.55f;
            main.startColor = particleColor;

            var col = _aura.colorOverLifetime;
            col.enabled = true;
            col.color = BuildFadeGradient(color);
        }
    }

    private Color GetColor(Elements element)
    {
        foreach (var entry in palette)
            if (entry.element == element) return entry.color;

        // Paleta por defecto (colores LDR; el brillo lo da glowStrength + Bloom).
        switch (element)
        {
            case Elements.Fire:    return new Color(1.0f, 0.35f, 0.10f);
            case Elements.Water:   return new Color(0.20f, 0.55f, 1.0f);
            case Elements.Earth:   return new Color(0.55f, 0.38f, 0.18f);
            case Elements.Wind:    return new Color(0.70f, 1.0f, 0.85f);
            case Elements.Nature:  return new Color(0.30f, 1.0f, 0.35f);
            case Elements.Thunder: return new Color(1.0f, 0.92f, 0.30f);
            case Elements.Ice:     return new Color(0.60f, 0.90f, 1.0f);
            case Elements.Lava:    return new Color(1.0f, 0.25f, 0.05f);
            case Elements.Dark:    return new Color(0.55f, 0.15f, 0.80f);
            case Elements.Light:   return new Color(1.0f, 0.95f, 0.80f);
            default:               return new Color(0.35f, 0.70f, 1.0f); // None: cian místico
        }
    }

    // --- Generación del sistema de partículas por código ---
    private void BuildAura()
    {
        var go = new GameObject("AuraVFX");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        // El padre está rotado -90° en X y escalado x50. Alineamos el emisor con el mundo
        // (rotación identidad) para que el disco de emisión quede horizontal y el polvo suba recto.
        go.transform.rotation = Quaternion.identity;

        _aura = go.AddComponent<ParticleSystem>();
        _aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _aura.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local; // ignora el escalado x50 del padre
        main.startLifetime = particleLifetime;
        main.startSpeed = 0f; // la subida la da Velocity over Lifetime (en mundo)
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 200;
        main.playOnAwake = true;

        var emission = _aura.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;

        var shape = _aura.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Donut; // disco hueco en la base
        shape.radius = emitRadius;
        shape.donutRadius = emitRadius * 0.5f;
        shape.position = new Vector3(0f, baseHeightOffset, 0f);

        // Subida real en espacio de mundo.
        var vel = _aura.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.y = new ParticleSystem.MinMaxCurve(riseSpeed * 0.6f, riseSpeed);
        vel.x = new ParticleSystem.MinMaxCurve(-riseSpeed * 0.1f, riseSpeed * 0.1f);
        vel.z = new ParticleSystem.MinMaxCurve(-riseSpeed * 0.1f, riseSpeed * 0.1f);

        // Crece un poco al subir y se desvanece.
        var sol = _aura.sizeOverLifetime;
        sol.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.4f, 1f),
            new Keyframe(1f, 0.7f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        _auraRenderer = go.GetComponent<ParticleSystemRenderer>();
        _auraRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        _auraRenderer.material = BuildAuraMaterial();
        _auraRenderer.sortingFudge = 0f;

        _aura.Play();
    }

    private Gradient BuildFadeGradient(Color color)
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.6f, 0.25f),
                new GradientAlphaKey(0.35f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            });
        return g;
    }

    private static Texture2D _softDot;

    private Material BuildAuraMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        var mat = new Material(shader);

        if (_softDot == null) _softDot = CreateSoftDotTexture(64);

        // URP Particles/Unlit usa _BaseMap/_BaseColor; Sprites/Default usa _MainTex.
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _softDot);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", _softDot);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

        // Blending aditivo para un look luminoso/místico.
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
        if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.One);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;

        return mat;
    }

    private static Texture2D CreateSoftDotTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        float center = (size - 1) * 0.5f;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Falloff suave tipo niebla.
                float a = Mathf.Clamp01(1f - dist);
                a = a * a;
                pixels[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
