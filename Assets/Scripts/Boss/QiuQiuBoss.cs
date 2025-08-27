using UnityEngine;

namespace BossBattle.Boss
{
    /// <summary>
    /// 球球Boss - 具有滚动攻击和弹跳能力的Boss
    /// </summary>
    public class QiuQiuBoss : BossController
    {
        [Header("球球特殊技能")]
        public float rollAttackSpeed = 8f;
        public float bounceHeight = 5f;
        public float shockwaveRange = 3f;
        public GameObject shockwaveEffect;
        
        private bool isRolling = false;
        private bool isBouncing = false;
        private Vector3 rollDirection;
        private float rollTimer = 0f;
        private float rollDuration = 2f;
        
        protected override void Start()
        {
            bossName = "球球";
            base.Start();
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isDead) return;
            
            // 处理滚动攻击
            if (isRolling)
            {
                HandleRollAttack();
            }
            
            // 处理弹跳攻击
            if (isBouncing)
            {
                HandleBounceAttack();
            }
        }
        
        protected override void PerformAttack()
        {
            if (player == null) return;
            
            // 随机选择攻击方式
            int attackType = Random.Range(0, 3);
            
            switch (attackType)
            {
                case 0:
                    StartRollAttack();
                    break;
                case 1:
                    StartBounceAttack();
                    break;
                case 2:
                    StartShockwaveAttack();
                    break;
            }
        }
        
        void StartRollAttack()
        {
            UnityEngine.Debug.Log("球球使用滚动攻击！");
            
            isRolling = true;
            rollTimer = 0f;
            
            // 计算滚动方向
            rollDirection = (player.position - transform.position).normalized;
            rollDirection.y = 0;
            
            if (animator != null)
            {
                animator.SetTrigger("RollAttack");
            }
        }
        
        void HandleRollAttack()
        {
            rollTimer += Time.deltaTime;
            
            if (rollTimer < rollDuration)
            {
                // 快速滚动向玩家
                transform.position += rollDirection * rollAttackSpeed * Time.deltaTime;
                transform.Rotate(Vector3.right, rollAttackSpeed * 100 * Time.deltaTime);
                
                // 检测碰撞
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Player"))
                    {
                        // 对玩家造成伤害
                        UnityEngine.Debug.Log("球球的滚动攻击命中玩家！");
                        // 这里可以调用玩家的受伤方法
                        break;
                    }
                }
            }
            else
            {
                isRolling = false;
                rollTimer = 0f;
            }
        }
        
        void StartBounceAttack()
        {
            UnityEngine.Debug.Log("球球使用弹跳攻击！");
            
            isBouncing = true;
            
            // 向上弹跳
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.up * bounceHeight;
            }
            
            if (animator != null)
            {
                animator.SetTrigger("BounceAttack");
            }
            
            // 延迟执行落地攻击
            Invoke(nameof(LandingAttack), 1.5f);
        }
        
        void HandleBounceAttack()
        {
            // 在空中时可以添加一些特效
            if (transform.position.y > 1f)
            {
                // 添加空中特效
            }
        }
        
        void LandingAttack()
        {
            UnityEngine.Debug.Log("球球落地冲击！");
            
            isBouncing = false;
            
            // 创建冲击波效果
            if (shockwaveEffect != null)
            {
                GameObject effect = Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 检测冲击波范围内的玩家
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shockwaveRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    UnityEngine.Debug.Log("球球的落地冲击命中玩家！");
                    // 这里可以调用玩家的受伤方法
                    break;
                }
            }
        }
        
        void StartShockwaveAttack()
        {
            UnityEngine.Debug.Log("球球使用冲击波攻击！");
            
            // 创建冲击波
            if (shockwaveEffect != null)
            {
                GameObject effect = Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 检测范围内的玩家
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shockwaveRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    UnityEngine.Debug.Log("球球的冲击波攻击命中玩家！");
                    // 这里可以调用玩家的受伤方法
                    break;
                }
            }
            
            if (animator != null)
            {
                animator.SetTrigger("ShockwaveAttack");
            }
        }
        
        protected override void OnTakeDamage()
        {
            base.OnTakeDamage();
            
            // 球球受伤时会短暂变红
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                StartCoroutine(FlashRed(renderer));
            }
        }
        
        private System.Collections.IEnumerator FlashRed(Renderer renderer)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            renderer.material.color = originalColor;
        }
        
        protected override void OnDeath()
        {
            base.OnDeath();
            
            // 球球死亡时爆炸效果
            if (shockwaveEffect != null)
            {
                GameObject explosion = Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
                explosion.transform.localScale *= 2f; // 放大爆炸效果
                Destroy(explosion, 3f);
            }
            
            // 延迟销毁Boss对象
            Destroy(gameObject, 3f);
        }
        
        void OnDrawGizmosSelected()
        {
            // 在编辑器中显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shockwaveRange);
        }
    }
}
