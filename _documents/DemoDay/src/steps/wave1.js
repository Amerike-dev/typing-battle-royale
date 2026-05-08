// Wave 1 — 10 tickets: TBR-010, TBR-011, TBR-013, TBR-014, TBR-017, TBR-020, TBR-024, TBR-026, TBR-033, TBR-037
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-010': `## Estado actual (bug confirmado)

\\\`GameOverState.cs:21, 28, 38\\\` accede a \\\`NetworkManagerMock.Instance.Controllers\\\`. \\\`NetworkManagerMock\\\` es para \\\`Debug.unity\\\` y **Netcode no expone \\\`.Controllers\\\`**, por lo que en partida real → \\\`NullReferenceException\\\` al terminar.

\\\`GameplayManager.SetupSpawns()\\\` (líneas 199-215, comentado) también referenciaba \\\`NetworkManagerMock\\\`. Está commented-out pero debe limpiarse para no confundir.

## Pasos

### 1. Reproducir el crash

En partida real, terminá la partida (timeout o último vivo). Esperás un \\\`NRE\\\` apuntando a \\\`GameOverState.cs\\\` línea 21, 28 o 38.

### 2. Identificar lo que necesita

\\\`GameOverState\\\` usa \\\`Controllers\\\` para:

- Listar \\\`PlayerStats\\\` y mostrar el leaderboard.
- Apagar el \\\`PlayerController\\\` de cada uno.

### 3. Reemplazar la fuente de datos

\\\`\\\`\\\`csharp
List<PlayerStats> stats = new();
foreach (var c in NetworkManager.Singleton.ConnectedClientsList) {
    if (c.PlayerObject == null) continue;
    var ps = c.PlayerObject.GetComponent<PlayerStats>();
    if (ps != null) stats.Add(ps);
    var pc = c.PlayerObject.GetComponent<PlayerController>();
    if (pc != null) pc.enabled = false;
}
\\\`\\\`\\\`

### 4. Eliminar la dependencia

Buscá con grep \\\`NetworkManagerMock\\\` por todo el proyecto. Eliminá referencias **fuera** de \\\`Debug.unity\\\`. El archivo en sí puede quedar solo para esa escena de debug, o eliminarse en [TBR-019].

### 5. Limpiar código comentado

Borrá \\\`GameplayManager.SetupSpawns\\\` líneas 199-215 — no aporta y confunde.

## Cómo verificar

- Partida real con 2 jugadores → uno muere de timeout o por last-alive → EndGameUI aparece **sin NRE**.
- Console **limpia** después de \\\`TriggerGameOver\\\`.
- \\\`grep "NetworkManagerMock"\\\` solo devuelve \\\`Debug.unity\\\` y el propio script.
`,

'TBR-011': `## Estado actual

\\\`PlayerStats\\\` es un POCO local (\\\`public class PlayerStats\\\` en \\\`Assets/Data\\\`). \\\`TakeDamage\\\` modifica HP solo en el cliente que lo llama. **Otros clientes no ven los cambios**. Es la causa raíz de que el daño no funcione en multijugador.

## Pasos

### 1. Convertir a NetworkBehaviour

Opción A: hacer que \\\`PlayerStats\\\` extienda \\\`NetworkBehaviour\\\`. Opción B (recomendada): crear \\\`PlayerStatsNet\\\` separado y dejar \\\`PlayerStats\\\` como wrapper local.

\\\`\\\`\\\`csharp
public class PlayerStatsNet : NetworkBehaviour {
    public NetworkVariable<float> currentHP    = new(100f, default, NetworkVariableWritePermission.Server);
    public NetworkVariable<int>   currentLifes = new(3,    default, NetworkVariableWritePermission.Server);
    public NetworkVariable<int>   killCount    = new(0,    default, NetworkVariableWritePermission.Server);
}
\\\`\\\`\\\`

### 2. TakeDamage como ServerRpc

\\\`\\\`\\\`csharp
[ServerRpc(RequireOwnership = false)]
public void TakeDamageServerRpc(float dmg, ulong attackerId) {
    if (currentHP.Value <= 0) return;
    currentHP.Value = Mathf.Max(0, currentHP.Value - dmg);
    if (currentHP.Value <= 0) HandleDeathServer(attackerId);
}
\\\`\\\`\\\`

### 3. HUD reactivo

\\\`\\\`\\\`csharp
currentHP.OnValueChanged += (_, n) => UpdateBar(n);
\\\`\\\`\\\`

en \\\`HUDController\\\` o \\\`PlayerStats\\\` (si fuiste opción A).

### 4. Kill count

Al confirmar muerte server-side:

\\\`\\\`\\\`csharp
var attackerStats = NetworkManager.Singleton.ConnectedClients[attackerId]
    .PlayerObject.GetComponent<PlayerStatsNet>();
attackerStats.killCount.Value++;
\\\`\\\`\\\`

### 5. Test multi-instance

Abrí 2 instances en Editor + ParrelSync (o build standalone + Editor). Dañá a uno desde el otro y verificá que \\\`currentHP\\\` baja en **ambas** pantallas.

## Cómo verificar

- 2 clientes: A daña a B → ambos ven HP bajar al mismo tiempo.
- killCount incrementa solo en el atacante.
- Reconexión muestra los valores actuales (NetworkVariable persiste server-side).
`,

'TBR-013': `## Estado actual

\\\`AudioManager\\\` es singleton pero \\\`PlaySFX\\\`, \\\`ChangeMusic\\\`, \\\`SetVolume\\\` están **vacíos**. \\\`MusicController.CrossfadeTo\\\` también vacío. \\\`AudioSettings.Save/Load\\\` vacíos. El equipo confirma que tampoco hay clips importados.

## Pasos

### 1. PlaySFX

\\\`\\\`\\\`csharp
public void PlaySFX(string id) {
    var entry = _db.GetEntry(id);
    if (entry == null) { Debug.LogWarning($"[AUDIO] SFX missing: {id}"); return; }
    var src = GetFreeSource();
    src.clip = entry.clip;
    src.volume = entry.volume * sfxVolume * masterVolume;
    src.pitch = entry.randomizePitch ? Random.Range(0.95f, 1.05f) : 1f;
    src.Play();
}
\\\`\\\`\\\`

### 2. Pool de AudioSources

\\\`sfxPool\\\` ya existe; dimensionarlo a 8-16. Round-robin para evitar crear/destruir.

\\\`\\\`\\\`csharp
AudioSource GetFreeSource() {
    foreach (var s in sfxPool) if (!s.isPlaying) return s;
    return sfxPool[Time.frameCount % sfxPool.Count]; // fallback
}
\\\`\\\`\\\`

### 3. ChangeMusic con crossfade

\\\`\\\`\\\`csharp
public void ChangeMusic(string id, float duration = 0.5f) {
    var entry = _db.GetEntry(id);
    StartCoroutine(_music.CrossfadeTo(entry.clip, duration));
}
\\\`\\\`\\\`

### 4. SetVolume con canales

\\\`\\\`\\\`csharp
public void SetVolume(string channel, float v) {
    switch(channel) {
        case "master": masterVolume = v; break;
        case "music":  musicVolume  = v; break;
        case "sfx":    sfxVolume    = v; break;
    }
    PlayerPrefs.SetFloat($"vol.{channel}", v);
}
\\\`\\\`\\\`

### 5. Persistencia

En \\\`AudioManager.Awake\\\`:

\\\`\\\`\\\`csharp
masterVolume = PlayerPrefs.GetFloat("vol.master", 1f);
musicVolume  = PlayerPrefs.GetFloat("vol.music",  0.7f);
sfxVolume    = PlayerPrefs.GetFloat("vol.sfx",    1f);
\\\`\\\`\\\`

### 6. AudioDataBase.GetEntry

Agregá un método de búsqueda por nombre dentro del SO.

## Cómo verificar

- \\\`AudioManager.Instance.PlaySFX("ui_click")\\\` en consola → suena.
- \\\`ChangeMusic\\\` entre dos clips → fade suave de 0.5s.
- Cambiar slider master → afecta a todo el audio.
- Cerrar y reabrir el juego → volumen persiste.

## Dependencias

- [TBR-022] necesario para tener clips reales.
`,

'TBR-014': `## Estado actual

\\\`LobbyController\\\` acepta cualquier número de clientes. No hay UI que muestre quién está conectado.

## Pasos

### 1. ConnectionApprovalCallback

Antes de \\\`StartHost()\\\` en \\\`LobbyController.OnHostButtonClicked\\\`:

\\\`\\\`\\\`csharp
NetworkManager.Singleton.ConnectionApprovalCallback = (req, res) => {
    if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 4) {
        res.Approved = false;
        res.Reason = "Sala llena (4/4)";
        return;
    }
    res.Approved = true;
    res.CreatePlayerObject = true;
};
\\\`\\\`\\\`

### 2. Lista en vivo

Suscribir \\\`OnClientConnectedCallback\\\` y \\\`OnClientDisconnectCallback\\\` para refrescar la UI:

\\\`\\\`\\\`csharp
NetworkManager.Singleton.OnClientConnectedCallback += _ => RefreshSlots();
NetworkManager.Singleton.OnClientDisconnectCallback += _ => RefreshSlots();
\\\`\\\`\\\`

### 3. UI con 4 slots

Diseñar 4 paneles fijos en \\\`LobbyUIController\\\`. Cada uno muestra:

- "Esperando…" si el slot está vacío.
- Avatar (color del slot 0/1/2/3) + nombre si está conectado.

### 4. Botón "Empezar"

Visible solo si \\\`IsHost\\\`. Habilitado si \\\`ConnectedClients.Count >= 2\\\`.

### 5. Cliente rechazado

Manejar \\\`ConnectionApprovalResponse.Reason\\\` en cliente y mostrar dialog "Sala llena".

## Cómo verificar

- 5to cliente intenta conectarse → ve mensaje "Sala llena".
- UI del host muestra slots ocupados en tiempo real.
- Un cliente desconecta → su slot vuelve a "Esperando…".
`,

'TBR-017': `## Estado actual (bug confirmado en código)

\\\`GameplayManager.cs:118\\\`:

\\\`\\\`\\\`csharp
if (OwnerClientId == 0)  // ← TODOS los clientes pasan
\\\`\\\`\\\`

\\\`OwnerClientId\\\` para un \\\`NetworkBehaviour\\\` de escena es siempre 0 (el server). Los clientes no-host **también** pasan este check y rompen el spawn (raíz de [TBR-007]).

Adicional: el cliente espera \\\`WaitForSeconds(5f)\\\` ciegos sin saber si el host realmente terminó.

## Pasos

### 1. Cambiar el check

\\\`GameplayManager.cs:118\\\`:

\\\`\\\`\\\`csharp
if (NetworkManager.Singleton.IsServer) {
    spawnsPoints.Clear();
    // ...
    AssignPlayersToSpawnPoints();
    NotifySpawnDoneClientRpc();
} else {
    while (!_allSpawned) yield return null;
}
\\\`\\\`\\\`

### 2. Eliminar la espera ciega

En vez de \\\`yield return new WaitForSeconds(5f);\\\`:

\\\`\\\`\\\`csharp
[ClientRpc]
void NotifySpawnDoneClientRpc() => _allSpawned = true;
\\\`\\\`\\\`

Cliente espera el flag.

### 3. Buscar otros checks similares

\\\`grep -r "OwnerClientId == 0" Assets/\\\` y reemplazar por \\\`IsHost\\\` / \\\`IsServer\\\` según corresponda.

## Cómo verificar

- Logs de \\\`AssignPlayersToSpawnPoints\\\` aparecen **solo en el host**.
- Cliente no muestra "Espera unos 5 segundos" en consola.
- Latencia alta (1s+): el cliente sigue viendo todos los spawns sin race.
`,

'TBR-020': `## Estado actual

\\\`EndGameUI\\\` y \\\`PauseController\\\` cargan \\\`"MainMenu"\\\` por nombre. **Esa escena no está en build** (\\\`EditorBuildSettings.asset\\\` solo tiene \\\`LobbyScene\\\`/\\\`CharacterSelect\\\`/\\\`GameplayScene\\\`). El botón fallará en build de producción.

## Pasos

### Decidir entre dos opciones

#### Opción A — Reactivar MainMenu como entry point

- Agregar \\\`MainMenu.unity\\\` al build settings (índice 0).
- Mover \\\`LobbyScene\\\` al índice 1.
- \\\`MainMenuController.PlayGame()\\\` carga \\\`"LobbyScene"\\\` (no "CharacterSelect" como hoy).

#### Opción B (recomendada) — Usar LobbyScene como menú principal

- En \\\`EndGameUI\\\` y \\\`PauseController\\\`, reemplazar \\\`SceneLoader.LoadScene("MainMenu")\\\` por \\\`SceneLoader.LoadScene("LobbyScene")\\\`.
- Eliminar \\\`MainMenu.unity\\\` (en [TBR-019]).

### Pasos finales (cualquier opción)

1. Documentar la decisión en \\\`Specs.md\\\`.
2. Aplicar el cambio en \\\`EndGameUI.OnMainMenuButton\\\` y \\\`PauseController.SceneMenu\\\`.
3. Test en build de Windows.

## Cómo verificar

- Build de Windows.
- Iniciar partida → terminar → click "Volver al menú" → no falla.
- Pausa → "Volver al menú" → no falla.
`,

'TBR-024': `## Estado actual

Materiales actuales son standard PBR (con cápsulas placeholder). El equipo quiere look toon.

## Pasos

### 1. Decisión URP vs Built-in

Verificá que use **URP** (\\\`Assets/Settings/*.asset\\\`). Si no, considerá migrar antes (riesgo de romper materiales).

### 2. Shader toon

- **A** (recomendado): asset gratuito \\\`URP Toon Shader\\\`.
- **B**: custom Shader Graph con \\\`Step\\\` node sobre NDotL.

### 3. Material toon base

Aplicar a personajes y monolitos. 2-3 bandas (dark/mid/bright). Specular highlight opcional.

### 4. Outline

- **Inverted Hull**: mesh duplicado con normal-flip y color negro. Simple pero coste extra.
- **Post-process Sobel**: edge detection en \\\`Renderer Feature\\\`. Más caro, uniforme.

### 5. Probar luces

Directional + 1-2 point lights. Las bandas deben verse correctas en distintas posiciones de cámara.

### 6. Comparar costo

Frame Debugger antes y después. Si sube > 30%, optimizá.

## Cómo verificar

- Personajes y monolitos con bandas claras.
- Outlines visibles desde todo ángulo.
- Performance similar al estado pre-shader.
`,

'TBR-026': `## Estado actual

Toda la UI usa \\\`SetActive\\\` directo — sin transiciones suaves.

## Pasos

### 1. Importar DOTween

\\\`DOTween\\\` (free) desde Asset Store. \\\`Tools → Demigiant → DOTween Setup\\\`.

### 2. Helper UIAnimator

\\\`\\\`\\\`csharp
public static class UIAnimator {
    public static void FadeIn(CanvasGroup g, float dur = 0.25f) {
        g.alpha = 0; g.gameObject.SetActive(true);
        g.DOFade(1, dur);
    }
    public static void FadeOut(CanvasGroup g, float dur = 0.2f) {
        g.DOFade(0, dur).OnComplete(() => g.gameObject.SetActive(false));
    }
    public static void Pop(Transform t, float scale = 1.1f, float dur = 0.15f) {
        t.localScale = Vector3.one;
        t.DOScale(scale, dur).SetLoops(2, LoopType.Yoyo);
    }
}
\\\`\\\`\\\`

### 3. Reemplazar SetActive

En \\\`PauseController\\\`, \\\`EndGameUI\\\`, \\\`SpellBookUI\\\`: usar \\\`UIAnimator.FadeIn/Out\\\`.

### 4. Countdown 3-2-1

En \\\`WaitingState.CountdownRoutine\\\`, agregar \\\`UIAnimator.Pop(text.transform)\\\` en cada número.

### 5. HUD intro

Slide del HUD desde abajo al iniciar partida (\\\`DOAnchorPosY\\\` con \\\`Ease.OutBack\\\`).

### 6. Hover en botones

Componente \\\`UIHoverScale\\\` que aplique \\\`Pop\\\` al \\\`PointerEnter\\\`.

### 7. Performance

Max ~30 tweens activos. Profiler: < 0.1ms/frame.

## Cómo verificar

- Pause → fade-in suave.
- Countdown con escala pop.
- HUD entra animado al inicio.
`,

'TBR-033': `## Estado actual

Los jugadores se identifican por \\\`OwnerClientId\\\` (un número) y \\\`PlayerIDGenerator\\\` ("WIZ-XXXX"). No se puede personalizar.

## Pasos

### 1. InputField en LobbyScene

\\\`TMP_InputField\\\` con label "Tu nombre". Validación en \\\`onValueChanged\\\`: 3-16 chars, sin whitespace solo.

### 2. Persistencia local

Al cambiar:

\\\`\\\`\\\`csharp
PlayerPrefs.SetString("playerName", value);
PlayerPrefs.Save();
\\\`\\\`\\\`

Al \\\`Start\\\`, cargar:

\\\`\\\`\\\`csharp
input.text = PlayerPrefs.GetString("playerName", "");
\\\`\\\`\\\`

### 3. Sync por red

Agregar a \\\`NetworkPlayerController\\\`:

\\\`\\\`\\\`csharp
public NetworkVariable<FixedString64Bytes> playerName = new("",
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
\\\`\\\`\\\`

### 4. Setear al spawn

En \\\`OnNetworkSpawn\\\` (cliente owner):

\\\`\\\`\\\`csharp
if (IsOwner)
    playerName.Value = PlayerPrefs.GetString("playerName", "Wizard");
\\\`\\\`\\\`

### 5. Mostrar en EnemyLabel y EndGameUI

Leer \\\`net.playerName.Value.ToString()\\\` en lugar de \\\`OwnerClientId.ToString()\\\`.

### 6. Fallback

Si vacío después de leer PlayerPrefs, usar \\\`PlayerIDGenerator.NewID()\\\`.

## Cómo verificar

- Escribir nombre → jugar partida → enemigos ven ese nombre sobre el avatar.
- Cerrar app, volver → el nombre persiste.
- EndGameUI muestra nombres custom en el leaderboard.

## Dependencias

- [TBR-014] lobby con UI lista para mostrar.
`,

'TBR-037': `## Estado actual

No existe prompt visual de proximidad a monolitos. \`PlayerInteractorView\` ya detecta \`NearestMonolith\` por triggers, pero no hay UI ni listener de tecla "E".

## Pasos

### 1. Verificar el detector existente

Abrí \`Assets/Features/Environment_and_Interaction/PlayerInteractorView.cs\`. Confirmá que \`NearestMonolith\` se setea/limpia en \`OnTriggerEnter/Exit\` con el tag o layer correcto.

Poné un \`Debug.Log\` temporal en \`OnTriggerEnter\` para verificar que dispara cuando te acercás a un monolito.

- **Si nunca dispara** → revisá el Collider (es Trigger?), el LayerMask del monolito, y que \`MonolithView\` tenga el componente correcto.
- **Si dispara pero \`NearestMonolith\` queda null** → revisá la asignación dentro del handler.

### 2. UI del prompt

Crear \`MonolithPromptUI.cs\`. Diseño: un \`Canvas\` world-space hijo del prefab del monolito, oculto por default. Texto: **"Pulsa E"**.

En \`MonolithView\`:

\`\`\`csharp
[SerializeField] private GameObject promptCanvas;
public void ShowPrompt(bool show) {
    if (promptCanvas != null) promptCanvas.SetActive(show);
}

void LateUpdate() {
    if (promptCanvas != null && promptCanvas.activeSelf)
        promptCanvas.transform.LookAt(Camera.main.transform);
}
\`\`\`

### 3. Sincronizar con NearestMonolith

En \`PlayerInteractorView\` (cliente local):

\`\`\`csharp
MonolithView _shownPrompt;
void Update() {
    if (NearestMonolith == _shownPrompt) return;
    _shownPrompt?.ShowPrompt(false);
    NearestMonolith?.ShowPrompt(true);
    _shownPrompt = NearestMonolith;
}
\`\`\`

### 4. Listener de "E"

En \`PlayerController\` (o un nuevo \`MonolithInteractInput\`), agregá un \`InputActionReference\` para "E":

\`\`\`csharp
public InputActionReference interactAction;

void OnEnable() { interactAction.action.started += OnInteract; interactAction.action.Enable(); }
void OnDisable() { interactAction.action.started -= OnInteract; interactAction.action.Disable(); }

void OnInteract(InputAction.CallbackContext ctx) {
    var mono = interactorView.NearestMonolith;
    if (mono == null || mono.IsExhausted) return;
    MonolithLevelSelectUI.Instance.Show(mono); // → TBR-038
}
\`\`\`

### 5. Estado "agotado"

Si el monolito está exhausted ([TBR-016]), el prompt no aparece. Alternativa: aparece en gris con texto "Agotado".

## Cómo verificar

- Acercarse a monolito → texto "Pulsa E" flotando sobre él, mira a la cámara.
- Alejarse → texto desaparece.
- 2 monolitos cercanos → solo el más cercano muestra prompt.
- Pulsar E con prompt visible → modal de selección abre ([TBR-038]).
- Monolito exhausted → no hay prompt o muestra "Agotado".
`,

});
