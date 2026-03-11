using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestisce la visualizzazione della barra di salute del player.
/// Ascolta gli eventi OnHealthChanged da PlayerHealth e aggiorna il UI di conseguenza.
/// Supporta animazione smooth del riempimento della barra con opzione disattivabile.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    // Riferimento al componente PlayerHealth che traccia la salute del player
    [SerializeField] private PlayerHealth _playerHealth;
    // Immagine UI che rappresenta la barra di salute (Image con tipo "Filled")
    [SerializeField] private Image _fillImage;
    // Testo UI che mostra la salute numerica (es: "75 / 100")
    [SerializeField] private Text _healthText;

    [Header("Animation Settings")]
    // Flag che abilita/disabilita l'animazione smooth del riempimento della barra
    [SerializeField] private bool _animateChanges = true;
    // Velocità dell'animazione di riempimento (fattore di Lerp)
    // Valori tipici: 1-10 (più alto = più veloce)
    [SerializeField] private float _animationSpeed = 5f;

    // ════════════════════════════════════════════════════════════════
    // VARIABILI INTERNE
    // ════════════════════════════════════════════════════════════════

    // Il valore target di fillAmount verso cui animare la barra
    // Varia da 0 (completamente vuota) a 1 (completamente piena)
    private float _targetFillAmount = 1f;

    /// <summary>
    /// Inizializza la health bar all'avvio della scena.
    /// Si sottoscrive all'evento OnHealthChanged del PlayerHealth per aggiornarsi automaticamente.
    /// </summary>
    void Start()
    {
        // VALIDAZIONE E SETUP
        if (_playerHealth != null)
        {
            // SUBSCRIBE ALL'EVENTO
            // AddListener registra questo script per ascoltare OnHealthChanged
            // Ogni volta che la salute cambia, UpdateHealthBar() sarà invocato automaticamente
            // Firma dell'evento: OnHealthChanged<int, int> (currentHealth, maxHealth)
            // _playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            
            // INIZIALIZZAZIONE
            // Aggiorna il display iniziale con la salute attuale
            UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
        else
        {
            // DEBUG: Se PlayerHealth non è assegnato, avvisa lo sviluppatore
            Debug.LogError("PlayerHealth reference missing on HealthBar!");
        }
    }

    /// <summary>
    /// Pulisce i listener quando la barra di salute viene distrutta.
    /// È importante disiscriversi dagli eventi per evitare memory leaks.
    /// </summary>
    void OnDestroy()
    {
        // UNSUBSCRIBE DALL'EVENTO
        // Rimuove questo script dalla lista di listener di OnHealthChanged
        // Importante: se non lo fai, PlayerHealth manterrà un riferimento a uno script distrutto
        // Causando "trying to invoke a function on a destroyed object" error
        if (_playerHealth != null)
        {
            // _playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }

    /// <summary>
    /// Aggiorna la posizione di fillAmount della barra verso il valore target ogni frame.
    /// Usa Lerp per creare un'animazione smooth se abilitata.
    /// </summary>
    void Update()
    {
        // ANIMAZIONE SMOOTH (SE ABILITATA)
        if (_animateChanges && _fillImage != null)
        {
            // Interpola smoothly il valore attuale di fillAmount verso il target
            // Parametri:
            //   - _fillImage.fillAmount: valore attuale (0-1)
            //   - _targetFillAmount: valore desiderato (calcolato in UpdateHealthBar())
            //   - _animationSpeed * Time.deltaTime: fattore di interpolazione
            //     Più alto _animationSpeed, più veloce la transizione
            _fillImage.fillAmount = Mathf.Lerp(
                _fillImage.fillAmount, 
                _targetFillAmount, 
                _animationSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Aggiorna la visualizzazione della barra e il testo della salute.
    /// Viene invocato automaticamente quando OnHealthChanged viene disparato.
    /// Supporta sia animazione che aggiornamento istantaneo.
    /// </summary>
    /// <param name="currentHealth">Salute attuale del player (valore assoluto)</param>
    /// <param name="maxHealth">Salute massima del player (valore assoluto)</param>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // PROTEZIONE: Se l'immagine della barra non esiste, non fare nulla
        if (_fillImage == null) return;

        // CALCOLO PERCENTUALE
        // Converti i valori assoluti (75/100) in percentuale (0.75)
        // Cast a float è necessario: divisione tra int darebbe risultato troncato
        // Es: 75 / 100 = 0 (int), 75f / 100f = 0.75f (float)
        float healthPercentage = (float)currentHealth / (float)maxHealth;

        // APPLICAZIONE DEL VALORE
        if (_animateChanges)
        {
            // Modalità ANIMATA
            // Salva il target e lascia che Update() lo raggiunga gradualmente
            // Questo crea un effetto smooth di "svuotamento" della barra
            _targetFillAmount = healthPercentage;
        }
        else
        {
            // Modalità ISTANTANEA
            // Applica il valore immediatamente senza transizione
            // Utile se preferisci aggiornamenti rapidi (meno "dolce" visivamente)
            _fillImage.fillAmount = healthPercentage;
        }

        // AGGIORNAMENTO TESTO (OPZIONALE)
        // Mostra il valore numerico di salute (es: "75 / 100")
        if (_healthText != null)
        {
            // Usa string interpolation ($"...") per formattare la stringa
            // Equivalente a: $"{currentHealth}/{maxHealth}"
            _healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
}