```
🎮 ESCENA: SCN_MainMap_Graybox
│
├── 🧙‍♂️ Player_Wizard (GameObject) | Tag: "Player"
│   │
│   ├── TypingInputView.cs (Componente MonoBehaviour)
│   │   ▼ Variables Expuestas en el Inspector:
│   │   ├── UI View Ref: [Arrastrar componente TypingUIView del Canvas]
│   │   ├── Caster View Ref: [Arrastrar componente SpellCasterView de este objeto]
│   │   // 💡 Nota Interna: TypingCombatLogic y DamageCalculator (POCOs) NO 
│   │   // aparecen aquí. Se instancian dinámicamente al presionar un botón 
│   │   // para iniciar un hechizo.
│   │
│   ├── SpellCasterView.cs (Componente MonoBehaviour)
│   │   ▼ Variables Expuestas en el Inspector:
│   │   └── Cast Point: [Arrastrar Transform hijo 🪄 Punta_Baculo]
│   │
│   └── 🎨 ART_Char_MagoBase_01 (GameObject Hijo)
│       └── 🪄 Punta_Baculo (Transform vacío)
│           // Punto de origen donde se instanciarán los efectos de partículas 
│           // (hechizos de fuego, hielo, rayo).
│
└── 🖼️ UI_MainCanvas (Canvas Global)
    └── ⚔️ HUD_Battle (GameObject / Panel)
        └── 📖 Panel_TypingUI (GameObject)
            ├── TypingUIView.cs (Componente MonoBehaviour)
            │   ▼ Variables Expuestas en el Inspector:
            │   ├── Spell Text UI: [Arrastrar 📝 Txt_Runas_Target] (TextMeshProUGUI)
            │   ├── Progress Bar: [Arrastrar 📊 Img_BarraEscritura] (Image - Fill)
            │   └── Mistake Feedback Color: #FF0000 (Color)
            │
            ├── 📝 Txt_Runas_Target (TextMeshProUGUI)
            │   // Muestra el string de SpellData.RunesToType que el jugador debe teclear.
            │
            └── 📊 Img_BarraEscritura (Image)
                // Componente UI Image con Image Type configurado en "Filled" (Horizontal).
                // Recibirá el float de GetCompletionPercentage() de la lógica POCO.
```

## Relación Código-Editor

Desacoplamiento Visual: Al separar TypingUIView.cs en el Canvas, el equipo de 8 artistas digitales puede iterar sobre el diseño de la barra de progreso de escritura y los iconos de hechizos  sin tocar la lógica del jugador ni generar conflictos en el control de versiones (Git).

Event.current.character: En TypingInputView.cs, la captura de texto debe hacerse preferentemente usando el evento OnGUI() para interceptar caracteres reales (considerando mayúsculas, minúsculas o símbolos si los hechizos lo requieren) en lugar de mapear cada tecla individualmente en el nuevo Input System.

Instanciación de VFX: Cuando el TypingCombatLogic confirma que IsSpellComplete() es true, el SpellCasterView.cs toma el GameObject VfxPrefab almacenado en el SpellData y lo instancia usando las coordenadas del Cast Point. Esto permite que los artistas creen decenas de hechizos visualmente distintos y solo tengan que arrastrarlos a la configuración de datos.
