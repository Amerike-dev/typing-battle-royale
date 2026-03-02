# GDD de Audio: Battle Royale Elemental

## 1. Sistema de Música
La banda sonora se gestionará mediante **Unity Events** que se comunican con un `MusicManager`. El sistema de música se divide en 4 etapas de partida, donde las primeras fases son ambientales y las finales son de alta tensión.

| Etapa | Tipo de Música | Comportamiento en Unity |
| :--- | :--- | :--- |
| **Etapa 1** | **Atmosférica I** | Texturas sonoras leves. Permite máxima concentración en el entorno. |
| **Etapa 2** | **Atmosférica II** | Evolución sutil de la anterior, añade una base rítmica muy tenue. |
| **Etapa 3** | **Intermedio** | Transición con percusión marcada. Eleva la tensión entre la calma y el final. |
| **Etapa 4** | **Clímax** | Música de alta tensión. Sonidos agudos y BPM acelerado (Partida por acabar). |

---

## 2. Mecánica de Typing (Feedback Sonoro)
El sistema de mecanografía debe ser altamente responsivo. Cada pulsación dispara un evento de audio 2D.

* **Tecla Correcta:** Sonido de "aprobación".
* **Tecla Incorrecta:** Sonido de "desaprobación".
* **Hechizo Completado:** Al terminar el hechizo, se dispara el **Sonido del Elemento** (Fuego, Agua, Tierra, Aire, etc.), confirmando que el hechizo ha sido lanzado con éxito.

---

## 3. Efectos de Sonido (SFX) y Entorno

### A. Monolitos (Eventos Globales)
Cuando un monolito aparece en el mapa, su sonido debe ser informativo para todos los jugadores:
* **Aparición Elemental:** Al salir, el monolito genera un sonido característico de su elemento que se escucha en **todo el mapa**.
* **Propósito:** Notificar a los jugadores la disponibilidad de nuevos hechizos sin necesidad de contacto visual.

### B. Hechizos
Los sonidos de combate están vinculados estrictamente a la naturaleza de su elemento:
* **Fuego:** Combustión y explosiones.
* **Agua:** Salpicaduras pesadas.
* **Tierra/Roca:** Impactos sordos, desmoronamientos y vibraciones de baja frecuencia.
* **Viento:** Sonidos cortantes ("whoosh").

---

## 4. Voces y Expresiones (Estilo "Clash Royale")
Se evita el uso de diálogos extensos para optimizar recursos y mantener un ritmo ágil.

* **Voz No Verbal:** Uso de onomatopeyas, gruñidos de esfuerzo y risas.
* **Frases Cortas:** Si se habla, serán términos de una sola palabra o frases icónicas muy breves (ej. "¡Fire!", "¡Whoosh!").
* **Esfuerzos:** Sonidos cortos para saltos, daño recibido (hit) y lanzamientos de hechizos de alto nivel.

---

## 5. Especificaciones Técnicas

### Mezcla y Espacialización (Audio Mixer)
1.  **Grupo UI/Typing (2D):** Sonidos que van directo a los auriculares del jugador.
2.  **Grupo Mundo (3D):** Hechizos de otros jugadores y ambiente.
3.  **Grupo Global (Monolitos):** Sonidos 3D que se escucharan en todo el mapa (Tiene que ayudar a indicar la dirección del monolito).

### Formatos Sugeridos
* **Música:** `.ogg` (Compresión eficiente para archivos largos).
* **SFX/Typing:** `.wav` (44.1kHz / 16-bit para evitar latencia en la respuesta del teclado).

### Nomenclatura de Archivos
* `MUS_Stage_[1-4]`
* `SFX_Type_Correct` / `SFX_Type_Error`
* `SFX_Spell_[Elemento]`
* `VO_Hero_[Accion]`