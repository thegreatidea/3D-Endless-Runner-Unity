using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using DG.Tweening; // Add this at the top

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;



    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject startMenuUI; 
    [SerializeField] private GameObject topBarUI;
    [SerializeField] private GameObject tapToPlayUI; 
    [SerializeField] private Transform player;
    [SerializeField] private GameObject blackBG;
    [SerializeField] private Vector3 startPosition ;

    [Header("Text Mesh Pro")]
    [SerializeField] private TextMeshProUGUI countdownText; // Drag your countdown UI text here
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalCoinsText;
    [SerializeField] private TextMeshProUGUI currentCoinsText;
    [SerializeField] private TextMeshProUGUI highScoreText;


    [Space(10)]
    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip ButtonInClip;
    [SerializeField] private AudioClip ButtonOutClip;
   

    [Header("Resolution Settings")]
    [Range(0.5f, 1f)] public float resolutionScale = 0.75f;
    private static bool resolutionApplied = false;

    [Header("Gameplay Stats")]
    public int score = 0;
    public int coins = 0;
    public float scoreRate = 10f; // how fast score increases over time

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera menuCamera;
    [SerializeField] private CinemachineCamera runningCamera;

    private bool isGameOver = false;
    private bool isGameStarted = false; // Add this line
    private int highScore = 0;
    private int finalCoins = 0;
    private int currentCoins = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        startPosition = player.position;
        UpdateUI();
        isGameOver = false;

        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        // Set initial camera priorities
        if (menuCamera != null) menuCamera.Priority = 2;
        if (runningCamera != null) runningCamera.Priority = 1;
        
        // Top bar UI should be hidden at the start
        if (topBarUI != null) topBarUI.SetActive(false);

        // Apply resolution settings if not already applied
#if UNITY_IOS || UNITY_ANDROID
        if (!resolutionApplied)
        {
            ApplyResolution();
            resolutionApplied = true;
        }
#endif
    }

    void Update()
    {
        if (!isGameStarted)
        {
            bool inputDetected = false;

            // Touch input (mobile & simulator)
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    if (!IsPointerOverUI(Input.GetTouch(i).position))
                    {
                        inputDetected = true;
                        break;
                    }
                }
            }

            // Mouse input (editor/desktop)
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUI(Input.mousePosition))
                    inputDetected = true;
            }

            // Keyboard input (optional, only if not over UI)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!IsPointerOverUI(Input.mousePosition))
                    inputDetected = true;
            }

            if (inputDetected)
            {
                StartGame();
            }
            return;
        }

        if (!isGameOver)
        {
            score = (int)Vector3.Distance(startPosition, player.transform.position);
            UpdateUI();
        }
    }

    void ApplyResolution()
    {
        int targetWidth = Mathf.RoundToInt(Screen.currentResolution.width * resolutionScale);
        int targetHeight = Mathf.RoundToInt(Screen.currentResolution.height * resolutionScale);
        Screen.SetResolution(targetWidth, targetHeight, true);
        Debug.Log($"Resolution set to {targetWidth}x{targetHeight}");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            string scoreStr = score < 1000000 ? score.ToString().PadLeft(6, '0') : score.ToString();
            scoreText.text = scoreStr;
        }
        if (coinText != null) coinText.text = coins.ToString();
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
        if (currentCoinsText != null)
            currentCoinsText.text = TotalCoins.ToString();
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Leave()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        if (pauseMenuUI != null)    
            pauseMenuUI.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (ButtonInClip != null)
            SoundFXManager.Instance.PlaySoundEffect(ButtonInClip, transform, 1f);
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            pauseMenuUI.transform.localScale = Vector3.zero; // Ensure starting scale
            pauseMenuUI.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Use unscaled time!
        }
        if (startMenuUI != null)
            startMenuUI.SetActive(true);
        if (topBarUI != null)
            topBarUI.SetActive(false);
        Debug.Log("Game Paused");
    }
    public void ResumeGame()
    {
        if (ButtonOutClip != null)
            SoundFXManager.Instance.PlaySoundEffect(ButtonOutClip, transform, 1f);
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Keep it active for animation
            pauseMenuUI.transform.localScale = Vector3.one; // Ensure starting scale
            pauseMenuUI.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true) // Use unscaled time!
            .OnComplete(() => pauseMenuUI.SetActive(false)); // Deactivate after animation
        }
        if (startMenuUI != null)
            startMenuUI.SetActive(false);
        if (topBarUI != null)
            topBarUI.SetActive(true);
        StartCoroutine(ResumeAfterDelay(3f)); // Resume after 3 seconds

    }

    private IEnumerator ResumeAfterDelay(float delay)
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        string[] steps = { "3", "2", "1", "Go!" };
        for (int i = 0; i < steps.Length; i++)
        {
            if (countdownText != null)
            {
                countdownText.text = steps[i];
                countdownText.transform.localScale = Vector3.zero;
                countdownText.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
            yield return new WaitForSecondsRealtime(1f);
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (ButtonInClip != null)
            SoundFXManager.Instance.PlaySoundEffect(ButtonInClip, transform, 1f);
        if (settingsMenuUI != null)
        {
            settingsMenuUI.SetActive(true); // Activate first!
            settingsMenuUI.transform.localScale = Vector3.zero; // Ensure starting scale
            settingsMenuUI.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Use unscaled time!
        }
        if (blackBG != null)
        {
            blackBG.SetActive(true);
        }
    }
    public void CloseSettings()
    {
        if (ButtonOutClip != null)
            SoundFXManager.Instance.PlaySoundEffect(ButtonOutClip, transform, 1f);
        if (settingsMenuUI != null)
        {
            settingsMenuUI.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true) // Use unscaled time!
                    .OnComplete(() => settingsMenuUI.SetActive(false)); // Deactivate after animation
        }
        if (blackBG != null)
        {
            blackBG.SetActive(false); // Deactivate immediately
        }
    }
    public void StartGame()
    {
        isGameStarted = true;
        if (tapToPlayUI != null)
            tapToPlayUI.SetActive(false);
        if (startMenuUI != null)
            startMenuUI.SetActive(false);
        if (topBarUI != null)
            topBarUI.SetActive(true);

        // Switch camera priorities for smooth blend
        if (menuCamera != null) menuCamera.Priority = 1;
        if (runningCamera != null) runningCamera.Priority = 2;

        // Tell the player to start running
        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.StartRunning();
    }

    public IEnumerator HandleDeathSequence()
    {
        isGameOver = true;

        // Update high score if needed
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        finalCoins = coins;

        yield return new WaitForSeconds(1.0f);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score;

        if (finalCoinsText != null)
            finalCoinsText.text = "Final Coins: " + finalCoins;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);

            // Set initial scale and canvas group for effect
            gameOverUI.transform.localScale = Vector3.zero;
            var cg = gameOverUI.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = gameOverUI.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Bounce scale-in and fade-in
            gameOverUI.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBounce)
                .SetUpdate(true);
            cg.DOFade(1f, 0.4f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);
        }
        if (blackBG != null)
        {
            blackBG.SetActive(true);
        }

        // Add finalCoins to TotalCoins and save
        TotalCoins += finalCoins;

        Debug.Log("Game Over! Final Score: " + score + ", Final Coins: " + finalCoins + ", High Score: " + highScore + ", Total Coins: " + TotalCoins);
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public int TotalCoins
    {
        get => PlayerPrefs.GetInt("TotalCoins", 0);
        set
        {
            PlayerPrefs.SetInt("TotalCoins", value);
            PlayerPrefs.Save();
        }
    }
}
