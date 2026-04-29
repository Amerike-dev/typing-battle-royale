using UnityEngine;
using Unity.Netcode;

public class IDController : NetworkBehaviour
{
    [Header("3D Models configuration")]
    [SerializeField] private SkinInfo[] arraySkins;
    
    public NetworkVariable<int> skinIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0);
    public NetworkVariable<bool> already = new NetworkVariable<bool>(false);

    private GameObject visualModel;
    
    public override void OnNetworkSpawn()
    {
        if (SelectController.Instance != null)
        {
            SelectController.Instance.SyncPlayer(OwnerClientId);
            
            if (IsOwner)
            {
                SelectController.Instance.RegisterLocalPlayer(this);
            }
        }
        else
        {
            Invoke(nameof(RetrySync), 0.1f);
        }

        if (IsOwner)
        {
            Debug.Log($"<color=cyan>Player {OwnerClientId} spawnado. IsOwner: {IsOwner}</color>");
            IPHolder.Instance?.SetPlayerId(OwnerClientId);
        }
        
        skinIndex.OnValueChanged += (oldV, newV) => {
            Debug.Log($"Skin cambió a: {newV}");
            Update3DModel();
        };
        colorIndex.OnValueChanged += (oldV, newV) => {
            Debug.Log($"Color cambió a: {newV}");
            Update3DModel();
        };
        
        Update3DModel();
    }
    
    public void ChangeSelection(int skinDir, int colorDir)
    {
        if (!IsOwner || already.Value) return;

        if (skinDir != 0)
            skinIndex.Value = (skinIndex.Value + skinDir + arraySkins.Length) % arraySkins.Length;

        if (colorDir != 0)
            colorIndex.Value = (colorIndex.Value + colorDir + 3) % 3;
    }

    public void Update3DModel()
    {
        if (SelectController.Instance == null) return;
        if (OwnerClientId >= (ulong)SelectController.Instance.wizardDisplayGO.Length) return;

        if (visualModel != null) Destroy(visualModel);
        
        Transform transformImage = SelectController.Instance.wizardDisplayGO[OwnerClientId].transform;
        Vector3 worldPosition = transformImage.position;

        GameObject prefab = arraySkins[skinIndex.Value].models[colorIndex.Value];
        visualModel = Instantiate(prefab, worldPosition, Quaternion.identity);
        visualModel.transform.position += Camera.main.transform.forward * -1f; 
        visualModel.transform.localScale = new Vector3(1, 1, 1); 
    }

    [ClientRpc]
    public void SetPlayerColorClientRpc(Color newColor)
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Debug.Log($"<color=yellow>Player {OwnerClientId} desconectado.</color>");
        }
    }

    private void OnGUI()
    {
        if (!IsOwner) return;

        GUI.Label(new Rect(10, 10, 300, 20), $"ID: {OwnerClientId}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Position: {transform.position}");
    }
    
    private void RetrySync()
    {
        if (SelectController.Instance != null)
        {
            SelectController.Instance.SyncPlayer(OwnerClientId);
        }
    }
}
