flowchart TD

A[Inicio de Partida] --> B[Spawn Central]
B --> C{¿Explorar o Atacar?}

C -->|Explorar| D[Buscar Monolito Cercano N1/N2]
C -->|Atacar| E[Combate Temprano]

D --> F{¿Resolver Puzle / Derrotar Enemigos Básicos?}
F -->|Sí| G[Obtener Recursos / Vida / Stats]
F -->|No| H[Seguir Explorando]

G --> I{¿Ir a Monolito Lejano?}
H --> I

I -->|Sí| J[Explorar Zona Lejana N2/N3]
I -->|No| K[Buscar Área Segura]

J --> L[Conflicto con Otros Jugadores]
L --> M{¿Sobrevive?}

M -->|Sí| N[Fortalecido → Pelear por Victoria]
M -->|No| O[Respawn Inteligente]

O --> P{¿Monolito sin jugadores cerca?}
P -->|Sí| Q[Respawn cerca de Monolito libre]
P -->|No| R[Respawn cerca del Centro]

K --> S[Área Segura Intermedia]
S --> N
E --> M