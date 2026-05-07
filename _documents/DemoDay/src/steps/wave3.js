// Wave 3 — 9 tickets: TBR-003, TBR-006, TBR-008, TBR-009, TBR-015, TBR-016, TBR-025, TBR-031, TBR-032
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-003': `## Estado actual

\\\`TargetSystem.cs\\\` está **realmente vacío** (\\\`FindClosestTarget\\\` y \\\`ToggleLockOn\\\` son stubs sin lógica). Lo confirmamos con el equipo.

## Pasos

### 1. API mínima

\\\`\\\`\\\`csharp
public Transform CurrentTarget { get; private set; }
public Transform FindClosestTarget(Vector3 from, float maxRadius);
public void Cycle();   // rota al siguiente vivo
public void Clear();
\\\`\\\`\\\`

### 2. Implementar FindClosestTarget

- Iterar \\\`NetworkManager.Singleton.ConnectedClientsList\\\`.
- Por cada \\\`PlayerObject\\\`, obtener \\\`PlayerStats\\\`.
- **Descartar** al jugador local y a los muertos (\\\`!isAlive\\\`).
- Calcular \\\`Vector3.Distance\\\` y filtrar por \\\`< maxRadius\\\`.
- Retornar el más cercano.

### 3. Lock indicador visual

- Crear prefab \\\`TargetMarker\\\` (anillo o flecha 3D).
- Reparentarlo al \\\`CurrentTarget\\\` con offset Y arriba.
- Removerlo en \\\`Clear()\\\`.

### 4. Hookup en BattleState

\\\`\\\`\\\`csharp
// BattleState.Enter()
targetSystem.CurrentTarget = targetSystem.FindClosestTarget(player.position, 30f);

// BattleState.Exit()
targetSystem.Clear();
\\\`\\\`\\\`

### 5. Retarget reactivo

En \\\`Update\\\` de TargetSystem, si el target murió o salió del radio, re-buscar.

### 6. Tab para ciclar

Bind a tecla Tab → \\\`targetSystem.Cycle()\\\`.

## Cómo verificar

- Entrar a BattleState con un enemigo cerca → marcador visible sobre él.
- Matar al target → marcador salta al siguiente más cercano.
- Pulsar Tab → cambia a otro vivo.
- Salir de BattleState → marcador desaparece.

## Dependencias

- [TBR-005] — necesitamos \\\`isAlive\\\` real para excluir muertos.
- [TBR-011] — sin sync, "vivo" puede ser inconsistente entre clientes.
`,

'TBR-006': `## Estado actual

El prefab del jugador trae un Canvas world-space con su barra de HP. Esto duplica trabajo, complica orden de render y aumenta el costo de NetworkObject por jugador.

## Pasos

### 1. Localizar el Canvas en el prefab

Abrí cada \\\`SkinInfo.gameplayPrefabs[i]\\\` (\\\`Assets/Prefabs/Players/...\\\`). Identificá el Canvas hijo y los componentes Slider/Image/Text que muestran HP.

### 2. Eliminar del prefab

- Borrá el Canvas hijo del prefab.
- Remové referencias en \\\`NetworkPlayerController\\\` o \\\`PlayerController\\\` si las hubiera.

### 3. HUDController es la única fuente de verdad

Debe vivir en el Canvas screen-space de \\\`GameplayScene\\\`. Suscribirse SOLO al \\\`PlayerStats\\\` del jugador local.

### 4. Identificar al jugador local

En \\\`HUDController.Start\\\` (después de OnNetworkSpawn de los players):

\\\`\\\`\\\`csharp
var localPo = NetworkManager.Singleton.LocalClient.PlayerObject;
stats = localPo.GetComponent<PlayerStats>();
\\\`\\\`\\\`

### 5. EnemyLabel sigue en world-space

Pero solo el nombre/ID — sin barra de HP flotante sobre los enemigos.

## Cómo verificar

- En Editor multi-instance, cada cliente solo ve **un** HUD (el suyo).
- No hay barras de HP flotando sobre los enemigos (solo \\\`EnemyLabel\\\`).
- Profiler muestra menos draw calls de UI.

## Dependencias

- [TBR-005] el HUD necesita los eventos de PlayerStats.
- [TBR-011] HP sincronizado para que el HUD del cliente refleje cambios remotos.
`,

'TBR-008': `## Estado actual

\\\`RespawnController\\\` existe pero solo teletransporta al caer del mapa. **No hay UI de muerte ni cámara espectadora del killer**.

## Pasos

### 1. Death event con killer

En \\\`PlayerStats\\\`, en lugar de \\\`Action OnLifeLost\\\`, exponer:

\\\`\\\`\\\`csharp
public Action<ulong> OnLifeLostWithKiller;
// dispatcher:
OnLifeLostWithKiller?.Invoke(lastDamageFromClientId);
\\\`\\\`\\\`

\\\`lastDamageFromClientId\\\` se setea en \\\`TakeDamageServerRpc\\\` desde [TBR-011].

### 2. DeathUI overlay

Crear \\\`DeathUI.cs\\\` con:

\\\`\\\`\\\`csharp
public void Show(string killerName, int respawnSeconds);
public void Hide();
\\\`\\\`\\\`

Usar UIAnimator (TBR-026) para fade-in.

### 3. Cámara al killer

En \\\`CameraController\\\`:

\\\`\\\`\\\`csharp
public void FollowSpectate(Transform target);
public void RestoreLocal();
\\\`\\\`\\\`

Mientras dure el countdown (3-5s), parentar la cámara al killer con offset Y +2, Z -3.

### 4. Invisibilidad temporal

- Desactivar \\\`MeshRenderer\\\` y \\\`CharacterController\\\` del jugador muerto.
- **NO** destruir el NetworkObject — sigue vivo, solo invisible.

### 5. Respawn

Al terminar el countdown:

- Re-habilitar \\\`CharacterController\\\` y \\\`MeshRenderer\\\`.
- Reset \\\`HP\\\` al máximo (vía ServerRpc).
- Teletransportar a un spawn point libre (servidor decide).
- Cámara vuelve al jugador local.

### 6. Killer murió durante la espera

Suscribir \\\`stats.OnAllLifeLost\\\` del target del spectate. Si dispara, saltar a otro vivo.

## Cómo verificar

- Recibir damage letal con vidas > 0 → DeathUI 3s, cámara al asesino.
- Si el killer muere en esos 3s → cámara salta automáticamente.
- Tras 3s: respawn en un spawn point libre, HP full.
- Sin vidas restantes → no respawn, va a [TBR-009].
`,

'TBR-009': `## Estado actual

\\\`PlayerStats.OnAllLifeLost\\\` no está conectado a nada. Cuando un jugador agota sus vidas el sistema no hace nada con él (queda en el mapa o se cae infinitamente).

## Pasos

### 1. Estado spectator

Agregar a \\\`PlayerStats\\\`:

\\\`\\\`\\\`csharp
public bool isSpectating { get; private set; }
\\\`\\\`\\\`

Al \\\`OnAllLifeLost\\\`:

\\\`\\\`\\\`csharp
isSpectating = true;
\\\`\\\`\\\`

### 2. Apagar input y físicas

\\\`\\\`\\\`csharp
GetComponent<PlayerController>().enabled = false;
GetComponent<CharacterController>().enabled = false;
GetComponent<MeshRenderer>().enabled = false;
GetComponent<Collider>().enabled = false;
\\\`\\\`\\\`

### 3. UI de espectador

\\\`SpectatorUI\\\` con texto:

> "Estás muerto · ← → cambiar jugador · Esperando supervivientes"

Y nombre del actual target.

### 4. Cycle target

\\\`SpectatorController.NextAlive()\\\` itera \\\`ConnectedClientsList\\\`, salta los muertos. Cámara hace follow al elegido.

### 5. Auto-jump si target muere

Suscribirse al \\\`OnAllLifeLost\\\` del current target y saltar al siguiente vivo automáticamente.

### 6. Salida del modo

Al \\\`GameOverState.Enter\\\` → todos los espectadores también ven el podio (TBR-032).

## Cómo verificar

- Quemar todas las vidas → \\\`isSpectating=true\\\`, jugador invisible.
- ← → cicla entre los vivos.
- El último vivo → \\\`TriggerGameOver\\\` (TBR-012) y EndGameUI aparece para todos incluido el espectador.

## Dependencias

- [TBR-005] muerte real.
- [TBR-012] el último vivo dispara GameOver.
`,

'TBR-015': `## Estado actual

Si un cliente se desconecta a media partida, su prefab queda vivo. El conteo de "último vivo" puede romperse y dar fin de partida incorrecto.

## Pasos

### 1. Hook al evento

En \\\`GameplayManager.OnNetworkSpawn\\\` (server):

\\\`\\\`\\\`csharp
if (IsServer)
    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
\\\`\\\`\\\`

### 2. OnClientDisconnect

\\\`\\\`\\\`csharp
void OnClientDisconnect(ulong id) {
    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client)) {
        var po = client.PlayerObject;
        if (po != null) {
            var ps = po.GetComponent<PlayerStatsNet>();
            if (ps != null) {
                ps.isAlive = false;
                ps.OnAllLifeLost?.Invoke();
            }
            po.Despawn(true);
        }
    }
    CheckLastAlive(); // de TBR-012
}
\\\`\\\`\\\`

### 3. EndGameUI

Al construir el leaderboard, marcar entries cuyo \\\`PlayerObject == null\\\` como **"Desconectado"**.

### 4. Sin reconexión durante partida

Implementación mínima: en \\\`ConnectionApprovalCallback\\\`, si \\\`stateMachine.currentState is PlayState\\\` → reject con reason "Partida en curso".

## Cómo verificar

- 4 jugadores → uno cierra el juego → quedan 3 en partida y nadie con HP fantasma.
- Quedan 1 → \\\`TriggerGameOver\\\` se dispara con ese sobreviviente.
- EndGameUI lista al desconectado como "Desconectado".

## Dependencias

- [TBR-012] CheckLastAlive ya implementado.
`,

'TBR-016': `## Estado actual

\\\`MonolithSpawn.SpawnMonolith\\\` usa \\\`Instantiate\\\` plano (no \\\`NetworkObject.Spawn\\\`). Cada cliente ve sus propios monolitos no sincronizados — interactuar con uno no afecta a los demás.

## Pasos

### 1. Convertir el prefab

Abrí \\\`Monolith.prefab\\\` y agregá:

- \\\`NetworkObject\\\` (no necesita NetworkTransform si son estáticos).

### 2. Spawn server-side

En \\\`MonolithSpawn.SpawnMonolith\\\`:

\\\`\\\`\\\`csharp
if (!NetworkManager.Singleton.IsServer) return;
var go = Instantiate(monolithPrefab, pos, Quaternion.identity);
go.GetComponent<NetworkObject>().Spawn();
\\\`\\\`\\\`

### 3. Estado sincronizado

\\\`MonolithController\\\`:

\\\`\\\`\\\`csharp
public NetworkVariable<bool> isExhausted =
    new(false, default, NetworkVariableWritePermission.Server);
\\\`\\\`\\\`

### 4. TryInteract como ServerRpc

\\\`\\\`\\\`csharp
[ServerRpc(RequireOwnership = false)]
void InteractServerRpc(ulong requesterId) {
    if (isExhausted.Value) return;
    GiveSpellToPlayer(requesterId, mySpell);
    isExhausted.Value = true;
    OnExhaustedClientRpc();
}
\\\`\\\`\\\`

### 5. Cooldown / respawn

Opcional: timer server-side que setea \\\`isExhausted = false\\\` tras N segundos. O respawn en otra posición vía \\\`MonolithSpawn.SpawnMonolith\\\` cuando se agota.

## Cómo verificar

- 2 clientes ven los mismos N monolitos en mismas posiciones.
- Cliente A interactúa → ambos ven el monolito agotado.
- Cliente B intenta interactuar al mismo → es rechazado.

## Dependencias

- [TBR-007] necesitamos red real funcional antes.
`,

'TBR-025': `## Estado actual

\\\`SkinInfo.gameplayPrefabs\\\` apunta a placeholders (cápsulas/cubos). Modelos finales no integrados.

## Pasos

### 1. Requirements

- 4 personajes × 4 colores = **16 prefabs**.
- Cada uno con: mesh + rig + materials toon ([TBR-024]) + Animator (\\\`Idle / Run / Jump / Casting / Hit / Death\\\`).

### 2. Importar modelos

\\\`Assets/Models/Characters/{Name}/\\\`. \\\`Rig: Humanoid\\\` (recomendado) o \\\`Generic\\\`. \\\`Read/Write Enabled: off\\\`.

### 3. Animator Controller compartido

\\\`PlayerAnimator.controller\\\` único. Parámetros: \\\`IsMoving\\\` (bool), \\\`Speed\\\` (float), \\\`Casting\\\` (trigger), \\\`Hit\\\` (trigger), \\\`Death\\\` (bool).

### 4. Crear prefabs

Partir del prefab actual y reemplazar el visual. **Mantener**: \\\`NetworkObject\\\`, \\\`NetworkTransform\\\`, \\\`PlayerController\\\`, \\\`CharacterController\\\` con misma escala.

### 5. Asignar a SkinInfo

Actualizar los 16 slots del array \\\`gameplayPrefabs\\\`.

### 6. Test en red

[TBR-007] resuelto antes. 4 clientes con skins distintas → todos ven a todos.

## Cómo verificar

- Spawnear los 4 skins → look consistente.
- Casting trigger correcto en cada uno.
- No hay clipping de manos en idle.
- Network sync sigue funcionando.
`,

'TBR-031': `## Estado actual

Sin feedback visual al recibir o dar daño. Combate se siente "blando".

## Pasos

### 1. CameraController.Shake

\\\`\\\`\\\`csharp
public void Shake(float intensity, float duration) {
    StartCoroutine(ShakeRoutine(intensity, duration));
}
IEnumerator ShakeRoutine(float intensity, float duration) {
    var orig = transform.localPosition;
    var t = 0f;
    while (t < duration) {
        transform.localPosition = orig + Random.insideUnitSphere * intensity;
        t += Time.deltaTime; yield return null;
    }
    transform.localPosition = orig;
}
\\\`\\\`\\\`

### 2. Flash rojo

Overlay \\\`Image\\\` rojo en \\\`HUDController\\\` con alpha tween (DOTween). Suscribir a \\\`OnDamageTaken\\\` y disparar:

\\\`\\\`\\\`csharp
flashImg.color = new Color(1, 0, 0, 0.4f);
flashImg.DOFade(0, 0.2f);
\\\`\\\`\\\`

### 3. Hit-stop

\\\`Time.timeScale = 0.05f\\\` durante 30ms y restaurar:

\\\`\\\`\\\`csharp
Time.timeScale = 0.05f;
yield return new WaitForSecondsRealtime(0.03f);
Time.timeScale = 1f;
\\\`\\\`\\\`

NO afecta a Netcode (usa NetworkTime), pero sí pausa partículas y animaciones locales — eso es lo que querés.

### 4. Feedback al impactar al enemigo

Solo para el caster. Al confirmar daño, vibración del crosshair (scale tween rápido).

### 5. No bloquear input

Todo el feedback dura < 200ms para no romper el flow.

## Cómo verificar

- Recibir daño → shake + flash rojo + 30ms de pausa.
- Pegar a enemigo → crosshair vibra.
- Recibir 5 hits seguidos → no se acumulan flashes infinitos.

## Dependencias

- [TBR-005] muerte/daño funcional.
- [TBR-026] DOTween.
`,

'TBR-032': `## Estado actual

\\\`EndGameUI\\\` muestra panel plano con winner ID y stats. Sin impacto visual para pitch.

## Pasos

### 1. Sub-canvas o escena del podio

Sub-canvas dentro de GameplayScene activado por \\\`GameOverState.Enter\\\`. O escena adicional con \\\`AdditiveLoad\\\`.

### 2. Tres pedestales 3D

Modelo estilo: 1ro centro alto, 2do izquierda, 3ro derecha, 4to atrás bajo.

### 3. Posicionar a los players

Al \\\`GameOverState.Enter\\\`:

\\\`\\\`\\\`csharp
var ranked = stats.OrderByDescending(s => s.killCount).ToList();
for (int i = 0; i < Mathf.Min(4, ranked.Count); i++)
    SpawnAtPedestal(ranked[i], i);
\\\`\\\`\\\`

### 4. Animación de entrada

Pedestales suben desde abajo en cascada con DOTween. Delay de 0.2s entre cada uno.

### 5. Confetti

\\\`ParticleSystem confetti\\\` sobre el pedestal #1.

### 6. Stats overlay

Debajo de cada pedestal: kills, damage, WPM medio.

### 7. Música de victoria

Track distinto al de gameplay ([TBR-013]).

## Cómo verificar

- Terminar partida → cinemática del podio.
- Ganador clarísimamente más alto y central.
- Confetti visible.
- Música cambia.

## Dependencias

- [TBR-012] TriggerGameOver decide al ganador.
- [TBR-026] DOTween.
`,

});
