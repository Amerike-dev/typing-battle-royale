using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour

{
    [SerializeField] private AudioClip currentClip;

    public IEnumerator CrossfadeTo(AudioSource source, AudioClip newClip, float duration, float targetVolume)
    {
        if (source == null)
        {
            Debug.LogError("AudioSource is null. Cannot perform crossfade.");
            yield break;
        }

        float startVolume = source.volume;

        // Fade out
        if (source.isPlaying)
        {
            float fadeOutDuration = duration / 2f;
            float timer = 0f;

            while (timer < fadeOutDuration)
            {
                source.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        source.Stop();
        source.clip = newClip;
        source.Play();
        currentClip = newClip;

        // Fade in
        float fadeInDuration = duration / 2f;
        float fadeInTimer = 0f;
        while (fadeInTimer < fadeInDuration)
        {
            source.volume = Mathf.Lerp(0f, targetVolume, fadeInTimer / fadeInDuration);
            fadeInTimer += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume;
    }
}