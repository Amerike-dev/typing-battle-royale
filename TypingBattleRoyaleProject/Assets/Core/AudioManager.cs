using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioDataBase _db;
    [SerializeField] private MusicController _music;
    [SerializeField] private AudioSettings _settings;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private List<AudioSource> sfxPool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        _settings.Load();
    }

    public void PlaySFX(string name)
    {
        AudioEntry entry = _db.GetEntry(name);

        if (entry == null)
        {
            Debug.LogWarning($"[AUDIO] SFX missing: {name}");
            return;
        }

          Debug.Log($"[AUDIO] Playing SFX: {name}");

        AudioSource source = GetFreeSource();

        source.clip = entry.clip;

        source.volume =
            entry.defaultVolume *
            _settings.sfxVolume *
            _settings.masterVolume;

        source.pitch = entry.pitch;

        source.loop = entry.loop;

        source.Play();
    }

    public void ChangeMusic(string name, float duration = 0.5f)
    {
        AudioEntry entry = _db.GetEntry(name);

        if (entry == null)
        {
            Debug.LogWarning($"[AUDIO] Music missing: {name}");
            return;
        }
        Debug.Log($"[AUDIO] Changing music to: {name}");

        StartCoroutine(_music.CrossfadeTo(entry.clip, duration));
    }

    public void SetVolume(string channel, float value)
    {
        value = Mathf.Clamp01(value);

        switch (channel)
        {
            case "master":
                _settings.masterVolume = value;
                AudioListener.volume = value;
                break;

            case "music":
                _settings.musicVolume = value;

                if (_musicSource != null)
                {
                    _musicSource.volume =
                        value * _settings.masterVolume;
                }

                break;

            case "sfx":
                _settings.sfxVolume = value;
                break;
        }

        _settings.Save();
    }

    private AudioSource GetFreeSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        return sfxPool[Time.frameCount % sfxPool.Count];
    }
}