using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoesOfTheRuins
{
    /// <summary>Creates the restrained moonlight grade used by the production level.</summary>
    public static class MoonlitPostProcessing
    {
        public static Volume Configure(GameObject host)
        {
            MoonlitLightingProfile lighting = MoonlitLightingProfile.Readable;
            Volume volume = host.GetComponent<Volume>() ?? host.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            VolumeProfile profile = volume.profile;
            profile.name = "Moonlit Ruins Runtime Profile";

            Bloom bloom = GetOrAdd<Bloom>(profile);
            bloom.active = true;
            bloom.threshold.Override(.85f);
            bloom.intensity.Override(.32f);
            bloom.scatter.Override(.62f);

            ColorAdjustments color = GetOrAdd<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(lighting.PostExposure);
            color.contrast.Override(lighting.Contrast);
            color.saturation.Override(-4f);
            color.colorFilter.Override(new Color(.93f, .96f, 1f));

            Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            Vignette vignette = GetOrAdd<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(.14f);
            vignette.smoothness.Override(.42f);

            FilmGrain grain = GetOrAdd<FilmGrain>(profile);
            grain.active = MotionClarityProfile.UseFilmGrain;
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0f);

            MotionBlur motionBlur = GetOrAdd<MotionBlur>(profile);
            motionBlur.active = MotionClarityProfile.UseMotionBlur;
            motionBlur.intensity.Override(0f);

            return volume;
        }

        public static void ConfigureCamera(Camera camera)
        {
            camera.allowHDR = true;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet(out T component) ? component : profile.Add<T>(true);
        }
    }
}
