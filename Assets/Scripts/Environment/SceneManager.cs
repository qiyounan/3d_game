using UnityEngine;
using System.Collections.Generic;

namespace BossBattle.Environment
{
    /// <summary>
    /// 场景管理器 - 管理战斗场景的生成和环境元素
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [Header("场景设置")]
        public Vector3 arenaSize = new Vector3(30f, 0f, 30f);
        public float wallHeight = 5f;
        public Material arenaMaterial;
        public Material wallMaterial;
        
        [Header("环境元素")]
        public GameObject[] obstaclePrefabs;
        public GameObject[] decorationPrefabs;
        public int obstacleCount = 5;
        public int decorationCount = 10;
        
        [Header("光照设置")]
        public Light mainLight;
        public Color ambientColor = new Color(0.2f, 0.2f, 0.3f);
        public float lightIntensity = 1.2f;
        
        [Header("性能优化")]
        public bool enableOcclusion = true;
        public bool enableLOD = true;
        public int maxDrawDistance = 100;
        
        private List<GameObject> environmentObjects = new List<GameObject>();
        
        void Start()
        {
            CreateArena();
            SetupLighting();
            PopulateEnvironment();
            OptimizeForMobile();
        }
        
        void CreateArena()
        {
            // 创建地面
            CreateGround();
            
            // 创建围墙
            CreateWalls();
            
            UnityEngine.Debug.Log("战斗竞技场创建完成");
        }
        
        void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(arenaSize.x / 10f, 1f, arenaSize.z / 10f);
            ground.tag = "Ground";
            
            // 应用材质
            if (arenaMaterial != null)
            {
                Renderer renderer = ground.GetComponent<Renderer>();
                renderer.material = arenaMaterial;
            }
            else
            {
                // 创建简单的地面材质
                Material groundMat = new Material(Shader.Find("Standard"));
                groundMat.color = new Color(0.4f, 0.3f, 0.2f); // 棕色地面
                ground.GetComponent<Renderer>().material = groundMat;
            }
            
            environmentObjects.Add(ground);
        }
        
        void CreateWalls()
        {
            float halfX = arenaSize.x / 2f;
            float halfZ = arenaSize.z / 2f;
            
            // 创建四面墙
            CreateWall(new Vector3(0, wallHeight / 2f, halfZ), new Vector3(arenaSize.x, wallHeight, 1f)); // 北墙
            CreateWall(new Vector3(0, wallHeight / 2f, -halfZ), new Vector3(arenaSize.x, wallHeight, 1f)); // 南墙
            CreateWall(new Vector3(halfX, wallHeight / 2f, 0), new Vector3(1f, wallHeight, arenaSize.z)); // 东墙
            CreateWall(new Vector3(-halfX, wallHeight / 2f, 0), new Vector3(1f, wallHeight, arenaSize.z)); // 西墙
        }
        
        void CreateWall(Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Arena Wall";
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.tag = "Wall";
            
            // 应用材质
            if (wallMaterial != null)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                renderer.material = wallMaterial;
            }
            else
            {
                // 创建简单的墙壁材质
                Material wallMat = new Material(Shader.Find("Standard"));
                wallMat.color = new Color(0.6f, 0.6f, 0.6f); // 灰色墙壁
                wall.GetComponent<Renderer>().material = wallMat;
            }
            
            environmentObjects.Add(wall);
        }
        
        void SetupLighting()
        {
            // 设置环境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            
            // 设置主光源
            if (mainLight == null)
            {
                GameObject lightObj = new GameObject("Main Light");
                mainLight = lightObj.AddComponent<Light>();
                lightObj.transform.position = new Vector3(0, 10f, 0);
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            }
            
            mainLight.type = LightType.Directional;
            mainLight.intensity = lightIntensity;
            mainLight.shadows = LightShadows.Soft;
            mainLight.color = Color.white;
            
            // 移动端优化：降低阴影质量
            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 20f;
            }
        }
        
        void PopulateEnvironment()
        {
            // 添加障碍物
            PlaceObstacles();
            
            // 添加装饰物
            PlaceDecorations();
        }
        
        void PlaceObstacles()
        {
            for (int i = 0; i < obstacleCount; i++)
            {
                Vector3 position = GetRandomPositionInArena();
                
                // 确保不会阻挡主要路径
                if (IsPositionValid(position, 3f))
                {
                    GameObject obstacle = CreateObstacle(position);
                    if (obstacle != null)
                    {
                        environmentObjects.Add(obstacle);
                    }
                }
            }
        }
        
        GameObject CreateObstacle(Vector3 position)
        {
            GameObject obstacle;
            
            if (obstaclePrefabs.Length > 0)
            {
                // 使用预制体
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                obstacle = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // 创建简单的障碍物
                obstacle = CreateSimpleObstacle(position);
            }
            
            obstacle.tag = "Obstacle";
            return obstacle;
        }
        
        GameObject CreateSimpleObstacle(Vector3 position)
        {
            // 随机选择障碍物类型
            PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Capsule };
            PrimitiveType type = types[Random.Range(0, types.Length)];
            
            GameObject obstacle = GameObject.CreatePrimitive(type);
            obstacle.name = "Simple Obstacle";
            obstacle.transform.position = position;
            
            // 随机大小
            float scale = Random.Range(1f, 3f);
            obstacle.transform.localScale = Vector3.one * scale;
            
            // 随机颜色
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(Random.value, Random.value, Random.value);
            obstacle.GetComponent<Renderer>().material = mat;
            
            return obstacle;
        }
        
        void PlaceDecorations()
        {
            for (int i = 0; i < decorationCount; i++)
            {
                Vector3 position = GetRandomPositionInArena();
                
                GameObject decoration = CreateDecoration(position);
                if (decoration != null)
                {
                    environmentObjects.Add(decoration);
                }
            }
        }
        
        GameObject CreateDecoration(Vector3 position)
        {
            GameObject decoration;
            
            if (decorationPrefabs.Length > 0)
            {
                // 使用预制体
                GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
                decoration = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // 创建简单的装饰物
                decoration = CreateSimpleDecoration(position);
            }
            
            decoration.tag = "Decoration";
            
            // 装饰物不应该有碰撞器
            Collider collider = decoration.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            return decoration;
        }
        
        GameObject CreateSimpleDecoration(Vector3 position)
        {
            GameObject decoration = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            decoration.name = "Simple Decoration";
            decoration.transform.position = position;
            decoration.transform.localScale = Vector3.one * Random.Range(0.3f, 0.8f);
            
            // 随机颜色（较暗，作为装饰）
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(Random.value * 0.5f, Random.value * 0.5f, Random.value * 0.5f);
            decoration.GetComponent<Renderer>().material = mat;
            
            return decoration;
        }
        
        Vector3 GetRandomPositionInArena()
        {
            float x = Random.Range(-arenaSize.x / 2f + 2f, arenaSize.x / 2f - 2f);
            float z = Random.Range(-arenaSize.z / 2f + 2f, arenaSize.z / 2f - 2f);
            
            // 确保在地面上
            float y = 0.5f;
            
            return new Vector3(x, y, z);
        }
        
        bool IsPositionValid(Vector3 position, float minDistance)
        {
            // 检查是否距离中心太近（玩家生成点）
            if (Vector3.Distance(position, Vector3.zero) < minDistance)
            {
                return false;
            }
            
            // 检查是否与其他对象重叠
            Collider[] overlapping = Physics.OverlapSphere(position, 1f);
            return overlapping.Length == 0;
        }
        
        void OptimizeForMobile()
        {
            // 移动端性能优化
            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // 降低渲染质量
                QualitySettings.SetQualityLevel(1); // Low quality
                
                // 禁用一些高级特性
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                
                // 设置较低的粒子系统质量
                QualitySettings.particleRaycastBudget = 64;
                
                // 优化阴影
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 15f;
            }
            
            // 设置LOD
            if (enableLOD)
            {
                SetupLODGroups();
            }
            
            // 设置遮挡剔除
            if (enableOcclusion)
            {
                SetupOcclusionCulling();
            }
        }
        
        void SetupLODGroups()
        {
            foreach (GameObject obj in environmentObjects)
            {
                if (obj != null && obj.GetComponent<LODGroup>() == null)
                {
                    LODGroup lodGroup = obj.AddComponent<LODGroup>();
                    
                    // 简单的LOD设置
                    LOD[] lods = new LOD[2];
                    lods[0] = new LOD(0.6f, obj.GetComponentsInChildren<Renderer>());
                    lods[1] = new LOD(0.1f, new Renderer[0]); // 远距离时不渲染
                    
                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                }
            }
        }
        
        void SetupOcclusionCulling()
        {
            // 为大型对象添加遮挡剔除
            foreach (GameObject obj in environmentObjects)
            {
                if (obj != null && obj.name.Contains("Wall"))
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.allowOcclusionWhenDynamic = true;
                    }
                }
            }
        }
        
        public void ClearEnvironment()
        {
            foreach (GameObject obj in environmentObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            environmentObjects.Clear();
        }
        
        void OnDrawGizmosSelected()
        {
            // 显示竞技场边界
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, arenaSize);
            
            // 显示墙壁高度
            Gizmos.color = Color.red;
            Vector3 wallTop = new Vector3(arenaSize.x, wallHeight, arenaSize.z);
            Gizmos.DrawWireCube(Vector3.up * wallHeight / 2f, wallTop);
        }
        
        [ContextMenu("Regenerate Environment")]
        void RegenerateEnvironment()
        {
            ClearEnvironment();
            CreateArena();
            PopulateEnvironment();
        }
    }
}
