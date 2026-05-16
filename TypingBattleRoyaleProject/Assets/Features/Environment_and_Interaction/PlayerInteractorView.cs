using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView MonolithView { get; private set; }
    public DebugPop debugPop;

    public GameObject[] Monoliths;
    public GameObject NearMonolith;

    [SerializeField] Vector2 signalHidePos;
    [SerializeField] Vector2 signalShowPos;

    public float proximityRange = 3f;
    public float checkerMonolith = 0.5f;

    public bool isVisible = false;

    private void Start()
    {
        StartCoroutine(CheckMonolith());
        StartCoroutine(FindAllMonolithsRoutine());
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
            if (!isVisible)
            {
                isVisible = true;
                debugPop.MoveSignal(signalShowPos, 1f);
            }
            Debug.Log("El monolito m�s cercano es " + NearMonolith.name);
        }
        else
        {
            MonolithView = null;
            if (isVisible)
            {
                isVisible = false;
                debugPop.MoveSignal(signalHidePos, 0f);
            }
        }
    }

    IEnumerator FindAllMonolithsRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        Monoliths = FindObjectsByType<MonolithView>(FindObjectsSortMode.None).Select(m => m.gameObject).ToArray();
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