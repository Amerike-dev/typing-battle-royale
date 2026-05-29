using UnityEngine;
using UnityEngine.UI;

public class OptionsAudioPanel : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("vol.master", 1f));
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("vol.music", 0.7f));
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("vol.sfx", 1f));
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
    }

    private void OnDisable()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    private void OnMasterChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetVolume("master", v);
    }

    private void OnMusicChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetVolume("music", v);
    }

    private void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetVolume("sfx", v);
    }
}
