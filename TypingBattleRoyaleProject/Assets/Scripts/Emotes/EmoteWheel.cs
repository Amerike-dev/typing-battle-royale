using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rueda radial de emotes (UI LOCAL, solo el jugador dueño). La construye PlayerController en runtime.
/// No tiene lógica de red: solo muestra los iconos en círculo y resalta el que apunta el selector.
/// </summary>
public class EmoteWheel : MonoBehaviour
{
    private readonly List<Image> _icons = new List<Image>();
    private RectTransform _root;
    private RectTransform _selector;
    private int _count;

    [SerializeField] private float _radius = 230f;
    [SerializeField] private float _deadzone = 45f;
    [SerializeField] private float _iconSize = 95f;

    /// <summary>Índice resaltado actualmente (-1 = ninguno / cancelar).</summary>
    public int CurrentIndex { get; private set; } = -1;

    public void Build(EmoteSet set)
    {
        var canvasGo = new GameObject("EmoteWheelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var rootGo = new GameObject("Wheel", typeof(RectTransform), typeof(CanvasGroup));
        _root = rootGo.GetComponent<RectTransform>();
        _root.SetParent(canvasGo.transform, false);
        _root.anchorMin = _root.anchorMax = _root.pivot = new Vector2(0.5f, 0.5f);
        _root.anchoredPosition = Vector2.zero;

        _count = set != null && set.emotes != null ? set.emotes.Length : 0;
        for (int i = 0; i < _count; i++)
        {
            float ang = Mathf.PI / 2f - i * (Mathf.PI * 2f / _count); // empieza arriba, sentido horario
            Vector2 pos = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * _radius;

            var iconGo = new GameObject("Emote_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = iconGo.GetComponent<RectTransform>();
            rt.SetParent(_root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(_iconSize, _iconSize);

            var img = iconGo.GetComponent<Image>();
            img.sprite = set.emotes[i] != null ? set.emotes[i].sprite : null;
            img.preserveAspect = true;
            img.raycastTarget = false;
            _icons.Add(img);
        }

        var selGo = new GameObject("Selector", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _selector = selGo.GetComponent<RectTransform>();
        _selector.SetParent(_root, false);
        _selector.anchorMin = _selector.anchorMax = _selector.pivot = new Vector2(0.5f, 0.5f);
        _selector.sizeDelta = new Vector2(30f, 30f);
        var selImg = selGo.GetComponent<Image>();
        selImg.color = new Color(1f, 1f, 1f, 0.65f);
        selImg.raycastTarget = false;

        gameObject.SetActive(false);
    }

    public void Open()
    {
        CurrentIndex = -1;
        gameObject.SetActive(true);
        if (_selector != null) _selector.anchoredPosition = Vector2.zero;
        Highlight(-1);
    }

    public void Close() => gameObject.SetActive(false);

    /// <summary>Mueve el selector y devuelve el índice apuntado (-1 si está en la zona muerta central).</summary>
    public int UpdateSelector(Vector2 dir)
    {
        if (_selector == null) return -1;

        if (dir.magnitude > _radius) dir = dir.normalized * _radius;
        _selector.anchoredPosition = dir;

        int idx = -1;
        if (dir.magnitude >= _deadzone && _count > 0)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float best = float.MaxValue;
            for (int i = 0; i < _count; i++)
            {
                float ea = (Mathf.PI / 2f - i * (Mathf.PI * 2f / _count)) * Mathf.Rad2Deg;
                float diff = Mathf.Abs(Mathf.DeltaAngle(ang, ea));
                if (diff < best) { best = diff; idx = i; }
            }
        }

        if (idx != CurrentIndex) { CurrentIndex = idx; Highlight(idx); }
        return CurrentIndex;
    }

    private void Highlight(int idx)
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i] == null) continue;
            bool on = i == idx;
            _icons[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            _icons[i].rectTransform.localScale = on ? Vector3.one * 1.3f : Vector3.one;
        }
    }
}
