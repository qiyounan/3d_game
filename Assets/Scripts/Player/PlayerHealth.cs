using UnityEngine;
using UnityEngine.UI;

namespace BossBattle.Player
{
    /// <summary>
    /// 玩家生命值系统
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("生命值设置")]
        public float maxHealth = 100f;
        public float currentHealth;
        
        [Header("UI显示")]
        public Slider healthBar;
        public Text healthText;
        
        [Header("受伤效果")]
        public float invulnerabilityTime = 1f;
        public GameObject damageEffect;
        
        private bool isInvulnerable = false;
        private float invulnerabilityTimer = 0f;
        
        // 事件
        public System.Action<float> OnHealthChanged;
        public System.Action OnPlayerDeath;
        
        void Start()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }
        
        void Update()
        {
            // 处理无敌时间
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (isInvulnerable || currentHealth <= 0) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            UnityEngine.Debug.Log($"玩家受到 {damage} 点伤害！剩余生命值: {currentHealth}");
            
            // 触发无敌时间
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityTime;
            
            // 创建受伤特效
            if (damageEffect != null)
            {
                GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            
            // 更新UI
            UpdateHealthUI();
            
            // 触发事件
            OnHealthChanged?.Invoke(currentHealth);
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 受伤反应
                StartCoroutine(DamageFlash());
            }
        }
        
        public void Heal(float healAmount)
        {
            if (currentHealth <= 0) return;
            
            float oldHealth = currentHealth;
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            float actualHeal = currentHealth - oldHealth;
            UnityEngine.Debug.Log($"玩家恢复了 {actualHeal} 点生命值！当前生命值: {currentHealth}");
            
            // 更新UI
            UpdateHealthUI();
            
            // 触发事件
            OnHealthChanged?.Invoke(currentHealth);
            
            // 治疗特效
            StartCoroutine(HealFlash());
        }
        
        void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            }
        }
        
        void Die()
        {
            UnityEngine.Debug.Log("玩家死亡！");
            
            // 触发死亡事件
            OnPlayerDeath?.Invoke();
            
            // 可以在这里添加死亡动画、音效等
            
            // 禁用玩家控制
            var controller = GetComponent<FirstPersonController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            var combatSystem = GetComponent<Combat.CombatSystem>();
            if (combatSystem != null)
            {
                combatSystem.enabled = false;
            }
            
            // 显示游戏结束界面
            ShowGameOverUI();
        }
        
        void ShowGameOverUI()
        {
            // 这里可以显示游戏结束界面
            UnityEngine.Debug.Log("显示游戏结束界面");
            
            // 暂停游戏
            Time.timeScale = 0f;
        }
        
        private System.Collections.IEnumerator DamageFlash()
        {
            // 受伤时的红色闪烁效果
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                // 可以添加屏幕红色闪烁效果
                // 这里简化处理
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private System.Collections.IEnumerator HealFlash()
        {
            // 治疗时的绿色闪烁效果
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                // 可以添加屏幕绿色闪烁效果
                // 这里简化处理
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        public bool IsAlive()
        {
            return currentHealth > 0;
        }
        
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }
        
        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }
        
        // 重置生命值（用于重新开始游戏）
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
            UpdateHealthUI();
            
            // 重新启用控制
            var controller = GetComponent<FirstPersonController>();
            if (controller != null)
            {
                controller.enabled = true;
            }
            
            var combatSystem = GetComponent<Combat.CombatSystem>();
            if (combatSystem != null)
            {
                combatSystem.enabled = true;
            }
            
            // 恢复游戏时间
            Time.timeScale = 1f;
        }
    }
}
