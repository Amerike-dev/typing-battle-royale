using UnityEngine;

public class DebbugController : MonoBehaviour
{

    public string DebugText;

    void Start()
    {
        CombatLogic.SetText(DebugText);
        MonolithData monolith = new MonolithData("01", 5, "Ola, soy Homero Chino");
    }

    private void Update()
    {
        CombatLogic.ValidateCharacter();
    }

}
