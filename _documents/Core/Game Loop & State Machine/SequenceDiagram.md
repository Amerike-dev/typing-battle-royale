```mermaid
sequenceDiagram
    autonumber
    participant Unity as Unity Engine
    participant GM as GameManager (MB)
    participant SM as StateMachine (POCO)
    participant ExpState as ExplorationState (POCO)
    participant BatState as BattleState (POCO)
    participant UI as UIManager (MB)

    %% Inicialización
    Unity->>GM: Awake() / Start()
    GM->>SM: Initialize(new ExplorationState())
    SM->>ExpState: Enter()
    ExpState->>UI: ShowExplorationHUD()

    %% Game Loop - Exploración
    loop Cada Frame (Modo Exploración)
        Unity->>GM: Update()
        GM->>SM: Tick()
        SM->>ExpState: Execute()
        Note right of ExpState: Lógica de movilidad total<br/>y búsqueda de recursos
    end

    %% Transición a Combate (Mecánica Dual)
    Note over GM, UI: Evento de Transición (Ej. Disparo recibido o inicio de Monolito)
    GM->>SM: TransitionTo(new BattleState())
    
    SM->>ExpState: Exit()
    Note right of ExpState: Deshabilita inputs de movimiento
    
    SM->>BatState: Enter()
    BatState->>UI: ShowBattleHUD()

    %% Game Loop - Batalla
    loop Cada Frame (Modo Batalla)
        Unity->>GM: Update()
        GM->>SM: Tick()
        SM->>BatState: Execute()
        Note right of BatState: Lógica estática y<br/>enfoque en escritura
    end