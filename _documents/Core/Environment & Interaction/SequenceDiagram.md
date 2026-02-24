```mermaid
sequenceDiagram
    autonumber
    participant Input as Jugador (Input)
    participant PlayerView as PlayerInteractorView (MB)
    participant MonoView as MonolithView (MB)
    participant Controller as InteractionController (POCO)
    participant GM as GameStateManager (MB)

    %% Fase de Exploración y Detección
    Note over Input, GM: MODO EXPLORACIÓN (Movilidad Total)
    loop Cada Frame (Update)
        PlayerView->>PlayerView: ScanForInteractables() usando SphereCast
    end
    
    PlayerView->>MonoView: Detecta colisionador en capa 'Interactable'
    MonoView-->>PlayerView: Retorna MonolithID
    
    PlayerView->>Controller: IsWithinRange(InteractionRequest)
    Controller-->>PlayerView: true (Distancia validada matemáticamente)
    PlayerView->>MonoView: ToggleInteractionPrompt(true)
    
    %% Fase de Interacción
    Input->>PlayerView: Presiona tecla de interacción (Ej. 'F')
    PlayerView->>Controller: TryUnlockMonolith(InteractionRequest, out MonolithData)
    
    Controller->>Controller: ValidatePlayerTier()
    alt Requisitos Cumplidos
        Controller-->>PlayerView: true (Devuelve MonolithData con el reto)
        PlayerView->>MonoView: ToggleInteractionPrompt(false)
        PlayerView->>GM: RequestStateChange(BattleState)
        
        Note over Input, GM: MODO BATALLA (Estático)
        GM-->>PlayerView: Confirma transición de estado
        PlayerView->>MonoView: PlayUnlockVFX() (Partículas mágicas)
        
        %% Aquí se conectaría el TypingCombatController que diseñamos antes
        Note right of GM: Inicia el Reto de Escritura con el texto de MonolithData
        
    else Requisitos No Cumplidos o Agotado
        Controller-->>PlayerView: false
        PlayerView->>MonoView: Mostrar UI de error ("Nivel Insuficiente")
    end