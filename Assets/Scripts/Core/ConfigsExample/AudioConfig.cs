using UnityEngine;

namespace Game.Services
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Configs/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float voiceVolume = 0.9f;
        
        [Header("Audio Mixer Settings")]
        public string masterMixerParameter = "MasterVolume";
        public string musicMixerParameter = "MusicVolume";
        public string sfxMixerParameter = "SFXVolume";
        public string voiceMixerParameter = "VoiceVolume";
        
        [Header("Audio Pool Settings")]
        public int audioSourcePoolSize = 20;
        public int maxSimultaneousSounds = 32;
        
        [Header("Music Settings")]
        public bool enableMusicFade = true;
        public float musicFadeDuration = 2f;
        public bool loopBackgroundMusic = true;
        
        [Header("SFX Settings")]
        public float sfxMaxDistance = 50f;
        public AnimationCurve sfxFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
        
        public float GetVolumeDecibels(float normalizedVolume)
        {
            return normalizedVolume > 0 ? Mathf.Log10(normalizedVolume) * 20 : -80f;
        }
        
        private void OnValidate()
        {
            audioSourcePoolSize = Mathf.Clamp(audioSourcePoolSize, 5, 100);
            maxSimultaneousSounds = Mathf.Clamp(maxSimultaneousSounds, 8, 64);
            musicFadeDuration = Mathf.Clamp(musicFadeDuration, 0.1f, 10f);
            sfxMaxDistance = Mathf.Clamp(sfxMaxDistance, 1f, 200f);
        }
    }
}