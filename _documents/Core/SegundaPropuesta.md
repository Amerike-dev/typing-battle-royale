flowchart TD

A[Inicio de Partida] --> B[Spawn Cercano a Monolitos]
B --> C{¿Ir a Monolito?}

C -->|Sí| D[Conflicto Inmediato]
C -->|No| E[Explorar Brevemente]

D --> F{Nivel Disponible}
F -->|Nivel 1| G[Captura Monolito N1]
F -->|Nivel 2 Liberado| H[Captura Monolito N2]
F -->|Nivel 3 Liberado| I[Captura Monolito N3]

G --> J[Zona Safe Typing Cercana]
H --> J
I --> J

J --> K[Regresar al Combate]

D --> L{¿Muere?}
L -->|Sí| M[Respawn en Zona con Menos Jugadores]
L -->|No| N[Continúa Combate]

M --> C
K --> D
N --> D