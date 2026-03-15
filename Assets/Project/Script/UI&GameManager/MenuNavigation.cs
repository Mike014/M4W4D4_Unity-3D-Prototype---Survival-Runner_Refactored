using UnityEngine;
using UnityEngine.SceneManagement; // Necessario per gestire il caricamento delle scene
using UnityEngine.UI; // Necessario per accedere ai componenti Button

/// Gestisce la navigazione del menu principale del gioco.
/// Fornisce funzioni per avviare il gioco ed uscire dall'applicazione.
/// I bottoni vengono assegnati tramite Inspector e vengono collegati automaticamente in Start().
public class MenuNavigation : MonoBehaviour
{
    [Header("UI References")]
    // Riferimento al bottone "New Game"
    [SerializeField] private Button _newGameButton;
    // Riferimento al bottone "Exit Game"
    [SerializeField] private Button _exitGameButton;

    /// Inizializza i bottoni collegando i loro onClick events ai rispettivi metodi.
    /// Viene eseguito all'avvio della scena.
    void Start()
    {
        // VALIDAZIONE: Controlla che i bottoni siano stati assegnati
        if (_newGameButton == null)
        {
            Debug.LogError("New Game Button non assegnato nell'Inspector!");
            return;
        }
        if (_exitGameButton == null)
        {
            Debug.LogError("Exit Game Button non assegnato nell'Inspector!");
            return;
        }

        // COLLEGAMENTO DEI BOTTONI
        // AddListener registra questo script come listener dei click events
        // Quando un bottone viene cliccato, il rispettivo metodo viene invocato
        _newGameButton.onClick.AddListener(StartGame);
        _exitGameButton.onClick.AddListener(ExitGame);

        Debug.Log("Menu Navigation inizializzato correttamente!");
    }

    /// Avvia il gioco caricando la scena principale.
    /// Viene invocato quando il giocatore clicca il bottone "New Game" nel menu.
    /// La scena del gioco deve essere aggiunta nelle Build Settings (File > Build Settings) con indice 1.
    /// 
    /// SETUP RICHIESTO:
    /// - Build Settings Scene 0: Menu principale (questa scena)
    /// - Build Settings Scene 1: Scena del gioco principale
    public void StartGame()
    {
        // DEBUG: Conferma nella console che il bottone è stato cliccato
        Debug.Log("Avvio gioco in corso...");
        
        // CARICAMENTO DELLA SCENA
        // SceneManager.LoadScene() carica una scena usando il suo indice nelle Build Settings
        // Indice 1 = la scena del gioco principale
        // La scena attuale (menu) verrà scaricata e sostituita dalla nuova scena
        SceneManager.LoadScene(1);
        
        // NOTA ALTERNATIVA: Se preferisci caricare per nome invece che per indice:
        // SceneManager.LoadScene("GameScene");
        // Questo è più leggibile e non dipende dall'ordine nelle Build Settings
    }

    /// Esce dall'applicazione.
    /// Viene invocato quando il giocatore clicca il bottone "Exit Game" nel menu.
    /// 
    /// IMPORTANTE - LIMITAZIONI:
    /// - Application.Quit() funziona SOLO nella build finale del gioco
    /// - NON funziona quando esegui il gioco dall'editor di Unity
    /// - Per testare se funziona, devi fare una build standalone (File > Build and Run)
    public void ExitGame()
    {
        // DEBUG LOG
        // Stampa un messaggio nella console per confermare che il metodo è stato chiamato
        // Utile durante lo sviluppo per verificare che il bottone sia collegato correttamente
        Debug.Log("Uscita dal gioco in corso...");
        
        // CHIUSURA DELL'APPLICAZIONE
        // Application.Quit() termina immediatamente l'esecuzione del programma
        // 
        // COMPORTAMENTO IN EDITOR vs BUILD:
        // - In Editor: Questo comando viene IGNORATO (Unity protegge se stesso)
        // - In Build: L'applicazione si chiude e torna al desktop
        // 
        // Se vuoi testare durante lo sviluppo, usa una build!
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// Pulizia: rimuove i listener quando il menu viene distrutto.
    /// Importante per evitare memory leaks se il menu viene ricaricato.
    void OnDestroy()
    {
        // UNSUBSCRIBE DAGLI EVENTI
        // Rimuove i listener per evitare che vengano invocati su script distrutti
        if (_newGameButton != null)
        {
            _newGameButton.onClick.RemoveListener(StartGame);
        }
        if (_exitGameButton != null)
        {
            _exitGameButton.onClick.RemoveListener(ExitGame);
        }
    }
}