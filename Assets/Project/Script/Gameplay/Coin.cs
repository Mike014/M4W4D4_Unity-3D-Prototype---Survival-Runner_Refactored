using UnityEngine;

/// Rappresenta una moneta collezionabile nel gioco.
/// 
/// ARCHITETTURA EVENT-DRIVEN PURA (no Singleton):
/// - Pubblica l'evento quando viene raccolta
/// - Non conosce GameManager
public class Coin : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _scoreValue = 1;
    [SerializeField] private float _timeBonus = 5f;
    [SerializeField] private bool _isSpecial = false;

    // Riferimento locale a GameEvents (trovato tramite FindObjectOfType)
    private GameEvents _gameEvents;

    private void Awake()
    {
        // Cerchiamo il componente GameEvents nella scena
        _gameEvents = GameEvents.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[COIN DEBUG] _isSpecial value = {_isSpecial}");
            Debug.Log($"[COIN DEBUG] _scoreValue = {_scoreValue}");
            Debug.Log($"[COIN DEBUG] _timeBonus = {_timeBonus}");

            if (_gameEvents != null)
            {
                _gameEvents.PublishCoinCollected(_scoreValue, _timeBonus, _isSpecial);
            }

            Destroy(gameObject);
        }
    }
}