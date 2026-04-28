using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinInfo", menuName = "Scriptable Objects/SkinInfo")]
public class SkinInfo : ScriptableObject
{
    public string skinName;
    public List<GameObject> skinsPrefab = new List<GameObject>(3);
    [Range(0, 2)] public int variationColor = 0;

    public void SetSkin()
    {

    }
}
