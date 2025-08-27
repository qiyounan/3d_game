using UnityEngine;
using System.Collections;

namespace BossBattle.Combat
{
    /// <summary>
    /// 伤害系统 - 处理伤害计算、类型和效果
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        [Header("伤害类型")]
        public DamageType damageType = DamageType.Physical;
        public float baseDamage = 25f;
        public float criticalChance = 0.1f;
        public float criticalMultiplier = 2f;
        
        [Header("伤害效果")]
        public GameObject damageEffectPrefab;
        public GameObject criticalEffectPrefab;
        public float effectDuration = 1f;
        
        [Header("击退效果")]
        public bool hasKnockback = true;
        public float knockbackForce = 5f;
        public float knockbackDuration = 0.3f;
        
        [Header("状态效果")]
        public StatusEffect[] statusEffects;
        
        public enum DamageType
        {
            Physical,
            Fire,
            Ice,
            Lightning,
            Poison,
            Heal
        }
        
        [System.Serializable]
        public class StatusEffect
        {
            public StatusType type;
            public float duration;
            public float value;
            public float tickInterval = 1f;
        }
        
        public enum StatusType
        {
            Burn,
            Freeze,
            Stun,
            Poison,
            Slow,
            Regeneration
        }
        
        /// <summary>
        /// 对目标造成伤害
        /// </summary>
        public DamageResult DealDamage(GameObject target, Vector3 hitPoint, Vector3 hitDirection)
        {
            DamageResult result = new DamageResult();
            
            // 计算最终伤害
            float finalDamage = CalculateDamage(target);
            result.damage = finalDamage;
            result.damageType = damageType;
            result.hitPoint = hitPoint;
            result.hitDirection = hitDirection;
            
            // 检查暴击
            if (Random.value < criticalChance)
            {
                finalDamage *= criticalMultiplier;
                result.isCritical = true;
            }
            
            // 应用伤害
            ApplyDamage(target, finalDamage, result);
            
            // 创建视觉效果
            CreateDamageEffects(result);
            
            // 应用击退
            if (hasKnockback)
            {
                ApplyKnockback(target, hitDirection);
            }
            
            // 应用状态效果
            ApplyStatusEffects(target);
            
            return result;
        }
        
        float CalculateDamage(GameObject target)
        {
            float damage = baseDamage;
            
            // 根据目标类型调整伤害
            if (target.CompareTag("Player"))
            {
                // 对玩家的伤害计算
                Player.PlayerHealth playerHealth = target.GetComponent<Player.PlayerHealth>();
                if (playerHealth != null && playerHealth.IsInvulnerable())
                {
                    return 0f; // 无敌状态不受伤害
                }
            }
            else if (target.CompareTag("Enemy"))
            {
                // 对敌人的伤害计算
                Boss.BossController bossController = target.GetComponent<Boss.BossController>();
                if (bossController != null && bossController.isDead)
                {
                    return 0f; // 已死亡不受伤害
                }
            }
            
            // 添加随机变化
            damage += Random.Range(-damage * 0.1f, damage * 0.1f);
            
            return Mathf.Max(0f, damage);
        }
        
        void ApplyDamage(GameObject target, float damage, DamageResult result)
        {
            if (damageType == DamageType.Heal)
            {
                // 治疗逻辑
                Player.PlayerHealth playerHealth = target.GetComponent<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(damage);
                }
            }
            else
            {
                // 伤害逻辑
                if (target.CompareTag("Player"))
                {
                    Player.PlayerHealth playerHealth = target.GetComponent<Player.PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
                else if (target.CompareTag("Enemy"))
                {
                    Boss.BossController bossController = target.GetComponent<Boss.BossController>();
                    if (bossController != null)
                    {
                        bossController.TakeDamage(damage);
                    }
                }
            }
            
            UnityEngine.Debug.Log($"对 {target.name} 造成 {damage:F1} 点{damageType}伤害" + (result.isCritical ? " (暴击!)" : ""));
        }
        
        void CreateDamageEffects(DamageResult result)
        {
            GameObject effectPrefab = result.isCritical ? criticalEffectPrefab : damageEffectPrefab;
            
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, result.hitPoint, Quaternion.LookRotation(result.hitDirection));
                
                // 设置效果颜色
                SetEffectColor(effect, result.damageType);
                
                Destroy(effect, effectDuration);
            }
            
            // 创建伤害数字显示
            CreateDamageNumber(result);
        }
        
        void SetEffectColor(GameObject effect, DamageType type)
        {
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                switch (type)
                {
                    case DamageType.Physical:
                        main.startColor = Color.white;
                        break;
                    case DamageType.Fire:
                        main.startColor = Color.red;
                        break;
                    case DamageType.Ice:
                        main.startColor = Color.cyan;
                        break;
                    case DamageType.Lightning:
                        main.startColor = Color.yellow;
                        break;
                    case DamageType.Poison:
                        main.startColor = Color.green;
                        break;
                    case DamageType.Heal:
                        main.startColor = Color.green;
                        break;
                }
            }
        }
        
        void CreateDamageNumber(DamageResult result)
        {
            // 创建浮动伤害数字
            GameObject damageNumberObj = new GameObject("Damage Number");
            damageNumberObj.transform.position = result.hitPoint + Vector3.up;
            
            // 添加Canvas组件
            Canvas canvas = damageNumberObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // 添加文本组件
            UnityEngine.UI.Text damageText = damageNumberObj.AddComponent<UnityEngine.UI.Text>();
            damageText.text = result.damage.ToString("F0");
            damageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            damageText.fontSize = 24;
            damageText.alignment = TextAnchor.MiddleCenter;
            
            // 设置颜色
            if (result.isCritical)
            {
                damageText.color = Color.yellow;
                damageText.fontSize = 32;
            }
            else if (result.damageType == DamageType.Heal)
            {
                damageText.color = Color.green;
                damageText.text = "+" + damageText.text;
            }
            else
            {
                damageText.color = Color.red;
            }
            
            // 设置RectTransform
            RectTransform rectTransform = damageText.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 50);
            
            // 添加动画
            StartCoroutine(AnimateDamageNumber(damageNumberObj));
        }
        
        IEnumerator AnimateDamageNumber(GameObject damageNumber)
        {
            Vector3 startPos = damageNumber.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;
            
            float duration = 1.5f;
            float elapsed = 0f;
            
            UnityEngine.UI.Text text = damageNumber.GetComponent<UnityEngine.UI.Text>();
            Color startColor = text.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 位置动画
                damageNumber.transform.position = Vector3.Lerp(startPos, endPos, t);
                
                // 透明度动画
                Color color = startColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                text.color = color;
                
                yield return null;
            }
            
            Destroy(damageNumber);
        }
        
        void ApplyKnockback(GameObject target, Vector3 direction)
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 knockbackDir = direction.normalized;
                knockbackDir.y = 0.3f; // 添加一点向上的力
                
                targetRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                
                // 短暂禁用移动控制
                StartCoroutine(DisableMovementTemporarily(target));
            }
        }
        
        IEnumerator DisableMovementTemporarily(GameObject target)
        {
            Player.FirstPersonController playerController = target.GetComponent<Player.FirstPersonController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                yield return new WaitForSeconds(knockbackDuration);
                playerController.enabled = true;
            }
        }
        
        void ApplyStatusEffects(GameObject target)
        {
            StatusEffectManager statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                statusManager = target.AddComponent<StatusEffectManager>();
            }
            
            foreach (var effect in statusEffects)
            {
                statusManager.ApplyStatusEffect(effect);
            }
        }
    }
    
    /// <summary>
    /// 伤害结果数据
    /// </summary>
    public class DamageResult
    {
        public float damage;
        public DamageSystem.DamageType damageType;
        public bool isCritical;
        public Vector3 hitPoint;
        public Vector3 hitDirection;
    }
}
