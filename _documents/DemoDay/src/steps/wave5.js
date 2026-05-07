// Wave 5 — 2 tickets: TBR-002, TBR-023
// Cada agente puede trabajar este archivo de forma independiente.

Object.assign(window.TICKET_STEPS = window.TICKET_STEPS || {}, {

'TBR-002': `## Estado actual

\\\`SpellBookUI.Refresh()\\\` existe pero **no tiene caller**. \\\`CastInputController.spellText\\\` es fijo y no se elige antes de tipear. Al entrar a BattleState hoy no aparece ninguna lista.

## Pasos

### 1. Abrir el spellbook al entrar a BattleState

En \\\`PlayerController.ExplorationState()\\\` (línea 114), después de \\\`ChangeState(battleState)\\\`:

\\\`\\\`\\\`csharp
spellBookUI.OpenForPlayer(stats, inventory);
\\\`\\\`\\\`

### 2. Refresh con el inventario real

En \\\`SpellBookUI.OpenForPlayer\\\`:

\\\`\\\`\\\`csharp
var spells = inventory.GetSpellsByTier(stats.tier);
Refresh(spells, currentPage);
\\\`\\\`\\\`

### 3. Selección

Al hacer click en un slot:

- \\\`CastInputController.spellText = selectedSpell.runeChallenge;\\\`
- Activar el \\\`TMP_InputField\\\` y darle focus.
- Cerrar el panel del spellbook.

### 4. Cancelar

Botón **Cancelar** o tecla ESC vuelve a \\\`PlayState\\\` sin penalización (no decrementar nada). Disparar \\\`onExplorationState = true\\\` en \\\`PlayerController\\\`.

### 5. Filtrado por tier

Slots de tier > playerTier deben aparecer en gris (\\\`opacity 0.4\\\`) y no clickables.

## Cómo verificar

- Tipear binding → aparece la lista de hechizos disponibles.
- Seleccionar hechizo → typing del rune correspondiente arranca.
- ESC cancela → regresa a PlayState con \\\`onExplorationState=true\\\`.
- Hechizos de tier alto se muestran pero no son seleccionables.
`,

'TBR-023': `## Estado actual

\\\`SpellData\\\` no tiene campos de VFX asignados. Cualquier cast es invisible.

## Pasos

### 1. Verificar el alcance real

Antes de hacer 50 sistemas, contá los \\\`SpellData\\\` SO existentes en \\\`Assets/ScriptableObjects/Spells/\\\`. Si hay menos, ajustá expectativa con producto.

### 2. Templates por elemento

En lugar de 50 únicos: **6 elementos** × **3 fases** (cast / projectile / hit) = **18 sistemas base**, con variantes de tier.

### 3. Crear ParticleSystem prefabs

\\\`Assets/VFX/Spells/{Element}/{Phase}.prefab\\\`. Empezá con 1 elemento completo, validá look y performance, replicá patrón.

### 4. Asignar a SpellData

Bloqueado por [TBR-004] que extiende \\\`SpellData\\\` con \\\`vfxCast\\\`, \\\`vfxProjectile\\\`, \\\`vfxHit\\\`.

### 5. Pooling obligatorio

\\\`PoolManager\\\` (ya existe pero sin caller) debe servir todos los VFX. Pre-warm en \\\`GameplayManager.Start\\\`.

### 6. Cell shading compatible

Coordinar con [TBR-024] para que los shaders soporten partículas.

### 7. Optimization

GPU Instancing en materiales. \\\`MaxParticles\\\` razonable (50-100).

## Cómo verificar

- Cada elemento → 3 VFX visibles (mano, proyectil, impacto).
- 4 jugadores casteando simultáneamente → 60fps.
- Profiler: \\\`Instantiate\\\` count = 0 en hot path.
`,

});
