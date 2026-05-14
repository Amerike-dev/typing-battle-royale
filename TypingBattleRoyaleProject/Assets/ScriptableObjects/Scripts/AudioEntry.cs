using UnityEngine;

[CreateAssetMenu(fileName = "AudioEntry", menuName = "Scriptable Objects/AudioEntry")]
public class AudioEntry : ScriptableObject
{
    public string id;

    public AudioClip clip;
<<<<<<< HEAD
    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0.1f, 3f)] public float pitch = 1f;
=======

    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    public float pitch = 1f;

>>>>>>> main
    public bool loop;
    public bool randomizePitch;
}
