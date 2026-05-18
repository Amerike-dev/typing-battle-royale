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
    public float pulseSpeed = 5f;
    public float minScale = 0.95f;
    public float maxScale = 1.05f;
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
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2f);
            spinner.localScale = new Vector3(scale, scale, scale);
        }
    }

    IEnumerator LoadAsync(string sceneName)
    {
        float duration = 3f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timeElapsed / duration);
            
            if (progressBar != null)
                progressBar.value = progress;
                
            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
