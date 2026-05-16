using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView MonolithView { get; private set; }
    public DebugPop debugPop;

    private readonly List<MonolithView> registeredMonoliths = new List<MonolithView>();
    public GameObject NearMonolith;

    [SerializeField] Vector2 signalHidePos;
    [SerializeField] Vector2 signalShowPos;

    public float proximityRange = 3f;
    public float checkerMonolith = 0.5f;

    public bool isVisible = false;

    private void Start()
    {
        StartCoroutine(CheckMonolith());
    }

    private void RefreshMonolithList()
    {
        var found = FindObjectsByType<MonolithView>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var monolith in found)
        {
            if (!registeredMonoliths.Contains(monolith))
            {
                registeredMonoliths.Add(monolith);
                Debug.Log($"[PlayerInteractor] Monolito encontrado: {monolith.name} | Total: {registeredMonoliths.Count}");
            }
        }

        registeredMonoliths.RemoveAll(m => m == null);
    }

    public void NearMonolithCheck()
    {
        float nearestDistance = Mathf.Infinity;
        NearMonolith = null;

        foreach (var monolith in registeredMonoliths)
        {
            if (monolith == null) continue;

            float distance = Vector3.Distance(monolith.transform.position, transform.position);

            if (distance < proximityRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                NearMonolith = monolith.gameObject;
            }
        }

        if (NearMonolith != null)
        {
            MonolithView = NearMonolith.GetComponent<MonolithView>();
            if (!isVisible)
            {
                isVisible = true;
                if (debugPop != null) debugPop.MoveSignal(signalShowPos, 1f);
            }
        }
        else
        {
            MonolithView = null;
            if (isVisible)
            {
                isVisible = false;
                if (debugPop != null) debugPop.MoveSignal(signalHidePos, 0f);
            }
        }
    }


    IEnumerator CheckMonolith()
    {
        while (true)
        {
            RefreshMonolithList();
            NearMonolithCheck();
            yield return new WaitForSeconds(checkerMonolith);
        }
    }
}