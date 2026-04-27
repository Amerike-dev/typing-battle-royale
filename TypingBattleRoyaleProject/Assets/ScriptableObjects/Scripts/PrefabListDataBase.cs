using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefab List", menuName = "CharacterList")]
public class PrefabListDatabase : ScriptableObject
{
    public List<CharacterData> characters;
}