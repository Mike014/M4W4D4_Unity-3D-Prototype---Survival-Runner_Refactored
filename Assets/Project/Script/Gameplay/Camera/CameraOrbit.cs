using UnityEngine;

/// Gestisce una camera che orbita intorno a un target (il player).
/// La camera segue il mouse per ruotare, con supporto per collisioni con i muri
/// e smoothing per evitare scatti improvvisi.
public class CameraOrbit : MonoBehaviour
{
    #region Inspector Variables
    [Header("Target Settings")]
    // Transform del player attorno al quale la camera orbita
    [SerializeField] private Transform _target;
    
    [Header("Orbit Settings")]
    // Velocità di rotazione della camera rispetto al movimento del mouse
    [SerializeField] private float _mouseSensitivity = 2f; 
    // Distanza desiderata tra camera e player (quando non ci sono muri)
    [SerializeField] private float _distance = 5f;
    // Limite inferiore dell'angolo verticale
    [SerializeField] private float _minVerticalAngle = -20f;
    // Limite superiore dell'angolo verticale
    [SerializeField] private float _maxVerticalAngle = 70f;
    
    [Header("Collision & Smoothing")]
    // Abilita il raycast per evitare che la camera penetri i muri
    [SerializeField] private bool _enableCollision = true;
    // Layer mask che identifica quali oggetti bloccano la camera
    [SerializeField] private LayerMask _collisionLayers; 
    // Spazio cuscinetto tra camera e muro
    [SerializeField] private float _collisionPadding = 0.2f;
    // Tempo di smorzamento per il movimento della camera (più alto = più smooth)
    [SerializeField] private float _positionSmoothTime = 0.12f;
    #endregion

    #region Private State
    // Angolo orizzontale (rotazione sinistra/destra) in gradi
    private float _currentX = 0f;
    // Angolo verticale (rotazione su/giù) in gradi
    private float _currentY = 0f;
    // Velocità interna usata da SmoothDamp
    private Vector3 _currentVelocity = Vector3.zero;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // SETUP INPUT DEL MOUSE
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // INIZIALIZZAZIONE ANGOLI basata sulla rotazione attuale della camera
        Vector3 angles = transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
    }

    void Update()
    {
        HandleInput();
        
        // SBLOCCO CURSORE CON ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        // LateUpdate garantisce che il player si sia già mosso
        if (_target == null) return;
        
        UpdateCameraTransform();
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        UpdateAngles(mouseX, mouseY);
    }

    private void UpdateAngles(float mouseX, float mouseY)
    {
        _currentX += mouseX * _mouseSensitivity;
        _currentY -= mouseY * _mouseSensitivity;
        
        // CLAMP VERTICALE per evitare rotazioni innaturali
        _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
    }
    #endregion

    #region Camera Transform Logic
    private void UpdateCameraTransform()
    {
        // 1. ROTAZIONE
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // 2. POSIZIONE DESIDERATA
        Vector3 direction = rotation * new Vector3(0, 0, -_distance);
        Vector3 desiredPosition = _target.position + direction;

        // 3. COLLISIONI
        float finalDistance = CalculateFinalDistance(_target.position, desiredPosition);

        // 4. POSIZIONE FINALE CORRETTA
        Vector3 finalPosition = _target.position + (direction.normalized * finalDistance);

        // 5. APPLICA SMOOTHING
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            finalPosition, 
            ref _currentVelocity, 
            _positionSmoothTime
        );
        
        // 6. GUARDA IL TARGET
        transform.LookAt(_target.position + Vector3.up * 1.5f);
    }
    #endregion

    #region Collision Logic
    private float CalculateFinalDistance(Vector3 startPos, Vector3 endPos)
    {
        if (!_enableCollision) return _distance;

        return CheckCameraCollision(startPos, endPos);
    }

    private float CheckCameraCollision(Vector3 startPos, Vector3 endPos)
    {
        RaycastHit hit;
        // Raycast dal target verso la camera
        if (Physics.Raycast(startPos, endPos - startPos, out hit, _distance, _collisionLayers))
        {
            // Ritorna la distanza accorciata meno il padding
            return Mathf.Clamp(hit.distance - _collisionPadding, 0.5f, _distance);
        }
        return _distance;
    }
    #endregion
}