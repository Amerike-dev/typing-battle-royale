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

    private void Start()
    {
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
                MonolithLevelSelectUI.Instance.Show(controller, player);
            }
        }
    }

    private void RefreshMonolithList()
    {
        registeredMonoliths.Clear();
        registeredMonoliths.AddRange(MonolithView.AllMonoliths);
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
            var player = GetComponent<PlayerController>();
            if (player != null && player.IsOwner)
            {
                RefreshMonolithList();
                NearMonolithCheck();
            }
            yield return new WaitForSeconds(checkerMonolith);
        }
    }
}