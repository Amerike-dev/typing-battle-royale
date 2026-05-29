using UnityEngine;

/// <summary>
/// Iconos de controles para los overlays de instrucciones. Asset único en Resources
/// (SpellBookInstructions), cargado en runtime, así no hay que cablear sprites en cada escena/prefab.
/// </summary>
[CreateAssetMenu(fileName = "InstructionIcons", menuName = "Scriptable Objects/InstructionIcons")]
public class InstructionIcons : ScriptableObject
{
    [Tooltip("ws_0: seleccionar (flechas arriba/abajo).")]
    public Sprite selectIcon;
    [Tooltip("ws_1: tecla A (página anterior).")]
    public Sprite pageLeftIcon;
    [Tooltip("ws_2: tecla D (página siguiente).")]
    public Sprite pageRightIcon;
    [Tooltip("enterGroup: confirmar.")]
    public Sprite confirmIcon;
}
