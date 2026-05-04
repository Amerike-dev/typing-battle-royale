using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    [SerializeField] private AudioDataBase _db;
    [SerializeField] private MusicController _music;
    [SerializeField] private AudioSettings _settings;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private List<AudioSource> sfxPool = new List<AudioSource>();


    public void PlaySFX(string name)
    {

    }

    public void ChangeMusic(string name)
    {

    }

    public void SetVolume(AudioClip type, float value)
    {

    }
}
