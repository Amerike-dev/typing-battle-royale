using UnityEngine;
using DG.Tweening;
using System.Collections;

public static class UIAnimator
{
    public static void FadeIn(CanvasGroup canvasUI, float time)
    {
        canvasUI.alpha = 0; 
        canvasUI.gameObject.SetActive(true);
        canvasUI.DOFade(1, time);
    }
    public static void FadeOut(CanvasGroup canvasUI, float time)
    {
        canvasUI.DOFade(0, time).OnComplete(() => canvasUI.gameObject.SetActive(false));
    }
    //Referencia de como se tiene que utilizar en la clase MonoBihaviour.
    //Si se utiliza en con algun metodo Fade enonces comparte el "float time"
    /*[SerializeField] RectTransform panelUI;
    [SerializeField] float time;
    Coroutine panelUIMove;
    public void MoveSignal(Vector2 target)
    {
        if (panelUIMove != null)
        {
            StopCoroutine(panelUIMove);
        }
        panelUIMove = StartCoroutine(PanelUIMove(panelUI,target, time));
    }*/

    public static IEnumerator PanelUIMove(RectTransform canvasGrupUI, Vector2 target, float time)
    {
        Vector2 startPos = canvasGrupUI.anchoredPosition;
        float duration = 0.3f;
        while (time < duration)
        {
            canvasGrupUI.anchoredPosition = Vector2.Lerp(startPos, target, time / duration);
            time += Time.deltaTime;

            yield return null;
        }
        canvasGrupUI.anchoredPosition = target;
    }
}
