// using NUnit.Framework;
// using UnityEngine;

// public class GameManagerTests
// {
//     private GameManager _gameManager;
//     private GameObject _gameManagerObj;
//     private GameObject _eventManagerObj;

//     [SetUp]
//     public void Setup()
//     {
//         // ✅ NUOVO: Crea GameEvents PRIMA di GameManager
//         // GameManager lo cercherà con FindObjectOfType nel Start()
//         _eventManagerObj = new GameObject("_EventManager");
//         _eventManagerObj.AddComponent<GameEvents>();

//         // CREA IL GAMEOBJECT DEL GAME MANAGER
//         _gameManagerObj = new GameObject("GameManager");
//         _gameManager = _gameManagerObj.AddComponent<GameManager>();
//     }

//     [TearDown]
//     public void Teardown()
//     {
//         // Pulisci GameManager
//         if (_gameManagerObj != null)
//         {
//             Object.DestroyImmediate(_gameManagerObj);
//         }

//         // ✅ NUOVO: Pulisci anche GameEvents
//         if (_eventManagerObj != null)
//         {
//             Object.DestroyImmediate(_eventManagerObj);
//         }
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 1: IsTimeExpired() - Logica di Scadenza Timer
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che IsTimeExpired() ritorna true quando il tempo è 0
//     /// ✅ TESTABILE: Pura logica booleana
//     /// 
//     /// NOTA: Questo test NON è cambiato perché testa logica pura
//     /// che non dipende da Singleton o Event System
//     /// </summary>
//     [Test]
//     public void IsTimeExpired_ReturnsTrue_WhenTimeIsZero()
//     {
//         // ACT
//         bool result = _gameManager.IsTimeExpired(0f);

//         // ASSERT
//         Assert.IsTrue(result, "IsTimeExpired dovrebbe ritornare true quando tempo = 0");
//     }

//     /// <summary>
//     /// Verifica che IsTimeExpired() ritorna true quando il tempo è negativo
//     /// (Può succedere se il timer non viene stoppato al raggiungimento di 0)
//     /// </summary>
//     [Test]
//     public void IsTimeExpired_ReturnsTrue_WhenTimeIsNegative()
//     {
//         // ACT
//         bool result = _gameManager.IsTimeExpired(-5f);

//         // ASSERT
//         Assert.IsTrue(result, "IsTimeExpired dovrebbe ritornare true quando tempo < 0");
//     }

//     /// <summary>
//     /// Verifica che IsTimeExpired() ritorna false quando il tempo è positivo
//     /// </summary>
//     [Test]
//     public void IsTimeExpired_ReturnsFalse_WhenTimeIsPositive()
//     {
//         // ACT
//         bool result = _gameManager.IsTimeExpired(10f);

//         // ASSERT
//         Assert.IsFalse(result, "IsTimeExpired dovrebbe ritornare false quando tempo > 0");
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 2: ShouldPlayerWin() - Condizione di Vittoria
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che ShouldPlayerWin() ritorna true quando monete == requisito
//     /// Il giocatore vince se raccoglie ESATTAMENTE RequiredCoins monete
//     /// </summary>
//     [Test]
//     public void ShouldPlayerWin_ReturnsTrue_WhenCoinsEqualRequired()
//     {
//         // ACT: 5 monete raccolte, 5 richieste
//         bool result = _gameManager.ShouldPlayerWin(5, 5);

//         // ASSERT
//         Assert.IsTrue(result, "Dovrebbe vincere quando monete == requisito");
//     }

//     /// <summary>
//     /// Verifica che ShouldPlayerWin() ritorna true quando monete > requisito
//     /// Il giocatore vince anche se raccoglie PIÙ delle monete richieste
//     /// </summary>
//     [Test]
//     public void ShouldPlayerWin_ReturnsTrue_WhenCoinsExceedRequired()
//     {
//         // ACT: 7 monete raccolte, 5 richieste
//         bool result = _gameManager.ShouldPlayerWin(7, 5);

//         // ASSERT
//         Assert.IsTrue(result, "Dovrebbe vincere quando monete > requisito");
//     }

//     /// <summary>
//     /// Verifica che ShouldPlayerWin() ritorna false quando monete < requisito
//     /// Il giocatore non vince ancora se non ha raccolto abbastanza monete
//     /// </summary>
//     [Test]
//     public void ShouldPlayerWin_ReturnsFalse_WhenCoinsLessThanRequired()
//     {
//         // ACT: 3 monete raccolte, 5 richieste
//         bool result = _gameManager.ShouldPlayerWin(3, 5);

//         // ASSERT
//         Assert.IsFalse(result, "Non dovrebbe vincere quando monete < requisito");
//     }

//     /// <summary>
//     /// Verifica il caso limite: 0 monete raccolte, 0 richieste
//     /// (Vittoria istantanea se il requisito è 0)
//     /// </summary>
//     [Test]
//     public void ShouldPlayerWin_ReturnsTrue_WhenBothZero()
//     {
//         // ACT
//         bool result = _gameManager.ShouldPlayerWin(0, 0);

//         // ASSERT
//         Assert.IsTrue(result, "Dovrebbe vincere quando entrambi sono 0");
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 3: ConvertTimeToMinutesSeconds() - Conversione Tempo
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che ConvertTimeToMinutesSeconds() converte correttamente secondi in minuti:secondi
//     /// 65 secondi = 1 minuto e 5 secondi
//     /// ✅ TESTABILE: Pura matematica
//     /// </summary>
//     [Test]
//     public void ConvertTimeToMinutesSeconds_ConvertsCorrectly()
//     {
//         // ACT: 65 secondi = 1:05
//         int[] result = _gameManager.ConvertTimeToMinutesSeconds(65f);

//         // ASSERT
//         Assert.AreEqual(1, result[0], "Minuti dovrebbero essere 1");
//         Assert.AreEqual(5, result[1], "Secondi dovrebbero essere 5");
//     }

//     /// <summary>
//     /// Verifica la conversione di 150 secondi = 2:30
//     /// </summary>
//     [Test]
//     public void ConvertTimeToMinutesSeconds_WithTwoAndHalfMinutes()
//     {
//         // ACT: 150 secondi = 2:30
//         int[] result = _gameManager.ConvertTimeToMinutesSeconds(150f);

//         // ASSERT
//         Assert.AreEqual(2, result[0], "Minuti dovrebbero essere 2");
//         Assert.AreEqual(30, result[1], "Secondi dovrebbero essere 30");
//     }

//     /// <summary>
//     /// Verifica il caso con secondi frazionari (float)
//     /// 45.7 secondi → 0:45 (FloorToInt tronca i decimali)
//     /// </summary>
//     [Test]
//     public void ConvertTimeToMinutesSeconds_TruncatesDecimals()
//     {
//         // ACT: 45.7 secondi dovrebbe diventare 0:45 (non 0:46)
//         int[] result = _gameManager.ConvertTimeToMinutesSeconds(45.7f);

//         // ASSERT
//         Assert.AreEqual(0, result[0], "Minuti dovrebbero essere 0");
//         Assert.AreEqual(45, result[1], "Secondi dovrebbero essere 45 (troncato)");
//     }

//     /// <summary>
//     /// Verifica il caso limite: 0 secondi
//     /// </summary>
//     [Test]
//     public void ConvertTimeToMinutesSeconds_WithZeroTime()
//     {
//         // ACT
//         int[] result = _gameManager.ConvertTimeToMinutesSeconds(0f);

//         // ASSERT
//         Assert.AreEqual(0, result[0], "Minuti dovrebbero essere 0");
//         Assert.AreEqual(0, result[1], "Secondi dovrebbero essere 0");
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 4: FormatTimeToString() - Formattazione Stringa
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che FormatTimeToString() formatta correttamente nel formato MM:SS
//     /// 1 minuto e 5 secondi → "01:05"
//     /// ✅ TESTABILE: Pura string formatting
//     /// </summary>
//     [Test]
//     public void FormatTimeToString_FormatsWithPadding()
//     {
//         // ACT: 1 minuto, 5 secondi
//         string result = _gameManager.FormatTimeToString(1, 5);

//         // ASSERT: Dovrebbe avere padding con zero ("01:05" non "1:5")
//         Assert.AreEqual("01:05", result, "Dovrebbe formattare come 01:05 con padding");
//     }

//     /// <summary>
//     /// Verifica il formattaggio di 0:0 → "00:00"
//     /// </summary>
//     [Test]
//     public void FormatTimeToString_FormatsZeroTime()
//     {
//         // ACT
//         string result = _gameManager.FormatTimeToString(0, 0);

//         // ASSERT
//         Assert.AreEqual("00:00", result, "Zero dovrebbe essere formattato come 00:00");
//     }

//     /// <summary>
//     /// Verifica il formattaggio di 2:45 → "02:45"
//     /// </summary>
//     [Test]
//     public void FormatTimeToString_FormatsMultipleMinutes()
//     {
//         // ACT
//         string result = _gameManager.FormatTimeToString(2, 45);

//         // ASSERT
//         Assert.AreEqual("02:45", result);
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 5: ShouldTimerBeRed() - Cambio Colore Timer
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che ShouldTimerBeRed() ritorna true quando tempo <= 15 secondi
//     /// (Avvertimento visivo di urgenza)
//     /// </summary>
//     [Test]
//     public void ShouldTimerBeRed_ReturnsTrueWhenTimeIs15()
//     {
//         // ACT: Esattamente 15 secondi
//         bool result = _gameManager.ShouldTimerBeRed(15f);

//         // ASSERT
//         Assert.IsTrue(result, "Timer dovrebbe essere rosso a 15 secondi");
//     }

//     /// <summary>
//     /// Verifica che ShouldTimerBeRed() ritorna true quando tempo < 15 secondi
//     /// </summary>
//     [Test]
//     public void ShouldTimerBeRed_ReturnsTrueWhenTimeLessThan15()
//     {
//         // ACT: 5 secondi rimasti
//         bool result = _gameManager.ShouldTimerBeRed(5f);

//         // ASSERT
//         Assert.IsTrue(result, "Timer dovrebbe essere rosso quando tempo < 15");
//     }

//     /// <summary>
//     /// Verifica che ShouldTimerBeRed() ritorna false quando tempo > 15 secondi
//     /// </summary>
//     [Test]
//     public void ShouldTimerBeRed_ReturnsFalseWhenTimeGreaterThan15()
//     {
//         // ACT: 20 secondi rimasti
//         bool result = _gameManager.ShouldTimerBeRed(20f);

//         // ASSERT
//         Assert.IsFalse(result, "Timer non dovrebbe essere rosso quando tempo > 15");
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 6: CalculateNewCoinCount() - Aggiunta Monete
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che CalculateNewCoinCount() calcola correttamente il nuovo contatore
//     /// 3 monete attuali + 1 aggiunta = 4 totali
//     /// ✅ TESTABILE: Pura aritmetica
//     /// </summary>
//     [Test]
//     public void CalculateNewCoinCount_AddsCorrectly()
//     {
//         // ACT: 3 + 1 = 4
//         int result = _gameManager.CalculateNewCoinCount(3, 1);

//         // ASSERT
//         Assert.AreEqual(4, result, "3 + 1 dovrebbe essere 4");
//     }

//     /// <summary>
//     /// Verifica l'aggiunta di multiple monete
//     /// 2 monete + 3 aggiunte = 5 totali
//     /// </summary>
//     [Test]
//     public void CalculateNewCoinCount_AddsMultipleCoin()
//     {
//         // ACT: 2 + 3 = 5
//         int result = _gameManager.CalculateNewCoinCount(2, 3);

//         // ASSERT
//         Assert.AreEqual(5, result);
//     }

//     /// <summary>
//     /// Verifica il caso limite: 0 monete + 0 aggiunte = 0
//     /// </summary>
//     [Test]
//     public void CalculateNewCoinCount_WithZeroCoin()
//     {
//         // ACT
//         int result = _gameManager.CalculateNewCoinCount(0, 0);

//         // ASSERT
//         Assert.AreEqual(0, result);
//     }

//     // ─────────────────────────────────────────────────────────────────────────────
//     // TEST 7: CalculateNewTimeRemaining() - Tempo Bonus per Monete Speciali
//     // ─────────────────────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Verifica che CalculateNewTimeRemaining() aggiunge tempo SOLO se isSpecial = true
//     /// Moneta speciale: aggiungi 10 secondi
//     /// ✅ TESTABILE: Pura logica condizionale
//     /// </summary>
//     [Test]
//     public void CalculateNewTimeRemaining_AddsBonus_WhenSpecial()
//     {
//         // ACT: 100 secondi + 10 bonus (speciale)
//         float result = _gameManager.CalculateNewTimeRemaining(100f, 10f, true);

//         // ASSERT
//         Assert.AreEqual(110f, result, 0.0001f, "Dovrebbe aggiungere 10 secondi quando speciale");
//     }

//     /// <summary>
//     /// Verifica che CalculateNewTimeRemaining() NON aggiunge tempo se isSpecial = false
//     /// Moneta normale: NO bonus
//     /// </summary>
//     [Test]
//     public void CalculateNewTimeRemaining_NoBonus_WhenNotSpecial()
//     {
//         // ACT: 100 secondi, NO bonus (non speciale)
//         float result = _gameManager.CalculateNewTimeRemaining(100f, 10f, false);

//         // ASSERT: Dovrebbe rimane 100 (il bonus di 10 non viene aggiunto)
//         Assert.AreEqual(100f, result, 0.0001f, "NON dovrebbe aggiungere bonus quando non speciale");
//     }

//     /// <summary>
//     /// Verifica il caso con tempo bonus = 0
//     /// </summary>
//     [Test]
//     public void CalculateNewTimeRemaining_WithZeroBonus()
//     {
//         // ACT: 100 secondi + 0 bonus (speciale)
//         float result = _gameManager.CalculateNewTimeRemaining(100f, 0f, true);

//         // ASSERT
//         Assert.AreEqual(100f, result, 0.0001f, "0 bonus dovrebbe non cambiare il tempo");
//     }

//     /// <summary>
//     /// Verifica che il bonus funziona con numeri frazionari
//     /// 95.5 + 7.5 = 103.0
//     /// </summary>
//     [Test]
//     public void CalculateNewTimeRemaining_WithFloatValues()
//     {
//         // ACT: 95.5 + 7.5 = 103.0 (speciale)
//         float result = _gameManager.CalculateNewTimeRemaining(95.5f, 7.5f, true);

//         // ASSERT
//         Assert.AreEqual(103f, result, 0.0001f, "Dovrebbe funzionare con float");
//     }
// }