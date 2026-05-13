# Diagramas — Typing Battle Royale

> Diagramas de flujo y secuencia generados por ingeniería inversa del proyecto Unity.
> Render: Mermaid (GitHub / VS Code Markdown Preview con extensión Mermaid).
> Fecha del análisis: 2026-05-06.

---

## 1. Diagrama de Flujo de Escenas (alto nivel)

Sólo 3 escenas están registradas en `ProjectSettings/EditorBuildSettings.asset`:
`LobbyScene` (índice 0, entry point), `CharacterSelect` (1), `GameplayScene` (2).
El resto son escenas huérfanas (test/legacy).

```mermaid
flowchart TD
    Start([Inicio del juego]) --> Lobby

    subgraph Build["Escenas en Build (3)"]
        Lobby["LobbyScene<br/>(entry point)"]
        CharSel["CharacterSelect"]
        Gameplay["GameplayScene"]
    end

    Lobby -- "Host: StartHost + LoadScene" --> CharSel
    Lobby -- "Cliente: StartClient<br/>(sigue al host por Netcode SceneManager)" --> CharSel
    CharSel -- "ConfirmSelection (solo Host)" --> Gameplay
    Gameplay -- "EndGameUI: Play Again" --> Gameplay
    Gameplay -- "EndGameUI: Main Menu" --> MainMenuOrphan[("MainMenu (HUÉRFANA)<br/>no está en build")]
    Gameplay -- "Pause → SceneMenu" --> MainMenuOrphan

    classDef orphan fill:#ffe0e0,stroke:#aa0000,color:#660000;
    class MainMenuOrphan orphan
```

> **Hallazgo crítico:** los botones "Main Menu" del EndGame y de Pause apuntan a `MainMenu`, pero esa escena no está en build → la carga fallará en build de producción.

### Escenas huérfanas (no referenciadas en el flujo activo)

```mermaid
flowchart LR
    O1[MainMenu.unity]:::orphan
    O2[LoadingScreen.unity]:::orphan
    O3[Debug.unity<br/>NetworkManagerMock]:::orphan
    O4[SampleScene.unity]:::orphan
    O5[PauseTest.unity]:::orphan
    O6[TestRafa.unity]:::orphan
    O7[TestJumpPad.unity]:::orphan
    O8[TypingDebug.unity]:::orphan
    O9[Graybox.unity]:::orphan
    O10[prueba.unity]:::orphan
    O11[SceneClient.unity]:::orphan
    O12[DeathPlayer.unity]:::orphan

    classDef orphan fill:#ffe0e0,stroke:#aa0000,color:#660000;
```

---

## 2. Diagrama de Secuencia — Flujo Multijugador (Host)

```mermaid
sequenceDiagram
    actor HostUser as Host
    participant LobbyUI as LobbyScene UI
    participant LobbyCtrl as LobbyController
    participant Transport as UnityTransport
    participant NetMgr as NetworkManager.Singleton
    participant NetSM as NetMgr.SceneManager
    participant CharSel as CharacterSelect

    HostUser->>LobbyUI: Click "Host"
    LobbyUI->>LobbyCtrl: OnHostButtonClicked()
    LobbyCtrl->>Transport: SetConnectionData("0.0.0.0", 7777)
    LobbyCtrl->>NetMgr: StartHost()
    NetMgr-->>LobbyCtrl: success = true
    LobbyCtrl->>NetSM: LoadScene("CharacterSelect", Single)
    NetSM-->>CharSel: carga sincronizada (Host + clientes)
    Note over CharSel: Cada cliente recibe sus IDController spawn
```

---

## 3. Diagrama de Secuencia — Flujo Multijugador (Cliente)

```mermaid
sequenceDiagram
    actor ClientUser as Cliente
    participant LobbyUI as LobbyScene UI
    participant LobbyCtrl as LobbyController
    participant Transport as UnityTransport
    participant NetMgr as NetworkManager.Singleton
    participant Host

    ClientUser->>LobbyUI: Escribe IP del Host
    ClientUser->>LobbyUI: Click "Join"
    LobbyUI->>LobbyCtrl: OnJoinButtonClicked()
    LobbyCtrl->>Transport: SetConnectionData(targetIP, 7777)
    LobbyCtrl->>NetMgr: StartClient()
    NetMgr->>Host: TCP/UDP connect
    Host-->>NetMgr: ApprovalCheck OK
    Note over NetMgr: Cliente queda en LobbyScene<br/>hasta que Host cargue siguiente escena
    Host-->>NetMgr: SceneManager.LoadScene sync
    NetMgr-->>ClientUser: Cliente entra a CharacterSelect
```

---

## 4. Diagrama de Secuencia — Selección de Personaje

```mermaid
sequenceDiagram
    actor User
    participant SelCtrl as SelectController<br/>(Singleton)
    participant IDCtrl as IDController<br/>(NetworkBehaviour)
    participant CharSelCtrl as CharacterSelectController
    participant NetSM as NetworkManager.SceneManager

    Note over IDCtrl: Spawnea por jugador conectado
    IDCtrl->>SelCtrl: RegisterLocalPlayer(this)
    SelCtrl->>SelCtrl: SyncPlayer(OwnerClientId)<br/>asigna color por ID (R/G/Y/B)

    User->>SelCtrl: UpArrow / DownArrow (skin)
    SelCtrl->>IDCtrl: skinIndex.Value++
    IDCtrl-->>IDCtrl: OnValueChanged → Update3DModel

    User->>SelCtrl: LeftArrow / RightArrow (color)
    SelCtrl->>IDCtrl: colorIndex.Value++

    User->>SelCtrl: OKClick()
    SelCtrl->>IDCtrl: already.Value = true

    Note over CharSelCtrl: Solo Host pulsa "Confirmar"
    User->>CharSelCtrl: ConfirmSelection() (Host)
    CharSelCtrl->>SelCtrl: SaveAllSelections()
    SelCtrl->>IDCtrl: savedSelections[clientId] = {skin, color}
    CharSelCtrl->>NetSM: LoadScene("GameplayScene")
```

---

## 5. Diagrama de Secuencia — Spawn en GameplayScene

```mermaid
sequenceDiagram
    participant NetSM as NetworkManager.SceneManager
    participant GM as GameplayManager<br/>(Singleton de escena)
    participant SM as StateMachine
    participant IDStore as IDController.savedSelections
    participant Skins as SkinInfo.gameplayPrefabs
    participant NetMgr as NetworkManager.Singleton

    NetSM-->>GM: GameplayScene cargada
    GM->>GM: Awake() → singleton
    GM->>GM: Start() → InitializeStates()
    GM->>SM: new StateMachine(waitingState)
    GM->>GM: SpawnPlayers() → coroutine PopulateSpawnPoint()

    alt Es Host (OwnerClientId == 0)
        GM->>GM: Define 4 spawn points fijos
        GM->>GM: RandomizeSpawnPoints (Fisher-Yates)
        loop foreach clientId in NetMgr.ConnectedClientsIds
            GM->>IDStore: savedSelections[clientId]
            GM->>Skins: prefab = arraySkins[skinIdx].gameplayPrefabs[colorIdx]
            GM->>NetMgr: Instantiate + NetworkObject.SpawnWithOwnership(clientId)
        end
    else Es Cliente
        GM-->>GM: WaitForSeconds(5f)
        Note over GM: Espera "ciega" — confía en que el Host ya spawneó
    end

    GM->>SM: ChangeState(waitingState)
```

---

## 6. Diagrama de Estados — StateMachine de Gameplay

`GameplayManager` instancia los estados pero arranca en `waitingState`. `BattleState` es una sub-máquina paralela activada desde `PlayState` cuando el jugador presiona la tecla de cambio.

```mermaid
stateDiagram-v2
    [*] --> WaitingState: Start()

    WaitingState: WaitingState\nPlayerController OFF\nCountdown 3-2-1-Lucha (4s)
    PlayState: PlayState\nPlayerController ON\nLoop principal
    GameOverState: GameOverState\nTime.timeScale = 0\nEndGameCanvas visible
    BattleState: BattleState (sub-modo)\nCastInputController ON\nmoveSpeed = 0\nAnim: Casting

    WaitingState --> PlayState: countdown finaliza (4s)
    PlayState --> BattleState: PlayerController.ExplorationState()\n(tecla binding)
    BattleState --> PlayState: spell completado / cancelado\n(OnSpellCast)
    PlayState --> GameOverState: TriggerGameOver(winnerID)\n(timer 0 o último vivo)
    GameOverState --> [*]: Reload / MainMenu
```

> **Nota:** existe también `ExplorationState` definido pero `GameplayManager` arranca en `waitingState` y nunca lo usa explícitamente. Hay una **StateMachine duplicada** en `GameManager.cs` (singleton DontDestroyOnLoad) que crea un `ExplorationState` pero su `Update()` se sobrescribe efectivamente al construir `GameplayManager`.

---

## 7. Diagrama de Secuencia — Casteo de Hechizo (Typing Combat)

```mermaid
sequenceDiagram
    actor Player
    participant PC as PlayerController
    participant SM as StateMachine
    participant BS as BattleState
    participant CIC as CastInputController
    participant SUI as SpellUIController
    participant GM as GameplayManager

    Player->>PC: presiona tecla "modo combate"
    PC->>SM: ChangeState(battleState)
    SM->>BS: Enter()
    BS->>PC: NullMoveSpeed()
    BS->>CIC: enabled = true
    BS->>BS: Anim trigger "Casting"

    loop cada keystroke
        Player->>CIC: tipea letra (TMP_InputField)
        CIC->>CIC: CastText(string) — valida vs spellText
        alt carácter correcto
            CIC->>SUI: UpdateDisplay(idx, hasError=false)
        else carácter incorrecto
            CIC->>CIC: incorrectInput++
            CIC->>SUI: UpdateDisplay(idx, hasError=true)
        end
    end

    CIC->>CIC: EvaluateAccuracy() → WPM, Accuracy
    CIC-->>GM: OnSpellCast event
    GM->>SM: ChangeState(playState)
    SM->>BS: Exit() → restaura moveSpeed, CIC OFF
```

---

## 8. Diagrama de Secuencia — Game Over

```mermaid
sequenceDiagram
    participant Trigger as Timer / lastAlive
    participant GM as GameplayManager
    participant SM as StateMachine
    participant GOS as GameOverState
    participant Mock as NetworkManagerMock⚠️
    participant UI as EndGameUI

    Trigger->>GM: TriggerGameOver(winnerID)
    GM->>SM: ChangeState(gameOverState)
    SM->>GOS: Enter()
    GOS->>Mock: Mock.Instance.Controllers
    Note over Mock: ⚠️ NetworkManagerMock no existe<br/>en partida con red real → NRE
    GOS->>GOS: Desactiva todos los PlayerController
    GOS->>UI: EndGameCanvas.alpha = 1
    GOS->>UI: Populate(winnerID, List<PlayerStats>)
    GOS->>GOS: Time.timeScale = 0

    alt Player Again
        UI->>UI: SceneLoader.Reload() → recarga GameplayScene
    else Main Menu
        UI->>UI: SceneLoader.LoadScene("MainMenu") ⚠️ no en build
    end
```

---

## 9. Diagrama de Componentes — Arquitectura

```mermaid
flowchart TB
    subgraph Persistent["Singletons DontDestroyOnLoad"]
        SF[ScreenFader]
        GM[GameManager<br/>+ InGameTimer]
        IPH[IPHolder]
        SC[SelectController]
        AM[AudioManager - stub]
    end

    subgraph Lobby["LobbyScene"]
        LC[LobbyController]
        LUI[LobbyUIController]
        NMC[NetworkManagerConfigurator]
        IPV[IPValidator]
    end

    subgraph CharSel["CharacterSelect"]
        CSC[CharacterSelectController]
        SC2[SelectController instance]
        IDC[IDController - per player<br/>NetworkBehaviour]
    end

    subgraph Gameplay["GameplayScene"]
        GPM[GameplayManager<br/>scene singleton]
        SMx[StateMachine]
        PC[PlayerController]
        CC[CameraController]
        CIC[CastInputController]
        SUI[SpellUIController]
        HUD[HUDController]
        Pause[PauseController]
        Resp[RespawnController]
        MS[MonolithSpawn]
        Mono[MonolithView/Controller]
        PIV[PlayerInteractorView]
        EUI[EndGameUI]
        Anim[PlayerAnimatorView]
        ELbl[EnemyLabel]
        LP[LaunchPad]
    end

    subgraph Network["Unity Netcode"]
        NM[NetworkManager.Singleton]
        UTP[UnityTransport]
        NPC[NetworkPlayerController]
    end

    subgraph Data["Datos / SO"]
        PS[PlayerStats]
        MD[MonolithData]
        SD[SpellData / Spell]
        SI[SkinInfo]
        CD[CharacterData]
        ADB[AudioDataBase]
    end

    LC --> NM
    NMC --> NM
    CSC --> NM
    CSC --> SC
    IDC --> SC
    SC --> IDC
    GPM --> NM
    GPM --> SMx
    GPM --> PC
    GPM --> CIC
    GPM --> EUI
    PC --> CC
    PC --> SMx
    CIC --> SUI
    HUD --> PS
    Resp --> PC
    MS --> Mono
    PIV --> Mono
    Mono --> SD
    SUI --> SD
    GPM --> SI
    AM --> ADB

    classDef stub fill:#fff3cd,stroke:#856404;
    class AM stub
```

---

## 10. Diagrama de Secuencia — Bootstrap Completo (resumen)

```mermaid
sequenceDiagram
    participant App
    participant Lobby
    participant Net as NetworkManager (Netcode)
    participant CharSel
    participant Gameplay
    participant SM as StateMachine

    App->>Lobby: Carga LobbyScene (índice 0)
    Lobby->>Net: StartHost / StartClient
    Lobby->>CharSel: NetSM.LoadScene("CharacterSelect")
    CharSel->>CharSel: Selección sincronizada via NetworkVariable
    CharSel->>Gameplay: NetSM.LoadScene("GameplayScene") (Host)
    Gameplay->>Gameplay: GameplayManager.SpawnPlayers
    Gameplay->>SM: WaitingState (3-2-1-Lucha)
    SM->>SM: PlayState (loop principal)
    SM->>SM: BattleState ↔ PlayState (typing combat)
    SM->>SM: GameOverState (timer 0 o último vivo)
    Gameplay-->>App: Reload o vuelta a "MainMenu" (huérfana)
```
