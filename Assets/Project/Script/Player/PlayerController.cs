using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    // Velocità massima di movimento del player (unità al secondo)
    [SerializeField] private float _moveSpeed = 7f;

    [Header("Jump Settings")]
    // Forza applicata al jump (usa ForceMode.Impulse, quindi è in kg·m/s)
    [SerializeField] private float _jumpForce = 6f;
    // Transform che indica il punto dove controllare se il player tocca il suolo
    [SerializeField] private Transform _groundCheck;
    // Raggio della sfera usata per controllare il contatto con il suolo
    [SerializeField] private float _groundCheckRadius = 0.2f;
    // Layer mask che identifica quali layer sono considerati "terreno"
    [SerializeField] private LayerMask _groundLayer;

    [Header("Interaction Settings")]
    // Forza della spinta quando colpisci un muro/ostacolo
    [SerializeField] private float _pushForce = 2f;
    // Durata del "recoil" (paralisi input) dopo una collisione, in secondi
    [SerializeField] private float _recoilDuration = 0.65f;
    // Danno inflitto quando colpisci un ostacolo (non i muri di confine)
    [SerializeField] private int _obstacleDamage = 7;

    [Header("References")]
    [SerializeField]private Camera _mainCamera;

    // Rigidbody del player - cachato per evitare GetComponent() ogni frame
    private Rigidbody _rb;
    // Reference al sistema di salute del player - cachato per performance
    private PlayerHealth _health;
    // Refactor
    
    // Input manager - tracciamo gli input in Update(), li usiamo in FixedUpdate()
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;

    // Stato fisico
    private bool _isGrounded;
    // Timer che conta i secondi rimasti del recoil (paralisi dopo collisione)
    private float _recoilTimer = 0f;

    private void Awake()
    {
        // Recupera il Rigidbody dello stesso GameObject per evitare GetComponent() ripetuti
        _rb = GetComponent<Rigidbody>();
        // Recupera il componente PlayerHealth dello stesso GameObject
        _health = GetComponent<PlayerHealth>();
        if (_mainCamera == null)
           _mainCamera = Camera.main;
        
    }

    void Start()
    {
        // Assicurati che il Rigidbody non ruoti (il player rimane sempre in piedi)
        // Importante: freezeRotation previene rotazioni indesiderate dalla fisica
        // if (_rb.freezeRotation != true){}
        _rb.freezeRotation = true;
    }

    void Update()
    {
        // GESTIONE RECOIL TIMER
        // Decrementa il timer recoil ogni frame (il recoil scade gradualmente)
        if (_recoilTimer > 0)
        {
            _recoilTimer -= Time.deltaTime;
        }

        // RACCOLTA INPUT
        // GetAxis ritorna valori -1 / 0 / 1 (smooth) per movimento continuo
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        // GetButtonDown ritorna true solo nel frame in cui il bottone è premuto
        // Memorizza lo stato in _jumpInput per usarlo in FixedUpdate()
        if (Input.GetButtonDown("Jump"))
        {
            _jumpInput = true;
        }

        // GROUND CHECK
        // CheckSphere crea una sfera invisibile in _groundCheck.position
        // Ritorna true se la sfera sovrappone qualcosa nel layer _groundLayer
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    void FixedUpdate()
    {
        // FixedUpdate è sincronizzato col timestep della fisica (predefinito 0.02s)
        // Tutti i movimenti fisici devono avvenire qui, non in Update()
        MovePlayer();
        HandleJump();
    }
    
    void MovePlayer()
    {
        // Se il player è in recoil (dopo aver colpito qualcosa), non può muoversi
        if (_recoilTimer > 0) return;

        // Prendi la direzione forward e right della camera principale
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;

        // Azzeramento della componente Y: il movimento è solo orizzontale (no salita/discesa)
        // Questo impedisce che il tilt della camera influenzi il movimento verticale
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalize: rende i vettori lunghezza 1, così la velocità è uniforme in tutte le direzioni
        // (diagonale = ortogonale, non diagonale = più veloce senza questo)
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Combina input orizzontale (sterzata) e verticale (avanti/indietro) 
        // rispetto agli assi della camera
        Vector3 moveDirection = cameraRight * _horizontalInput + cameraForward * _verticalInput;
        moveDirection.Normalize();

        // Applica la velocità al Rigidbody
        // Manteniamo la velocità Y inalterata (non alterare il salto/gravità)
        // Sostituiamo solo X e Z con il movimento desiderato
        _rb.velocity = new Vector3(
            moveDirection.x * _moveSpeed,
            _rb.velocity.y,                  // Conserva la velocità verticale (gravità, salto)
            moveDirection.z * _moveSpeed
        );
    }

    void HandleJump()
    {
        // Salta solo se:
        // 1. Il player ha premuto il tasto salto (_jumpInput == true)
        // 2. Il player è a terra (_isGrounded == true)
        if (_jumpInput && _isGrounded)
        {
            // AddForce con ForceMode.Impulse: applica una spinta istantanea
            // (ignora il deltaTime, è l'unico jump per questo input)
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        // Resetta sempre l'input dopo averlo processato
        // (altrimenti il player continuerebbe a saltare finché non rilascia il tasto)
        _jumpInput = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Controlla il tag dell'oggetto per identificare il tipo di collisione
        bool isBoundary = collision.gameObject.CompareTag("Boundaries");
        bool isObstacle = collision.gameObject.CompareTag("Obstacle");

        // Reagisci solo se hai colpito un confine o un ostacolo (non altri oggetti)
        if (isBoundary || isObstacle)
        {
            // STEP 1: CALCOLA LA DIREZIONE DI SPINTA
            // collision.contacts[0] è il primo punto di contatto (generalmente l'unico rilevante)
            // .normal è il vettore perpendicolare alla superficie (punta verso questo rigidbody)
            Vector3 pushDirection = collision.contacts[0].normal;
            // Azzeramento Y: la spinta è solo orizzontale (non vuoi che voli in aria)
            pushDirection.y = 0;
            // Normalize: rendi il vettore di lunghezza 1 (velocità uniforme)
            pushDirection.Normalize();

            // STEP 2: APPLICA LA FISICA DELLA SPINTA
            // Azzera la velocità attuale per un "reset" pulito (evita accumulo di velocità)
            _rb.velocity = Vector3.zero;
            // Applica una forza istantanea nella direzione calcolata
            _rb.AddForce(pushDirection * _pushForce, ForceMode.Impulse);

            // STEP 3: ATTIVA IL RECOIL (PARALISI INPUT)
            // Imposta il timer recoil: per i prossimi _recoilDuration secondi,
            // il player non potrà muoversi (controllato in MovePlayer())
            _recoilTimer = _recoilDuration;

            // STEP 4: LOGICA SPECIFICA: OSTACOLI INFLIGGONO DANNO
            if (isObstacle)
            {
                // Verifica che il reference a PlayerHealth sia valido
                // (potrebbe non esistere se non assegnato o rimosso)
                if (_health != null)
                {
                    // Infligi il danno dell'ostacolo al player
                    _health.TakeDamage(_obstacleDamage);
                }
                Debug.Log($"Colpito Ostacolo! Danno ricevuto: {_obstacleDamage}");
            }
            else
            {
                // Se è un confine (boundary), non infliggi danno (solo spinta)
                Debug.Log("Colpito Muro di confine!");
            }
        }
    }
}