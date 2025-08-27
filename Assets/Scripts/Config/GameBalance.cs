using UnityEngine;

namespace BossBattle.Config
{
    /// <summary>
    /// 游戏平衡性配置 - 集中管理游戏数值平衡
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Boss Battle/Game Balance")]
    public class GameBalance : ScriptableObject
    {
        [Header("玩家设置")]
        public PlayerConfig player;
        
        [Header("Boss设置")]
        public BossConfig qiuqiuBoss;
        public BossConfig guoguoBoss;
        
        [Header("技能设置")]
        public SkillConfig[] skills;
        
        [Header("战斗设置")]
        public CombatConfig combat;
        
        [Header("移动端优化")]
        public MobileConfig mobile;
        
        [System.Serializable]
        public class PlayerConfig
        {
            [Header("生命值")]
            public float maxHealth = 100f;
            public float healthRegenRate = 0f;
            
            [Header("移动")]
            public float moveSpeed = 5f;
            public float jumpForce = 8f;
            public float mouseSensitivity = 100f;
            
            [Header("战斗")]
            public float attackDamage = 25f;
            public float attackRange = 3f;
            public float attackCooldown = 1f;
            
            [Header("防御")]
            public float invulnerabilityTime = 1f;
            public float damageReduction = 0f;
        }
        
        [System.Serializable]
        public class BossConfig
        {
            [Header("基础属性")]
            public string bossName = "Boss";
            public float maxHealth = 200f;
            public float moveSpeed = 3f;
            
            [Header("攻击属性")]
            public float attackDamage = 30f;
            public float attackRange = 5f;
            public float attackCooldown = 2f;
            
            [Header("AI设置")]
            public float detectionRange = 15f;
            public float aggressiveness = 0.7f;
            public float defensiveness = 0.3f;
            
            [Header("特殊技能")]
            public float specialSkillCooldown = 10f;
            public float specialSkillDamage = 50f;
        }
        
        [System.Serializable]
        public class SkillConfig
        {
            public string skillName = "技能";
            public float damage = 40f;
            public float cooldown = 3f;
            public float range = 5f;
            public float manaCost = 10f;
            public float healAmount = 0f;
        }
        
        [System.Serializable]
        public class CombatConfig
        {
            [Header("伤害系统")]
            public float criticalChance = 0.1f;
            public float criticalMultiplier = 2f;
            public float damageVariance = 0.1f;
            
            [Header("击退效果")]
            public float knockbackForce = 5f;
            public float knockbackDuration = 0.3f;
            
            [Header("状态效果")]
            public float burnDamagePerSecond = 5f;
            public float poisonDamagePerSecond = 3f;
            public float slowEffectMultiplier = 0.5f;
            public float stunDuration = 2f;
        }
        
        [System.Serializable]
        public class MobileConfig
        {
            [Header("触控设置")]
            public float joystickSensitivity = 1f;
            public float touchSensitivity = 2f;
            public float deadZone = 0.1f;
            
            [Header("性能设置")]
            public int targetFrameRate = 30;
            public float renderScale = 1f;
            public bool enableVSync = false;
            
            [Header("UI设置")]
            public float buttonSize = 80f;
            public float uiScale = 1f;
            public bool showPerformanceInfo = false;
        }
        
        // 单例访问
        private static GameBalance _instance;
        public static GameBalance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameBalance>("GameBalance");
                    if (_instance == null)
                    {
                        UnityEngine.Debug.LogError("GameBalance配置文件未找到！请在Resources文件夹中创建GameBalance.asset");
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 应用平衡性设置到游戏对象
        /// </summary>
        public void ApplyToPlayer(Player.PlayerHealth playerHealth, Player.FirstPersonController playerController, Combat.CombatSystem combatSystem)
        {
            if (playerHealth != null)
            {
                playerHealth.maxHealth = player.maxHealth;
                playerHealth.invulnerabilityTime = player.invulnerabilityTime;
            }
            
            if (playerController != null)
            {
                playerController.moveSpeed = player.moveSpeed;
                playerController.jumpForce = player.jumpForce;
                playerController.mouseSensitivity = player.mouseSensitivity;
            }
            
            if (combatSystem != null)
            {
                combatSystem.attackDamage = player.attackDamage;
                combatSystem.attackRange = player.attackRange;
                combatSystem.attackCooldown = player.attackCooldown;
            }
        }
        
        /// <summary>
        /// 应用Boss设置
        /// </summary>
        public void ApplyToBoss(Boss.BossController boss, BossType bossType)
        {
            BossConfig config = bossType == BossType.QiuQiu ? qiuqiuBoss : guoguoBoss;
            
            if (boss != null && config != null)
            {
                boss.bossName = config.bossName;
                boss.maxHealth = config.maxHealth;
                boss.moveSpeed = config.moveSpeed;
                boss.attackRange = config.attackRange;
                boss.attackCooldown = config.attackCooldown;
                
                // 重置当前血量
                boss.currentHealth = boss.maxHealth;
            }
        }
        
        /// <summary>
        /// 应用技能设置
        /// </summary>
        public void ApplyToSkills(Combat.CombatSystem combatSystem)
        {
            if (combatSystem != null && skills != null)
            {
                for (int i = 0; i < skills.Length && i < combatSystem.skills.Length; i++)
                {
                    if (combatSystem.skills[i] != null && skills[i] != null)
                    {
                        combatSystem.skills[i].damage = skills[i].damage;
                        combatSystem.skills[i].cooldown = skills[i].cooldown;
                        combatSystem.skills[i].range = skills[i].range;
                        combatSystem.skills[i].manaCost = skills[i].manaCost;
                        combatSystem.skills[i].healAmount = skills[i].healAmount;
                    }
                }
            }
        }
        
        /// <summary>
        /// 应用移动端设置
        /// </summary>
        public void ApplyMobileSettings()
        {
            Application.targetFrameRate = mobile.targetFrameRate;
            QualitySettings.vSyncCount = mobile.enableVSync ? 1 : 0;
            
            // 应用渲染缩放 (需要URP包才能使用)
            // 如果没有安装URP，这部分代码会被跳过
        }
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        [ContextMenu("Reset to Default Values")]
        public void ResetToDefaults()
        {
            // 玩家默认值
            player = new PlayerConfig();
            
            // Boss默认值
            qiuqiuBoss = new BossConfig
            {
                bossName = "球球",
                maxHealth = 150f,
                moveSpeed = 4f,
                attackDamage = 25f,
                aggressiveness = 0.8f
            };
            
            guoguoBoss = new BossConfig
            {
                bossName = "果果",
                maxHealth = 120f,
                moveSpeed = 2.5f,
                attackDamage = 20f,
                aggressiveness = 0.6f,
                defensiveness = 0.4f
            };
            
            // 技能默认值
            skills = new SkillConfig[3]
            {
                new SkillConfig { skillName = "火球术", damage = 40f, cooldown = 3f, range = 10f },
                new SkillConfig { skillName = "冲击波", damage = 30f, cooldown = 5f, range = 5f },
                new SkillConfig { skillName = "治疗术", healAmount = 50f, cooldown = 8f }
            };
            
            // 战斗默认值
            combat = new CombatConfig();
            
            // 移动端默认值
            mobile = new MobileConfig();
        }
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool ValidateConfig()
        {
            bool isValid = true;
            
            // 检查玩家配置
            if (player.maxHealth <= 0)
            {
                UnityEngine.Debug.LogError("玩家最大生命值必须大于0");
                isValid = false;
            }

            if (player.moveSpeed <= 0)
            {
                UnityEngine.Debug.LogError("玩家移动速度必须大于0");
                isValid = false;
            }

            // 检查Boss配置
            if (qiuqiuBoss.maxHealth <= 0 || guoguoBoss.maxHealth <= 0)
            {
                UnityEngine.Debug.LogError("Boss最大生命值必须大于0");
                isValid = false;
            }

            // 检查技能配置
            foreach (var skill in skills)
            {
                if (skill.cooldown < 0)
                {
                    UnityEngine.Debug.LogError($"技能 {skill.skillName} 的冷却时间不能为负数");
                    isValid = false;
                }
            }
            
            return isValid;
        }
        
        public enum BossType
        {
            QiuQiu,
            GuoGuo
        }
    }
}
