using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectController : MonoBehaviour
{
    public static SelectController Instance;
    public Image[] wizardDisplayGO;
    public GameObject startButton;
    private IDController localPlayerScript;

    [Header("UI Unica")]
    [SerializeField] private GameObject arrowsPanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            startButton.SetActive(NetworkManager.Singleton.IsServer);

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SyncPlayer(client.ClientId);
            }

            IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);
            foreach (IDController jugador in allPlayers)
            {
                if (jugador.IsOwner)
                {
                    RegisterLocalPlayer(jugador);;
                }
                jugador.Update3DModel();
            }
        }
    }

    public void RegisterLocalPlayer(IDController script)
    {
        localPlayerScript = script;

        if (arrowsPanel != null && wizardDisplayGO[script.OwnerClientId] != null)
        {
            arrowsPanel.transform.SetParent(wizardDisplayGO[script.OwnerClientId].transform, false);
            RectTransform rt = arrowsPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            arrowsPanel.SetActive(true);
        }
    }
    public void SaveAllSelections()
    {
        IDController.savedSelections.Clear();

        IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);

        foreach (IDController player in allPlayers)
        {
            IDController.savedSelections[player.OwnerClientId] =
                new IDController.PlayerSelection(player.skinIndex.Value, player.colorIndex.Value);

            Debug.Log($"Guardado Player {player.OwnerClientId} | Skin {player.skinIndex.Value} | Color {player.colorIndex.Value}");
        }
    }
    
    public void UpArrow() => localPlayerScript?.ChangeSelection(1, 0);
    public void DownArrow() => localPlayerScript?.ChangeSelection(-1, 0);
    public void RightArrow() => localPlayerScript?.ChangeSelection(0, 1);
    public void LeftArrow() => localPlayerScript?.ChangeSelection(0, -1);

    public void OKClick()
    {
        if (localPlayerScript != null) localPlayerScript.already.Value = true;
    }

    public void SyncPlayer(ulong ID)
    {
        if (ID > (ulong)wizardDisplayGO.Length || wizardDisplayGO == null) return;

        if (wizardDisplayGO[ID] == null)
        {
            Debug.LogError("No image set");
            return;
        }

        switch (ID)
        {
            case 0:
                wizardDisplayGO[ID].color = Color.red;
                break;

            case 1:
                wizardDisplayGO[ID].color = Color.green;
                break;

            case 2:
                wizardDisplayGO[ID].color = Color.yellow;
                break;

            case 3:
                wizardDisplayGO[ID].color = Color.blue;
                break;

            default:
                wizardDisplayGO[ID].color = Color.white;
                break;
        }
    }
}
