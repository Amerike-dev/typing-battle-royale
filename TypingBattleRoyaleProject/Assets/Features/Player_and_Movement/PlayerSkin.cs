using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Aplica el material de skin al modelo del jugador en TODOS los clientes.
/// El servidor define el colorIndex al spawnear (GameplayManager) y se sincroniza
/// vía NetworkVariable; cada cliente aplica skins[colorIndex] a sus renderers.
///
/// Va en el root del prefab de gameplay. La herramienta de Editor (CharacterSetupTool)
/// rellena 'renderers' (los SkinnedMeshRenderer del modelo) y 'skins' (3 materiales).
/// </summary>
public class PlayerSkin : NetworkBehaviour
{
    [Tooltip("Renderers del modelo a los que se aplica la skin.")]
    public Renderer[] renderers;

    [Tooltip("Materiales de skin indexados por colorIndex. Debe coincidir con el array del SkinInfo del personaje.")]
    public Material[] skins;

    public NetworkVariable<int> colorIndex =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        colorIndex.OnValueChanged += OnColorChanged;
        Apply(colorIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        colorIndex.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(int previous, int current) => Apply(current);

    /// <summary>Llamado por el servidor justo después de spawnear el jugador.</summary>
    public void ServerSetColor(int index)
    {
        if (!IsServer) return;
        colorIndex.Value = index;
        Apply(index); // aplica de inmediato en el server/host
    }

    private void Apply(int index)
    {
        if (skins == null || skins.Length == 0 || renderers == null) return;

        Material mat = skins[Mathf.Clamp(index, 0, skins.Length - 1)];
        if (mat == null) return;

        var face = GetComponentInChildren<CharacterFace>(true);

        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (face != null && face.IsFaceRenderer(r)) continue; // no pisamos la cara
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    /// <summary>
    /// Aplica un material a todos los renderers bajo 'root'. Usado por el Character Select
    /// (preview no networked) para colorear el modelo según el colorIndex local.
    /// </summary>
    public static void ApplyTo(GameObject root, Material mat)
    {
        if (root == null || mat == null) return;

        var faces = root.GetComponentsInChildren<CharacterFace>(true);

        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (IsFaceRenderer(r, faces)) continue; // la cara tiene su propio material/quad
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    private static bool IsFaceRenderer(Renderer r, CharacterFace[] faces)
    {
        if (faces == null) return false;
        foreach (var f in faces)
            if (f != null && f.IsFaceRenderer(r)) return true;
        return false;
    }
}
