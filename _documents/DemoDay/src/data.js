// =====================================================================
// Typing Battle Royale — Demo Day backlog (datos)
// Solo datos estáticos. Render y lógica viven en app.js.
// =====================================================================

const TEAM = [
    { id: 'Flan',    name: 'Flan',    color: '#8b5cf6' }, // violeta
    { id: 'Jorge',   name: 'Jorge',   color: '#3b82f6' }, // azul
    { id: 'Ches',    name: 'Ches',    color: '#22c55e' }, // verde
    { id: 'Alicia',  name: 'Alicia',  color: '#f97316' }, // naranja
    { id: 'Juan',    name: 'Juan',    color: '#ec4899' }, // rosa
    { id: 'Barrera', name: 'Barrera', color: '#06b6d4' }, // cian
    { id: 'Angel',   name: 'Angel',   color: '#eab308' }, // amarillo
    { id: 'Banyo',   name: 'Banyo',   color: '#14b8a6' }  // teal
];

const TICKETS = [
    // ─── Pedidos directos (TBR-001 a TBR-009) ───
    {
        id: 'TBR-001',
        title: 'Completar menú de pausa funcional',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Ches', status: 'done',
        summary:
            'PauseController abre el panel y pausa con Time.timeScale, pero faltan: SFX mute, sliders de volumen vinculados, botón Reanudar funcional, botón "Volver al menú" que apunte a una escena válida (hoy carga "MainMenu" que no está en build), y bloquear la apertura cuando GameOverState esté activo.',
        acceptance: [
            'Tecla ESC abre/cierra el menú con Time.timeScale 0/1.',
            'Botón Reanudar restaura timeScale y oculta el panel.',
            'Botón Volver al Menú carga LobbyScene (escena que sí está en build).',
            'Sliders de volumen Master/Music/SFX persistentes via PlayerPrefs.',
            'No abre menú durante GameOverState ni durante WaitingState.',
            'En multijugador la pausa es local (no afecta a los otros).'
        ],
        files: [
            'Assets/Scripts/Controllers/PauseController.cs',
            'Assets/Scripts/Controllers/VolumeController.cs',
            'Assets/Core/AudioManager.cs',
            'Assets/Scripts/SceneLoader.cs'
        ],
        deps: ['TBR-013', 'TBR-020']
    },
    {
        id: 'TBR-002',
        title: 'Spell Book selector dentro de BattleState',
        type: 'feature', priority: 'high', effort: 'L',
        assignee: 'Banyo', status: 'done',
        summary:
            'Cuando el jugador entra a BattleState, mostrar la lista de hechizos del PlayerInventory (separada por tier). Al elegir uno, asignar el Spell SO a CastInputController.currentSpell y spellText = Spell.runeString para que el typing sea sobre ese spell. Si el inventario está vacío, fallback a default spell (TBR-044).',
        acceptance: [
            'Al entrar a BattleState aparece SpellBookUI con los hechizos desbloqueados.',
            'Filtrado por tier según playerTier (gris para tiers superiores, amarillo seleccionado).',
            'Mouse wheel o teclas pageUp/pageDown navegan páginas (3 slots/pág).',
            'Al confirmar selección: CastInputController.currentSpell = Spell, spellText = Spell.runeString.',
            'Si PlayerInventory está vacío, usa el default de TBR-044 (Bola de Fuego).',
            'Cancelar selección regresa a ExplorationState sin penalización.',
            'PlayerInventory.GetSpellsByTier alimenta la UI.'
        ],
        files: [
            'Assets/Scripts/Controllers/SpellBookUI.cs',
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs',
            'Assets/Scripts/Models/PlayerInventory.cs',
            'Assets/ScriptableObjects/Scripts/Spell.cs'
        ],
        deps: ['TBR-004', 'TBR-044']
    },
    {
        id: 'TBR-003',
        title: 'Auto-lock de enemigos cercanos en BattleState',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Flan', status: 'done',
        summary:
            'TargetSystem.cs hoy es stub (FindClosestTarget y ToggleLockOn vacíos). Implementar selección automática del enemigo más cercano dentro de un radio configurable cuando se entra a BattleState, con indicador visual sobre el target (anillo/flecha) y persistencia mientras dure el casteo. Sin manual cycling (Tab toggles battle mode).',
        acceptance: [
            'Al entrar a BattleState, FindClosestTarget retorna el NetworkPlayer vivo más cercano dentro del radio.',
            'Indicador visual flotante sobre el target (similar a EnemyLabel pero con icono de lock).',
            'Si el target muere o sale del radio, retarget automático al siguiente vivo en rango.',
            'En ExplorationState el lock se desactiva.',
            'Excluye al jugador local (OwnerClientId) y a jugadores con PlayerStatsNet.isAlive=false.',
            'Si no hay targets vivos en rango: target=null y el spell se reproduce sin daño (feedback "sin objetivo").'
        ],
        files: [
            'Assets/Scripts/Controllers/TargetSystem.cs',
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs',
            'Assets/Features/Player_and_Movement/EnemyLabel.cs',
            'Assets/Scripts/LAN/PlayerStatsNet.cs'
        ],
        deps: ['TBR-005', 'TBR-043']
    },
    {
        id: 'TBR-004',
        title: 'Animaciones de cast + integración VFX-daño (la red de VFX ya existe)',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Flan', status: 'todo',
        summary:
            'La red de VFX ya está implementada (SpellNetworkController con ServerRpc + ClientRpc + SpellCatalog + ProjectileVFX). Falta: (1) animación de cast del Animator del player cuando se dispare el evento, (2) feedback de "sin objetivo en rango" cuando target=null, (3) integración con accuracy multiplier — ver TBR-048 para el daño server-side y cooldown. Este ticket cubre solo la capa visual/animación cliente.',
        acceptance: [
            'Animator del player dispara trigger "Casting" cuando CastInputController.OnSpellCast se dispara.',
            'PlayerAnimatorView lee el archetype del Spell y elige animación distinta para Projectile vs AOE vs Aura vs Beam.',
            'Si TargetSystem.target == null al castear: VFX se reproduce pero con tinta gris/feedback "sin objetivo".',
            'El VFX local del caster (cast en mano) se reproduce vía SpellCatalog.Get + Bind antes de la trayectoria del proyectil.',
            'Cuando el proyectil impacta (TBR-048), se reproduce VFX_Hit en el target.',
            'Validado: 4 jugadores casteando simultáneamente sin frame drops perceptibles.'
        ],
        files: [
            'Assets/Scripts/VFX/SpellNetworkController.cs',
            'Assets/Scripts/VFX/SpellCatalog.cs',
            'Assets/Scripts/VFX/SpellVFXBinder.cs',
            'Assets/Scripts/VFX/ProjectileVFX.cs',
            'Assets/Features/Player_and_Movement/PlayerAnimatorView.cs',
            'Assets/Scripts/Controllers/TargetSystem.cs',
            'Assets/ScriptableObjects/Scripts/Spell.cs'
        ],
        deps: ['TBR-003', 'TBR-011', 'TBR-048']
    },
    {
        id: 'TBR-005',
        title: 'Muerte por barra de vida agotada',
        type: 'feature', priority: 'critical', effort: 'M',
        assignee: 'Jorge', status: 'done',
        summary:
            'PlayerStats expone TakeDamage y LoseLife pero no hay loop que detecte HP <= 0 → restar vida → respawn. Hoy isAlive nunca cambia a false en flujo real. Implementar la cadena: damage → check HP → si HP<=0 invocar OnAllLifeLost (si lives==0) o OnLifeLost + respawn.',
        acceptance: [
            'PlayerStats.TakeDamage decrementa currentHP y dispara OnDamageTaken.',
            'Si currentHP ≤ 0 y currentLifes > 0 → LoseLife() resta una vida y dispara OnLifeLost (luego respawn vía TBR-008).',
            'Si currentLifes = 0 → OnAllLifeLost → activa modo espectador (TBR-009).',
            'isAlive refleja estado real.',
            'Sincronizado por NetworkVariable en NetworkPlayerController.',
            'HUDController actualiza barra y vidas vía eventos.'
        ],
        files: [
            'Assets/Data/PlayerStats.cs',
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Scripts/LAN/NetworkPlayerController.cs',
            'Assets/Scripts/Controllers/RespawnController.cs'
        ],
        deps: ['TBR-011']
    },
    {
        id: 'TBR-006',
        title: 'Mover barra de vida del prefab al Canvas UI global',
        type: 'tech', priority: 'medium', effort: 'S',
        assignee: 'Ches', status: 'done',
        summary:
            'El prefab del player tiene un Canvas mundial con su barra de vida. Eso causa duplicación, problemas de orden de render y trabajo extra de NetworkObject. Quitarlo del prefab y dejar la HUD del jugador local en un único Canvas screen-space del HUDController.',
        acceptance: [
            'Player prefabs ya no contienen Canvas hijo.',
            'HUDController es el único responsable de mostrar HP/vidas del jugador local.',
            'Para jugadores remotos solo se muestra EnemyLabel (nombre/ID), sin barra de HP flotante.',
            'No queda referencia muerta en NetworkPlayerController al Canvas eliminado.'
        ],
        files: [
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Scripts/LAN/NetworkPlayerController.cs',
            'Assets/Features/Player_and_Movement/EnemyLabel.cs'
        ],
        deps: ['TBR-005']
    },
    {
        id: 'TBR-007',
        title: 'Bug: los jugadores no se ven entre sí en red',
        type: 'bug', priority: 'critical', effort: 'M',
        assignee: "Jorge", status: 'done',
        summary:
            'En partida real cada cliente solo ve a su propio personaje. Tres causas confirmadas en código: (1) GameplayManager.cs:118 usa OwnerClientId == 0 que evalúa true en todos los clientes; (2) PlayerController (MonoBehaviour de escena) mueve un transform que no es el del NetworkObject spawneado; (3) posible falta de NetworkTransform en el prefab.',
        acceptance: [
            'Cada cliente ve a los 4 jugadores moviéndose con sus respectivos skins/colores.',
            'El prefab spawneado tiene NetworkObject + NetworkTransform configurados.',
            'NetworkObject.SpawnWithOwnership(clientId) se invoca solo en el server (Host).',
            'Ningún Renderer/MeshRoot se desactiva por IsOwner.',
            'Validado con 2, 3 y 4 clientes en red real (no NetworkManagerMock).'
        ],
        files: [
            'Assets/Scripts/Controllers/GameplayManager.cs',
            'Assets/Scripts/LAN/NetworkPlayerController.cs',
            'Assets/Scripts/LAN/NetworkManagerConfigurator.cs',
            'Assets/ScriptableObjects/Scripts/SkinInfo.cs'
        ],
        deps: ['TBR-017']
    },
    {
        id: 'TBR-008',
        title: 'UI de muerte + cámara espectadora del asesino + respawn por vidas',
        type: 'feature', priority: 'high', effort: 'L',
        assignee: 'Flan', status: 'todo',
        summary:
            'Al morir mostrar overlay de muerte (con cuenta regresiva al respawn y vidas restantes), cámara sigue al jugador que mató al local hasta que el contador termine, luego respawn en spawn point libre. Si no quedan vidas, deriva a TBR-009.',
        acceptance: [
            'OnLifeLost dispara DeathUI overlay con texto "Te eliminó {killerName} — Respawn en {N}s".',
            'Cámara hace ParentOverride al killer durante la cuenta regresiva (3-5s configurable).',
            'Player invisible y sin colisión durante la espera.',
            'Al expirar el countdown: respawn en spawn point libre, HP al 100 %, cámara vuelve al player.',
            'Si killer muere durante la espera, transición a otro player vivo.',
            'Sincronizado por red: el server decide spawn point y comunica killer.'
        ],
        files: [
            'Assets/Scripts/Controllers/RespawnController.cs',
            'Assets/Scripts/Controllers/CameraController.cs',
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Data/PlayerStats.cs'
        ],
        deps: ['TBR-005']
    },
    {
        id: 'TBR-009',
        title: 'Modo espectador al perder todas las vidas',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Flan', status: 'done',
        summary:
            'Cuando un jugador agota sus vidas (OnAllLifeLost) entra en modo espectador: no respawnea, su NetworkObject queda invisible/sin colisión, la cámara cicla entre los jugadores aún vivos con teclas izquierda/derecha. Sale del modo cuando termina la partida (TriggerGameOver).',
        acceptance: [
            'OnAllLifeLost desactiva PlayerController y CharacterController del jugador.',
            'UI espectador con instrucción "← →: cambiar jugador".',
            'Cámara hace follow al jugador vivo seleccionado (sin override de input).',
            'Si el espectador-target muere, salta automáticamente al siguiente vivo.',
            'GameplayManager.TriggerGameOver se dispara cuando ConnectedClientsList.Where(IsAlive).Count == 1 ó timer expira.',
            'En GameOverState, los espectadores también ven EndGameUI.'
        ],
        files: [
            'Assets/Scripts/Controllers/CameraController.cs',
            'Assets/Scripts/Controllers/PlayerController.cs',
            'Assets/Scripts/Controllers/GameplayManager.cs',
            'Assets/Data/PlayerStats.cs'
        ],
        deps: ['TBR-005', 'TBR-012']
    },

    // ─── Adicionales detectados en el análisis (TBR-010 a TBR-020) ───
    {
        id: 'TBR-010',
        title: 'Migrar GameOverState fuera de NetworkManagerMock',
        type: 'bug', priority: 'critical', effort: 'S',
        assignee: 'Ches', status: 'done',
        summary:
            'GameOverState.cs:21,28,38 lee NetworkManagerMock.Instance.Controllers, que no existe en partida con red real (Netcode no expone .Controllers). Provoca NullReferenceException al terminar la partida. Migrar a NetworkManager.Singleton.ConnectedClientsList y consultar PlayerStats vía componente.',
        acceptance: [
            'Toda referencia a NetworkManagerMock fuera de Debug.unity eliminada.',
            'GameOverState recolecta List<PlayerStats> iterando NetworkManager.Singleton.ConnectedClientsList y leyendo el componente del NetworkObject del jugador.',
            'EndGameUI.Populate funciona correctamente con red real probada con 4 clientes.',
            'NetworkManagerMock queda restringido a la escena Debug.unity (o eliminado por completo).'
        ],
        files: [
            'Assets/Scripts/Models/GameOverState.cs',
            'Assets/Features/Network/NetworkManagerMock.cs',
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-011',
        title: 'Sincronizar PlayerStats por red (HP, vidas, kills)',
        type: 'tech', priority: 'critical', effort: 'L',
        assignee: 'Flan', status: 'done',
        summary:
            'PlayerStats hoy es POCO local: TakeDamage en un cliente no llega al resto. Convertir HP/vidas/kills a NetworkVariable<float>/<int> en NetworkPlayerController y exponer ServerRpc TakeDamageServerRpc(damage, attackerId).',
        acceptance: [
            'NetworkVariable<float> currentHP / NetworkVariable<int> currentLifes / NetworkVariable<int> killCount con permisos write-server.',
            'TakeDamageServerRpc valida en el server, modifica NetworkVariable y dispara los Action de PlayerStats vía OnValueChanged.',
            'killCount se incrementa en el servidor al detectar HP del target ≤ 0 con attackerId.',
            'HUDController sigue mostrando los valores locales al suscribirse a OnValueChanged.',
            'Funciona consistentemente con 4 clientes (test de pelea cruzada).'
        ],
        files: [
            'Assets/Data/PlayerStats.cs',
            'Assets/Scripts/LAN/NetworkPlayerController.cs',
            'Assets/Scripts/Controllers/HUDController.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-012',
        title: 'Detección "último vivo" → TriggerGameOver',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Jorge', status: 'done',
        summary:
            'Falta la lógica que cierra la partida cuando solo queda un jugador con vidas. Suscribirse a OnAllLifeLost desde GameplayManager y comprobar el contador de vivos.',
        acceptance: [
            'Server suscribe OnAllLifeLost de cada PlayerStats.',
            'Cuando el conteo de jugadores con isAlive=true llega a 1 → TriggerGameOver(survivor.ID).',
            'Si el timer del InGameTimer llega a 0 antes, gana quien tenga más kills (desempate por HP, luego por WPM promedio).',
            'TriggerGameOver se invoca solo en server y replica con ClientRpc.'
        ],
        files: [
            'Assets/Scripts/Controllers/GameplayManager.cs',
            'Assets/Scripts/Models/GameOverState.cs',
            'Assets/Features/Gameloop_and_StateMachine/InGameTimer.cs'
        ],
        deps: ['TBR-011']
    },
    {
        id: 'TBR-013',
        title: 'Implementar AudioManager (música y SFX)',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Barrera', status: 'done',

        summary:
            'AudioManager, MusicController y AudioSettings están como stubs. Implementar PlaySFX(name) usando AudioDataBase, ChangeMusic con crossfade en MusicController, persistir volúmenes con Save/Load y conectar VolumeController.',
        acceptance: [
            'AudioManager.PlaySFX(name) toma AudioEntry del AudioDataBase y reproduce en sfxPool.',
            'AudioManager.ChangeMusic(name) hace crossfade de 0.5 s entre clips.',
            'AudioSettings.Save/Load persisten volúmenes Master/Music/SFX en PlayerPrefs.',
            'VolumeController liga sliders del menú a SetVolume.',
            'Eventos clave disparan SFX: OnSpellCast, OnDamageTaken, OnLifeLost, scene transitions.'
        ],
        files: [
            'Assets/Core/AudioManager.cs',
            'Assets/Scripts/Controllers/MusicController.cs',
            'Assets/Scripts/AudioSettings.cs',
            'Assets/Scripts/Controllers/VolumeController.cs',
            'Assets/ScriptableObjects/Scripts/AudioDataBase.cs',
            'Assets/ScriptableObjects/Scripts/AudioEntry.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-014',
        title: 'Lobby — capacidad configurable (4–8) + lista en vivo',
        type: 'feature', priority: 'low', effort: 'M',
        assignee: 'Ches', status: 'done',
        summary:
            'LobbyController acepta cualquier cantidad de clientes. Ahora se quiere probar hasta 8 jugadores. Hacer la capacidad configurable (campo serializado, default 4, max 8) y mostrar lista en vivo de jugadores conectados con su nombre/ID. NO bloquear hard a 4: validar el límite contra el campo configurable.',
        acceptance: [
            'Campo serializado int maxPlayers (default 4) en LobbyController, validable hasta 8.',
            'NetworkManager.ConnectionApprovalCallback rechaza al conectado N+1.',
            'LobbyUIController muestra lista en vivo de jugadores conectados.',
            'Botón "Empezar partida" deshabilitado si conectados < 2.',
            'Botón visible solo para Host.',
            'Mensaje al rechazado refleja el límite real ("Sala llena (4/4)" / "Sala llena (8/8)").'
        ],
        files: [
            'Assets/Scripts/LAN/LobbyController.cs',
            'Assets/Scripts/LAN/LobbyUIController.cs',
            'Assets/Scripts/LAN/NetworkManagerConfigurator.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-015',
        title: 'Manejo de desconexión durante partida',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Jorge', status: 'done',
        summary:
            'Hoy si un cliente se desconecta el server no lo sabe limpiamente: su prefab queda vivo y puede romper la condición de "último vivo". Manejar OnClientDisconnectCallback para despawnear, marcar isAlive=false y reevaluar fin de partida.',
        acceptance: [
            'NetworkManager.OnClientDisconnectCallback elimina el NetworkObject del jugador desconectado.',
            'Su PlayerStats se marca como isAlive=false y dispara OnAllLifeLost.',
            'Si solo queda 1 jugador conectado → TriggerGameOver.',
            'EndGameUI muestra "Desconectado" en vez de stats si aplica.',
            'No se puede reconectar a la partida en curso (entra como espectador o se rechaza).'
        ],
        files: [
            'Assets/Scripts/LAN/NetworkManagerConfigurator.cs',
            'Assets/Scripts/Controllers/GameplayManager.cs'
        ],
        deps: ['TBR-012']
    },
    {
        id: 'TBR-016',
        title: 'Sincronizar Monolitos por red',
        type: 'tech', priority: 'medium', effort: 'M',
        assignee: 'Ches', status: 'done',
        summary:
            'MonolithSpawn instancia GameObjects con Instantiate plano: cada cliente ve sus propios monolitos no sincronizados. Convertir prefab Monolith a NetworkObject y spawnear desde el Host con NetworkObject.Spawn(). TryInteract debe ejecutarse vía ServerRpc.',
        acceptance: [
            'Prefab Monolith tiene NetworkObject (+ NetworkTransform si se mueven).',
            'MonolithSpawn corre solo en server y usa NetworkObject.Spawn().',
            'TryInteract es ServerRpc, valida rango y reparte el spell al solicitante.',
            'Estado "exhausto" se sincroniza vía NetworkVariable<bool>.',
            'Cooldown / respawn de monolitos configurable y sincronizado.'
        ],
        files: [
            'Assets/Core/MonolithSpawn.cs',
            'Assets/Features/Environment_and_Interaction/MonolithController.cs',
            'Assets/Features/Environment_and_Interaction/MonolithView.cs',
            'Assets/Features/Environment_and_Interaction/PlayerInteractorView.cs'
        ],
        deps: ['TBR-007']
    },
    {
        id: 'TBR-017',
        title: 'Sustituir OwnerClientId==0 por IsHost y eliminar WaitForSeconds(5f)',
        type: 'tech', priority: 'medium', effort: 'S',
        assignee: "Jorge", status: 'done',
        summary:
            'GameplayManager.PopulateSpawnPoint usa OwnerClientId == 0 como check de Host (frágil) y hace que los clientes esperen 5 s ciegos. Reemplazar por NetworkManager.Singleton.IsHost / IsServer y sincronizar el spawn por evento o ClientRpc.',
        acceptance: [
            'No quedan checks tipo "OwnerClientId == 0" para distinguir Host.',
            'Cliente no usa WaitForSeconds ciego: espera un evento (OnSpawnComplete ClientRpc) o la propia sincronización del NetworkObject.',
            'Confirmado que con red de alta latencia (>1s) los clientes siguen viendo todos los spawns.'
        ],
        files: [
            'Assets/Scripts/Controllers/GameplayManager.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-018',
        title: 'Spawn points como Transforms en escena (no hardcoded)',
        type: 'tech', priority: 'low', effort: 'S',
        assignee: "Jorge", status: 'done',
        summary:
            'Los 4 spawn points están escritos como Vector3 literales en código. Convertir a array de Transforms serializados en GameplayManager para que el level designer los mueva sin tocar código.',
        acceptance: [
            'Campo serializado Transform[] spawnPoints en GameplayManager.',
            'PopulateSpawnPoint usa esos Transforms.',
            'Mínimo 4 spawn points; warning en consola si hay menos jugadores que puntos disponibles.',
            'No queda ningún Vector3 literal de spawn en el .cs.'
        ],
        files: [
            'Assets/Scripts/Controllers/GameplayManager.cs',
            'Assets/Features/Network/SpawnCalculator.cs'
        ],
        deps: ['TBR-017']
    },
    {
        id: 'TBR-019',
        title: 'Limpieza: escenas y código huérfano',
        type: 'tech', priority: 'low', effort: 'S',
        assignee: 'Jorge', status: 'done',
        summary:
            'Eliminar/archivar 12 escenas no referenciadas y stubs nunca llamados (SpellCaster, partes de TargetSystem si TBR-003 las reescribe, MusicController si se reemplaza por TBR-013).',
        acceptance: [
            'Las escenas no necesarias están movidas a una carpeta `_archive/` o eliminadas.',
            'No queda ningún script con todas sus funciones vacías que ya no se planee implementar.',
            'EditorBuildSettings limpio.',
            'git log conserva la historia (no se hace force-push para eliminar).'
        ],
        files: [
            'Assets/Scenes/',
            'Assets/Scripts/Controllers/SpellCaster.cs',
            'Assets/Features/Network/NetworkManagerMock.cs'
        ],
        deps: ['TBR-002', 'TBR-003', 'TBR-010', 'TBR-013']
    },
    {
        id: 'TBR-020',
        title: 'Decidir destino del botón "Volver al Menú"',
        type: 'tech', priority: 'medium', effort: 'S',
        assignee: 'Ches', status: 'done',
        summary:
            'EndGameUI y PauseController cargan "MainMenu" por nombre, pero MainMenu.unity no está en build → fallará. Opción A: meter MainMenu en build y reactivar el flujo previo. Opción B (recomendada): redirigir a LobbyScene como menú principal.',
        acceptance: [
            'Decisión escrita en Specs.md sobre A vs B.',
            'Si A: MainMenu.unity en EditorBuildSettings y MainMenuController revisado para iniciar StartHost previo.',
            'Si B: EndGameUI y PauseController cargan "LobbyScene" en su lugar y se elimina la referencia "MainMenu".',
            'Test: terminar una partida y pulsar "Volver al menú" no genera error en build de Windows.'
        ],
        files: [
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs',
            'Assets/Scripts/Controllers/PauseController.cs',
            'Assets/Scripts/Controllers/MainMenuController.cs',
            'ProjectSettings/EditorBuildSettings.asset'
        ],
        deps: []
    },

    // ─── Pulido para pitch demo (TBR-021 a TBR-036) ───
    {
        id: 'TBR-021',
        title: 'Integrar AudioManager en todos los eventos del juego',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Barrera', status: 'todo',
        summary:
            'Una vez que AudioManager esté implementado (TBR-013), conectar PlaySFX/ChangeMusic a todos los eventos: cast de hechizo, hit de daño, pérdida de vida, jump, footsteps, monolith unlock, countdown, transiciones de escena, hover/click de UI. Música distinta por escena con crossfade.',
        acceptance: [
            'Cada evento crítico dispara AudioManager.PlaySFX con el ID correcto.',
            'Cambiar de escena hace ChangeMusic con crossfade de 0.5 s.',
            'Volúmenes globales (Master/Music/SFX) afectan todo.',
            'Sin debug.log de "audio missing" en consola tras una partida completa.',
            'No se cortan SFX al solaparse (sfxPool dimensionado).'
        ],
        files: [
            'Assets/Core/AudioManager.cs',
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Data/PlayerStats.cs',
            'Assets/Scripts/SceneLoader.cs'
        ],
        deps: ['TBR-013']
    },
    {
        id: 'TBR-022',
        title: 'Biblioteca de SFX completa (importación + cataloging)',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Barrera', status: 'todo',
        summary:
            'Importar y catalogar en AudioDataBase los clips para todas las acciones. Mínimo 25 clips únicos.',
        acceptance: [
            'AudioDataBase contiene ≥25 AudioEntry distintos.',
            'Cada SpellData tiene asignado su SFX cast/hit por elemento.',
            'Convención de nombres consistente (ej. SFX_Spell_Fire_Cast).',
            'Loudness normalizado entre clips (no diferencias chocantes).',
            'Documentación de los IDs en un README dentro de la carpeta de audio.'
        ],
        files: [
            'Assets/ScriptableObjects/Scripts/AudioDataBase.cs',
            'Assets/ScriptableObjects/Scripts/AudioEntry.cs',
            'Assets/Audio/'
        ],
        deps: ['TBR-013']
    },
    {
        id: 'TBR-023',
        title: 'VFX para los ~50 hechizos (arquitectura de arquetipos + tiers)',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Alicia', status: 'todo',
        summary:
            'La base ya está hecha: Spell.cs con tier/archetype/runeString, SpellVFXBinder con multiplicadores T1=1x/T2=1.4x/T3=2x sobre size/emission, ProjectileVFX local determinista, VFX_Projectile.prefab + M_Fire_T1.mat creados via VFXPrefabBuilder editor tool. Ahora hay que: (1) crear los 5 arquetipos restantes (AOE/Aura/Beam/Summon/Buff-Debuff) via tool (TBR-045), (2) un material por elemento (TBR-046), (3) mapear cada Spell SO a su archetype + asignar materialVFX correcto (TBR-047). Este ticket es el "umbrella" que se completa cuando TBR-045/046/047 estén done.',
        acceptance: [
            'Los 6 arquetipos VFX existen como prefabs: VFX_Projectile (hecho), VFX_AOE, VFX_Aura, VFX_Beam, VFX_Summon, VFX_BuffDebuff.',
            'Existe un material por elemento bajo Assets/Materials/VFX/: M_Fire, M_Water, M_Earth, M_Air, M_Ice, M_Lava, M_Lightning, M_Nature, M_Light, M_Darkness.',
            'Cada uno de los ~50 Spell SOs tiene asignados: tier, archetype, runeString, materialVFX, damage, range, speed.',
            'Reproducción sincronizada via SpellNetworkController.ClientRpc (sin daño, daño en TBR-048).',
            'PoolManager registra entradas para los 6 archetypes con size=20 cada uno.',
            '60 fps con 4 jugadores casteando simultáneamente (validar contra TBR-036).'
        ],
        files: [
            'Assets/Scripts/VFX/SpellVFXBinder.cs',
            'Assets/Scripts/VFX/ProjectileVFX.cs',
            'Assets/Scripts/VFX/SpellCatalog.cs',
            'Assets/Editor/VFX/VFXPrefabBuilder.cs',
            'Assets/Prefabs/VFX/',
            'Assets/Materials/VFX/',
            'Assets/ScriptableObjects/Objects/Spells/'
        ],
        deps: ['TBR-045', 'TBR-046', 'TBR-047']
    },
    {
        id: 'TBR-024',
        title: 'Cell-shading shader pass (estilo toon)',
        type: 'feature', priority: 'medium', effort: 'L',
        assignee: 'Juan', status: 'done',
        summary:
            'Implementar shader cell-shading custom o vía package (URP Toon Shader). Aplicar a personajes, monolitos y entorno. Outline por post-process o Geometry pass. Validar performance vs PBR estándar.',
        acceptance: [
            'Materiales toon aplicados a player skins, monolitos y props del mapa.',
            'Outlines visibles y consistentes (post-process Sobel o inverted hull).',
            'Soporta luces direccionales y point light al menos.',
            '60fps en hardware target con 4 jugadores en pantalla.',
            'Look documentado con screenshots before/after.'
        ],
        files: [
            'Assets/Shaders/',
            'Assets/Materials/',
            'Assets/Settings/'
        ],
        deps: []
    },
    {
        id: 'TBR-025',
        title: 'Integración de modelos y rigs reales (skins finales)',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Juan', status: 'todo',
        summary:
            'Reemplazar prefabs placeholder de SkinInfo por modelos finales de los 4 personajes con sus 4 variantes de color cada uno. Animator con state machine: Idle / Run / Jump / Casting / Hit / Death.',
        acceptance: [
            'SkinInfo[] tiene 4 entries con 4 gameplayPrefabs cada uno (16 prefabs en total).',
            'Animator funciona con los triggers IsMoving / Speed / Casting / Hit / Death.',
            'Bounding box similar entre los 4 modelos para mantener consistencia de cámara.',
            'No hay clipping de armas/manos en idle ni en cast.',
            'Validado con red real con 4 jugadores con skins distintas.'
        ],
        files: [
            'Assets/ScriptableObjects/Scripts/SkinInfo.cs',
            'Assets/Features/Player_and_Movement/PlayerAnimatorView.cs',
            'Assets/Models/Characters/'
        ],
        deps: ['TBR-007']
    },
    {
        id: 'TBR-026',
        title: 'Animaciones de UI (tweens y micro-interacciones)',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'Polish UX: fade-in/out de paneles, slide del HUD al iniciar partida, pop del countdown 3-2-1-Lucha, pulse en target lockeado, hover con scale 1.05 en botones, transición de selección en SpellBookUI. Usar DOTween (free) o LeanTween.',
        acceptance: [
            'Toda apertura/cierre de panel tiene tween < 0.3 s.',
            'El countdown 3-2-1-Lucha tiene pop+fade y feedback de pantalla.',
            'Hover de cualquier botón con feedback visual.',
            'No hay snap visual feo; todos los cambios de UI son interpolados.',
            'Sin frame drops por tweens (validado en Profiler).'
        ],
        files: [
            'Assets/Scripts/Controllers/PauseController.cs',
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Scripts/Controllers/SpellBookUI.cs',
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-027',
        title: 'Splash screen estática → lobby',
        type: 'feature', priority: 'medium', effort: 'S',
        assignee: 'Ches', status: 'done',
        summary:
            'Pantalla de inicio simple: logo del estudio (1.5s) → fade → logo del juego con tagline (2s) → fade → LobbyScene. Sin cinemática ni Timeline. Solo estética, no cambia el flujo. Cualquier tecla salta a LobbyScene.',
        acceptance: [
            'Escena Splash añadida al index 0 del build (LobbyScene pasa a 1).',
            'Cualquier tecla salta a LobbyScene inmediatamente.',
            'Música de intro con fade.',
            'Logo y tagline definidos por el equipo.',
            'No bloquea más de 4s acumulados en flujo.'
        ],
        files: [
            'Assets/Scenes/Splash.unity',
            'Assets/Scripts/Controllers/SplashController.cs',
            'ProjectSettings/EditorBuildSettings.asset'
        ],
        deps: ['TBR-026']
    },
    {
        id: 'TBR-028',
        title: 'Polish del Lobby (estados listos + countdown de inicio)',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Juan', status: 'todo',
        summary:
            'Mostrar slots de jugadores con avatar/iniciales/estado "Esperando…" o "Listo ✓". Cuando todos confirman (o el host pulsa Empezar), countdown sincronizado de 3 segundos antes de cargar siguiente escena. Animaciones de entrada/salida cuando alguien se conecta/desconecta. Música de lobby propia.',
        acceptance: [
            'UI con slots fijos visibles desde que el host inicia (cantidad según TBR-014).',
            'Cada slot muestra nombre, color y "Listo".',
            'Botón "Empezar" deshabilitado hasta que todos los conectados estén listos.',
            'Countdown 3-2-1 sincronizado vía ClientRpc, abortable si alguien deja "listo".',
            'Animaciones de slot in/out (TBR-026).'
        ],
        files: [
            'Assets/Scripts/LAN/LobbyController.cs',
            'Assets/Scripts/LAN/LobbyUIController.cs'
        ],
        deps: ['TBR-014', 'TBR-026']
    },
    {
        id: 'TBR-029',
        title: 'Loading screen real (reemplazar checkboxes manuales)',
        type: 'feature', priority: 'medium', effort: 'S',
        assignee: 'Ches', status: 'done',
        summary:
            'LoadingScreenController hoy tiene checkboxes manuales (checkServer, checkPlayers, checkMap, checkLoot) — debug. Reemplazar por SceneManager.LoadSceneAsync con barra de progreso real y un tip aleatorio de gameplay rotando.',
        acceptance: [
            'Barra de progreso sigue AsyncOperation.progress real.',
            'Tip de gameplay aleatorio (lista de ≥10) por carga.',
            'Animación de spinner/loader.',
            'Eliminados los SerializeField de checks manuales.',
            'LoadingScreen.unity está en build settings.'
        ],
        files: [
            'Assets/Scripts/Controllers/LoadingScreenController.cs',
            'Assets/Scenes/LoadingScreen.unity',
            'ProjectSettings/EditorBuildSettings.asset'
        ],
        deps: ['TBR-026']
    },
    {
        id: 'TBR-030',
        title: 'Skybox + iluminación + post-process del mapa',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Flan', status: 'todo',
        summary:
            'Skybox custom (procedural HDR o cubemap), luces baked, niebla de profundidad, post-process volume con bloom + color grading + vignette. Estética coherente con cell shading.',
        acceptance: [
            'Skybox visible y consistente con la paleta.',
            'Lightmaps baked para reducir cómputo en runtime.',
            'Volumen post-process global con bloom suave, color grading cálido y vignette ligero.',
            'Niebla configurada para dar profundidad sin tapar el combate.',
            '60fps mantenido tras los cambios.'
        ],
        files: [
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Settings/',
            'Assets/Materials/Skybox/'
        ],
        deps: ['TBR-024']
    },
    {
        id: 'TBR-031',
        title: 'Hit feedback visual (shake, flash, hit-stop)',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Alicia', status: 'todo',
        summary:
            'Al recibir daño: camera shake corto (~120ms), flash rojo en bordes del HUD, hit-stop (Time.timeScale 0) de ~30ms para weight. Al impactar a un enemigo: pequeño zoom y vibración del crosshair.',
        acceptance: [
            'OnDamageTaken dispara CameraController.Shake(intensity, duration).',
            'Flash rojo en HUDController es radial y dura 200 ms.',
            'Hit-stop nunca rompe networking (no afecta NetworkTime).',
            'Feedback al impactar al enemigo es local del que casteó (no de todos).',
            'Ningún feedback se queda atascado tras desactivarse el componente.'
        ],
        files: [
            'Assets/Scripts/Controllers/CameraController.cs',
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Data/PlayerStats.cs'
        ],
        deps: ['TBR-005', 'TBR-026']
    },
    {
        id: 'TBR-032',
        title: 'Podium + leaderboard animado en EndGame',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Flan', status: 'todo',
        summary:
            'Reemplazar el panel plano de EndGameUI por un podio 3D con los jugadores ordenados (ganador en el centro, alto), animación de entrada en cascada y confetti para el primero. Stats principales por jugador (kills, daño, WPM medio).',
        acceptance: [
            'Pedestals visibles, alturas según ranking final.',
            'Animación de entrada con tweens (TBR-026).',
            'Confetti particle system para el #1.',
            'Stats por jugador legibles desde 1m de distancia.',
            'Música de victoria distinta a la de gameplay.'
        ],
        files: [
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs',
            'Assets/Scripts/Models/GameOverState.cs',
            'Assets/Prefabs/Podium/'
        ],
        deps: ['TBR-012', 'TBR-026']
    },
    {
        id: 'TBR-033',
        title: 'Custom player name en LobbyScene',
        type: 'feature', priority: 'medium', effort: 'S',
        assignee: 'Jorge', status: 'done',
        summary:
            'Añadir input "Tu nombre" en LobbyScene, persistente en PlayerPrefs. Propagar como NetworkVariable<FixedString64Bytes> en NetworkPlayerController y mostrar en EnemyLabel y leaderboard final.',
        acceptance: [
            'Input field con validación (3-16 chars, no whitespace solo).',
            'Persistencia en PlayerPrefs entre runs.',
            'NetworkVariable sincronizada para todos los clientes.',
            'EnemyLabel y EndGameUI muestran el nombre custom.',
            'Si está vacío, fallback a PlayerIDGenerator.'
        ],
        files: [
            'Assets/Scripts/LAN/LobbyController.cs',
            'Assets/Scripts/LAN/NetworkPlayerController.cs',
            'Assets/Features/Player_and_Movement/EnemyLabel.cs',
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-034',
        title: 'Pantalla de stats finales del jugador',
        type: 'feature', priority: 'low', effort: 'S',
        assignee: 'Barrera', status: 'todo',
        summary:
            'Después del podio, pantalla personal con: kills, daño infligido/recibido, hechizos casteados, WPM medio, accuracy media, hechizo más usado, tiempo más rápido de cast.',
        acceptance: [
            'Stats agregados por el server durante toda la partida.',
            'Panel muestra los 7 stats del jugador local.',
            'Al pulsar Continuar avanza al menú principal.',
            'Stats reseteados al cargar GameplayScene de nuevo.',
            'Animación de números (count-up) para impacto visual.'
        ],
        files: [
            'Assets/Features/TypingCombat_and_System/EndGameUI.cs',
            'Assets/Data/PlayerStats.cs'
        ],
        deps: ['TBR-032']
    },
    {
        id: 'TBR-035',
        title: 'Modo demo / attract mode para pitch presencial',
        type: 'feature', priority: 'high', effort: 'L',
        assignee: "Jorge", status: 'todo',
        summary:
            'Si nadie toca controles en LobbyScene durante 30s, arranca una partida demo con bots IA básicos: se mueven, casean spells, mueren y respawnean. Cualquier tecla aborta el demo y vuelve al lobby.',
        acceptance: [
            'Timer de 30s sin input dispara el modo demo.',
            'Bots con Behaviour Tree mínimo: Idle → MoveToMonolith → CastSpell → MoveToEnemy → Repeat.',
            'Stats fake plausibles (no todos al máximo).',
            'Cualquier input en cualquier escena vuelve al menú principal.',
            'Loop infinito: al terminar partida demo, vuelve a lobby y reinicia el contador.'
        ],
        files: [
            'Assets/Scripts/AI/DemoBotController.cs',
            'Assets/Scripts/Controllers/AttractModeController.cs',
            'Assets/Scripts/LAN/LobbyController.cs'
        ],
        deps: ['TBR-005', 'TBR-009']
    },
    {
        id: 'TBR-036',
        title: 'Performance pass — target 60fps consistentes',
        type: 'tech', priority: 'medium', effort: 'M',
        assignee: 'Ches', status: 'done',
        summary:
            'Profiling con Unity Profiler tras integrar VFX (TBR-023) y shader (TBR-024). Identificar batching breaks, GC allocs en bucle, draw calls excesivos. Asegurar pooling activo. Target: 60fps en hardware modesto.',
        acceptance: [
            'Profile capturado en partida full sin spikes >16ms.',
            'GC allocs por frame ≈ 0 en gameplay.',
            'Draw calls < 200 en partida.',
            'Pool activo para proyectiles y partículas.',
            'Build de Windows ejecuta a 60fps en hardware target.'
        ],
        files: [
            'Assets/Scripts/Controllers/PoolManager.cs',
            'Assets/Settings/QualitySettings.asset'
        ],
        deps: ['TBR-023', 'TBR-024']
    },

    // ─── Mecánica principal del juego (TBR-037 a TBR-042) ───
    {
        id: 'TBR-037',
        title: 'Prompt de proximidad a monolitos',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Angel', status: 'done',
        summary:
            'Al entrar al rango de un monolito (PlayerInteractorView.NearestMonolith != null), mostrar un texto flotante "Pulsa E" sobre el monolito. Al salir del rango, ocultar. Listener de input "E" que dispara el modal de selección (TBR-038). Prompt es local (no replica por red).',
        acceptance: [
            'Texto aparece a < N metros, desaparece a > N (configurable).',
            'Solo el jugador local lo ve.',
            'Si hay múltiples monolitos en rango, prompt solo en el más cercano.',
            'Pulsar E con prompt visible dispara TBR-038.',
            'Si el monolito está exhausted (TBR-016), el prompt no aparece o muestra "agotado".'
        ],
        files: [
            'Assets/Features/Environment_and_Interaction/PlayerInteractorView.cs',
            'Assets/Features/Environment_and_Interaction/MonolithView.cs',
            'Assets/Scripts/Controllers/PlayerController.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-038',
        title: 'Modal de selección de nivel en monolito',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'Al pulsar E (TBR-037), abrir modal con 3 botones que muestran los 3 niveles del elemento del monolito (ej. agua: Nivel 1 / 2 / 3). Mouse hover destaca; click selecciona y dispara TBR-039. ESC cancela sin efecto.',
        acceptance: [
            'Modal aparece centrado al pulsar E sobre un monolito disponible.',
            '3 botones reflejan los SpellData configurados en MonolithController para el elemento del monolito.',
            'Cada botón muestra nombre del spell, tier, e icono del elemento.',
            'Mouse hover → highlight con tween (TBR-026).',
            'Click → cierra modal y dispara typing challenge (TBR-039).',
            'ESC → cierra modal sin consumir el monolito.',
            'Movimiento del jugador bloqueado mientras el modal está abierto.'
        ],
        files: [
            'Assets/Features/Environment_and_Interaction/MonolithController.cs',
            'Assets/Data/MonolithData.cs',
            'Assets/Scripts/Controllers/PlayerController.cs'
        ],
        deps: ['TBR-037', 'TBR-026']
    },
    {
        id: 'TBR-039',
        title: 'Typing challenge de desbloqueo de hechizo en monolito',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Jorge', status: 'todo',
        summary:
            'Una vez seleccionado el nivel (TBR-038), mostrar el rune text del SpellData. Bloquear movimiento (PlayerController.NullMoveSpeed). El jugador tipea sin error; un solo error reinicia el char actual (no penaliza). Al completar exitoso, agregar SpellData al PlayerInventory y marcar el monolito como exhausted (TBR-016). ESC cancela.',
        acceptance: [
            'Texto del rune visible mientras typing está activo.',
            'PlayerController.NullMoveSpeed activo durante el challenge.',
            'Char en error se resalta en rojo, no avanza el cursor.',
            'Completar exitoso → PlayerInventory.AddSpell(spellData) (vía ServerRpc para sync).',
            'Monolito marcado como exhausted server-side (NetworkVariable de TBR-016).',
            'ESC → cancela, restaura moveSpeed, monolito sigue disponible.',
            'Recibir daño mortal durante el typing → cancela (TBR-005).'
        ],
        files: [
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Scripts/Models/PlayerInventory.cs',
            'Assets/Scripts/Controllers/PlayerController.cs',
            'Assets/Features/Environment_and_Interaction/MonolithController.cs'
        ],
        deps: ['TBR-038', 'TBR-016']
    },
    {
        id: 'TBR-040',
        title: 'Cámara estática y movimiento bloqueado en BattleState',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Banyo', status: 'done',
        summary:
            'Al entrar a BattleState la cámara debe quedar estática (sin look libre del mouse) y apuntar al target lockeado por TargetSystem. Movimiento bloqueado (NullMoveSpeed). Salir del estado restaura cámara y movimiento.',
        acceptance: [
            'Al entrar a BattleState, mover el mouse no rota la cámara.',
            'Cámara apunta automáticamente al target (TBR-003) o al frente si no hay target.',
            'PlayerController.moveSpeed = 0 durante BattleState.',
            'Salir de BattleState (Tab) → cámara vuelve a control libre y moveSpeed restaurado.',
            'Si el target se mueve, la cámara lo sigue suavemente (Slerp).'
        ],
        files: [
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs',
            'Assets/Scripts/Controllers/CameraController.cs',
            'Assets/Scripts/Controllers/PlayerController.cs'
        ],
        deps: ['TBR-003']
    },
    {
        id: 'TBR-041',
        title: 'Indicador visual de rango y filtrado de target en BattleState',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'En BattleState mostrar un anillo/cone visual del rango de auto-target. Solo enemigos dentro del rango son seleccionables por TargetSystem. Si no hay enemigos en rango, target = null y los spells se ejecutan sin damage (feedback "sin objetivo").',
        acceptance: [
            'Anillo o cono visible en el suelo durante BattleState.',
            'Float configurable maxRange en TargetSystem.',
            'Solo enemigos dentro del rango son retornados por FindClosestTarget.',
            'Si no hay target en rango → cast feedback "sin objetivo en rango" + daño 0.',
            'El anillo desaparece al salir de BattleState.'
        ],
        files: [
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs',
            'Assets/Scripts/Controllers/TargetSystem.cs',
            'Assets/Prefabs/UI/BattleRangeIndicator.prefab'
        ],
        deps: ['TBR-003']
    },
    {
        id: 'TBR-042',
        title: 'Tab interrumpe el cast y sale de BattleState',
        type: 'feature', priority: 'medium', effort: 'S',
        assignee: 'Angel', status: 'todo',
        summary:
            'Tab es el botón único de toggle Exploration↔Battle (TBR-043). En BattleState con typing activo: cancela el cast, limpia buffer, NO consume spell ni aplica daño, cancela VFX en preparación y vuelve a ExplorationState. Sin typing activo, Tab simplemente cambia a ExplorationState.',
        acceptance: [
            'Tab durante typing → cast cancelado + transición a ExplorationState.',
            'CastInputController.spellText y currentSpell se limpian.',
            'Inventory no se afecta (no se consume el spell).',
            'ExplorationState restaurado: cámara libre, movimiento OK.',
            'VFX cast en preparación se cancelan localmente.',
            'Cooldown del spell NO se aplica si el cast fue cancelado.'
        ],
        files: [
            'Assets/Scripts/Controllers/PlayerController.cs',
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs'
        ],
        deps: ['TBR-002', 'TBR-043']
    },

    // ─── Mecánica final + sistema VFX completo (TBR-043 a TBR-048) ───
    {
        id: 'TBR-043',
        title: 'State Machine por jugador: Exploration ↔ Battle con Tab',
        type: 'feature', priority: 'critical', effort: 'M',
        assignee: 'Banyo', status: 'done',
        summary:
            'Implementar la maquina de estados local por jugador: ExplorationState (movimiento + cámara libre) y BattleState (UI de typing visible, cámara fija al target, movimiento bloqueado). Tab alterna entre ambos. Estado es local de cada cliente (no se sincroniza por red — solo afecta input/cámara del jugador local).',
        acceptance: [
            'PlayerController arranca en ExplorationState al spawnear.',
            'En ExplorationState: WASD mueve, mouse rota cámara, CastInputController está disabled, SpellBookUI oculto.',
            'Tab → transición a BattleState: movimiento bloqueado (moveSpeed=0), cámara apunta a TargetSystem.target, SpellBookUI visible.',
            'Tab desde BattleState → ExplorationState (ver TBR-042 para cancelar cast pendiente).',
            'Cambio de estado dispara eventos OnEnterBattle / OnExitBattle para que HUD/Animator/SFX reaccionen.',
            'GameOver/Death/Spectator suspenden la state machine (no se puede entrar a BattleState muerto).',
            'Estado solo afecta al cliente local; otros jugadores no necesitan saber en qué estado estamos.'
        ],
        files: [
            'Assets/Scripts/Controllers/PlayerController.cs',
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Features/Gameloop_and_StateMachine/BattleState.cs',
            'Assets/Scripts/Models/ExplorationState.cs',
            'Assets/Scripts/Models/PlayState.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-044',
        title: 'Default Spell fallback (Bola de Fuego) en CastInputController',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Banyo', status: 'done',
        summary:
            'Mientras TBR-002 (SpellBook) y TBR-039 (desbloqueo en monolito) no estén integrados, cada jugador debe poder castear igual. Asignar Bola de Fuego como default cuando PlayerInventory.spells está vacío para validar el loop completo y probar VFX/daño en red sin depender del flujo de monolitos.',
        acceptance: [
            'CastInputController tiene un campo defaultSpell asignado en el inspector del prefab del jugador.',
            'Si PlayerInventory está vacío al entrar a BattleState, currentSpell = defaultSpell y spellText = defaultSpell.runeString.',
            'Cuando TBR-002 + TBR-039 estén integrados, este default solo aplica como fallback de emergencia.',
            'Bola de Fuego SO ya tiene runeString="boladefuego" y materialVFX asignado (TBR-046).',
            'Test: 4 jugadores en LAN sin tocar monolitos pueden castear Bola de Fuego entre sí.'
        ],
        files: [
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Scripts/Models/PlayerInventory.cs',
            'Assets/ScriptableObjects/Objects/Spells/Fire/Tier 1/Bola de Fuego.asset'
        ],
        deps: ['TBR-046']
    },
    {
        id: 'TBR-045',
        title: 'Editor tool: crear los 5 arquetipos VFX restantes',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Banyo', status: 'done',
        summary:
            'VFXPrefabBuilder.cs hoy genera VFX_Projectile.prefab. Extender el tool con menu items para generar los 5 arquetipos faltantes: VFX_AOE (suelo expansivo), VFX_Aura (loop sobre el caster), VFX_Beam (rayo continuo entre caster y target), VFX_Summon (criatura efímera), VFX_BuffDebuff (icono+partícula sobre target). Cada uno con su SpellVFXBinder + script específico de comportamiento.',
        acceptance: [
            'Menu Tools/TBR/Build VFX_AOE Prefab → genera Prefabs/VFX/VFX_AOE.prefab con ParticleSystem en disco horizontal expansivo.',
            'Menu Tools/TBR/Build VFX_Aura Prefab → genera prefab loop con simulationSpace=Local que se parente al caster.',
            'Menu Tools/TBR/Build VFX_Beam Prefab → genera prefab con LineRenderer + ParticleSystem para rayo persistente entre 2 puntos.',
            'Menu Tools/TBR/Build VFX_Summon Prefab → genera prefab con placeholder mesh + ParticleSystem (placeholder hasta TBR-025).',
            'Menu Tools/TBR/Build VFX_BuffDebuff Prefab → genera prefab pequeño que sigue al target con icono floating.',
            'Cada uno tiene componente específico: AOEVFX, AuraVFX, BeamVFX, SummonVFX, BuffDebuffVFX (paralelos a ProjectileVFX).',
            'Cada componente recibe Spell vía Launch(spell, origin, direction|target) y usa duration/range/speed del SO.',
            'Idempotente: correr el menu 2 veces no duplica prefabs.'
        ],
        files: [
            'Assets/Editor/VFX/VFXPrefabBuilder.cs',
            'Assets/Scripts/VFX/AOEVFX.cs',
            'Assets/Scripts/VFX/AuraVFX.cs',
            'Assets/Scripts/VFX/BeamVFX.cs',
            'Assets/Scripts/VFX/SummonVFX.cs',
            'Assets/Scripts/VFX/BuffDebuffVFX.cs',
            'Assets/Prefabs/VFX/'
        ],
        deps: []
    },
    {
        id: 'TBR-046',
        title: 'Materiales por elemento (10 elementos) bajo Assets/Materials/VFX/',
        type: 'feature', priority: 'high', effort: 'S',
        assignee: 'Alicia', status: 'todo',
        summary:
            'Hoy solo existe M_Fire_T1.mat. Crear un material URP/Particles/Unlit por elemento del enum Elements (Fire, Water, Earth, Air, Nature, Thunder/Lightning, Dark, Light, Ice, Lava) con color de marca apropiado por elemento. Un solo material por elemento — el tier se controla en el binder (size/emission), no necesita 3 materiales por elemento.',
        acceptance: [
            '10 archivos .mat en Assets/Materials/VFX/: M_Fire, M_Water, M_Earth, M_Air, M_Nature, M_Lightning, M_Darkness, M_Light, M_Ice, M_Lava.',
            'Todos usan shader Universal Render Pipeline/Particles/Unlit con additive blending.',
            'Colores: Fire=naranja rojizo, Water=azul, Earth=marrón, Air=gris-blanco, Nature=verde, Lightning=amarillo, Darkness=violeta oscuro, Light=blanco-dorado, Ice=cian claro, Lava=rojo intenso.',
            'EmissionColor configurado para que brillen con bloom (post-process de TBR-030).',
            'Editor tool: extender VFXPrefabBuilder con menu Tools/TBR/Build All Element Materials que genera los 10.',
            'Idempotente: si ya existe, no sobreescribe (solo crea los faltantes).'
        ],
        files: [
            'Assets/Editor/VFX/VFXPrefabBuilder.cs',
            'Assets/Materials/VFX/',
            'Assets/Scripts/Brand/WotKBrand.cs'
        ],
        deps: []
    },
    {
        id: 'TBR-047',
        title: 'Mapeo de los ~50 Spell SOs a archetype + asignar materialVFX',
        type: 'tech', priority: 'high', effort: 'M',
        assignee: 'Alicia', status: 'todo',
        summary:
            'Iterar todos los Spell SOs de Assets/ScriptableObjects/Objects/Spells/** y asignar: archetype (Projectile/AOE/Aura/Beam/Summon/Buff/Debuff), tier (TierOne/TierTwo/TierThree según carpeta), runeString (palabra sin espacios sin tildes en lowercase), materialVFX (M_<elemento>.mat según elementType). Hacer un editor tool que lo automatice y luego revisar manualmente los casos especiales (ej. Curación = Buff, no proyectil).',
        acceptance: [
            'Menu Tools/TBR/Auto-Assign Spell Archetypes que recorre todos los Spell SOs.',
            'tier se infiere del path: ".../Tier 1/..." → TierOne, etc.',
            'archetype default = Projectile pero respeta el campo si ya fue asignado manualmente.',
            'runeString default = spellName.ToLower().Replace(" ", "").Quitar tildes/ñ — pero solo si está vacío (no sobreescribir).',
            'materialVFX se asigna según elementType (mapping del enum a los .mat de TBR-046).',
            'Genera reporte console con los spells modificados y los que necesitan revisión manual.',
            'Validación final manual: cada uno de los ~50 spells revisado y firmado por un asignee.'
        ],
        files: [
            'Assets/Editor/VFX/VFXPrefabBuilder.cs',
            'Assets/ScriptableObjects/Scripts/Spell.cs',
            'Assets/ScriptableObjects/Objects/Spells/'
        ],
        deps: ['TBR-045', 'TBR-046']
    },
    {
        id: 'TBR-048',
        title: 'Server-side damage + accuracy multiplier + cooldown en cast',
        type: 'feature', priority: 'critical', effort: 'M',
        assignee: 'Ches', status: 'done',
        summary:
            'La red de VFX ya está pero NO aplica daño (SpellNetworkController solo dispara ClientRpc visual). Falta: (1) ProjectileVFX.OnTriggerEnter aplica daño solo en IsServer; (2) PlayerStatsNet.TakeDamageServerRpc(damage, attackerId) que decrementa currentHP y dispara OnDamageTaken; (3) accuracy multiplier (TypingStats.GetDamageBonusMultiplier) se pasa por el ServerRpc; (4) cooldown por SpellNetworkController para evitar spam.',
        acceptance: [
            'ProjectileVFX.OnTriggerEnter detecta PlayerStatsNet y llama TakeDamageServerRpc solo si IsServer y other.OwnerClientId != caster.',
            'PlayerStatsNet.TakeDamageServerRpc decrementa currentHP (NetworkVariable) y dispara OnDamageTaken via OnValueChanged.',
            'CastSpellServerRpc valida cooldown: SpellNetworkController guarda lastCastTime por spellId y rechaza casts antes de spell.cooldown segundos.',
            'damage final = spell.damage × accuracy multiplier (TypingStats.GetDamageBonusMultiplier(accuracy)) — accuracy se manda desde el client en el ServerRpc.',
            'Si accuracy < 0.3 → multiplier 0 (sin daño) pero el VFX se reproduce (feedback).',
            'killCount del atacante incrementa cuando currentHP del target llega a 0 (dispara TBR-005 chain).',
            'Validado con 4 clientes: el daño es consistente, no hay daño doble por race conditions.'
        ],
        files: [
            'Assets/Scripts/VFX/SpellNetworkController.cs',
            'Assets/Scripts/VFX/ProjectileVFX.cs',
            'Assets/Scripts/LAN/PlayerStatsNet.cs',
            'Assets/Scripts/Models/TypingStats.cs',
            'Assets/Scripts/Models/DamageCalculator.cs'
        ],
        deps: ['TBR-011', 'TBR-023']
    },

    // ─── Polish pass para Demo Day (TBR-049 a TBR-057) ───
    // Branding de referencia: _documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf
    // Paleta (Assets/Scripts/Brand/WotKBrand.cs):
    //   RojoLotti     #E40046 — acento principal / CTAs / daño
    //   GatorOrange   #FA4616 — secundario cálido / highlights de fuego
    //   VibrantViolet #485CC7 — secundario frío / mana / UI fría
    //   Black         #070504 — fondos
    //   White         #FFFFFF — texto principal
    // Logos: _documents/Artist/_extracted/final/{logo_wotk_black,isotipo_black,studio_redbloodcells}.png
    // Tipografía: ver brand book (display heavy + body sans). Si no está importada, usar TMP_FontAsset
    // referenciado en WotKBrand y dejar TODO para reemplazar por la oficial.
    {
        id: 'TBR-049',
        title: 'Polish escena Splash — fade in/out global + branding WotK',
        type: 'feature', priority: 'medium', effort: 'S',
        assignee: 'Jorge', status: 'done',
        summary:
            'La escena Splash (TBR-027) ya carga logos pero sin transiciones suaves: aparición y corte abrupto. Aplicar fade-in/fade-out a TODOS los elementos visibles (logo estudio Red Blood Cells → logo Wizards of the Keyboard + tagline → transición a LobbyScene) usando CanvasGroup.alpha y DOTween. Fondo negro brand (#070504), logos centrados, tagline en blanco con tipografía del brand book. Cualquier tecla skipea con fade-out acelerado (no corte seco).',
        acceptance: [
            'SplashController orquesta una secuencia con CanvasGroup por panel: fade-in 0.5s → hold 1.5s → fade-out 0.5s.',
            'Secuencia: pantalla negra → logo studio_redbloodcells.png (1.5s visible) → fade cruzado → logo_wotk_black.png + tagline (2s visible) → fade-out a negro → load LobbyScene.',
            'Fondo color WotKBrand.Black (#070504); logos centrados con padding seguro (≥10% de pantalla).',
            'Tagline en WotKBrand.White, fuente del brand book (placeholder TMP si la oficial no está importada — dejar comentario con TODO).',
            'Cualquier tecla durante la secuencia: corta el hold actual y fuerza fade-out 0.2s antes de cargar lobby (no hay corte seco).',
            'Música de intro también hace fade (AudioSource.DOFade) sincronizada con la imagen.',
            'Sin frame drops en 60fps; los tweens usan SetUpdate(true) para sobrevivir Time.timeScale=0 (no aplica aquí pero por consistencia).',
            'Tiempo total acumulado ≤ 4.5s en flujo normal; ≤ 0.7s si el usuario presiona tecla.'
        ],
        files: [
            'Assets/Scenes/Splash.unity',
            'Assets/Scripts/Controllers/SplashController.cs',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/Sprites/Brand/logo_wotk_black.png',
            'Assets/Sprites/Brand/studio_redbloodcells.png',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-050',
        title: 'Polish escena Lobby — aplicar identidad visual del brand book',
        type: 'feature', priority: 'high', effort: 'L',
        assignee: 'Angel', status: 'done',
        summary:
            'El LobbyScene hoy es funcional pero gráficamente plano. Rediseñarlo siguiendo el BRAND BOOK WIZARDS OF THE KEYBOARD.pdf: fondo con isotipo en marca de agua, header con logo_wotk_black, slots de jugadores con frame magico (bordes redondeados + acento RojoLotti), tipografía del brand, botones con estilo de "rune card" (fondo Black, borde GatorOrange en hover, texto White). Mantener funcionalidad existente (capacidad 4-8, lista en vivo, ready states) — este ticket es 100% visual.',
        acceptance: [
            'Fondo de la pantalla: WotKBrand.Black con isotipo_sticker.png como watermark central a ~10% alpha + sutil vignette violet/red.',
            'Header superior con logo_wotk_black.png a la izquierda y texto "LOBBY" en mayúsculas con tipografía display del brand book.',
            'Slots de jugador estilo "rune card": 9-slice frame con bordes redondeados (~16px), fondo #070504 a 90% alpha, borde 2px en WotKBrand.VibrantViolet por defecto, RojoLotti cuando el slot está marcado "Listo".',
            'Cada slot muestra: avatar circular con halo del color del jugador, nombre en tipografía body del brand, badge de estado ("Esperando" / "Listo ✓").',
            'Botón principal "Empezar" / "Listo": fondo WotKBrand.RojoLotti, texto White, hover scale 1.05 + glow GatorOrange (usar Outline u Image con tint).',
            'Botones secundarios (cancel, settings): outline-only (borde VibrantViolet, fondo transparente), texto White.',
            'Inputs (nombre, IP) con borde inferior 1px en GatorOrange y fondo translúcido; placeholder en gris a 60% alpha.',
            'Chips/pills (host, ping, ready count) usan la paleta del brand: pill RojoLotti para host, VibrantViolet para clientes.',
            'Layout responsive a 1280×720 y 1920×1080 sin overlaps; safe-area 64px alrededor de los bordes.',
            'Todos los colores hardcoded reemplazados por refs a WotKBrand.cs (no #RRGGBB sueltos en escena o prefab).',
            'Funcionalidad de TBR-014/TBR-028/TBR-033 intacta; este ticket no toca lógica de red.'
        ],
        files: [
            'Assets/Scenes/LobbyScene.unity',
            'Assets/Scripts/LAN/LobbyUIController.cs',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/Editor/Brand/WotKBrandSetup.cs',
            'Assets/Prefabs/UI/LobbySlot.prefab',
            'Assets/Sprites/Brand/',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-051',
        title: 'Polish UI SpellBook — rune card style + animaciones de selección',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: null, status: 'todo',
        summary:
            'SpellBookUI (TBR-002) funciona pero su layout es funcional/debug: lista plana con texto. Rediseñar cada slot como una "rune card" con icono del elemento, nombre del spell en tipografía display, runeString destacada, daño/cooldown como mini-stats, y borde por tier (TierOne=VibrantViolet, TierTwo=GatorOrange, TierThree=RojoLotti). Selección con scale-up + glow del color del elemento. Paginación con flechas estilizadas. Todo siguiendo el brand book.',
        acceptance: [
            'Cada slot de spell se renderiza como una "rune card" (~180×220px) con: icono del elemento arriba, nombre del spell en display font, runeString grande en mono font, mini-stats (dmg, cooldown) abajo.',
            'Fondo del slot: WotKBrand.Black 85% alpha + textura de pergamino sutil (opcional, placeholder si no hay asset).',
            'Borde del slot por tier: 2px VibrantViolet (T1), GatorOrange (T2), RojoLotti (T3). Slots de tiers bloqueados a 30% alpha + ícono de candado.',
            'Slot seleccionado: scale 1.08 + outline glow en color del elemento (lerp con WotKBrand) + leve elevación (sombra inferior).',
            'Hover: scale 1.03 + tween 0.12s ease-out (DOTween).',
            'Paginación: flechas izquierda/derecha con icono custom + indicador "página X/Y" en VibrantViolet; mouse wheel ya conectado por TBR-002.',
            'Header del panel: título "GRIMORIO" en display font + chip del tier actual del jugador (color por tier).',
            'Aparición del SpellBookUI al entrar a BattleState: fade + slide up (0.25s) desde la parte inferior; salida inversa al cancelar/castear.',
            'No bloquea framerate: máximo 9 cards renderizadas a la vez (3 por página × 3 visibles si scroll), pool si más.',
            'Todos los colores referencian WotKBrand; ningún hex hardcoded fuera de WotKBrand.cs.'
        ],
        files: [
            'Assets/Scripts/Controllers/SpellBookUI.cs',
            'Assets/Prefabs/UI/SpellCard.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/ScriptableObjects/Scripts/Spell.cs',
            'Assets/Sprites/UI/SpellBook/',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-052',
        title: 'Polish UI Typing — caja de runas + feedback de tecleo on-brand',
        type: 'feature', priority: 'high', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'La UI de typing actual muestra el spellText como texto plano. Rediseñarla como una "caja de runas" centrada en pantalla durante BattleState: el spellText completo visible en gris claro, las letras ya tecleadas en GatorOrange con leve glow, la letra activa (next) en RojoLotti con pulso, las erróneas en rojo apagado con shake corto. Barra de accuracy/WPM debajo en estilo brand. Indicador de cast progress estilo "rune circle" alrededor del player.',
        acceptance: [
            'TypingHUD se renderiza como un panel central inferior (anchored bottom-center, offset 120px) con safe-area.',
            'Fondo del panel: gradiente vertical de WotKBrand.Black 0% (top) → 90% (bottom) para que respire sobre el gameplay.',
            'spellText completo en mono font, tamaño 48-64px: letras pendientes en #FFFFFF 60% alpha, tecleadas en WotKBrand.GatorOrange con outer-glow 4px, letra activa en WotKBrand.RojoLotti con pulse (scale 1.0↔1.12 en 0.6s loop).',
            'Error de tecla: la letra activa parpadea rojo apagado (#FA4616 desaturado) + shake horizontal 6px durante 0.18s + SFX (existente).',
            'Mini-stats sobre la caja: accuracy% (0-100) en pill RojoLotti si <50%, GatorOrange si 50-80%, VibrantViolet si >80%; WPM al lado en chip outline-only.',
            'Cast progress indicator: anillo circular alrededor del player local (world-space canvas) que se rellena en RojoLotti según el progreso del spellText; desaparece al castear o cancelar.',
            'Aparición/desaparición de la UI sigue el evento OnEnterBattle/OnExitBattle (TBR-043): fade + slide 0.2s.',
            'Cuando se completa el cast con éxito: la caja hace flash en GatorOrange + scale 1.1 + fade-out (0.25s).',
            'Compatible con TBR-026 (tweens existentes); no rompe los hit-feedbacks de TBR-031.',
            'Sin hex hardcoded: todo via WotKBrand.cs.'
        ],
        files: [
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Features/TypingCombat_and_System/SpellUIController.cs',
            'Assets/Scripts/Controllers/CastInputController.cs',
            'Assets/Prefabs/UI/TypingHUD.prefab',
            'Assets/Prefabs/UI/CastProgressRing.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-053',
        title: 'Polish UI Exploration State — minimapa, crosshair y HUD ambient',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'En ExplorationState (TBR-043) la UI es casi nula. Diseñar un HUD ambient on-brand: barra de vida ornamentada (esquina sup-izq) estilo "tinta sobre pergamino", contador de vidas como pequeños iconos de gota-de-mana, mini compass/heading en la sup-der, contador de monolitos visitados, prompts contextuales ("Pulsa TAB para combate" cerca de enemigos, "Pulsa E para interactuar" cerca de monolitos). Estética calmada con paleta fría (VibrantViolet dominante).',
        acceptance: [
            'HUD de exploración con 4 zonas: sup-izq (HP+vidas), sup-der (compass+monolitos), inf-izq (mini-leaderboard colapsado), inf-der (prompts contextuales).',
            'Barra de HP estilo "rune meter": frame de 9-slice en VibrantViolet, fill en lerp(GatorOrange→RojoLotti) según HP%, textura de pergamino sutil de fondo.',
            'Vidas restantes como 3 iconos de gota (sprite custom o placeholder); vacías en outline gris a 40% alpha.',
            'Compass superior derecho: arco horizontal con marcas N/E/S/W y dots para spawn points; pequeño, ~280px ancho.',
            'Prompts contextuales: pill outline-only que aparece con fade+slide cuando el player está en rango (uses TBR-038 raycast detector). Texto ej "[TAB] Combatir" en GatorOrange / "[E] Interactuar" en VibrantViolet.',
            'Mini-leaderboard inf-izq: lista colapsada de 4 jugadores con HP%, nombre y color; expandible con Tab-hold (no conflict con TBR-043 — usar hold).',
            'Color global de la capa exploration: dominante VibrantViolet con acentos GatorOrange para CTAs.',
            'Transición Exploration↔Battle: el HUD de exploración hace fade-out 0.2s mientras el typing HUD (TBR-052) entra; al revés al salir.',
            'Sin hex hardcoded; todo via WotKBrand.cs.',
            '60fps mantenido (perfilar canvas raycasts).'
        ],
        files: [
            'Assets/Scripts/Controllers/HUDController.cs',
            'Assets/Scripts/Controllers/PlayerController.cs',
            'Assets/Prefabs/UI/ExplorationHUD.prefab',
            'Assets/Prefabs/UI/ContextPrompt.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/Features/Environment_and_Interaction/PlayerInteractorView.cs',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-054',
        title: 'Polish UI Pausa — panel modal grimorio + sliders on-brand',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Angel', status: 'todo',
        summary:
            'El menú de pausa (TBR-001) hoy es funcional pero un panel gris. Rediseñarlo como un "tomo abierto" central: fondo del juego con blur+darken (CanvasGroup 0→0.7), panel central con frame ornamental, título "PAUSA" en display font, botones Reanudar/Volver al Lobby con estilo brand, sliders de volumen Master/Music/SFX con knob personalizado y fill en RojoLotti, footer con créditos cortos del estudio. Mantener funcionalidad existente.',
        acceptance: [
            'Al pulsar ESC: blur background (Volume override transitorio o RawImage con Material custom) + dark overlay 70%, fade-in 0.18s.',
            'Panel central ~640×720px con frame ornamental (9-slice + esquinas decorativas en sprites del brand) sobre fondo WotKBrand.Black 95%.',
            'Título "PAUSA" en display font, tamaño 56px, color White con underline en GatorOrange.',
            'Botones (Reanudar / Volver al Lobby / Salir): full-width, altura 56px, fondo Black + borde 2px VibrantViolet en idle, hover/focus → fondo RojoLotti + borde GatorOrange + scale 1.02.',
            'Sliders Master/Music/SFX: track de 4px en VibrantViolet 40%, fill en RojoLotti, knob circular 18px en GatorOrange con sombra; valor numérico 0-100 a la derecha.',
            'Persistencia ya cubierta por TBR-001; este ticket NO toca lógica de PlayerPrefs ni AudioManager.',
            'Footer del panel: línea con "Wizards of the Keyboard · Red Blood Cells" en mono font 12px, color White 50% alpha.',
            'Animación de entrada/salida: panel escala desde 0.92→1.0 con bounce ease (0.22s); fondo blur fade.',
            'ESC vuelve a cerrar respetando Time.timeScale=1 al final del fade-out.',
            'Compatible con TBR-026; usa los mismos tweens base.',
            'Sin hex hardcoded; todo via WotKBrand.cs.'
        ],
        files: [
            'Assets/Scripts/Controllers/PauseController.cs',
            'Assets/Scripts/Controllers/VolumeController.cs',
            'Assets/Prefabs/UI/PauseMenu.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/Sprites/UI/PauseFrame/',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-055',
        title: 'Polish background del mapa — skydome, parallax y atmósfera',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Juan', status: 'todo',
        summary:
            'GameplayScene hoy tiene skybox plano y horizonte vacío. Implementar un background atmosférico estilo cell-shaded coherente con WotK: skydome con gradiente VibrantViolet (cenit) → GatorOrange (horizonte) → Black (zenith inferior), 2-3 capas de montañas/torres lejanas en parallax sutil (rotación con la cámara), partículas ambientales lentas (motas, polen, brasas) y fog de profundidad. Coordinar con TBR-030 (post-process) — este ticket es el background per se.',
        acceptance: [
            'Skydome custom (sphere flip + shader unlit con gradient) reemplaza el skybox por defecto.',
            'Gradient: cenit VibrantViolet (#485CC7), horizonte GatorOrange (#FA4616), zenith inferior Black (#070504). Ajustable por tiempo del día (variable serializada 0-1 si hay tiempo, si no, fijo).',
            '2-3 capas de silhouettes lejanas (montañas + torres mágicas) renderizadas en su propio layer y cámara secundaria con parallax (rotation proxy) — no traslación, solo yaw para que rote con el player.',
            'Particle systems ambientales: motas de luz lentas (worldSpace, baja densidad ~200 partículas en el área del mapa) en tinta GatorOrange/VibrantViolet.',
            'Fog Exponential Squared activado con color en WotKBrand.Black con leve tinte VibrantViolet, density tal que objetos a >120m se fundan al horizonte.',
            'Sin frame drops: el skydome + parallax no debe pasar de 1 draw call por capa; las particles culling al frustum.',
            'No interfiere con gameplay: ningún objeto del background recibe físicas ni colisiones.',
            'Skydome y silhouettes baked en lightmap (estáticos).',
            'Compatible con TBR-030 (este ticket NO redefine post-process — solo background geometry/shaders).',
            'Validación visual a 60fps en LAN con 4 jugadores.'
        ],
        files: [
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Materials/Skybox/WotK_Skydome.mat',
            'Assets/Shaders/WotK_SkydomeGradient.shader',
            'Assets/Prefabs/Environment/BackgroundLayers.prefab',
            'Assets/Prefabs/Environment/AmbientParticles.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-056',
        title: 'Polish luces del mapa — key light, rim y luces mágicas dinámicas',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Alicia', status: 'todo',
        summary:
            'Iluminación actual es la Directional Light default. Setup completo: key light direccional cálida (tinte GatorOrange) con sombras suaves, fill light fría (tinte VibrantViolet) sin sombras, rim light sutil sobre los players para separación, point lights mágicas pulsando suavemente en los monolitos y spawn points, y un leve flicker en áreas de combate. Lightmapping para estática + realtime solo donde sea necesario.',
        acceptance: [
            'Directional Light "Key": rotación ~45° desde el horizonte, color lerp(White, GatorOrange, 0.3), intensidad 1.1, soft shadows.',
            'Directional Light "Fill": opuesta a la key, color VibrantViolet (#485CC7), intensidad 0.35, sin sombras.',
            'Rim light por player: shader del player con _RimColor en WotKBrand.GatorOrange y _RimPower 3 (si no existe, dejar TODO y usar una luz spotlight global proyectada).',
            'Monolitos: cada uno con un point light realtime, color por elemento (mapping del enum Elements → WotKBrand), intensity oscilando suavemente entre 1.5 y 2.2 a 0.8 Hz (sine wave).',
            'Spawn points: point light tenue color VibrantViolet, intensidad 0.8, range 6m; se apaga 2s después de un respawn (anim via tween).',
            'Lightmapping baked para todos los static lights; los monolitos/spawns son realtime mixed.',
            'Sin frame drops: máximo 8 realtime lights simultáneas en pantalla, culling por distancia.',
            'Compatible con TBR-030 (post-process bloom recoge los puntos brillantes correctamente).',
            'Validar a 60fps con 4 jugadores en LAN, varios monolitos visibles.',
            'Colores via WotKBrand.cs (sin hex sueltos en escena).'
        ],
        files: [
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Settings/URP-HighFidelity.asset',
            'Assets/Prefabs/Environment/MonolithLight.prefab',
            'Assets/Prefabs/Environment/SpawnPointLight.prefab',
            'Assets/Scripts/Brand/WotKBrand.cs',
            'Assets/Features/Environment_and_Interaction/MonolithView.cs',
            '_documents/Artist/BRAND BOOK WIZARDS OF THE KEYBOARD.pdf'
        ],
        deps: []
    },
    {
        id: 'TBR-057',
        title: 'Polish NPCs ambientales — pájaros/luciérnagas que cruzan el mapa',
        type: 'feature', priority: 'low', effort: 'S',
        assignee: 'Barrera', status: 'done',
        summary:
            'Para dar vida al mapa sin tocar gameplay, agregar NPCs pasivos puramente visuales: bandadas de pájaros (sprites o quads texturizados) que cruzan el cielo cada 20-40s en path lineal, y luciérnagas (point lights pequeños + sprite billboard) flotando lentamente en zonas bajas. Sin colisión, sin red (solo cliente local), sin lógica de juego. Deterministas no — pueden variar entre clientes.',
        acceptance: [
            'AmbientNPCSpawner componente en GameplayScene con 2 pools: BirdsFlock y Firefly.',
            'BirdsFlock: cada 20-40s (random range) spawnea 3-6 pájaros que cruzan el cielo en path lineal con leve sine offset en Y; sprites billboard 64×64; despawn al salir del frustum extendido.',
            'Pájaros: silueta oscura (color WotKBrand.Black) sobre el cielo para que se lean contrastando con el skydome (TBR-055). Animación de aleteo simple (flipbook 4 frames o SpriteRenderer rotation).',
            'Firefly: 8-12 instancias estáticas al spawn de la escena, vuelan en path Perlin noise dentro de un volumen (BoxCollider trigger para definir el área); cada una tiene point light pequeño (range 2m) en GatorOrange + sprite billboard 8×8.',
            'Las firefly pulsan suavemente (alpha + light intensity) entre 0.4 y 1.0 a frecuencias ligeramente distintas para que se sientan vivas.',
            'No tienen Rigidbody, no tienen colliders sólidos (excepto el trigger de área para fireflies), no son NetworkObject.',
            'Spawning solo en cliente local: no se sincroniza por red (cada player ve los suyos).',
            'Performance: máximo 30 NPCs activos a la vez (pool); sin frame drops a 60fps.',
            'Colores via WotKBrand; ningún hex hardcoded.',
            'Toggle de debug en el componente para desactivar en builds si causa drops.'
        ],
        files: [
            'Assets/Scripts/Environment/AmbientNPCSpawner.cs',
            'Assets/Scripts/Environment/AmbientBird.cs',
            'Assets/Scripts/Environment/AmbientFirefly.cs',
            'Assets/Prefabs/Environment/AmbientBird.prefab',
            'Assets/Prefabs/Environment/AmbientFirefly.prefab',
            'Assets/Sprites/Environment/bird_silhouette.png',
            'Assets/Sprites/Environment/firefly.png',
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Scripts/Brand/WotKBrand.cs'
        ],
        deps: []
    },

    // ─── Integración de maquetas de islas (TBR-058 a TBR-061) ───
    // Track separado para implementar los modelos que están entregando los artistas en
    // la escena de gameplay. Una isla por ticket para paralelizar import + setup.
    // Convención: cada isla es un bioma/zona del mapa, ubicada en su propio offset world-space.
    {
        id: 'TBR-058',
        title: 'Implementar maqueta Isla 1 en GameplayScene',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Juan', status: 'done',
        summary:
            'Integrar el modelo entregado por los artistas para la Isla 1 (bioma central / zona de spawn). Import del FBX, generar prefab compuesto con colliders y materiales, ubicarlo en GameplayScene en el offset definido y validar que el flujo gameplay (spawn, navegación, monolitos sobre la isla) funciona sin clipping ni cracks.',
        acceptance: [
            'Modelo importado bajo Assets/Models/Environment/Island_01/ con import settings correctos (Read/Write off salvo NavMesh, scale factor coherente con escala del juego, normales calculadas).',
            'Prefab Assets/Prefabs/Environment/Island_01.prefab con pivot en (0,0,0) y bounding box correcto.',
            'MeshColliders para terreno caminable; BoxColliders/MeshColliders convex para props grandes (rocas, árboles).',
            'Materiales asignados — placeholder URP/Lit hasta que TBR-024 (cell-shading) esté disponible; entonces migrar.',
            'LOD Group configurado si el modelo trae múltiples LODs; si no, mínimo culling con LODBias razonable.',
            'Isla ubicada en GameplayScene en su offset asignado (a definir con level design, por defecto centro del mapa 0,0,0).',
            'NavMesh baked sobre el terreno caminable si la isla soporta NPCs (TBR-057).',
            '4 spawn points (TBR-018) ubicados sobre la isla sin caer fuera del terreno ni dentro de geometría.',
            'Monolitos (TBR-016) pueden colocarse sobre la isla sin clipping; al menos 2 puntos válidos identificados.',
            'Performance: 60fps con la isla en escena sola (sin otras islas) en hardware target.',
            'Sin missing scripts ni warnings de import al abrir la escena.'
        ],
        files: [
            'Assets/Models/Environment/Island_01/',
            'Assets/Prefabs/Environment/Island_01.prefab',
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Features/Environment_and_Interaction/'
        ],
        deps: []
    },
    {
        id: 'TBR-059',
        title: 'Implementar maqueta Isla 2 en GameplayScene',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Juan', status: 'done',
        summary:
            'Integrar el modelo de la Isla 2 (segundo bioma) entregado por los artistas. Mismo pipeline que TBR-058 pero con el modelo y la ubicación correspondientes. Validar coherencia visual con la Isla 1 si comparten frontera o si hay puentes/conexiones.',
        acceptance: [
            'Modelo importado bajo Assets/Models/Environment/Island_02/ con import settings consistentes con Island_01.',
            'Prefab Assets/Prefabs/Environment/Island_02.prefab con pivot en (0,0,0).',
            'MeshColliders + colliders para props como en TBR-058.',
            'Materiales placeholder URP/Lit; migrar a cell-shading cuando TBR-024 esté listo.',
            'Ubicada en GameplayScene en offset asignado (a definir con level design).',
            'NavMesh baked si aplica (TBR-057).',
            'Al menos 1 spawn point (TBR-018) y 2 monolitos (TBR-016) válidos sobre la isla.',
            'Si comparte frontera con Isla 1: sin gaps visibles ni Z-fighting en la unión.',
            'Performance: 60fps con Isla 1 + Isla 2 en escena.',
            'Sin warnings de import.'
        ],
        files: [
            'Assets/Models/Environment/Island_02/',
            'Assets/Prefabs/Environment/Island_02.prefab',
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Features/Environment_and_Interaction/'
        ],
        deps: []
    },
    {
        id: 'TBR-060',
        title: 'Implementar maqueta Isla 3 en GameplayScene',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Juan', status: 'todo',
        summary:
            'Integrar el modelo de la Isla 3 (tercer bioma) entregado por los artistas. Mismo pipeline que TBR-058. Validar transiciones con las islas adyacentes.',
        acceptance: [
            'Modelo importado bajo Assets/Models/Environment/Island_03/ con import settings consistentes.',
            'Prefab Assets/Prefabs/Environment/Island_03.prefab con pivot en (0,0,0).',
            'Colliders correctos para terreno y props.',
            'Materiales placeholder URP/Lit; migrar a cell-shading cuando TBR-024 esté listo.',
            'Ubicada en GameplayScene en offset asignado.',
            'NavMesh baked si aplica.',
            'Al menos 1 spawn point (TBR-018) y 2 monolitos (TBR-016) válidos sobre la isla.',
            'Sin gaps ni Z-fighting con islas adyacentes.',
            'Performance: 60fps con Isla 1 + Isla 2 + Isla 3 en escena.',
            'Sin warnings de import.'
        ],
        files: [
            'Assets/Models/Environment/Island_03/',
            'Assets/Prefabs/Environment/Island_03.prefab',
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Features/Environment_and_Interaction/'
        ],
        deps: []
    },
    {
        id: 'TBR-061',
        title: 'Implementar maqueta Isla 4 en GameplayScene',
        type: 'feature', priority: 'medium', effort: 'M',
        assignee: 'Juan', status: 'todo',
        summary:
            'Integrar el modelo de la Isla 4 (cuarto bioma) entregado por los artistas. Mismo pipeline que TBR-058. Última isla del mapa; con esta debe quedar el contorno completo de la arena para el demo day.',
        acceptance: [
            'Modelo importado bajo Assets/Models/Environment/Island_04/ con import settings consistentes.',
            'Prefab Assets/Prefabs/Environment/Island_04.prefab con pivot en (0,0,0).',
            'Colliders correctos para terreno y props.',
            'Materiales placeholder URP/Lit; migrar a cell-shading cuando TBR-024 esté listo.',
            'Ubicada en GameplayScene en offset asignado.',
            'NavMesh baked si aplica.',
            'Al menos 1 spawn point (TBR-018) y 2 monolitos (TBR-016) válidos sobre la isla.',
            'Sin gaps ni Z-fighting con islas adyacentes.',
            'Performance: 60fps con las 4 islas + 4 jugadores casteando (validar contra TBR-036).',
            'Mapa completo navegable de extremo a extremo sin caerse al vacío en zonas previstas.',
            'Sin warnings de import.'
        ],
        files: [
            'Assets/Models/Environment/Island_04/',
            'Assets/Prefabs/Environment/Island_04.prefab',
            'Assets/Scenes/GameplayScene.unity',
            'Assets/Features/Environment_and_Interaction/'
        ],
        deps: []
    }
];
