using System;
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

    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _musicVolume = 0.7f;
    [SerializeField] private float _sfxVolume = 1f;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _masterVolume = PlayerPrefs.GetFloat("vol.master", 1f); 
        _musicVolume = PlayerPrefs.GetFloat("vol.music", 0.7f); 
        _sfxVolume = PlayerPrefs.GetFloat("vol.sfx", 1f);
    }

    public void PlaySFX(string id)
    {
        var entry = _db.GetEntry(id);

        if(entry == null)
        {
            Debug.LogWarning($"[AUDIO] SFX missing: {id}");
            return;
        }

        var scr = GetFreeSource();
        scr.clip = entry.clip;
        scr.volume = entry.volume * _sfxVolume * _masterVolume;
        scr.pitch = entry.randomizePitch ? entry.pitch * UnityEngine.Random.Range(0.95f, 1.05f) : entry.pitch;
        scr.Play();
    }

    public AudioSource GetFreeSource()
    {
        foreach(var src in sfxPool)
        {
            if (!src.isPlaying)
                return src;
        }

        return sfxPool[Time.frameCount % sfxPool.Count];
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

    public void ChangeMusic(string id, float duration = 0.5f)
    {
        var entry = _db.GetEntry(id);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[AUDIO] Music entry '{id}' not found");
            return;
        }

        float targetVolume = entry.volume * _musicVolume * _masterVolume;
        StartCoroutine(_music.CrossfadeTo(_musicSource, entry.clip, duration, targetVolume));
    }
}

