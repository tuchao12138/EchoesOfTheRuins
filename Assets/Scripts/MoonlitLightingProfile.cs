using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>
    /// Central colour and exposure contract for the production level. Values deliberately keep the
    /// stealth mood while retaining enough mid-tone information for ordinary laptop displays.
    /// </summary>
    public readonly struct MoonlitLightingProfile
    {
        public readonly Color AmbientSky;
        public readonly Color AmbientEquator;
        public readonly Color AmbientGround;
        public readonly Color FogColor;
        public readonly Color WarmLight;
        public readonly Color CyanLight;
        public readonly float AmbientIntensity;
        public readonly float MoonIntensity;
        public readonly float EntryFillIntensity;
        public readonly float CourtyardFillIntensity;
        public readonly float PostExposure;
        public readonly float Contrast;

        private MoonlitLightingProfile(
            Color ambientSky,
            Color ambientEquator,
            Color ambientGround,
            Color fogColor,
            Color warmLight,
            Color cyanLight,
            float ambientIntensity,
            float moonIntensity,
            float entryFillIntensity,
            float courtyardFillIntensity,
            float postExposure,
            float contrast)
        {
            AmbientSky = ambientSky;
            AmbientEquator = ambientEquator;
            AmbientGround = ambientGround;
            FogColor = fogColor;
            WarmLight = warmLight;
            CyanLight = cyanLight;
            AmbientIntensity = ambientIntensity;
            MoonIntensity = moonIntensity;
            EntryFillIntensity = entryFillIntensity;
            CourtyardFillIntensity = courtyardFillIntensity;
            PostExposure = postExposure;
            Contrast = contrast;
        }

        public static MoonlitLightingProfile Readable => new MoonlitLightingProfile(
            new Color(.24f, .31f, .48f),
            new Color(.15f, .20f, .32f),
            new Color(.075f, .09f, .14f),
            new Color(.12f, .17f, .27f),
            new Color(1f, .50f, .18f),
            new Color(.05f, .84f, 1f),
            1.08f,
            .96f,
            1.35f,
            .92f,
            .52f,
            6f);
    }
}
