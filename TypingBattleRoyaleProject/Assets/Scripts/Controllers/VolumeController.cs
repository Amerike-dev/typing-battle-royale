using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private string volumeType;

    private void Start()

    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        if(volumeSlider != null)
        {
            AudioManager.Instance.SetVolume(volumeType, volumeSlider.value);
            PlayerPrefs.SetFloat($"vol.{volumeType}", AudioListener.volume);
            PlayerPrefs.Save();
        }
    }
}