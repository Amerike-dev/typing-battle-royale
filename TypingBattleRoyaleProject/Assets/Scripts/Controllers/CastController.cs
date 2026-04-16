using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class CastController : MonoBehaviour
{
    public string palabraObjetivo = "FIRE";
    public string CastText = "";
    public int indiceCorrecto = 0;
    public bool spellComplete=false;
    public int _stringIndex;


    public event Action OnSpellCast;

    public PlayerUI playerUI;
    public int incorrectInput = 0;
    public int lastInInput = 0;
    public string spellText;

    [Header("Typing Stats")]
    [SerializeField] private InputActionReference _cast;
    private bool _casting;
    private TypingStats _typingStats;
    private float _timeElapsed;
    private int _totalKeysPressed;
    public float wordsPerMinute;
    public float accuracy;

    [Header("ViewFeedback")]
    public SpellUIController uiController;

    //Revision de Texto
    void CalcularIndiceCorrecto(string cast, string spell, ref int correctas, ref int incorrectas)
    {
        for (int i = 0; i < cast.Length; i++)
        {
            // 1 Verificar si aún hay letras en la palabra objetivo para comparar
            if (i < spell.Length)
            {
                if (cast[i] == spell[i])
                {

                    if (i == spell.Length)
                    {
                        spellComplete = true;
                    }
                }
                else
                {
                    incorrectInput++;
                    incorrectas++;
                }
                    
            }
            else
            {
                // 2 Si el usuario escribió letras de más, cuentan como errores
                incorrectas++;
            }
        }
    }
    void Escribir(char c)
    {
        //Proceso
        CastText += c;
        //indiceCorrecto = CalcularIndiceCorrecto(CastText, palabraObjetivo);
        //Variables
        _totalKeysPressed++;
    }
    void Borrar()
    {
        if (CastText.Length == 0)
            return;

        CastText = CastText.Substring(0, CastText.Length - 1);

        //indiceCorrecto = CalcularIndiceCorrecto(CastText, palabraObjetivo);
    }

    //Evaluacion del Cast
    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        _typingStats.timeElapsed = _timeElapsed;
        _typingStats.hits = spellText.Length - incorrectInput;
        _typingStats.totalKeystrokes = _totalKeysPressed;
        wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();
        _casting = false;

        OnSpellCast?.Invoke();
        //Se reinicia el script en el onDisable, aqui antes de eso iria la funcion que castea el hechizo y ensear el wpm y el accuracy en un display

        gameObject.SetActive(false);
    }
    public IEnumerator CountTimeElapsed()
    {
        while (_casting)
        {
            _timeElapsed++;
            yield return new WaitForSeconds(1f);
        }
    }

}
