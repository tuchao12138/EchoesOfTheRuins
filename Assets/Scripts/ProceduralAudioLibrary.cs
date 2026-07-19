using System;
using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Small original audio cues generated locally, so the submission has no untracked licence risk.</summary>
    public static class ProceduralAudioLibrary
    {
        private const int SampleRate = 22050;

        public static AudioClip CreatePickup() => Build("Core Pickup", .72f, t =>
        {
            float envelope = Mathf.Sin(Mathf.PI * t);
            float frequency = Mathf.Lerp(420f, 960f, t);
            return Mathf.Sin(t * frequency * Mathf.PI * 2f * .72f) * envelope * .42f;
        });

        public static AudioClip CreateAlert() => Build("Guardian Alert", .9f, t =>
        {
            float pulse = Mathf.Sin(t * 8f * Mathf.PI) > 0f ? 1f : .38f;
            float wave = Mathf.Sin(t * Mathf.Lerp(150f, 235f, t) * Mathf.PI * 2f);
            return wave * pulse * (1f - t) * .4f;
        });

        public static AudioClip CreateUnlock() => Build("Gate Unlock", 1.8f, t =>
        {
            float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t * 1.15f)) * (1f - t * .35f);
            float chord = Mathf.Sin(t * 220f * Mathf.PI * 2f) + Mathf.Sin(t * 330f * Mathf.PI * 2f) * .65f + Mathf.Sin(t * 440f * Mathf.PI * 2f) * .4f;
            return chord * envelope * .18f;
        });

        public static AudioClip CreateWind() => Build("Citadel Wind", 4f, t =>
        {
            float slow = Mathf.Sin(t * Mathf.PI * 4f) * .5f + .5f;
            float texture = Mathf.Sin(t * 73f) * .35f + Mathf.Sin(t * 131f) * .15f;
            return texture * (.12f + slow * .08f);
        });

        public static AudioClip CreateAttackTelegraph() => Build("Attack Telegraph", .62f, t =>
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 16f) > 0f ? 1f : .2f;
            return Mathf.Sin(t * Mathf.Lerp(180f, 310f, t) * Mathf.PI * 2f) * pulse * (1f - t) * .34f;
        });

        public static AudioClip CreateSwordStrike() => Build("Sword Strike", .34f, t =>
        {
            float crack = Mathf.Sin(t * 950f * Mathf.PI * 2f) * (1f - t);
            return crack * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t * 8f)) * .35f;
        });

        public static AudioClip CreatePlayerHit() => Build("Player Hit", .42f, t =>
        {
            float envelope = 1f - t;
            return (Mathf.Sin(t * 92f * Mathf.PI * 2f) + Mathf.Sin(t * 138f * Mathf.PI * 2f) * .4f) * envelope * .3f;
        });

        public static AudioClip CreateObjectivePulse() => Build("Objective Pulse", .48f, t =>
        {
            float envelope = Mathf.Sin(Mathf.PI * t);
            return Mathf.Sin(t * Mathf.Lerp(360f, 610f, t) * Mathf.PI * 2f) * envelope * .22f;
        });

        private static AudioClip Build(string name, float duration, Func<float, float> sample)
        {
            int sampleCount = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++) data[index] = Mathf.Clamp(sample(index / (float)sampleCount), -.95f, .95f);
            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
