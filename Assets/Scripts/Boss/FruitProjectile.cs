using UnityEngine;

namespace BossBattle.Boss
{
    /// <summary>
    /// 果实投掷物 - 果果Boss的攻击投掷物
    /// </summary>
    public class FruitProjectile : MonoBehaviour
    {
        [Header("投掷物设置")]
        public float damage = 15f;
        public float explosionRadius = 2f;
        public GameObject explosionEffect;
        public LayerMask playerLayer = 1;
        
        private bool hasExploded = false;
        
        void Start()
        {
            // 添加碰撞检测
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            // 确保有碰撞器
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
                sphereCol.radius = 0.2f;
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (hasExploded) return;
            
            // 检查是否碰到玩家或地面
            if (other.CompareTag("Player") || other.CompareTag("Ground"))
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
            
            UnityEngine.Debug.Log("果实爆炸！");
            
            // 创建爆炸特效
            if (explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 检测爆炸范围内的玩家
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    UnityEngine.Debug.Log($"果实爆炸对玩家造成 {damage} 点伤害！");
                    
                    // 这里可以调用玩家的受伤方法
                    // PlayerController playerController = hitCollider.GetComponent<PlayerController>();
                    // if (playerController != null)
                    // {
                    //     playerController.TakeDamage(damage);
                    // }
                    
                    break;
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
