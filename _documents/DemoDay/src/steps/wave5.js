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

Ya existe la base de la arquitectura VFX y la Bola de Fuego se ve correctamente en red:

- \\\`Spell.cs\\\` define los campos \\\`tier\\\`, \\\`archetype\\\`, \\\`runeString\\\`, \\\`materialVFX\\\` y los parámetros de partícula (\\\`startSize\\\`, \\\`startSpeed\\\`, \\\`emissionRate\\\`, \\\`shapeRadius\\\`, \\\`loop\\\`, \\\`particleLifeDuration\\\`).
- \\\`SpellVFXBinder.cs\\\` lee el SO y multiplica \\\`startSize\\\` y \\\`emissionRate\\\` según el tier (T1=1x, T2=1.4x/1.5x, T3=2x/2.5x).
- \\\`ProjectileVFX.cs\\\` mueve el VFX por \\\`spell.speed\\\` y vive \\\`range/speed\\\` segundos.
- \\\`VFXPrefabBuilder.cs\\\` genera \\\`VFX_Projectile.prefab\\\` + \\\`M_Fire_T1.mat\\\` desde \\\`Tools/TBR/Build VFX_Projectile Prefab\\\`.
- \\\`SpellNetworkController.cs\\\` resuelve el spellId desde \\\`SpellCatalog\\\` y dispara el VFX via ClientRpc en todos los clientes (sin daño, eso queda para [TBR-048]).
- \\\`Bola de Fuego.asset\\\`: \\\`archetype=Projectile\\\`, \\\`tier=TierOne\\\`, \\\`runeString="desechos de fenix"\\\`, \\\`materialVFX=M_Fire_T1\\\`.

Lo que falta es completar los otros 5 arquetipos, los 9 materiales por elemento restantes y mapear los ~50 spells. Este ticket es el "umbrella": queda cerrado cuando [TBR-045] + [TBR-046] + [TBR-047] estén done.

## Los 6 arquetipos VFX

Cada arquetipo tiene un prefab base, un script de comportamiento paralelo a \\\`ProjectileVFX\\\` y se selecciona en cada \\\`Spell\\\` SO por el campo \\\`archetype\\\`.

| # | Arquetipo | Enum value | Comportamiento | Ejemplo de spell |
|---|-----------|------------|----------------|------------------|
| 1 | **Projectile** | \\\`SpellTypes.Projectile\\\` (0) | Se mueve en línea recta a \\\`spell.speed\\\` durante \\\`range/speed\\\` s, despawn al colisionar. | Bola de Fuego, Lanzallamas |
| 2 | **AOE** | \\\`SpellTypes.AOE\\\` (6) | Disco expansivo en el suelo, escala a \\\`spell.range\\\`, vida = \\\`spell.duration\\\`. | Pilar de llamas, Onda sísmica |
| 3 | **Aura** | \\\`SpellTypes.Aura\\\` (5) | Loop con \\\`simulationSpace=Local\\\`, parent al caster, vida = \\\`spell.duration\\\`. | Espada de llamas, Escudo |
| 4 | **Beam** | \\\`SpellTypes.Beam\\\` (8, nuevo) | LineRenderer + ParticleSystem entre caster y target (o caster + direction*range). | Rayo continuo |
| 5 | **Summon** | \\\`SpellTypes.Summon\\\` (2) | Placeholder mesh + ParticleSystem, vive \\\`spell.duration\\\`, sin colisión todavía. | Fénix, espíritus |
| 6 | **BuffDebuff** | \\\`SpellTypes.Buff\\\` (3) o \\\`Debuff\\\` (4) | Partícula pequeña que sigue al \\\`target.position + offset\\\`, vida = \\\`spell.duration\\\`. | Curación, Veneno |

## Pasos para cerrar el umbrella

### 1. Construir los 5 arquetipos restantes ([TBR-045])

Desde Unity, menú \\\`Tools/TBR/Build All Archetype Prefabs\\\`. Esto genera:

- \\\`Assets/Prefabs/VFX/VFX_Projectile.prefab\\\` (ya existía)
- \\\`Assets/Prefabs/VFX/VFX_AOE.prefab\\\`
- \\\`Assets/Prefabs/VFX/VFX_Aura.prefab\\\`
- \\\`Assets/Prefabs/VFX/VFX_Beam.prefab\\\`
- \\\`Assets/Prefabs/VFX/VFX_Summon.prefab\\\`
- \\\`Assets/Prefabs/VFX/VFX_BuffDebuff.prefab\\\`

Cada uno tiene \\\`SpellVFXBinder\\\` + su script específico (\\\`AOEVFX\\\`, \\\`AuraVFX\\\`, \\\`BeamVFX\\\`, \\\`SummonVFX\\\`, \\\`BuffDebuffVFX\\\`). Los builders son idempotentes: correr el menú dos veces no duplica nada.

### 2. Crear los 10 materiales por elemento ([TBR-046])

Hoy solo existe \\\`M_Fire_T1.mat\\\`. Faltan: \\\`M_Water\\\`, \\\`M_Earth\\\`, \\\`M_Air\\\`, \\\`M_Nature\\\`, \\\`M_Lightning\\\`, \\\`M_Darkness\\\`, \\\`M_Light\\\`, \\\`M_Ice\\\`, \\\`M_Lava\\\`. Un solo material por elemento (el tier se controla en el binder, no en el material). Shader: \\\`Universal Render Pipeline/Particles/Unlit\\\` con additive blending.

### 3. Mapear cada Spell SO a su arquetipo + material ([TBR-047])

Por cada SO en \\\`Assets/ScriptableObjects/Objects/Spells/**\\\`:

1. Setear \\\`archetype\\\` según semántica del hechizo (proyectil/AOE/aura/beam/summon/buff/debuff).
2. Setear \\\`tier\\\` según la carpeta (\\\`Tier 1/\\\` → \\\`TierOne\\\`, etc.).
3. Setear \\\`runeString\\\` (palabra sin espacios, sin tildes/ñ, lowercase) si está vacío.
4. Setear \\\`materialVFX\\\` según \\\`elementType\\\` (mapping al \\\`M_<elemento>.mat\\\` correspondiente).
5. Validar manualmente los casos raros (curación = Buff, no Projectile; etc.).

Un editor tool en \\\`Tools/TBR/Auto-Assign Spell Archetypes\\\` hace el primer pase automático y deja log de los que necesitan revisión manual.

### 4. Routing por arquetipo en SpellNetworkController — YA WIREADO ✅

\\\`SpellNetworkController.PlaySpellVFXClientRpc\\\` ahora switchea por \\\`spell.archetype\\\` y llama al \\\`Launch(...)\\\` correcto de cada arquetipo. **No tocar este archivo** salvo que estés agregando un arquetipo nuevo al enum \\\`SpellTypes\\\`.

Lo que ya hace el routing:

- \\\`Projectile\\\` → \\\`SpawnFromPool("VFX_Projectile")\\\` → \\\`ProjectileVFX.Launch(spell, direction)\\\`
- \\\`AOE\\\` → \\\`SpawnFromPool("VFX_AOE")\\\` → \\\`AOEVFX.Launch(spell, origin)\\\`
- \\\`Aura\\\` → \\\`SpawnFromPool("VFX_Aura")\\\` → \\\`AuraVFX.Launch(spell, casterTransform)\\\` (auto-parent al caster)
- \\\`Beam\\\` → \\\`SpawnFromPool("VFX_Beam")\\\` → \\\`BeamVFX.Launch(spell, casterTransform, direction)\\\`
- \\\`Summon\\\` → \\\`SpawnFromPool("VFX_Summon")\\\` → \\\`SummonVFX.Launch(spell, origin, direction)\\\`
- \\\`Buff\\\` / \\\`Debuff\\\` → \\\`SpawnFromPool("VFX_BuffDebuff")\\\` → \\\`BuffDebuffVFX.Launch(spell, target)\\\` (target = caster mientras [TBR-048] no agregue selección de target)
- Default → cae a Projectile (fallback seguro)

Robustez ya implementada:

- Si \\\`PoolManager\\\` no tiene el tag, hace \\\`Instantiate(<arquetipo>VfxPrefab)\\\` con el campo del Inspector.
- Si no hay prefab fallback, logea warning sin crashear.
- Resuelve el \\\`casterTransform\\\` iterando \\\`SpawnManager.SpawnedObjects\\\` por \\\`OwnerClientId\\\` (funciona en server y clients).

Lo único que tiene que hacer el equipo:

1. Arrastrar los 6 prefabs (\\\`VFX_Projectile\\\`, \\\`VFX_AOE\\\`, etc.) a los campos del componente \\\`SpellNetworkController\\\` en el prefab del Player.
2. Configurar el \\\`archetype\\\` correcto en cada Spell SO ([TBR-047] lo hace masivo).

### 5. Registrar los 6 archetypes en PoolManager

Pre-warm con \\\`size=20\\\` cada uno (\`6 * 20 = 120 GameObjects\` de partículas inactivos). Tag = nombre del prefab.

### 6. Validación de performance

Coordinar con [TBR-036]: 4 jugadores casteando simultáneamente, mantener 60fps en hardware target. Profiler:

- \\\`Instantiate\\\` count = 0 en hot path (todo viene del pool).
- GC allocs por frame en bucle de \\\`Update\\\` de los VFX = 0.

## Tuning de partículas — "lo que quiero" → "qué modifico"

Cada VFX se configura en dos lugares: campos del **Spell SO** (\\\`Assets/ScriptableObjects/Objects/Spells/<Elemento>/Tier N/<Spell>.asset\\\`) que aplican a TODAS las instancias del spell, y el **prefab del arquetipo** (\\\`Assets/Prefabs/VFX/VFX_<Arquetipo>.prefab\\\`) que aplica a todos los spells de ese arquetipo.

### A. Campos editables por spell (Spell SO)

| Lo que quiero | Campo en el Spell SO | Notas |
|---------------|----------------------|-------|
| **Más partículas por segundo** | \\\`emissionRate\\\` ↑ (default 50) | En tier escala × {1, 1.5, 2.5} |
| **Menos densidad de partículas** | \\\`emissionRate\\\` ↓ | |
| **Partículas más grandes** | \\\`startSize\\\` ↑ (default 1) | En tier escala × {1, 1.4, 2} |
| **Partículas más chicas** | \\\`startSize\\\` ↓ | |
| **Proyectil más rápido** | \\\`speed\\\` ↑ (default 50) | Mueve el GameObject. También ↓ vida porque vida = \\\`range/speed\\\` |
| **Proyectil más lento** | \\\`speed\\\` ↓ | Vive más tiempo en pantalla |
| **Partículas individuales más rápidas** | \\\`startSpeed\\\` ↑ (default 2) | Velocidad de cada partícula al spawn (NO mueve el GameObject) |
| **Mayor alcance** | \\\`range\\\` ↑ (default 50) | Projectile: vida más larga · AOE: disco más grande · Beam: rayo más largo |
| **Más duración (AOE/Aura/Beam/Summon/BuffDebuff)** | \\\`duration\\\` ↑ | Tiempo total que el VFX vive en pantalla |
| **Partículas que duran más individualmente** | \\\`particleLifeDuration\\\` ↑ | Cada partícula vive más antes de desaparecer |
| **Más esparcidas al nacer** | \\\`shapeRadius\\\` ↑ (default 2) | Radio de la esfera/círculo de emisión |
| **Repite emisión (loop) sí / no** | \\\`loop\\\` true/false | Aura/Beam: true · AOE: false |
| **Cambiar color/elemento visual** | \\\`materialVFX\\\` | M_Fire / M_Water / M_Earth / etc. |
| **Más daño** | \\\`damage\\\` ↑ | Se aplica en [TBR-048], no afecta el VFX |
| **Aplicar slow/freeze/poison al impactar** | \\\`debuff\\\` (StatusEffects) | Pendiente de wireup en [TBR-048] |

### B. Lo que NO está expuesto en el SO — editar el prefab

Estos viven en \\\`Assets/Prefabs/VFX/VFX_<Arquetipo>.prefab\\\` → ParticleSystem (afectan a todos los spells de ese arquetipo):

| Lo que quiero | Módulo del ParticleSystem | Campo |
|---------------|---------------------------|-------|
| **Más gravedad (que caigan)** | Main | \\\`Gravity Modifier\\\` ↑ (0 = sin gravedad, 1 = 9.8 m/s²) |
| **Que las partículas se aceleren** | Velocity over Lifetime | \\\`Linear\\\` curva creciente |
| **Que se frenen al final** | Limit Velocity over Lifetime | \\\`Speed\\\` con damping |
| **Estela detrás de cada partícula** | Trails | \\\`Ratio\\\` > 0 + asignar material |
| **Cambian de color con el tiempo** | Color over Lifetime | Gradient |
| **Crecen o decrecen con el tiempo** | Size over Lifetime | Curve |
| **Rotan al moverse** | Rotation over Lifetime | \\\`Angular Velocity\\\` |
| **Empujadas por viento/fuerza global** | Force over Lifetime | Vector |
| **Que dejen marcas en el suelo** | Collision + Sub Emitters | \\\`Type=World\\\` |
| **Más densidad bajo presión** | Main | \\\`Max Particles\\\` ↑ (default 400 en AOE, 200 en otros) |

Si una propiedad del prefab debería variar por spell (no por arquetipo), promoverla a campo del SO: agregar el campo en \\\`Spell.cs\\\` y aplicarlo en \\\`SpellVFXBinder.Bind\\\`.

### C. Cómo escalar una propiedad nueva por tier

\\\`SpellVFXBinder.cs\\\` ya escala dos cosas por tier:

\\\`\\\`\\\`csharp
static readonly float[] SizeMul     = { 1f, 1.4f, 2f };   // T1, T2, T3
static readonly float[] EmissionMul = { 1f, 1.5f, 2.5f };
\\\`\\\`\\\`

Para que otra propiedad escale por tier (ejemplo: gravedad):

1. **Exponer el campo en \\\`Spell.cs\\\`**:
   \\\`\\\`\\\`csharp
   public float gravityModifier = 0f;
   \\\`\\\`\\\`

2. **Agregar la tabla de multiplicadores en \\\`SpellVFXBinder.cs\\\`**:
   \\\`\\\`\\\`csharp
   static readonly float[] GravityMul = { 1f, 1.3f, 1.8f };
   \\\`\\\`\\\`

3. **Aplicarla en \\\`Bind(spell)\\\`** dentro del módulo \\\`main\\\`:
   \\\`\\\`\\\`csharp
   int t = Mathf.Clamp((int)spell.tier, 0, GravityMul.Length - 1);
   main.gravityModifier = spell.gravityModifier * GravityMul[t];
   \\\`\\\`\\\`

4. **Asignar en cada Spell SO**: \\\`gravityModifier = 0.5\\\` (Bola de Fuego T1) → en T2 sale 0.65, en T3 sale 0.9.

El patrón es el mismo para cualquier propiedad: campo en el SO → array de multiplicadores → \\\`main.X = spell.X * Mul[tier]\\\` en el binder.

### D. Recetas rápidas por arquetipo

- **Fuego T3 dramático**: \\\`startSize=1.5\\\`, \\\`emissionRate=80\\\`, \\\`particleLifeDuration=1.5\\\`, \\\`materialVFX=M_Fire\\\`. Con tier T3 escala a size=3, emission=200.
- **Aura defensiva calmada**: \\\`loop=true\\\`, \\\`startSpeed=0.3\\\`, \\\`emissionRate=20\\\`, \\\`shapeRadius=1.5\\\`, \\\`duration=5\\\`.
- **AOE explosivo corto**: \\\`loop=false\\\`, \\\`emissionRate=0\\\` (usa burst del prefab), \\\`range=6\\\`, \\\`duration=0.8\\\`.
- **Beam continuo**: \\\`loop=true\\\`, \\\`duration=2\\\`, \\\`range=15\\\`, \\\`emissionRate=100\\\`.
- **Curación buff**: archetype=Buff, \\\`startSize=0.2\\\`, \\\`emissionRate=25\\\`, \\\`duration=3\\\`, \\\`materialVFX=M_Light\\\`.

## Cómo verificar

- Menú \\\`Tools/TBR/Build All Archetype Prefabs\\\` genera los 6 prefabs sin warnings.
- Correrlo dos veces no duplica nada (idempotencia).
- \\\`SpellCatalog\\\` contiene todos los \\\`Spell\\\` SO del proyecto.
- Cada uno de los ~50 spells reproduce el VFX correcto al castearse en red real con 4 jugadores.
- 60fps sostenido con 4 jugadores casteando.
`,

});
