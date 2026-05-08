using UnityEngine;
using System.Collections;

public class DebugPoppop : MonoBehaviour
{
    [Header("Proximidad del Jugador")]
    public CanvasGroup canvasGroup;
    public RectTransform imageUI;
    public GameObject player;
    public Vector2 upImagePos;
    public Vector2 downImagePos;
    public float detectionRadius = 20f;
    public float fadeTime = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Poppop()
    {
        float proximity = Vector3.Distance(player.transform.position, transform.position);
        if (proximity < detectionRadius)
        {
            Debug.Log("jahsgd");
            StartCoroutine(FadeOn());
        }
        if (proximity > detectionRadius)
        {
            StartCoroutine(FadeOFF());
        }
    }
    IEnumerator FadeOn()
    {
        float time = 0f;
        while (time < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    IEnumerator FadeOFF()
    {
        float time = 0f;
        while (time < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
