using UnityEngine;
using UnityEngine.UI;

public class SelectController : MonoBehaviour
{
    public static SelectController Instance;

    [SerializeField] private Image[] wizardDisplayGO;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
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
