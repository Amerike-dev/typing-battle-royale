using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinInfo", menuName = "Scriptable Objects/SkinInfo")]
public class SkinInfo : ScriptableObject
{
    public string skinName;
    public GameObject[] models;
}
