using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip currentClip;

    public IEnumerator CrossfadeTo(AudioClip newClip, float duration = 0.5f)
    {
        if (newClip == null)
        {
            yield break;
        }

        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / duration;

            yield return null;
        }

        musicSource.Stop();

        musicSource.clip = newClip;
        currentClip = newClip;

        musicSource.Play();

        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / duration;

            yield return null;
        }

        musicSource.volume = startVolume;
    }
}