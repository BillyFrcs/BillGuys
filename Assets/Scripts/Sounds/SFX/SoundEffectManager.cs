using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Sounds.SFX
{
    public interface ISoundManager
    {
        public void PlaySoundEffect(String soundName, Boolean isPlay);
    }

    [Serializable]
    public class Sound
    {
        [Tooltip("Name Of Sound Effect")] public string name;

        // Audio components
        public AudioClip Clip;
        public AudioMixerGroup Mixer;

        [Range(0f, 1f)] public float volume = 1f;

        [Range(-3f, 3f)] public float pitch = 1f;

        public bool canLoop = false;

        [HideInInspector] public AudioSource SoundEffectsSource;
    }

    public class SoundEffectManager : MonoBehaviour, ISoundManager
    {
        [Tooltip("Sound Effects List")] [SerializeField] private Sound[] _SoundEffects;

        public static SoundEffectManager Instance;

        private void Awake()
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

            foreach (var soundEffect in _SoundEffects)
            {
                soundEffect.SoundEffectsSource = gameObject.AddComponent<AudioSource>();

                soundEffect.SoundEffectsSource.clip = soundEffect.Clip;

                soundEffect.SoundEffectsSource.outputAudioMixerGroup = soundEffect.Mixer;

                soundEffect.SoundEffectsSource.volume = soundEffect.volume;

                soundEffect.SoundEffectsSource.pitch = soundEffect.pitch;

                soundEffect.SoundEffectsSource.loop = soundEffect.canLoop;
            }
        }

        /// <summary>
        /// Play sound effects 
        /// </summary>
        /// <param name="soundName">String</param>
        /// <param name="isPlay">Boolean</param>
        public void PlaySoundEffect(String soundName, Boolean isPlay = false)
        {
            var soundEffect = Array.Find(_SoundEffects, soundItem => soundItem.name == soundName);

            if (isPlay)
            {
                soundEffect.SoundEffectsSource.Play();
            }
            else
            {
                soundEffect.SoundEffectsSource.Stop();
            }
        }
    }
}