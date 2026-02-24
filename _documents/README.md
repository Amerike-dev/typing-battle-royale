# 🧙‍♂️ Wizards of the Keyboard (MVP) - Documentación Técnica

Esta documentación está diseñada para alinear al equipo multidisciplinario (Desarrolladores y Artistas) durante los Sprints de producción, asegurando una arquitectura escalable, optimizada para multijugador LAN y un flujo de trabajo sin cuellos de botella.

---

## 🏛️ Arquitectura General

El proyecto utiliza una combinación del patrón **MVC (Model-View-Controller)** para desacoplar la lógica de los componentes visuales de Unity, y el **Patrón de Estados (State Machine)** para gestionar la "Mecánica Dual" (Exploración vs. Batalla). Se prioriza el uso de clases POCO (Plain Old C# Objects) para las lógicas complejas, manteniendo los `MonoBehaviours` lo más ligeros posible.

### 🗺️ Diagramas Globales
Consulta estos documentos para entender el panorama completo del juego antes de adentrarte en módulos específicos:

* [🧩 Diagrama de Clases General (MVC + Estados)](./ClassDiagram.md)
* [🔄 Diagrama de Flujo (Game Loop y Mecánica Dual)](./Flowchart.md)
* [📊 Modelo Entidad-Relación (Estadísticas Locales de Partida)](./EntityRelationship.md)

---

## ⚙️ Módulos Principales (Core)

La lógica del juego está dividida en 4 sistemas modulares aislados. Cada directorio contiene su propio diagrama de clases de alta resolución, diagrama de secuencia (flujo temporal) y mapa de integración para la jerarquía de Unity.

### 1. 🪨 Environment & Interaction
Gestiona el sistema de monolitos, rangos de detección de colisiones optimizadas (OverlapSphere) y el desbloqueo de niveles de hechizos mediante requisitos de tier.
* [Diagrama de Clases](./Core/Environment%20&%20Interaction/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Environment%20&%20Interaction/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Environment%20&%20Interaction/IntegrationMap.md)

### 2. 🔁 Game Loop & State Machine
Administra el ciclo de vida de la partida, el reloj global y las transiciones limpias entre el modo de movilidad total (Exploración) y el modo estático (Batalla) inyectando el `Update` de Unity hacia estados POCO puros.
* [Diagrama de Clases](./Core/Game%20Loop%20&%20State%20Machine/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Game%20Loop%20&%20State%20Machine/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Game%20Loop%20&%20State%20Machine/IntegrationMap.md)

### 3. 🏃 Player & Movement
Contiene las matemáticas vectoriales para la física y el movimiento en primera persona, el procesamiento de inputs y la comunicación con el `Animator` del mago, respetando los bloqueos dictados por la máquina de estados.
* [Diagrama de Clases](./Core/Player%20&%20Movement/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Player%20&%20Movement/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Player%20&%20Movement/IntegrationMap.md)

### 4. ⚔️ Typing Combat System
El motor de validación ortográfica puro. Se encarga de capturar las pulsaciones de teclas (`Event.current.character`), calcular el progreso, las palabras por minuto (WPM), la precisión y el multiplicador de daño sin depender de la carga del motor gráfico.
* [Diagrama de Clases](./Core/Typing%20Combat%20System/ClassDiagram_HighRes.md)
* [Diagrama de Secuencia](./Core/Typing%20Combat%20System/SequenceDiagram.md)
* [Mapa de Integración en Unity](./Core/Typing%20Combat%20System/IntegrationMap.md)

---

## 🎨 Game Design y Base de Datos (Scriptable Objects)

Para consultar las reglas de balance, la paleta de colores, la biblia de arte Toon-Comedico y la guía de integración de *Texture Atlases* para los artistas, dirígete al directorio de Game Design:

* [📖 Documentación de Game Design y Arte](./GameDesign/)

### 🗄️ Base de Datos Inicial (Hechizos)
Los datos de los hechizos (Daño, Textos a teclear, Niveles, Iconos y Prefabs de VFX) se gestionan a través de `ScriptableObjects`. Esto permite al equipo de arte y diseño de niveles crear e iterar sobre el contenido directamente desde el Inspector de Unity sin modificar código. 
*(Nota: La base de datos no requiere persistencia inicial y arranca en blanco; solo registra datos estadísticos en memoria durante la simulación de la partida).*

---

## 🛠️ Buenas Prácticas de Desarrollo
1. **POCO First:** Cualquier cálculo pesado (daño, validación de texto, vectores) debe vivir en una clase de C# puro.
2. **Dumb Views:** Los `MonoBehaviours` y scripts de UI deben limitarse a capturar inputs, renderizar partículas o encender/apagar objetos (Setters visuales). No deben tomar decisiones lógicas.
3. **No Material Sprawl:** Para mantener el rendimiento óptimo del MVP multijugador, usar siempre los Atlas de texturas asignados. No crear materiales individuales por prop.
