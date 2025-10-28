using UnityEngine;
using ProjectMayhem.Manager;

namespace ProjectMayhem.Manager
{
    /// <summary>
    /// AudioManager handles all audio playback with EventBus integration
    /// </summary>
    public class AudioManager : GenericSingleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip menuMusic;

        // Properties
        public float MasterVolume 
        { 
            get => masterVolume; 
            set 
            { 
                masterVolume = Mathf.Clamp01(value);
                UpdateVolumeLevels();
            } 
        }

        public float MusicVolume 
        { 
            get => musicVolume; 
            set 
            { 
                musicVolume = Mathf.Clamp01(value);
                UpdateVolumeLevels();
            } 
        }

        public float SfxVolume 
        { 
            get => sfxVolume; 
            set 
            { 
                sfxVolume = Mathf.Clamp01(value);
                UpdateVolumeLevels();
            } 
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
        }

        private void OnEnable()
        {
            // Subscribe to EventBus events
            // EventBus.Subscribe<PlaySoundEvent>(OnPlaySound);
            // EventBus.Subscribe<PlayMusicEvent>(OnPlayMusic);
        }

        private void OnDisable()
        {
            // Unsubscribe from EventBus events
            // EventBus.Unsubscribe<PlaySoundEvent>(OnPlaySound);
            // EventBus.Unsubscribe<PlayMusicEvent>(OnPlayMusic);
        }

        private void Start()
        {
            // Play background music on start
            if (backgroundMusic != null)
            {
                PlayMusic(backgroundMusic);
            }
        }

        /// <summary>
        /// Initialize audio sources if not assigned
        /// </summary>
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            UpdateVolumeLevels();
        }

        /// <summary>
        /// Update volume levels for all audio sources
        /// </summary>
        private void UpdateVolumeLevels()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }

        /// <summary>
        /// Handle play sound event from EventBus
        /// </summary>
        /// <param name="e">Play sound event data</param>
        private void OnPlaySound(PlaySoundEvent e)
        {
            PlaySound(e.clip, e.position);
        }

        /// <summary>
        /// Handle play music event from EventBus
        /// </summary>
        /// <param name="e">Play music event data</param>
        private void OnPlayMusic(PlayMusicEvent e)
        {
            PlayMusic(e.clip);
        }

        /// <summary>
        /// Play a sound effect at a specific position
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="position">World position to play the sound</param>
        public void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null audio clip");
                return;
            }

            // Play sound at position using PlayClipAtPoint for 3D audio
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
            Debug.Log($"[AudioManager] Playing sound: {clip.name} at position {position}");
        }

        /// <summary>
        /// Play a sound effect using the SFX audio source
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        public void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null audio clip");
                return;
            }

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Playing sound: {clip.name}");
            }
        }

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">Audio clip to play as music</param>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null music clip");
                return;
            }

            if (musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
                Debug.Log($"[AudioManager] Playing music: {clip.name}");
            }
        }

        /// <summary>
        /// Stop current music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
                Debug.Log("[AudioManager] Music stopped");
            }
        }

        /// <summary>
        /// Pause current music
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] Music paused");
            }
        }

        /// <summary>
        /// Resume paused music
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.UnPause();
                Debug.Log("[AudioManager] Music resumed");
            }
        }

        /// <summary>
        /// Set master volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = volume;
        }

        /// <summary>
        /// Play menu music
        /// </summary>
        public void PlayMenuMusic()
        {
            if (menuMusic != null)
            {
                PlayMusic(menuMusic);
            }
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (backgroundMusic != null)
            {
                PlayMusic(backgroundMusic);
            }
        }
    }
}
