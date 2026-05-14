using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    [FormerlySerializedAs("sliderLoading")]
    public Slider progressBar;

    [FormerlySerializedAs("estadoTxt")]
    public TextMeshProUGUI tipsText;
    
    public RectTransform spinner;

    [Header("Settings")]
    public float spinnerSpeed = -200f;
    
    public static string SceneToLoad = "CharacterSelect";

    private string[] tips = {
        "Mientras más rápido tipees, haces más daño.",
        "Acc < 30% no daña — pero el cooldown corre igual.",
        "Asegúrate de escribir correctamente, los errores cuestan tiempo.",
        "Las palabras largas infligen más daño a tus oponentes.",
        "Mantén un ritmo constante para evitar penalizaciones."
    };

    void Start()
    {
        if (progressBar != null) 
            progressBar.value = 0;
            
        if (tipsText != null)
        {
            tipsText.text = tips[Random.Range(0, tips.Length)];
        }

        StartCoroutine(LoadAsync(SceneToLoad));
    }

    void Update()
    {
        if (spinner != null)
        {
            spinner.Rotate(0, 0, spinnerSpeed * Time.deltaTime);
        }
    }

    IEnumerator LoadAsync(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) yield break;

        op.allowSceneActivation = false;
        
        while (op.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.value = op.progress;
            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;
            
        yield return new WaitForSeconds(1f);
        op.allowSceneActivation = true;
    }
}
