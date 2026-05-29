using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Agrega un OwnerNetworkAnimator (NetworkAnimator con autoridad del owner) a los prefabs
/// de gameplay de cada personaje y lo enlaza al Animator del modelo (el mismo que ya usa
/// PlayerAnimatorView). Así las animaciones que dispara el jugador local se replican a los
/// demás clientes.
///
/// Menú: Tools > Characters > Add NetworkAnimator to Gameplay Prefabs
///
/// Es re-ejecutable (idempotente): no duplica el componente si ya existe.
/// </summary>
public static class AddNetworkAnimatorTool
{
    private static readonly string[] PrefabPaths =
    {
        "Assets/Prefabs/Characters/Berry_Gameplay.prefab",
        "Assets/Prefabs/Characters/Ixia_Gameplay.prefab",
        "Assets/Prefabs/Characters/Klug_Gameplay.prefab",
        "Assets/Prefabs/Characters/Wander_Gameplay.prefab",
    };

    [MenuItem("Tools/Characters/Add NetworkAnimator to Gameplay Prefabs")]
    public static void Run()
    {
        int ok = 0, skipped = 0, failed = 0;

        foreach (var path in PrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                Debug.LogError($"[AddNetworkAnimator] No se pudo cargar el prefab: {path}");
                failed++;
                continue;
            }

            try
            {
                var view = root.GetComponentInChildren<PlayerAnimatorView>(true);
                if (view == null)
                {
                    Debug.LogError($"[AddNetworkAnimator] {path}: no se encontró PlayerAnimatorView.");
                    failed++;
                    continue;
                }

                Animator modelAnimator = view.playerAnimator != null
                    ? view.playerAnimator
                    : root.GetComponentInChildren<Animator>(true);

                if (modelAnimator == null)
                {
                    Debug.LogError($"[AddNetworkAnimator] {path}: no se encontró un Animator de modelo.");
                    failed++;
                    continue;
                }

                // El NetworkAnimator debe vivir en el GameObject que tiene el NetworkObject (la raíz).
                GameObject host = root;

                var netAnim = host.GetComponent<NetworkAnimator>();
                if (netAnim == null)
                {
                    netAnim = host.AddComponent<OwnerNetworkAnimator>();
                    Debug.Log($"[AddNetworkAnimator] {path}: OwnerNetworkAnimator agregado.");
                }
                else
                {
                    Debug.Log($"[AddNetworkAnimator] {path}: ya tenía un NetworkAnimator ({netAnim.GetType().Name}).");
                    skipped++;
                }

                // Enlazar el Animator del modelo (campo serializado privado m_Animator).
                var so = new SerializedObject(netAnim);
                var prop = so.FindProperty("m_Animator");
                if (prop != null)
                {
                    prop.objectReferenceValue = modelAnimator;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning($"[AddNetworkAnimator] {path}: no se encontró el campo m_Animator; asigna el Animator a mano.");
                }

                // Que PlayerAnimatorView conozca el NetworkAnimator para enrutar los triggers.
                view.networkAnimator = netAnim;
                EditorUtility.SetDirty(view);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                ok++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AddNetworkAnimator] Listo. Guardados={ok}, ya existentes={skipped}, fallidos={failed}.");
    }
}
