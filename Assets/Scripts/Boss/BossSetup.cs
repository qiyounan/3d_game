using UnityEngine;
using UnityEngine.UI;

namespace BossBattle.Boss
{
    /// <summary>
    /// Boss设置脚本 - 自动配置Boss组件和UI
    /// </summary>
    public class BossSetup : MonoBehaviour
    {
        [Header("Boss设置")]
        public bool autoSetup = true;
        public BossType bossType = BossType.QiuQiu;
        
        [Header("3D模型设置")]
        public GameObject bossModel;
        public Material bossMaterial;
        public Vector3 modelScale = Vector3.one;
        
        [Header("UI设置")]
        public Vector3 nameUIOffset = new Vector3(0, 3f, 0);
        public Vector2 nameUISize = new Vector2(200f, 50f);
        public Font nameFont;
        
        public enum BossType
        {
            QiuQiu,
            GuoGuo
        }
        
        void Start()
        {
            if (autoSetup)
            {
                SetupBoss();
            }
        }
        
        void SetupBoss()
        {
            // 设置Boss标签
            if (!gameObject.CompareTag("Enemy"))
            {
                gameObject.tag = "Enemy";
            }
            
            // 创建3D模型
            SetupBossModel();
            
            // 设置物理组件
            SetupPhysics();
            
            // 设置名称UI
            SetupNameUI();
            
            // 添加对应的Boss脚本
            SetupBossScript();
            
            UnityEngine.Debug.Log($"{bossType} Boss设置完成！");
        }
        
        void SetupBossModel()
        {
            GameObject model = null;
            
            if (bossModel != null)
            {
                // 使用提供的模型
                model = Instantiate(bossModel, transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = modelScale;
            }
            else
            {
                // 创建简单的几何体模型
                switch (bossType)
                {
                    case BossType.QiuQiu:
                        model = CreateQiuQiuModel();
                        break;
                    case BossType.GuoGuo:
                        model = CreateGuoGuoModel();
                        break;
                }
            }
            
            if (model != null)
            {
                // 应用材质
                if (bossMaterial != null)
                {
                    Renderer renderer = model.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = bossMaterial;
                    }
                }
                
                // 移除碰撞器（Boss本体使用CharacterController或Rigidbody）
                Collider modelCollider = model.GetComponent<Collider>();
                if (modelCollider != null)
                {
                    DestroyImmediate(modelCollider);
                }
            }
        }
        
        GameObject CreateQiuQiuModel()
        {
            // 创建球形模型
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "QiuQiu Model";
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = new Vector3(2f, 2f, 2f);
            
            // 设置材质颜色为红色
            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = Color.red;
                material.SetFloat("_Metallic", 0.3f);
                material.SetFloat("_Glossiness", 0.8f);
                renderer.material = material;
            }
            
            return sphere;
        }
        
        GameObject CreateGuoGuoModel()
        {
            // 创建胶囊形模型（像果实）
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "GuoGuo Model";
            capsule.transform.SetParent(transform);
            capsule.transform.localPosition = Vector3.zero;
            capsule.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            
            // 设置材质颜色为绿色
            Renderer renderer = capsule.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = Color.green;
                material.SetFloat("_Metallic", 0.1f);
                material.SetFloat("_Glossiness", 0.6f);
                renderer.material = material;
            }
            
            return capsule;
        }
        
        void SetupPhysics()
        {
            // 添加Rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            // 设置Rigidbody属性
            rb.mass = 5f;
            rb.drag = 2f;
            rb.angularDrag = 5f;
            rb.freezeRotation = true; // 防止Boss翻倒
            
            // 添加碰撞器
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CapsuleCollider>();
            }
            
            // 设置碰撞器属性
            collider.height = 2f;
            collider.radius = 1f;
            collider.center = new Vector3(0, 1f, 0);
        }
        
        void SetupNameUI()
        {
            // 创建Canvas
            GameObject canvasObj = new GameObject("Name Canvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = nameUIOffset;
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // 设置Canvas大小
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = nameUISize;
            canvasRect.localScale = Vector3.one * 0.01f; // 缩小以适应世界空间
            
            // 添加CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            
            // 添加GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // 创建背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvasObj.transform);
            
            Image background = backgroundObj.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f);
            
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 创建名称文本
            GameObject textObj = new GameObject("Name Text");
            textObj.transform.SetParent(canvasObj.transform);
            
            Text nameText = textObj.AddComponent<Text>();
            nameText.text = GetBossName();
            nameText.font = nameFont != null ? nameFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.fontSize = 24;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = nameText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // 创建血条
            CreateHealthBar(canvasObj);
        }
        
        void CreateHealthBar(GameObject parentCanvas)
        {
            // 创建血条背景
            GameObject healthBarBg = new GameObject("Health Bar Background");
            healthBarBg.transform.SetParent(parentCanvas.transform);
            
            Image bgImage = healthBarBg.AddComponent<Image>();
            bgImage.color = Color.red;
            
            RectTransform bgRect = bgImage.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.1f, 0.1f);
            bgRect.anchorMax = new Vector2(0.9f, 0.3f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 创建血条前景
            GameObject healthBarFg = new GameObject("Health Bar Foreground");
            healthBarFg.transform.SetParent(healthBarBg.transform);
            
            Slider healthSlider = healthBarFg.AddComponent<Slider>();
            healthSlider.value = 1f;
            healthSlider.interactable = false;
            
            // 设置血条填充
            Image fillImage = healthBarFg.AddComponent<Image>();
            fillImage.color = Color.green;
            fillImage.type = Image.Type.Filled;
            
            RectTransform fillRect = fillImage.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            healthSlider.fillRect = fillRect;
        }
        
        void SetupBossScript()
        {
            // 移除现有的Boss脚本
            BossController existingController = GetComponent<BossController>();
            if (existingController != null)
            {
                DestroyImmediate(existingController);
            }
            
            // 添加对应的Boss脚本
            switch (bossType)
            {
                case BossType.QiuQiu:
                    QiuQiuBoss qiuqiu = gameObject.AddComponent<QiuQiuBoss>();
                    SetupBossReferences(qiuqiu);
                    break;
                    
                case BossType.GuoGuo:
                    GuoGuoBoss guoguo = gameObject.AddComponent<GuoGuoBoss>();
                    SetupBossReferences(guoguo);
                    break;
            }
        }
        
        void SetupBossReferences(BossController bossController)
        {
            // 设置UI引用
            Canvas nameCanvas = GetComponentInChildren<Canvas>();
            if (nameCanvas != null)
            {
                bossController.nameCanvas = nameCanvas;
                
                Text nameText = nameCanvas.GetComponentInChildren<Text>();
                if (nameText != null)
                {
                    bossController.nameText = nameText;
                }
                
                Slider healthBar = nameCanvas.GetComponentInChildren<Slider>();
                if (healthBar != null)
                {
                    bossController.healthBar = healthBar;
                }
            }
            
            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                bossController.player = player.transform;
            }
        }
        
        string GetBossName()
        {
            switch (bossType)
            {
                case BossType.QiuQiu:
                    return "球球";
                case BossType.GuoGuo:
                    return "果果";
                default:
                    return "Boss";
            }
        }
        
        // 编辑器方法
        [ContextMenu("Setup Boss")]
        void SetupBossFromMenu()
        {
            SetupBoss();
        }
        
        [ContextMenu("Reset Boss")]
        void ResetBoss()
        {
            // 清理组件
            BossController[] controllers = GetComponents<BossController>();
            foreach (var controller in controllers)
            {
                DestroyImmediate(controller);
            }
            
            // 清理UI
            Canvas nameCanvas = GetComponentInChildren<Canvas>();
            if (nameCanvas != null)
            {
                DestroyImmediate(nameCanvas.gameObject);
            }
            
            // 清理模型
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }
            
            foreach (var child in children)
            {
                if (child.name.Contains("Model"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            UnityEngine.Debug.Log("Boss组件已重置！");
        }
    }
}
