using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
[CreateAssetMenu(fileName = "Character", menuName = "Characters/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public GameObject characterPrefab;
    public List<Color> skins;
}
