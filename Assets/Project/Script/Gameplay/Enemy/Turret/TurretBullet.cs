using UnityEngine;

/// <summary>
/// Gestisce il comportamento di un proiettile sparato da una torretta.
/// Si muove in linea retta, infligge danno al player al contatto,
/// e si distrugge dopo un certo tempo di vita o quando colpisce qualcosa.
/// 
/// REFACTORING: La logica di movimento e collisione è separata dalle dipendenze
/// (Destroy, OnTriggerEnter, GetComponentInParent) per facilitare il testing.
/// </summary>
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

    /// <summary>
    /// Logica pura: Calcola il vettore di movimento per questo frame.
    /// Non modifica stato, non dipende da nient'altro.
    /// </summary>
    private Vector3 CalculateMovement(float speed, float deltaTime)
    {
        // MOVIMENTO IN LINEA RETTA
        // Translate muove l'oggetto nella direzione specificata
        // Vector3.forward = (0, 0, 1) nella direzione "avanti" dell'oggetto locale
        // speed * deltaTime: calcola la distanza da percorrere questo frame
        // (Time.deltaTime assicura movimento frame-rate indipendente)
        return Vector3.forward * speed * deltaTime;
    }

    /// <summary>
    /// Logica pura: Determina se il proiettile dovrebbe essere distrutto in base a una collisione.
    /// Esamina il collider colpito e decide se è un ostacolo, il player, o passabile (turret/bullet).
    /// ✅ TESTABILE: Puoi passare qualsiasi Collider e verificare il risultato
    /// </summary>
    private bool ShouldDestroyBullet(Collider other, out bool playerHit)
    {
        playerHit = false;

        // STEP 1: IGNORA I TRIGGER (SENSORI E ZONE DI ATTIVAZIONE)
        // I collider con isTrigger == true sono usati per rilevare posizioni, non per collisioni fisiche
        // Se colpissimo un trigger, continueremmo a volare attraverso (comportamento indesiderato)
        if (other.isTrigger)
        {
            return false;  // Non distruggere il proiettile su un trigger
        }

        // STEP 2: CERCA IL COMPONENTE PLAYERHEALTH SUL TARGET
        // GetComponentInParent cerca il componente NON SOLO sul collider stesso,
        // ma anche su tutti i parent GameObject
        // Questo è importante se il collider è su un "bone" (es. player arm/leg mesh)
        // e il componente PlayerHealth è sul root GameObject del player
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        // Se abbiamo trovato PlayerHealth, il target è il player
        if (playerHealth != null)
        {
            playerHit = true;  // Flag per sapere che abbiamo colpito il player
            return true;       // Distruggi il proiettile
        }

        // Se NON è il player, controlla se è un oggetto che NON deve fermare il proiettile
        // (Turret che ha sparato e Bullet di altre torrette devono passare attraverso)
        if (!other.CompareTag("Turret") && !other.CompareTag("Bullet"))
        {
            // Se non è Player, non è Turret e non è Bullet, allora è un muro/ostacolo
            return true;  // Distruggi il proiettile (ha colpito una superficie solida)
        }

        // Se la condizione è falsa significa che è o una Turret o un Bullet
        // In questo caso NON distruggere il proiettile (continua a volare)
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        // DEBUG: Log nella console per vedere cosa abbiamo colpito durante lo sviluppo
        Debug.Log($"Proiettile ha colpito: {other.name} (Tag: {other.tag})");

        // STEP 1: DETERMINA SE IL PROIETTILE DEVE ESSERE DISTRUTTO
        // Usa la logica pura ShouldDestroyBullet() per decidere
        bool playerHit = false;
        bool shouldDestroy = ShouldDestroyBullet(other, out playerHit);

        // STEP 2: SE HA COLPITO IL PLAYER, INFLIGGI DANNO
        if (playerHit)
        {
            // Cerca di nuovo il componente PlayerHealth (è già stato trovato in ShouldDestroyBullet)
            // Idealmente, dovremmo passarlo come parametro, ma per ora facciamo così per chiarezza
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
            }
        }

        // STEP 3: SE DEVE ESSERE DISTRUTTO, DISTRUGGI IL PROIETTILE
        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }
}