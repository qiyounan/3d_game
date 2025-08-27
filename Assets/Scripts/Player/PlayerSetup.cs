using UnityEngine;
using BossBattle.Combat;

namespace BossBattle.Player
{
    /// <summary>
    /// 玩家设置脚本 - 自动配置玩家组件
    /// </summary>
    public class PlayerSetup : MonoBehaviour
    {
        [Header("玩家设置")]
        public bool autoSetup = true;
        
        [Header("组件引用")]
        public Camera playerCamera;
        public Transform groundCheck;
        public Transform attackPoint;
        
        [Header("地面检测设置")]
        public float groundCheckRadius = 0.4f;
        public LayerMask groundMask = 1;
        
        void Start()
        {
            if (autoSetup)
            {
                SetupPlayer();
            }
        }
        
        void SetupPlayer()
        {
            // 设置玩家标签
            if (!gameObject.CompareTag("Player"))
            {
                gameObject.tag = "Player";
            }
            
            // 添加CharacterController
            CharacterController controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<CharacterController>();
                controller.height = 2f;
                controller.radius = 0.5f;
                controller.center = new Vector3(0, 1f, 0);
            }
            
            // 设置摄像机
            SetupCamera();
            
            // 设置地面检测
            SetupGroundCheck();
            
            // 设置攻击点
            SetupAttackPoint();
            
            // 添加必要组件
            SetupComponents();
            
            UnityEngine.Debug.Log("玩家设置完成！");
        }
        
        void SetupCamera()
        {
            if (playerCamera == null)
            {
                // 查找子对象中的摄像机
                playerCamera = GetComponentInChildren<Camera>();
                
                if (playerCamera == null)
                {
                    // 创建摄像机
                    GameObject cameraObj = new GameObject("Player Camera");
                    cameraObj.transform.SetParent(transform);
                    cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                    
                    playerCamera = cameraObj.AddComponent<Camera>();
                    cameraObj.AddComponent<AudioListener>();
                    
                    // 设置为主摄像机
                    playerCamera.tag = "MainCamera";
                }
            }
            
            // 配置摄像机设置
            playerCamera.fieldOfView = 60f;
            playerCamera.nearClipPlane = 0.1f;
            playerCamera.farClipPlane = 1000f;
        }
        
        void SetupGroundCheck()
        {
            if (groundCheck == null)
            {
                // 创建地面检测点
                GameObject groundCheckObj = new GameObject("Ground Check");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, 0.1f, 0);
                groundCheck = groundCheckObj.transform;
            }
        }
        
        void SetupAttackPoint()
        {
            if (attackPoint == null)
            {
                // 创建攻击点
                GameObject attackPointObj = new GameObject("Attack Point");
                attackPointObj.transform.SetParent(playerCamera.transform);
                attackPointObj.transform.localPosition = new Vector3(0, -0.2f, 1f);
                attackPoint = attackPointObj.transform;
            }
        }
        
        void SetupComponents()
        {
            // 添加FirstPersonController
            FirstPersonController fpController = GetComponent<FirstPersonController>();
            if (fpController == null)
            {
                fpController = gameObject.AddComponent<FirstPersonController>();
            }
            
            // 设置FirstPersonController的引用
            fpController.playerBody = transform;
            fpController.playerCamera = playerCamera;
            fpController.groundCheck = groundCheck;
            fpController.groundDistance = groundCheckRadius;
            fpController.groundMask = groundMask;
            
            // 添加PlayerHealth
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = gameObject.AddComponent<PlayerHealth>();
            }
            
            // 添加CombatSystem
            CombatSystem combatSystem = GetComponent<CombatSystem>();
            if (combatSystem == null)
            {
                combatSystem = gameObject.AddComponent<CombatSystem>();
            }
            
            // 设置CombatSystem的引用
            combatSystem.attackPoint = attackPoint;
        }
        
        void OnDrawGizmosSelected()
        {
            // 显示地面检测范围
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
            
            // 显示攻击点
            if (attackPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(attackPoint.position, 0.2f);
                
                // 显示攻击方向
                Gizmos.DrawRay(attackPoint.position, attackPoint.forward * 3f);
            }
        }
        
        // 编辑器中的设置按钮
        [ContextMenu("Setup Player")]
        void SetupPlayerFromMenu()
        {
            SetupPlayer();
        }
        
        [ContextMenu("Reset Player")]
        void ResetPlayer()
        {
            // 移除自动添加的组件
            FirstPersonController fpController = GetComponent<FirstPersonController>();
            if (fpController != null)
            {
                DestroyImmediate(fpController);
            }
            
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                DestroyImmediate(playerHealth);
            }
            
            CombatSystem combatSystem = GetComponent<CombatSystem>();
            if (combatSystem != null)
            {
                DestroyImmediate(combatSystem);
            }
            
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                DestroyImmediate(controller);
            }
            
            UnityEngine.Debug.Log("玩家组件已重置！");
        }
    }
}
