using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;

public class IDController : NetworkBehaviour
{
    [Header("3D Models configuration")]
    [SerializeField] private SkinInfo[] arraySkins;
    
    public NetworkVariable<int> skinIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> already = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        "", 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    private GameObject visualModel;

    [SerializeField] private GameObject canvasLabelGO;
    [SerializeField] private EnemyLabel myEnemyLabel;

    //Mis cambios para crear una memoria que guarde la seleccion
    public struct PlayerSelection
    {
        public int skinIndex;
        public int colorIndex;

        public PlayerSelection(int skinIndex, int colorIndex)
        {
            this.skinIndex = skinIndex;
            this.colorIndex = colorIndex;
        }
    }

    public static Dictionary<ulong, PlayerSelection> savedSelections = new Dictionary<ulong, PlayerSelection>();
    //aqui terminamos
    
    public override void OnNetworkSpawn()
    {
        if (SelectController.Instance != null)
        {
            SelectController.Instance.SyncPlayer(OwnerClientId);
            
            if (IsOwner) SelectController.Instance.RegisterLocalPlayer(this);
        }
        else
        {
            Invoke(nameof(RetrySync), 0.1f);
        }

        if (IsOwner)
        {
            Debug.Log($"<color=cyan>Player {OwnerClientId} spawnado. IsOwner: {IsOwner}</color>");
            IPHolder.Instance?.SetPlayerId(OwnerClientId);
            string savedName = PlayerPrefs.GetString("playerName", "");
            
            if (string.IsNullOrWhiteSpace(savedName) || savedName.Length < 3) savedName = PlayerIDGenerator.GenerateID();
            
            playerName.Value = new FixedString64Bytes(savedName);
        }
        
        skinIndex.OnValueChanged += (oldV, newV) => {
            Debug.Log($"Skin cambió a: {newV}");
            Update3DModel();
        };
        colorIndex.OnValueChanged += (oldV, newV) => {
            Debug.Log($"Color cambió a: {newV}");
            Update3DModel();
        };

        if (canvasLabelGO != null)
        {
            if (!IsOwner)
                canvasLabelGO.SetActive(true);
            else
                canvasLabelGO.SetActive(false);
        }
        
        playerName.OnValueChanged += (oldV, newV) => { 
            Debug.Log($"<color=orange>[JorSalasDEV - Sync]</color> El jugador {OwnerClientId} actualizó su nombre de {oldV} a: {newV}");
            UpdateLabel(); 
        };
        
        Update3DModel();
        Invoke(nameof(UpdateLabel), 0.2f);
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
        UpdateLabel();
    }
    
    private void UpdateLabel()
    {
        string currentName = playerName.Value.ToString();
        
        Debug.Log($"<color=cyan>[JorSalasDEV - SYNC]</color> El jugador {OwnerClientId} intentará ponerse el nombre: '{currentName}'");

        if (myEnemyLabel != null)
        {
            myEnemyLabel.SetLabel(currentName);
            
            if (!IsOwner)
                myEnemyLabel.gameObject.SetActive(true);
            else 
                myEnemyLabel.gameObject.SetActive(false); 
        }
        else
        {
            Debug.LogError($"<color=red>[JorSalasDEV - ERROR]</color> ¡La variable myEnemyLabel no está asignada en el Inspector del jugador {OwnerClientId}!");
        }
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
