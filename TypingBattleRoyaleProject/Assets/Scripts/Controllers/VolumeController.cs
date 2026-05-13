using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if(volumeSlider != null)
        {
            volumeSlider.value = 1;
            SetVolume();
        }
    }

    public void SetVolume()
    {
        if(volumeSlider != null)
        {
            AudioManager.instance.SetVolume(null, volumeSlider.value);
            PlayerPrefs.SetFloat("vol.master", AudioListener.volume);
            PlayerPrefs.Save();
        }
    }
    
}
