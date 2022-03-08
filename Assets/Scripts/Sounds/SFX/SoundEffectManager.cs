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

            foreach (var SoundEffect in _SoundEffects)
            {
                SoundEffect.SoundEffectsSource = gameObject.AddComponent<AudioSource>();

                SoundEffect.SoundEffectsSource.clip = SoundEffect.Clip;

                SoundEffect.SoundEffectsSource.outputAudioMixerGroup = SoundEffect.Mixer;

                SoundEffect.SoundEffectsSource.volume = SoundEffect.volume;

                SoundEffect.SoundEffectsSource.pitch = SoundEffect.pitch;

                SoundEffect.SoundEffectsSource.loop = SoundEffect.canLoop;
            }
        }

        /// <summary>
        /// Play sound effects 
        /// </summary>
        /// <param name="soundName">String</param>
        /// <param name="isPlay">Boolean</param>
        public void PlaySoundEffect(String soundName, Boolean isPlay = false)
        {
            var SoundEffect = Array.Find(_SoundEffects, soundItem => soundItem.name == soundName);

            if (isPlay)
            {
                SoundEffect.SoundEffectsSource.Play();
            }
            else
            {
                SoundEffect.SoundEffectsSource.Stop();
            }
        }
    }
}