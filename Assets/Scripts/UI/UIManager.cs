using UnityEngine;
using UnityEngine.UI;
using BossBattle.Player;

namespace BossBattle.UI
{
    /// <summary>
    /// UI管理器 - 管理所有UI界面和元素
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("主要UI面板")]
        public GameObject gameplayUI;
        public GameObject pauseUI;
        public GameObject gameOverUI;
        public GameObject victoryUI;
        public GameObject settingsUI;
        
        [Header("游戏内UI元素")]
        public Slider healthBar;
        public Text healthText;
        public Text bossCountText;
        public Text gameStatusText;
        
        [Header("移动端控制UI")]
        public MobileInputController mobileInputController;
        
        [Header("暂停菜单按钮")]
        public Button pauseButton;
        public Button resumeButton;
        public Button restartButton;
        public Button settingsButton;
        public Button quitButton;
        
        [Header("游戏结束UI按钮")]
        public Button gameOverRestartButton;
        public Button gameOverQuitButton;
        
        [Header("胜利UI按钮")]
        public Button victoryRestartButton;
        public Button victoryQuitButton;
        
        private PlayerHealth playerHealth;
        private GameManager gameManager;
        
        // 单例模式
        public static UIManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            InitializeUI();
            SetupButtonEvents();
        }
        
        void InitializeUI()
        {
            // 查找组件
            if (playerHealth == null)
                playerHealth = FindObjectOfType<PlayerHealth>();
            
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            // 设置初始UI状态
            ShowGameplayUI();
            
            // 订阅玩家生命值变化事件
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnPlayerDeath += ShowGameOverUI;
            }
            
            // 初始化生命值显示
            UpdateHealthUI(playerHealth != null ? playerHealth.currentHealth : 100f);
        }
        
        void SetupButtonEvents()
        {
            // 暂停菜单按钮
            if (pauseButton != null)
                pauseButton.onClick.AddListener(ShowPauseUI);
            
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettingsUI);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            
            // 游戏结束按钮
            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(RestartGame);
            
            if (gameOverQuitButton != null)
                gameOverQuitButton.onClick.AddListener(QuitGame);
            
            // 胜利按钮
            if (victoryRestartButton != null)
                victoryRestartButton.onClick.AddListener(RestartGame);
            
            if (victoryQuitButton != null)
                victoryQuitButton.onClick.AddListener(QuitGame);
        }
        
        public void ShowGameplayUI()
        {
            SetActiveUI(gameplayUI);
            
            // 在移动端显示控制UI
            if (mobileInputController != null)
            {
                bool isMobile = Application.platform == RuntimePlatform.Android || 
                               Application.platform == RuntimePlatform.IPhonePlayer;
                mobileInputController.gameObject.SetActive(isMobile);
            }
        }
        
        public void ShowPauseUI()
        {
            SetActiveUI(pauseUI);
            
            if (gameManager != null)
            {
                gameManager.PauseGame();
            }
        }
        
        public void ShowGameOverUI()
        {
            SetActiveUI(gameOverUI);
        }
        
        public void ShowVictoryUI()
        {
            SetActiveUI(victoryUI);
        }
        
        public void ShowSettingsUI()
        {
            SetActiveUI(settingsUI);
        }
        
        void SetActiveUI(GameObject activeUI)
        {
            // 隐藏所有UI面板
            if (gameplayUI != null) gameplayUI.SetActive(false);
            if (pauseUI != null) pauseUI.SetActive(false);
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (victoryUI != null) victoryUI.SetActive(false);
            if (settingsUI != null) settingsUI.SetActive(false);
            
            // 显示指定的UI面板
            if (activeUI != null)
            {
                activeUI.SetActive(true);
            }
        }
        
        public void UpdateHealthUI(float currentHealth)
        {
            if (playerHealth == null) return;
            
            float maxHealth = playerHealth.maxHealth;
            float healthPercentage = currentHealth / maxHealth;
            
            // 更新血条
            if (healthBar != null)
            {
                healthBar.value = healthPercentage;
            }
            
            // 更新血量文本
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            }
        }
        
        public void UpdateBossCountUI(int defeated, int total)
        {
            if (bossCountText != null)
            {
                bossCountText.text = $"Boss: {defeated} / {total}";
            }
        }
        
        public void UpdateGameStatusUI(string status)
        {
            if (gameStatusText != null)
            {
                gameStatusText.text = status;
            }
        }
        
        // 按钮事件方法
        public void ResumeGame()
        {
            ShowGameplayUI();
            
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
        }
        
        public void RestartGame()
        {
            ShowGameplayUI();
            
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
        
        public void QuitGame()
        {
            if (gameManager != null)
            {
                gameManager.QuitGame();
            }
        }
        
        // 设置相关方法
        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = volume;
        }
        
        public void SetGraphicsQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }
        
        public void ToggleFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        void OnDestroy()
        {
            // 清理事件订阅
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
                playerHealth.OnPlayerDeath -= ShowGameOverUI;
            }
        }
    }
}
