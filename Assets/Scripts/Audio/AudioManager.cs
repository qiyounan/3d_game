using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BossBattle.Audio
{
    /// <summary>
    /// 音效管理器 - 管理游戏中的所有音效和背景音乐
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("音频源")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        
        [Header("背景音乐")]
        public AudioClip menuMusic;
        public AudioClip battleMusic;
        public AudioClip victoryMusic;
        public AudioClip gameOverMusic;
        
        [Header("音效")]
        public AudioClip[] attackSounds;
        public AudioClip[] hitSounds;
        public AudioClip[] skillSounds;
        public AudioClip[] uiSounds;
        public AudioClip[] footstepSounds;
        
        [Header("Boss音效")]
        public AudioClip[] bossAttackSounds;
        public AudioClip[] bossHurtSounds;
        public AudioClip[] bossDeathSounds;
        
        [Header("环境音效")]
        public AudioClip ambientSound;
        public AudioClip[] randomAmbientSounds;
        public float ambientSoundInterval = 30f;
        
        [Header("音量设置")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;
        
        [Header("3D音效设置")]
        public float maxDistance = 20f;
        public AnimationCurve distanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private List<AudioSource> activeSources = new List<AudioSource>();
        private Coroutine ambientCoroutine;
        
        // 单例模式
        public static AudioManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            StartAmbientSounds();
        }
        
        void InitializeAudioManager()
        {
            // 创建音频源
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("Music Source");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("Ambient Source");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            // 初始化音频字典
            InitializeAudioDictionary();
            
            // 应用音量设置
            UpdateVolumeSettings();
        }
        
        void InitializeAudioDictionary()
        {
            // 添加音效到字典中便于查找
            AddAudioClipsToDict("attack", attackSounds);
            AddAudioClipsToDict("hit", hitSounds);
            AddAudioClipsToDict("skill", skillSounds);
            AddAudioClipsToDict("ui", uiSounds);
            AddAudioClipsToDict("footstep", footstepSounds);
            AddAudioClipsToDict("boss_attack", bossAttackSounds);
            AddAudioClipsToDict("boss_hurt", bossHurtSounds);
            AddAudioClipsToDict("boss_death", bossDeathSounds);
        }
        
        void AddAudioClipsToDict(string prefix, AudioClip[] clips)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    audioClips[$"{prefix}_{i}"] = clips[i];
                }
            }
        }
        
        void UpdateVolumeSettings()
        {
            AudioListener.volume = masterVolume;
            
            if (musicSource != null)
                musicSource.volume = musicVolume;
            
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
            
            if (ambientSource != null)
                ambientSource.volume = ambientVolume;
        }
        
        // 背景音乐控制
        public void PlayMusic(AudioClip clip, bool fadeIn = true)
        {
            if (clip == null || musicSource == null) return;
            
            if (fadeIn && musicSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPlayMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }
        
        public void PlayBattleMusic()
        {
            PlayMusic(battleMusic);
        }
        
        public void PlayVictoryMusic()
        {
            PlayMusic(victoryMusic);
        }
        
        public void PlayGameOverMusic()
        {
            PlayMusic(gameOverMusic);
        }
        
        public void StopMusic(bool fadeOut = true)
        {
            if (musicSource == null) return;
            
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }
        }
        
        IEnumerator FadeOutAndPlayMusic(AudioClip newClip)
        {
            float startVolume = musicSource.volume;
            
            // 淡出当前音乐
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / 1f; // 1秒淡出
                yield return null;
            }
            
            // 播放新音乐
            musicSource.clip = newClip;
            musicSource.Play();
            
            // 淡入新音乐
            while (musicSource.volume < startVolume)
            {
                musicSource.volume += startVolume * Time.deltaTime / 1f; // 1秒淡入
                yield return null;
            }
            
            musicSource.volume = startVolume;
        }
        
        IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / 1f;
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = startVolume;
        }
        
        // 音效播放
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            
            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }
        
        public void PlaySFX(string clipName, float volume = 1f)
        {
            if (audioClips.ContainsKey(clipName))
            {
                PlaySFX(audioClips[clipName], volume);
            }
        }
        
        public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
        {
            if (clips.Length > 0)
            {
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                PlaySFX(clip, volume);
            }
        }
        
        public void PlayAttackSound()
        {
            PlayRandomSFX(attackSounds);
        }
        
        public void PlayHitSound()
        {
            PlayRandomSFX(hitSounds);
        }
        
        public void PlaySkillSound(int skillIndex)
        {
            if (skillIndex < skillSounds.Length)
            {
                PlaySFX(skillSounds[skillIndex]);
            }
        }
        
        public void PlayUISound(int soundIndex)
        {
            if (soundIndex < uiSounds.Length)
            {
                PlaySFX(uiSounds[soundIndex]);
            }
        }
        
        public void PlayBossAttackSound()
        {
            PlayRandomSFX(bossAttackSounds);
        }
        
        public void PlayBossHurtSound()
        {
            PlayRandomSFX(bossHurtSounds);
        }
        
        public void PlayBossDeathSound()
        {
            PlayRandomSFX(bossDeathSounds);
        }
        
        // 3D音效播放
        public void Play3DSFX(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            
            GameObject tempAudioObj = new GameObject("Temp Audio");
            tempAudioObj.transform.position = position;
            
            AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volume * sfxVolume;
            tempSource.spatialBlend = 1f; // 3D音效
            tempSource.maxDistance = maxDistance;
            tempSource.rolloffMode = AudioRolloffMode.Custom;
            tempSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, distanceCurve);
            
            tempSource.Play();
            
            // 播放完成后销毁
            Destroy(tempAudioObj, clip.length);
        }
        
        // 环境音效
        void StartAmbientSounds()
        {
            if (ambientSound != null && ambientSource != null)
            {
                ambientSource.clip = ambientSound;
                ambientSource.Play();
            }
            
            if (randomAmbientSounds.Length > 0)
            {
                ambientCoroutine = StartCoroutine(PlayRandomAmbientSounds());
            }
        }
        
        IEnumerator PlayRandomAmbientSounds()
        {
            while (true)
            {
                yield return new WaitForSeconds(ambientSoundInterval + Random.Range(-5f, 5f));
                
                if (randomAmbientSounds.Length > 0)
                {
                    AudioClip clip = randomAmbientSounds[Random.Range(0, randomAmbientSounds.Length)];
                    PlaySFX(clip, 0.3f);
                }
            }
        }
        
        // 音量控制
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumeSettings();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
                ambientSource.volume = ambientVolume;
        }
        
        // 暂停/恢复
        public void PauseAll()
        {
            if (musicSource != null && musicSource.isPlaying)
                musicSource.Pause();
            
            if (ambientSource != null && ambientSource.isPlaying)
                ambientSource.Pause();
        }
        
        public void ResumeAll()
        {
            if (musicSource != null)
                musicSource.UnPause();
            
            if (ambientSource != null)
                ambientSource.UnPause();
        }
        
        void OnDestroy()
        {
            if (ambientCoroutine != null)
            {
                StopCoroutine(ambientCoroutine);
            }
        }
    }
}
