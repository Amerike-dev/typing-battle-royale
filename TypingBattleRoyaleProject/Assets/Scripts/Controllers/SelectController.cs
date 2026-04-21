using UnityEngine;
using UnityEngine.UI;

public class SelectController : MonoBehaviour
{
    [SerializeField] private Image[] wizardDisplayGO = new Image[4];
    public GameObject container;

    void Start()
    {
        wizardDisplayGO = container.GetComponentsInChildren<Image>();

        SetColor(IPHolder.Instance.PlayerID);
    }

    void SetColor(ulong ID)
    {
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
