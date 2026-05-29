using UnityEngine;
using TMPro;

/// <summary>
/// Tema visual para el typeo por teclas-sprite. Asset único en Resources, cargado por TypingOverlay,
/// así no hay que cablear sprites/fuente en cada prefab/escena.
/// </summary>
[CreateAssetMenu(fileName = "KeycapTheme", menuName = "Scriptable Objects/KeycapTheme")]
public class KeycapTheme : ScriptableObject
{
    [Tooltip("Tecla pendiente (aún no escrita) — d6 1.")]
    public Sprite filledKey;
    [Tooltip("Tecla ya escrita — d6_outline 1.")]
    public Sprite outlineKey;
    [Tooltip("Fuente de las letras (Gontserrat Bold).")]
    public TMP_FontAsset font;
}
