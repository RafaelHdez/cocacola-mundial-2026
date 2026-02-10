using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuración del Juego")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int pointsPerSave = 100; // Los puntos que ganará el jugador por balón detenido
    [SerializeField] private int penaltyPerMiss = 50; // Los puntos que se restarán por gol concedido
    
    [SerializeField] private float gameOverInteractionDelay = 2.0f;
    
    [Header("Referencias Visuales")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private HandController handController;

    [Header("Feedback Visual")]
    [SerializeField] private Color normalScoreColor = Color.white;
    [SerializeField] private Color winScoreColor = Color.green;
    [SerializeField] private Color loseScoreColor = Color.red;
    
    // Estado del juego
    public bool IsGameActive { get; private set; } = false;
    public float CurrentTime { get; private set; }
    public float TotalDuration => gameDuration;
    
    private int _currentScore = 0;

    // Input Actions
    private InputAction _pressAction;
    private InputAction _positionAction;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Configuración del singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _pressAction = new InputAction(name: "Touch", binding: "<Pointer>/press");
        _positionAction = new InputAction(name: "Position", binding: "<Pointer>/position");
    }

    private void OnEnable()
    {
        _pressAction.Enable();
        _positionAction.Enable();
    }

    private void OnDisable()
    {
        _pressAction.Disable();
        _positionAction.Disable();
    }

    void Start()
    {
        if(gameOverPanel) gameOverPanel.SetActive(false);
        if(countdownText) countdownText.gameObject.SetActive(false);
        
        IsGameActive = false; // El juego empieza pausado
        if (scoreText) scoreText.color = normalScoreColor;
        UpdateScoreUI();

        // Iniciamos la rutina de preparación en lugar del juego directo
        StartCoroutine(StartSequenceRoutine());
    }
    
    private IEnumerator StartSequenceRoutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            // Mensaje inicial
            countdownText.text = "¡PREPÁRATE!";
            yield return new WaitForSeconds(1.5f); // Tiempo para leer

            // Cuenta progresiva como pediste (1, 2, 3)
            countdownText.text = "1";
            // Aquí podrías poner un sonido: AudioSource.PlayOneShot(beepClip);
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "¡YA!";
        }

        // Dejamos el "¡YA!" medio segundo más y lo quitamos
        yield return new WaitForSeconds(0.5f);
        if (countdownText) countdownText.gameObject.SetActive(false);
        
        // --- ARRANCA EL JUEGO REAL ---
        StartGame();
    }

    void Update()
    {
        if (IsGameActive)
        {
            UpdateTimer();
            HandleInput();
        }
    }

    private void StartGame()
    {
        CurrentTime = gameDuration;
        _currentScore = 0;
        UpdateScoreUI();
        IsGameActive = true; // Esto activará el Spawner automáticamente
    }

    private void UpdateTimer()
    {
        CurrentTime -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(CurrentTime).ToString(); 
        }

        if (CurrentTime <= 0)
        {
            EndGame();
        }
    }

    private void HandleInput()
    {
        if (_pressAction.WasPerformedThisFrame())
        {
            Vector2 pointerPosition = _positionAction.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Ball"))
                {
                    if (handController)
                    {
                        handController.MoveHandToPosition(hit.point);
                    }
                    
                    ExplodeBall(hit.transform.gameObject);
                }
            }
        }
    }

    private void ExplodeBall(GameObject ball)
    {
        if (explosionVFX) 
        {
            Instantiate(explosionVFX, ball.transform.position, Quaternion.identity);
        }

        _currentScore += pointsPerSave;
        UpdateScoreUI();
        StartCoroutine(FlashScoreColor(winScoreColor));

        Destroy(ball);
    }

    public void RegisterMiss()
    {
        if (!IsGameActive)
        {
            return;
        }
        
        _currentScore -= penaltyPerMiss;

        if (_currentScore < 0)
        {
            _currentScore = 0;
        }
        
        UpdateScoreUI();
        StartCoroutine(FlashScoreColor(loseScoreColor));
    }

    private void UpdateScoreUI()
    {
        if (scoreText)
        {
            scoreText.text = "Puntos: " + _currentScore.ToString();
        }
    }

    private IEnumerator FlashScoreColor(Color targetColor)
    {
        if (scoreText)
        {
            scoreText.color = targetColor;
            scoreText.transform.localScale = Vector3.one * 1.2f;

            yield return new WaitForSeconds(0.15f);

            scoreText.color = normalScoreColor;
            scoreText.transform.localScale = Vector3.one;
        }
    }

    private void EndGame()
    {
        IsGameActive = false;
        CurrentTime = 0;
        if (timerText) timerText.text = "0";
        Debug.Log("Juego Terminado");

        StartCoroutine(GameOverSequenceRoutine());
    }
    
    private IEnumerator GameOverSequenceRoutine()
    {
        if (gameOverPanel)
        {
            // 1. Obtenemos el CanvasGroup (Requisito: Agrégalo en el Inspector al Panel)
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            
            // 2. Antes de mostrar el panel, bloqueamos la interacción
            if (cg != null)
            {
                cg.interactable = false; // BOTONES BLOQUEADOS
                cg.blocksRaycasts = false; // NO DETECTA CLICKS
                cg.alpha = 0; // Empieza invisible (opcional)
            }

            // 3. Activamos el objeto
            gameOverPanel.SetActive(true);

            // 4. Pequeña animación de Fade In (Opcional, se ve bonito)
            float fadeSpeed = 2f;
            if (cg != null)
            {
                while (cg.alpha < 1)
                {
                    cg.alpha += Time.deltaTime * fadeSpeed;
                    yield return null;
                }
                cg.alpha = 1;
            }

            // 5. ESPERAMOS EL TIEMPO DE SEGURIDAD
            // Aquí es donde el jugador intenta dar clicks a lo loco pero no pasa nada
            yield return new WaitForSeconds(gameOverInteractionDelay);

            // 6. Ahora sí, habilitamos los botones
            if (cg != null)
            {
                cg.interactable = true; // YA PUEDEN CLICKAR
                cg.blocksRaycasts = true;
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}