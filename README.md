# Unity 3D Prototype — Survival Runner

Un prototipo di gioco 3D in Unity in cui il giocatore deve raccogliere un numero di monete entro un limite di tempo, evitando ostacoli e proiettili nemici. Il progetto adotta un'architettura **event-driven pura** (senza Singleton), con logica di gioco separata per favorire la testabilità.

---

## Gameplay

- **Obiettivo:** raccogliere le monete richieste (default: 5) prima che il timer scada.
- **Sconfitta:** il timer raggiunge 0 oppure il player perde tutta la salute.
- **Vittoria:** il player raccoglie tutte le monete necessarie.
- Le **monete speciali** aggiungono secondi al timer.
- Le **torrette** tracciano e sparano al player quando è nel loro raggio d'azione.
- Gli **ostacoli** infliggono danno e spingono il player indietro (recoil).

---

## Struttura del Progetto

```
Assets/
├── Project/
│   └── Script/
│       ├── Debug/
│       │   ├── CameraDebugVisualization.cs
│       │   └── HealthTester.cs
│       ├── Gameplay/
│       │   ├── Camera/
│       │   │   └── CameraOrbit.cs
│       │   ├── Enemy/Turret/
│       │   │   ├── TurretController.cs
│       │   │   └── TurretBullet.cs
│       │   ├── Coin.cs
│       │   └── Door.cs
│       ├── Player/
│       │   ├── PlayerController.cs
│       │   └── PlayerHealth.cs
│       └── UI&GameManager/
│           ├── GameEvents.cs
│           ├── GameManager.cs
│           ├── HealthBar.cs
│           └── MenuNavigation.cs
└── Tests/
    ├── CameraOrbitTests.cs
    ├── GameManagerTests.cs
    ├── PlayerControllerTests.cs
    └── TurretControllerTests.cs
```

---

## Architettura

Il progetto usa un sistema **event-driven** centralizzato tramite `GameEvents`.

- Nessun Singleton: tutti gli script trovano `GameEvents` tramite `FindObjectOfType`.
- La comunicazione avviene tramite eventi C# (`Action<T>`), non tramite riferimenti diretti.
- La logica pura è separata dalle dipendenze Unity per consentire il testing con NUnit.

```
Coin ──────────► GameEvents.OnCoinCollected ──────────► GameManager
PlayerHealth ──► GameEvents.OnGameOver ───────────────► GameManager
GameManager ───► GameEvents.PublishGameOver
               ► GameEvents.PublishCoinCountChanged
               ► GameEvents.PublishTimeChanged
               ► GameEvents.PublishVictoryConditionMet
```

**Setup scena:** creare un GameObject vuoto chiamato `_EventManager` e aggiungere il componente `GameEvents`.

---

## Script

### Player

#### `PlayerController.cs`
Gestisce il movimento, il salto e le collisioni fisiche del player.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_moveSpeed` | float | 7 | Velocità di movimento (unità/s) |
| `_jumpForce` | float | 6 | Forza del salto (impulso) |
| `_groundCheck` | Transform | — | Punto di controllo contatto col suolo |
| `_groundCheckRadius` | float | 0.2 | Raggio della sfera per il ground check |
| `_groundLayer` | LayerMask | — | Layer considerati "terreno" |
| `_pushForce` | float | 2 | Forza di spinta alla collisione |
| `_recoilDuration` | float | 0.65 | Durata in secondi della paralisi post-collisione |
| `_obstacleDamage` | int | 7 | Danno ricevuto toccando un ostacolo |

**Comportamento:**
- Il movimento è relativo alla direzione della camera (non agli assi del mondo).
- Il salto funziona solo quando il player è a terra (ground check con `Physics.CheckSphere`).
- Se il player tocca un oggetto con tag `Boundaries` → spinta senza danno.
- Se tocca un oggetto con tag `Obstacle` → spinta + danno + recoil (blocco input).

---

#### `PlayerHealth.cs`
Gestisce il sistema di salute del player.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_maxHealth` | int | 100 | Salute massima |
| `_delayBeforeGameOver` | float | 0.5 | Secondi di attesa prima di pubblicare il game over |

**Metodi pubblici:**
- `TakeDamage(int damage)` — riduce la salute, pubblica `OnDamageTaken`. Se scende a 0, pubblica `OnDeath` e avvia la coroutine di game over.
- `Heal(int amount)` — aumenta la salute entro `_maxHealth`, pubblica `OnHealed`.
- `SetMaxHealth(int newMax, bool healToFull)` — modifica la salute massima.

**UnityEvent esposti:**
- `OnHealthChanged(int current, int max)`
- `OnDeath`
- `OnDamageTaken`
- `OnHealed`

---

### Gameplay

#### `Coin.cs`
Moneta collezionabile. Quando il player la tocca, pubblica `OnCoinCollected` tramite `GameEvents` e si autodistrugge.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_scoreValue` | int | 1 | Valore in monete |
| `_timeBonus` | float | 5 | Secondi bonus aggiunti al timer (solo se speciale) |
| `_isSpecial` | bool | false | Se true, aggiunge `_timeBonus` al timer |

---

#### `Door.cs`
Porta che si apre con un'animazione Lerp quando viene chiamato `Open()`.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_openOffset` | Vector3 | (0, 5, 0) | Spostamento della porta quando si apre |
| `_openSpeed` | float | 2 | Velocità di apertura (fattore di Lerp) |

**Metodo pubblico:**
- `Open()` — abilita il movimento verso la posizione aperta.

> Nota: `GameManager` tiene un riferimento alla `Door` ma attualmente non chiama `Open()` automaticamente; il collegamento è predisposto per estensioni future.

---

#### `TurretController.cs`
Torretta nemica che traccia il player e spara proiettili quando è nel suo raggio.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_partToRotate` | Transform | — | Parte della torretta che ruota (es. la testa) |
| `_firePoint` | Transform | — | Punto di spawn dei proiettili |
| `_bulletPrefab` | GameObject | — | Prefab del proiettile |
| `_rotationSpeed` | float | 5 | Velocità di rotazione verso il target (Lerp) |
| `_modelCorrection` | float | 0 | Correzione angolare del modello 3D (-180 / +180) |
| `_fireRate` | float | 1 | Colpi al secondo |

**Comportamento:**
- Rileva il player tramite un `SphereCollider` impostato come trigger.
- Quando il player entra nel raggio, la torretta inizia a ruotare e sparare.
- Il raggio d'attacco è visualizzato nell'Editor come sfera rossa (Gizmo).
- La logica pura (`CanShootNow`, `ResetFireCountdown`, `CalculateTargetRotation`) è separata per il testing.

---

#### `TurretBullet.cs`
Proiettile sparato dalla torretta. Si muove in linea retta, infligge danno al player e si autodistrugge.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_speed` | float | 20 | Velocità (unità/s) |
| `_damage` | int | 10 | Danno inflitto al player |
| `_lifeTime` | float | 1.5 | Tempo di vita massimo in secondi |

**Collisioni:**
- Ignora i collider con `isTrigger = true`.
- Ignora oggetti con tag `Turret` e `Bullet` (non si blocca da soli).
- Infligge danno e si distrugge colpendo il player (`PlayerHealth`).
- Si distrugge colpendo qualsiasi altra superficie solida.

---

#### `CameraOrbit.cs`
Camera in terza persona che orbita attorno al player controllata dal mouse.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_target` | Transform | — | Transform del player |
| `_mouseSensitivity` | float | 2 | Sensibilità del mouse |
| `_distance` | float | 5 | Distanza desiderata dalla camera al player |
| `_minVerticalAngle` | float | -20 | Limite inferiore angolo verticale |
| `_maxVerticalAngle` | float | 70 | Limite superiore angolo verticale |
| `_enableCollision` | bool | true | Abilita il raycast anti-muro |
| `_collisionLayers` | LayerMask | — | Layer che bloccano la camera |
| `_collisionPadding` | float | 0.2 | Spazio cuscinetto tra camera e muro |
| `_positionSmoothTime` | float | 0.12 | Tempo di smorzamento del SmoothDamp |

**Comportamento:**
- Si aggiorna in `LateUpdate` per seguire il player dopo il suo movimento.
- Usa `SmoothDamp` per un movimento fluido.
- Il raycast dal player verso la camera riduce la distanza se c'è un ostacolo.
- `ESC` sblocca il cursore.

---

### UI & GameManager

#### `GameEvents.cs`
Hub centrale degli eventi del gioco. Va aggiunto a un GameObject nella scena (`_EventManager`).

**Eventi C# disponibili:**

| Evento | Firma | Descrizione |
|---|---|---|
| `OnCoinCollected` | `(int amount, float timeBonus, bool isSpecial)` | Moneta raccolta |
| `OnGameOver` | `(bool hasWon)` | Fine partita |
| `OnTimeChanged` | `(float timeRemaining)` | Timer aggiornato |
| `OnCoinCountChanged` | `(int current, int required)` | Contatore monete aggiornato |
| `OnVictoryConditionMet` | `()` | Condizione di vittoria raggiunta |

**Metodi `Publish*`:** wrappano l'invocazione degli eventi con log di debug incluso.

---

#### `GameManager.cs`
Gestisce lo stato globale: timer, monete, vittoria/sconfitta e navigazione tra scene.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_timerText` | Text | — | UI Text del timer |
| `_coinText` | Text | — | UI Text del contatore monete |
| `_victoryImage` | Image | — | Immagine di vittoria |
| `_defeatImage` | Image | — | Immagine di sconfitta |
| `_backToMenuButtonVictory` | Button | — | Bottone "Menu" nella schermata vittoria |
| `_restartButtonDefeat` | Button | — | Bottone "Riprova" nella schermata sconfitta |
| `_backToMenuButtonDefeat` | Button | — | Bottone "Menu" nella schermata sconfitta |
| `_timeRemaining` | float | 120 | Tempo iniziale in secondi |
| `_exitDoor` | Door | — | Riferimento alla porta di uscita |
| `RequiredCoins` | int | 5 | Monete necessarie per vincere |

**Metodi pubblici (testabili):**

| Metodo | Descrizione |
|---|---|
| `IsTimeExpired(float)` | Ritorna `true` se il tempo è ≤ 0 |
| `ShouldPlayerWin(int, int)` | Ritorna `true` se monete ≥ richieste |
| `ConvertTimeToMinutesSeconds(float)` | Converte secondi in `[minuti, secondi]` |
| `FormatTimeToString(int, int)` | Formatta come `"MM:SS"` |
| `ShouldTimerBeRed(float)` | Ritorna `true` se tempo ≤ 15 s |
| `CalculateNewCoinCount(int, int)` | Somma corrente + aggiunta |
| `CalculateNewTimeRemaining(float, float, bool)` | Aggiunge bonus solo se moneta speciale |

---

#### `HealthBar.cs`
Barra di salute UI che si aggiorna automaticamente ascoltando `PlayerHealth.OnHealthChanged`.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_playerHealth` | PlayerHealth | — | Riferimento al componente salute del player |
| `_fillImage` | Image | — | Immagine UI di tipo "Filled" |
| `_healthText` | Text | — | Testo opzionale (es. "75 / 100") |
| `_animateChanges` | bool | true | Abilita animazione smooth del riempimento |
| `_animationSpeed` | float | 5 | Velocità dell'animazione (fattore Lerp) |

---

#### `MenuNavigation.cs`
Gestisce i bottoni del menu principale.

| Campo Inspector | Tipo | Descrizione |
|---|---|---|
| `_newGameButton` | Button | Avvia il gioco (carica scena indice 1) |
| `_exitGameButton` | Button | Chiude l'applicazione |

> `Application.Quit()` funziona solo nella build. Nell'Editor viene usato `EditorApplication.isPlaying = false`.

---

### Debug

#### `CameraDebugVisualization.cs`
Script di debug per visualizzare orientamento e posizione della camera principale.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_forwardLineColor` | Color | Blu | Direzione forward della camera |
| `_upLineColor` | Color | Verde | Direzione up della camera |
| `_rightLineColor` | Color | Rosso | Direzione right della camera |
| `_lineLength` | float | 10 | Lunghezza delle linee di debug |
| `_drawDebugLines` | bool | true | Disegna le linee ogni frame |
| `_logToConsole` | bool | true | Stampa info camera all'avvio |

**Metodi pubblici:**
- `RefreshDebugInfo()` — ristampa le informazioni della camera in console.
- `DrawDebugPoint(Vector3, Color, float)` — disegna un punto di debug nello spazio.

> Da disabilitare nella build finale.

---

#### `HealthTester.cs`
Script di test per infliggere danno o curare il player tramite click del mouse.

| Campo Inspector | Tipo | Default | Descrizione |
|---|---|---|---|
| `_playerHealth` | PlayerHealth | — | Riferimento al player |
| `_damageAmount` | int | 10 | Danno inflitto con click sinistro |
| `_healAmount` | int | 10 | Cura applicata con click destro |

> Solo per sviluppo. Da rimuovere nella build finale.

---

## Test

I test si trovano in `Assets/Tests/` e usano **NUnit** tramite Unity Test Runner. Le dipendenze Unity (Transform, Rigidbody) vengono iniettate via **Reflection** per testare metodi privati.

| File | Classi testate | Test |
|---|---|---|
| `PlayerControllerTests.cs` | `PlayerController` | Rigidbody, salto, recoil, movimento |
| `GameManagerTests.cs` | `GameManager` | Timer, monete, condizioni vittoria, formattazione |
| `TurretControllerTests.cs` | `TurretController`, `TurretBullet` | Rotazione, rateo di fuoco, collisioni proiettile |
| `CameraOrbitTests.cs` | `CameraOrbit` | Angoli orizzontale/verticale, clamping |

---

## Setup

### Tag richiesti
I seguenti tag devono essere configurati in **Edit > Project Settings > Tags and Layers**:

| Tag | Usato da |
|---|---|
| `Player` | TurretController, TurretBullet, Coin |
| `Boundaries` | PlayerController |
| `Obstacle` | PlayerController |
| `Turret` | TurretBullet |
| `Bullet` | TurretBullet |
| `MainCamera` | CameraDebugVisualization |

### Build Settings
| Indice | Scena |
|---|---|
| 0 | Menu principale |
| 1 | Scena di gioco |

### Scena di gioco — componenti richiesti
- Un GameObject `_EventManager` con il componente `GameEvents`.
- Un GameObject `GameManager` con il componente `GameManager` e tutti i riferimenti UI assegnati.
- Il player deve avere `PlayerController`, `PlayerHealth`, `Rigidbody` e un figlio `GroundCheck`.
- La camera deve avere `CameraOrbit` con il target assegnato al player.
