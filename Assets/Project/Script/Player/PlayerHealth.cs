using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Death Animation Settings")]
    [SerializeField] private float _delayBeforeGameOver = 0.5f;
    #endregion

    #region Private Dependencies
    private GameEvents _gameEvents;
    private PlayerController _controller;
    #endregion

    #region Public Events
    // Publisher puro — nessuno dall'esterno può invocarli
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnDamageTaken;
    public event Action OnHealed;
    #endregion

    #region Public Properties
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;

    // Controlla lo stato vitale del personaggio in tempo reale.
    // Restituisce true se la salute attuale (_currentHealth) è esaurita (<= 0).
    public bool IsDead => _currentHealth <= 0;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Trova GameEvents una volta all'inizio (Caching)
        // cambio di get da awake a metodo 
        _controller = GetComponent<PlayerController>();

        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        // Inizializza l'UI o altri listener dopo che tutto è stato configurato in Awake
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    #endregion

    #region Public Methods (Health Management)
    public void TakeDamage(int damage)
    {
        // Guard Clause: se è già morto, interrompi l'esecuzione
        if (IsDead) return;

        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, _maxHealth);
        Debug.Log($"[TakeDamage Methods]CurrentHealth is : {_currentHealth}");

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnDamageTaken?.Invoke();

        Debug.Log($"Damage Taken: {damage}. Health: {_currentHealth}/{_maxHealth}");

        if (IsDead) Die();
    }

    public void Heal(int healAmount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Clamp(_currentHealth + healAmount, 0, _maxHealth);
        Debug.Log($"[Heal Methods]CurrentHealth is : {_currentHealth}");

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnHealed?.Invoke();

        Debug.Log($"Healed: {healAmount}. Health: {_currentHealth}/{_maxHealth}");
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        _maxHealth = newMaxHealth;
        _currentHealth = healToFull
            ? _maxHealth
            : Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    #endregion

    #region Private Methods (Death Sequence)
    private void Die()
    {
        Debug.Log("Player Died");
        OnDeath?.Invoke();

        if (_controller != null)
            _controller.enabled = false;

        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(_delayBeforeGameOver);

        _gameEvents = GameEvents.Instance;

        // Pubblica l'evento tramite GameEvents (trovato una volta in Awake)
        if (_gameEvents != null)
            _gameEvents.PublishGameOver(false);
        else
            Debug.LogError("[PLAYERHEALTH] GameEvents is null!");
    }
    #endregion
}