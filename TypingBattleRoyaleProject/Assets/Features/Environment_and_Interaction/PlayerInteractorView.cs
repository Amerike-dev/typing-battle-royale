using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    void OnEnable()
    {
        MonolithView.OnMonolithRegistered += HandleMonolithRegistered;
        MonolithView.OnMonolithUnregistered += HandleMonolithUnregistered;

        foreach (var monolith in FindObjectsByType<MonolithView>(FindObjectsSortMode.None))
        {
            HandleMonolithRegistered(monolith);
        }
    }

    private void OnDisable()
    {
        MonolithView.OnMonolithRegistered -= HandleMonolithRegistered;
        MonolithView.OnMonolithUnregistered -= HandleMonolithUnregistered;
    }

    private void HandleMonolithRegistered(MonolithView monolith)
    {
        if (!registeredMonoliths.Contains(monolith))
        {
            registeredMonoliths.Add(monolith);
            Debug.Log($"[PlayerInteractor] Monolito registrado: {monolith.name}. Total: {registeredMonoliths.Count}");
        }
    }

    private void HandleMonolithUnregistered(MonolithView monolith)
    {
        registeredMonoliths.Remove(monolith);
    }

    private void Start()
    {
        StartCoroutine(CheckMonolith());
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
                debugPop.MoveSignal(signalShowPos, 1f);
            }
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

        Debug.Log($"[Check] Monolitos registrados: {registeredMonoliths.Count}");
        foreach (var m in registeredMonoliths)
            Debug.Log($"  → {m.name} pos:{m.transform.position} dist:{Vector3.Distance(m.transform.position, transform.position)}");
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