using UnityEngine;
using System.Collections;

public class DebugPop : MonoBehaviour
{
    public Coroutine PopMove;
    public RectTransform startPos;
    public RectTransform endPos;
    public RectTransform Pop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Pop.anchoredPosition = Vector2.Lerp(startPos.anchoredPosition, endPos.anchoredPosition, 2f);
    }
}
