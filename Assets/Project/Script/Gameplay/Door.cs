// using UnityEngine;


// /// Gestisce il comportamento di una porta che si apre quando viene sbloccata.
// /// La porta si sposta da una posizione iniziale a una posizione aperta usando interpolazione smooth.

// public class Door : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     // Offset (spostamento) da applicare alla porta quando si apre
//     // Es: (0, 5, 0) sposta la porta di 5 unità verso l'alto
//     [SerializeField] private Vector3 _openOffset = new Vector3(0, 5, 0);
//     // Velocità di movimento della porta (fattore di lerp)
//     // Valori tipici: 1-5 (più alto = più veloce)
//     [SerializeField] private float _openSpeed = 2f;

//     // ════════════════════════════════════════════════════════════════
//     // VARIABILI INTERNE
//     // ════════════════════════════════════════════════════════════════

//     // Flag che indica se la porta è in corso di apertura
//     private bool _isOpen = false;
//     // La posizione verso cui la porta si sta muovendo (posizione aperta desiderata)
//     private Vector3 _targetPosition;

//     /// <summary>
//     /// Inizializza la porta salvando la sua posizione di chiusura come target iniziale.
//     /// La porta inizia chiusa e non si muove finché Open() non viene chiamato.
//     /// </summary>
//     void Start()
//     {
//         // Salva la posizione attuale come target iniziale (posizione di chiusura)
//         // Necessario affinché Lerp abbia un punto di partenza definito
//         _targetPosition = transform.position;
//     }

//     /// <summary>
//     /// Sblocca e avvia l'apertura della porta.
//     /// Calcola la posizione target aggiungendo l'offset alla posizione attuale.
//     /// Invocato da altri script quando l'evento di apertura avviene
//     /// (es: quando il player raccoglie una chiave).
//     /// </summary>
//     public void Open()
//     {
//         // Abilita il movimento della porta
//         _isOpen = true;
//         // Calcola la posizione target: posizione attuale + offset di apertura
//         // Es: se la porta è a (0, 0, 0) e _openOffset = (0, 5, 0), target = (0, 5, 0)
//         _targetPosition = transform.position + _openOffset;
//         // Debug log per confirmazione visiva in console durante lo sviluppo
//         Debug.Log("Porta Sbloccata!");
//     }

//     /// <summary>
//     /// Aggiorna la posizione della porta ogni frame se è in apertura.
//     /// Usa Vector3.Lerp per un movimento fluido e naturale verso la posizione target.
//     /// </summary>
//     void Update()
//     {
//         // Esegui il movimento SOLO se la porta è stata aperta
//         if (_isOpen)
//         {
//             // INTERPOLAZIONE SMOOTH CON LERP
//             // Vector3.Lerp interpola linearmente tra due posizioni
//             // Parametri:
//             //   - transform.position: posizione attuale della porta
//             //   - _targetPosition: posizione desiderata (aperta)
//             //   - Time.deltaTime * _openSpeed: fattore di interpolazione (0-1)
//             //     Time.deltaTime: tempo trascorso questo frame (60fps → ~0.0167s)
//             //     _openSpeed: moltiplicatore (velocità di movimento)
//             //     Esempio: 0.0167 * 2 = 0.0334 (3.34% di movimento verso target questo frame)
//             // 
//             // Nota IMPORTANTE: Lerp non raggiunge mai ESATTAMENTE il target
//             // (la distanza si riduce ogni frame, ma asintoticamente)
//             // Per distanze piccole, la porta si ferma "praticamente" al target
//             transform.position = Vector3.Lerp(
//                 transform.position, 
//                 _targetPosition, 
//                 Time.deltaTime * _openSpeed
//             );
//         }
//     }
// }