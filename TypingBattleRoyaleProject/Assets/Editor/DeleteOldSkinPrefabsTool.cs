using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Borra los prefabs de gameplay viejos basados en formas (Botas/Capsule/Cono/Cube/
/// Cylinder/Sphere) y los SkinInfo Character1-6 que ya no se usan (el juego ahora usa
/// Berry/Ixia/Klug/Wander). Después limpia las entradas colgantes de DefaultNetworkPrefabs.
///
/// Menú: Tools > Characters > Delete Old Skin Prefabs (cleanup)
///
/// NO toca Player.prefab, DemoPlayer.prefab ni los prefabs de Berry/Ixia/Klug/Wander.
/// Es re-ejecutable (idempotente): ignora lo que ya no existe.
/// </summary>
public static class DeleteOldSkinPrefabsTool
{
    private const string NETWORK_PREFABS = "Assets/DefaultNetworkPrefabs.asset";

    private static readonly string[] OldShapeFolders =
    {
        "Botas/Gameplay/BotasGameplay_01",
        "Capsule/Gameplay/CapsuleGameplay_01",
        "Cono/Gameplay/ConoGameplay_01",
        "Cube/Gameplay/CubeGameplay_01",
        "Cylinder/Gameplay/CylinderGameplay_01",
        "Sphere/Gameplay/CylinderGameplay_01",
    };

    private static readonly string[] OldSkinInfoAssets =
    {
        "Assets/Prefabs/PrefabList/SkinsInfo/Character1.asset",
        "Assets/Prefabs/PrefabList/SkinsInfo/Character2.asset",
        "Assets/Prefabs/PrefabList/SkinsInfo/Character3.asset",
        "Assets/Prefabs/PrefabList/SkinsInfo/Character4.asset",
        "Assets/Prefabs/PrefabList/SkinsInfo/Character5.asset",
        "Assets/Prefabs/PrefabList/SkinsInfo/Character6.asset",
    };

    [MenuItem("Tools/Characters/Delete Old Skin Prefabs (cleanup)")]
    public static void Run()
    {
        var toDelete = new List<string>();

        const string baseDir = "Assets/Prefabs/PrefabList/SkinsInfo/";
        foreach (var stem in OldShapeFolders)
        {
            // Cada forma tiene el prefab base y sus duplicados " 1" y " 2".
            toDelete.Add($"{baseDir}{stem}.prefab");
            toDelete.Add($"{baseDir}{stem} 1.prefab");
            toDelete.Add($"{baseDir}{stem} 2.prefab");
        }
        toDelete.AddRange(OldSkinInfoAssets);

        int deleted = 0, missing = 0, failed = 0;

        foreach (var path in toDelete)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) == null)
            {
                missing++;
                continue;
            }

            if (AssetDatabase.DeleteAsset(path))
            {
                Debug.Log($"[DeleteOldSkin] Borrado: {path}");
                deleted++;
            }
            else
            {
                Debug.LogError($"[DeleteOldSkin] No se pudo borrar: {path}");
                failed++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int cleanedRefs = CleanNetworkPrefabs();

        Debug.Log($"[DeleteOldSkin] Listo. Borrados={deleted}, ya inexistentes={missing}, fallidos={failed}, refs limpiadas en DefaultNetworkPrefabs={cleanedRefs}.");
    }

    private static int CleanNetworkPrefabs()
    {
        var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NETWORK_PREFABS);
        if (list == null)
        {
            Debug.LogWarning($"[DeleteOldSkin] No se encontró {NETWORK_PREFABS}.");
            return 0;
        }

        // Copiamos para poder remover mientras recorremos.
        var entries = new List<NetworkPrefab>(list.PrefabList);
        int removed = 0;

        foreach (var entry in entries)
        {
            if (entry == null || entry.Prefab == null)
            {
                list.Remove(entry);
                removed++;
            }
        }

        if (removed > 0)
        {
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
        }

        return removed;
    }
}
