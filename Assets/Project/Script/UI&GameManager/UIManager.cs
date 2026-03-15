using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
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

    // Riferimento locale a GameEvents 
    private GameEvents _gameEvents;

    private void Awake()
    {
        // Cerchiamo il componente GameEvents && GameManager nella scena
        _gameEvents = GameEvents.Instance;
        SubscribeToEvents();

    }

    void Start()
    {
        // Cerchiamo il componente GameEvents && GameManager nella scena
        _gameEvents = GameEvents.Instance;
        // Debug.Log($"[UIManager] GameEvents is null: {_gameEvents == null}");
        // Debug.Log($"[UIManager] Victory button interactable: {_backToMenuButtonVictory?.interactable}");

        if (_victoryImage != null)
        {
            _victoryImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Victory Image NON assegnata!");
        }

        if (_defeatImage != null)
        {
            _defeatImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Defeat Image NON assegnata!");
        }

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

    private void BackToMenu()
    {
        Debug.Log("[UIManager] BackToMenu chiamato");
        GameEvents.Instance.PublishBackToMenuRequested();
    }

    private void RestartGame()
    {
        Debug.Log("[UIManager] RestartGame chiamato");
        GameEvents.Instance.PublishRestartRequested();
    }

    private void HandleCoinCountChanged(int current, int required)
    {
        if (_coinText != null)
            _coinText.text = $"Monete : {current} / {required}";
    }

    private void HandleGameOver(bool hasWon)
    {
        if (hasWon)
        {
            _victoryImage.gameObject.SetActive(true);
        }
        else
        {
            _defeatImage.gameObject.SetActive(true);
        }
    }

    private void HandleTimeChanged(float timeRemaining)
    {
        int[] time = ConvertTimeToMinutesSeconds(timeRemaining);
        _timerText.text = FormatTimeToString(time[0], time[1]);
        _timerText.color = ShouldTimerBeRed(timeRemaining) ? Color.red : Color.white;
    }

    private void SubscribeToEvents()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnCoinCountChanged += HandleCoinCountChanged;
            _gameEvents.OnGameOver += HandleGameOver;
            // _gameEvents.OnBackToMenuRequested += BackToMenu;
            // _gameEvents.OnRestartRequested += RestartGame;
            _gameEvents.OnTimeChanged += HandleTimeChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnCoinCountChanged -= HandleCoinCountChanged;
            _gameEvents.OnGameOver -= HandleGameOver;
            // _gameEvents.OnBackToMenuRequested -= BackToMenu;
            // _gameEvents.OnRestartRequested -= RestartGame;
            _gameEvents.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (_backToMenuButtonVictory != null)
            _backToMenuButtonVictory.onClick.RemoveListener(BackToMenu);
        if (_restartButtonDefeat != null)
            _restartButtonDefeat.onClick.RemoveListener(RestartGame);
        if (_backToMenuButtonDefeat != null)
            _backToMenuButtonDefeat.onClick.RemoveListener(BackToMenu);
    }

    public int[] ConvertTimeToMinutesSeconds(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return new int[] { minutes, seconds };
    }

    public bool ShouldTimerBeRed(float timeRemaining)
    {
        return timeRemaining <= 15f;
    }

    public string FormatTimeToString(int minutes, int seconds)
    {
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
