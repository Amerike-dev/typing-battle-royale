using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform targetUI;

    [SerializeField] float hoverScale = 1.05f;
    [SerializeField] float duration = 0.1f;

    Vector3 originalScale;
    Coroutine scaleRoutine;

    private void Awake()
    {
        originalScale = targetUI.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(originalScale);
    }

    void ScaleTo(Vector3 targetScale)
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(SmoothScale(targetScale));
    }

    IEnumerator SmoothScale(Vector3 targetScale)
    {
        Vector3 startScale = targetUI.localScale;

        float time = 0f;

        while (time < duration)
        {
            targetUI.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        targetUI.localScale = targetScale;
    }
}
