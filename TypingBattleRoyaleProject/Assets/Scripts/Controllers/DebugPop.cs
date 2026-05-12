using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebugPop : MonoBehaviour
{
    public Image Pop;
    Coroutine signalMove;
    public CanvasGroup Siganl;
    public void MoveSignal(Vector2 target)
    {
        if(signalMove != null)
        {
            StopCoroutine(signalMove);
        }
        signalMove = StartCoroutine(NearMonolithSignal(target));
    }

    IEnumerator NearMonolithSignal(Vector2 target)
    {
        Vector2 startPos = Pop.rectTransform.anchoredPosition;

        float time = 0f;
        float duration = 0.3f;

        while (time < duration)
        {
            Pop.rectTransform.anchoredPosition = Vector2.Lerp(startPos, target, time / duration);
            time += Time.deltaTime;

            yield return null;
        }

        Pop.rectTransform.anchoredPosition = target;
    }
}
