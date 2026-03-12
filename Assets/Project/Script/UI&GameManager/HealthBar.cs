using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    #region Inspector Variables
    [Header("References")]
    // Riferimento al componente PlayerHealth che traccia la salute del player
    [SerializeField] private PlayerHealth _playerHealth;

    // Immagine UI che rappresenta la barra di salute (Image con tipo "Filled")
    [SerializeField] private Image _fillImage;

    [Header("Animation Settings")]
    // Flag che abilita/disabilita l'animazione smooth del riempimento della barra
    [SerializeField] private bool _animateChanges = true;

    // Velocità dell'animazione di riempimento (fattore di Lerp)
    // Valori tipici: 1-10 (più alto = più veloce)
    [SerializeField, Range(1f, 10f)] private float _animationSpeed = 5f;
    #endregion

    #region Private State
    // Il valore target di fillAmount verso cui animare la barra
    // Varia da 0 (completamente vuota) a 1 (completamente piena)
    private float _targetFillAmount = 1f;
    #endregion

    private void Update()
    {
        if (!_animateChanges || _fillImage == null) return;

        _fillImage.fillAmount = Mathf.MoveTowards(_fillImage.fillAmount, _targetFillAmount, _animationSpeed * Time.deltaTime);
    }

    #region Unity Event Subscription
    // Quando PlayerHealth invoca OnHealthChanged, HealthBar reagisce automaticamente. 
    private void OnEnable()
    {
        if (_playerHealth == null)
        {
            Debug.LogError("[HealthBar] PlayerHealth reference missing!!");
            return;
        }

        _playerHealth.OnHealthChanged += UpdateHealthBar;
        
        // Sincronizza lo stato iniziale appena ci si iscrive per evitare che la barra parta piena/vuota erroneamente
        UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
    }

    private void OnDisable()
    {
        // Importante: disiscriversi sempre per evitare memory leak o errori su oggetti distrutti
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
    #endregion

    #region UI Update Logic
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (_fillImage == null) return;

        // Calcolo della percentuale con cast esplicito a float per evitare la divisione intera (che darebbe sempre 0 o 1)
        float percentage = (float)currentHealth / maxHealth;

        if (_animateChanges)
        {
            _targetFillAmount = percentage;
        }
        else
        {
            _fillImage.fillAmount = percentage;
            _targetFillAmount = percentage; // Sincronizziamo comunque il target per evitare "salti" se l'animazione viene riattivata
        }
    }
    #endregion
}