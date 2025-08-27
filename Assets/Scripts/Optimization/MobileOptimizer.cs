using UnityEngine;
using System.Collections;

namespace BossBattle.Optimization
{
    /// <summary>
    /// 移动端性能优化器 - 自动检测设备性能并调整设置
    /// </summary>
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("性能检测")]
        public bool autoDetectPerformance = true;
        public float performanceTestDuration = 2f;
        
        [Header("质量设置")]
        public QualityProfile lowEndProfile;
        public QualityProfile midEndProfile;
        public QualityProfile highEndProfile;
        
        [Header("动态调整")]
        public bool enableDynamicAdjustment = true;
        public float targetFrameRate = 30f;
        public float adjustmentInterval = 5f;
        
        private DevicePerformance detectedPerformance = DevicePerformance.Unknown;
        private float averageFrameRate = 60f;
        private int frameCount = 0;
        private float frameTimeSum = 0f;
        
        public enum DevicePerformance
        {
            Unknown,
            LowEnd,
            MidEnd,
            HighEnd
        }
        
        [System.Serializable]
        public class QualityProfile
        {
            [Header("渲染设置")]
            public int qualityLevel = 0;
            public int targetFrameRate = 30;
            public bool vsync = false;
            
            [Header("阴影设置")]
            public ShadowQuality shadowQuality = ShadowQuality.Disable;
            public ShadowResolution shadowResolution = ShadowResolution.Low;
            public float shadowDistance = 10f;
            
            [Header("纹理设置")]
            public int textureQuality = 2;
            public int anisotropicFiltering = 0;
            
            [Header("粒子设置")]
            public int particleRaycastBudget = 32;
            public bool softParticles = false;
            
            [Header("其他设置")]
            public bool realtimeReflectionProbes = false;
            public int antiAliasing = 0;
            public float renderScale = 1f;
        }
        
        void Start()
        {
            // 只在移动端运行
            if (Application.platform != RuntimePlatform.Android && 
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                enabled = false;
                return;
            }
            
            InitializeOptimization();
        }
        
        void InitializeOptimization()
        {
            // 设置初始质量
            ApplyQualityProfile(lowEndProfile);
            
            if (autoDetectPerformance)
            {
                StartCoroutine(DetectDevicePerformance());
            }
            
            if (enableDynamicAdjustment)
            {
                StartCoroutine(DynamicPerformanceAdjustment());
            }
        }
        
        IEnumerator DetectDevicePerformance()
        {
            UnityEngine.Debug.Log("开始检测设备性能...");
            
            // 重置统计
            frameCount = 0;
            frameTimeSum = 0f;
            
            float startTime = Time.time;
            
            // 运行性能测试
            while (Time.time - startTime < performanceTestDuration)
            {
                frameCount++;
                frameTimeSum += Time.deltaTime;
                yield return null;
            }
            
            // 计算平均帧率
            averageFrameRate = frameCount / frameTimeSum;
            
            // 根据帧率判断设备性能
            if (averageFrameRate >= 45f)
            {
                detectedPerformance = DevicePerformance.HighEnd;
                ApplyQualityProfile(highEndProfile);
            }
            else if (averageFrameRate >= 25f)
            {
                detectedPerformance = DevicePerformance.MidEnd;
                ApplyQualityProfile(midEndProfile);
            }
            else
            {
                detectedPerformance = DevicePerformance.LowEnd;
                ApplyQualityProfile(lowEndProfile);
            }
            
            UnityEngine.Debug.Log($"设备性能检测完成: {detectedPerformance} (平均帧率: {averageFrameRate:F1})");
        }
        
        IEnumerator DynamicPerformanceAdjustment()
        {
            while (true)
            {
                yield return new WaitForSeconds(adjustmentInterval);
                
                // 检查当前帧率
                float currentFrameRate = 1f / Time.deltaTime;
                
                if (currentFrameRate < targetFrameRate * 0.8f)
                {
                    // 帧率过低，降低质量
                    ReduceQuality();
                }
                else if (currentFrameRate > targetFrameRate * 1.2f && detectedPerformance != DevicePerformance.LowEnd)
                {
                    // 帧率过高，可以提升质量
                    IncreaseQuality();
                }
            }
        }
        
        void ApplyQualityProfile(QualityProfile profile)
        {
            if (profile == null) return;
            
            // 应用质量等级
            QualitySettings.SetQualityLevel(profile.qualityLevel);
            
            // 设置目标帧率
            Application.targetFrameRate = profile.targetFrameRate;
            
            // 设置垂直同步
            QualitySettings.vSyncCount = profile.vsync ? 1 : 0;
            
            // 阴影设置
            QualitySettings.shadows = profile.shadowQuality;
            QualitySettings.shadowResolution = profile.shadowResolution;
            QualitySettings.shadowDistance = profile.shadowDistance;
            
            // 纹理设置
            QualitySettings.globalTextureMipmapLimit = profile.textureQuality;
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)profile.anisotropicFiltering;
            
            // 粒子设置
            QualitySettings.particleRaycastBudget = profile.particleRaycastBudget;
            QualitySettings.softParticles = profile.softParticles;
            
            // 其他设置
            QualitySettings.realtimeReflectionProbes = profile.realtimeReflectionProbes;
            QualitySettings.antiAliasing = profile.antiAliasing;
            
            // 渲染缩放 (需要URP包才能使用)
            // if (profile.renderScale != 1f)
            // {
            //     SetRenderScale(profile.renderScale);
            // }
            
            UnityEngine.Debug.Log($"应用质量配置: 质量等级 {profile.qualityLevel}, 目标帧率 {profile.targetFrameRate}");
        }
        
        void SetRenderScale(float scale)
        {
            // 设置渲染缩放（需要URP或HDRP包）
            // 由于没有安装URP包，这个功能暂时禁用
            // #if UNITY_2019_3_OR_NEWER
            // UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.renderScale = scale;
            // #endif
        }
        
        void ReduceQuality()
        {
            switch (detectedPerformance)
            {
                case DevicePerformance.HighEnd:
                    detectedPerformance = DevicePerformance.MidEnd;
                    ApplyQualityProfile(midEndProfile);
                    break;
                case DevicePerformance.MidEnd:
                    detectedPerformance = DevicePerformance.LowEnd;
                    ApplyQualityProfile(lowEndProfile);
                    break;
                case DevicePerformance.LowEnd:
                    // 已经是最低质量，进一步优化
                    ApplyEmergencyOptimizations();
                    break;
            }
            
            UnityEngine.Debug.Log($"降低质量设置到: {detectedPerformance}");
        }
        
        void IncreaseQuality()
        {
            switch (detectedPerformance)
            {
                case DevicePerformance.LowEnd:
                    detectedPerformance = DevicePerformance.MidEnd;
                    ApplyQualityProfile(midEndProfile);
                    break;
                case DevicePerformance.MidEnd:
                    detectedPerformance = DevicePerformance.HighEnd;
                    ApplyQualityProfile(highEndProfile);
                    break;
            }
            
            UnityEngine.Debug.Log($"提升质量设置到: {detectedPerformance}");
        }
        
        void ApplyEmergencyOptimizations()
        {
            // 紧急优化措施
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.globalTextureMipmapLimit = 3; // 最低纹理质量
            QualitySettings.particleRaycastBudget = 16;
            QualitySettings.softParticles = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.antiAliasing = 0;
            
            // 降低目标帧率
            Application.targetFrameRate = 20;
            
            // 禁用一些视觉效果
            DisableNonEssentialEffects();
            
            UnityEngine.Debug.Log("应用紧急优化措施");
        }
        
        void DisableNonEssentialEffects()
        {
            // 禁用非必要的视觉效果
            ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
            foreach (var particle in particles)
            {
                if (particle.gameObject.name.Contains("Decoration") || 
                    particle.gameObject.name.Contains("Ambient"))
                {
                    particle.gameObject.SetActive(false);
                }
            }
            
            // 禁用装饰性光源
            Light[] lights = FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.type != LightType.Directional && light.intensity < 1f)
                {
                    light.gameObject.SetActive(false);
                }
            }
        }
        
        public void ForceQualityLevel(DevicePerformance performance)
        {
            detectedPerformance = performance;
            
            switch (performance)
            {
                case DevicePerformance.LowEnd:
                    ApplyQualityProfile(lowEndProfile);
                    break;
                case DevicePerformance.MidEnd:
                    ApplyQualityProfile(midEndProfile);
                    break;
                case DevicePerformance.HighEnd:
                    ApplyQualityProfile(highEndProfile);
                    break;
            }
        }
        
        public DevicePerformance GetDetectedPerformance()
        {
            return detectedPerformance;
        }
        
        public float GetAverageFrameRate()
        {
            return averageFrameRate;
        }
        
        void OnGUI()
        {
            if (UnityEngine.Debug.isDebugBuild)
            {
                // 显示性能信息
                GUI.Label(new Rect(10, 10, 200, 20), $"设备性能: {detectedPerformance}");
                GUI.Label(new Rect(10, 30, 200, 20), $"平均帧率: {averageFrameRate:F1}");
                GUI.Label(new Rect(10, 50, 200, 20), $"当前帧率: {1f / Time.deltaTime:F1}");
                GUI.Label(new Rect(10, 70, 200, 20), $"质量等级: {QualitySettings.GetQualityLevel()}");
            }
        }
    }
}
