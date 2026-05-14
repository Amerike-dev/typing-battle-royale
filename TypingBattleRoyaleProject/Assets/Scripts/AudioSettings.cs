using UnityEngine;

[System.Serializable]
public class AudioSettings
{
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;

    public void Save()
    {
        PlayerPrefs.SetFloat("vol.master", masterVolume);
        PlayerPrefs.SetFloat("vol.music", musicVolume);
        PlayerPrefs.SetFloat("vol.sfx", sfxVolume);

        PlayerPrefs.Save();
    }

    public void Load()
    {
        masterVolume = PlayerPrefs.GetFloat("vol.master", 1f);
        musicVolume = PlayerPrefs.GetFloat("vol.music", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("vol.sfx", 1f);
    }
}
