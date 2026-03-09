using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// Gestisce lo stato globale del gioco: timer, monete, vittoria/sconfitta.
/// 
/// ARCHITETTURA EVENT-DRIVEN PURA (no Singleton):
/// - Trova GameEvents tramite FindObjectOfType (non Singleton)
/// - ASCOLTA gli eventi pubblicati da Coin e PlayerHealth
/// - PUBBLICA gli eventi che altri script devono conoscere
/// - Logica pura rimane testabile e separata
/// 
/// Vantaggi:
/// ✓ GameManager non è un Singleton
/// ✓ GameEvents non è un Singleton
/// ✓ Nessun accoppiamento statico
/// ✓ Testabile: puoi creare istanze fake
public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _coinText;

    [Header("End Game UI")]
    [SerializeField] private Image _victoryImage;
    [SerializeField] private Button _backToMenuButtonVictory;
    [SerializeField] private Image _defeatImage;
    [SerializeField] private Button _restartButtonDefeat;
    [SerializeField] private Button _backToMenuButtonDefeat;

    [Header("Timer Settings")]
    [SerializeField] private float _timeRemaining = 120f;

    [Header("Level Settings")]
    [SerializeField] private Door _exitDoor;
    public int RequiredCoins = 5;

    // ════════════════════════════════════════════════════════════════
    // VARIABILI INTERNE
    // ════════════════════════════════════════════════════════════════

    private int _currentCoins = 0;
    private bool _isGameOver = false;
    private bool _hasWon = false;
    
    // Riferimento locale a GameEvents (trovato tramite FindObjectOfType)
    private GameEvents _gameEvents;

    // ════════════════════════════════════════════════════════════════
    // LIFECYCLE UNITY
    // ════════════════════════════════════════════════════════════════

    void Start()
    {
        // Cerchiamo il componente GameEvents nella scena
        _gameEvents = GameEvents.Instance; 

        // Sottoscrivi agli eventi
        SubscribeToEvents();

        // Setup UI
        Time.timeScale = 1f;

        if (_victoryImage != null)
        {
            _victoryImage.enabled = false;
        }
        else
        {
            Debug.LogError("Victory Image NON assegnata!");
        }

        if (_defeatImage != null)
        {
            _defeatImage.enabled = false;
        }
        else
        {
            Debug.LogError("Defeat Image NON assegnata!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateCoinDisplay();

        if (_timerText == null || _coinText == null)
        {
            Debug.LogError("Assegna i componenti Text (Timer e Coins) nell'Inspector!");
        }

        if (_backToMenuButtonVictory != null)
        {
            _backToMenuButtonVictory.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Victory) non assegnato!");
        }

        if (_restartButtonDefeat != null)
        {
            _restartButtonDefeat.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Restart Button (Defeat) non assegnato!");
        }

        if (_backToMenuButtonDefeat != null)
        {
            _backToMenuButtonDefeat.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Back To Menu Button (Defeat) non assegnato!");
        }
    }

    void Update()
    {
        if (_isGameOver) return;

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(_timeRemaining);
            // Pubblica l'evento che la UI può ascoltare
            if (_gameEvents != null)
            {
                _gameEvents.PublishTimeChanged(_timeRemaining);
            }
        }
        else
        {
            _timeRemaining = 0;
            UpdateTimerDisplay(0);
            PublishGameOverEvent(false); // Sconfitta per timeout
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        if (_backToMenuButtonVictory != null)
        {
            _backToMenuButtonVictory.onClick.RemoveListener(BackToMenu);
        }

        if (_restartButtonDefeat != null)
        {
            _restartButtonDefeat.onClick.RemoveListener(RestartGame);
        }

        if (_backToMenuButtonDefeat != null)
        {
            _backToMenuButtonDefeat.onClick.RemoveListener(BackToMenu);
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SUBSCRIPTION MANAGEMENT
    // ════════════════════════════════════════════════════════════════

    private void SubscribeToEvents()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnCoinCollected += HandleCoinCollected;
            _gameEvents.OnGameOver += HandleGameOver;
            Debug.Log("[GameManager] Event listeners registered");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnCoinCollected -= HandleCoinCollected;
            _gameEvents.OnGameOver -= HandleGameOver;
            Debug.Log("[GameManager] Event listeners unregistered");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ════════════════════════════════════════════════════════════════

    private void HandleCoinCollected(int amount, float timeBonus, bool isSpecial)
    {
        if (_isGameOver) return;

        _currentCoins = CalculateNewCoinCount(_currentCoins, amount);
        _timeRemaining = CalculateNewTimeRemaining(_timeRemaining, timeBonus, isSpecial);

        if (isSpecial)
        {
            Debug.Log($"⭐ MONETA SPECIALE RACCOLTA! +{timeBonus} secondi bonus!");
        }
        else
        {
            Debug.Log($"Moneta normale raccolta. +0 secondi.");
        }

        UpdateCoinDisplay();

        if (_gameEvents != null)
        {
            _gameEvents.PublishCoinCountChanged(_currentCoins, RequiredCoins);
        }

        if (ShouldPlayerWin(_currentCoins, RequiredCoins))
        {
            if (_gameEvents != null)
            {
                _gameEvents.PublishVictoryConditionMet();
            }
            PublishGameOverEvent(true);
        }
    }

    private void HandleGameOver(bool hasWon)
    {
        if (_isGameOver) return;
        PublishGameOverEvent(hasWon);
    }

    // ════════════════════════════════════════════════════════════════
    // LOGICA PURA - Testabile
    // ════════════════════════════════════════════════════════════════

    public bool IsTimeExpired(float timeRemaining)
    {
        return timeRemaining <= 0f;
    }

    public bool ShouldPlayerWin(int currentCoins, int requiredCoins)
    {
        return currentCoins >= requiredCoins;
    }

    public int[] ConvertTimeToMinutesSeconds(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return new int[] { minutes, seconds };
    }

    public string FormatTimeToString(int minutes, int seconds)
    {
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public bool ShouldTimerBeRed(float timeRemaining)
    {
        return timeRemaining <= 15f;
    }

    public int CalculateNewCoinCount(int currentCoins, int amountToAdd)
    {
        return currentCoins + amountToAdd;
    }

    public float CalculateNewTimeRemaining(float currentTime, float timeBonus, bool isSpecial)
    {
        if (isSpecial)
        {
            return currentTime + timeBonus;
        }
        return currentTime;
    }

    // ════════════════════════════════════════════════════════════════
    // UI MANAGEMENT
    // ════════════════════════════════════════════════════════════════

    private void UpdateCoinDisplay()
    {
        if (_coinText != null)
        {
            _coinText.text = "Monete: " + _currentCoins + " / " + RequiredCoins;
        }
    }

    private void UpdateTimerDisplay(float timeToDisplay)
    {
        int[] timeComponents = ConvertTimeToMinutesSeconds(timeToDisplay);
        int minutes = timeComponents[0];
        int seconds = timeComponents[1];

        string formattedTime = FormatTimeToString(minutes, seconds);
        _timerText.text = formattedTime;

        if (ShouldTimerBeRed(timeToDisplay))
        {
            _timerText.color = Color.red;
        }
        else
        {
            _timerText.color = Color.white;
        }
    }

    private void PublishGameOverEvent(bool hasWon)
    {
        if (_isGameOver) return;

        _isGameOver = true;
        _hasWon = hasWon;

        Debug.Log($"[GameManager] Publishing GameOver event - HasWon: {hasWon}");

        if (_gameEvents != null)
        {
            _gameEvents.PublishGameOver(hasWon);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (hasWon)
        {
            if (_victoryImage != null)
            {
                _victoryImage.gameObject.SetActive(true);
                Debug.Log("🎉 HAI VINTO!");
            }
        }
        else
        {
            if (_defeatImage != null)
            {
                _defeatImage.gameObject.SetActive(true);
                Debug.Log("💀 HAI PERSO!");
            }
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SCENE MANAGEMENT
    // ════════════════════════════════════════════════════════════════

    private void BackToMenu()
    {
        _isGameOver = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Ritorno al menu principale...");
        SceneManager.LoadScene(0);
    }

    private void RestartGame()
    {
        _isGameOver = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Riavvio della partita...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UnlockDoor()
    {
        if (_exitDoor != null)
        {
            _exitDoor.Open();
        }
    }
}