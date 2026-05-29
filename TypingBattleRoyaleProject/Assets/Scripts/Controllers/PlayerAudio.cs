using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance;
    public AudioSource audioSource;
    public AudioEntry[] audioEntries;

    public void Awake()
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
        audioSource.clip   = audio.clip;
        audioSource.volume = audio.volume;
        audioSource.pitch  = audio.randomizePitch ? Random.Range(audio.pitch * 0.9f, audio.pitch * 1.1f) : audio.pitch;
        audioSource.loop   = audio.loop;
        audioSource.Play();
    }

    public void ChangeSoundById(string id)
    {
        foreach (AudioEntry entry in audioEntries)
        {
            if (entry.id == id)
            {
                ChangeSound(entry);
                return;
            }
        }
        Debug.LogWarning($"AudioEntry con id '{id}' no encontrado.");
    }
}