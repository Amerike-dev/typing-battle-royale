using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DemoBotController : MonoBehaviour 
{
    public enum DemoBotState { Idle, MoveToMonolith, CastSpell, MoveToEnemy, Dead }

    [Header("Configuración IA")]
    public DemoBotState currentState = DemoBotState.Idle;
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float spellCastDuration = 1.5f;
    [SerializeField] private float interactionRange = 2f;

    private NavMeshAgent agent;
    private float stateTimer;
    private Transform currentTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        TransitionToState(DemoBotState.Idle);
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case DemoBotState.Idle:
                if (stateTimer <= 0) TransitionToState(DemoBotState.MoveToMonolith);
                break;
            case DemoBotState.MoveToMonolith:
                if (HasReachedDestination()) TransitionToState(DemoBotState.CastSpell);
                break;
            case DemoBotState.CastSpell:
                if (stateTimer <= 0) TransitionToState(DemoBotState.MoveToEnemy);
                break;
            case DemoBotState.MoveToEnemy:
                if (HasReachedDestination()) TransitionToState(DemoBotState.Idle);
                break;
            case DemoBotState.Dead:
                break;
        }
    }

    private void TransitionToState(DemoBotState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case DemoBotState.Idle:
                agent.isStopped = true;
                stateTimer = idleDuration;
                break;
            case DemoBotState.MoveToMonolith:
                agent.isStopped = false;
                MonolithTag monolith = FindAnyObjectByType<MonolithTag>();
                if (monolith != null)
                {
                    currentTarget = monolith.transform;
                    agent.SetDestination(currentTarget.position);
                }
                break;
            case DemoBotState.CastSpell:
                agent.isStopped = true;
                stateTimer = spellCastDuration;
                ExecuteFakeSpell();
                break;
            case DemoBotState.MoveToEnemy:
                agent.isStopped = false;
                currentTarget = FindClosestEnemy();
                if (currentTarget != null) agent.SetDestination(currentTarget.position);
                else TransitionToState(DemoBotState.Idle);
                break;
            case DemoBotState.Dead:
                agent.isStopped = true;
                break;
        }
    }

    private bool HasReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance || agent.remainingDistance <= interactionRange)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) return true;
            }
        }
        return false;
    }

    private void ExecuteFakeSpell()
    {
        float accuracy = Random.Range(0.70f, 0.95f);
        if (Random.value <= accuracy) Debug.Log($"{gameObject.name} casteó un hechizo ¡Y ACERTÓ!");
    }

    private Transform FindClosestEnemy()
    {
        DemoBotController[] allBots = FindObjectsByType<DemoBotController>(FindObjectsSortMode.None);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var bot in allBots)
        {
            if (bot == this || bot.currentState == DemoBotState.Dead) continue;
            
            float dist = Vector3.Distance(transform.position, bot.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = bot.transform;
            }
        }
        return closest;
    }
}