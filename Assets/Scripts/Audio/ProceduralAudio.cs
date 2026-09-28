using System;
using UnityEngine;

namespace CastleAssault.Audio
{
    /// <summary>
    /// Sintetizador procedural de audio que genera clips de sonido retro/arcade en tiempo real.
    /// Permite reproducir todos los efectos sonoros sin requerir assets externos de audio.
    /// </summary>
    public class ProceduralAudio : MonoBehaviour
    {
        public static ProceduralAudio Instance { get; private set; }

        private AudioSource _audioSource;

        private AudioClip _clipArrowWhoosh;
        private AudioClip _clipArrowImpact;
        private AudioClip _clipShieldBlock;
        private AudioClip _clipRockSmash;
        private AudioClip _clipOilSplash;
        private AudioClip _clipCollectible;
        private AudioClip _clipPlayerHurt;
        private AudioClip _clipPlayerDeath;
        private AudioClip _clipShieldActivate;
        private AudioClip _clipRecordFanfare;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;

            GenerateClips();
        }

        private void GenerateClips()
        {
            _clipArrowWhoosh = SynthesizeNoiseSweep(0.2f, 900f, 300f, 0.5f);
            _clipArrowImpact = SynthesizeClang(0.15f, 500f, 2500f, 0.4f);
            _clipShieldBlock = SynthesizeMetallicClang(0.35f, 1400f, 2200f);
            _clipRockSmash = SynthesizeExplosion(0.45f, 120f, 0.8f);
            _clipOilSplash = SynthesizeSquish(0.3f, 200f, 600f);
            _clipCollectible = SynthesizeFanfare(new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.08f);
            _clipPlayerHurt = SynthesizeToneSweep(0.2f, 350f, 120f, 0.6f);
            _clipPlayerDeath = SynthesizeToneSweep(0.6f, 400f, 60f, 0.9f);
            _clipShieldActivate = SynthesizeToneSweep(0.25f, 300f, 880f, 0.4f);
            _clipRecordFanfare = SynthesizeFanfare(new float[] { 440f, 554f, 659f, 880f, 1108f }, 0.12f);
        }

        public void PlayArrowWhoosh() => PlayOneShot(_clipArrowWhoosh, 0.4f);
        public void PlayArrowImpact() => PlayOneShot(_clipArrowImpact, 0.6f);
        public void PlayShieldBlock() => PlayOneShot(_clipShieldBlock, 0.85f);
        public void PlayRockSmash() => PlayOneShot(_clipRockSmash, 0.9f);
        public void PlayOilSplash() => PlayOneShot(_clipOilSplash, 0.7f);
        public void PlayCollectible() => PlayOneShot(_clipCollectible, 0.8f);
        public void PlayPlayerHurt() => PlayOneShot(_clipPlayerHurt, 0.75f);
        public void PlayPlayerDeath() => PlayOneShot(_clipPlayerDeath, 1.0f);
        public void PlayShieldActivate() => PlayOneShot(_clipShieldActivate, 0.6f);
        public void PlayRecordFanfare() => PlayOneShot(_clipRecordFanfare, 0.9f);

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }

        #region Sound Synthesizers
        private static AudioClip SynthesizeMetallicClang(float duration, float f1, float f2)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 12f); // Caída rápida
                float s1 = Mathf.Sin(2f * Mathf.PI * f1 * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * f2 * t);
                float noise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 30f) * 0.4f;

                samples[i] = (s1 * 0.5f + s2 * 0.3f + noise) * env;
            }

            AudioClip clip = AudioClip.Create("ShieldClang", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeToneSweep(float duration, float startFreq, float endFreq, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * currentFreq / sampleRate;

                float env = Mathf.Sin(t * Mathf.PI);
                samples[i] = Mathf.Sin(phase) * env * volume;
            }

            AudioClip clip = AudioClip.Create("ToneSweep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeNoiseSweep(float duration, float startFreq, float endFreq, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Sin(t * Mathf.PI);
                float n = (UnityEngine.Random.value * 2f - 1f) * 0.5f;
                float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(startFreq, endFreq, t) * (i / (float)sampleRate));
                samples[i] = (n * 0.6f + tone * 0.4f) * env * volume;
            }

            AudioClip clip = AudioClip.Create("NoiseSweep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeClang(float duration, float f1, float f2, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 20f);
                float s = Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t);
                samples[i] = s * 0.5f * env * volume;
            }

            AudioClip clip = AudioClip.Create("Clang", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeExplosion(float duration, float startFreq, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 6f);
                float rumble = Mathf.Sin(2f * Mathf.PI * (startFreq * Mathf.Exp(-t * 4f)) * t);
                float noise = (UnityEngine.Random.value * 2f - 1f);
                samples[i] = (rumble * 0.6f + noise * 0.4f) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Explosion", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeSquish(float duration, float f1, float f2)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Sin(t * Mathf.PI);
                float f = Mathf.Lerp(f1, f2, Mathf.Sin(t * 15f) * 0.5f + 0.5f);
                float s = Mathf.Sin(2f * Mathf.PI * f * (i / (float)sampleRate));
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.3f;
                samples[i] = (s * 0.6f + noise) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create("Squish", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip SynthesizeFanfare(float[] notes, float noteDuration)
        {
            int sampleRate = 44100;
            int noteSamples = (int)(noteDuration * sampleRate);
            int totalSamples = noteSamples * notes.Length;
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                float freq = notes[n];
                int offset = n * noteSamples;
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / noteSamples;
                    float env = Mathf.Sin(t * Mathf.PI);
                    float s = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                    samples[offset + i] = s * env * 0.7f;
                }
            }

            AudioClip clip = AudioClip.Create("Fanfare", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
        #endregion
    }
}
