```mermaid
classDiagram
    %% ==========================================
    %% MODELS (POCO - Datos Puros)
    %% ==========================================
    class MovementData {
        +float BaseSpeed
        +float Gravity
        +float JumpForce
        +Vector3 CurrentVelocity
        +bool IsGrounded
        +bool IsMovementLocked
    }

    class PlayerInputData {
        +Vector2 MoveAxis
        +Vector2 LookAxis
        +bool JumpPressed
    }

    %% ==========================================
    %% CONTROLLERS (POCO - Lógica Matemática Pura)
    %% ==========================================
    class PlayerMovementLogic {
        -MovementData moveData
        +void SetMovementLock(bool isLocked)
        +Vector3 CalculateMovementVector(PlayerInputData input, float deltaTime)
        +Vector3 CalculateGravityAndJump(PlayerInputData input, float deltaTime)
        +bool CheckIfMoving(Vector3 moveVector)
    }

    %% ==========================================
    %% VIEWS (MonoBehaviour - Físicas, Inputs y Animación)
    %% ==========================================
    class PlayerPawnView {
        <<MonoBehaviour>>
        -CharacterController charController
        -Transform cameraTransform
        -PlayerMovementLogic movementLogic
        -PlayerInputData currentInput
        +Update()
        -CaptureInput()
        -ApplyMovement()
    }

    class PlayerAnimatorView {
        <<MonoBehaviour>>
        -Animator wizardAnimator
        +void UpdateMovementAnimation(bool isMoving)
        +void TriggerCastingPose()
        +void TriggerIdlePose()
    }

    %% ==========================================
    %% RELACIONES
    %% ==========================================
    PlayerPawnView *-- PlayerMovementLogic : Instancia y usa
    PlayerMovementLogic --> MovementData : Modifica y lee
    PlayerMovementLogic ..> PlayerInputData : Recibe como parámetro
    PlayerPawnView --> PlayerAnimatorView : Notifica cambios de estado