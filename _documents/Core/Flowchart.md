```mermaid
flowchart TD
    %% Estilos
    classDef state fill:#2E86C1,stroke:#1B4F72,stroke-width:2px,color:#fff;
    classDef action fill:#28B463,stroke:#186A3B,stroke-width:2px,color:#fff;
    classDef combat fill:#CB4335,stroke:#7B241C,stroke-width:2px,color:#fff;
    classDef decision fill:#F1C40F,stroke:#9A7D0A,stroke-width:2px,color:#000;
    classDef terminal fill:#8E44AD,stroke:#512E5F,stroke-width:3px,color:#fff;

    Start((Inicio de Partida LAN)):::terminal --> Spawn[Aparición en Arena de Inicio]
    Spawn --> ModeExp[ESTADO: EXPLORACIÓN
    Movilidad total
    Búsqueda de recursos]:::state
    
    ModeExp --> Decision{¿Qué encuentra
    el jugador?}:::decision
    
    %% Flujo de Monolitos
    Decision -->|Encuentra Monolito| Monolith[Interactúa con Monolito de Runas
    Niveles 1, 2 o 3]:::action
    Monolith --> MonoChallenge[Reto de Escritura]
    MonoChallenge -->|Éxito| UnlockSpell[Desbloquea Hechizo de Mayor Poder\nActualiza Libro Rúnico]:::action
    UnlockSpell --> ModeExp
    
    %% Flujo de Combate
    Decision -->|Encuentra Enemigo| ModeBat[ESTADO: BATALLA MANUAL
    Posición Estática
    Pose de Lectura]:::combat
    ModeBat --> SelectSpell[Abre Libro de Hechizos
    Selecciona Hechizo]:::combat
    SelectSpell --> TypeAction[Inicia Transcripción 
    Agilidad Dactilar y Ortografía]:::combat
    
    TypeAction --> InputCheck{¿Tecla/Letra Correcta?}:::decision
    InputCheck -->|No| Penalty[Penalización de Tiempo / Falla]:::combat
    Penalty --> TypeAction
    
    InputCheck -->|Sí| ProgressUI[Actualiza Barra de Progreso
    de Escritura]:::action
    ProgressUI --> SpellComplete{¿Palabra Completada?}:::decision
    
    SpellComplete -->|No| TypeAction
    SpellComplete -->|Sí| CastSpell[Ejecuta Hechizo
    Instancia VFX Fuego/Hielo/Rayo]:::action
    
    CastSpell --> ResolveDmg[Cálculo de Daño Generado y Recibido]
    
    ResolveDmg --> CheckHealthPlayer{¿Jugador
    Sin HP?}:::decision
    ResolveDmg --> CheckHealthEnemy{¿Enemigo
    Sin HP?}:::decision
    
    CheckHealthPlayer -->|Sí| GameOver((Game Over
    Eliminado)):::terminal
    CheckHealthPlayer -->|No| ModeBat
    
    CheckHealthEnemy -->|No| ModeBat
    CheckHealthEnemy -->|Sí| CheckWin{¿Es el último mago en pie?}:::decision
    
    CheckWin -->|No| LooBack[Regresa a Explorar]
    LooBack --> ModeExp
    CheckWin -->|Sí| Victory((¡Victoria del Torneo!)):::terminal