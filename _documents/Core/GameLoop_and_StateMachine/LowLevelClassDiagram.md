```mermaid
classDiagram
    %% ==========================================
    %% MODELS (POCO - Datos Puros)
    %% ==========================================
    class MatchSessionData {
        +string MatchID
        +int PlayersAlive
        +float MatchTimer
        +bool IsMatchActive
    }

    %% ==========================================
    %% CORE STATE MACHINE (POCO - Lógica pura)
    %% ==========================================
    class StateMachine {
        -IGameState currentState
        +void Initialize(IGameState startingState)
        +void TransitionTo(IGameState nextState)
        +void Tick()
    }

    class IGameState {
        <<interface>>
        +void Enter()
        +void Execute()
        +void Exit()
    }

    class ExplorationState {
        -PlayerMovementController movementController
        +void Enter()
        +void Execute()
        +void Exit()
    }

    class BattleState {
        -TypingCombatController combatController
        +void Enter()
        +void Execute()
        +void Exit()
    }

    %% ==========================================
    %% MANAGERS / VIEWS (MonoBehaviour)
    %% ==========================================
    class GameManager {
        <<MonoBehaviour>>
        -StateMachine gameStateMachine
        -MatchSessionData currentMatch
        +void Awake()
        +void Update()
        +void TriggerBattleEvent()
        +void EndMatch()
    }

    class UIManager {
        <<MonoBehaviour>>
        +void ShowExplorationHUD()
        +void ShowBattleHUD()
        +void UpdateMatchTimer(float time)
    }

    %% ==========================================
    %% RELACIONES
    %% ==========================================
    GameManager "1" *-- "1" StateMachine : Contiene y actualiza
    GameManager --> MatchSessionData : Gestiona datos
    StateMachine "1" o-- "1" IGameState : Estado actual
    IGameState <|.. ExplorationState : Implementa
    IGameState <|.. BattleState : Implementa
    
    ExplorationState ..> UIManager : Solicita UI
    BattleState ..> UIManager : Solicita UI