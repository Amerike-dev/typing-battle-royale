using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataBase", menuName = "Scriptable Objects/AudioDataBase")]
public class AudioDataBase : ScriptableObject
{
    [SerializeField] private List<AudioEntry> list = new List<AudioEntry>();
    

    public void GetEntry(string name)
    {

    }
}
