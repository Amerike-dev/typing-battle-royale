using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class AudioChango : NetworkBehaviour
{
    public static AudioChango Instance { get; private set; }

    [Header("Referencias")]
    public AudioSource audioSource;

    [Header("Audios")]
    public AudioEntry caracterSelect;
    public AudioEntry playerDeath;
    public AudioEntry winner;
    public AudioEntry monolithDisappear;
    public AudioEntry gameStart;

    private Queue<AudioEntry> audioQueue = new Queue<AudioEntry>();
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayCaracterSelect() => RequestPlayServerRpc(0);
    public void PlayPlayerDeath() => RequestPlayServerRpc(1);
    public void PlayWinner() => RequestPlayServerRpc(2);
    public void PlayMonolithDisappear() => RequestPlayServerRpc(3);
    public void PlayGameStart() => RequestPlayServerRpc(4);

    [Rpc(SendTo.Server)]
    private void RequestPlayServerRpc(int eventId)
    {
        PlayOnAllClientsRpc(eventId);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayOnAllClientsRpc(int eventId)
    {
        AudioEntry entry = GetEntryById(eventId);
        if (entry == null) return;

        audioQueue.Enqueue(entry);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (audioQueue.Count > 0)
        {
            AudioEntry entry = audioQueue.Dequeue();
            ApplyEntryToSource(entry);
            audioSource.Play();

            Debug.Log("[Presenter] Reproduciendo: " + entry.id);

            yield return new WaitForSeconds(entry.clip.length / audioSource.pitch);
        }

        isPlaying = false;
    }

    private AudioEntry GetEntryById(int eventId)
    {
        switch (eventId)
        {
            case 0: return caracterSelect;
            case 1: return playerDeath;
            case 2: return winner;
            case 3: return monolithDisappear;
            case 4: return gameStart;
            default: return null;
        }
    }

    private void ApplyEntryToSource(AudioEntry entry)
    {
        audioSource.clip = entry.clip;
        audioSource.volume = entry.volume;
        audioSource.loop = false;

        audioSource.pitch = entry.randomizePitch
            ? Random.Range(entry.pitch * 0.9f, entry.pitch * 1.1f)
            : entry.pitch;
    }
}