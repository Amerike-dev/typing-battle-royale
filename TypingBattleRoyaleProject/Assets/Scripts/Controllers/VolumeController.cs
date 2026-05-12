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
            AudioListener.volume = Mathf.Clamp01(volumeSlider.value);
            Debug.Log(AudioListener.volume);
        }
    }
    
}
