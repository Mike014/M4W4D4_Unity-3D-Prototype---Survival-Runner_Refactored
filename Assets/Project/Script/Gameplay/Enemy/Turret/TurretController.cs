using UnityEngine;
/// Gestisce il comportamento di una torretta che traccia il player e spara proiettili.
/// Ruota verso il player quando è nel raggio di rilevamento, mantiene un rateo di fuoco costante,
/// e visualizza il raggio di attacco nell'editor per debug.
/// 
/// REFACTORING: La logica di rotazione, fuoco e calcoli è separata dalle dipendenze
/// (Transform, Instantiate, FindGameObjectWithTag) per facilitare il testing.
public class TurretController : MonoBehaviour
{
    [Header("References")]
    // Transform della parte che ruota (es. la "testa" della torretta)
    [SerializeField] private Transform _partToRotate;
    // Transform del punto da cui escono i proiettili (generalmente la "bocca" della torretta)
    [SerializeField] private Transform _firePoint;
    // Prefab del proiettile da istanziare quando spariamo
    [SerializeField] private GameObject _bulletPrefab;

    [Header("Settings")]
    // Velocità di rotazione della torretta verso il target (lerp factor)
    [SerializeField] private float _rotationSpeed = 5f;
    // Correzione manuale dell'angolo di rotazione (utile se il modello 3D non è allineato)
    // Range: da -180 a 180 gradi per evitare valori nonsensali
    [Range(-180f, 180f)]
    [SerializeField] private float _modelCorrection = 0f;

    [Header("Combat Settings")]
    // Rateo di fuoco: quanti colpi al secondo (es. 2 = 2 colpi/sec = 1 colpo ogni 0.5s)
    [SerializeField] private float _fireRate = 1f;

    // ───────────────────────────────────────────────────────────────────────────────
    // VARIABILI INTERNE
    // ───────────────────────────────────────────────────────────────────────────────

    // Transform del player (cachato per evitare FindGameObjectWithTag() ogni frame)
    private Transform _playerTransform;
    // Flag che indica se il player è attualmente dentro il raggio di rilevamento della torretta
    private bool _isPlayerInRange = false;
    // Timer che conta i secondi rimasti prima del prossimo sparo
    // (scende da (1/_fireRate) a 0, poi resetta)
    private float _fireCountdown = 0f;

    void Start()
    {
        // INIZIALIZZAZIONE: TROVA IL PLAYER
        // FindGameObjectWithTag è costoso, ma viene fatto solo una volta in Start()
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            _playerTransform = playerObj.transform;
        }

        // FALLBACK: Se _partToRotate non è assegnata, usa il transform della torretta stessa
        if (_partToRotate == null) 
        {
            _partToRotate = transform;
        }
    }

    void Update()
    {
        // PROTEZIONE: Se il player non esiste, non fare nulla
        if (_playerTransform == null) return;

        // ESECUZIONE LOGICA PRINCIPALE SOLO SE PLAYER È IN RANGE
        // Evita calcoli inutili quando il player è lontano (ottimizzazione)
        if (_isPlayerInRange)
        {
            // STEP 1: TRACCIA E RUOTA VERSO IL PLAYER
            TrackPlayer();

            // STEP 2: GESTIONE DELLO SPARO
            // Controlla se è tempo di sparare usando la logica pura CanShootNow()
            if (CanShootNow(_fireCountdown))
            {
                Shoot();
                // Resetta il timer usando la logica pura ResetFireCountdown()
                _fireCountdown = ResetFireCountdown(_fireRate);
            }

            // DECREMENTO TIMER
            // Ogni frame, il timer scende di Time.deltaTime secondi
            // Quando raggiunge 0, è tempo di sparare di nuovo
            _fireCountdown -= Time.deltaTime;
        }
    }

    /// Logica pura: Controlla se è tempo di sparare.
    /// Non modifica stato, restituisce solo true/false.
    /// TESTABILE: Puoi passare qualsiasi fireCountdown
    private bool CanShootNow(float fireCountdown)
    {
        // Se il countdown è sceso a 0 o sotto, è tempo di sparare
        return fireCountdown <= 0f;
    }

    /// Logica pura: Calcola il nuovo countdown per il prossimo sparo.
    /// TESTABILE: Pura matematica (1 / fireRate)
    private float ResetFireCountdown(float fireRate)
    {
        // Resetta il timer: 1 / _fireRate = intervallo tra spari
        // Es: fireRate=2 → 1/2 = 0.5s tra un sparo e l'altro
        return 1f / fireRate;
    }

    /// Calcola il movimento del timer nel tempo.
    /// TESTABILE: Pura logica delta-time
    private float UpdateFireCountdown(float currentCountdown, float deltaTime)
    {
        // Decrementa il countdown di un frame
        return currentCountdown - deltaTime;
    }

    /// Calcola il quaternione di rotazione desiderato per guardare il player.
    /// Logica pura: non modifica state, non dipende da Transform/Input.
    /// TESTABILE: Puoi passare qualsiasi playerPos e turretPos
    private Quaternion CalculateTargetRotation(Vector3 playerPos, Vector3 turretPos, float modelCorrection)
    {
        // CALCOLO DELLA DIREZIONE
        Vector3 direction = playerPos - turretPos;
        
        // APPIATTISCI L'ASSE Y (MOVIMENTO SOLO ORIZZONTALE)
        // Azzera la componente Y della direzione
        direction.y = 0;

        // PROTEZIONE: Se la direzione è zero, ritorna rotazione identity
        // LookRotation() non può lavorare con un vettore zero
        if (direction == Vector3.zero)
        {
            return Quaternion.identity;
        }

        // CREA IL QUATERNIONE DI ROTAZIONE
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // APPLICA LA CORREZIONE DEL MODELLO
        Quaternion correctedTarget = lookRotation * Quaternion.Euler(0f, modelCorrection, 0f);

        return correctedTarget;
    }

    /// Calcola la rotazione del turret verso il player e la applica.
    /// Delega il calcolo a CalculateTargetRotation() (testabile)
    /// e applica il risultato con Lerp (smooth).
    void TrackPlayer()
    {
        // STEP 1: CALCOLA LA ROTAZIONE DESIDERATA (Logica pura)
        Quaternion targetRotation = CalculateTargetRotation(
            _playerTransform.position,
            _partToRotate.position,
            _modelCorrection
        );

        // STEP 2: APPLICA LA ROTAZIONE CON SMOOTHING
        // Lerp interpola tra la rotazione attuale e la rotazione target
        // Parametri:
        //   - _partToRotate.rotation: rotazione attuale
        //   - targetRotation: rotazione desiderata (calcolata sopra)
        //   - Time.deltaTime * _rotationSpeed: fattore di interpolazione (velocità)
        // Più alto _rotationSpeed, più veloce la rotazione (max 1 = istantanea)
        _partToRotate.rotation = Quaternion.Lerp(
            _partToRotate.rotation,
            targetRotation,
            Time.deltaTime * _rotationSpeed
        );
    }

    /// Istanzia un proiettile al firePoint e lo orienta verso il player.
    /// La rotazione del proiettile viene forzata per garantire che voli dritto.
    void Shoot()
    {
        // VALIDAZIONE: Controlla che tutti i riferimenti necessari esistono
        if (_bulletPrefab != null && _firePoint != null)
        {
            // STEP 1: ISTANZIA IL PROIETTILE
            // Instantiate crea una copia del prefab alla posizione e rotazione del firePoint
            // Parametri:
            //   - _bulletPrefab: il prefab da clonare
            //   - _firePoint.position: posizione dello spawn (punta della torretta)
            //   - _firePoint.rotation: rotazione iniziale (dovrebbe già puntare verso il player)
            GameObject bulletObj = Instantiate(
                _bulletPrefab, 
                _firePoint.position, 
                _firePoint.rotation
            );

            // STEP 2: CORREZIONE FORZATA DELLA ROTAZIONE DEL PROIETTILE
            // Anche se la torretta è orientata correttamente, a volte il firePoint
            // potrebbe non essere perfettamente allineato (errori di setup nel modello)
            // LookAt forza il proiettile a guardare direttamente il player istantaneamente
            if (_playerTransform != null)
            {
                bulletObj.transform.LookAt(_playerTransform);
            }
        }
        else
        {
            // DEBUG: Se mancano i reference, avvisa lo sviluppatore nella console
            Debug.LogError("Manca BulletPrefab o FirePoint sulla torretta!");
        }
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // SISTEMA DI RILEVAMENTO (Trigger Collider)
    // ───────────────────────────────────────────────────────────────────────────────

    /// Viene invocato quando un collider entra nel trigger della torretta.
    /// Se è il player, abilita la modalità "combattimento" della torretta.
    void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra ha il tag "Player"
        if (other.CompareTag("Player"))
        {
            // Abilita la torretta: inizia a tracciare e sparare
            _isPlayerInRange = true;
        }
    }

    /// Viene invocato quando un collider esce dal trigger della torretta.
    /// Se è il player, disabilita la modalità "combattimento" della torretta.
    void OnTriggerExit(Collider other)
    {
        // Controlla se l'oggetto che esce ha il tag "Player"
        if (other.CompareTag("Player"))
        {
            // Disabilita la torretta: smette di tracciare e sparare
            _isPlayerInRange = false;
        }
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // DEBUG VISIVO (Solo in Editor)
    // ───────────────────────────────────────────────────────────────────────────────

    /// Disegna il raggio di rilevamento della torretta come sfera di Gizmo.
    /// Visibile solo quando il GameObject è selezionato nell'editor.
    /// Utile per debuggare e visualizzare il raggio di attacco.
    void OnDrawGizmosSelected()
    {
        // Colore del gizmo (rosso = zona di attacco)
        Gizmos.color = Color.red;
        
        // Recupera il SphereCollider usato come trigger di rilevamento
        SphereCollider rangeCollider = GetComponent<SphereCollider>();
        
        // Se il collider esiste, disegna una sfera wire al raggio specificato
        if (rangeCollider != null)
        {
            // DrawWireSphere crea una sfera wireframe (non piena)
            // Parametri:
            //   - transform.position: centro della sfera (posizione della torretta)
            //   - rangeCollider.radius: raggio della sfera di rilevamento
            Gizmos.DrawWireSphere(transform.position, rangeCollider.radius);
        }
    }
}