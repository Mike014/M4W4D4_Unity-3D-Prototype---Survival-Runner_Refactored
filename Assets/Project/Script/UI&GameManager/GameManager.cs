using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float _timeRemaining = 120f;

    [Header("Level Settings")]
    // [SerializeField] private Door _exitDoor;
    public int RequiredCoins = 5;

    // ════════════════════════════════════════════════════════════════
    // VARIABILI INTERNE
    // ════════════════════════════════════════════════════════════════
    private int _currentCoins = 0;
    private bool _isGameOver = false;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_isGameOver) return;

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            // Pubblica l'evento che la UI può ascoltare
            if (_gameEvents != null)
            {
                _gameEvents.PublishTimeChanged(_timeRemaining);
            }
        }
        else
        {
            _timeRemaining = 0;
            PublishGameOverEvent(false); // Sconfitta per timeout
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
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
            _gameEvents.OnBackToMenuRequested += BackToMenu;
            _gameEvents.OnRestartRequested += RestartGame;
            Debug.Log("[GameManager] Event listeners registered");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnCoinCollected -= HandleCoinCollected;
            _gameEvents.OnGameOver -= HandleGameOver;
            _gameEvents.OnBackToMenuRequested -= BackToMenu;
            _gameEvents.OnRestartRequested -= RestartGame;
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

    private void PublishGameOverEvent(bool hasWon)
    {
        // Debug.Log($"Has Won è : {hasWon}");
        if (_isGameOver) return;
        _isGameOver = true;
        _gameEvents.PublishGameOver(hasWon);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ════════════════════════════════════════════════════════════════
    // SCENE MANAGEMENT
    // ════════════════════════════════════════════════════════════════

    private void BackToMenu()
    {
        Debug.Log("[UIManager] BackToMenu chiamato");
        _isGameOver = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Ritorno al menu principale...");
        // SceneManager.LoadScene(0);
        SceneManager.LoadScene("MainMenu");

    }

    private void RestartGame()
    {
        Debug.Log("[UIManager] RestartGame chiamato");
        _isGameOver = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Riavvio della partita...");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Level1");
    }

    // private void UnlockDoor()
    // {
    //     if (_exitDoor != null)
    //     {
    //         _exitDoor.Open();
    //     }
    // }
}