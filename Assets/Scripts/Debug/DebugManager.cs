using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace BossBattle.Debug
{
    /// <summary>
    /// 调试管理器 - 提供游戏调试和测试功能
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        [Header("调试设置")]
        public bool enableDebugMode = true;
        public bool showFPS = true;
        public bool showMemoryUsage = true;
        public bool showPlayerInfo = true;
        public bool showBossInfo = true;
        
        [Header("快捷键")]
        public KeyCode toggleDebugKey = KeyCode.F1;
        public KeyCode godModeKey = KeyCode.F2;
        public KeyCode killAllBossesKey = KeyCode.F3;
        public KeyCode resetGameKey = KeyCode.F4;
        
        [Header("性能监控")]
        public float updateInterval = 0.5f;
        
        private bool debugUIVisible = false;
        private bool godModeEnabled = false;
        private float deltaTime = 0f;
        private float fps = 0f;
        private float memoryUsage = 0f;
        private StringBuilder debugText = new StringBuilder();
        
        // 组件引用
        private Player.PlayerHealth playerHealth;
        private Player.FirstPersonController playerController;
        private Combat.CombatSystem combatSystem;
        private Boss.BossController[] bosses;
        
        // 单例模式
        public static DebugManager Instance { get; private set; }
        
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
            // 只在开发版本中启用
            if (!UnityEngine.Debug.isDebugBuild)
            {
                enableDebugMode = false;
                enabled = false;
                return;
            }
            
            FindGameComponents();
            InvokeRepeating(nameof(UpdatePerformanceStats), 0f, updateInterval);
        }
        
        void Update()
        {
            if (!enableDebugMode) return;
            
            HandleInput();
            UpdateDeltaTime();
        }
        
        void HandleInput()
        {
            // 切换调试UI
            if (Input.GetKeyDown(toggleDebugKey))
            {
                debugUIVisible = !debugUIVisible;
            }
            
            // 上帝模式
            if (Input.GetKeyDown(godModeKey))
            {
                ToggleGodMode();
            }
            
            // 击杀所有Boss
            if (Input.GetKeyDown(killAllBossesKey))
            {
                KillAllBosses();
            }
            
            // 重置游戏
            if (Input.GetKeyDown(resetGameKey))
            {
                ResetGame();
            }
        }
        
        void UpdateDeltaTime()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
        
        void UpdatePerformanceStats()
        {
            // 计算FPS
            fps = 1f / deltaTime;
            
            // 计算内存使用
            memoryUsage = System.GC.GetTotalMemory(false) / 1024f / 1024f; // MB
        }
        
        void FindGameComponents()
        {
            playerHealth = FindObjectOfType<Player.PlayerHealth>();
            playerController = FindObjectOfType<Player.FirstPersonController>();
            combatSystem = FindObjectOfType<Combat.CombatSystem>();
            bosses = FindObjectsOfType<Boss.BossController>();
        }
        
        void ToggleGodMode()
        {
            godModeEnabled = !godModeEnabled;
            
            if (playerHealth != null)
            {
                if (godModeEnabled)
                {
                    // 启用上帝模式
                    playerHealth.currentHealth = playerHealth.maxHealth;
                    UnityEngine.Debug.Log("上帝模式已启用");
                }
                else
                {
                    UnityEngine.Debug.Log("上帝模式已禁用");
                }
            }
        }
        
        void KillAllBosses()
        {
            bosses = FindObjectsOfType<Boss.BossController>();
            foreach (var boss in bosses)
            {
                if (boss != null && !boss.isDead)
                {
                    boss.TakeDamage(boss.maxHealth);
                }
            }
            UnityEngine.Debug.Log("所有Boss已被击杀");
        }
        
        void ResetGame()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
            UnityEngine.Debug.Log("游戏已重置");
        }
        
        void OnGUI()
        {
            if (!enableDebugMode || !debugUIVisible) return;
            
            // 设置GUI样式
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
            
            // 主调试面板
            GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));
            GUILayout.BeginVertical(boxStyle);
            
            GUILayout.Label("=== 调试信息 ===", style);
            
            // 性能信息
            if (showFPS)
            {
                DrawPerformanceInfo(style);
            }
            
            // 玩家信息
            if (showPlayerInfo)
            {
                DrawPlayerInfo(style);
            }
            
            // Boss信息
            if (showBossInfo)
            {
                DrawBossInfo(style);
            }
            
            // 调试按钮
            DrawDebugButtons();
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            // 快捷键提示
            DrawShortcutHelp();
        }
        
        void DrawPerformanceInfo(GUIStyle style)
        {
            GUILayout.Label("--- 性能信息 ---", style);
            GUILayout.Label($"FPS: {fps:F1}", style);
            GUILayout.Label($"帧时间: {deltaTime * 1000:F1}ms", style);
            
            if (showMemoryUsage)
            {
                GUILayout.Label($"内存使用: {memoryUsage:F1}MB", style);
            }
            
            GUILayout.Label($"质量等级: {QualitySettings.GetQualityLevel()}", style);
            GUILayout.Space(10);
        }
        
        void DrawPlayerInfo(GUIStyle style)
        {
            GUILayout.Label("--- 玩家信息 ---", style);
            
            if (playerHealth != null)
            {
                GUILayout.Label($"生命值: {playerHealth.currentHealth:F0}/{playerHealth.maxHealth:F0}", style);
                GUILayout.Label($"无敌状态: {playerHealth.IsInvulnerable()}", style);
            }
            
            if (playerController != null)
            {
                Vector3 pos = playerController.transform.position;
                GUILayout.Label($"位置: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})", style);
                GUILayout.Label($"移动速度: {playerController.moveSpeed:F1}", style);
            }
            
            GUILayout.Label($"上帝模式: {godModeEnabled}", style);
            GUILayout.Space(10);
        }
        
        void DrawBossInfo(GUIStyle style)
        {
            GUILayout.Label("--- Boss信息 ---", style);
            
            bosses = FindObjectsOfType<Boss.BossController>();
            
            if (bosses.Length == 0)
            {
                GUILayout.Label("没有找到Boss", style);
            }
            else
            {
                foreach (var boss in bosses)
                {
                    if (boss != null)
                    {
                        string status = boss.isDead ? "已死亡" : "存活";
                        GUILayout.Label($"{boss.bossName}: {boss.currentHealth:F0}/{boss.maxHealth:F0} ({status})", style);
                        GUILayout.Label($"状态: {boss.currentState}", style);
                    }
                }
            }
            
            GUILayout.Space(10);
        }
        
        void DrawDebugButtons()
        {
            GUILayout.Label("--- 调试功能 ---");
            
            if (GUILayout.Button("切换上帝模式"))
            {
                ToggleGodMode();
            }
            
            if (GUILayout.Button("击杀所有Boss"))
            {
                KillAllBosses();
            }
            
            if (GUILayout.Button("重置游戏"))
            {
                ResetGame();
            }
            
            if (GUILayout.Button("满血"))
            {
                if (playerHealth != null)
                {
                    playerHealth.Heal(playerHealth.maxHealth);
                }
            }
            
            if (GUILayout.Button("传送到中心"))
            {
                if (playerController != null)
                {
                    playerController.transform.position = Vector3.zero + Vector3.up;
                }
            }
        }
        
        void DrawShortcutHelp()
        {
            GUIStyle helpStyle = new GUIStyle();
            helpStyle.fontSize = 12;
            helpStyle.normal.textColor = Color.yellow;
            
            float y = Screen.height - 100;
            GUI.Label(new Rect(10, y, 300, 20), $"{toggleDebugKey}: 切换调试UI", helpStyle);
            GUI.Label(new Rect(10, y + 20, 300, 20), $"{godModeKey}: 上帝模式", helpStyle);
            GUI.Label(new Rect(10, y + 40, 300, 20), $"{killAllBossesKey}: 击杀所有Boss", helpStyle);
            GUI.Label(new Rect(10, y + 60, 300, 20), $"{resetGameKey}: 重置游戏", helpStyle);
        }
        
        Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        // 公共方法供其他脚本调用
        public void LogDebugInfo(string message)
        {
            if (enableDebugMode)
            {
                UnityEngine.Debug.Log($"[DEBUG] {message}");
            }
        }
        
        public void LogWarning(string message)
        {
            if (enableDebugMode)
            {
                UnityEngine.Debug.LogWarning($"[WARNING] {message}");
            }
        }
        
        public void LogError(string message)
        {
            if (enableDebugMode)
            {
                UnityEngine.Debug.LogError($"[ERROR] {message}");
            }
        }
        
        public bool IsGodModeEnabled()
        {
            return godModeEnabled;
        }
        
        public float GetCurrentFPS()
        {
            return fps;
        }
        
        public float GetMemoryUsage()
        {
            return memoryUsage;
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (enableDebugMode)
            {
                UnityEngine.Debug.Log($"应用程序暂停状态: {pauseStatus}");
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (enableDebugMode)
            {
                UnityEngine.Debug.Log($"应用程序焦点状态: {hasFocus}");
            }
        }
    }
}
