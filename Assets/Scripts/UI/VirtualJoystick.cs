using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BossBattle.UI
{
    /// <summary>
    /// 虚拟摇杆 - 处理移动端摇杆输入
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("摇杆组件")]
        public RectTransform joystickBackground;
        public RectTransform joystickHandle;
        
        [Header("摇杆设置")]
        public float joystickRange = 50f;
        public bool dynamicJoystick = false;
        public float returnSpeed = 5f;
        
        [Header("输入设置")]
        public bool invertX = false;
        public bool invertY = false;
        public float deadZone = 0.1f;
        
        private Vector2 joystickCenter;
        private Vector2 inputVector;
        private bool isDragging = false;
        private Canvas parentCanvas;
        private Camera uiCamera;
        
        // 事件
        public System.Action<Vector2> OnJoystickInput;
        
        void Start()
        {
            // 获取父Canvas
            parentCanvas = GetComponentInParent<Canvas>();
            
            // 获取UI摄像机
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                uiCamera = parentCanvas.worldCamera;
            }
            
            // 设置摇杆中心
            if (joystickBackground != null)
            {
                joystickCenter = joystickBackground.anchoredPosition;
            }
            
            // 初始化摇杆位置
            ResetJoystick();
        }
        
        void Update()
        {
            // 如果不在拖拽状态，让摇杆回到中心
            if (!isDragging)
            {
                ReturnToCenter();
            }
            
            // 处理输入
            ProcessInput();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            
            // 动态摇杆：将摇杆移动到触摸位置
            if (dynamicJoystick)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    transform as RectTransform,
                    eventData.position,
                    uiCamera,
                    out localPoint
                );
                
                joystickCenter = localPoint;
                joystickBackground.anchoredPosition = joystickCenter;
            }
            
            OnDrag(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            
            // 动态摇杆：隐藏摇杆或重置位置
            if (dynamicJoystick)
            {
                joystickBackground.gameObject.SetActive(false);
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // 显示摇杆（用于动态摇杆）
            if (dynamicJoystick && !joystickBackground.gameObject.activeInHierarchy)
            {
                joystickBackground.gameObject.SetActive(true);
            }
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                uiCamera,
                out localPoint
            );
            
            // 限制在摇杆范围内
            Vector2 direction = localPoint;
            float distance = Mathf.Clamp(direction.magnitude, 0, joystickRange);
            
            // 计算输入向量
            inputVector = direction.normalized * (distance / joystickRange);
            
            // 应用死区
            if (inputVector.magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }
            
            // 应用反转设置
            if (invertX) inputVector.x = -inputVector.x;
            if (invertY) inputVector.y = -inputVector.y;
            
            // 更新摇杆手柄位置
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = inputVector * joystickRange;
            }
        }
        
        void ReturnToCenter()
        {
            if (joystickHandle != null)
            {
                // 平滑回到中心
                joystickHandle.anchoredPosition = Vector2.Lerp(
                    joystickHandle.anchoredPosition,
                    Vector2.zero,
                    returnSpeed * Time.deltaTime
                );
                
                // 如果足够接近中心，直接设置为零
                if (joystickHandle.anchoredPosition.magnitude < 0.1f)
                {
                    joystickHandle.anchoredPosition = Vector2.zero;
                    inputVector = Vector2.zero;
                }
            }
        }
        
        void ProcessInput()
        {
            // 触发输入事件
            OnJoystickInput?.Invoke(inputVector);
        }
        
        void ResetJoystick()
        {
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
            
            inputVector = Vector2.zero;
            isDragging = false;
            
            // 动态摇杆初始隐藏
            if (dynamicJoystick && joystickBackground != null)
            {
                joystickBackground.gameObject.SetActive(false);
            }
        }
        
        // 公共方法
        public Vector2 GetInputVector()
        {
            return inputVector;
        }
        
        public float GetHorizontalInput()
        {
            return inputVector.x;
        }
        
        public float GetVerticalInput()
        {
            return inputVector.y;
        }
        
        public bool IsActive()
        {
            return isDragging || inputVector.magnitude > 0.01f;
        }
        
        // 设置方法
        public void SetJoystickRange(float range)
        {
            joystickRange = range;
        }
        
        public void SetDeadZone(float deadZone)
        {
            this.deadZone = Mathf.Clamp01(deadZone);
        }
        
        public void SetInvertX(bool invert)
        {
            invertX = invert;
        }
        
        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }
        
        public void SetDynamicMode(bool dynamic)
        {
            dynamicJoystick = dynamic;
            
            if (dynamic)
            {
                joystickBackground.gameObject.SetActive(false);
            }
            else
            {
                joystickBackground.gameObject.SetActive(true);
                joystickBackground.anchoredPosition = joystickCenter;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // 在Scene视图中显示摇杆范围
            if (joystickBackground != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 worldPos = joystickBackground.position;
                Gizmos.DrawWireSphere(worldPos, joystickRange);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(worldPos, joystickRange * deadZone);
            }
        }
    }
}
