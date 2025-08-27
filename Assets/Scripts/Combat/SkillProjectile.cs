using UnityEngine;

namespace BossBattle.Combat
{
    /// <summary>
    /// 技能投掷物 - 处理技能投掷物的行为
    /// </summary>
    public class SkillProjectile : MonoBehaviour
    {
        [Header("投掷物设置")]
        public float damage = 25f;
        public float explosionRadius = 2f;
        public float speed = 15f;
        public LayerMask enemyLayer = 1;
        
        [Header("特效")]
        public GameObject explosionEffect;
        public GameObject trailEffect;
        
        private bool hasExploded = false;
        private Rigidbody rb;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false; // 技能投掷物通常不受重力影响
            }
            
            // 确保有碰撞器
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
                sphereCol.radius = 0.2f;
                sphereCol.isTrigger = true;
            }
            
            // 创建拖尾特效
            if (trailEffect != null)
            {
                GameObject trail = Instantiate(trailEffect, transform.position, transform.rotation);
                trail.transform.SetParent(transform);
            }
        }
        
        void Update()
        {
            // 如果没有刚体控制，手动移动
            if (rb == null || rb.isKinematic)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (hasExploded) return;
            
            // 检查是否碰到敌人或障碍物
            if (other.CompareTag("Enemy") || other.CompareTag("Ground") || other.CompareTag("Wall"))
            {
                Explode();
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (hasExploded) return;
            
            // 碰到任何物体都爆炸
            Explode();
        }
        
        void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;
            
            UnityEngine.Debug.Log("技能投掷物爆炸！");
            
            // 创建爆炸特效
            if (explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }
            
            // 检测爆炸范围内的敌人
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    var bossController = hitCollider.GetComponent<Boss.BossController>();
                    if (bossController != null)
                    {
                        bossController.TakeDamage(damage);
                        UnityEngine.Debug.Log($"技能对 {bossController.name} 造成 {damage} 点伤害！");
                        
                        // 添加击退效果
                        Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                        knockbackDirection.y = 0; // 只在水平方向击退
                        
                        Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                        if (enemyRb != null)
                        {
                            enemyRb.AddForce(knockbackDirection * 5f, ForceMode.Impulse);
                        }
                    }
                }
            }
            
            // 销毁投掷物
            Destroy(gameObject);
        }
        
        void OnDrawGizmosSelected()
        {
            // 在编辑器中显示爆炸范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
