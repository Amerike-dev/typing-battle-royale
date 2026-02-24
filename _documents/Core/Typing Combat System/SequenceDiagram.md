```mermaid
sequenceDiagram
    autonumber
    participant Hardware as Teclado (Jugador)
    participant InputView as TypingInputView (MB)
    participant Logic as TypingCombatLogic (POCO)
    participant UI as TypingUIView (MB)
    participant DmgCalc as DamageCalculator (POCO)
    participant Caster as SpellCasterView (MB)

    %% Inicialización del Hechizo
    Note over InputView, UI: El jugador selecciona un hechizo del libro
    InputView->>Logic: InitializeSpell(SpellData)
    Logic->>Logic: Reinicia TypingStats y currentRuneIndex = 0
    InputView->>UI: SetupSpellUI(SpellData.RunesToType)

    %% Bucle de Transcripción
    loop Hasta que IsSpellComplete() == true
        Hardware->>InputView: Presiona Tecla (ej. 'A')
        InputView->>Logic: ValidateKeystroke('A')
        
        alt Tecla Correcta
            Logic-->>InputView: true
            InputView->>UI: HighlightCorrectLetter(currentRuneIndex)
            InputView->>UI: UpdateProgressBar(percentage)
        else Tecla Incorrecta
            Logic-->>InputView: false
            InputView->>UI: TriggerMistakeFeedback() (Flash rojo/Sonido)
        end
    end

    %% Resolución del Hechizo
    Note over InputView, Caster: Hechizo completado con éxito
    InputView->>Logic: GetFinalStats()
    Logic-->>InputView: Retorna TypingStats (WPM, Precisión)
    
    InputView->>DmgCalc: CalculateFinalDamage(SpellData, TypingStats)
    DmgCalc-->>InputView: Retorna float (Daño Final)
    
    InputView->>Caster: InstantiateSpellVFX(SpellData.VfxPrefab)
    Note right of Caster: Instancia efectos de partículas<br/>(fuego, hielo, rayo) 
    
    %% Aquí el daño final se enviaría al gestor de red (cuando se implemente LAN) o al enemigo local
