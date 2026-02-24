[ArtBible_01_ColorPalette]: Definir los códigos hexadecimales exactos para la paleta saturada y triádica, separando tonos para las zonas de peligro, magias y naturaleza.

[ArtBible_02_ShapeLanguage]: Ilustrar ejemplos visuales del acabado "Chunky" y las siluetas exageradas requeridas para estandarizar el estilo Toon-Comedico en el equipo.

[Concept_Char_MagoBase]: Abocetar la vista ortogonal frontal y lateral del personaje principal respetando la proporción de 3.5 a 4 cabezas de alto.

[Concept_Char_Variations]: Diseñar tres alternativas visuales de túnicas y sombreros que puedan acoplarse sobre la misma estructura base del avatar.

[Concept_Prop_LibroRunico]: Dibujar el arma principal en sus estados abierto y cerrado, asegurando que las ramas torcidas o el humor en el detalle sean notorios.

[Concept_Env_Monolitos]: Proponer los bocetos de las estructuras de progresión para los niveles 1, 2 y 3, escalando su impacto e indicando colores dorados o púrpuras.

[Concept_Env_BiomaBase]: Crear un arte conceptual del ecosistema general, incluyendo árboles mágicos y ruinas con verdes bosque y marrones saturados.

[Concept_Env_ArenaInicio]: Diseñar el punto céntrico de aparición del mapa abierto, destacándolo visualmente del resto de los elementos modulares del terreno.

[Concept_UI_IconosHechizo]: Ilustrar los identificadores visuales en 2D para las magias de fuego, hielo y rayo manteniendo el tono humorístico establecido.

[Concept_UI_HUD_Batalla]: Diseñar las barras de progreso de escritura priorizando la claridad de lectura en pantalla para la mecánica de combate.

[Block_Char_MagoBase]: Generar el modelo 3D primitivo del personaje en la pose T para verificar escalas directamente en el motor gráfico.

[Block_Prop_Libro]: Crear una malla simple de la herramienta de lanzamiento de hechizos para que los desarrolladores comiencen a anclarla a la cámara o manos.

[Block_Env_KitModular]: Construir los cubos y cilindros representativos para probar paredes inclinadas, vegetación simple y obstáculos del mundo.

[Block_Env_Monolitos]: Posicionar primitivas con las dimensiones correspondientes a los tres niveles rúnicos para testear rangos de interacción matemáticos.

[Block_Map_Layout]: Ensamblar el terreno de pruebas inicial en Unity utilizando los elementos modulares grises para definir rutas y fluidez de movimiento.

[Tech_Atlas_Template_Env]: Configurar el archivo base de 2048x2048 definiendo las áreas de color sólido para preparar la técnica de "Color Palette Mapping".

[QA_Art_Blocking_Review]: Realizar una sesión de validación cruzada con el equipo de desarrollo para confirmar que todas las escalas primitivas coinciden con la regla de 1 unidad igual a 1 metro.

[Sculpt_Char_MagoBase]: Iniciar el esculpido tridimensional del avatar enfocándose en manos y pies grandes para dar peso a las futuras animaciones.

[Sculpt_Env_PiedrasMagicas]: Modelar formaciones rocosas y ruinas asegurando bordes biselados marcados y evitando por completo las líneas rectas perfectas.

[Sculpt_Prop_Monolitos]: Detallar la geometría de las tres estructuras rúnicas garantizando que su silueta sea reconocible incluso bajo efectos de iluminación en sombra.

[Model_Char_MagoBase]: Reducir la geometría esculpida para cumplir con el límite estricto de menos de 2,000 triángulos, enfatizando siempre la silueta exagerada.

[Model_Char_Variations]: Crear la topología optimizada de las tres opciones de túnicas y sombreros intercambiables manteniendo el estilo semi-SD.

[Model_Env_KitModular]: Finalizar la malla de baja poligonización para piedras, árboles y ruinas, asegurando que no superen los 500 triángulos por objeto.

[Model_Env_ArenaInicio]: Refinar la geometría del centro del mapa abierto para su correcta integración escalar en el motor gráfico.

[Model_Prop_Monolitos]: Construir los tres niveles de estructuras rúnicas optimizadas con el pivote posicionado exactamente en la base del objeto.

[Model_Prop_LibroHechizos]: Generar la versión final del arma principal, manteniéndolo como un recurso independiente para su posterior vinculación a la mano del personaje.

[UV_Char_MagoBase]: Desplegar las coordenadas del personaje principal preparándolas para la técnica de mapeo por paleta de colores.

[UV_Env_KitModular]: Acomodar los mapas de las piezas del entorno para que compartan eficientemente el mismo espacio de textura sin superposiciones.

[Texture_Atlas_Environment]: Agrupar todos los elementos de la naturaleza y señales en un único archivo de 2048x2048 píxeles.

[Texture_Atlas_Characters]: Pintar los gradientes y colores sólidos para el mago y sus tres variantes cosméticas en un lienzo consolidado.

[Tech_Material_Setup]: Configurar en el motor gráfico un máximo de 2 a 3 materiales por escena completa utilizando exclusivamente los atlas generados.

[Rig_Char_MagoBase]: Construir el esqueleto articulado del avatar utilizando una nomenclatura clara en la jerarquía, como Bip_Hips o Bip_Hand_L.

[Rig_Prop_LibroHechizos]: Configurar los huesos de las tapas y páginas para permitir las transiciones mecánicas entre los estados abierto y cerrado.

[WeightPaint_Char_MagoBase]: Asignar la influencia de los vértices a los huesos para asegurar deformaciones correctas en las extremidades grandes y túnicas.

[Anim_Char_IdleLoop]: Crear un ciclo de espera perfecto y fluido para representar inactividad en la fase de movilidad libre.

[Anim_Char_RunLoop]: Animar la carrera del personaje destacando el tamaño de las extremidades para acentuar el tono caricaturesco y el movimiento exagerado.

[Anim_Char_CombatPose]: Diseñar la postura estática de lectura rúnica, garantizando que el modelo no bloquee la visión del jugador en la cámara de primera persona.

[VFX_Spell_Fuego]: Crear el sistema de partículas con colores naranjas intensos y rojos "ponche", optimizado para el prefabricado del motor.

[VFX_Spell_Hielo]: Diseñar los efectos visuales utilizando azules cian y blancos azulados, evitando estrictamente el blanco puro para mantener el estilo visual.

[QA_Art_Checklist_Models]: Aplicar transformaciones de escala a 1,1,1 y verificar que la nomenclatura de archivos cumpla con el estándar (ej. ART_Prop_Nombre_01) antes de la exportación final.

[VFX_Spell_Rayo]: Crear el efecto visual de partículas eléctricas  asegurando que los destellos no saturen la legibilidad de la pantalla durante el combate estático.

[VFX_Monolito_Unlock]: Diseñar la retroalimentación visual (partículas y brillo) que indicará al jugador que la estructura rúnica ha sido activada con éxito para cambiar de nivel.

[VFX_Daño_Impacto]: Construir el emisor de partículas que se instanciará sobre los enemigos al recibir un golpe exitoso de las magias elementales.

[UI_HUD_BarraProgreso]: Exportar los gráficos 2D para la barra de escritura, incluyendo sus estados vacío, llenado y retroalimentación visual de error.

[UI_HUD_SpellIcons]: Finalizar y exportar los íconos 2D de las magias (Fuego, Hielo, Rayo) respetando la paleta saturada  para el libro rúnico.

[UI_Menu_Assets]: Crear los recursos visuales, botones y tipografías para la pantalla de inicio y los menús del juego manteniendo el tono "Toon" cómico.

[UI_HUD_Crosshair]: Diseñar el retículo de apuntado y los marcadores de impacto garantizando alto contraste contra los fondos verdes bosque y marrones de la naturaleza.

[Env_Map_Dressing]: Ensamblar el terreno final en Unity sustituyendo los bloques del Graybox por el kit modular de piedras, árboles y ruinas terminado.

[Env_Lighting_Setup]: Configurar la iluminación global y direccional de la escena asegurando que las siluetas exageradas resalten correctamente bajo la técnica de Cell Shading.

[Anim_Prop_LibroApertura]: Animar la transición mecánica de las tapas y páginas del arma principal para cuando el jugador pase del modo Exploración al modo Batalla.

[Anim_Env_Vegetacion]: Configurar el movimiento sutil en los planos de las hojas y árboles utilizando Alpha clipping manejado posteriormente por los desarrolladores.

[Export_Char_FBX]: Generar los archivos finales del mago y sus variantes aplicando la escala 1,1,1 y el formato optimizado .fbx.

[Export_Env_FBX]: Exportar todo el kit modular de entorno y los monolitos asegurando que el pivote se encuentre estrictamente en la base de cada objeto.

[Export_Prop_FBX]: Entregar los modelos del libro rúnico y otros accesorios menores validando que la nomenclatura de entrega comience con ART_Prop_Nombre_01.

[QA_Art_Integration]: Revisar en conjunto con el Art Lead que todos los modelos implementados conserven sus mapeos UV referenciados únicamente a los Texture Atlas de 2048x2048 permitidos.

[Polish_Visual_Bugs]: Corregir las superposiciones de geometría y errores de renderizado detectados durante las pruebas locales previas a la entrega del Hito 3.

[Pitch_Render_Personajes]: Producir imágenes estáticas de alta calidad del mago base y sus variantes posando dinámicamente para la presentación del proyecto.

[Pitch_Render_Entorno]: Realizar capturas de pantalla estilizadas de la arena de inicio y los monolitos  para mostrar la calidad visual del mundo abierto en la carpeta de ventas.

[Pitch_Trailer_Capture]: Grabar el material de juego demostrando claramente la transición de la propuesta de valor entre la movilidad total y la mecánica de combate por escritura.

[Pitch_Trailer_Editing]: Editar el video promocional del MVP destacando la estética de "sábado por la mañana" y la jugabilidad única para la presentación final ante el Cuerpo Docente y cazatalentos.