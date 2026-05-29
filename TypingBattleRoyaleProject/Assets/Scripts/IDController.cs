using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;

public class IDController : NetworkBehaviour
{
    [Header("3D Models configuration")]
    [SerializeField] private SkinInfo[] arraySkins;

    [Header("3D Model spawn tuning")]
    [Tooltip("Escala aplicada al modelo 3D después de instanciarlo.")]
    [SerializeField] private Vector3 _modelScale = Vector3.one;
    [Tooltip("Offset adicional sumado a la posición del spawn point (en world space).")]
    [SerializeField] private Vector3 _modelPositionOffset = Vector3.zero;
    [Tooltip("Si está activo, usa el offset legacy: empuja el modelo 1 unidad hacia la cámara (solo aplica cuando no hay spawn point asignado en SelectController).")]
    [SerializeField] private bool _useLegacyCameraOffset = true;
    
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

        already.OnValueChanged += (oldV, newV) => {
            Debug.Log($"<color=green>[READY]</color> Player {OwnerClientId} listo: {newV}");

            // Al marcarse "Listo", el modelo de preview hace el salto una vez (feedback visual en todos los clientes).
            if (newV) PlayReadyJump();

            if (SelectController.Instance != null)
            {
                SelectController.Instance.ShowReadyUI(OwnerClientId, newV);

                if (IsServer) SelectController.Instance.CheckAllPlayersReady();
            }
        };

        if (already.Value && SelectController.Instance != null)
            SelectController.Instance.ShowReadyUI(OwnerClientId, true);

        if (canvasLabelGO != null)
        {
            if (!IsOwner)
                canvasLabelGO.SetActive(true);
            else
                canvasLabelGO.SetActive(false);
        }
        
        playerName.OnValueChanged += (oldV, newV) => {
            Debug.Log($"<color=orange>[Sync]</color> El jugador {OwnerClientId} actualizó su nombre de {oldV} a: {newV}");
            UpdateLabel();

            if (already.Value && SelectController.Instance != null)
                SelectController.Instance.ShowReadyUI(OwnerClientId, true);
        };
        
        Update3DModel();
        Invoke(nameof(UpdateLabel), 0.2f);
    }
    
    public void ChangeSelection(int skinDir, int colorDir)
    {
        if (!IsOwner || already.Value) return;

        if (skinDir != 0)
        {
            skinIndex.Value = (skinIndex.Value + skinDir + arraySkins.Length) % arraySkins.Length;
            // Al cambiar de personaje, reencuadramos el color al rango del nuevo (puede tener menos skins).
            colorIndex.Value = Mathf.Clamp(colorIndex.Value, 0, ColorCount() - 1);
        }

        if (colorDir != 0)
        {
            int count = ColorCount();
            colorIndex.Value = (colorIndex.Value + colorDir + count) % count;
        }
    }

    /// <summary>Cantidad de variantes de color (materiales) del personaje actualmente seleccionado.</summary>
    private int ColorCount()
    {
        if (arraySkins == null || arraySkins.Length == 0) return 1;
        SkinInfo skin = arraySkins[skinIndex.Value];
        if (skin == null || skin.skins == null || skin.skins.Length == 0) return 1;
        return skin.skins.Length;
    }

    public void Update3DModel()
    {
        if (SelectController.Instance == null) return;

        if (visualModel != null) Destroy(visualModel);

        Vector3 spawnPosition;
        Quaternion spawnRotation;
        Vector3 spawnScale;
        Transform spawnPoint = SelectController.Instance.GetModelSpawnPoint(OwnerClientId);

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position + _modelPositionOffset;
            spawnRotation = spawnPoint.rotation;
            spawnScale = Vector3.Scale(spawnPoint.lossyScale, _modelScale);
        }
        else
        {
            if (OwnerClientId >= (ulong)SelectController.Instance.wizardDisplayGO.Length) return;
            Transform transformImage = SelectController.Instance.wizardDisplayGO[OwnerClientId].transform;
            spawnPosition = transformImage.position + _modelPositionOffset;
            spawnRotation = Quaternion.identity;
            spawnScale = _modelScale;

            if (_useLegacyCameraOffset && Camera.main != null)
                spawnPosition += Camera.main.transform.forward * -1f;
        }

        SkinInfo skin = arraySkins[skinIndex.Value];

        // El jugador local actualiza el texto del nombre del personaje en la UI de selección.
        if (IsOwner && skin != null && SelectController.Instance != null)
            SelectController.Instance.UpdateCharacterName(skin.skinName);

        if (skin == null || skin.previewModel == null) return;

        visualModel = Instantiate(skin.previewModel, spawnPosition, spawnRotation);
        visualModel.transform.localScale = spawnScale;

        if (skin.skins != null && skin.skins.Length > 0)
        {
            int ci = Mathf.Clamp(colorIndex.Value, 0, skin.skins.Length - 1);
            PlayerSkin.ApplyTo(visualModel, skin.skins[ci]);
        }

        UpdateLabel();
    }
    
    /// <summary>
    /// Dispara una vez la animación de salto en el modelo de preview de Character Select.
    /// El preview tiene un Animator "crudo" (sin PlayerAnimatorView), así que seteamos el trigger directo.
    /// </summary>
    private void PlayReadyJump()
    {
        if (visualModel == null) return;

        Animator animator = visualModel.GetComponentInChildren<Animator>(true);
        if (animator == null) return;

        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == "Jump")
            {
                animator.SetTrigger("Jump");
                return;
            }
        }
    }

    private void UpdateLabel()
    {
        string currentName = playerName.Value.ToString();

        Debug.Log($"<color=cyan>[SYNC]</color> El jugador {OwnerClientId} intentará ponerse el nombre: '{currentName}'");

        if (myEnemyLabel == null) return;

        myEnemyLabel.SetLabel(currentName);
        myEnemyLabel.gameObject.SetActive(!IsOwner);
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

    /// <summary>
    /// El host lo dispara al pulsar "Empezar": muestra el panel de instrucciones y el contador
    /// en todos los clientes antes de transicionar a la GameplayScene.
    /// </summary>
    [ClientRpc]
    public void ShowInstructionsClientRpc(float seconds)
    {
        if (InstructionsPanelController.Instance != null)
            InstructionsPanelController.Instance.Begin(seconds);
        else
            Debug.LogWarning("[IDController] No se encontró InstructionsPanelController en la escena.");
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Debug.Log($"<color=yellow>Player {OwnerClientId} desconectado.</color>");
        }

        if (SelectController.Instance != null)
        {
            SelectController.Instance.RefreshSlotVisibility(OwnerClientId);
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
