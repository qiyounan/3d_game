using UnityEngine;
using UnityEngine.UI;
using BossBattle.Player;
using BossBattle.Boss;

namespace BossBattle
{
    /// <summary>
    /// 游戏管理器 - 控制游戏流程和状态
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("游戏状态")]
        public GameState currentState = GameState.Playing;
        
        [Header("Boss设置")]
        public BossController[] bosses;
        public Transform[] bossSpawnPoints;
        
        [Header("玩家")]
        public PlayerHealth playerHealth;
        public Transform playerSpawnPoint;
        
        [Header("UI界面")]
        public GameObject gameUI;
        public GameObject pauseMenu;
        public GameObject gameOverMenu;
        public GameObject victoryMenu;
        
        [Header("UI文本")]
        public Text bossCountText;
        public Text gameStatusText;
        
        private int defeatedBossCount = 0;
        private int totalBossCount = 0;
        
        public enum GameState
        {
            Playing,
            Paused,
            GameOver,
            Victory
        }
        
        // 单例模式
        public static GameManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            InitializeGame();
        }
        
        void Update()
        {
            HandleInput();
            UpdateUI();
        }
        
        void InitializeGame()
        {
            // 设置游戏状态
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            
            // 统计Boss数量
            totalBossCount = bosses.Length;
            defeatedBossCount = 0;
            
            // 生成Boss
            SpawnBosses();
            
            // 设置玩家事件
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath += OnPlayerDeath;
            }
            
            // 初始化UI
            UpdateGameUI();
        }
        
        void SpawnBosses()
        {
            for (int i = 0; i < bosses.Length && i < bossSpawnPoints.Length; i++)
            {
                if (bosses[i] != null && bossSpawnPoints[i] != null)
                {
                    // 实例化Boss
                    GameObject bossObj = Instantiate(bosses[i].gameObject, bossSpawnPoints[i].position, bossSpawnPoints[i].rotation);
                    BossController bossController = bossObj.GetComponent<BossController>();
                    
                    if (bossController != null)
                    {
                        // 订阅Boss死亡事件
                        bossController.OnBossDeath += OnBossDefeated;
                    }
                }
            }
        }
        
        void HandleInput()
        {
            // 暂停游戏（ESC键或移动端暂停按钮）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }
        
        void UpdateUI()
        {
            // 更新Boss计数显示
            if (bossCountText != null)
            {
                bossCountText.text = $"Boss: {defeatedBossCount} / {totalBossCount}";
            }
            
            // 更新游戏状态显示
            if (gameStatusText != null)
            {
                switch (currentState)
                {
                    case GameState.Playing:
                        gameStatusText.text = "战斗中...";
                        break;
                    case GameState.Paused:
                        gameStatusText.text = "游戏暂停";
                        break;
                    case GameState.GameOver:
                        gameStatusText.text = "游戏结束";
                        break;
                    case GameState.Victory:
                        gameStatusText.text = "胜利！";
                        break;
                }
            }
        }
        
        void UpdateGameUI()
        {
            // 显示/隐藏相应的UI界面
            if (gameUI != null)
                gameUI.SetActive(currentState == GameState.Playing);
            
            if (pauseMenu != null)
                pauseMenu.SetActive(currentState == GameState.Paused);
            
            if (gameOverMenu != null)
                gameOverMenu.SetActive(currentState == GameState.GameOver);
            
            if (victoryMenu != null)
                victoryMenu.SetActive(currentState == GameState.Victory);
        }
        
        public void PauseGame()
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            UpdateGameUI();
            UnityEngine.Debug.Log("游戏暂停");
        }
        
        public void ResumeGame()
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            UpdateGameUI();
            UnityEngine.Debug.Log("游戏继续");
        }
        
        public void RestartGame()
        {
            UnityEngine.Debug.Log("重新开始游戏");
            
            // 重置游戏状态
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            defeatedBossCount = 0;
            
            // 重置玩家
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
                
                // 重置玩家位置
                if (playerSpawnPoint != null)
                {
                    playerHealth.transform.position = playerSpawnPoint.position;
                    playerHealth.transform.rotation = playerSpawnPoint.rotation;
                }
            }
            
            // 清除现有Boss
            BossController[] existingBosses = FindObjectsOfType<BossController>();
            foreach (var boss in existingBosses)
            {
                Destroy(boss.gameObject);
            }
            
            // 重新生成Boss
            SpawnBosses();
            
            // 更新UI
            UpdateGameUI();
        }
        
        public void QuitGame()
        {
            UnityEngine.Debug.Log("退出游戏");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        void OnPlayerDeath()
        {
            UnityEngine.Debug.Log("玩家死亡，游戏结束");
            currentState = GameState.GameOver;
            UpdateGameUI();
        }
        
        void OnBossDefeated()
        {
            defeatedBossCount++;
            UnityEngine.Debug.Log($"Boss被击败！已击败: {defeatedBossCount}/{totalBossCount}");
            
            // 检查是否所有Boss都被击败
            if (defeatedBossCount >= totalBossCount)
            {
                OnAllBossesDefeated();
            }
        }
        
        void OnAllBossesDefeated()
        {
            UnityEngine.Debug.Log("所有Boss都被击败了！玩家胜利！");
            currentState = GameState.Victory;
            UpdateGameUI();
        }
        
        // 公共方法供UI按钮调用
        public void OnResumeButtonClicked()
        {
            ResumeGame();
        }
        
        public void OnRestartButtonClicked()
        {
            RestartGame();
        }
        
        public void OnQuitButtonClicked()
        {
            QuitGame();
        }
        
        void OnDestroy()
        {
            // 清理事件订阅
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath -= OnPlayerDeath;
            }
        }
    }
}
