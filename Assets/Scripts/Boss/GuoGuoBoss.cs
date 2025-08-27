using UnityEngine;

namespace BossBattle.Boss
{
    /// <summary>
    /// 果果Boss - 具有投掷攻击和治疗能力的Boss
    /// </summary>
    public class GuoGuoBoss : BossController
    {
        [Header("果果特殊技能")]
        public GameObject fruitProjectile;
        public Transform throwPoint;
        public float throwForce = 10f;
        public float healAmount = 20f;
        public float healCooldown = 10f;
        public GameObject healEffect;
        
        [Header("投掷攻击")]
        public int burstCount = 3;
        public float burstInterval = 0.3f;
        
        private float lastHealTime = 0f;
        private bool isHealing = false;
        private bool isThrowing = false;
        
        protected override void Start()
        {
            bossName = "果果";
            base.Start();
            
            // 如果没有设置投掷点，创建一个
            if (throwPoint == null)
            {
                GameObject throwPointObj = new GameObject("ThrowPoint");
                throwPointObj.transform.SetParent(transform);
                throwPointObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
                throwPoint = throwPointObj.transform;
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isDead) return;
            
            // 检查是否需要治疗
            if (currentHealth < maxHealth * 0.3f && Time.time - lastHealTime >= healCooldown && !isHealing)
            {
                StartHealing();
            }
        }
        
        protected override void PerformAttack()
        {
            if (player == null || isThrowing) return;
            
            // 随机选择攻击方式
            int attackType = Random.Range(0, 3);
            
            switch (attackType)
            {
                case 0:
                    StartSingleThrow();
                    break;
                case 1:
                    StartBurstThrow();
                    break;
                case 2:
                    StartArcThrow();
                    break;
            }
        }
        
        void StartSingleThrow()
        {
            UnityEngine.Debug.Log("果果使用单发投掷攻击！");
            
            isThrowing = true;
            
            if (animator != null)
            {
                animator.SetTrigger("SingleThrow");
            }
            
            // 延迟投掷
            Invoke(nameof(ThrowSingleFruit), 0.5f);
        }
        
        void ThrowSingleFruit()
        {
            if (fruitProjectile != null && throwPoint != null && player != null)
            {
                GameObject fruit = Instantiate(fruitProjectile, throwPoint.position, throwPoint.rotation);
                
                // 计算投掷方向
                Vector3 direction = (player.position - throwPoint.position).normalized;
                
                Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();
                if (fruitRb != null)
                {
                    fruitRb.velocity = direction * throwForce;
                }
                
                // 添加伤害组件
                FruitProjectile projectile = fruit.GetComponent<FruitProjectile>();
                if (projectile == null)
                {
                    projectile = fruit.AddComponent<FruitProjectile>();
                }
                projectile.damage = 15f;
                
                // 自动销毁
                Destroy(fruit, 5f);
            }
            
            isThrowing = false;
        }
        
        void StartBurstThrow()
        {
            UnityEngine.Debug.Log("果果使用连发投掷攻击！");
            
            isThrowing = true;
            
            if (animator != null)
            {
                animator.SetTrigger("BurstThrow");
            }
            
            // 连续投掷
            StartCoroutine(BurstThrowCoroutine());
        }
        
        private System.Collections.IEnumerator BurstThrowCoroutine()
        {
            for (int i = 0; i < burstCount; i++)
            {
                ThrowSingleFruit();
                yield return new WaitForSeconds(burstInterval);
            }
            
            isThrowing = false;
        }
        
        void StartArcThrow()
        {
            UnityEngine.Debug.Log("果果使用弧形投掷攻击！");
            
            isThrowing = true;
            
            if (animator != null)
            {
                animator.SetTrigger("ArcThrow");
            }
            
            // 延迟投掷
            Invoke(nameof(ThrowArcFruit), 0.5f);
        }
        
        void ThrowArcFruit()
        {
            if (fruitProjectile != null && throwPoint != null && player != null)
            {
                GameObject fruit = Instantiate(fruitProjectile, throwPoint.position, throwPoint.rotation);
                
                // 计算抛物线投掷
                Vector3 targetPosition = player.position;
                Vector3 direction = targetPosition - throwPoint.position;
                direction.y = 0;
                
                float distance = direction.magnitude;
                direction = direction.normalized;
                
                // 添加向上的力以形成抛物线
                Vector3 velocity = direction * throwForce + Vector3.up * (throwForce * 0.5f);
                
                Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();
                if (fruitRb != null)
                {
                    fruitRb.velocity = velocity;
                }
                
                // 添加伤害组件
                FruitProjectile projectile = fruit.GetComponent<FruitProjectile>();
                if (projectile == null)
                {
                    projectile = fruit.AddComponent<FruitProjectile>();
                }
                projectile.damage = 20f;
                
                // 自动销毁
                Destroy(fruit, 5f);
            }
            
            isThrowing = false;
        }
        
        void StartHealing()
        {
            UnityEngine.Debug.Log("果果开始治疗！");
            
            isHealing = true;
            lastHealTime = Time.time;
            
            if (animator != null)
            {
                animator.SetTrigger("Heal");
            }
            
            // 创建治疗特效
            if (healEffect != null)
            {
                GameObject effect = Instantiate(healEffect, transform.position, Quaternion.identity);
                effect.transform.SetParent(transform);
                Destroy(effect, 3f);
            }
            
            // 延迟执行治疗
            Invoke(nameof(PerformHeal), 1f);
        }
        
        void PerformHeal()
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            // 更新血条
            if (healthBar != null)
            {
                healthBar.value = currentHealth;
            }
            
            UnityEngine.Debug.Log($"果果恢复了 {healAmount} 点血量！当前血量: {currentHealth}");
            
            isHealing = false;
        }
        
        protected override void HandleChasing()
        {
            // 果果保持一定距离进行远程攻击
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer < attackRange * 0.7f)
            {
                // 太近了，后退
                Vector3 direction = (transform.position - player.position).normalized;
                direction.y = 0;
                
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else if (distanceToPlayer > attackRange * 1.5f)
            {
                // 太远了，前进
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            
            // 始终面向玩家
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", distanceToPlayer < attackRange * 0.7f || distanceToPlayer > attackRange * 1.5f);
                animator.SetBool("IsAttacking", false);
            }
        }
        
        protected override void OnTakeDamage()
        {
            base.OnTakeDamage();
            
            // 果果受伤时会短暂变绿
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                StartCoroutine(FlashGreen(renderer));
            }
        }
        
        private System.Collections.IEnumerator FlashGreen(Renderer renderer)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            renderer.material.color = originalColor;
        }
        
        protected override void OnDeath()
        {
            base.OnDeath();
            
            // 果果死亡时散落果实
            for (int i = 0; i < 5; i++)
            {
                if (fruitProjectile != null)
                {
                    Vector3 randomDirection = Random.insideUnitSphere;
                    randomDirection.y = Mathf.Abs(randomDirection.y);
                    
                    GameObject fruit = Instantiate(fruitProjectile, 
                        transform.position + Vector3.up, 
                        Quaternion.identity);
                    
                    Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();
                    if (fruitRb != null)
                    {
                        fruitRb.velocity = randomDirection * 5f;
                    }
                    
                    Destroy(fruit, 3f);
                }
            }
            
            // 延迟销毁Boss对象
            Destroy(gameObject, 3f);
        }
        
        void OnDrawGizmosSelected()
        {
            // 在编辑器中显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // 显示最佳攻击距离
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange * 0.7f);
            Gizmos.DrawWireSphere(transform.position, attackRange * 1.5f);
        }
    }
}
