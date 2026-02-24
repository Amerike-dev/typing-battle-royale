[Setup_01_Git_Repo]: Configurar el archivo .gitignore específico para Unity e inicializar las ramas main y develop para habilitar el trabajo simultáneo del equipo.

[Setup_02_URP_Config]: Asignar el perfil de renderizado Cell Shading en los Project Settings y establecer la resolución base de la compilación LAN.

[Setup_03_Layer_Tags]: Crear la capa "Interactables" y la etiqueta "Player" en el editor para que los sistemas de físicas futuras puedan hacer filtros correctos.

[Setup_04_Folder_Structure]: Replicar la jerarquía de directorios Core, Features, Scenes y Data dentro de Assets según la documentación técnica.

[GL_01_IGameState_Interface]: Declarar los métodos abstractos Enter, Execute y Exit en una estructura C# pura.

[GL_02_StateMachine_Core]: Implementar el motor de transiciones y el método Tick que gestionará el estado activo sin heredar de MonoBehaviour.

[GL_03_ExplorationState_POCO]: Crear la clase concreta del modo de movilidad dejando listos los callbacks para la inyección de dependencias de movimiento.

[GL_04_BattleState_POCO]: Estructurar la clase concreta del modo estático que se encargará de habilitar el input de texto y deshabilitar las físicas.

[GL_05_MatchSessionData]: Definir las variables estructurales para rastrear el ID de partida, temporizador global y la bandera de actividad del juego.

[GL_06_GameManager_Component]: Desarrollar el MonoBehaviour global que instanciará la máquina de estados en el Awake y le pasará el Update de Unity.

[GL_07_UIManager_Toggle]: Exponer las referencias públicas de los Canvas principales y crear métodos simples SetActive para alternar entre el HUD de exploración y el de combate.

[Mov_01_MovementData_POCO]: Establecer las propiedades flotantes de velocidad, gravedad, fuerza de salto y el vector tridimensional resultante.

[Mov_02_PlayerInputData_Struct]: Crear el contenedor de datos bidimensionales que almacenará la lectura cruda de los ejes WASD y la cámara.

[Mov_03_MovementLogic_Math]: Programar las operaciones de cálculo vectorial para el desplazamiento y la caída sin aplicar transformaciones directas al objeto.

[Mov_04_LockMovement_Logic]: Implementar el condicional interno que devuelva Vector3.zero en las operaciones matemáticas cuando la bandera de combate esté activa.

[Mov_05_PlayerPawnView_Input]: Configurar la captura del teclado tradicional en el Update y delegar la ejecución física al CharacterController.

[Mov_06_PlayerAnimatorView_Triggers]: Crear funciones públicas que reciban booleanos del estado de movimiento para pasarlos directamente al componente Animator.

[Mov_07_CastingPose_Event]: Conectar la transición hacia el modo de batalla con la ejecución del trigger de animación estática de lectura rúnica.

[Typ_01_SpellData_ScriptableObject]: Programar la clase base con atributos de menú para que soporte variables de tier, daño, prefabs de VFX y el string de runas a teclear.

[Typ_02_TypingStats_POCO]: Configurar los contadores enteros de aciertos, errores y pulsaciones totales, junto con las variables de tiempo.

[Typ_03_WPM_Calculation]: Implementar las fórmulas matemáticas que devuelvan las palabras por minuto y el porcentaje de precisión ortográfica basándose en las estadísticas.

[Typ_04_CombatLogic_Engine]: Construir el validador principal que compare el carácter ingresado contra la posición del índice actual del string objetivo.

[Typ_05_SpellCompletion_Math]: Desarrollar la función que evalúe si el índice de tipeo actual ha alcanzado la longitud total de la cadena de texto requerida.

[Typ_06_TypingInputView_Capture]: Usar el evento OnGUI de Unity para extraer el carácter crudo del sistema operativo e inyectarlo al validador ortográfico.

[Typ_07_TypingUIView_Feedback]: Exponer elementos TextMeshPro y de Image Fill para reflejar visualmente el avance del índice correcto o parpadear en rojo al fallar.

[Typ_08_DamageCalculator_POCO]: Construir la clase pura que reciba el daño base del SpellData y aplique los modificadores matemáticos finales.

[Typ_09_WPMBonus_Math]: Implementar la fórmula que otorgue un multiplicador de daño positivo si las palabras por minuto superan el umbral establecido.

[Typ_10_AccuracyPenalty_Math]: Programar la reducción porcentual del daño total si la precisión ortográfica de la transcripción cae por debajo del límite aceptable.

[Typ_11_SpellCasterView_VFX]: Codificar la instanciación del prefabricado visual asignado en la base de datos (fuego, hielo o rayo) utilizando el Transform del báculo.

[Typ_12_CombatResolution_Event]: Conectar la salida del cálculo de daño con el GameManager para aplicar el valor resultante a la entidad objetivo local.

[Env_01_MonolithData_POCO]: Definir la estructura de datos que almacenará el identificador único, el nivel del 1 al 3 y el texto del reto rúnico.

[Env_02_InteractionRequest_Struct]: Crear el contenedor que agrupe la posición del jugador, la posición del objetivo y las estadísticas actuales para pasarlo al validador.

[Env_03_InteractionController_Logic]: Establecer el cerebro puro del sistema que administrará las validaciones de distancia y los requisitos de nivel sin depender de Unity.

[Env_04_DistanceMath_Logic]: Programar el cálculo vectorial para verificar matemáticamente si el InteractionRequest se encuentra dentro del rango máximo permitido.

[Env_05_TierValidation_Logic]: Codificar la comprobación que asegure que el nivel actual del jugador es igual o superior al requisito de la estructura seleccionada.

[Env_06_PlayerInteractorView_Scan]: Implementar Physics.OverlapSphereNonAlloc filtrando por la capa específica para detectar objetos cercanos sin generar basura en memoria.

[Env_07_MonolithView_Component]: Exponer el identificador y nivel en el Inspector, además de gestionar la bandera local que marca la estructura como agotada.

[Env_08_InteractionPrompt_UI]: Vincular la detección de proximidad con la activación y desactivación del Canvas local que indica la tecla de acción.

[Env_09_Input_InteractKey]: Mapear la tecla de interacción en el Update del jugador para disparar el flujo de TryUnlockMonolith al presionarla cerca de un objetivo.

[Env_10_MonolithVFX_Trigger]: Crear el método público que reproduzca el sistema de partículas mágicas cuando el controlador confirme el éxito del desbloqueo.

[Stat_01_PlayerStats_POCO]: Establecer el contenedor de la salud actual, la salud máxima, el nivel de hechizos desbloqueados y el estado vital del mago.

[Stat_02_HealthMath_Logic]: Implementar las funciones de sustracción de daño y validación booleana para determinar si los puntos de vida han llegado a cero.

[Stat_03_SpellBook_Data]: Configurar una lista interna en memoria que guarde las referencias a los ScriptableObjects obtenidos durante la partida.

[Stat_04_TierUpgrade_Logic]: Escribir el método que incremente el nivel del jugador al superar con éxito el reto de escritura de una estructura de progresión.

[GL_08_StateTransition_Monolith]: Conectar la validación exitosa del InteractionController con la solicitud formal de cambio hacia el modo de Batalla estático.

[GL_09_StateTransition_Victory]: Enlazar la finalización del hechizo y la derrota del objetivo local para devolver el control al modo de Exploración.

[GL_10_MatchTimer_Logic]: Codificar la cuenta regresiva en milisegundos dentro del POCO de la sesión y exponer el tiempo restante de la partida.

[GL_11_MatchTimer_UI]: Vincular el UIManager para que lea la variable de tiempo del POCO y actualice el componente TextMeshProUGUI cada segundo.

[Net_01_NetworkManagerMock_Setup]: Crear el MonoBehaviour temporal que simulará la experiencia competitiva local optimizada.

[Net_02_MockDamageEvent_Log]: Implementar una función de simulación que imprima en consola el daño infligido y recibido antes de integrar la red real.

[Spw_01_SpawnPoint_Data]: Definir la estructura de coordenadas tridimensionales que representará las ubicaciones viables dentro de la arena de inicio.

[Spw_02_SpawnCalculator_Logic]: Implementar el algoritmo puro que seleccione aleatoriamente una posición óptima de la lista de nodos del mapa abierto.

[Spw_03_GameManager_Init]: Integrar el calculador de aparición al principio del ciclo de vida para posicionar al jugador base  antes de habilitar los controles.

[Cam_01_FirstPerson_Setup]: Configurar la cámara principal para la exploración en primera persona, garantizando que el modelo se siga viendo en el mundo.

[Cam_02_MouseLook_Input]: Mapear la lectura de los ejes X e Y del ratón dentro del contenedor de variables de control tridimensional.

[Cam_03_CameraMath_Logic]: Programar la rotación del transform horizontal en el eje Y y la rotación local vertical en el eje X con restricción de ángulos muertos.

[Cam_04_PawnView_Sync]: Conectar el cálculo matemático de visión con los componentes de Unity del avatar durante el modo de movilidad total.

[Cam_05_BattleFocus_Transition]: Bloquear la rotación de la cámara mediante la máquina de estados al entrar en el modo estático enfocado en escritura.

[Col_01_CharacterController_Fit]: Ajustar el radio y altura del colisionador cilíndrico asumiendo que una unidad equivale a un metro en Unity.

[Col_02_EnvironmentLayer_Mask]: Configurar la matriz de físicas global para que la entidad colisione adecuadamente con el kit modular de piedras y árboles.

[Col_03_Gravity_Math]: Refinar la detección de suelo mediante un raycast esférico originado en el pivote de la base del objeto  para aplicar la aceleración de caída.

[Net_03_PlayerID_Generation]: Asignar una cadena alfanumérica única al instanciar al usuario para preparar la arquitectura de red hacia la experiencia competitiva local.

[Env_11_MapBounds_Data]: Crear una clase estricta que contenga los vectores delimitadores mínimos y máximos del bioma de fantasía.

[Env_12_Boundary_Logic]: Implementar la validación matemática que compare la posición resultante del vector de movimiento contra los límites establecidos del mundo.

[Env_13_Monolith_Spawns]: Extender el calculador de apariciones para distribuir dinámicamente las estructuras de nivel 1 al 3 a lo largo del terreno jugable.

[Env_14_Graybox_Scene]: Ensamblar el nivel de prueba inicial utilizando primitivas para bloquear escalas de modelos base sin texturas.

[UI_01_Crosshair_Canvas]: Desarrollar el elemento central fijo de la pantalla para la fase de búsqueda de recursos que facilite apuntar a interactuables.

[UI_02_DamageFloat_Logic]: Programar el instanciador de texto emergente temporal al resolver las matemáticas de ataque tras finalizar el reto de escritura.

[UI_03_FloatingText_Anim]: Configurar la interpolación de opacidad a cero y desplazamiento vertical positivo para la retroalimentación del daño infligido.

[Stat_05_Death_Event]: Suscribir el administrador global a la validación de vida igual a cero para disparar el flujo de eliminación y desactivar el renderizado del avatar.

[Stat_06_KillCount_Tracker]: Agregar un sumador de enteros a las estadísticas de la partida al confirmar que los puntos de vida del enemigo llegaron a su límite.

[Stat_07_Victory_Condition]: Evaluar constantemente la cantidad de combatientes activos en el arreglo para disparar el evento de triunfo cuando el valor sea igual a uno.

[UI_04_EndMatch_Screen]: Construir el panel que consolide el daño infligido, recibido y la agilidad dactilar calculada  al concluir el ciclo de juego.

[Typ_13_SpellBook_UI]: Generar la cuadrícula dinámica en la interfaz que lea el arreglo de referencias de datos desbloqueadas y muestre los sprites correspondientes.

[Typ_14_SpellSelection_Input]: Mapear las teclas numéricas para inyectar el identificador del libro rúnico deseado al validador para iniciar la transcripción.

[Anim_01_IdleWalk_Blend]: Configurar el árbol de transiciones bidimensionales interpolando la variable de velocidad flotante calculada por el POCO de movimiento.

[Anim_02_Casting_Layer]: Establecer una máscara de avatar superior para el brazo derecho que permita sostener el libro rúnico independientemente de las físicas de las piernas.

[Anim_03_Death_Ragdoll]: Habilitar las físicas secundarias y apagar el componente de controlador de cápsula principal al recibir la confirmación de vida en cero.

[VFX_01_SpellImpact_Event]: Programar la detección de colisiones de partículas mágicas para instanciar marcas de quemadura o escarcha temporal sobre los materiales de las paredes.

[VFX_02_MonolithAura_Toggle]: Controlar el ciclo de vida de los emisores volumétricos apagándolos cuando la bandera local marque la estructura como ya utilizada o agotada.

[Aud_01_TypingSFX_Manager]: Implementar un conjunto de clips sonoros de máquina de escribir que varíen su afinación (pitch) aleatoriamente con cada pulsación ortográfica validada.

[Aud_02_MistakeBuzzer_Trigger]: Conectar la devolución falsa del validador de texto con la reproducción espacializada de un efecto sonoro de error penalizador.

[Aud_03_SpellCast_Audio]: Asignar un componente emisor tridimensional en la punta del báculo para reproducir el estallido correspondiente al prefabricado instanciado.

[Aud_04_Ambience_Loop]: Cargar y reproducir en bucle la pista estéreo de fondo del bioma de fantasía al iniciar el estado inicial de movilidad libre.

[Aud_05_BattleMusic_Crossfade]: Ejecutar la interpolación gradual de volumen bajando la pista pasiva y subiendo la tensión musical al ocurrir el cambio en la máquina de estados.

[UI_05_HitMarker_Canvas]: Renderizar un retículo de confirmación aspas rojas en el centro de la pantalla al registrar numéricamente daño exitoso sobre un oponente.

[UI_06_LowHealth_Vignette]: Modificar el canal alfa de un borde sangrante perimetral de forma inversamente proporcional a la fracción matemática de salud restante.

[UI_07_SpellCooldown_Fill]: Restringir visualmente la selección rúnica aplicando un sombreado oscuro radial sincronizado con el temporizador de enfriamiento.

[UI_08_Leaderboard_Mock]: Construir la tabla estática de final de partida que organice de mayor a menor los nombres de los competidores locales y sus eliminaciones totales.

[UI_09_MainMenu_Flow]: Programar la carga asíncrona de la escena de mundo abierto desde la pantalla de título inicial.

[Net_04_Transform_Sync]: Interpolar matemáticamente las posiciones de los avatares en clientes remotos para disfrazar el retraso natural de la conexión LAN.

[Net_05_AnimState_RPC]: Enviar comandos de red para asegurar que todos los competidores vean simultáneamente la transición hacia la pose estática de lectura.

[Net_06_Damage_Authority]: Centralizar la resta de los puntos de salud exclusivamente en la computadora anfitriona (Host) para evitar desincronizaciones entre instancias.

[Opt_01_TextureAtlas_Check]: Desarrollar un script utilitario en el editor que detecte y alerte si algún modelo importado incumple la regla de usar el mapa de 2048x2048.

[Opt_02_VFX_Pooling]: Crear un administrador de memoria que recicle los proyectiles mágicos inactivos en lugar de instanciarlos y destruirlos constantemente.

[Core_01_GameLoop_Reset]: Limpiar todas las variables estáticas y reiniciar las colecciones para permitir rondas consecutivas sin necesidad de cerrar el ejecutable.

[Demo_01_CheatCodes_Input]: Habilitar combinaciones de teclas de administrador para otorgar invulnerabilidad o nivel tres inmediato para facilitar la presentación a profesores.

[Demo_02_Build_Settings]: Fijar la resolución, bloquear la sincronización vertical y desactivar los mensajes de depuración en consola para compilar el ejecutable final optimizado.

[QA_01_Typing_EdgeCases]: Añadir filtros al capturador de eventos para ignorar silenciosamente bloqueos de mayúsculas (Caps Lock), tabulaciones y dobles espacios accidentales.

[QA_02_Feature_Freeze]: Bloquear la rama principal de código a modificaciones de mecánicas, limitando los commits estrictamente a la eliminación de bugs visuales y matemáticos críticos.