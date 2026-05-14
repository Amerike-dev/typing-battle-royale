using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public void TestSFX()
    {
        AudioManager.Instance.PlaySFX("ui_click");
    }

    public void TestMusic()
    {
        AudioManager.Instance.ChangeMusic("battle_theme");
    }
}