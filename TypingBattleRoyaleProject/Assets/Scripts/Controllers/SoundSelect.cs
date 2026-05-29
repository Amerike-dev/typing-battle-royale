using UnityEngine;

public class SoundSelect : MonoBehaviour
{
    public static SoundSelect Instance;
    public AudioSource audioSource;
    public AudioEntry selectSound;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ChangeSound(AudioEntry audio)
    {
        if (audio == null || audio.clip == null) return;
        audioSource.Stop();
        audioSource.clip = audio.clip;
        audioSource.volume = audio.volume;
        audioSource.pitch = audio.randomizePitch ? Random.Range(audio.pitch * 0.9f, audio.pitch * 1.1f) : audio.pitch;
        audioSource.loop = audio.loop;
        audioSource.Play();
    }
    public void PlaySelectSound()
    {
        ChangeSound(selectSound);
    }
}