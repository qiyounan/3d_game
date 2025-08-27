using UnityEngine;

namespace BossBattle.Player
{
    /// <summary>
    /// 玩家动画控制器 - 处理第一人称视角下的手部动画
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("动画设置")]
        public Animator handAnimator;
        public Transform handModel;
        
        [Header("武器设置")]
        public Transform weaponHolder;
        public GameObject[] weapons;
        public int currentWeaponIndex = 0;
        
        [Header("动画参数")]
        public string isWalkingParam = "IsWalking";
        public string isRunningParam = "IsRunning";
        public string attackTrigger = "Attack";
        public string skill1Trigger = "Skill1";
        public string skill2Trigger = "Skill2";
        public string skill3Trigger = "Skill3";
        
        private FirstPersonController playerController;
        private Combat.CombatSystem combatSystem;
        private bool isMoving = false;
        
        void Start()
        {
            playerController = GetComponent<FirstPersonController>();
            combatSystem = GetComponent<Combat.CombatSystem>();
            
            // 如果没有手部模型，创建一个简单的
            if (handModel == null)
            {
                SetupHandModel();
            }
            
            // 设置武器
            SetupWeapons();
        }
        
        void Update()
        {
            UpdateMovementAnimation();
        }
        
        void SetupHandModel()
        {
            // 创建手部模型容器
            GameObject handContainer = new GameObject("Hand Model");
            handContainer.transform.SetParent(Camera.main.transform);
            handContainer.transform.localPosition = new Vector3(0.3f, -0.3f, 0.5f);
            handContainer.transform.localRotation = Quaternion.identity;
            
            handModel = handContainer.transform;
            
            // 创建简单的手部表示（立方体）
            GameObject handCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handCube.transform.SetParent(handModel);
            handCube.transform.localPosition = Vector3.zero;
            handCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.3f);
            
            // 移除碰撞器
            Collider handCollider = handCube.GetComponent<Collider>();
            if (handCollider != null)
            {
                DestroyImmediate(handCollider);
            }
            
            // 添加动画器
            handAnimator = handContainer.AddComponent<Animator>();
            
            UnityEngine.Debug.Log("创建了简单的手部模型");
        }
        
        void SetupWeapons()
        {
            if (weaponHolder == null && handModel != null)
            {
                // 创建武器持有点
                GameObject weaponHolderObj = new GameObject("Weapon Holder");
                weaponHolderObj.transform.SetParent(handModel);
                weaponHolderObj.transform.localPosition = new Vector3(0, 0, 0.2f);
                weaponHolder = weaponHolderObj.transform;
            }
            
            // 激活当前武器
            SwitchWeapon(currentWeaponIndex);
        }
        
        void UpdateMovementAnimation()
        {
            if (playerController == null || handAnimator == null) return;
            
            // 检测移动状态
            bool wasMoving = isMoving;
            isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
            
            // 更新动画参数
            if (handAnimator != null)
            {
                handAnimator.SetBool(isWalkingParam, isMoving);
                
                // 可以根据移动速度设置跑步动画
                bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
                handAnimator.SetBool(isRunningParam, isRunning);
            }
            
            // 简单的手部摆动动画（如果没有Animator）
            if (handAnimator == null && handModel != null)
            {
                SimpleHandBob();
            }
        }
        
        void SimpleHandBob()
        {
            if (!isMoving) return;
            
            // 简单的上下摆动
            float bobAmount = 0.02f;
            float bobSpeed = 10f;
            
            Vector3 originalPos = new Vector3(0.3f, -0.3f, 0.5f);
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            
            handModel.localPosition = originalPos + new Vector3(0, bobOffset, 0);
        }
        
        public void PlayAttackAnimation()
        {
            if (handAnimator != null)
            {
                handAnimator.SetTrigger(attackTrigger);
            }
            else
            {
                // 简单的攻击动画
                StartCoroutine(SimpleAttackAnimation());
            }
        }
        
        public void PlaySkillAnimation(int skillIndex)
        {
            if (handAnimator != null)
            {
                string triggerName = "";
                switch (skillIndex)
                {
                    case 0:
                        triggerName = skill1Trigger;
                        break;
                    case 1:
                        triggerName = skill2Trigger;
                        break;
                    case 2:
                        triggerName = skill3Trigger;
                        break;
                }
                
                if (!string.IsNullOrEmpty(triggerName))
                {
                    handAnimator.SetTrigger(triggerName);
                }
            }
            else
            {
                // 简单的技能动画
                StartCoroutine(SimpleSkillAnimation());
            }
        }
        
        public void SwitchWeapon(int weaponIndex)
        {
            if (weapons == null || weapons.Length == 0) return;
            
            // 隐藏所有武器
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].SetActive(false);
                }
            }
            
            // 显示当前武器
            if (weaponIndex >= 0 && weaponIndex < weapons.Length && weapons[weaponIndex] != null)
            {
                weapons[weaponIndex].SetActive(true);
                currentWeaponIndex = weaponIndex;
                
                // 确保武器在正确的位置
                if (weaponHolder != null)
                {
                    weapons[weaponIndex].transform.SetParent(weaponHolder);
                    weapons[weaponIndex].transform.localPosition = Vector3.zero;
                    weapons[weaponIndex].transform.localRotation = Quaternion.identity;
                }
            }
        }
        
        private System.Collections.IEnumerator SimpleAttackAnimation()
        {
            if (handModel == null) yield break;
            
            Vector3 originalPos = handModel.localPosition;
            Vector3 attackPos = originalPos + new Vector3(0, 0, 0.2f);
            
            // 向前推
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                handModel.localPosition = Vector3.Lerp(originalPos, attackPos, t);
                yield return null;
            }
            
            // 回到原位
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                handModel.localPosition = Vector3.Lerp(attackPos, originalPos, t);
                yield return null;
            }
            
            handModel.localPosition = originalPos;
        }
        
        private System.Collections.IEnumerator SimpleSkillAnimation()
        {
            if (handModel == null) yield break;
            
            Vector3 originalPos = handModel.localPosition;
            Vector3 skillPos = originalPos + new Vector3(0, 0.1f, 0);
            
            // 向上举
            float duration = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                handModel.localPosition = Vector3.Lerp(originalPos, skillPos, t);
                yield return null;
            }
            
            // 回到原位
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                handModel.localPosition = Vector3.Lerp(skillPos, originalPos, t);
                yield return null;
            }
            
            handModel.localPosition = originalPos;
        }
    }
}
