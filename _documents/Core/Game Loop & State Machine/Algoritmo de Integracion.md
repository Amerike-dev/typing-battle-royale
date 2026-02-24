```
🎮 ESCENA: SCN_MainMap_Graybox
│
├── ⚙️ [Core_Systems] (GameObject vacío - Ideal para DontDestroyOnLoad)
│   │
│   ├── GameManager.cs (Componente MonoBehaviour)
│   │   ▼ Variables Expuestas en el Inspector:
│   │   ├── Match Duration Secs: 600.0 (float) // Tiempo total de la partida
│   │   ├── UI Manager Ref: [Arrastrar componente UIManager] (UIManager)
│   │   // 💡 Nota Interna: StateMachine y MatchSessionData (POCOs) NO se 
│   │   // exponen aquí. Se instancian dinámicamente en el Awake() del GameManager.
│   │
│   └── UIManager.cs (Componente MonoBehaviour)
│       ▼ Variables Expuestas en el Inspector:
│       ├── Exploration HUD Ref: [Arrastrar 🧭 HUD_Exploration] (GameObject)
│       ├── Battle HUD Ref: [Arrastrar ⚔️ HUD_Battle] (GameObject)
│       └── Match Timer Text: [Arrastrar ⏱️ Txt_Timer] (TextMeshProUGUI)
│
└── 🖼️ UI_MainCanvas (Canvas Global)
    │
    ├── 🧭 HUD_Exploration (GameObject / Panel - Activo por defecto)
    │   ├── Reticula_Centro (Image)
    │   └── Indicadores_Movimiento (UI Elements)
    │
    └── ⚔️ HUD_Battle (GameObject / Panel - Desactivado por defecto)
        ├── 📖 Contenedor_LibroHechizos (UI Layout)
        ├── ⏱️ Txt_Timer (TextMeshProUGUI - Reloj global de la partida)
        └── [Scripts de UI específicos del Typing System irán aquí]
```

## Relación Código-Editor

Inicialización de POCOs: En el método Awake() del GameManager.cs, el desarrollador debe instanciar los datos puros:

    currentMatch = new MatchSessionData { MatchID = "LAN_001", MatchTimer = matchDurationSecs, IsMatchActive = true };

    gameStateMachine = new StateMachine();

    gameStateMachine.Initialize(new ExplorationState());

Gestión de UI: El UIManager.cs debe ser un componente sumamente "tonto" (Dumb View). Sus métodos ShowExplorationHUD() y ShowBattleHUD() simplemente hacen un .SetActive(true/false) sobre las referencias de los paneles expuestos en el Inspector. Quien dicta cuándo llamarlos son los estados ExplorationState y BattleState durante sus respectivos métodos Enter().

Transiciones Limpias: Dado que el equipo trabajará en Sprints cortos de 2 semanas, mantener esta separación permite que un desarrollador programe las reglas del BattleState (POCO) sin preocuparse por romper las referencias visuales en el Inspector, ya que de eso se encarga exclusivamente el UIManager.

## Glosario
POCOs => Clases puras de C#
