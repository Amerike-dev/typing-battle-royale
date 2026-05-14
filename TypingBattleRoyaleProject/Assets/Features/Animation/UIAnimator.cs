using UnityEngine;
using System.Collections;

public static class UIAnimator
{
    /*public void MoveSignal(bool signalMove,Vector2 target, float targetAlpha)
    {
        if (signalMove != null)
        {
            StopCoroutine(Fade);
        }
        signalMove = StartCoroutine(Fade(canvasGroup, targetAlpha));
    }
    IEnumerator Fade(CanvasGroup canvasGroup,float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;

        float time = 0f;
        float duration = 0.3f;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            time += Time.deltaTime;

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }*/
}
