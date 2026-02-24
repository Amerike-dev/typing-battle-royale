## Arquitectura
```
🎮 ESCENA: SCN_MainMap_Graybox
│
├── ⚙️ [Sistema_Global] (GameObject vacío)
│   └── GameStateManager.cs (Componente MonoBehaviour)
│       // Administra los estados globales (Exploración/Batalla)
│
├── 🧙‍♂️ Player_Wizard (GameObject) | Tag: "Player"
│   ├── Animator (Componente para animaciones del Mago)
│   ├── CharacterController o Rigidbody (Componente de físicas)
│   └── PlayerInteractorView.cs (Componente MonoBehaviour)
│       ▼ Variables Expuestas en el Inspector:
│       ├── Scan Radius: 5.0 (float) 
│       └── Interactable Layer: "Interactables" (LayerMask desplegable)
│       // 💡 Nota Interna: InteractionController (POCO) NO se ve en el 
│       // Inspector. Se instancia por código dentro de PlayerInteractorView.
│
└── 🪨 ART_Prop_MonolitoNvl1_01 (Prefab) | Layer: "Interactables"
    ├── MeshFilter (Modelo 3D del monolito)
    ├── MeshRenderer (Material asignado: Atlas_Environment_2048)
    ├── SphereCollider (IsTrigger = true, define el área física de detección)
    ├── MonolithView.cs (Componente MonoBehaviour)
    │   ▼ Variables Expuestas en el Inspector:
    │   ├── Mon
```

## Relación Código-Editor

Gestión de POCOs: El script PlayerInteractorView.cs debe instanciar InteractionController = new InteractionController(5.0f) en su método Awake(). Las clases como InteractionRequest o MonolithData se crean "al vuelo" (on the fly) en memoria cada vez que el jugador entra en el área del SphereCollider del Monolito.

Capas (Layers): Es crucial crear una Layer específica en Unity llamada "Interactables". El Prefab del monolito debe asignarse a esta capa. De esta forma, el OverlapSphere del jugador solo calculará físicas contra los monolitos, evitando sobrecargar el procesador calculando colisiones contra el suelo o árboles.

Referencias Visuales: El componente MonolithView.cs debe tener referencias serializadas ([SerializeField]) hacia su hijo UI_PromptCanvas y VFX_SpawnPoint para poder encenderlos y apagarlos según la distancia validada por el 
InteractionController.


## Glosario
POCOs => Clases puras de C#
