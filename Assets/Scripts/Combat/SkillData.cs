using UnityEngine;

namespace BossBattle.Combat
{
    /// <summary>
    /// 技能类型枚举
    /// </summary>
    public enum SkillType
    {
        Projectile,  // 投掷物技能
        AOE,         // 范围攻击技能
        Heal,        // 治疗技能
        Buff         // 增益技能
    }
    
    /// <summary>
    /// 技能数据 - 存储技能的基础信息
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Boss Battle/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基础信息")]
        public string skillName = "新技能";
        public string description = "技能描述";
        public Sprite skillIcon;
        
        [Header("技能属性")]
        public SkillType skillType = SkillType.Projectile;
        public float damage = 0f;
        public float healAmount = 0f;
        public float range = 5f;
        public float cooldown = 3f;
        public float manaCost = 10f;
        
        [Header("视觉效果")]
        public GameObject effectPrefab;
        public GameObject projectilePrefab;
        public AudioClip soundEffect;
        
        [Header("运行时数据")]
        [System.NonSerialized]
        public float lastUsedTime = 0f;
        
        /// <summary>
        /// 检查技能是否可以使用
        /// </summary>
        public bool CanUse()
        {
            return Time.time - lastUsedTime >= cooldown;
        }
        
        /// <summary>
        /// 获取剩余冷却时间
        /// </summary>
        public float GetCooldownRemaining()
        {
            return Mathf.Max(0f, cooldown - (Time.time - lastUsedTime));
        }
        
        /// <summary>
        /// 获取冷却进度（0-1）
        /// </summary>
        public float GetCooldownProgress()
        {
            if (cooldown <= 0f) return 1f;
            return Mathf.Clamp01((Time.time - lastUsedTime) / cooldown);
        }
    }
}
