using UnityEngine;

[CreateAssetMenu(fileName = "AudioEntry", menuName = "Scriptable Objects/AudioEntry")]
public class AudioEntry : ScriptableObject
{
    public string name;
    public AudioClip clip;
    public float defaultVolume;
    public float pitch;
    public bool loop;
}
