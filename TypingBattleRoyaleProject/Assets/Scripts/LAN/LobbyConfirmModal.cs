using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyConfirmModal : MonoBehaviour
{
    [Header("References (optional - autobuilt si quedan vacíos)")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_Text _acceptLabel;
    [SerializeField] private TMP_Text _cancelLabel;

    [Header("Style")]
    [SerializeField] private Color _backdropColor = new Color(0f, 0f, 0f, 0.92f);
    [SerializeField] private Color _panelColor = new Color(0.07f, 0.07f, 0.11f, 0.97f);
    [SerializeField] private Color _acceptColor = new Color(0.2f, 0.56f, 0.27f, 1f);
    [SerializeField] private Color _cancelColor = new Color(0.6f, 0.18f, 0.18f, 1f);
    [SerializeField] private Vector2 _panelSize = new Vector2(480f, 210f);

    private Action _onConfirm;
    private Action _onCancel;
    private bool _wired;

    private void Awake()
    {
        WireListeners();
        HideImmediate();
    }

    private void WireListeners()
    {
        if (_wired) return;

        if (_acceptButton != null) _acceptButton.onClick.AddListener(HandleAccept);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(HandleCancel);

        _wired = _acceptButton != null && _cancelButton != null;
    }

    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        ShowInternal(message, onConfirm, onCancel, "Aceptar", "Cancelar", showCancel: true);
    }

    public void ShowInfo(string message, Action onClose = null, string okLabel = "OK")
    {
        ShowInternal(message, onClose, null, okLabel, null, showCancel: false);
    }

    private void ShowInternal(string message, Action onConfirm, Action onCancel, string acceptLabel, string cancelLabel, bool showCancel)
    {
        if (!_wired) WireListeners();

        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (_messageText != null) _messageText.text = message;

        if (_acceptLabel != null && !string.IsNullOrEmpty(acceptLabel)) _acceptLabel.text = acceptLabel;
        if (_cancelLabel != null && !string.IsNullOrEmpty(cancelLabel)) _cancelLabel.text = cancelLabel;
        if (_cancelButton != null) _cancelButton.gameObject.SetActive(showCancel);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }
    }

    public void Hide()
    {
        HideImmediate();
        _onConfirm = null;
        _onCancel = null;
    }

    private void HideImmediate()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
        gameObject.SetActive(false);
    }

    private void HandleAccept()
    {
        Action cb = _onConfirm;
        Hide();
        cb?.Invoke();
    }

    private void HandleCancel()
    {
        Action cb = _onCancel;
        Hide();
        cb?.Invoke();
    }

    public static LobbyConfirmModal BuildRuntime(Transform canvasParent, TMP_FontAsset font, Sprite buttonSprite)
    {
        GameObject root = new GameObject("ConfirmModal", typeof(RectTransform), typeof(CanvasGroup), typeof(LobbyConfirmModal));
        root.transform.SetParent(canvasParent, false);

        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        LobbyConfirmModal modal = root.GetComponent<LobbyConfirmModal>();
        modal._canvasGroup = root.GetComponent<CanvasGroup>();

        GameObject backdrop = new GameObject("Backdrop", typeof(Image), typeof(Button));
        backdrop.transform.SetParent(root.transform, false);
        StretchFill(backdrop.GetComponent<RectTransform>());
        backdrop.GetComponent<Image>().color = modal._backdropColor;

        Button backdropBtn = backdrop.GetComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(modal.HandleCancel);

        GameObject panel = new GameObject("Panel", typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(root.transform, false);

        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = Vector2.zero;
        prt.sizeDelta = modal._panelSize;

        Image panelImg = panel.GetComponent<Image>();
        panelImg.color = modal._panelColor;
        if (buttonSprite != null) panelImg.sprite = buttonSprite;
        panelImg.type = Image.Type.Sliced;

        VerticalLayoutGroup vlg = panel.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        modal._messageText = CreateText(panel.transform, "Message", "...", font, 20, Color.white);
        modal._messageText.alignment = TextAlignmentOptions.Center;
        LayoutElement msgLe = modal._messageText.gameObject.AddComponent<LayoutElement>();
        msgLe.flexibleHeight = 1f;
        msgLe.minHeight = 64f;

        GameObject row = new GameObject("ButtonsRow", typeof(HorizontalLayoutGroup));
        row.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 14f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        LayoutElement rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 46f;

        modal._cancelButton = CreateActionButton(row.transform, "Cancelar", modal._cancelColor, font, buttonSprite, out modal._cancelLabel);
        modal._acceptButton = CreateActionButton(row.transform, "Aceptar", modal._acceptColor, font, buttonSprite, out modal._acceptLabel);

        modal._wired = false;
        modal.WireListeners();
        modal.HideImmediate();

        return modal;
    }

    public void SetButtonLabels(string acceptLabel, string cancelLabel)
    {
        if (_acceptLabel != null && !string.IsNullOrEmpty(acceptLabel)) _acceptLabel.text = acceptLabel;
        if (_cancelLabel != null && !string.IsNullOrEmpty(cancelLabel)) _cancelLabel.text = cancelLabel;
    }

    private static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string content, TMP_FontAsset font, float size, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        return tmp;
    }

    private static Button CreateActionButton(Transform parent, string label, Color tint, TMP_FontAsset font, Sprite sprite, out TMP_Text labelText)
    {
        GameObject go = new GameObject($"{label}Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.color = tint;
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = 42f;

        labelText = CreateText(go.transform, "Label", label, font, 18f, Color.white);
        RectTransform lrt = labelText.rectTransform;
        StretchFill(lrt);

        Button btn = go.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        btn.colors = colors;

        return btn;
    }
}
