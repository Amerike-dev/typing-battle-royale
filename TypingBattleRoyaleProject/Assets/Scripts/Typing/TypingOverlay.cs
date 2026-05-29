using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Overlay de typeo reutilizable (monolito y casteo). Crea su propio canvas ScreenSpaceOverlay de alto
/// sorting (tapa el HUD y la escena), con un fondo full-screen, un header arriba y el hechizo pintado como
/// una cadena de "teclas" (sprites): d6 1 = pendiente, d6_outline 1 = ya escrita. Letra por letra, con
/// wrap por palabra. El error pinta la tecla actual en rojo y la sacude.
///
/// No tiene lógica de typeo: los controllers (MonolithTypingChallenge / CastInputController) llaman
/// Show(header, texto) al empezar y UpdateProgress(index, hasError) en cada tecla.
/// </summary>
public class TypingOverlay : MonoBehaviour
{
    [SerializeField] private float keySize = 70f;
    [SerializeField] private float keySpacing = 8f;
    [SerializeField] private float spaceWidth = 40f;
    [SerializeField] private float lineSpacing = 16f;
    [SerializeField] private float maxRowWidth = 1300f;
    [SerializeField] private int sortingOrder = 30;
    [SerializeField] private Color bgColor = new Color(0.04f, 0.03f, 0.08f, 1f);
    [Tooltip("Color de la letra en teclas NO escritas (pendientes).")]
    [SerializeField] private Color pendingLetterColor = Color.black;
    [Tooltip("Color de la letra en teclas escritas y en la actual.")]
    [SerializeField] private Color activeLetterColor = Color.white;
    [SerializeField] private Color pendingTint = Color.white;
    [SerializeField] private Color typedTint = new Color(0.55f, 1f, 0.7f, 1f);
    [SerializeField] private Color errorTint = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float headerFontSize = 52f;

    private KeycapTheme _theme;
    private Canvas _canvas;
    private Image _bgImage;
    private RectTransform _root;
    private RectTransform _keysContainer;
    private TextMeshProUGUI _header;
    private RectTransform _typedPanel;
    private TextMeshProUGUI _typedText;

    private string _text = "";
    private Image[] _keyImg;
    private TextMeshProUGUI[] _keyLetter;
    private Vector2[] _basePos;
    private bool[] _isSpace;
    private Color _currentColor = new Color(1f, 0.95f, 0.4f, 1f); // tinte de la tecla actual (color del elemento)
    private Coroutine _shake;

    private void EnsureBuilt()
    {
        if (_canvas != null) return;

        _theme = Resources.Load<KeycapTheme>("KeycapTheme");

        var go = new GameObject("TypingOverlayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        _canvas = go.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Fondo full-screen (tapa HUD + escena).
        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.SetParent(go.transform, false);
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        _bgImage = bgGo.GetComponent<Image>();
        _bgImage.color = bgColor;
        _bgImage.raycastTarget = false; // puramente visual; no roba el foco del InputField

        var rootGo = new GameObject("Content", typeof(RectTransform));
        _root = rootGo.GetComponent<RectTransform>();
        _root.SetParent(go.transform, false);
        _root.anchorMin = Vector2.zero; _root.anchorMax = Vector2.one;
        _root.offsetMin = Vector2.zero; _root.offsetMax = Vector2.zero;

        // Header (arriba, fuera del área de teclas).
        var hGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer));
        var hRt = hGo.GetComponent<RectTransform>();
        hRt.SetParent(_root, false);
        hRt.anchorMin = new Vector2(0.5f, 1f); hRt.anchorMax = new Vector2(0.5f, 1f); hRt.pivot = new Vector2(0.5f, 1f);
        hRt.anchoredPosition = new Vector2(0f, -120f);
        hRt.sizeDelta = new Vector2(1700f, 110f);
        _header = hGo.AddComponent<TextMeshProUGUI>();
        if (_theme != null && _theme.font != null) _header.font = _theme.font;
        _header.fontSize = headerFontSize;
        _header.alignment = TextAlignmentOptions.Center;
        _header.color = Color.white;
        _header.raycastTarget = false;

        // Contenedor de teclas (centrado).
        var kGo = new GameObject("Keys", typeof(RectTransform));
        _keysContainer = kGo.GetComponent<RectTransform>();
        _keysContainer.SetParent(_root, false);
        _keysContainer.anchorMin = _keysContainer.anchorMax = _keysContainer.pivot = new Vector2(0.5f, 0.5f);
        _keysContainer.anchoredPosition = Vector2.zero;

        // Panel del texto crudo tipeado (visible en casteo para poder corregir; oculto en monolito).
        var tpGo = new GameObject("TypedPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _typedPanel = tpGo.GetComponent<RectTransform>();
        _typedPanel.SetParent(_root, false);
        _typedPanel.anchorMin = new Vector2(0.5f, 0f);
        _typedPanel.anchorMax = new Vector2(0.5f, 0f);
        _typedPanel.pivot = new Vector2(0.5f, 0f);
        _typedPanel.anchoredPosition = new Vector2(0f, 120f);
        _typedPanel.sizeDelta = new Vector2(950f, 96f);
        var tpImg = tpGo.GetComponent<Image>();
        tpImg.color = new Color(0f, 0f, 0f, 0.55f);
        tpImg.raycastTarget = false;

        var ttGo = new GameObject("TypedText", typeof(RectTransform), typeof(CanvasRenderer));
        var ttRt = ttGo.GetComponent<RectTransform>();
        ttRt.SetParent(_typedPanel, false);
        ttRt.anchorMin = Vector2.zero; ttRt.anchorMax = Vector2.one;
        ttRt.offsetMin = new Vector2(20f, 8f); ttRt.offsetMax = new Vector2(-20f, -8f);
        _typedText = ttGo.AddComponent<TextMeshProUGUI>();
        if (_theme != null && _theme.font != null) _typedText.font = _theme.font;
        _typedText.fontSize = 44f;
        _typedText.alignment = TextAlignmentOptions.Center;
        _typedText.color = Color.white;
        _typedText.raycastTarget = false;
        _typedPanel.gameObject.SetActive(false);

        go.SetActive(false);
    }

    public void Show(string header, string text, Color currentColor, float backgroundAlpha, bool showTypedText)
    {
        EnsureBuilt();
        _currentColor = currentColor;
        if (_bgImage != null)
            _bgImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, Mathf.Clamp01(backgroundAlpha));
        if (_typedPanel != null)
        {
            _typedPanel.gameObject.SetActive(showTypedText);
            if (_typedText != null) _typedText.text = "";
        }
        _canvas.gameObject.SetActive(true);
        if (_header != null) _header.text = header;
        BuildKeys(text);
        UpdateProgress(0, false);
    }

    /// <summary>Actualiza el panel con el texto crudo que está escribiendo el jugador (para corregir en casteo).</summary>
    public void UpdateTypedText(string typed)
    {
        if (_typedText != null) _typedText.text = typed ?? "";
    }

    public void Hide()
    {
        if (_shake != null) { StopCoroutine(_shake); _shake = null; }
        if (_canvas != null) _canvas.gameObject.SetActive(false);
    }

    private void BuildKeys(string text)
    {
        _text = text ?? "";

        for (int i = _keysContainer.childCount - 1; i >= 0; i--)
            Destroy(_keysContainer.GetChild(i).gameObject);

        int n = _text.Length;
        _keyImg = new Image[n];
        _keyLetter = new TextMeshProUGUI[n];
        _basePos = new Vector2[n];
        _isSpace = new bool[n];

        // Tokens: cada palabra (letras contiguas) y cada espacio se tratan como una unidad
        // independiente. Así el espacio es una "tecla" visible (espacio.png) y, además, sirve como
        // punto de corte para el wrap (cuando hay palabras largas no se arman renglones eternos).
        // (isSpace, inicio, longitud)
        var tokens = new List<(bool isSpace, int start, int len)>();
        for (int i = 0; i < n; )
        {
            if (_text[i] == ' ')
            {
                tokens.Add((true, i, 1));
                i++;
            }
            else
            {
                int s = i;
                while (i < n && _text[i] != ' ') i++;
                tokens.Add((false, s, i - s));
            }
        }

        float WordWidth(int len) => len * keySize + Mathf.Max(0, len - 1) * keySpacing;
        float TokenWidth((bool isSpace, int start, int len) t) => t.isSpace ? keySize : WordWidth(t.len);

        // Wrap por token. Todo va separado por keySpacing (incluido el espacio); un espacio que
        // caería al inicio de un renglón se descarta para no dejarlo colgando.
        var rows = new List<List<int>>();
        var rowWidths = new List<float>();
        var current = new List<int>();
        float curW = 0f;
        for (int ti = 0; ti < tokens.Count; ti++)
        {
            float tw = TokenWidth(tokens[ti]);

            if (current.Count == 0)
            {
                if (tokens[ti].isSpace) continue; // no iniciar renglón con un espacio
                current.Add(ti); curW = tw;
                continue;
            }

            float add = keySpacing + tw;
            if (curW + add > maxRowWidth)
            {
                rows.Add(current); rowWidths.Add(curW);
                current = new List<int>(); curW = 0f;
                if (tokens[ti].isSpace) continue; // el espacio sirvió de corte; no lo dibujamos
                current.Add(ti); curW = tw;
                continue;
            }

            current.Add(ti); curW += add;
        }
        if (current.Count > 0) { rows.Add(current); rowWidths.Add(curW); }

        int rowCount = rows.Count;
        float totalH = rowCount * keySize + Mathf.Max(0, rowCount - 1) * lineSpacing;
        float topY = totalH * 0.5f - keySize * 0.5f;

        for (int r = 0; r < rowCount; r++)
        {
            float rowW = rowWidths[r];
            float cursorLeft = -rowW * 0.5f;
            float y = topY - r * (keySize + lineSpacing);
            var row = rows[r];

            for (int k = 0; k < row.Count; k++)
            {
                if (k > 0) cursorLeft += keySpacing;
                var t = tokens[row[k]];

                if (t.isSpace)
                {
                    float center = cursorLeft + keySize * 0.5f;
                    CreateKey(t.start, new Vector2(center, y), true);
                    cursorLeft += keySize;
                }
                else
                {
                    for (int c = 0; c < t.len; c++)
                    {
                        if (c > 0) cursorLeft += keySpacing;
                        int idx = t.start + c;
                        float center = cursorLeft + keySize * 0.5f;
                        CreateKey(idx, new Vector2(center, y), false);
                        cursorLeft += keySize;
                    }
                }
            }
        }
    }

    private void CreateKey(int idx, Vector2 pos, bool isSpace)
    {
        var go = new GameObject("Key_" + idx, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(_keysContainer, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(keySize, keySize);
        rt.anchoredPosition = pos;

        var img = go.GetComponent<Image>();
        if (isSpace) img.sprite = _theme != null ? _theme.spaceKey : null;
        else img.sprite = _theme != null ? _theme.filledKey : null;
        // La tecla de espacio se dibuja cuadrada (estiramos para corregir el "apretado" en Y del png).
        img.preserveAspect = !isSpace;
        img.raycastTarget = false;
        _keyImg[idx] = img;
        _basePos[idx] = pos;
        _isSpace[idx] = isSpace;

        if (isSpace) return; // el espacio no lleva letra encima

        var lGo = new GameObject("L", typeof(RectTransform), typeof(CanvasRenderer));
        var lRt = lGo.GetComponent<RectTransform>();
        lRt.SetParent(rt, false);
        lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
        lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
        var tmp = lGo.AddComponent<TextMeshProUGUI>();
        if (_theme != null && _theme.font != null) tmp.font = _theme.font;
        tmp.text = char.ToUpper(_text[idx]).ToString();
        tmp.fontSize = keySize * 0.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = pendingLetterColor;
        tmp.raycastTarget = false;
        _keyLetter[idx] = tmp;
    }

    public void UpdateProgress(int currentIndex, bool hasError)
    {
        if (_keyImg == null) return;

        for (int i = 0; i < _keyImg.Length; i++)
        {
            var img = _keyImg[i];
            if (img == null) continue;

            bool space = _isSpace != null && i < _isSpace.Length && _isSpace[i];

            if (i < currentIndex)
            {
                if (!space && _theme != null) img.sprite = _theme.outlineKey;
                img.color = typedTint;
                img.rectTransform.anchoredPosition = _basePos[i];
                img.rectTransform.localScale = Vector3.one;
                if (_keyLetter[i] != null) _keyLetter[i].color = activeLetterColor;
            }
            else if (i == currentIndex)
            {
                if (!space && _theme != null) img.sprite = _theme.filledKey;
                img.color = hasError ? errorTint : _currentColor; // color del elemento del hechizo
                img.rectTransform.localScale = Vector3.one * 1.12f;
                if (_keyLetter[i] != null) _keyLetter[i].color = activeLetterColor;
            }
            else
            {
                if (!space && _theme != null) img.sprite = _theme.filledKey;
                img.color = pendingTint;
                img.rectTransform.anchoredPosition = _basePos[i];
                img.rectTransform.localScale = Vector3.one;
                if (_keyLetter[i] != null) _keyLetter[i].color = pendingLetterColor; // no escrita -> negro
            }
        }

        if (hasError && currentIndex >= 0 && currentIndex < _keyImg.Length && _keyImg[currentIndex] != null)
        {
            if (_shake != null) StopCoroutine(_shake);
            _shake = StartCoroutine(ShakeKey(currentIndex));
        }
    }

    private IEnumerator ShakeKey(int idx)
    {
        var rt = _keyImg[idx].rectTransform;
        Vector2 baseP = _basePos[idx];
        float t = 0f;
        const float dur = 0.35f;
        while (t < dur && _keyImg != null && idx < _keyImg.Length && _keyImg[idx] != null)
        {
            t += Time.unscaledDeltaTime;
            float damp = 1f - (t / dur);
            float off = Mathf.Sin(t * 60f) * 10f * damp;
            rt.anchoredPosition = baseP + new Vector2(off, 0f);
            yield return null;
        }
        if (_keyImg != null && idx < _keyImg.Length && _keyImg[idx] != null)
            rt.anchoredPosition = baseP;
        _shake = null;
    }

    /// <summary>Color representativo de cada elemento (misma paleta que el glow del monolito).</summary>
    public static Color ElementColor(Elements element)
    {
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
            default:               return new Color(0.35f, 0.70f, 1.0f);
        }
    }
}
