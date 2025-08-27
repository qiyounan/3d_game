using UnityEngine;
using BossBattle.Player;

namespace BossBattle.Combat
{
    /// <summary>
    /// 战斗系统 - 处理玩家的攻击和技能
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("攻击设置")]
        public float attackRange = 3f;
        public float attackDamage = 25f;
        public float attackCooldown = 1f;
        public LayerMask enemyLayer = 1;
        
        [Header("技能设置")]
        public SkillData[] skills = new SkillData[3];
        
        [Header("特效")]
        public GameObject attackEffect;
        public Transform attackPoint;
        
        private float lastAttackTime = 0f;
        private Camera playerCamera;
        
        void Start()
        {
            playerCamera = Camera.main;
            
            // 如果没有设置攻击点，创建一个
            if (attackPoint == null)
            {
                GameObject attackPointObj = new GameObject("AttackPoint");
                attackPointObj.transform.SetParent(transform);
                attackPointObj.transform.localPosition = new Vector3(0, 0, 1f);
                attackPoint = attackPointObj.transform;
            }
            
            // 初始化技能
            InitializeSkills();
        }
        
        void InitializeSkills()
        {
            // 技能1：火球术
            if (skills.Length > 0 && skills[0] == null)
            {
                skills[0] = ScriptableObject.CreateInstance<SkillData>();
                skills[0].skillName = "火球术";
                skills[0].damage = 40f;
                skills[0].cooldown = 3f;
                skills[0].range = 10f;
                skills[0].skillType = SkillType.Projectile;
            }
            
            // 技能2：冲击波
            if (skills.Length > 1 && skills[1] == null)
            {
                skills[1] = ScriptableObject.CreateInstance<SkillData>();
                skills[1].skillName = "冲击波";
                skills[1].damage = 30f;
                skills[1].cooldown = 5f;
                skills[1].range = 5f;
                skills[1].skillType = SkillType.AOE;
            }
            
            // 技能3：治疗术
            if (skills.Length > 2 && skills[2] == null)
            {
                skills[2] = ScriptableObject.CreateInstance<SkillData>();
                skills[2].skillName = "治疗术";
                skills[2].healAmount = 50f;
                skills[2].cooldown = 8f;
                skills[2].skillType = SkillType.Heal;
            }
        }
        
        public void PerformAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
            {
                UnityEngine.Debug.Log("攻击冷却中...");
                return;
            }

            lastAttackTime = Time.time;

            UnityEngine.Debug.Log("执行普通攻击！");
            
            // 创建攻击特效
            if (attackEffect != null && attackPoint != null)
            {
                GameObject effect = Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
                Destroy(effect, 1f);
            }
            
            // 检测攻击范围内的敌人
            Vector3 attackDirection = playerCamera.transform.forward;
            RaycastHit[] hits = Physics.RaycastAll(attackPoint.position, attackDirection, attackRange, enemyLayer);
            
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    // 对敌人造成伤害
                    var bossController = hit.collider.GetComponent<Boss.BossController>();
                    if (bossController != null)
                    {
                        bossController.TakeDamage(attackDamage);
                        UnityEngine.Debug.Log($"对 {bossController.name} 造成 {attackDamage} 点伤害！");
                    }
                    break; // 只攻击第一个敌人
                }
            }
        }
        
        public void UseSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= skills.Length || skills[skillIndex] == null)
            {
                UnityEngine.Debug.Log("无效的技能索引！");
                return;
            }

            SkillData skill = skills[skillIndex];

            if (Time.time - skill.lastUsedTime < skill.cooldown)
            {
                UnityEngine.Debug.Log($"{skill.skillName} 冷却中... 剩余时间: {skill.cooldown - (Time.time - skill.lastUsedTime):F1}秒");
                return;
            }

            skill.lastUsedTime = Time.time;

            UnityEngine.Debug.Log($"使用技能: {skill.skillName}");
            
            switch (skill.skillType)
            {
                case SkillType.Projectile:
                    UseProjectileSkill(skill);
                    break;
                case SkillType.AOE:
                    UseAOESkill(skill);
                    break;
                case SkillType.Heal:
                    UseHealSkill(skill);
                    break;
            }
        }
        
        void UseProjectileSkill(SkillData skill)
        {
            UnityEngine.Debug.Log($"发射 {skill.skillName}！");
            
            // 创建投掷物
            if (skill.projectilePrefab != null && attackPoint != null)
            {
                GameObject projectile = Instantiate(skill.projectilePrefab, attackPoint.position, attackPoint.rotation);
                
                // 设置投掷物方向
                Vector3 direction = playerCamera.transform.forward;
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction * 15f; // 投掷速度
                }
                
                // 设置伤害
                var projectileScript = projectile.GetComponent<SkillProjectile>();
                if (projectileScript == null)
                {
                    projectileScript = projectile.AddComponent<SkillProjectile>();
                }
                projectileScript.damage = skill.damage;
                projectileScript.explosionRadius = skill.range;
                
                // 自动销毁
                Destroy(projectile, 5f);
            }
            else
            {
                // 如果没有投掷物预制体，使用射线攻击
                Vector3 attackDirection = playerCamera.transform.forward;
                RaycastHit hit;
                
                if (Physics.Raycast(attackPoint.position, attackDirection, out hit, skill.range, enemyLayer))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        var bossController = hit.collider.GetComponent<Boss.BossController>();
                        if (bossController != null)
                        {
                            bossController.TakeDamage(skill.damage);
                            UnityEngine.Debug.Log($"{skill.skillName} 命中 {bossController.name}，造成 {skill.damage} 点伤害！");
                        }
                    }
                }
            }
        }
        
        void UseAOESkill(SkillData skill)
        {
            UnityEngine.Debug.Log($"释放 {skill.skillName}！");
            
            // 创建AOE特效
            if (skill.effectPrefab != null)
            {
                GameObject effect = Instantiate(skill.effectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 检测范围内的所有敌人
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, skill.range, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    var bossController = hitCollider.GetComponent<Boss.BossController>();
                    if (bossController != null)
                    {
                        bossController.TakeDamage(skill.damage);
                        UnityEngine.Debug.Log($"{skill.skillName} 对 {bossController.name} 造成 {skill.damage} 点伤害！");
                    }
                }
            }
        }
        
        void UseHealSkill(SkillData skill)
        {
            UnityEngine.Debug.Log($"使用 {skill.skillName}！");
            
            // 创建治疗特效
            if (skill.effectPrefab != null)
            {
                GameObject effect = Instantiate(skill.effectPrefab, transform.position, Quaternion.identity);
                effect.transform.SetParent(transform);
                Destroy(effect, 2f);
            }
            
            // 治疗玩家
            var playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(skill.healAmount);
                UnityEngine.Debug.Log($"恢复了 {skill.healAmount} 点生命值！");
            }
        }
        
        public float GetSkillCooldownRemaining(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= skills.Length || skills[skillIndex] == null)
                return 0f;
            
            float remaining = skills[skillIndex].cooldown - (Time.time - skills[skillIndex].lastUsedTime);
            return Mathf.Max(0f, remaining);
        }
        
        void OnDrawGizmosSelected()
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            if (attackPoint != null)
            {
                Gizmos.DrawRay(attackPoint.position, attackPoint.forward * attackRange);
            }
            
            // 显示技能范围
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null && skills[i].skillType == SkillType.AOE)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, skills[i].range);
                }
            }
        }
    }
}
