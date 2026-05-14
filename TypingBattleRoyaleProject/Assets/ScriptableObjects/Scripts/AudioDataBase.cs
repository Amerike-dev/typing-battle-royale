using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataBase", menuName = "Scriptable Objects/AudioDataBase")]
public class AudioDataBase : ScriptableObject
{
<<<<<<< HEAD
    [SerializeField] private List<AudioEntry> list = new List<AudioEntry>();

    public AudioEntry GetEntry(string name)
    {
        return list.FirstOrDefault(entry => entry != null && entry.name == name);
=======
    [SerializeField] private List<AudioEntry> list = new();

    public AudioEntry GetEntry(string id)
    {
        return list.Find(e => e.id == id);
>>>>>>> main
    }
}
