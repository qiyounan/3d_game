using UnityEngine;
using UnityEngine.UI;
using BossBattle.Combat;

namespace BossBattle.UI
{
    /// <summary>
    /// 技能按钮 - 处理单个技能按钮的显示和交互
    /// </summary>
    public class SkillButton : MonoBehaviour
    {
        [Header("技能设置")]
        public int skillIndex = 0;
        public SkillData skillData;
        
        [Header("UI组件")]
        public Button button;
        public Image skillIcon;
        public Image cooldownOverlay;
        public Text cooldownText;
        public Text skillNameText;
        
        [Header("视觉效果")]
        public Color normalColor = Color.white;
        public Color disabledColor = Color.gray;
        public Color cooldownColor = new Color(1f, 1f, 1f, 0.5f);
        
        [Header("动画设置")]
        public bool useClickAnimation = true;
        public float clickAnimationScale = 0.9f;
        public float animationDuration = 0.1f;
        
        private CombatSystem combatSystem;
        private bool isOnCooldown = false;
        private Vector3 originalScale;
        
        // 事件
        public System.Action<int> OnSkillButtonClicked;
        
        void Start()
        {
            InitializeButton();
            SetupComponents();
        }
        
        void Update()
        {
            UpdateCooldownDisplay();
        }
        
        void InitializeButton()
        {
            // 查找CombatSystem
            if (combatSystem == null)
            {
                combatSystem = FindObjectOfType<CombatSystem>();
            }
            
            // 保存原始缩放
            originalScale = transform.localScale;
            
            // 设置按钮事件
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            
            // 从CombatSystem获取技能数据
            if (combatSystem != null && skillIndex < combatSystem.skills.Length)
            {
                skillData = combatSystem.skills[skillIndex];
            }
            
            // 更新UI显示
            UpdateSkillDisplay();
        }
        
        void SetupComponents()
        {
            // 自动查找组件
            if (button == null)
                button = GetComponent<Button>();
            
            if (skillIcon == null)
                skillIcon = transform.Find("Icon")?.GetComponent<Image>();
            
            if (cooldownOverlay == null)
                cooldownOverlay = transform.Find("CooldownOverlay")?.GetComponent<Image>();
            
            if (cooldownText == null)
                cooldownText = transform.Find("CooldownText")?.GetComponent<Text>();
            
            if (skillNameText == null)
                skillNameText = transform.Find("SkillName")?.GetComponent<Text>();
            
            // 初始化冷却遮罩
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
                cooldownOverlay.gameObject.SetActive(false);
            }
            
            // 初始化冷却文本
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
        
        void UpdateSkillDisplay()
        {
            if (skillData == null) return;
            
            // 更新技能图标
            if (skillIcon != null && skillData.skillIcon != null)
            {
                skillIcon.sprite = skillData.skillIcon;
            }
            
            // 更新技能名称
            if (skillNameText != null)
            {
                skillNameText.text = skillData.skillName;
            }
        }
        
        void UpdateCooldownDisplay()
        {
            if (skillData == null || combatSystem == null) return;
            
            float cooldownRemaining = combatSystem.GetSkillCooldownRemaining(skillIndex);
            bool wasOnCooldown = isOnCooldown;
            isOnCooldown = cooldownRemaining > 0;
            
            if (isOnCooldown)
            {
                // 显示冷却效果
                ShowCooldownEffect(cooldownRemaining);
                
                // 禁用按钮
                if (button != null)
                {
                    button.interactable = false;
                }
                
                // 改变颜色
                if (skillIcon != null)
                {
                    skillIcon.color = cooldownColor;
                }
            }
            else
            {
                // 隐藏冷却效果
                HideCooldownEffect();
                
                // 启用按钮
                if (button != null)
                {
                    button.interactable = true;
                }
                
                // 恢复颜色
                if (skillIcon != null)
                {
                    skillIcon.color = normalColor;
                }
                
                // 如果刚刚结束冷却，播放准备就绪动画
                if (wasOnCooldown && !isOnCooldown)
                {
                    PlayReadyAnimation();
                }
            }
        }
        
        void ShowCooldownEffect(float cooldownRemaining)
        {
            // 更新冷却遮罩
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(true);
                float cooldownProgress = 1f - (cooldownRemaining / skillData.cooldown);
                cooldownOverlay.fillAmount = 1f - cooldownProgress;
            }
            
            // 更新冷却文本
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = cooldownRemaining.ToString("F1");
            }
        }
        
        void HideCooldownEffect()
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(false);
            }
            
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
        
        void OnButtonClicked()
        {
            if (isOnCooldown) return;
            
            // 播放点击动画
            if (useClickAnimation)
            {
                PlayClickAnimation();
            }
            
            // 使用技能
            if (combatSystem != null)
            {
                combatSystem.UseSkill(skillIndex);
            }
            
            // 触发事件
            OnSkillButtonClicked?.Invoke(skillIndex);
            
            UnityEngine.Debug.Log($"使用技能: {skillData?.skillName ?? "未知技能"}");
        }
        
        void PlayClickAnimation()
        {
            StartCoroutine(ClickAnimationCoroutine());
        }
        
        void PlayReadyAnimation()
        {
            StartCoroutine(ReadyAnimationCoroutine());
        }
        
        private System.Collections.IEnumerator ClickAnimationCoroutine()
        {
            // 缩小
            float elapsed = 0f;
            Vector3 targetScale = originalScale * clickAnimationScale;
            
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // 恢复
            elapsed = 0f;
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        private System.Collections.IEnumerator ReadyAnimationCoroutine()
        {
            // 轻微放大表示准备就绪
            float elapsed = 0f;
            Vector3 targetScale = originalScale * 1.1f;
            float duration = 0.2f;
            
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        // 公共方法
        public void SetSkillData(SkillData newSkillData)
        {
            skillData = newSkillData;
            UpdateSkillDisplay();
        }
        
        public void SetSkillIndex(int index)
        {
            skillIndex = index;
            
            // 重新获取技能数据
            if (combatSystem != null && skillIndex < combatSystem.skills.Length)
            {
                skillData = combatSystem.skills[skillIndex];
                UpdateSkillDisplay();
            }
        }
        
        public bool IsOnCooldown()
        {
            return isOnCooldown;
        }
        
        public float GetCooldownRemaining()
        {
            if (combatSystem != null)
            {
                return combatSystem.GetSkillCooldownRemaining(skillIndex);
            }
            return 0f;
        }
    }
}
