using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    private bool _loggedOwnerActive = false;
    private int _lastLoggedMonolithCount = -1;
    private GameObject _lastLoggedNear = null;

    private void Start()
    {
        Debug.Log($"[MONOLITH-POP] PlayerInteractorView.Start on '{gameObject.name}' — debugPop={(debugPop != null ? "OK" : "NULL")}, proximityRange={proximityRange}, checkerInterval={checkerMonolith}s, hidePos={signalHidePos}, showPos={signalShowPos}");
        StartCoroutine(CheckMonolith());
    }
    
    void Update()
    {
        var player = GetComponent<PlayerController>();
        
        if (player == null || !player.IsOwner) return; 

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                return;
            }
        }
        
        if (Keyboard.current.eKey.wasPressedThisFrame && NearMonolith != null)
        {
            var controller = NearMonolith.GetComponent<MonolithController>();

            if (controller != null && player != null)
            {
                var animatorView = player.playerAnimatorView != null
                    ? player.playerAnimatorView
                    : player.GetComponentInChildren<PlayerAnimatorView>(true);
                if (animatorView != null) animatorView.TriggerInteract();

                MonolithLevelSelectUI.Instance.Show(controller, player);
            }
        }
    }

    private void RefreshMonolithList()
    {
        registeredMonoliths.Clear();
        registeredMonoliths.AddRange(MonolithView.AllMonoliths);
        registeredMonoliths.RemoveAll(m => m == null);

        if (registeredMonoliths.Count != _lastLoggedMonolithCount)
        {
            Debug.Log($"[MONOLITH-POP] AllMonoliths count changed: {_lastLoggedMonolithCount} -> {registeredMonoliths.Count}");
            _lastLoggedMonolithCount = registeredMonoliths.Count;
        }
    }

    public void NearMonolithCheck()
    {
        float nearestDistance = Mathf.Infinity;
        NearMonolith = null;

        float closestObserved = Mathf.Infinity;
        string closestName = "<none>";

        foreach (var monolith in registeredMonoliths)
        {
            if (monolith == null) continue;

            float distance = Vector3.Distance(monolith.transform.position, transform.position);

            if (distance < closestObserved)
            {
                closestObserved = distance;
                closestName = monolith.gameObject.name;
            }

            if (distance < proximityRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                NearMonolith = monolith.gameObject;
            }
        }

        if (NearMonolith != _lastLoggedNear)
        {
            if (NearMonolith != null)
            {
                Debug.Log($"[MONOLITH-POP] NEAR -> {NearMonolith.name} @ {nearestDistance:F2} (range={proximityRange})");
            }
            else
            {
                Debug.Log($"[MONOLITH-POP] NEAR -> none. Closest in scene: {closestName} @ {closestObserved:F2} (range={proximityRange})");
            }
            _lastLoggedNear = NearMonolith;
        }

        if (NearMonolith != null)
        {
            MonolithView = NearMonolith.GetComponent<MonolithView>();
            if (!isVisible)
            {
                isVisible = true;
                Debug.Log($"[MONOLITH-POP] SHOW signal -> debugPop={(debugPop != null ? "OK" : "NULL")}");
                if (debugPop != null) debugPop.MoveSignal(signalShowPos, 1f);
            }
        }
        else
        {
            MonolithView = null;
            if (isVisible)
            {
                isVisible = false;
                Debug.Log($"[MONOLITH-POP] HIDE signal -> debugPop={(debugPop != null ? "OK" : "NULL")}");
                if (debugPop != null) debugPop.MoveSignal(signalHidePos, 0f);
            }
        }
    }


    IEnumerator CheckMonolith()
    {
        while (true)
        {
            var player = GetComponent<PlayerController>();
            if (player != null && player.IsOwner)
            {
                if (!_loggedOwnerActive)
                {
                    Debug.Log($"[MONOLITH-POP] CheckMonolith activated for owner '{gameObject.name}'");
                    _loggedOwnerActive = true;
                }
                RefreshMonolithList();
                NearMonolithCheck();
            }
            yield return new WaitForSeconds(checkerMonolith);
        }
    }
}