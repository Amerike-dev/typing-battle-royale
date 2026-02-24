```mermaid
classDiagram
    %% ==========================================
    %% MODELS (POCO - Datos Puros)
    %% ==========================================
    class SpellData {
        +string SpellID
        +string SpellName
        +string RunesToType
        +float BaseDamage
        +GameObject VfxPrefab
    }

    class TypingStats {
        +int TotalKeystrokes
        +int CorrectKeystrokes
        +int Mistakes
        +float StartTime
        +float EndTime
        +float CalculateWPM()
        +float CalculateAccuracy()
    }

    %% ==========================================
    %% CONTROLLERS (POCO - Lógica de Escritura Pura)
    %% ==========================================
    class TypingCombatLogic {
        -SpellData currentSpell
        -TypingStats currentStats
        -int currentRuneIndex
        +void InitializeSpell(SpellData spell)
        +bool ValidateKeystroke(char inputChar)
        +float GetCompletionPercentage()
        +bool IsSpellComplete()
        +TypingStats GetFinalStats()
    }

    class DamageCalculator {
        +float CalculateFinalDamage(SpellData spell, TypingStats stats)
        -float ApplyWPMBonus(float baseDamage, float wpm)
        -float ApplyAccuracyPenalty(float baseDamage, float accuracy)
    }

    %% ==========================================
    %% VIEWS (MonoBehaviour - Input y Renderizado)
    %% ==========================================
    class TypingInputView {
        <<MonoBehaviour>>
        -TypingCombatLogic typingLogic
        +Update()
        -void OnGUI() %% Útil para capturar Event.current.character
        -void ProcessInput(char key)
    }

    class TypingUIView {
        <<MonoBehaviour>>
        +void SetupSpellUI(string spellText)
        +void UpdateProgressBar(float percentage)
        +void HighlightCorrectLetter(int index)
        +void TriggerMistakeFeedback()
    }

    class SpellCasterView {
        <<MonoBehaviour>>
        -Transform castPoint
        +void InstantiateSpellVFX(GameObject vfxPrefab)
    }

    %% ==========================================
    %% RELACIONES
    %% ==========================================
    TypingInputView *-- TypingCombatLogic : "Instancia y consulta"
    TypingCombatLogic --> SpellData : "Lee"
    TypingCombatLogic *-- TypingStats : "Modifica"
    
    TypingInputView --> TypingUIView : "Actualiza barras de progreso "
    TypingInputView --> DamageCalculator : "Solicita cálculo al finalizar"
    TypingInputView --> SpellCasterView : "Ordena instanciar VFX "
