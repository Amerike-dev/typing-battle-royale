using UnityEngine;

public class UISwitchTest : MonoBehaviour
{
    public CamaraController CamaraController;
    public GameObject Spell;
    public GameObject Explor;
    public void SpellOn() 
    {
        if (CamaraController.OnCamaraMove) 
        {
            Explor.SetActive(false);
            CamaraController.OnCamaraMove = false;
            Spell.SetActive(true);
        }
    }
    public void ExplorOn()
    {
        if(!CamaraController.OnCamaraMove) 
        {
            Spell.SetActive(false);
            CamaraController.OnCamaraMove = true;
            Explor.SetActive(true);
        }
    }
}
