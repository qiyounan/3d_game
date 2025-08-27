using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BossBattle.Combat
{
    /// <summary>
    /// 状态效果管理器 - 管理角色身上的各种状态效果
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("状态效果显示")]
        public Transform statusIconParent;
        public GameObject statusIconPrefab;
        
        private Dictionary<DamageSystem.StatusType, ActiveStatusEffect> activeEffects = 
            new Dictionary<DamageSystem.StatusType, ActiveStatusEffect>();
        
        private class ActiveStatusEffect
        {
            public DamageSystem.StatusEffect effect;
            public float remainingTime;
            public float nextTickTime;
            public GameObject iconObject;
            public Coroutine tickCoroutine;
        }
        
        void Start()
        {
            // 如果没有状态图标父对象，创建一个
            if (statusIconParent == null)
            {
                CreateStatusIconParent();
            }
        }
        
        void CreateStatusIconParent()
        {
            GameObject iconParentObj = new GameObject("Status Icons");
            iconParentObj.transform.SetParent(transform);
            iconParentObj.transform.localPosition = Vector3.up * 2.5f;
            
            // 添加Canvas组件
            Canvas canvas = iconParentObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.localScale = Vector3.one * 0.01f;
            
            statusIconParent = iconParentObj.transform;
        }
        
        public void ApplyStatusEffect(DamageSystem.StatusEffect effect)
        {
            if (activeEffects.ContainsKey(effect.type))
            {
                // 刷新现有效果
                RefreshStatusEffect(effect);
            }
            else
            {
                // 添加新效果
                AddNewStatusEffect(effect);
            }
        }
        
        void AddNewStatusEffect(DamageSystem.StatusEffect effect)
        {
            ActiveStatusEffect activeEffect = new ActiveStatusEffect
            {
                effect = effect,
                remainingTime = effect.duration,
                nextTickTime = Time.time + effect.tickInterval
            };
            
            activeEffects[effect.type] = activeEffect;
            
            // 创建状态图标
            CreateStatusIcon(activeEffect);
            
            // 开始效果
            StartStatusEffect(activeEffect);
            
            UnityEngine.Debug.Log($"应用状态效果: {effect.type} (持续时间: {effect.duration}秒)");
        }
        
        void RefreshStatusEffect(DamageSystem.StatusEffect effect)
        {
            if (activeEffects.ContainsKey(effect.type))
            {
                ActiveStatusEffect activeEffect = activeEffects[effect.type];
                activeEffect.remainingTime = effect.duration;
                activeEffect.nextTickTime = Time.time + effect.tickInterval;
                
                UnityEngine.Debug.Log($"刷新状态效果: {effect.type}");
            }
        }
        
        void CreateStatusIcon(ActiveStatusEffect activeEffect)
        {
            if (statusIconParent == null || statusIconPrefab == null) return;
            
            GameObject iconObj = Instantiate(statusIconPrefab, statusIconParent);
            activeEffect.iconObject = iconObj;
            
            // 设置图标
            UnityEngine.UI.Image iconImage = iconObj.GetComponent<UnityEngine.UI.Image>();
            if (iconImage != null)
            {
                iconImage.color = GetStatusEffectColor(activeEffect.effect.type);
            }
            
            // 设置位置
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int iconCount = activeEffects.Count;
                rectTransform.anchoredPosition = new Vector2((iconCount - 1) * 60, 0);
            }
        }
        
        Color GetStatusEffectColor(DamageSystem.StatusType type)
        {
            switch (type)
            {
                case DamageSystem.StatusType.Burn:
                    return Color.red;
                case DamageSystem.StatusType.Freeze:
                    return Color.cyan;
                case DamageSystem.StatusType.Stun:
                    return Color.yellow;
                case DamageSystem.StatusType.Poison:
                    return Color.green;
                case DamageSystem.StatusType.Slow:
                    return Color.blue;
                case DamageSystem.StatusType.Regeneration:
                    return Color.green;
                default:
                    return Color.white;
            }
        }
        
        void StartStatusEffect(ActiveStatusEffect activeEffect)
        {
            // 立即应用效果
            ApplyStatusEffectImmediate(activeEffect);
            
            // 开始持续效果
            if (activeEffect.effect.tickInterval > 0)
            {
                activeEffect.tickCoroutine = StartCoroutine(StatusEffectTick(activeEffect));
            }
            
            // 开始倒计时
            StartCoroutine(StatusEffectCountdown(activeEffect));
        }
        
        void ApplyStatusEffectImmediate(ActiveStatusEffect activeEffect)
        {
            switch (activeEffect.effect.type)
            {
                case DamageSystem.StatusType.Freeze:
                    ApplyFreeze(activeEffect);
                    break;
                case DamageSystem.StatusType.Stun:
                    ApplyStun(activeEffect);
                    break;
                case DamageSystem.StatusType.Slow:
                    ApplySlow(activeEffect);
                    break;
            }
        }
        
        IEnumerator StatusEffectTick(ActiveStatusEffect activeEffect)
        {
            while (activeEffect.remainingTime > 0)
            {
                yield return new WaitForSeconds(activeEffect.effect.tickInterval);
                
                if (activeEffect.remainingTime <= 0) break;
                
                ApplyStatusEffectTick(activeEffect);
            }
        }
        
        void ApplyStatusEffectTick(ActiveStatusEffect activeEffect)
        {
            switch (activeEffect.effect.type)
            {
                case DamageSystem.StatusType.Burn:
                    ApplyBurnTick(activeEffect);
                    break;
                case DamageSystem.StatusType.Poison:
                    ApplyPoisonTick(activeEffect);
                    break;
                case DamageSystem.StatusType.Regeneration:
                    ApplyRegenerationTick(activeEffect);
                    break;
            }
        }
        
        IEnumerator StatusEffectCountdown(ActiveStatusEffect activeEffect)
        {
            while (activeEffect.remainingTime > 0)
            {
                yield return new WaitForSeconds(0.1f);
                activeEffect.remainingTime -= 0.1f;
                
                // 更新图标显示
                UpdateStatusIcon(activeEffect);
            }
            
            // 效果结束
            RemoveStatusEffect(activeEffect.effect.type);
        }
        
        void UpdateStatusIcon(ActiveStatusEffect activeEffect)
        {
            if (activeEffect.iconObject != null)
            {
                UnityEngine.UI.Image iconImage = activeEffect.iconObject.GetComponent<UnityEngine.UI.Image>();
                if (iconImage != null)
                {
                    float alpha = Mathf.Clamp01(activeEffect.remainingTime / activeEffect.effect.duration);
                    Color color = iconImage.color;
                    color.a = alpha;
                    iconImage.color = color;
                }
            }
        }
        
        public void RemoveStatusEffect(DamageSystem.StatusType type)
        {
            if (activeEffects.ContainsKey(type))
            {
                ActiveStatusEffect activeEffect = activeEffects[type];
                
                // 停止协程
                if (activeEffect.tickCoroutine != null)
                {
                    StopCoroutine(activeEffect.tickCoroutine);
                }
                
                // 移除效果
                RemoveStatusEffectImmediate(activeEffect);
                
                // 销毁图标
                if (activeEffect.iconObject != null)
                {
                    Destroy(activeEffect.iconObject);
                }
                
                activeEffects.Remove(type);
                
                UnityEngine.Debug.Log($"移除状态效果: {type}");
            }
        }
        
        void RemoveStatusEffectImmediate(ActiveStatusEffect activeEffect)
        {
            switch (activeEffect.effect.type)
            {
                case DamageSystem.StatusType.Freeze:
                    RemoveFreeze();
                    break;
                case DamageSystem.StatusType.Stun:
                    RemoveStun();
                    break;
                case DamageSystem.StatusType.Slow:
                    RemoveSlow();
                    break;
            }
        }
        
        // 具体状态效果实现
        void ApplyFreeze(ActiveStatusEffect activeEffect)
        {
            // 冻结效果 - 禁用移动
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
        
        void RemoveFreeze()
        {
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
        
        void ApplyStun(ActiveStatusEffect activeEffect)
        {
            // 眩晕效果 - 禁用所有控制
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            CombatSystem combatSystem = GetComponent<CombatSystem>();
            
            if (playerController != null) playerController.enabled = false;
            if (combatSystem != null) combatSystem.enabled = false;
        }
        
        void RemoveStun()
        {
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            CombatSystem combatSystem = GetComponent<CombatSystem>();
            
            if (playerController != null) playerController.enabled = true;
            if (combatSystem != null) combatSystem.enabled = true;
        }
        
        void ApplySlow(ActiveStatusEffect activeEffect)
        {
            // 减速效果 - 降低移动速度
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            if (playerController != null)
            {
                playerController.moveSpeed *= 0.5f;
            }
        }
        
        void RemoveSlow()
        {
            Player.FirstPersonController playerController = GetComponent<Player.FirstPersonController>();
            if (playerController != null)
            {
                playerController.moveSpeed *= 2f; // 恢复原速度
            }
        }
        
        void ApplyBurnTick(ActiveStatusEffect activeEffect)
        {
            // 燃烧伤害
            Player.PlayerHealth playerHealth = GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(activeEffect.effect.value);
            }
        }
        
        void ApplyPoisonTick(ActiveStatusEffect activeEffect)
        {
            // 中毒伤害
            Player.PlayerHealth playerHealth = GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(activeEffect.effect.value);
            }
        }
        
        void ApplyRegenerationTick(ActiveStatusEffect activeEffect)
        {
            // 生命恢复
            Player.PlayerHealth playerHealth = GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(activeEffect.effect.value);
            }
        }
        
        public bool HasStatusEffect(DamageSystem.StatusType type)
        {
            return activeEffects.ContainsKey(type);
        }
        
        public float GetStatusEffectRemainingTime(DamageSystem.StatusType type)
        {
            if (activeEffects.ContainsKey(type))
            {
                return activeEffects[type].remainingTime;
            }
            return 0f;
        }
        
        void OnDestroy()
        {
            // 清理所有状态效果
            foreach (var effect in activeEffects.Values)
            {
                if (effect.tickCoroutine != null)
                {
                    StopCoroutine(effect.tickCoroutine);
                }
            }
            activeEffects.Clear();
        }
    }
}
