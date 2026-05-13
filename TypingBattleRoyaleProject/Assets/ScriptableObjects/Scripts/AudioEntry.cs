using UnityEngine;

[CreateAssetMenu(fileName = "AudioEntry", menuName = "Scriptable Objects/AudioEntry")]
public class AudioEntry : ScriptableObject
{
    public string id;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    public float pitch = 1f;

    public bool loop;
}
