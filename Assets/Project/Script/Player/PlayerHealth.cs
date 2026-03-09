using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Gestisce il sistema di salute del player.
/// 
/// ARCHITETTURA EVENT-DRIVEN PURA (no Singleton):
/// - Trova GameEvents tramite FindObjectOfType
/// - Pubblica l'evento quando il player muore
/// - Non conosce GameManager
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnHealed;

    [Header("Death Animation Settings")]
    [SerializeField] private float _delayBeforeGameOver = 0.5f;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    private GameEvents _gameEvents;

    void Start()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        // Trova GameEvents una volta all'inizio
        _gameEvents = GameEvents.Instance; 
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnDamageTaken?.Invoke();

        Debug.Log($"Damage Taken: {damage}. Health: {_currentHealth}/{_maxHealth}");

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (IsDead) return;

        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        OnHealed?.Invoke();

        Debug.Log($"Healed: {healAmount}. Health: {_currentHealth}/{_maxHealth}");
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        _maxHealth = newMaxHealth;

        if (healToFull)
        {
            _currentHealth = _maxHealth;
        }
        else
        {
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died");
        OnDeath?.Invoke();

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(_delayBeforeGameOver);

        // ✅ Pubblica l'evento tramite GameEvents (trovato una volta in Start)
        if (_gameEvents != null)
        {
            _gameEvents.PublishGameOver(false);
        }
        else
        {
            Debug.LogError("[PLAYERHEALTH] GameEvents is null!");
        }
    }
}