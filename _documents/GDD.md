# 🧙‍♂️ Keyboard Battle Royale (MVP)
# Visión General 

Keyboard Battle Royale es un videojuego desarrollado en Unity 3D de género Battle Royale. WotK busca fusionar la exploración en primera persona con una mecánica de combate única basada en la escritura mecánica (typing-combat). El proyecto se concibe como un Producto Mínimo Viable (MVP) con el objetivo de brindar una nueva jugabilidad e innovar en el mercado actual. El fin del proyecto es ser presentado ante distribuidoras de la industria, empezando por el Demo Day en Amerike CDMX. 

Esta documentación está diseñada para alinear al equipo multidisciplinario (Desarrolladores y Artistas) durante los Sprints de producción, asegurando una arquitectura escalable, optimizada para multijugador LAN y un flujo de trabajo sin cuellos de botella.

# La Propuesta de Valor

A diferencia de los shooters convencionales la habilidad competitiva del juego se pasa a la agilidad dactilar y precisión ortográfica. Los jugadores asumen el rol de aprendices de mago en un torneo de fantasía medieval con un tono "Toon" cómico, donde el éxito depende de qué tan rápido y exacto puedan transcribir hechizos de sus libros rúnicos mientras están bajo fuego enemigo.

### 🗺️ Diagramas Globales

Consulta estos documentos para entender el panorama completo del juego antes de adentrarte en módulos específicos:

* [🧩 Diagrama de Clases General (MVC + Estados)](./Core/ClassDiagram.md)
* [🔄 Diagrama de Flujo (Game Loop y Mecánica Dual)](./Core/Flowchart.md)
* [📊 Modelo Entidad-Relación (Estadísticas Locales de Partida)](./Core/EntityRelationship.md)

---

# Componentes Clave del MVP 

- Mecánica Dual: Alternancia entre el modo Exploración (movilidad total para búsqueda de recursos) y modo Batalla (estático, enfoque en escritura y estrategia). 
- Sistema de Monolitos: Estructuras de progresión en el mapa que requieren retos de escritura para desbloquear hechizos de mayor poder. 
- Multijugador LAN: Experiencia competitiva local optimizada para estabilidad en entornos de prueba. 
- Estética Optimizada: Estilo Low Poly con técnica de Cell Shading, utilizando Texture Atlas para garantizar un alto rendimiento y tiempos de carga mínimos.

## ⚙️ Módulos Principales (Core)

La lógica del juego está dividida en 4 sistemas modulares aislados. Cada directorio contiene su propio diagrama de clases de alta resolución, diagrama de secuencia (flujo temporal) y mapa de integración para la jerarquía de Unity.

### 1. 🪨 Environment & Interaction

Gestiona el sistema de monolitos, rangos de detección de colisiones optimizadas (OverlapSphere) y el desbloqueo de niveles de hechizos mediante requisitos de tier.
* [Diagrama de Clases](./Core/Environment_and_Interaction/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Environment_and_Interaction/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Environment_and_Interaction/IntegrationMap.md)

### 2. 🔁 Game Loop & State Machine

Administra el ciclo de vida de la partida, el reloj global y las transiciones limpias entre el modo de movilidad total (Exploración) y el modo estático (Batalla) inyectando el `Update` de Unity hacia estados POCO puros.
* [Diagrama de Clases](./Core/GameLoop_and_StateMachine/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/GameLoop_and_StateMachine/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/GameLoop_and_StateMachine/IntegrationMap.md)

### 3. 🏃 Player & Movement

Contiene las matemáticas vectoriales para la física y el movimiento en primera persona, el procesamiento de inputs y la comunicación con el `Animator` del mago, respetando los bloqueos dictados por la máquina de estados.
* [Diagrama de Clases](./Core/Player_and_Movement/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Player_and_Movement/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Player_and_Movement/IntegrationMap.md)

### 4. ⚔️ Typing Combat System

El motor de validación ortográfica puro. Se encarga de capturar las pulsaciones de teclas (`Event.current.character`), calcular el progreso, las palabras por minuto (WPM), la precisión y el multiplicador de daño sin depender de la carga del motor gráfico.
* [Diagrama de Clases](./Core/TypingCombat_and_System/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/TypingCombat_and_System/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/TypingCombat_and_System/IntegrationMap.md)

---

## 🎨 Game Design y Base de Datos (Scriptable Objects)

Para consultar las reglas de balance, la paleta de colores, la biblia de arte Toon-Comedico y la guía de integración de *Texture Atlases* para los artistas, dirígete al directorio de Game Design:

* [📖 Documentación de Game Design y Arte](./GameDesign/)

### 🗄️ Base de Datos Inicial (Hechizos)

Los datos de los hechizos (Daño, Textos a teclear, Niveles, Iconos y Prefabs de VFX) se gestionan a través de `ScriptableObjects`. Esto permite al equipo de arte y diseño de niveles crear e iterar sobre el contenido directamente desde el Inspector de Unity sin modificar código. 
*(Nota: La base de datos no requiere persistencia inicial y arranca en blanco; solo registra datos estadísticos en memoria durante la simulación de la partida).*

## Art Desing

### El Concepto Visual: "Caos Mágico Caricaturesco" 

El estilo se define como Toon-Comedico. No buscamos realismo ni fantasía épica oscura. 
Buscamos la estética de un "sábado por la mañana": proporciones exageradas, colores vibrantes y formas que sugieran humor. 
- Pilar 1: Siluetas Exageradas. Si un objeto es importante, su forma debe ser reconocible incluso en sombras. 
- Pilar 2: Claridad de Lectura. Dado que es un Battle Royale, el jugador debe distinguir un mago de un árbol a 50 metros de distancia. 
- Pilar 3: Humor en el Detalle. Los báculos pueden ser ramas torcidas, los sombreros pueden estar remendados o ser demasiado grandes.

### Paleta de Color y Lenguaje Visual 

Utilizaremos una paleta Saturada y Triádica para generar contraste. 
- Magia de Fuego: Naranjas intensos y rojos "ponche". 
- Magia de Hielo: Azules cian y blancos azulados (evitar el blanco puro). 
- Entorno (Naturaleza): Verdes bosque y marrones saturados. 
- Zonas de Peligro/Monolitos: Púrpuras mágicos y dorados para indicar importancia. 

Regla de Oro: El color debe guiar al jugador. Los elementos interactuables 
(Monolitos) deben tener el valor de saturación más alto de la escena. 

### Guía de Modelado (Low Poly & Shapes) 

Buscamos un acabado "Chunky" (robusto). 
- Personajes: 
    - Proporción de 3.5 a 4 cabezas de alto (estilo semi-SD o Stylized). 
    - Manos y pies grandes para enfatizar las animaciones de escritura y movimiento. 
    - Importante: El libro de hechizos es un asset independiente pero vinculado al rig de la mano. 
- Entorno: 
    - Evitar líneas rectas perfectas. Los muros deben estar ligeramente inclinados, las piedras deben tener bordes biselados marcados. 
    - Uso de planos para vegetación simple (Alpha clipping manejado por devs).


### Referencias Visuales Sugeridas 

- Juegos: Clash Royale, Brawl Stars, Magicka (por el tono). 
- Estética: The Legend of Zelda: Wind Waker (por la limpieza de formas).