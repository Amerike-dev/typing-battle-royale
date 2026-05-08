// Wave 6 — 3 tickets: TBR-019, TBR-036, TBR-042
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-019': `## Estado actual

12 escenas huérfanas y 5 stubs nunca llamados. Limpieza pendiente para que el repo no acumule deuda visual.

## Pasos

### 1. Antes de borrar nada

Confirmá que estos tickets están terminados (porque pueden reescribir scripts marcados huérfanos):

- [TBR-002] usa \\\`SpellBookUI\\\`.
- [TBR-003] reescribe \\\`TargetSystem\\\`.
- [TBR-010] elimina la dependencia de \\\`NetworkManagerMock\\\`.
- [TBR-013] reemplaza \\\`MusicController\\\` con la implementación real.
- [TBR-020] decide el destino de \\\`MainMenu\\\`.

### 2. Mover escenas obsoletas

Crear \\\`Assets/_archive/Scenes/\\\` y mover ahí:

- \\\`SampleScene\\\`, \\\`prueba\\\`, \\\`TestRafa\\\`, \\\`TestJumpPad\\\`, \\\`TypingDebug\\\`, \\\`Graybox\\\`, \\\`PauseTest\\\`, \\\`SceneClient\\\`, \\\`DeathPlayer\\\`.

**No** las elimines del git (history); solo movelas para que el editor no las cargue por error.

### 3. Eliminar scripts realmente huérfanos

- \\\`SpellCaster.cs\\\` (vacío, sin caller).
- \\\`AudioSettings.cs\\\` (si TBR-013 lo reemplaza con implementación dentro de AudioManager).

### 4. Limpiar código comentado

\\\`GameplayManager.cs\\\` líneas 199-215 (el bloque \\\`SetupSpawns\\\` que usa \\\`NetworkManagerMock\\\`).

### 5. Build settings

Limpiar entries muertos en \\\`EditorBuildSettings.asset\\\` si hubieran quedado.

## Cómo verificar

- \\\`Assets/Scenes/\\\` solo contiene las 3-4 escenas activas.
- \\\`grep "NetworkManagerMock"\\\` solo devuelve la escena Debug.
- El proyecto compila y corre igual que antes.
`,

'TBR-036': `## Estado actual

Pre-VFX ([TBR-023]) y pre-shader ([TBR-024]) el juego corre OK con cápsulas. Tras esos cambios, target 60fps en hardware modesto (GTX 1060 o iGPU moderna).

## Pasos

### 1. Capturar baseline

Profiler con 4 jugadores casteando. Identificar:

- GC allocs/frame.
- SetPass calls.
- Draw calls.
- Frame time spike sources.

### 2. GC zero

Buscar allocs en bucle:

- \\\`new List<T>()\\\` en \\\`Update\\\` → reusar lista miembro.
- String concatenations → \\\`StringBuilder\\\`.
- LINQ en hot path → reemplazar con loops.

### 3. Pooling

[TBR-023] ya pide pool para VFX. Validá que también:

- Proyectiles usan pool.
- SFX AudioSources usan pool ([TBR-013]).

### 4. Static batching

Marcar \\\`Static\\\` todo lo que no se mueve (mapa, monolitos sin animación).

### 5. GPU instancing

Habilitar en materiales de partículas y de personajes (si scaling permite).

### 6. Build profiling

Profiler en build (no editor) con \\\`Development Build\\\` + \\\`Autoconnect Profiler\\\`.

### 7. Target verification

GTX 1060 o equivalente: 60fps consistentes en partida 4P en GameplayScene full.

## Cómo verificar

- Frame time < 16.6ms en 99% de los frames.
- GC alloc/frame ~ 0.
- Draw calls < 200.
- Build de Windows en hardware target → 60fps estables.

## Dependencias

- [TBR-023] VFX deben estar antes para perfilar el costo real.
- [TBR-024] shader toon también.
`,

'TBR-042': `## Estado actual

\`PlayerController.ExplorationState()\` ya togglea entre PlayState y BattleState al pulsar Tab (binding \`explorationState\` action). Pero **al salir mid-typing**, el estado del \`CastInputController\` queda en limbo: el InputField mantiene el texto y los listeners siguen subscritos.

## Pasos

### 1. Detectar typing activo

En \`CastInputController\`, exponer:

\`\`\`csharp
public bool IsTyping => inputField.isFocused && !string.IsNullOrEmpty(spellText);

public event Action OnCastCancelled;

public void Cancel() {
    if (!IsTyping) return;
    inputField.text = "";
    spellText = "";
    incorrectInput = 0;
    StopAllCoroutines();
    OnCastCancelled?.Invoke();
}
\`\`\`

### 2. Hook en BattleState.Exit

\`\`\`csharp
public override void Exit() {
    if (castInputController.IsTyping)
        castInputController.Cancel();
    castInputController.enabled = false;
    playerController.MoveSpeed();
    playerController.cameraController.OnCamaraMove = true;
}
\`\`\`

### 3. NO consumir el spell del inventory

\`SpellBookUI\` debe consumir el spell **solo en \`OnSpellCast\`** (cast exitoso), nunca en \`OnCastCancelled\`. Verificá:

\`\`\`csharp
castInputController.OnSpellCast += OnSpellSuccess;     // ← consume del inventory
castInputController.OnCastCancelled += OnSpellCancelled; // ← NO consume
\`\`\`

### 4. Cooldown NO aplica si fue cancelado

En la lógica de cooldown (parte de [TBR-004]):

\`\`\`csharp
castInputController.OnSpellCast += () => StartCooldown(currentSpell);
castInputController.OnCastCancelled += () => { /* no-op */ };
\`\`\`

### 5. Cancelar VFX en preparación

Si el VFX cast ya empezó local ([TBR-023]):

\`\`\`csharp
public override void Exit() {
    activeCastVFX?.Stop();
    activeCastVFX = null;
    // ... resto
}
\`\`\`

## Cómo verificar

- Tab → modo battle → seleccionar spell → empezar a tipear el rune.
- Tab durante typing → cast cancelado, vuelve a PlayState.
- Verificar \`PlayerInventory.GetUnlockedSpells().Count\` antes y después → mismo conteo.
- VFX preparándose se cancela visualmente.
- Cooldown del spell NO aplica.
- Re-entrar a battle → spell sigue disponible para otro intento.

## Dependencias

- [TBR-002] selección de spell desde el inventario (donde nace el typing).
`,

});
