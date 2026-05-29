using UnityEngine;

/// <summary>Tipo de animación con la que aparece el emote sobre el jugador.</summary>
public enum EmoteAnim
{
    PopBounce = 0, // aparece con rebote (escala con overshoot)
    FloatUp   = 1, // flota hacia arriba mientras se desvanece
    Shake     = 2, // vibra (enojo / roto)
    Spin      = 3, // aparece girando
    DropDown  = 4, // cae desde arriba
    Fade      = 5  // simple fade in/out
}

/// <summary>
/// Lista ordenada de emotes disponibles. El índice define el mapeo por red (debe ser idéntico
/// en todos los clientes), así que vive como asset único en Resources y lo cargan todos.
/// </summary>
[CreateAssetMenu(fileName = "EmoteSet", menuName = "Scriptable Objects/EmoteSet")]
public class EmoteSet : ScriptableObject
{
    [System.Serializable]
    public class Emote
    {
        public string name;
        public Sprite sprite;
        public EmoteAnim anim;
    }

    public Emote[] emotes;
}
