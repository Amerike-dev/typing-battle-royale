using UnityEngine;
using System.Collections;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView MonolithView { get; private set; }
    public DebugPop debugPop;

    public GameObject[] Monoliths;
    public GameObject NearMonolith;

    [SerializeField] Vector2 hiddenPos =new Vector2 (0,600);
    [SerializeField] Vector2 visiblePos =new Vector2 (0,400);

    public float proximityRange = 3f;
    public float checkerMonolith = 0.5f;

    public bool isVisible = false;

    private void Start()
    {
        StartCoroutine(CheckMonolith());
    }

    public void NearMonolithCheck()
    {
        float nearestDistance = Mathf.Infinity;

        NearMonolith = null;

        foreach (var monolith in Monoliths)
        {
            if (monolith == null) continue;

            float distance = Vector3.Distance(monolith.transform.position, transform.position);

            if (distance < proximityRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                NearMonolith = monolith;
            }
            
        }

        if (NearMonolith != null)
        {
            MonolithView = NearMonolith.GetComponent<MonolithView>();
            Debug.Log("El monolito más cercano es " + NearMonolith.name);
        }
        else
        {
            MonolithView = null;
        }

        if (NearMonolith != null)
        {
            if (!isVisible)
            {
                isVisible = true;
                debugPop.MoveSignal(visiblePos);
            }
        }
        else
        {
            if (isVisible)
            {
                isVisible = false;
                debugPop.MoveSignal(hiddenPos);
            }
        }
    }

    IEnumerator CheckMonolith()
    {
        while (true)
        {
            NearMonolithCheck();

            yield return new WaitForSeconds(checkerMonolith);
        }
    }
}