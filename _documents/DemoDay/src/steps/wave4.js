// Wave 4 — 6 tickets: TBR-004, TBR-034, TBR-035, TBR-039, TBR-040, TBR-041
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-004': `## Estado actual

\\\`OnSpellCast\\\` se dispara pero solo loguea \\\`"GameplayManager escucho el evento"\\\` (\\\`GameplayManager.cs:103\\\`). **No hay damage pipeline conectado**. \\\`DamageCalculator.CalculateDamage\\\` está definido pero **nunca se invoca**. \\\`SpellData\\\` no tiene campos de VFX/anim asignados.

## Pasos

### 1. Extender SpellData

Agregar campos:

\\\`\\\`\\\`csharp
public GameObject vfxCast;       // partículas en mano
public GameObject vfxProjectile; // proyectil teledirigido
public GameObject vfxHit;        // impacto
public float baseDamage;
public float cooldown;
\\\`\\\`\\\`

### 2. Damage pipeline

En \\\`HandleOnSpellCast\\\` de \\\`GameplayManager\\\`:

\\\`\\\`\\\`csharp
private void HandleOnSpellCast() {
    var acc = _castInputController.accuracy;
    var mult = TypingStats.GetDamageBonusMultiplier(acc);
    var dmg = currentSpell.baseDamage * mult;

    var target = targetSystem.CurrentTarget;
    if (target == null) return;

    var ps = target.GetComponent<PlayerStats>();
    ps.TakeDamageServerRpc(dmg, NetworkManager.Singleton.LocalClientId);
}
\\\`\\\`\\\`

Bloqueado por [TBR-011] (TakeDamage como ServerRpc).

### 3. VFX en cliente

\\\`ClientRpc PlaySpellVFXClientRpc(int spellId, Vector3 casterPos, ulong targetId)\\\`:

- Spawn \\\`vfxCast\\\` en mano del caster.
- Spawn \\\`vfxProjectile\\\` con tween hacia target (mira guiada — siempre acierta).
- Al llegar al target: spawn \\\`vfxHit\\\`.

### 4. Cooldown

Server-side: \\\`Dictionary<ulong, Dictionary<int, float>> lastCast\\\`. Bloquear cast si \\\`Time.time - last < cooldown\\\`.

### 5. Acc < 30 % aún reproduce VFX

Multiplier 0× pero igual reproducir VFX para feedback de "fallaste".

## Cómo verificar

- Castear un spell → VFX de mano + proyectil teledirigido + impacto.
- Console muestra \\\`Damage = base * 1.1\\\` con accuracy alta.
- Castear con accuracy baja → VFX se ve pero target no pierde HP.
- Cooldown impide doble cast inmediato.

## Dependencias

- [TBR-003] target lockeado.
- [TBR-011] HP sincronizado.
- [TBR-023] VFX assets reales.
`,

'TBR-034': `## Estado actual

Tras GameOver no se muestran stats personales del jugador.

## Pasos

### 1. Recolectar stats

Extender \\\`PlayerStats\\\` con campos agregados durante la partida:

\\\`\\\`\\\`csharp
public NetworkVariable<float> damageDealt;
public NetworkVariable<float> damageTaken;
public NetworkVariable<int>   spellsCast;
public float avgWpm;        // running average local
public float avgAccuracy;   // running average local
public float fastestCastSeconds;
\\\`\\\`\\\`

### 2. Pantalla personal

Después del podio (TBR-032), un panel "Tus stats" con los 7 valores.

### 3. Count-up animation

Números que suben de 0 al valor real con DOTween:

\\\`\\\`\\\`csharp
DOTween.To(() => 0, v => text.text = v.ToString(), finalValue, 1.5f);
\\\`\\\`\\\`

### 4. Reset

Al cargar GameplayScene de nuevo, resetear stats.

### 5. Botón Continuar

Al menú principal o reload de la partida.

## Cómo verificar

- Ganar/perder → ver tus stats personales con animación de count-up.
- Reload → stats reseteados.

## Dependencias

- [TBR-032] podio antes.
- [TBR-026] DOTween.
`,

'TBR-035': `## Estado actual

Sin attract mode. En pitch presencial el juego queda inactivo cuando nadie juega — pierde atención.

## Pasos

### 1. AttractModeController

En LobbyScene, contador de \\\`Time.unscaledTime - lastInputTime\\\`. Si > 30s sin input, dispara modo demo.

### 2. Bots

\\\`DemoBotController\\\` con BehaviourTree mínimo:

- **Idle** → buscar monolito → moverse → interact.
- **CastSpell** → seleccionar spell aleatorio del inventario → \\\`fakeAccuracy\\\` 70-95% → trigger pipeline de daño.
- **MoveToEnemy** → wandering hacia el más cercano.
- **Death** → respawn cuando aplique.

### 3. Single-host setup

Cuando arranca demo, el host arranca solo (\\\`StartHost\\\`) y spawnea **4 bots** como NetworkObjects "owned by server".

### 4. Cualquier input aborta

\\\`Update\\\` global checa \\\`Input.anyKey\\\` o cualquier botón → aborta demo y vuelve a Lobby.

### 5. Loop

Al terminar partida demo, vuelve a Lobby y reinicia el contador.

## Cómo verificar

- Dejar el juego en LobbyScene 30s sin tocar → arranca demo.
- Pulsar cualquier tecla → vuelve a Lobby.
- Demo no consume CPU/GPU excesivos.

## Dependencias

- [TBR-005] muerte real.
- [TBR-009] espectador (cuando todos los bots mueren).
`,

'TBR-039': `## Estado actual

No existe el typing challenge para monolitos. \`CastInputController\` solo se usa en BattleState ([TBR-002]). \`PlayerInventory.AddSpell\` existe pero ningún caller real lo invoca.

## Pasos

### 1. Decidir: instancia separada o reusar

**Opción A (recomendada)**: instancia separada de \`CastInputController\` (\`MonolithTypingController\`) para no mezclar contextos con BattleState.

**Opción B**: el mismo \`CastInputController\` con un flag de modo (\`combat\` / \`unlock\`).

### 2. MonolithTypingChallenge.cs (Opción A)

\`\`\`csharp
public static MonolithTypingChallenge Instance;
MonolithController _monolith;
SpellData _spell;

public void Begin(MonolithController monolith, SpellData spell) {
    _monolith = monolith; _spell = spell;
    typingInput.spellText = spell.runeChallenge;
    typingInput.OnSpellCast += OnComplete;
    PlayerController.Instance.NullMoveSpeed();
    panel.SetActive(true);
    typingInput.gameObject.SetActive(true);
    typingInput.inputField.ActivateInputField();
}

void OnComplete() {
    typingInput.OnSpellCast -= OnComplete;
    PlayerController.Instance.MoveSpeed();
    UnlockSpellServerRpc(_monolith.NetworkObjectId, _spell.id);
    panel.SetActive(false);
}
\`\`\`

### 3. ServerRpc para unlock + sync

\`\`\`csharp
[ServerRpc(RequireOwnership = false)]
void UnlockSpellServerRpc(ulong monolithNoId, string spellId, ServerRpcParams p = default) {
    var no = GetNetworkObject(monolithNoId);
    var mono = no?.GetComponent<MonolithController>();
    if (mono == null || mono.isExhausted.Value) return;
    mono.isExhausted.Value = true;
    AddSpellToInventoryClientRpc(spellId, p.Receive.SenderClientId);
}

[ClientRpc]
void AddSpellToInventoryClientRpc(string spellId, ulong owner) {
    if (NetworkManager.Singleton.LocalClientId != owner) return;
    var s = SpellRegistry.GetById(spellId);
    PlayerController.Instance.inventory.AddSpell(s);
}
\`\`\`

Bloqueado por [TBR-016] (monolitos como NetworkObject + \`isExhausted\` NetworkVariable).

### 4. Cancelar con ESC

Listener de ESC mientras typing activo:

\`\`\`csharp
void Update() {
    if (!panel.activeSelf) return;
    if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
}

void Cancel() {
    typingInput.OnSpellCast -= OnComplete;
    PlayerController.Instance.MoveSpeed();
    panel.SetActive(false);
    // monolito NO se marca exhausted
}
\`\`\`

### 5. Daño mortal cancela

Suscribirse a \`stats.OnLifeLost\` durante el challenge:

\`\`\`csharp
PlayerController.Instance.stats.OnLifeLost += Cancel;
\`\`\`

Necesita [TBR-005].

## Cómo verificar

- Seleccionar nivel → texto del rune visible, focus en el InputField.
- Tipear sin error hasta el final → spell aparece en \`PlayerInventory\` (verificar con \`inventory.GetUnlockedSpells().Count\`).
- Monolito queda agotado para todos los clientes (no solo el local).
- ESC en mitad → cancela, movimiento restaurado, monolito sigue disponible.
- Recibir damage letal en mitad → cancela.

## Dependencias

- [TBR-038] el modal selecciona el spell.
- [TBR-016] monolitos como NetworkObject para sincronizar exhausted.
`,

'TBR-040': `## Estado actual

\`BattleState.Enter\` ya hace \`NullMoveSpeed()\` y trigger animación "Casting", pero **la cámara sigue libre con el mouse** porque \`CameraController.OnCamaraMove\` no se desactiva.

## Pasos

### 1. Verificar BattleState actual

Abrí \`Assets/Features/Gameloop_and_StateMachine/BattleState.cs\`. Buscá \`Enter()\`. Confirmá que llama \`NullMoveSpeed\` pero no toca \`cameraController.OnCamaraMove\`.

### 2. Bloquear el look

\`\`\`csharp
public override void Enter() {
    castInputController.enabled = true;
    playerController.NullMoveSpeed();
    playerController.cameraController.OnCamaraMove = false; // ← AGREGAR
    playerAnimatorView.TriggerCasting();
    playerController.onExplorationState = true;
}
\`\`\`

### 3. Apuntar al target en LookAhead

\`CameraController.LookAhead\` actualmente interpola "hacia adelante". Cambiarlo para apuntar al target lockeado:

\`\`\`csharp
void LookAhead() {
    Vector3 lookPoint;
    var target = TargetSystem.Instance != null ? TargetSystem.Instance.CurrentTarget : null;
    if (target != null) {
        lookPoint = target.position + Vector3.up * 1.2f;
    } else {
        lookPoint = transform.position + transform.forward * 5f;
    }
    var dir = lookPoint - transform.position;
    transform.rotation = Quaternion.Slerp(transform.rotation,
        Quaternion.LookRotation(dir),
        Time.deltaTime * smoothSpeed);
}
\`\`\`

### 4. Restaurar al salir

\`\`\`csharp
public override void Exit() {
    castInputController.enabled = false;
    playerController.MoveSpeed();
    playerController.cameraController.OnCamaraMove = true; // ← AGREGAR
}
\`\`\`

### 5. Smooth tracking

Si el target se mueve, el Slerp suaviza. Tunear \`smoothSpeed\` (5-8 razonable) para que no sea demasiado lento ni instantáneo.

## Cómo verificar

- Pulsar Tab → cámara queda fija apuntando al target.
- Mover el mouse → cámara no rota libremente.
- Mover el target (otro jugador se mueve) → cámara lo sigue suavemente.
- Pulsar Tab de nuevo → cámara vuelve a control libre del mouse.

## Dependencias

- [TBR-003] el target lockeado viene de \`TargetSystem\`.
`,

'TBR-041': `## Estado actual

\`TargetSystem\` (cuando [TBR-003] esté implementado) retornará el más cercano sin filtro de rango. No hay indicador visual del rango.

## Pasos

### 1. Float configurable en TargetSystem

\`\`\`csharp
[SerializeField] private float maxRange = 15f;
public float MaxRange => maxRange;

public Transform FindClosestTarget(Vector3 from) {
    return Candidates(from)
        .Where(t => Vector3.Distance(t.position, from) < maxRange)
        .OrderBy(t => Vector3.Distance(t.position, from))
        .FirstOrDefault();
}
\`\`\`

### 2. Anillo visual

Crear \`BattleRangeIndicator.prefab\`:
- Un Quad rotado 90° (acostado en el suelo).
- Material con shader transparent que dibuja un anillo (textura PNG con alpha o shader procedural).
- Hijo del player prefab, default oculto.

### 3. Mostrar / ocultar con BattleState

En \`BattleState.Enter\`:

\`\`\`csharp
var indicator = playerController.battleRangeIndicator;
if (indicator != null) {
    indicator.SetActive(true);
    var diameter = TargetSystem.Instance.MaxRange * 2f;
    indicator.transform.localScale = new Vector3(diameter, 1f, diameter);
}
\`\`\`

En \`Exit\`: \`indicator.SetActive(false)\`.

### 4. Sin target en rango

En \`HandleOnSpellCast\` ([TBR-004]):

\`\`\`csharp
var target = TargetSystem.Instance.CurrentTarget;
if (target == null) {
    AudioManager.Instance.PlaySFX("sfx_no_target");
    UIAnimator.Pop(noTargetText.transform);
    return; // VFX se reproducen pero damage = 0
}
\`\`\`

### 5. Performance

El anillo es 1 quad → costo despreciable. Si querés algo más fancy, decal projector de URP.

## Cómo verificar

- Pulsar Tab → anillo visible en el suelo del player.
- Enemigo dentro del anillo → seleccionable y atacable.
- Enemigo fuera del anillo → no es seleccionable.
- Sin enemigos en rango → cast ejecuta VFX pero feedback "sin objetivo".
- Salir de Battle → anillo desaparece.

## Dependencias

- [TBR-003] \`TargetSystem.FindClosestTarget\` implementado.
`,

});
