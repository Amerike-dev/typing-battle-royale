using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [Tooltip("Segundos que se muestran las instrucciones antes de cargar la GameplayScene.")]
    [SerializeField] private float instructionsDuration = 20f;

    private bool _starting;

    public void ConfirmSelection()
    {
        if (_starting) return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No existe NetworkManager.");
            return;
        }

        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Solo el host puede iniciar la escena de gameplay.");
            return;
        }

        if (SelectController.Instance != null)
        {
            SelectController.Instance.SaveAllSelections();
        }
        else
        {
            Debug.LogError("No existe SelectController.Instance.");
            return;
        }

        _starting = true;

        float duration = InstructionsPanelController.Instance != null
            ? InstructionsPanelController.Instance.Duration
            : instructionsDuration;

        // Mostramos las instrucciones en TODOS los clientes (incluido el host) vía RPC del
        // objeto de jugador del host.
        var hostPlayer = NetworkManager.Singleton.LocalClient != null
            ? NetworkManager.Singleton.LocalClient.PlayerObject
            : null;
        var idc = hostPlayer != null ? hostPlayer.GetComponent<IDController>() : null;

        if (idc != null)
        {
            idc.ShowInstructionsClientRpc(duration);
        }
        else if (InstructionsPanelController.Instance != null)
        {
            // Fallback local por si no encontramos el objeto de jugador del host.
            InstructionsPanelController.Instance.Begin(duration);
        }

        StartCoroutine(StartAfterInstructions(duration));
    }

    private IEnumerator StartAfterInstructions(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
