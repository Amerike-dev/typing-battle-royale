using UnityEngine;

/// <summary>
/// Mapea cada <see cref="Elements"/> con el sprite de runa (pentagrama) correspondiente.
/// Las runas viven en Assets/Artist/AssetsIcons/Runes y están nombradas por elemento.
/// El asset se autopuebla con la herramienta de Editor "Tools/TBR/Setup Rune Cast Displays".
/// </summary>
[CreateAssetMenu(fileName = "RuneLibrary", menuName = "Scriptable Objects/Rune Library")]
public class RuneLibrary : ScriptableObject
{
    [System.Serializable]
    public struct RuneEntry
    {
        public Elements element;
        public Sprite sprite;
    }

    public RuneEntry[] runes;

    public Sprite GetSprite(Elements element)
    {
        if (runes == null) return null;
        for (int i = 0; i < runes.Length; i++)
        {
            if (runes[i].element == element) return runes[i].sprite;
        }
        return null;
    }
}
