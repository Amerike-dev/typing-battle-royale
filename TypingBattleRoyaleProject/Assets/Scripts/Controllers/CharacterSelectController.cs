using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void ConfirmSelection()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
