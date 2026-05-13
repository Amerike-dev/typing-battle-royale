using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [Header("Fade Time")]
    [SerializeField] private float studioFade = 1.5f;
    [SerializeField] private float gameFade = 2f;

    [SerializeField] private CanvasGroup studioCanvas;
    [SerializeField] private CanvasGroup gameCanvas;

    void Start()
    {
        studioCanvas.alpha = 0f;
        gameCanvas.alpha = 0f;

        StartCoroutine(SplashCorutine());
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            StopAllCoroutines();
            SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }
    }

    private IEnumerator SplashCorutine()
    {
        yield return StartCoroutine(FadeCanvas(studioCanvas, 1f, studioFade));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeCanvas(studioCanvas, 0f, 0.5f));

        yield return StartCoroutine(FadeCanvas(gameCanvas, 1f, gameFade));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeCanvas(gameCanvas, 0f, 0.5f));

        SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float alphaTarget, float duration)
    {
        float startAlpha = canvas.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            canvas.alpha = Mathf.Lerp(startAlpha, alphaTarget, time / duration);
            yield return null;
        }

        canvas.alpha = alphaTarget;
    }

}
