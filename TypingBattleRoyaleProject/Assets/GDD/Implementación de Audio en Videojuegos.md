# Introducción

El audio en este juego se asignará en todo el juego, es decir, que todos los jugadores podrán escuchar la misma música en todo momento. Esto significa que no podremos tener audio 3D aparte. Esto nos permite que los jugadores identifiquen acciones importantes como ataques, hechizos, errores de escritura o la cercanía de enemigos, incluso cuando no están visibles en pantalla.

---

# Middleware vs Sistema de Audio Nativo

## Middleware

### Ventajas
Se encuentra a disposición el control de la mezcla de audio sin modificar código, así como el manejo automático de prioridades de sonido.

### Desventajas
Curva de aprendizaje demasiado alta.  
Requiere integración adicional al motor, lo cual puede resultar excesivo para juegos pequeños.

---

## Sistema de Audio Nativo de Unity

### Ventajas
Facilidad de uso.  
Integración directa con el motor y bajo impacto en el rendimiento.

### Desventajas
Limitación en cuestiones de automatización.

---

# Audio Espacial y Posicionamiento 3D

## Audio Espacial en Pantalla Dividida

En un juego con pantalla dividida, todos los jugadores comparten una sola salida de audio. Por esta única razón, el posicionamiento sonoro se calculará desde un único punto de escucha global.

---

# Mezcla Dinámica y Priorización

## Mezcla Dinámica

La mezcla dinámica permitirá ajustar automáticamente el volumen de ciertos sonidos cuando ocurre un evento importante, ya sea cuando se obtenga la recompensa de un monolito o cuando empiece a ocurrir una batalla durante el combate, al igual que cuando se lancen hechizos.

---

## Priorización

Debido a la cantidad de sonidos que pueden reproducirse al mismo tiempo, es necesario definir qué sonidos son más importantes que otros.

Los sonidos prioritarios para el gameplay serían los hechizos, los cuales tendrían la más alta prioridad, así como las activaciones de monolitos, el momento de recibir dicha recompensa, cuando se derrota a un enemigo y cuando te encuentras con uno.

---

# Optimización de Memoria, Formatos y Conclusión

Unity ofrece distintos modos de carga de audio que permiten balancear calidad y rendimiento.

Sonidos tipo OneShot se cargan descomprimidos para evitar ciertos retrasos.  
Sonidos de duración media tipo Play se guardan en la RAM.  
La música de la partida, al ser demasiado larga o estar en bucle, se reproduce por streaming para evitar un consumo de memoria elevado.

---

## Recomendación

Usar archivos WAV para efectos cortos.  
Archivos OGG para hechizos, interfaz y música.