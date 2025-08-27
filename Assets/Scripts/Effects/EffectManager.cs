using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BossBattle.Effects
{
    /// <summary>
    /// 特效管理器 - 管理游戏中的粒子特效和视觉效果
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [Header("攻击特效")]
        public GameObject slashEffect;
        public GameObject impactEffect;
        public GameObject criticalHitEffect;
        public GameObject blockEffect;
        
        [Header("技能特效")]
        public GameObject fireballEffect;
        public GameObject shockwaveEffect;
        public GameObject healEffect;
        public GameObject buffEffect;
        
        [Header("环境特效")]
        public GameObject dustEffect;
        public GameObject sparkEffect;
        public GameObject smokeEffect;
        public GameObject explosionEffect;
        
        [Header("UI特效")]
        public GameObject levelUpEffect;
        public GameObject collectEffect;
        public GameObject teleportEffect;
        
        [Header("特效池设置")]
        public int poolSize = 20;
        public bool useObjectPooling = true;
        
        private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();
        
        // 单例模式
        public static EffectManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeEffectManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void InitializeEffectManager()
        {
            // 注册特效预制体
            RegisterEffect("slash", slashEffect);
            RegisterEffect("impact", impactEffect);
            RegisterEffect("critical", criticalHitEffect);
            RegisterEffect("block", blockEffect);
            RegisterEffect("fireball", fireballEffect);
            RegisterEffect("shockwave", shockwaveEffect);
            RegisterEffect("heal", healEffect);
            RegisterEffect("buff", buffEffect);
            RegisterEffect("dust", dustEffect);
            RegisterEffect("spark", sparkEffect);
            RegisterEffect("smoke", smokeEffect);
            RegisterEffect("explosion", explosionEffect);
            RegisterEffect("levelup", levelUpEffect);
            RegisterEffect("collect", collectEffect);
            RegisterEffect("teleport", teleportEffect);
            
            if (useObjectPooling)
            {
                InitializeObjectPools();
            }
        }
        
        void RegisterEffect(string name, GameObject prefab)
        {
            if (prefab != null)
            {
                effectPrefabs[name] = prefab;
            }
        }
        
        void InitializeObjectPools()
        {
            foreach (var kvp in effectPrefabs)
            {
                string effectName = kvp.Key;
                GameObject prefab = kvp.Value;
                
                Queue<GameObject> pool = new Queue<GameObject>();
                
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    pool.Enqueue(obj);
                }
                
                effectPools[effectName] = pool;
            }
        }
        
        /// <summary>
        /// 播放特效
        /// </summary>
        public GameObject PlayEffect(string effectName, Vector3 position, Quaternion rotation = default, float duration = 2f)
        {
            GameObject effect = GetEffect(effectName);
            if (effect == null) return null;
            
            effect.transform.position = position;
            effect.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            effect.SetActive(true);
            
            // 自动回收或销毁
            StartCoroutine(ReturnEffectAfterDelay(effect, effectName, duration));
            
            return effect;
        }
        
        /// <summary>
        /// 播放特效（带缩放）
        /// </summary>
        public GameObject PlayEffect(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, float duration = 2f)
        {
            GameObject effect = PlayEffect(effectName, position, rotation, duration);
            if (effect != null)
            {
                effect.transform.localScale = scale;
            }
            return effect;
        }
        
        GameObject GetEffect(string effectName)
        {
            if (!effectPrefabs.ContainsKey(effectName))
            {
                UnityEngine.Debug.LogWarning($"特效 '{effectName}' 未找到！");
                return null;
            }
            
            if (useObjectPooling && effectPools.ContainsKey(effectName))
            {
                return GetPooledEffect(effectName);
            }
            else
            {
                return Instantiate(effectPrefabs[effectName]);
            }
        }
        
        GameObject GetPooledEffect(string effectName)
        {
            Queue<GameObject> pool = effectPools[effectName];
            
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                // 池中没有可用对象，创建新的
                return Instantiate(effectPrefabs[effectName]);
            }
        }
        
        IEnumerator ReturnEffectAfterDelay(GameObject effect, string effectName, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effect != null)
            {
                ReturnEffect(effect, effectName);
            }
        }
        
        void ReturnEffect(GameObject effect, string effectName)
        {
            effect.SetActive(false);
            
            if (useObjectPooling && effectPools.ContainsKey(effectName))
            {
                effect.transform.SetParent(transform);
                effectPools[effectName].Enqueue(effect);
            }
            else
            {
                Destroy(effect);
            }
        }
        
        // 具体特效播放方法
        public void PlayAttackEffect(Vector3 position, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            PlayEffect("slash", position, rotation, 1f);
        }
        
        public void PlayImpactEffect(Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            PlayEffect("impact", position, rotation, 1.5f);
        }
        
        public void PlayCriticalHitEffect(Vector3 position)
        {
            PlayEffect("critical", position, Quaternion.identity, Vector3.one * 1.5f, 2f);
        }
        
        public void PlayBlockEffect(Vector3 position, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            PlayEffect("block", position, rotation, 1f);
        }
        
        public void PlayFireballEffect(Vector3 position, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            PlayEffect("fireball", position, rotation, 3f);
        }
        
        public void PlayShockwaveEffect(Vector3 position, float radius = 5f)
        {
            Vector3 scale = Vector3.one * radius;
            PlayEffect("shockwave", position, Quaternion.identity, scale, 2f);
        }
        
        public void PlayHealEffect(Vector3 position)
        {
            PlayEffect("heal", position, Quaternion.identity, 3f);
        }
        
        public void PlayExplosionEffect(Vector3 position, float scale = 1f)
        {
            Vector3 effectScale = Vector3.one * scale;
            PlayEffect("explosion", position, Quaternion.identity, effectScale, 3f);
        }
        
        public void PlayDustEffect(Vector3 position)
        {
            PlayEffect("dust", position, Quaternion.identity, 1f);
        }
        
        /// <summary>
        /// 创建简单的粒子特效
        /// </summary>
        public GameObject CreateSimpleParticleEffect(Vector3 position, Color color, int particleCount = 50, float lifetime = 2f)
        {
            GameObject effectObj = new GameObject("Simple Particle Effect");
            effectObj.transform.position = position;
            
            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();
            
            // 主模块设置
            var main = particles.main;
            main.startColor = color;
            main.startLifetime = lifetime;
            main.startSpeed = 5f;
            main.maxParticles = particleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // 发射模块
            var emission = particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, particleCount)
            });
            
            // 形状模块
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            // 速度模块
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            
            // 大小模块
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // 透明度模块
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;
            
            // 自动销毁
            Destroy(effectObj, lifetime + 1f);
            
            return effectObj;
        }
        
        /// <summary>
        /// 创建拖尾特效
        /// </summary>
        public GameObject CreateTrailEffect(Transform target, Color color, float width = 0.1f, float time = 1f)
        {
            GameObject trailObj = new GameObject("Trail Effect");
            trailObj.transform.SetParent(target);
            trailObj.transform.localPosition = Vector3.zero;
            
            TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
            Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
            trailMaterial.color = color;
            trail.material = trailMaterial;
            trail.startWidth = width;
            trail.endWidth = 0f;
            trail.time = time;
            trail.minVertexDistance = 0.1f;
            
            return trailObj;
        }
        
        /// <summary>
        /// 停止所有特效
        /// </summary>
        public void StopAllEffects()
        {
            ParticleSystem[] allParticles = FindObjectsOfType<ParticleSystem>();
            foreach (var particle in allParticles)
            {
                particle.Stop();
            }
        }
        
        /// <summary>
        /// 清理所有特效
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var pool in effectPools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            effectPools.Clear();
        }
        
        void OnDestroy()
        {
            ClearAllEffects();
        }
    }
}
