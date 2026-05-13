using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataBase", menuName = "Scriptable Objects/AudioDataBase")]
public class AudioDataBase : ScriptableObject
{
    [SerializeField] private List<AudioEntry> list = new();

    public AudioEntry GetEntry(string id)
    {
        return list.Find(e => e.id == id);
    }
}
