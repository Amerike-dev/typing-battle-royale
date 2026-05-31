using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomMusicPlayer : NetworkBehaviour
{
    public static RandomMusicPlayer Instance { get; private set; }

    [System.Serializable]
    public class ScenePlaylist
    {
        public string sceneName;
        public AudioEntry[] tracks;
    }

    [Header("Playlists por escena")]
    public ScenePlaylist[] playlists;

    [Header("Configuracion")]
    public float delayBetweenTracks = 0.5f;
    public bool avoidRepeat = true;
    public bool playOnStart = true;

    [Header("Fade")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.5f;

    [Header("Referencias")]
    public AudioSource audioSource;

    private Coroutine playRoutine;

    private NetworkVariable<int> currentTrackIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> trackStartTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private int lastPlayedIndex = -1;
    private List<int> shuffledIndices = new List<int>();
    private int shufflePosition = 0;
    private AudioEntry[] currentTracks;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshCurrentTracks();

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        lastPlayedIndex = -1;
        BuildShuffledList();

        if (!NetworkManager.Singleton.IsListening)
            playRoutine = StartCoroutine(FadeOutThenPlay(PlayNextRoutineLocal));
        else if (IsServer)
            StartCoroutine(FadeOutThenPlayNext());
    }

    private void Start()
    {
        RefreshCurrentTracks();

        if (playOnStart && !NetworkManager.Singleton.IsListening)
        {
            BuildShuffledList();
            playRoutine = StartCoroutine(PlayNextRoutineLocal());
        }
    }

    private void RefreshCurrentTracks()
    {
        string scene = SceneManager.GetActiveScene().name;

        foreach (ScenePlaylist playlist in playlists)
        {
            if (playlist.sceneName == scene)
            {
                currentTracks = playlist.tracks;
                return;
            }
        }

        Debug.Log("[MusicPlayer] No hay playlist para la escena: " + scene);
        currentTracks = null;
    }

    public override void OnNetworkSpawn()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        currentTrackIndex.OnValueChanged += OnTrackChanged;

        if (IsServer)
        {
            RefreshCurrentTracks();
            BuildShuffledList();
            StartCoroutine(DelayedStart());
        }
        else
        {
            if (currentTrackIndex.Value >= 0)
                SyncAndPlayTrack(currentTrackIndex.Value, trackStartTime.Value);
        }
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        PlayNext();
    }

    public override void OnNetworkDespawn()
    {
        currentTrackIndex.OnValueChanged -= OnTrackChanged;

        if (gameObject.activeInHierarchy)
        {
            RefreshCurrentTracks();
            BuildShuffledList();
            playRoutine = StartCoroutine(PlayNextRoutineLocal());
        }
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private IEnumerator FadeIn(float targetVolume)
    {
        audioSource.volume = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private IEnumerator FadeOutThenPlay(System.Func<IEnumerator> nextRoutine)
    {
        yield return StartCoroutine(FadeOut());
        playRoutine = StartCoroutine(nextRoutine());
    }

    private IEnumerator FadeOutThenPlayNext()
    {
        yield return StartCoroutine(FadeOut());
        PlayNext();
    }

    private IEnumerator PlayNextRoutineLocal()
    {
        if (currentTracks == null || currentTracks.Length == 0) yield break;

        if (delayBetweenTracks > 0f)
            yield return new WaitForSeconds(delayBetweenTracks);

        int index = GetNextIndex();
        if (index < 0) yield break;

        lastPlayedIndex = index;

        AudioEntry entry = currentTracks[index];
        ApplyEntryToSource(entry);
        audioSource.Play();

        yield return StartCoroutine(FadeIn(entry.volume));

        Debug.Log("[Local] Reproduciendo: " + entry.id);

        if (!entry.loop)
        {
            float clipLength = entry.clip.length / audioSource.pitch;
            float waitTime = clipLength - fadeOutDuration;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(FadeOut());
            playRoutine = StartCoroutine(PlayNextRoutineLocal());
        }
    }

    public void PlayNext()
    {
        if (!IsServer) return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayNextRoutine());
    }

    public void Stop()
    {
        if (!IsServer) return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        StopAllClientsRpc();
    }

    private IEnumerator PlayNextRoutine()
    {
        if (currentTracks == null || currentTracks.Length == 0) yield break;

        if (delayBetweenTracks > 0f)
            yield return new WaitForSeconds(delayBetweenTracks);

        int index = GetNextIndex();
        if (index < 0) yield break;

        lastPlayedIndex = index;

        trackStartTime.Value = (float)NetworkManager.Singleton.ServerTime.Time;
        currentTrackIndex.Value = index;
        ChangeScenePlaylistRpc(index, trackStartTime.Value);

        AudioEntry entry = currentTracks[index];
        ApplyEntryToSource(entry);
        audioSource.Play();

        yield return StartCoroutine(FadeIn(entry.volume));

        Debug.Log("[Server] Reproduciendo: " + entry.id);

        if (!entry.loop)
        {
            float clipLength = entry.clip.length / audioSource.pitch;
            float waitTime = clipLength - fadeOutDuration;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(FadeOut());
            PlayNext();
        }
    }

    private void OnTrackChanged(int oldIndex, int newIndex)
    {
        if (IsServer) return;
        SyncAndPlayTrack(newIndex, trackStartTime.Value);
    }

    [Rpc(SendTo.NotServer)]
    private void ChangeScenePlaylistRpc(int trackIndex, float startTime)
    {
        RefreshCurrentTracks();
        lastPlayedIndex = -1;
        BuildShuffledList();
        SyncAndPlayTrack(trackIndex, startTime);
    }

    private void SyncAndPlayTrack(int index, float serverStartTime)
    {
        if (currentTracks == null || index < 0 || index >= currentTracks.Length) return;

        AudioEntry entry = currentTracks[index];
        ApplyEntryToSource(entry);

        float elapsed = (float)NetworkManager.Singleton.ServerTime.Time - serverStartTime;
        float startOffset = Mathf.Clamp(elapsed, 0f, entry.clip.length);

        audioSource.time = startOffset;
        audioSource.volume = 0f;
        audioSource.Play();

        StartCoroutine(FadeIn(entry.volume));

        Debug.Log("[Client] Sincronizado: " + entry.id + " | offset: " + startOffset + "s");
    }

    [Rpc(SendTo.Everyone)]
    private void StopAllClientsRpc()
    {
        audioSource.Stop();
    }

    private void ApplyEntryToSource(AudioEntry entry)
    {
        audioSource.clip = entry.clip;
        audioSource.volume = entry.volume;
        audioSource.loop = entry.loop;

        audioSource.pitch = entry.randomizePitch
            ? Random.Range(entry.pitch * 0.9f, entry.pitch * 1.1f)
            : entry.pitch;
    }

    private int GetNextIndex()
    {
        if (currentTracks == null || currentTracks.Length == 0) return -1;
        if (currentTracks.Length == 1) return 0;

        if (shufflePosition >= shuffledIndices.Count)
        {
            BuildShuffledList();

            if (avoidRepeat && shuffledIndices.Count > 1 && shuffledIndices[0] == lastPlayedIndex)
            {
                int swap = Random.Range(1, shuffledIndices.Count);
                (shuffledIndices[0], shuffledIndices[swap]) = (shuffledIndices[swap], shuffledIndices[0]);
            }
        }

        return shuffledIndices[shufflePosition++];
    }

    private void BuildShuffledList()
    {
        shuffledIndices.Clear();
        shufflePosition = 0;

        if (currentTracks == null) return;

        for (int i = 0; i < currentTracks.Length; i++)
            shuffledIndices.Add(i);

        for (int i = shuffledIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledIndices[i], shuffledIndices[j]) = (shuffledIndices[j], shuffledIndices[i]);
        }
    }
}