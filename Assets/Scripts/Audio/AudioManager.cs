using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using WillakeD.CommonPatterns;

namespace Game.Audios
{
    public enum DirectionOptions
    {
        Left,
        Right
    }
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("References")]
        public AudioMixer masterMixerGroup;
        private AudioMixerGroup _musicMixerGroup;
        private AudioMixerGroup _sfxMixerGroup;
        private AudioMixerGroup _dialogueMixerGroup;
        private AudioMixerGroup _uiMixerGroup;
        public const string PARAM_NAME_MUSIC_VOLUME = "MusicVolume";
        public const string PARAM_NAME_SFX_VOLUME = "SFXVolume";
        public const string PARAM_NAME_DIALOGUE_VOLUME = "DialogueVolume";
        public const string PARAM_NAME_UI_VOLUME = "UIVolume";
        // Daigloue and UI are part of sfx, will adjusted and muted the volume all together

        public bool isSfxMuted { get; private set; } = false;
        public bool isMusicMuted { get; private set; } = false;
        public bool isMusicPaused { get; private set; } = false;

        [Header("Settings")]
        public float maxVolume = 0.0f;
        public float minVolume = -60.0f;
        public int sfxSourcePoolSize = 20;
        private AudioSource _musicSource;
        private Queue<AudioSource> _sfxSourcePool;
        private float _cachedSFXVolume = 0.0f;
        private float _cachedMusicVolume = 0.0f;

        public bool IsMusicPlaying { get => _musicSource.isPlaying; }
        public float MusicVolume { get => _cachedMusicVolume; }
        public float MusicVolumePercentage { get => (_cachedMusicVolume + 60.0f) / 60.0f; }
        public float SFXVolume { get => _cachedSFXVolume; }
        public float SFXVolumePercentage { get => (_cachedSFXVolume + 60.0f) / 60.0f; }
        private void Start()
        {
            _musicMixerGroup = masterMixerGroup.FindMatchingGroups("Music")[0];
            _sfxMixerGroup = masterMixerGroup.FindMatchingGroups("SFX")[0];
            _dialogueMixerGroup = masterMixerGroup.FindMatchingGroups("Dialogue")[0];
            _uiMixerGroup = masterMixerGroup.FindMatchingGroups("UI")[0];

            GameObject musicGo = new GameObject();
            musicGo.name = "music";
            musicGo.transform.SetParent(this.transform);
            _musicSource = musicGo.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = _musicMixerGroup;
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            _sfxSourcePool = new Queue<AudioSource>();

            for (int i = 0; i < sfxSourcePoolSize; i++)
            {
                GameObject go = new GameObject();
                go.name = $"sfx_{i}";
                go.transform.SetParent(this.transform);
                AudioSource source = go.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = _sfxMixerGroup;
                source.playOnAwake = false;
                _sfxSourcePool.Enqueue(source);
            }

            masterMixerGroup.SetFloat(PARAM_NAME_MUSIC_VOLUME, maxVolume);
            _cachedMusicVolume = maxVolume;
            masterMixerGroup.SetFloat(PARAM_NAME_SFX_VOLUME, maxVolume);
            _cachedSFXVolume = maxVolume;
        }

        public void SetMusicMuted(bool muted)
        {
            isMusicMuted = muted;

            SetMusicVolume(muted ? minVolume : _cachedMusicVolume);
        }

        public void SetSFXMuted(bool muted)
        {
            isSfxMuted = muted;

            SetSFXVolume(muted ? minVolume : _cachedMusicVolume);
        }

        // 0 ~ -60 db
        public void SetMusicVolume(float volume)
        {
            if (isMusicMuted == false)
            {
                masterMixerGroup.SetFloat(PARAM_NAME_MUSIC_VOLUME, volume);
            }
            _cachedMusicVolume = volume;
        }

        public void SetMusicVolumeByPercentage(float p)
        {
            float percentage = Mathf.Clamp(p, 0.0f, 1.0f);

            float volume = -60 + (60 * percentage);

            masterMixerGroup.SetFloat(PARAM_NAME_MUSIC_VOLUME, volume);

            _cachedMusicVolume = volume;
        }

        // 0 ~ -60 db
        public void SetSFXVolume(float volume)
        {
            if (isSfxMuted == false)
            {
                masterMixerGroup.SetFloat(PARAM_NAME_SFX_VOLUME, volume);
                masterMixerGroup.SetFloat(PARAM_NAME_DIALOGUE_VOLUME, volume);
                masterMixerGroup.SetFloat(PARAM_NAME_UI_VOLUME, volume);
            }
            _cachedSFXVolume = volume;
        }

        public void SetSFXVolumeByPercentage(float p)
        {
            float percentage = Mathf.Clamp(p, 0.0f, 1.0f);

            float volume = -60 + (60 * percentage);

            masterMixerGroup.SetFloat(PARAM_NAME_SFX_VOLUME, volume);
            masterMixerGroup.SetFloat(PARAM_NAME_DIALOGUE_VOLUME, volume);
            masterMixerGroup.SetFloat(PARAM_NAME_UI_VOLUME, volume);

            _cachedSFXVolume = volume;
        }

        public void ResetVolume()
        {
            SetMusicVolume(maxVolume);
            SetSFXVolume(maxVolume);
        }

        public AudioSource BorrowSFXSource()
        {
            return _sfxSourcePool.Dequeue();
        }

        public void ReturnSFXSource(AudioSource source)
        {
            source.outputAudioMixerGroup = _sfxMixerGroup;
            _sfxSourcePool.Enqueue(source);
        }

        public void PlaySFX(AudioClip clip, float volume = 1, float pitch = 1f)
        {
            if (isSfxMuted) return;
            AudioSource source = _sfxSourcePool.Dequeue();
            source.outputAudioMixerGroup = _sfxMixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.panStereo = 0f;
            source.PlayOneShot(clip);

            _sfxSourcePool.Enqueue(source);
        }

        public void PlayUI(AudioClip clip, float volume = 1, float pitch = 1f)
        {
            if (isSfxMuted) return;
            AudioSource source = _sfxSourcePool.Dequeue();
            source.outputAudioMixerGroup = _uiMixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.panStereo = 0f;
            source.PlayOneShot(clip);

            _sfxSourcePool.Enqueue(source);
        }

        public void PlaySFXDirectional(AudioClip clip, DirectionOptions direction, float volume = 1, float pitch = 1f)
        {
            if (isSfxMuted) return;
            AudioSource source = _sfxSourcePool.Dequeue();
            source.outputAudioMixerGroup = _sfxMixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.panStereo =
                direction == DirectionOptions.Left ? -0.75f : 0.75f;
            source.PlayOneShot(clip);

            _sfxSourcePool.Enqueue(source);
        }

        public void PlayMusic(AudioClip music, float volume = 1, float pitch = 1f)
        {
            if (isMusicMuted) return;

            _musicSource.clip = music;
            _musicSource.volume = volume;
            _musicSource.pitch = pitch;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void PauseMusic()
        {
            isMusicPaused = true;
            _musicSource.Pause();
        }

        public void UnpuaseMusic()
        {
            isMusicPaused = false;
            _musicSource.UnPause();
        }

        private int _incremental = 0;
        public Dictionary<int, AudioSource> _playingSFXLoop = new();
        public int PlaySFXLoop(AudioClip clip, float volume = 1, float pitch = 1f)
        {
            if (isSfxMuted) return -1;
            AudioSource source = _sfxSourcePool.Dequeue();
            source.outputAudioMixerGroup = _sfxMixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.panStereo = 0f;
            source.clip = clip;
            source.loop = true;
            source.Play();

            _incremental++;

            _playingSFXLoop.Add(_incremental, source);

            return _incremental;
        }

        public bool StopSFXLoop(int key)
        {
            if (_playingSFXLoop.TryGetValue(key, out AudioSource value))
            {
                value.Stop();
                value.loop = false;
                value.clip = null;
                _sfxSourcePool.Enqueue(value);
                return true;
            }

            return false;
        }
    }
}