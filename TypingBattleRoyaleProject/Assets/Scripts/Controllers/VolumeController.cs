using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
<<<<<<< HEAD
    public Slider volumeSlider;

    void Start()
=======
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private string volumeType;

    private void Start()
>>>>>>> main
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
<<<<<<< HEAD
        if(volumeSlider != null)
        {
            AudioManager.instance.SetVolume(null, volumeSlider.value);
            PlayerPrefs.SetFloat("vol.master", AudioListener.volume);
            PlayerPrefs.Save();
        }
=======
        AudioManager.Instance.SetVolume(volumeType, value);
>>>>>>> main
    }
}