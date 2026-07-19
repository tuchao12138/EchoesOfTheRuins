using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class MoonlitLightingProfileTests
    {
        [Test]
        public void ReadableProfile_PreservesMoonlitMoodWithoutCrushingShadows()
        {
            MoonlitLightingProfile profile = MoonlitLightingProfile.Readable;

            Assert.That(profile.AmbientIntensity, Is.GreaterThanOrEqualTo(1f));
            Assert.That(profile.MoonIntensity, Is.GreaterThanOrEqualTo(.9f));
            Assert.That(profile.AmbientGround.grayscale, Is.GreaterThanOrEqualTo(.07f));
            Assert.That(profile.EntryFillIntensity, Is.GreaterThanOrEqualTo(1.2f));
            Assert.That(profile.PostExposure, Is.GreaterThanOrEqualTo(.45f));
            Assert.That(profile.Contrast, Is.LessThanOrEqualTo(8f));
        }

        [Test]
        public void ReadableProfile_KeepsCyanAndWarmNavigationContrast()
        {
            MoonlitLightingProfile profile = MoonlitLightingProfile.Readable;

            Assert.That(profile.FogColor.b, Is.GreaterThan(profile.FogColor.r));
            Assert.That(profile.WarmLight.r, Is.GreaterThan(profile.WarmLight.b));
            Assert.That(profile.CyanLight.b, Is.GreaterThan(profile.CyanLight.r));
        }
    }
}
