# SPIKE Técnico – Evaluación de Soluciones de Red LAN en Unity

## 1. Introducción
El presente documento tiene como objetivo evaluar la viabilidad de utilizar distintas soluciones de networking en Unity para un videojuego tipo Battle Royale de magia con un máximo de 4 jugadores en entorno LAN (Local Area Network).
El proyecto requiere:
* Comunicación local (LAN)
* Baja latencia
* Alta estabilidad
* Modelo Host-Server (un jugador actúa como servidor y cliente)
Implementación adecuada para principiantes

No es necesario soporte para servidores dedicados en la nube ni conexiones vía Internet (WAN).
Se analizarán las siguientes opciones:
* Netcode for GameObjects (NGO) – Solución oficial de Unity
* Mirror – Librería open source heredera de UNET
* Fish-Net – Alternativa moderna optimizada


## 2. Evaluación de Unity Netcode for GameObjects (NGO)
### 2.1 ¿Requiere Internet obligatoriamente?
No necesariamente.

Netcode for GameObjects puede funcionar en LAN utilizando el transporte Unity 

Transport sin necesidad de Unity Relay.

Sin embargo:
* Muchos ejemplos oficiales integran Relay y Lobby.
* La documentación mezcla escenarios LAN e Internet.
* Puede generar confusión en principiantes.


### 2.2 ¿Puede funcionar sin Relay?
Sí. NGO puede funcionar de manera local utilizando IP directa.
No es obligatorio usar Unity Relay para conexiones LAN.

Sin embargo:

Unity promueve el uso de sus servicios (UGS).

El flujo natural en tutoriales suele incluir autenticación y Relay.

## 3. Evaluación de Mirror
### 3.1 Descripción General
Mirror es una librería open source basada en UNET.

Es muy utilizada en proyectos LAN y educativos.
### 3.2 ¿Requiere Internet?
No.

Funciona completamente en LAN utilizando IP directa.

No depende de servicios externos.
### 3.3 Soporte para LAN Discovery
Sí.

Incluye componente NetworkDiscovery que permite:

* Buscar automáticamente hosts en la red local.
* Evitar que el usuario tenga que escribir IP manualmente.
* Esto es una gran ventaja para principiantes.

## 4. Evaluación de Fish-Net
### 4.1 Descripción General

Fish-Net es una solución moderna con enfoque en:
* Alto rendimiento
* Sincronización optimizada
* Arquitecturas más avanzadas

### 4.2 ¿Requiere Internet?
No.

Puede funcionar perfectamente en LAN.

### 4.3 Soporte para LAN Discovery

Sí, pero requiere configuración adicional.

No es tan inmediato como Mirror.
## 5. Matriz de Decisión

| Criterio | NGO | Mirror |Fish-Net |
|--------------|--------------|--------------|---------------|
|Dificultad de configuración|Media| Baja |Media-Alta|
|Dependencia de Internet|No obligatoria, pero integrada con servicios|No|No|
|Soporte LAN Discovery|No integrado directamente|Sí (NetworkDiscovery)|Sí (requiere configuración)|
|Estabilidad para 4 jugadores|Alta|Muy Alta|Muy Alta|
|Curva de aprendizaje|Media|Baja|Media|

## 6. Requisito Host-Server

Las tres librerías permiten el modelo Host-Server.

Es decir:

* Un jugador puede iniciar como Host.
* Actúa como servidor y cliente simultáneamente.
* No requiere servidor dedicado externo.

Confirmación:

* NGO: Permite StartHost()
* Mirror: Permite StartHost()
* Fish-Net: Permite modo Host

Todas cumplen este requisito.

## 7. Diagrama de Flujo – Creación y Unión en LAN
Flujo básico de funcionamiento:

* Jugador A selecciona “Crear Partida”.
* El sistema ejecuta StartHost().
* Se abre servidor local en su máquina.


* Jugadores B, C y D seleccionan “Buscar Partida”.
* Sistema detecta host en red local (Broadcast / Discovery).
* Los jugadores se conectan automáticamente.
* Se sincroniza escena y comienza partida.


Flujo simplificado:

* Jugador A
 ->Crear partida
 ->Se convierte en Host
* Jugadores B, C, D
 -> Buscar partida
 -> Detectan IP local
 -> Se conectan
 -> Inicia partida
## 8. Recomendación Final
Para un proyecto:
* 4 jugadores
* Entorno LAN
* Enfoque educativo
* Estudiantes principiantes
* Prioridad en estabilidad y simplicidad

La opción más adecuada es:

**Mirror**

**Justificación:** 

* No depende de servicios externos.
* Tiene LAN Discovery integrado.
* Es extremadamente estable para 4 jugadores.
* Arquitectura sencilla.
* Gran cantidad de tutoriales.
* Menor curva de aprendizaje.
* Flujo clásico Host-Server fácil de entender.

NGO es una solución moderna y oficial, pero está más orientada a arquitecturas online e integración con Unity Services.

Fish-Net es muy potente, pero su enfoque en optimización lo hace más adecuado para proyectos más complejos o con mayor número de jugadores.

Para este caso específico, Mirror ofrece el mejor equilibrio entre:
* Simplicidad
* Estabilidad
* Claridad conceptual
* Facilidad de implementación

## 9. Conclusión
**Unity Netcode for GameObjects es viable en LAN, pero puede resultar más complejo para estudiantes principiantes debido a su integración con Unity Gaming Services.
Fish-Net ofrece alto rendimiento, pero puede ser excesivo para un entorno de 4 jugadores LAN.
Mirror representa la solución más práctica, estable y accesible para este proyecto académico.
Se recomienda su implementación como librería de networking principal para el Battle Royale de magia en entorno LAN.**
