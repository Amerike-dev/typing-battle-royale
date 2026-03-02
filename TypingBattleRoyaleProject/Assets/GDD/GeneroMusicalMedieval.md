# Spike: Genero Musical Medieval

## 1. Análisis de Referencias (Benchmarking)

Se analizaron tres juegos con enfoques distintos en el manejo de la magia y la competitividad para definir nuestro estándar:

| Juego | Estilo Musical | Uso de la Magia en el Audio | Manejo de la Tensión |
| :--- | :--- | :--- | :--- |
| **Hogwarts Legacy** | Orquesta | Sonidos "mágicos" clásicos (campanas, vientos madera). | Cambios suaves entre exploración y duelos de varitas. |
| **League of Legends** | Épico | Temas definidos por región/personaje. Uso de metales para poder. | La música reacciona a eventos globales (Baron, Dragón) y al final de la partida. |
| **Hades** | Rock Progresivo | Mezcla de instrumentos antiguos con guitarras eléctricas saturadas. | El BPM aumenta según la dificultad del encuentro. Es frenético y constante. |

---

## 2. Mapeo de Identidad Elemental

Para que el jugador "sienta" el elemento antes de verlo, definimos las siguientes texturas e instrumentos:

### Matriz Elemental
| Elemento | Textura Sonora | Instrumento Característico | Efecto de Sonido (SFX) Sugerido |
| :--- | :--- | :--- | :--- |
| **Fuego** | Agresiva, saturada | Sintetizadores / Guitarras distorsionadas | Llamas constantes y explosiones de baja frecuencia. |
| **Agua** | Fluida, resonante | Arpa / Pads de cristal / Campanas | Sonido de burbujeo, succión y ecos largos. |
| **Tierra** | Pesada, orgánica | Percusión / Maderas pesadas | Impactos secos de rocas y crujidos de suelo. |
| **Aire** | Volátil, ligera | Flautas sopladas con fuerza / Violín agudo | Silbidos de viento y ruido blanco. |

---

## 3. Estructura de Intensidad (Pacing)

La partida de 10 minutos la podemos dividir en estados para guiar la psicología del jugador:

1. **Fase Inicial:** *Tensión Creciente.* Música atmosférica que permita escuchar pasos y la activación de monolitos.
2. **Exploración:** *Identidad Elemental.* La música varía según la zona, manteniendo una intensidad media.
3. **Combate:** *Acción Dinámica.* Cambio inmediato a un ritmo de mayor BPM al iniciar un duelo. Se introducen capas de percusión agresiva.
4. **Clímax:** *Urgencia Extrema.* Se suman coros y sonidos metálicos. La música se vuelve "asfixiante" para presionar el cierre de la partida.

---

## 4. Evaluación Técnica

**Propuesta:** Implementación mediante Middleware (FMOD o Wwise).
* **Justificación:** Unity Audio nativo no gestiona de forma eficiente las transiciones por compases. Con un middleware, podemos evitar que el cambio de "Exploración" a "Combate" haga cortes bruscos.

---

## 5. Criterios de Aceptación (Entregables)

### Audio Moodboard
* **Playlist de Referencia:** [[LINK DE YOUTUBE](https://www.youtube.com/playlist?list=PLg9ZbveVa_nYFA4WZSP0BQX3XIXiNuDRi)]

### Documento de Definición
**Género Elegido:** *Mistico / Acción*.
Una base de orquesta épica (estilo LoL/Hogwarts) inyectada con la agresividad rítmica de Hades.

### Propuesta de Implementación
* **Eventos:** La música cambiara con base al tiempo y acciones del jugador, por lo que se puede tener eventos para manejar estos cambios.