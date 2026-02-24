```mermaid
classDiagram
    %% ==========================================
    %% CORE MANAGERS (MonoBehaviour)
    %% ==========================================
    class GameManager {
        <<MonoBehaviour>>
        -IGameState currentState
        +ChangeState(IGameState newState)
        -Update()
    }

    class NetworkManagerMock {
        <<MonoBehaviour>>
        +SyncPlayerState(PlayerStats stats)
        +SendDamageEvent(string targetId, float damage)
        +RequestSpawn()
    }

    %% ==========================================
    %% PATRÓN DE ESTADOS (Mecánica Dual)
    %% ==========================================
    class IGameState {
        <<interface>>
        +Enter()
        +Execute()
        +Exit()
    }
    class ExplorationState {
        +Enter()
        +Execute()
        +Exit()
    }
    class BattleState {
        +Enter()
        +Execute()
        +Exit()
    }

    IGameState <|.. ExplorationState
    IGameState <|.. BattleState
    GameManager "1" *-- "1" IGameState : Manages

    %% ==========================================
    %% MODELS (POCO - Sin MonoBehaviour - Datos puros)
    %% ==========================================
    class PlayerStats {
        +string PlayerID
        +float Health
        +float MaxHealth
        +int UnlockedSpellsLevel
        +bool IsAlive()
    }

    class SpellData {
        +string SpellName
        +string RequiredText
        +float BaseDamage
        +int Tier
    }

    %% ==========================================
    %% CONTROLLERS / LÓGICA CORE (POCO - Sin MonoBehaviour)
    %% ==========================================
    class TypingCombatController {
        -string targetWord
        -string currentInput
        +SetTargetWord(string word)
        +bool ValidateKeystroke(char key)
        +float GetCompletionPercentage()
        +bool IsWordComplete()
    }

    class DamageCalculator {
        +float CalculateFinalDamage(SpellData spell, PlayerStats attacker)
        +bool DetermineCriticalHit(float typingSpeedWPM)
    }

    class SpawnCalculator {
        -List~Vector3~ mapSpawnNodes
        +Vector3 GetOptimalSpawnPoint(List~Vector3~ enemyPositions)
        +Vector3 GetRandomMonolithSpawn(int tier)
    }

    %% ==========================================
    %% VIEWS (MonoBehaviour - Visuales y Unity Core)
    %% ==========================================
    class PlayerView {
        <<MonoBehaviour>>
        +UpdateMovement(Vector3 position)
        +PlayAnimation(string animState)
        +InstantiateVFX(GameObject vfxPrefab)
    }

    class TypingUIView {
        <<MonoBehaviour>>
        +UpdateProgressBar(float percentage)
        +ShowKeystrokeFeedback(bool isCorrect)
        +DisplaySpellText(string text)
    }

    class MonolithView {
        <<MonoBehaviour>>
        +ShowInteractionPrompt()
        +TriggerUnlockAnimation()
    }

    %% ==========================================
    %% RELACIONES
    %% ==========================================
    GameManager --> TypingCombatController : Uses
    GameManager --> DamageCalculator : Uses
    NetworkManagerMock --> SpawnCalculator : Uses

    TypingCombatController --> SpellData : Validates
    DamageCalculator --> SpellData : Reads
    DamageCalculator --> PlayerStats : Reads

    TypingUIView ..> TypingCombatController : Listens to Events
    PlayerView ..> PlayerStats : Observes state
    MonolithView ..> PlayerStats : Modifies (Unlock)