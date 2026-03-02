flowchart TD

A[Inicio de Partida] --> B[Spawn en Zona Propia]
B --> C{¿Recolectar o Invadir?}

C -->|Recolectar| D[Buscar Recursos en Zona]
C -->|Invadir| E[Ir a Puente]

D --> F{¿Ir a Monolito?}
F -->|Sí| G[Monolito N1/N2/N3]
F -->|No| H[Defender Zona]

E --> I[Cruzar Puente]
I --> J[Entrar a Zona Rival]

J --> K{¿Combate?}
K -->|Sí| L[Duelo]
K -->|No| M[Recolectar Recursos Enemigos]

L --> N{¿Muere?}
N -->|Sí| O[Respawn en Zona Inicial Alejada del Centro]
N -->|No| P[Control Territorial]

H --> Q[Rotación hacia Zona Adyacente]
G --> P
M --> P