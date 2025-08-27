using UnityEngine;
using System.Collections;

namespace BossBattle.Combat
{
    /// <summary>
    /// 战斗反馈系统 - 提供视觉、听觉和触觉反馈
    /// </summary>
    public class CombatFeedback : MonoBehaviour
    {
        [Header("屏幕震动")]
        public bool enableScreenShake = true;
        public float shakeIntensity = 0.5f;
        public float shakeDuration = 0.2f;
        
        [Header("屏幕闪烁")]
        public bool enableScreenFlash = true;
        public Color damageFlashColor = Color.red;
        public Color healFlashColor = Color.green;
        public float flashDuration = 0.1f;
        
        [Header("时间缓慢")]
        public bool enableTimeSlowdown = true;
        public float slowdownScale = 0.3f;
        public float slowdownDuration = 0.1f;
        
        [Header("音效")]
        public AudioClip[] hitSounds;
        public AudioClip[] criticalHitSounds;
        public AudioClip[] blockSounds;
        public AudioClip healSound;
        
        [Header("粒子效果")]
        public GameObject bloodEffect;
        public GameObject sparkEffect;
        public GameObject healEffect;
        public GameObject criticalEffect;
        
        private Camera playerCamera;
        private AudioSource audioSource;
        private GameObject screenFlashOverlay;
        
        // 单例模式
        public static CombatFeedback Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            playerCamera = Camera.main;
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            CreateScreenFlashOverlay();
        }
        
        void CreateScreenFlashOverlay()
        {
            // 创建屏幕闪烁覆盖层
            GameObject canvasObj = new GameObject("Screen Flash Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 创建闪烁图像
            GameObject flashObj = new GameObject("Flash Overlay");
            flashObj.transform.SetParent(canvasObj.transform);
            
            UnityEngine.UI.Image flashImage = flashObj.AddComponent<UnityEngine.UI.Image>();
            flashImage.color = new Color(1, 1, 1, 0);
            flashImage.raycastTarget = false;
            
            RectTransform flashRect = flashImage.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            
            screenFlashOverlay = flashObj;
        }
        
        /// <summary>
        /// 播放伤害反馈
        /// </summary>
        public void PlayDamageFeedback(DamageResult damageResult)
        {
            // 屏幕震动
            if (enableScreenShake)
            {
                float intensity = damageResult.isCritical ? shakeIntensity * 1.5f : shakeIntensity;
                StartScreenShake(intensity, shakeDuration);
            }
            
            // 屏幕闪烁
            if (enableScreenFlash)
            {
                StartScreenFlash(damageFlashColor, flashDuration);
            }
            
            // 时间缓慢（暴击时）
            if (enableTimeSlowdown && damageResult.isCritical)
            {
                StartTimeSlowdown(slowdownScale, slowdownDuration);
            }
            
            // 音效
            PlayDamageSound(damageResult);
            
            // 粒子效果
            CreateDamageParticles(damageResult);
            
            // 移动端震动
            TriggerHapticFeedback(damageResult);
        }
        
        /// <summary>
        /// 播放治疗反馈
        /// </summary>
        public void PlayHealFeedback(Vector3 position, float healAmount)
        {
            // 屏幕闪烁
            if (enableScreenFlash)
            {
                StartScreenFlash(healFlashColor, flashDuration);
            }
            
            // 音效
            if (healSound != null)
            {
                audioSource.PlayOneShot(healSound);
            }
            
            // 粒子效果
            if (healEffect != null)
            {
                GameObject effect = Instantiate(healEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        /// <summary>
        /// 播放格挡反馈
        /// </summary>
        public void PlayBlockFeedback(Vector3 position)
        {
            // 轻微震动
            if (enableScreenShake)
            {
                StartScreenShake(shakeIntensity * 0.3f, shakeDuration * 0.5f);
            }
            
            // 音效
            if (blockSounds.Length > 0)
            {
                AudioClip sound = blockSounds[Random.Range(0, blockSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
            
            // 火花效果
            if (sparkEffect != null)
            {
                GameObject effect = Instantiate(sparkEffect, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        
        void StartScreenShake(float intensity, float duration)
        {
            if (playerCamera != null)
            {
                StartCoroutine(ScreenShakeCoroutine(intensity, duration));
            }
        }
        
        IEnumerator ScreenShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPosition = playerCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                playerCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                yield return null;
            }
            
            playerCamera.transform.localPosition = originalPosition;
        }
        
        void StartScreenFlash(Color flashColor, float duration)
        {
            if (screenFlashOverlay != null)
            {
                StartCoroutine(ScreenFlashCoroutine(flashColor, duration));
            }
        }
        
        IEnumerator ScreenFlashCoroutine(Color flashColor, float duration)
        {
            UnityEngine.UI.Image flashImage = screenFlashOverlay.GetComponent<UnityEngine.UI.Image>();
            if (flashImage == null) yield break;
            
            // 闪烁到目标颜色
            flashColor.a = 0.3f;
            flashImage.color = flashColor;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.3f, 0f, elapsed / duration);
                
                Color color = flashColor;
                color.a = alpha;
                flashImage.color = color;
                
                yield return null;
            }
            
            // 确保完全透明
            Color finalColor = flashColor;
            finalColor.a = 0f;
            flashImage.color = finalColor;
        }
        
        void StartTimeSlowdown(float scale, float duration)
        {
            StartCoroutine(TimeSlowdownCoroutine(scale, duration));
        }
        
        IEnumerator TimeSlowdownCoroutine(float scale, float duration)
        {
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
        
        void PlayDamageSound(DamageResult damageResult)
        {
            AudioClip[] soundArray = damageResult.isCritical ? criticalHitSounds : hitSounds;
            
            if (soundArray.Length > 0)
            {
                AudioClip sound = soundArray[Random.Range(0, soundArray.Length)];
                audioSource.PlayOneShot(sound);
            }
        }
        
        void CreateDamageParticles(DamageResult damageResult)
        {
            GameObject effectPrefab = null;
            
            if (damageResult.isCritical && criticalEffect != null)
            {
                effectPrefab = criticalEffect;
            }
            else
            {
                switch (damageResult.damageType)
                {
                    case DamageSystem.DamageType.Physical:
                        effectPrefab = bloodEffect;
                        break;
                    case DamageSystem.DamageType.Fire:
                    case DamageSystem.DamageType.Lightning:
                        effectPrefab = sparkEffect;
                        break;
                    default:
                        effectPrefab = bloodEffect;
                        break;
                }
            }
            
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, damageResult.hitPoint, 
                    Quaternion.LookRotation(damageResult.hitDirection));
                Destroy(effect, 2f);
            }
        }
        
        void TriggerHapticFeedback(DamageResult damageResult)
        {
            // 移动端震动反馈
            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (damageResult.isCritical)
                {
                    // 强震动
                    Handheld.Vibrate();
                }
                else
                {
                    // 轻震动（需要自定义实现）
                    Handheld.Vibrate();
                }
            }
        }
        
        /// <summary>
        /// 创建简单的粒子效果
        /// </summary>
        public GameObject CreateSimpleParticleEffect(Vector3 position, Color color, int particleCount = 20)
        {
            GameObject effectObj = new GameObject("Simple Particle Effect");
            effectObj.transform.position = position;
            
            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = color;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.maxParticles = particleCount;
            
            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, particleCount)
            });
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            return effectObj;
        }
        
        void OnDestroy()
        {
            // 恢复时间缩放
            Time.timeScale = 1f;
        }
    }
}
