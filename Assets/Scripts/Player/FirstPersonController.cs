using UnityEngine;

namespace BossBattle.Player
{
    /// <summary>
    /// 第一人称控制器 - 处理玩家移动和视角控制
    /// </summary>
    public class FirstPersonController : MonoBehaviour
    {
        [Header("移动设置")]
        public float moveSpeed = 5f;
        public float jumpForce = 8f;
        public float gravity = -9.81f;
        
        [Header("视角控制")]
        public float mouseSensitivity = 100f;
        public Transform playerBody;
        public Camera playerCamera;
        
        [Header("地面检测")]
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float xRotation = 0f;
        
        // 移动输入
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jumpInput;
        
        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // 锁定光标到屏幕中心（PC端）
            if (Application.platform != RuntimePlatform.Android && 
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        
        void Update()
        {
            HandleInput();
            HandleMovement();
            HandleMouseLook();
        }
        
        void HandleInput()
        {
            // PC端输入
            if (Application.platform != RuntimePlatform.Android && 
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                moveInput.x = Input.GetAxis("Horizontal");
                moveInput.y = Input.GetAxis("Vertical");
                
                lookInput.x = Input.GetAxis("Mouse X");
                lookInput.y = Input.GetAxis("Mouse Y");
                
                jumpInput = Input.GetButtonDown("Jump");
            }
            // 移动端输入将通过UI控制器设置
        }
        
        void HandleMovement()
        {
            // 地面检测
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            // 移动
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            controller.Move(move * moveSpeed * Time.deltaTime);
            
            // 跳跃
            if (jumpInput && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
            
            // 重力
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        void HandleMouseLook()
        {
            // 水平旋转（Y轴）
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            playerBody.Rotate(Vector3.up * mouseX);
            
            // 垂直旋转（X轴）
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        // 供移动端UI调用的方法
        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }
        
        public void SetLookInput(Vector2 input)
        {
            lookInput = input;
        }
        
        public void SetJumpInput(bool input)
        {
            jumpInput = input;
        }
    }
}
