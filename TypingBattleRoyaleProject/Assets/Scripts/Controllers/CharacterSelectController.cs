using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void ConfirmSelection()
    {
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

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
