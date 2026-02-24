```mermaid
sequenceDiagram
    autonumber
    participant Input as Input System (Unity)
    participant Pawn as PlayerPawnView (MB)
    participant Logic as PlayerMovementLogic (POCO)
    participant Anim as PlayerAnimatorView (MB)
    participant CC as CharacterController (Unity)
    participant GM as GameStateManager (MB)

    %% MODO EXPLORACIÓN
    Note over Input, CC: ESTADO: MODO EXPLORACIÓN (Movimiento Libre)
    loop Cada Frame (Update)
        Pawn->>Input: CaptureInput() (Lee WASD/Mouse)
        Input-->>Pawn: Retorna PlayerInputData
        
        Pawn->>Logic: CalculateMovementVector(PlayerInputData, deltaTime)
        Logic-->>Pawn: Retorna Vector3 (Dirección final)
        
        Pawn->>Logic: CalculateGravityAndJump(...)
        Logic-->>Pawn: Retorna Vector3 (Fuerza vertical)
        
        Pawn->>CC: CC.Move(VectorFinal)
        
        Pawn->>Logic: CheckIfMoving(VectorFinal)
        Logic-->>Pawn: true / false
        Pawn->>Anim: UpdateMovementAnimation(isMoving)
        Note right of Anim: Interpola entre<br/>Idle y Walk
    end

    %% TRANSICIÓN A MODO BATALLA
    Note over Input, GM: EVENTO: TRANSICIÓN A MODO BATALLA
    GM->>Pawn: Notifica cambio de estado (Bloqueo)
    Pawn->>Logic: SetMovementLock(true)
    Pawn->>Anim: TriggerCastingPose()
    Note right of Anim: Activa la pose estática<br/>de lectura de libro

    %% MODO BATALLA
    loop Cada Frame (Update)
        Pawn->>Input: CaptureInput()
        Pawn->>Logic: CalculateMovementVector(...)
        Logic-->>Pawn: Retorna Vector3.zero (Porque IsMovementLocked = true)
        Pawn->>CC: CC.Move(Vector3.zero)
    end
