# Specs — Typing Battle Royale

> Resumen puntual y objetivo del estado actual del juego (ingeniería inversa).
> Fecha: 2026-05-06.
> Documento complementario: [Diagrams.md](Diagrams.md).

---

## 1. Stack técnico

| Item | Valor |
|---|---|
| Engine | Unity (proyecto en `TypingBattleRoyaleProject/`) |
| Networking | **Unity Netcode for GameObjects** (no Mirror, no Photon) |
| Transporte | UnityTransport (UTP) — puerto por defecto **7777** |
| Modelo de red | LAN P2P (Host + Clientes) |
| Input | New Input System (`InputActionReference`) |
| UI texto | TextMeshPro |
| Scripting | C# (un único assembly por defecto) |

---

## 2. Escenas

### En build (`EditorBuildSettings.asset`)

| # | Escena | Rol |
|---|---|---|
| 0 | **LobbyScene** (entry) | Host/Join LAN, input de IP |
| 1 | CharacterSelect | Selección de skin + color, lista listos |
| 2 | GameplayScene | Combate por typing, monolitos, pausa, fin de partida |

### Huérfanas (en `Assets/Scenes/` pero NO en build)

`MainMenu`, `LoadingScreen`, `Debug`, `SampleScene`, `PauseTest`, `TestRafa`, `TestJumpPad`, `TypingDebug`, `Graybox`, `prueba`, `SceneClient`, `DeathPlayer`.

> Atención: `EndGameUI` y `PauseController` cargan `"MainMenu"` por nombre — esa escena no está en build, así que el botón fallará en builds.

---

## 3. Flujo de navegación (resumido)

```
LobbyScene  ──Host StartHost / Cliente StartClient──▶  CharacterSelect
CharacterSelect  ──Host: ConfirmSelection──▶  GameplayScene
GameplayScene  ──Play Again──▶  GameplayScene (reload)
GameplayScene  ──Main Menu──▶  MainMenu  ⚠️ (huérfana, fallará en build)
```

- `LobbyController.gameScene` (campo serializado) está **sobreescrito en escena a `"CharacterSelect"`**, aunque el default en código es `"GameplayScene"`. Esa edición de inspector es la que mantiene el flujo correcto.
- `CharacterSelectController.ConfirmSelection()` carga `"GameplayScene"` con el `NetworkManager.SceneManager` (sincronizado).
- `SceneLoader.LoadScene()` aplica fade in/out de 0.3 s a través de `ScreenFader`.

---

## 4. Singletons / DontDestroyOnLoad

| Singleton | Archivo | Función |
|---|---|---|
| `ScreenFader` | `Scripts/ScreenFader.cs` | Fades entre escenas |
| `GameManager` | `Core/GameManager.cs` | Timer global; **su StateMachine no se usa** (la sobrescribe `GameplayManager`) |
| `IPHolder` | `Scripts/IPHolder.cs` | Guarda PlayerID local |
| `SelectController` | `Scripts/Controllers/SelectController.cs` | Persiste selección entre escenas |
| `AudioManager` | `Core/AudioManager.cs` | **Stub** — métodos vacíos |
| `IDController.savedSelections` | static dict | Selección de personaje serializada por `clientId` |

---

## 5. State Machine de gameplay

`GameplayManager.InitializeStates()` instancia 5 estados; arranca en `WaitingState`.

| Estado | Entrada | Acción | Salida |
|---|---|---|---|
| **WaitingState** | inicio de GameplayScene | Countdown 3–2–1–"¡Lucha!" (4 s), `PlayerController` desactivado | → PlayState |
| **PlayState** | tras countdown | `PlayerController` activo, loop principal | → BattleState (toggle) o GameOverState |
| **BattleState** | tecla de modo combate | `CastInputController` ON, `moveSpeed = 0`, anim "Casting" | → PlayState al terminar/cancelar el spell |
| **GameOverState** | `TriggerGameOver(winnerID)` | Apaga todos los `PlayerController`, muestra `EndGameCanvas`, `Time.timeScale = 0` | Reload o LoadScene(MainMenu) |
| **ExplorationState** | (definido pero no se usa de entrada) | Activa cámara libre | — |

> Hay duplicación: `GameManager` también construye una `StateMachine(ExplorationState)` que nunca se ejecuta porque `GameplayManager` crea su propia y la usa exclusivamente.

---

## 6. Networking — modelo

- **NetworkManager.Singleton + UnityTransport**, configurado en `LobbyScene` por `NetworkManagerConfigurator` (Awake setea ConnectionData y player prefab).
- **Host**: `StartHost()` → `NetSM.LoadScene("CharacterSelect")`.
- **Cliente**: `StartClient(targetIP)` → sigue al host por `NetworkManager.SceneManager`.
- **`IDController` (NetworkBehaviour)** — un objeto por jugador conectado. NetworkVariables: `skinIndex`, `colorIndex`, `already` (write-owner, read-everyone).
- **Spawn de jugadores**: lo hace **sólo el Host** en `GameplayManager.PopulateSpawnPoint()`:
  - 4 puntos hardcodeados, Fisher-Yates shuffle.
  - `Instantiate` + `NetworkObject.SpawnWithOwnership(clientId)`.
  - Cliente hace `WaitForSeconds(5f)` ciegos como sincronización.
- **`NetworkPlayerController`**: chequea `IsOwner` antes de mover; expone `SetPlayerColorClientRpc`.
- **`NetworkManagerMock`** (legacy/debug): usado en `Debug.unity` y referenciado por `GameOverState` — **incompatible con red real**.

---

## 7. Modelo de datos

| Clase | Tipo | Campos clave |
|---|---|---|
| `PlayerStats` | POCO | `currentHP`, `maxHP`, `currentLifes`, `maxLives`, `killCount`, `wPM`, eventos `OnLifeLost / OnDamageTaken / OnEnemyKilled` |
| `MovementData` | POCO | `speed`, `gravity`, `jumpForce`, `resultVector` |
| `MonolithData` | POCO | `ID`, `Level (1-3)`, `_runeChallenge`, `spellData` |
| `MatchSessionData` | POCO | `MatchID`, `Timer`, `IsActive` |
| `PlayerInventory` | POCO | `AddSpell / GetUnlockedSpells / GetSpellsByTier` |
| `TypingStats` | POCO | `GetWPM`, `GetAccuracy`, `GetDamageBonusMultiplier` |
| `DamageCalculator` | POCO | `CalculateDamage(SpellData)` — **no se invoca desde combate real** |
| `SpellData` | ScriptableObject | rune (texto), elemento, tier, VFX |
| `Spell` | ScriptableObject | nombre, elemento, tier, damage, efectos, VFX |
| `SkinInfo` | ScriptableObject | `models[]`, `gameplayPrefabs[]` |
| `CharacterData` | ScriptableObject | metadatos de personaje |
| `AudioDataBase / AudioEntry` | ScriptableObject | DB de audio |
| `PrefabListDataBase` | ScriptableObject | lista de prefabs referenciables |

---

## 8. Tabla de daño por accuracy (typing)

`TypingStats.GetDamageBonusMultiplier()`:

| Accuracy | Multiplicador |
|---|---|
| ≥ 90% | 1.1× |
| ≥ 80% | 1.0× |
| ≥ 50% | 0.8× |
| ≥ 30% | 0.5× |
| < 30% | 0× |

---

## 9. Reglas / parámetros de partida

| Parámetro | Valor | Fuente |
|---|---|---|
| Puerto LAN | 7777 | `LobbyController.defaultPort` |
| Spawn points | 4 fijos hardcodeados | `GameplayManager.PopulateSpawnPoint` |
| Monolitos iniciales | 4 | `MonolithSpawn.initialMonoliths` |
| Niveles de monolito | 1–3 | `MonolithData.Level` |
| Countdown previo | 4 s (3-2-1-Lucha) | `WaitingState.CountdownRoutine` |
| Timer de partida | configurable por fase | `InGameTimer.phasesDurations` |
| Espera ciega del cliente al spawn | 5 s | `GameplayManager.PopulateSpawnPoint` |
| Fade in/out de escena | 0.3 s + 0.3 s | `SceneLoader` |
| Color por playerID | 0=Rojo, 1=Verde, 2=Amarillo, 3=Azul | `SelectController.SyncPlayer` |
| Pitch cámara clamp | -90°..+90° | `CameraController` |
| Player ID format | `"WIZ-"` + 4 chars (A-Z, 0-9), 10 reintentos | `PlayerIDGenerator` |

---

## 10. Controllers — responsabilidades (escena GameplayScene)

| Controller | Responsabilidad |
|---|---|
| `GameplayManager` | Orquesta StateMachine, spawn, GameOver |
| `PlayerController` | Movimiento + jump + toggle exploración/combate |
| `CameraController` | FPS look + modo libre/forzado-frente |
| `CastInputController` | Captura typing, calcula WPM/accuracy, dispara `OnSpellCast` |
| `SpellUIController` | Coloreado verde/rojo del texto del hechizo |
| `HUDController` | HP, vidas, kills, timer (suscrito a eventos de `PlayerStats` y `InGameTimer.OnSecondElapsed`) |
| `PauseController` | ESC pausa, no se activa durante GameOver |
| `RespawnController` | Re-teletransporta al jugador al caer |
| `MonolithSpawn` | Spawnea N monolitos en puntos aleatorios |
| `PlayerInteractorView` | Detecta `NearestMonolith` por triggers |
| `MonolithView.TryInteract` | Desbloquea hechizo si no exhausto |
| `LaunchPad` | Trayectoria parabólica al entrar al trigger |
| `PlayerAnimatorView` | Wraps `Animator` (IsMoving, Speed, Casting) |
| `EnemyLabel` | Texto flotante con ID del enemigo, mira a cámara |
| `EndGameUI` | Panel final, botones Play Again / MainMenu |

---

## 11. Inventario de código no utilizado / stubs

### Scripts stub o sin implementar

| Script | Estado |
|---|---|
| `SpellCaster.cs` | Vacío — `ToggleTypingMode()`, `CastSpell()` no implementados, ningún caller |
| `TargetSystem.cs` | Vacío — `FindClosestTarget()`, `ToggleLockOn()` no implementados |
| `MusicController.cs` | `CrossfadeTo()` vacío |
| `AudioManager.cs` | `PlaySFX/ChangeMusic/SetVolume` no implementados (sólo singleton setup) |
| `AudioSettings.cs` | `Save() / Load()` vacíos |
| `LoadingScreenController.cs` | Usa checkboxes manuales (`checkServer`, `checkPlayers`, `checkMap`, `checkLoot`); además la escena que lo aloja (`LoadingScreen.unity`) está huérfana |

### Métodos / clases sin caller real

| Símbolo | Comentario |
|---|---|
| `GameManager.stateMachine` (con ExplorationState) | Sobreescrito por GameplayManager; nunca progresa |
| `DamageCalculator.CalculateDamage` | Definido pero nunca llamado desde combate real |
| `MonolithController.PopulateSpells` | Crea lista que no se consume |
| `SpellBookUI.Refresh` | Sin caller obvio (probable binding por inspector) |
| `PoolManager.SpawnFromPool` | Singleton creado pero ningún caller |
| `NetworkManagerMock` | Sólo en `Debug.unity`; aun así `GameOverState` lo referencia (bug) |

### Escenas huérfanas

`MainMenu`, `LoadingScreen`, `Debug`, `SampleScene`, `PauseTest`, `TestRafa`, `TestJumpPad`, `TypingDebug`, `Graybox`, `prueba`, `SceneClient`, `DeathPlayer`.

---

## 12. Bugs / inconsistencias detectados

1. **EndGameUI/PauseController cargan `"MainMenu"`**, pero `MainMenu.unity` no está en build → la transición fallará en builds.
2. **`GameOverState` referencia `NetworkManagerMock.Instance.Controllers`** (líneas 21, 28, 38) — no compatible con red real (Netcode no expone `.Controllers`). Debe migrar a `NetworkManager.Singleton.ConnectedClientsList` y buscar `NetworkBehaviour` por `clientId`.
3. **`LobbyController.gameScene` default es `"GameplayScene"`**, lo correcto sería `"CharacterSelect"`. Hoy funciona porque está parchado en el inspector de la escena — frágil.
4. **Espera ciega de 5 s en clientes** dentro de `GameplayManager.PopulateSpawnPoint` — race condition si el host tarda más; debería sincronizarse vía RPC o evento de spawn.
5. **`GameManager` y `GameplayManager` mantienen StateMachines paralelas** — una de ellas (la de GameManager) nunca se ejecuta efectivamente.
6. **`PlayerStats` es read-only sin constructor de inicialización** — depende de valores en prefab/editor; un cambio de prefab descuadrado deja a los jugadores con 0 HP/0 vidas sin error visible.
7. **Spawn points hardcodeados (`Vector3` literales)** en `PopulateSpawnPoint` — deberían venir de Transforms en escena para que el level designer los ajuste.
8. **`OwnerClientId == 0` como check de Host** — usar `IsHost` o `IsServer` es más correcto.
9. **`SelectController` es Singleton local pero `IDController.savedSelections` es estático** — si `SaveAllSelections` no se llama (cliente confirma antes que host), el dict queda vacío y los spawn de prefab usan índices por defecto/erróneos.
10. **`AudioManager`/`MusicController`/`AudioSettings` en estado stub** — el juego se shipea sin audio funcional.

---

## 13. Mapa de archivos clave

```
TypingBattleRoyaleProject/Assets/
├── Scenes/                             # 3 en build, 12 huérfanas
├── Core/
│   ├── GameManager.cs                  # Singleton DDOL, timer global (StateMachine huérfana)
│   ├── AudioManager.cs                 # Stub
│   └── MonolithSpawn.cs                # Spawn de monolitos
├── Features/
│   ├── Gameloop_and_StateMachine/
│   │   ├── StateMachine.cs
│   │   ├── BattleState.cs
│   │   └── InGameTimer.cs
│   ├── Network/
│   │   ├── NetworkManagerMock.cs       # Legacy/Debug — referenciado por GameOverState
│   │   ├── SpawnCalculator.cs
│   │   └── PlayerIDGenerator.cs
│   ├── Player_and_Movement/
│   │   ├── EnemyLabel.cs
│   │   ├── LaunchPad.cs
│   │   └── PlayerAnimatorView.cs
│   ├── Environment_and_Interaction/
│   │   ├── MonolithController.cs
│   │   ├── MonolithView.cs
│   │   └── PlayerInteractorView.cs
│   └── TypingCombat_and_System/
│       ├── EndGameUI.cs
│       └── SpellUIController.cs
├── Scripts/
│   ├── SceneLoader.cs                  # Wrapper con fade
│   ├── ScreenFader.cs                  # Singleton DDOL
│   ├── IDController.cs                 # NetworkBehaviour, savedSelections estático
│   ├── IPHolder.cs / IPValidator.cs
│   ├── AudioSettings.cs                # Stub
│   ├── Controllers/                    # Pause, HUD, Player, Cast, Camera, Gameplay…
│   ├── LAN/                            # Lobby, NetworkManagerConfigurator, MultiplayerSetupHelper (editor)
│   ├── Models/                         # IGameState, GameState, GameOverState, WaitingState, ExplorationState, PlayState, MatchSessionData, PlayerInventory, TypingStats, DamageCalculator
│   └── Structs/                        # PlayerInputData
├── Data/                               # PlayerStats, MovementData, MonolithData
├── ScriptableObjects/Scripts/          # Spell, SpellData, SkinInfo, CharacterData, AudioDataBase, AudioEntry, PrefabListDataBase
├── Struct/                             # InteractionRequest
└── ProjectSettings/
    └── EditorBuildSettings.asset       # Sólo 3 escenas habilitadas
```
