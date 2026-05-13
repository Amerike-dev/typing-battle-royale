using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebugPop : MonoBehaviour
{
    public Image Pop;
    Coroutine signalMove;
    public CanvasGroup Signal;
    public void MoveSignal(Vector2 target, float targetAlpha)
    {
        if(signalMove != null)
        {
            StopCoroutine(signalMove);
        }
        signalMove = StartCoroutine(NearMonolithSignal(target, targetAlpha));
    }

    IEnumerator NearMonolithSignal(Vector2 target, float targetAlpha)
    {
        Vector2 startPos = Pop.rectTransform.anchoredPosition;
        float startAlpha = Signal.alpha;

        float time = 0f;
        float duration = 0.3f;

        while (time < duration)
        {
            Pop.rectTransform.anchoredPosition = Vector2.Lerp(startPos, target, time / duration);
            Signal.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            time += Time.deltaTime;

            yield return null;
        }

        Pop.rectTransform.anchoredPosition = target;
        Signal.alpha = targetAlpha;
    }
}
