using UnityEngine;

/// Gestisce il comportamento di un proiettile sparato da una torretta.
/// Si muove in linea retta, infligge danno al player al contatto,
/// e si distrugge dopo un certo tempo di vita o quando colpisce qualcosa.
/// 
/// REFACTORING: La logica di movimento e collisione è separata dalle dipendenze
/// (Destroy, OnTriggerEnter, GetComponentInParent) per facilitare il testing.
public class TurretBullet : MonoBehaviour
{
    [Header("Settings")]
    // Velocità di movimento del proiettile (unità al secondo)
    [SerializeField] private float _speed = 20f;
    // Danno inflitto al player se colpito direttamente
    [SerializeField] private int _damage = 10;
    // Tempo massimo di vita del proiettile (evita che rimanga in gioco infinitamente)
    [SerializeField] private float _lifeTime = 1.5f;

    void Start()
    {
        // DISTRUZIONE AUTOMATICA DOPO TIMEOUT
        // Se il proiettile non colpisce nulla entro _lifeTime secondi, si distrugge
        // Questo previene accumulo di proiettili persi nello spazio (memory leak)
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        // MOVIMENTO IN LINEA RETTA
        // Delega il calcolo del movimento a metodo puro, poi applica il risultato
        Vector3 movement = CalculateMovement(_speed, Time.deltaTime);
        transform.Translate(movement);
    }

    /// Logica pura: Calcola il vettore di movimento per questo frame.
    /// Non modifica stato, non dipende da nient'altro.
    private Vector3 CalculateMovement(float speed, float deltaTime)
    {
        // MOVIMENTO IN LINEA RETTA
        // Translate muove l'oggetto nella direzione specificata
        // Vector3.forward = (0, 0, 1) nella direzione "avanti" dell'oggetto locale
        // speed * deltaTime: calcola la distanza da percorrere questo frame
        // (Time.deltaTime assicura movimento frame-rate indipendente)
        return Vector3.forward * speed * deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // DEBUG: Log nella console per vedere cosa abbiamo colpito durante lo sviluppo
        Debug.Log($"Proiettile ha colpito: {other.name} (Tag: {other.tag})");

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_damage);
            
        }
        Destroy(gameObject);
    }
}