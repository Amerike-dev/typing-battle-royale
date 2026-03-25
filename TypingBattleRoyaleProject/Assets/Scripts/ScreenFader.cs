using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public static IEnumerator FadeOut(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            Instance.canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        Instance.canvasGroup.alpha = 1f;
    }

    public static IEnumerator FadeIn(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            Instance.canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            yield return null;
        }

        Instance.canvasGroup.alpha = 0f;
    }
}
