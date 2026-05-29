using UnityEngine;

[CreateAssetMenu(fileName = "SkinInfo", menuName = "Scriptable Objects/SkinInfo")]
public class SkinInfo : ScriptableObject
{
    public string skinName;

    [Tooltip("Modelo visual para el Character Select (no networked). El material se aplica en runtime según colorIndex.")]
    public GameObject previewModel;

    [Tooltip("Prefab networked del jugador para Gameplay (registrado en DefaultNetworkPrefabs).")]
    public GameObject gameplayPrefab;

    [Tooltip("Materiales de skin indexados por colorIndex. La cantidad puede variar por personaje (Berry tiene 6; el resto 3).")]
    public Material[] skins;

    [Tooltip("Animator del personaje (idle/walk/cast).")]
    public RuntimeAnimatorController animator;
}
