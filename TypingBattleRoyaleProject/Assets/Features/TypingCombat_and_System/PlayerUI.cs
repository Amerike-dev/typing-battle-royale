using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public CastInputController CIController;
    public GameObject InG_UI;
    public GameObject InS_UI;
    public bool G_UI = true;

    //Texto que se va a mostrar en el SpellUI.
    public TextMeshProUGUI SpellText;
    public TMP_InputField InputSpellText;

    void Start()
    {
        InG_UI.SetActive(true);
        InS_UI.SetActive(false);
    }

    void Update()
    {
        CurrentText();
        SpellCompleted();
    }
    public void CurrentText()
    {
        SpellText.text = CIController.spellText;
    }
    
    public void SpellTypingON()
    {
        if (G_UI == true)
        {
            InG_UI.SetActive(false);
            InS_UI.SetActive(true);
            CIController.incorrectInput = 0;
            CombatLogic._stringIndex = 0;
            InputSpellText.Select();
            InputSpellText.ActivateInputField();
            G_UI = false;
        }
    }
    public void SpellTypingOFF()
    {
        if (G_UI == false)
        {
            CIController.lastInInput = CIController.incorrectInput;
            CIController.incorrectInput = 0;
            InputSpellText.text=string.Empty;
            InputSpellText.DeactivateInputField();
            InS_UI.SetActive(false);
            InG_UI.SetActive(true);
            G_UI=true;
        }
    }
    public void SpellCompleted()
    {
        if (CombatLogic.spellComplete)
        {
            SpellTypingOFF();
        }
    }
}
