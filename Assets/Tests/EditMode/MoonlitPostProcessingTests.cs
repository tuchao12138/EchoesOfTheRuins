using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoesOfTheRuins.Tests
{
    public sealed class MoonlitPostProcessingTests
    {
        [Test]
        public void Configure_AddsProductionGradeUrpLook()
        {
            GameObject root = new GameObject("Moonlit Look Test");

            try
            {
                MoonlitPostProcessing.Configure(root);

                Volume volume = root.GetComponent<Volume>();
                Assert.That(volume, Is.Not.Null);
                Assert.That(volume.isGlobal, Is.True);
                Assert.That(volume.profile.TryGet(out Bloom bloom), Is.True);
                Assert.That(bloom.intensity.value, Is.GreaterThanOrEqualTo(.25f));
                Assert.That(volume.profile.TryGet(out ColorAdjustments color), Is.True);
                Assert.That(color.postExposure.value, Is.GreaterThan(0f));
                Assert.That(volume.profile.TryGet(out Vignette vignette), Is.True);
                Assert.That(vignette.intensity.value, Is.InRange(.1f, .35f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
