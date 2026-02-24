```mermaid
classDiagram
    %% ==========================================
    %% MODELS (POCO - Datos Puros)
    %% ==========================================
    class MonolithData {
        +string MonolithID
        +int TierLevel %% Niveles 1 al 3
        +SpellData RewardSpell
        +string ChallengeText
        +bool IsDepleted
    }

    class InteractionRequest {
        +Vector3 PlayerPosition
        +Vector3 TargetPosition
        +string TargetID
        +PlayerStats PlayerStats
    }

    %% ==========================================
    %% CONTROLLERS (POCO - Lógica sin Unity)
    %% ==========================================
    class InteractionController {
        -float maxInteractionRange
        -Dictionary~string, MonolithData~ activeMonoliths
        +void LoadMapMonoliths(List~MonolithData~ monoliths)
        +bool IsWithinRange(InteractionRequest request)
        +bool TryUnlockMonolith(InteractionRequest request, out MonolithData data)
        -bool ValidatePlayerTier(PlayerStats stats, int monolithTier)
    }

    %% ==========================================
    %% VIEWS (MonoBehaviour - Físicas y Visuales)
    %% ==========================================
    class PlayerInteractorView {
        <<MonoBehaviour>>
        -float scanRadius
        -LayerMask interactableLayer
        -InteractionController interactionLogic
        +Update()
        -ScanForInteractables()
        -HandleInteractionInput()
    }

    class MonolithView {
        <<MonoBehaviour>>
        +string MonolithID
        +int AssignedTier
        +void ToggleInteractionPrompt(bool isVisible)
        +void PlayUnlockVFX()
        +void SetDepletedState()
    }
    
    class GameStateManager {
        <<MonoBehaviour>>
        +void RequestStateChange(IGameState newState)
    }

    %% ==========================================
    %% RELACIONES
    %% ==========================================
    PlayerInteractorView --> InteractionController : "Delega validación"
    InteractionController --> MonolithData : "Lee/Modifica estado"
    PlayerInteractorView ..> MonolithView : "Detecta mediante OverlapSphere/Raycast"
    PlayerInteractorView --> GameStateManager : "Solicita cambio a Modo Batalla"