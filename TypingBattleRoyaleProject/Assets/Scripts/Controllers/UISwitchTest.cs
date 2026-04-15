using UnityEngine;

public class UISwitchTest : MonoBehaviour
{
    public CameraController cameraController;
    public GameObject Spell;
    public GameObject Explor;
    public void SpellOn() 
    {
        if (cameraController.OnCamaraMove) 
        {
            Explor.SetActive(false);
            cameraController.OnCamaraMove = false;
            Spell.SetActive(true);
        }
    }
    public void ExplorOn()
    {
        if(!cameraController.OnCamaraMove) 
        {
            Spell.SetActive(false);
            cameraController.OnCamaraMove = true;
            Explor.SetActive(true);
        }
    }
}
