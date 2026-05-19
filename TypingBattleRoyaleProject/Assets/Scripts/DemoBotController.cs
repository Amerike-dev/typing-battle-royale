using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DemoBotController : MonoBehaviour 
{
    public enum DemoBotState { Idle, Wander, MoveToMonolith, MoveToEnemy, CastSpell, Flee, Dead }

    [Header("Configuración IA")]
    public DemoBotState currentState = DemoBotState.Idle;
    [SerializeField] private float idleDuration = 1.5f;
    [SerializeField] private float spellCastDuration = 1f;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float aggroRadius = 10f;

    [Header("Efectos Visuales")]
    [SerializeField] private ParticleSystem fakeSpellVFX;

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
        
        if (currentState == DemoBotState.CastSpell && currentTarget != null) FaceTarget(currentTarget.position);
        
        if (currentState == DemoBotState.Wander || currentState == DemoBotState.MoveToMonolith) CheckForEnemies();

        switch (currentState)
        {
            case DemoBotState.Idle:
                if (stateTimer <= 0) DecideNextAction();
                break;
            
            case DemoBotState.Wander:
            case DemoBotState.MoveToMonolith:
                if (HasReachedDestination()) TransitionToState(DemoBotState.Idle);
                break;

            case DemoBotState.MoveToEnemy:
                if (HasReachedDestination()) TransitionToState(DemoBotState.CastSpell);
                break;

            case DemoBotState.CastSpell:
                if (stateTimer <= 0) TransitionToState(DemoBotState.Flee);
                break;

            case DemoBotState.Flee:
                if (HasReachedDestination()) TransitionToState(DemoBotState.Idle);
                break;

            case DemoBotState.Dead:
                break;
        }
    }
    
    private void CheckForEnemies()
    {
        Transform enemy = FindClosestEnemyWithinRange();
        if (enemy != null)
        {
            DemoBotController enemyBot = enemy.GetComponent<DemoBotController>();
            
            if (enemyBot != null) EngageCombat(enemyBot);
        }
    }
    
    public void EngageCombat(DemoBotController opponent)
    {
        if (currentState == DemoBotState.Dead || currentState == DemoBotState.CastSpell || 
            currentState == DemoBotState.MoveToEnemy || currentState == DemoBotState.Flee) 
            return;

        currentTarget = opponent.transform;
        TransitionToState(DemoBotState.MoveToEnemy);
        opponent.EngageCombat(this);
    }

    private void DecideNextAction()
    {
        if (Random.value < 0.3f)
            TransitionToState(DemoBotState.MoveToMonolith);
        else
            TransitionToState(DemoBotState.Wander);
    }

    private void TransitionToState(DemoBotState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case DemoBotState.Idle:
                agent.isStopped = true;
                stateTimer = Random.Range(idleDuration * 0.5f, idleDuration * 1f);
                break;

            case DemoBotState.Wander:
                agent.isStopped = false;
                currentTarget = null;
                SetRandomDestination(8f);
                break;

            case DemoBotState.MoveToMonolith:
                agent.isStopped = false;
                MonolithTag monolith = FindAnyObjectByType<MonolithTag>();
                if (monolith != null)
                {
                    currentTarget = monolith.transform;
                    agent.SetDestination(currentTarget.position);
                }
                else TransitionToState(DemoBotState.Wander);
                break;

            case DemoBotState.MoveToEnemy:
                agent.isStopped = false;
                if (currentTarget != null) 
                    agent.SetDestination(currentTarget.position);
                break;

            case DemoBotState.CastSpell:
                agent.isStopped = true;
                stateTimer = spellCastDuration;
                ExecuteFakeSpell();
                break;

            case DemoBotState.Flee:
                agent.isStopped = false;
                if (currentTarget != null)
                    SetFleeDestination(currentTarget.position);
                else
                    SetRandomDestination(10f);
                break;

            case DemoBotState.Dead:
                agent.isStopped = true;
                break;
        }
    }
    
    private void SetRandomDestination(float radius)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized * radius;
        Vector3 randomPos = transform.position + new Vector3(randomDir.x, 0, randomDir.y);
        
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            TransitionToState(DemoBotState.Idle);
    }
    
    private void SetFleeDestination(Vector3 dangerPos)
    {
        Vector3 runDirection = (transform.position - dangerPos).normalized;
        Vector3 fleePos = transform.position + runDirection * 8f;
        
        if (NavMesh.SamplePosition(fleePos, out NavMeshHit hit, 8f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            SetRandomDestination(8f);
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
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
        if (fakeSpellVFX != null) fakeSpellVFX.Play();
        
        float accuracy = Random.Range(0.70f, 0.95f);
        if (Random.value <= accuracy) Debug.Log($"{gameObject.name} casteó un hechizo ¡Y ACERTÓ!");
    }
    
    private Transform FindClosestEnemyWithinRange()
    {
        DemoBotController[] allBots = FindObjectsByType<DemoBotController>(FindObjectsSortMode.None);
        Transform closest = null;
        float minDistance = aggroRadius;

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