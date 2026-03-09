using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEvents : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════
    // EVENTI PUBBLICI - Chiunque può sottoscrivere
    // ════════════════════════════════════════════════════════════════

    // L'istanza statica — unica per tutta la durata del gioco
    public static GameEvents Instance { get; private set; }

    // Eventi collegati a UIManager
    public event Action OnBackToMenuRequested;
    public event Action OnRestartRequested;

    /// Pubblicato quando il player raccoglie una moneta.
    /// I parametri sono: (amount, timeBonus, isSpecial)
    public event Action<int, float, bool> OnCoinCollected;

    /// Pubblicato quando la partita termina (vittoria o sconfitta).
    /// Il parametro è: (hasWon)
    public event Action<bool> OnGameOver;

    /// Pubblicato ogni volta che il timer cambia.
    /// Il parametro è: (timeRemaining)
    public event Action<float> OnTimeChanged;

    /// Pubblicato quando il contatore di monete cambia.
    /// Il parametro è: (currentCoins, requiredCoins)
    public event Action<int, int> OnCoinCountChanged;

    /// Pubblicato quando il player ha abbastanza monete per vincere
    public event Action OnVictoryConditionMet;

    // ════════════════════════════════════════════════════════════════
    // METODI MonoBehaviour
    // ════════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ════════════════════════════════════════════════════════════════
    // METODI PUBBLICI - Per pubblicare gli eventi
    // ════════════════════════════════════════════════════════════════

    public void PublishCoinCollected(int amount, float timeBonus, bool isSpecial)
    {
        Debug.Log($"[GameEvents] Coin Collected: +{amount} points, +{timeBonus}s bonus, Special={isSpecial}");
        OnCoinCollected?.Invoke(amount, timeBonus, isSpecial);
    }

    public void PublishGameOver(bool hasWon)
    {
        Debug.Log($"[GameEvents] Game Over - Won: {hasWon}");
        OnGameOver?.Invoke(hasWon);
    }

    public void PublishTimeChanged(float timeRemaining)
    {
        OnTimeChanged?.Invoke(timeRemaining);
    }

    public void PublishCoinCountChanged(int currentCoins, int requiredCoins)
    {
        Debug.Log($"[GameEvents] Coin Count Changed: {currentCoins}/{requiredCoins}");
        OnCoinCountChanged?.Invoke(currentCoins, requiredCoins);
    }

    public void PublishVictoryConditionMet()
    {
        Debug.Log("[GameEvents] Victory Condition Met!");
        OnVictoryConditionMet?.Invoke();
    }

    public void PublishBackToMenuRequested()
    {
        Debug.Log("[GameEvents] Back To Menu Requested");
        OnBackToMenuRequested?.Invoke();
    }

    public void PublishRestartRequested()
    {
        Debug.Log("[GameEvents] Restart Requested");
        OnRestartRequested?.Invoke();
    }

    public void ResetEvents()
    {
        OnCoinCollected = null;
        OnGameOver = null;
        OnTimeChanged = null;
        OnCoinCountChanged = null;
        OnVictoryConditionMet = null;
        OnBackToMenuRequested = null;
        OnRestartRequested = null;

        Debug.Log("[GameEvents] All events reset");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetEvents();
    }
}



