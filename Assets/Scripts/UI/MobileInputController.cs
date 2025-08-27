using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BossBattle.Player;
using BossBattle.Combat;

namespace BossBattle.UI
{
    /// <summary>
    /// 移动端输入控制器 - 处理虚拟摇杆和按钮输入
    /// </summary>
    public class MobileInputController : MonoBehaviour
    {
        [Header("摇杆设置")]
        public RectTransform joystickBackground;
        public RectTransform joystickHandle;
        public float joystickRange = 50f;

        [Header("按钮")]
        public Button attackButton;
        public Button jumpButton;
        public Button[] skillButtons = new Button[3];

        [Header("技能冷却显示")]
        public Image[] skillCooldownImages = new Image[3];
        public Text[] skillCooldownTexts = new Text[3];

        [Header("玩家控制器")]
        public FirstPersonController playerController;
        public CombatSystem combatSystem;
        
        private Vector2 joystickInput;
        private bool isDragging = false;
        private Vector2 joystickCenter;
        
        void Start()
        {
            // 设置摇杆中心位置
            joystickCenter = joystickBackground.anchoredPosition;

            // 自动查找组件
            if (playerController == null)
                playerController = FindObjectOfType<FirstPersonController>();
            if (combatSystem == null)
                combatSystem = FindObjectOfType<CombatSystem>();

            // 设置按钮事件
            SetupButtons();

            // 只在移动端显示UI
            bool isMobile = Application.platform == RuntimePlatform.Android ||
                           Application.platform == RuntimePlatform.IPhonePlayer;
            gameObject.SetActive(isMobile);
        }
        
        void Update()
        {
            HandleJoystick();

            // 将输入传递给玩家控制器
            if (playerController != null)
            {
                playerController.SetMoveInput(joystickInput);
            }

            // 更新技能冷却显示
            UpdateSkillCooldowns();
        }
        
        void SetupButtons()
        {
            // 攻击按钮
            if (attackButton != null)
            {
                attackButton.onClick.AddListener(OnAttackButtonPressed);
            }
            
            // 跳跃按钮
            if (jumpButton != null)
            {
                var jumpEventTrigger = jumpButton.gameObject.GetComponent<EventTrigger>();
                if (jumpEventTrigger == null)
                    jumpEventTrigger = jumpButton.gameObject.AddComponent<EventTrigger>();
                
                var pointerDown = new EventTrigger.Entry();
                pointerDown.eventID = EventTriggerType.PointerDown;
                pointerDown.callback.AddListener((data) => { OnJumpButtonPressed(); });
                jumpEventTrigger.triggers.Add(pointerDown);
                
                var pointerUp = new EventTrigger.Entry();
                pointerUp.eventID = EventTriggerType.PointerUp;
                pointerUp.callback.AddListener((data) => { OnJumpButtonReleased(); });
                jumpEventTrigger.triggers.Add(pointerUp);
            }
            
            // 技能按钮
            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (skillButtons[i] != null)
                {
                    int skillIndex = i; // 闭包变量
                    skillButtons[i].onClick.AddListener(() => OnSkillButtonPressed(skillIndex));
                }
            }
        }
        
        void HandleJoystick()
        {
            // 检测触摸输入
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPosition = touch.position;
                
                // 转换屏幕坐标到UI坐标
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickBackground.parent as RectTransform,
                    touchPosition,
                    null,
                    out Vector2 localPoint
                );
                
                // 检查是否在摇杆区域内
                float distance = Vector2.Distance(localPoint, joystickCenter);
                
                if (touch.phase == TouchPhase.Began && distance <= joystickRange * 2)
                {
                    isDragging = true;
                }
                
                if (isDragging)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        // 计算摇杆输入
                        Vector2 direction = localPoint - joystickCenter;
                        float magnitude = Mathf.Clamp(direction.magnitude, 0, joystickRange);
                        
                        joystickInput = direction.normalized * (magnitude / joystickRange);
                        
                        // 更新摇杆手柄位置
                        joystickHandle.anchoredPosition = joystickCenter + joystickInput * joystickRange;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        isDragging = false;
                        joystickInput = Vector2.zero;
                        joystickHandle.anchoredPosition = joystickCenter;
                    }
                }
            }
            else if (!isDragging)
            {
                // 没有触摸时重置摇杆
                joystickInput = Vector2.zero;
                joystickHandle.anchoredPosition = joystickCenter;
            }
        }
        
        void OnAttackButtonPressed()
        {
            UnityEngine.Debug.Log("攻击按钮被按下");
            if (combatSystem != null)
            {
                combatSystem.PerformAttack();
            }
        }
        
        void OnJumpButtonPressed()
        {
            if (playerController != null)
            {
                playerController.SetJumpInput(true);
            }
        }
        
        void OnJumpButtonReleased()
        {
            if (playerController != null)
            {
                playerController.SetJumpInput(false);
            }
        }
        
        void OnSkillButtonPressed(int skillIndex)
        {
            UnityEngine.Debug.Log($"技能 {skillIndex + 1} 被使用");
            if (combatSystem != null)
            {
                combatSystem.UseSkill(skillIndex);
            }
        }

        void UpdateSkillCooldowns()
        {
            if (combatSystem == null) return;

            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (i < skillCooldownImages.Length && skillCooldownImages[i] != null)
                {
                    float cooldownRemaining = combatSystem.GetSkillCooldownRemaining(i);

                    if (cooldownRemaining > 0)
                    {
                        // 显示冷却遮罩
                        skillCooldownImages[i].fillAmount = cooldownRemaining / (combatSystem.skills[i]?.cooldown ?? 1f);
                        skillCooldownImages[i].gameObject.SetActive(true);

                        // 显示冷却时间文本
                        if (i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
                        {
                            skillCooldownTexts[i].text = cooldownRemaining.ToString("F1");
                            skillCooldownTexts[i].gameObject.SetActive(true);
                        }

                        // 禁用按钮
                        if (skillButtons[i] != null)
                        {
                            skillButtons[i].interactable = false;
                        }
                    }
                    else
                    {
                        // 隐藏冷却效果
                        skillCooldownImages[i].gameObject.SetActive(false);

                        if (i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
                        {
                            skillCooldownTexts[i].gameObject.SetActive(false);
                        }

                        // 启用按钮
                        if (skillButtons[i] != null)
                        {
                            skillButtons[i].interactable = true;
                        }
                    }
                }
            }
        }
    }
}
