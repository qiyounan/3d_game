using UnityEngine;
using UnityEngine.UI;

namespace BossBattle.Boss
{
    /// <summary>
    /// Boss控制器基类 - 处理Boss的基础行为和状态
    /// </summary>
    public abstract class BossController : MonoBehaviour
    {
        [Header("Boss基础设置")]
        public string bossName = "Boss";
        public float maxHealth = 100f;
        public float moveSpeed = 3f;
        public float attackRange = 5f;
        public float attackCooldown = 2f;
        
        [Header("UI显示")]
        public Canvas nameCanvas;
        public Text nameText;
        public Slider healthBar;
        
        [Header("目标")]
        public Transform player;
        
        public float currentHealth;
        public float lastAttackTime;
        protected Animator animator;
        public bool isDead = false;

        // 事件
        public System.Action OnBossDeath;
        
        // Boss状态枚举
        public enum BossState
        {
            Idle,
            Chasing,
            Attacking,
            Dead
        }
        
        public BossState currentState = BossState.Idle;
        
        protected virtual void Start()
        {
            currentHealth = maxHealth;
            animator = GetComponent<Animator>();
            
            // 设置名称显示
            if (nameText != null)
            {
                nameText.text = bossName;
            }
            
            // 设置血条
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }
            
            // 查找玩家
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
            
            // 设置名称UI始终面向摄像机
            if (nameCanvas != null)
            {
                nameCanvas.worldCamera = Camera.main;
            }
        }
        
        protected virtual void Update()
        {
            if (isDead) return;
            
            UpdateNameUIRotation();
            UpdateState();
            HandleState();
        }
        
        void UpdateNameUIRotation()
        {
            // 让名称UI始终面向摄像机
            if (nameCanvas != null && Camera.main != null)
            {
                nameCanvas.transform.LookAt(Camera.main.transform);
                nameCanvas.transform.Rotate(0, 180, 0); // 翻转文字
            }
        }
        
        protected virtual void UpdateState()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            switch (currentState)
            {
                case BossState.Idle:
                    if (distanceToPlayer <= attackRange * 2)
                    {
                        currentState = BossState.Chasing;
                    }
                    break;
                    
                case BossState.Chasing:
                    if (distanceToPlayer <= attackRange)
                    {
                        if (Time.time - lastAttackTime >= attackCooldown)
                        {
                            currentState = BossState.Attacking;
                        }
                    }
                    else if (distanceToPlayer > attackRange * 3)
                    {
                        currentState = BossState.Idle;
                    }
                    break;
                    
                case BossState.Attacking:
                    // 攻击完成后返回追逐状态
                    if (Time.time - lastAttackTime >= 1f) // 攻击动画时间
                    {
                        currentState = BossState.Chasing;
                    }
                    break;
            }
        }
        
        protected virtual void HandleState()
        {
            switch (currentState)
            {
                case BossState.Idle:
                    HandleIdle();
                    break;
                case BossState.Chasing:
                    HandleChasing();
                    break;
                case BossState.Attacking:
                    HandleAttacking();
                    break;
            }
        }
        
        protected virtual void HandleIdle()
        {
            // 待机行为 - 可以添加巡逻等
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsAttacking", false);
            }
        }
        
        protected virtual void HandleChasing()
        {
            if (player == null) return;
            
            // 朝向玩家移动
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // 保持在地面上
            
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
                animator.SetBool("IsAttacking", false);
            }
        }
        
        protected virtual void HandleAttacking()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsAttacking", true);
            }
        }
        
        protected abstract void PerformAttack();
        
        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            // 更新血条
            if (healthBar != null)
            {
                healthBar.value = currentHealth;
            }
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 受伤反应
                OnTakeDamage();
            }
        }
        
        protected virtual void OnTakeDamage()
        {
            // 受伤时的反应，如播放受伤动画、音效等
            UnityEngine.Debug.Log($"{bossName} 受到伤害！剩余血量: {currentHealth}");
        }
        
        protected virtual void Die()
        {
            isDead = true;
            currentState = BossState.Dead;
            
            UnityEngine.Debug.Log($"{bossName} 被击败了！");
            
            if (animator != null)
            {
                animator.SetBool("IsDead", true);
            }
            
            // 可以添加死亡特效、掉落物品等
            OnDeath();
        }
        
        protected virtual void OnDeath()
        {
            // 死亡时的处理，如播放死亡动画、音效、给予奖励等

            // 触发死亡事件
            OnBossDeath?.Invoke();
        }
    }
}
