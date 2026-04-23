using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.value = 1;
        SetVolume();

    }

    public void SetVolume()
    {
        AudioListener.volume = Mathf.Clamp01(volumeSlider.value);
        Debug.Log(AudioListener.volume);
    }
    
}
