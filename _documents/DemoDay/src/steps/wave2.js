// Wave 2 — 12 tickets: TBR-001, TBR-005, TBR-007, TBR-012, TBR-018, TBR-021, TBR-022, TBR-027, TBR-028, TBR-029, TBR-030, TBR-038
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-001': `## Estado actual

\\\`PauseController\\\` abre el panel y maneja \\\`Time.timeScale\\\`, pero le falta:

- Botón **Volver al menú** carga \\\`"MainMenu"\\\` que **no está en build** (ver [TBR-020]).
- Sliders de volumen no persisten ni alimentan a \\\`AudioManager\\\` (depende de [TBR-013]).
- Sin guard contra abrir pausa durante \\\`WaitingState\\\` o \\\`GameOverState\\\`.

## Pasos

### 1. Guard de estado

En \\\`OnPausa()\\\`, antes de pausar:

\\\`\\\`\\\`csharp
var s = GameplayManager.Instance.stateMachine.currentState;
if (s is GameOverState || s is WaitingState) return;
\\\`\\\`\\\`

### 2. Persistir volúmenes

En cada cambio de slider:

\\\`\\\`\\\`csharp
PlayerPrefs.SetFloat("vol.master", v);
PlayerPrefs.Save();
\\\`\\\`\\\`

En \\\`AudioManager.Awake()\\\` cargá los valores guardados.

### 3. Cablear los sliders

\\\`VolumeController.SetVolume\\\` debe delegar a \\\`AudioManager.Instance.SetVolume(channel, v)\\\` en lugar de tocar \\\`AudioListener.volume\\\` directo. Bloqueado por [TBR-013].

### 4. Volver al menú

Esperá a que [TBR-020] decida el destino y reemplazá el string \\\`"MainMenu"\\\` en \\\`SceneMenu()\\\` por la escena correcta.

## Cómo verificar

- ESC abre y cierra el panel sin glitches.
- Cambiar slider → cerrar y reabrir el juego → el valor persiste.
- En partida 4-jugadores, una pausa local **no** congela a los demás.
`,

'TBR-005': `## Estado actual

\\\`PlayerStats\\\` tiene \\\`TakeDamage\\\` y \\\`LoseLife\\\` pero **nadie las llama** desde combate real. \\\`isAlive\\\` nunca cambia. La cadena daño → muerte → respawn está rota.

## Pasos

### 1. Verificar que el daño llega

Poné un \\\`Debug.Log\\\` en \\\`PlayerStats.TakeDamage\\\`:

\\\`\\\`\\\`csharp
public void TakeDamage(float damage) {
    Debug.Log($"[STATS] TakeDamage({damage}) on {ID}");
    // ...
}
\\\`\\\`\\\`

- **Si nunca aparece** → el bug está en el pipeline previo: revisá [TBR-004] (combate roto).
- **Si aparece pero solo en el cliente que daña** → [TBR-011] (HP no es NetworkVariable).

### 2. Cadena de muerte

Dentro de \\\`TakeDamage\\\`:

\\\`\\\`\\\`csharp
currentHP -= damage;
OnDamageTaken?.Invoke();
if (currentHP <= 0) HandleDeath(attackerId);
\\\`\\\`\\\`

### 3. HandleDeath

\\\`\\\`\\\`csharp
void HandleDeath(ulong killerId) {
    if (currentLifes > 1) {
        LoseLife();              // resta vida, dispara respawn (TBR-008)
    } else {
        currentLifes = 0;
        isAlive = false;
        OnAllLifeLost?.Invoke(); // → modo espectador (TBR-009)
    }
}
\\\`\\\`\\\`

### 4. HUD reactivo

En \\\`HUDController.OnEnable\\\`, suscribir:

\\\`\\\`\\\`csharp
stats.OnDamageTaken += UpdateHpBar;
stats.OnLifeLost    += UpdateLives;
stats.OnAllLifeLost += ShowSpectatorHud;
\\\`\\\`\\\`

### 5. Sincronía red

Bloqueado por [TBR-011]: \\\`currentHP\\\` y \\\`currentLifes\\\` deben ser \\\`NetworkVariable\\\` con write-server.

## Cómo verificar

- Console: \\\`stats.TakeDamage(50f)\\\` → log + HUD baja.
- \\\`TakeDamage(9999f)\\\` con \\\`currentLifes=3\\\` → \\\`OnLifeLost\\\` dispara, HUD muestra 2 vidas.
- Drenar todas las vidas → \\\`OnAllLifeLost\\\` se dispara, \\\`isAlive=false\\\`.
`,

'TBR-007': `## Estado actual del código (bug confirmado)

Después de leer el código, encontré **tres problemas** que probablemente causan el síntoma. El equipo confirma que probaron en red real esta semana, así que estos bugs **están vivos en runtime**.

### Problema A — \\\`OwnerClientId == 0\\\` en \\\`GameplayManager.cs:118\\\`

\\\`\\\`\\\`csharp
if (OwnerClientId == 0)  // ← TODOS los clientes pasan
{
    spawnsPoints.Clear(); ...
    AssignPlayersToSpawnPoints();
}
\\\`\\\`\\\`

\\\`GameplayManager\\\` es un objeto de escena. Para un \\\`NetworkBehaviour\\\` colocado en escena, \\\`OwnerClientId\\\` defaultea a 0 (server) **en todos los clientes**. Por lo tanto **cada cliente intenta spawnear** los jugadores. \\\`Spawn()\\\` solo funciona en el server, pero el \\\`Instantiate\\\` previo (línea 183) sí ejecuta y crea **fantasmas locales no networkeados** en cada cliente. Ver [TBR-017].

### Problema B — Doble controller de movimiento

- \\\`PlayerController.cs\\\` (MonoBehaviour, scene-object referenciado en \\\`GameplayManager._playerController\\\`) sí lee input via \\\`OnMove\\\` y llama \\\`_characterController.Move()\\\`.
- \\\`NetworkPlayerController.cs\\\` (NetworkBehaviour, en el prefab spawneado) lee \\\`moveInput\\\` que **nunca se actualiza** (línea 19, no hay listener).

El movimiento real ocurre en \\\`PlayerController\\\` que **no está en el prefab spawneado** sino en una escena. Por eso el transform del NetworkObject del jugador *no se mueve en el owner* y *NetworkTransform no replica* nada.

### Problema C — Posible falta de NetworkTransform

Verificá en cada \\\`SkinInfo.gameplayPrefabs[i]\\\` que tiene \\\`NetworkObject\\\` **Y** \\\`NetworkTransform\\\`. Sin NetworkTransform, la posición no replica aunque el owner sí se mueva.

## Pasos para diagnosticar y resolver

### 1. Reproducir y aislar

- Levantá Host + Cliente.
- En el Host, abrí Hierarchy → ¿ves N player NetworkObjects para N clientes? Si solo ves 1, el spawn falló.
- En el Cliente, ¿ves un fantasma "stuck" en el spawn original? Eso es el Instantiate sin Spawn (Problema A).

### 2. Logs para confirmar

En \\\`AssignPlayersToSpawnPoints\\\` agregá:

\\\`\\\`\\\`csharp
Debug.Log($"[SPAWN] from clientId={NetworkManager.Singleton.LocalClientId}, IsHost={IsHost}, IsServer={IsServer}");
\\\`\\\`\\\`

- Si aparece en cliente NO host → **Problema A confirmado**.

### 3. Fix Problema A (también [TBR-017])

\\\`GameplayManager.cs:118\\\`:

\\\`\\\`\\\`csharp
if (NetworkManager.Singleton.IsServer)  // antes: OwnerClientId == 0
\\\`\\\`\\\`

### 4. Fix Problema B — unificar el controller

Opciones:

- **A (recomendado)**: mover el código de movimiento de \\\`PlayerController\\\` al prefab del player como un único NetworkBehaviour, con gate \\\`if (!IsOwner) return;\\\`. Eliminar \\\`NetworkPlayerController\\\` (duplicado).
- **B**: dejar \\\`PlayerController\\\` como controlador de input local que delega a \\\`NetworkPlayerController\\\` vía referencia tras \\\`OnNetworkSpawn\\\`.

### 5. Verificar prefab

Abrí cada \\\`SkinInfo.gameplayPrefabs[i]\\\` y confirmá que tiene:

- \\\`NetworkObject\\\`
- \\\`NetworkTransform\\\` (modo Owner Authoritative)
- \\\`PlayerController\\\` con InputActionReferences asignados
- \\\`CharacterController\\\`

## Cómo verificar el fix

- 2 PCs en LAN: cada uno ve al otro moviéndose en tiempo real.
- Saltar y caminar en uno se refleja en el otro con latencia mínima.
- Hierarchy muestra exactamente N players para N clientes (no fantasmas).
`,

'TBR-012': `## Estado actual

No hay lógica que detecte fin de partida. La señal \\\`TriggerGameOver\\\` existe en \\\`GameplayManager\\\` (línea 217) pero **nadie la dispara**.

## Pasos

### 1. Suscribirse en server

En \\\`GameplayManager.OnNetworkSpawn\\\` (server-side):

\\\`\\\`\\\`csharp
if (!IsServer) return;
foreach (var c in NetworkManager.Singleton.ConnectedClientsList) {
    var ps = c.PlayerObject.GetComponent<PlayerStatsNet>();
    if (ps != null) ps.OnAllLifeLost += () => CheckLastAlive();
}
\\\`\\\`\\\`

### 2. CheckLastAlive

\\\`\\\`\\\`csharp
void CheckLastAlive() {
    if (!IsServer) return;
    var alive = NetworkManager.Singleton.ConnectedClientsList
        .Select(c => c.PlayerObject?.GetComponent<PlayerStatsNet>())
        .Where(ps => ps != null && ps.isAlive).ToList();
    if (alive.Count == 1) TriggerGameOver(alive[0].ID);
}
\\\`\\\`\\\`

### 3. Timer expirado

En \\\`InGameTimer.OnTimeUp\\\`:

- Ordenar por \\\`killCount\\\` desc.
- Desempate por \\\`currentHP\\\` desc.
- Luego por \\\`wPM\\\` desc.
- El #1 gana → \\\`TriggerGameOver(winner.ID)\\\`.

### 4. Sincronizar el GameOver

\\\`TriggerGameOver\\\` solo en server, replicar con \\\`ClientRpc EndGameClientRpc(string winnerId)\\\` para que todos los clientes cambien al \\\`GameOverState\\\` al mismo tiempo.

## Cómo verificar

- Partida 4P: matá 3 → \\\`TriggerGameOver\\\` se invoca con el ID del sobreviviente.
- Partida con timer corto (10s): al expirar, gana el de más kills.
- Clientes ven la pantalla de fin al mismo tiempo (no solo el host).

## Dependencias

- [TBR-011] sin sync de stats no hay forma fiable de saber quién está vivo.
`,

'TBR-018': `## Estado actual

\\\`GameplayManager.cs:123-126\\\` define los 4 spawn points como \\\`Vector3\\\` literales. El level designer no puede ajustarlos sin tocar código.

## Pasos

### 1. Usar el campo serializado existente

Ya existe \\\`[SerializeField] private Transform[] _spawnPoints;\\\` en línea 51, pero no se usa. Cambiá la lógica de \\\`PopulateSpawnPoint\\\`:

\\\`\\\`\\\`csharp
spawnsPoints.Clear();
foreach (var t in _spawnPoints) spawnsPoints.Add(t.position);
\\\`\\\`\\\`

### 2. En escena

- Crear 4+ GameObjects vacíos como hijos de un nodo \\\`SpawnPoints\\\` en \\\`GameplayScene\\\`.
- Asignar un Gizmo en \\\`OnDrawGizmos\\\` para verlos en la Scene view.
- Arrastrarlos al array \\\`_spawnPoints\\\` del \\\`GameplayManager\\\`.

### 3. Validación

\\\`\\\`\\\`csharp
if (_spawnPoints.Length < NetworkManager.Singleton.ConnectedClientsIds.Count)
    Debug.LogWarning($"[SPAWN] Solo hay {_spawnPoints.Length} puntos para {ConnectedClientsIds.Count} jugadores.");
\\\`\\\`\\\`

### 4. Eliminar literales

Borrá las 4 líneas con \\\`new Vector3(...)\\\` en \\\`PopulateSpawnPoint\\\`.

## Cómo verificar

- Mover un spawn point en escena → al jugar el jugador aparece en la nueva posición.
- Tener 5 jugadores y 4 spawn points → warning en consola, pero al menos los 4 primeros spawnean.
`,

'TBR-021': `## Estado actual

Aún cuando [TBR-013] esté hecho, los eventos del juego **no llaman** a \\\`AudioManager.PlaySFX\\\`. Tampoco se cambia música por escena.

## Pasos

### 1. Mapear eventos a SFX IDs

Convención: \\\`sfx_ui_click\\\`, \\\`sfx_jump\\\`, \\\`sfx_damage\\\`, \\\`sfx_death\\\`, \\\`sfx_spell_cast_{element}\\\`, \\\`sfx_monolith_unlock\\\`, \\\`sfx_countdown_beep\\\`, \\\`sfx_victory\\\`, \\\`sfx_defeat\\\`.

### 2. Cablear eventos del juego

- \\\`PlayerController.Jump\\\` → \\\`AudioManager.PlaySFX("sfx_jump")\\\`.
- \\\`PlayerStats.OnDamageTaken\\\` → \\\`sfx_damage\\\`.
- \\\`OnLifeLost\\\` → \\\`sfx_death\\\`.
- \\\`CastInputController.OnSpellCast\\\` → \\\`sfx_spell_cast_{element}\\\`.
- \\\`MonolithView.TryInteract\\\` → \\\`sfx_monolith_unlock\\\`.

### 3. UI buttons

Agregá un componente genérico \\\`UIClickSound\\\` que escuche \\\`onClick\\\` del Button y dispare el SFX. Asignalo a todos los botones.

### 4. Música por escena

En \\\`SceneLoader.LoadScene\\\` después del fade:

\\\`\\\`\\\`csharp
AudioManager.Instance.ChangeMusic(GetMusicForScene(sceneName));
\\\`\\\`\\\`

### 5. Música por estado de gameplay

- \\\`PlayState.Enter\\\` → cambiar a track de combate.
- \\\`PlayState.Exit\\\` → volver al track de exploration.

### 6. No romper Netcode

Todos los SFX son **client-side**. NO usar ServerRpc para SFX (ya están sincronizados implícitamente por el evento que los dispara via \\\`NetworkVariable.OnValueChanged\\\`).

## Cómo verificar

- Click cualquier botón → SFX de click.
- Recibir daño → SFX de damage.
- Cambio de escena → música cambia con crossfade.

## Dependencias

- [TBR-013] AudioManager implementado.
- [TBR-022] clips importados.
`,

'TBR-022': `## Estado actual

\\\`AudioDataBase\\\` está vacía. El equipo confirma que **no hay clips importados todavía**.

## Pasos

### 1. Compilar lista de SFX necesarios (~25 clips)

- **UI**: click, hover, error, success.
- **Player**: jump, footstep, damage_hit, death, respawn.
- **Combat**: spell_cast por elemento (fuego/agua/aire/tierra/rayo/oscuridad), spell_hit, spell_miss.
- **Monolith**: unlock, charge.
- **Match**: countdown_beep, lobby_join, victory, defeat.

### 2. Conseguir los clips

Libraries libres: **Freesound.org** (CC0/CC-BY), **ZapSplat**, **Kenney.nl** (CC0). Anotar atribución requerida en \\\`Assets/Audio/CREDITS.md\\\`.

### 3. Normalizar loudness

Audacity o ffmpeg \\\`-af loudnorm=I=-23:LRA=7:TP=-2\\\` para que todos estén en ~-23 LUFS.

### 4. Importar a Unity

\\\`Assets/Audio/SFX/\\\`. Settings:

- **Clips cortos**: \\\`Decompress on Load\\\`, \\\`Vorbis\\\` Q5.
- **Música**: \\\`Streaming\\\`, \\\`Vorbis\\\` Q7.

### 5. Crear AudioEntry SOs

Uno por clip. Convención de nombres consistente con los IDs de [TBR-021].

### 6. Llenar AudioDataBase

Drag de todos los AudioEntry al array de la base de datos.

### 7. README

\\\`Assets/Audio/README.md\\\` listando IDs, origen y licencia.

## Cómo verificar

- AudioDataBase tiene ≥25 entries únicos.
- \\\`AudioManager.PlaySFX(id)\\\` reproduce el clip correcto.
- No hay clips obviamente más fuertes que otros.
`,

'TBR-027': `## Estado actual

La app arranca directo en LobbyScene. Sin branding, sin intro — un pitch presencial pierde "wow factor".

## Pasos

### 1. Crear escena Splash

\\\`Assets/Scenes/Splash.unity\\\` con Canvas full-screen: logo del estudio + logo del juego + tagline.

### 2. SplashController.cs

Coroutine secuencial: fade-in studio (1.5s) → fade-in game logo (2s) → cinematic opcional → \\\`SceneLoader.LoadScene("LobbyScene")\\\`.

### 3. Skip rápido

\\\`\\\`\\\`csharp
void Update() {
    if (Input.anyKeyDown) {
        StopAllCoroutines();
        SceneLoader.LoadScene("LobbyScene");
    }
}
\\\`\\\`\\\`

### 4. Build settings

Agregá \\\`Splash.unity\\\` al **índice 0** y empujá \\\`LobbyScene\\\` a 1.

### 5. Cinemática opcional

Timeline de 5-8s con shots de gameplay. \\\`VideoPlayer\\\` con video pre-rendereado o cámaras + Timeline.

### 6. Música

Fade-in música de splash → silencio breve → música del lobby al cargar.

## Cómo verificar

- Build → splash 1.5s → game logo 2s → LobbyScene.
- Cualquier tecla salta directo a LobbyScene.
- Build size manejable (< 200MB).
`,

'TBR-028': `## Estado actual

\\\`LobbyScene\\\` muestra Host/Join + IP. Sin slots visibles, sin indicador de "listo", sin countdown sincronizado al iniciar.

## Pasos

### 1. Slots fijos

4 paneles (\\\`PlayerSlot_0..3\\\`): avatar (color del slot), nombre ([TBR-033]), estado "Esperando" o "Listo ✓".

### 2. Refresh en eventos

\\\`LobbyUIController\\\` suscribe \\\`OnClientConnected/Disconnected\\\` y cambios de \\\`IDController.already.Value\\\`.

### 3. Listo flag

Reusar \\\`already.Value\\\` (NetworkVariable existente). Botón "Listo" en UI lo togglea.

### 4. Botón "Empezar"

Visible solo en host. Habilitado si todos los conectados están \\\`already=true\\\` Y \\\`Count >= 2\\\`.

### 5. Countdown sincronizado

\\\`\\\`\\\`csharp
[ClientRpc] void StartCountdownClientRpc(int s) { /* tick local */ }
[ClientRpc] void AbortCountdownClientRpc() { /* cancel */ }
\\\`\\\`\\\`

Si alguien desmarca "listo" durante el countdown → abort.

### 6. Animaciones

Slot in/out con \\\`UIAnimator\\\` ([TBR-026]).

### 7. Música de lobby

Track distinto al de combate ([TBR-013]/[TBR-021]).

## Cómo verificar

- 2 listos + 1 pendiente → Empezar deshabilitado.
- Todos listos → click → countdown 3-2-1 sincronizado.
- Uno desmarca listo durante countdown → abort en todos.
`,

'TBR-029': `## Estado actual

\\\`LoadingScreenController\\\` usa **checkboxes manuales** (\\\`checkServer\\\`, \\\`checkPlayers\\\`, \\\`checkMap\\\`, \\\`checkLoot\\\`) — son placeholders de debug. La escena \\\`LoadingScreen.unity\\\` está huérfana.

## Pasos

### 1. AsyncOperation real

Reemplazar \\\`ProcesarPaso\\\`:

\\\`\\\`\\\`csharp
IEnumerator LoadAsync(string sceneName) {
    var op = SceneManager.LoadSceneAsync(sceneName);
    op.allowSceneActivation = false;
    while (op.progress < 0.9f) {
        progressBar.value = op.progress;
        yield return null;
    }
    progressBar.value = 1f;
    yield return new WaitForSeconds(0.3f);
    op.allowSceneActivation = true;
}
\\\`\\\`\\\`

### 2. Tip aleatorio

Pool de strings (\\\`tips/\\\`):

\\\`\\\`\\\`csharp
string[] tips = {
    "Mientras más rápido tipees, más daño hacés.",
    "Acc < 30% no daña — pero el cooldown corre igual.",
    // ...
};
tipsText.text = tips[Random.Range(0, tips.Length)];
\\\`\\\`\\\`

### 3. Animación

Spinner girando + tween de la barra (TBR-026).

### 4. Eliminar checkboxes

Borrar \\\`checkServer\\\`, \\\`checkPlayers\\\`, \\\`checkMap\\\`, \\\`checkLoot\\\` y la lógica switch del \\\`ProcesarPaso\\\`.

### 5. Build settings

Agregá \\\`LoadingScreen.unity\\\` al build.

### 6. Hookup

Usar entre LobbyScene y CharacterSelect (carga pesada) o entre CharacterSelect y GameplayScene.

## Cómo verificar

- Carga real → la barra avanza con el progreso de la AsyncOperation.
- Tip cambia cada vez que aparece la pantalla.
- Sin texto hardcoded de "Servidor OK / Jugadores OK".
`,

'TBR-030': `## Estado actual

\\\`GameplayScene\\\` probablemente con iluminación default + skybox procedural standard.

## Pasos

### 1. Skybox

\\\`Assets/Materials/Skybox/\\\`: cubemap HDR (libres en HDRI Haven) o procedural con tweaks de \\\`Sun Size\\\` y \\\`Sky Tint\\\`.

### 2. Iluminación baked

\\\`Window → Rendering → Lighting\\\`:

- Marcar como \\\`Static\\\` todo lo que no se mueve.
- Generar lightmaps. Iteración rápida: bake con resolución baja primero (32-64), final 256-512.

### 3. Post-process volume

Crear \\\`Volume\\\` global con:

- **Bloom** intensidad ~0.5
- **Color Adjustments** warm temperature +10
- **Vignette** intensity 0.2
- **Tonemapping** ACES

### 4. Niebla

\\\`Lighting → Environment → Fog\\\`. Color sutil. \\\`Density 0.01\\\` inicial. Da profundidad sin tapar el combate.

### 5. Coherencia con TBR-024

La paleta debe complementar el cell shading. Coordinar con el lead artistic.

## Cómo verificar

- Capturas before/after lado a lado.
- Frame budget mantiene 60fps.
- Look "vendible" en pitch.

## Dependencias

- [TBR-024] cell shading establece la paleta.
`,

'TBR-038': `## Estado actual

No hay UI para elegir nivel de hechizo en el monolito. \`MonolithController\` tiene la data de los \`SpellData\` pero no se exponen visualmente. \`MonolithController.PopulateSpells\` (orphan en el análisis original) probablemente retoma su uso aquí.

## Pasos

### 1. Verificar la data del monolito

Abrí \`Assets/Features/Environment_and_Interaction/MonolithController.cs\`. Confirmá que tiene un array \`SpellData[] spellsByLevel\` (3 entries) o equivalente. Si no existe:

\`\`\`csharp
[SerializeField] private SpellData[] spellsByLevel = new SpellData[3];
public SpellData GetSpell(int level) => spellsByLevel[Mathf.Clamp(level, 0, 2)];
public Element Element => element;
\`\`\`

### 2. UI del modal

Crear \`MonolithLevelSelectUI.cs\` — Canvas screen-space en \`GameplayScene\`, hijo del HUD root, default \`SetActive(false)\`:

- Header con icono del elemento del monolito.
- 3 botones (Nivel 1, 2, 3) cada uno mostrando \`spellData.spellName\` + tier + icono.
- Hover destaca con tween (depende de [TBR-026]).
- Botón **Cancelar** y listener de \`ESC\`.

### 3. Show / Select / Cancel

\`\`\`csharp
public static MonolithLevelSelectUI Instance;
MonolithController _current;

public void Show(MonolithController monolith) {
    _current = monolith;
    for (int i = 0; i < 3; i++) {
        var s = monolith.GetSpell(i);
        buttons[i].SetData(s, () => OnLevelSelected(s));
    }
    UIAnimator.FadeIn(canvasGroup);
    PlayerController.Instance.NullMoveSpeed(); // bloquea movimiento
}

void OnLevelSelected(SpellData s) {
    UIAnimator.FadeOut(canvasGroup);
    MonolithTypingChallenge.Instance.Begin(_current, s); // → TBR-039
}

public void OnCancel() {
    UIAnimator.FadeOut(canvasGroup);
    PlayerController.Instance.MoveSpeed(); // restaura
}
\`\`\`

### 4. Bloqueo de movimiento

\`PlayerController.NullMoveSpeed()\` mientras el modal está abierto. Restaurar al cerrar.

### 5. ESC cancela

Listener de tecla ESC mientras el modal está visible (binding al InputAction o \`Input.GetKeyDown(KeyCode.Escape)\`).

## Cómo verificar

- Pulsar E sobre monolito → modal aparece centrado con 3 botones.
- Cada botón muestra el spell del nivel correcto del elemento del monolito.
- Hover en botón → highlight con escala 1.05.
- Click → cierra modal y arranca [TBR-039].
- ESC o botón Cancelar → cierra modal sin efecto, monolito sigue disponible.
- Movimiento bloqueado mientras modal está abierto.

## Dependencias

- [TBR-037] el prompt dispara este modal.
- [TBR-026] tweens.
`,

});
