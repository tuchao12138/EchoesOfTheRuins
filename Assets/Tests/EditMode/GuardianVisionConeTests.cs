using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianVisionConeTests
    {
        [TestCase(GuardianState.Patrol, 0.2f, 0.18f, 0.55f, 1f)]
        [TestCase(GuardianState.Investigate, 0.6f, 1f, 0.58f, 0.08f)]
        [TestCase(GuardianState.Chase, 1f, 1f, 0.12f, 0.08f)]
        public void ColorForState_MapsStealthStateToReadableConeColor(
            GuardianState state,
            float suspicion,
            float red,
            float green,
            float blue)
        {
            Color color = GuardianVisionCone.ColorForState(state, suspicion);

            Assert.That(color.r, Is.EqualTo(red).Within(.001f));
            Assert.That(color.g, Is.EqualTo(green).Within(.001f));
            Assert.That(color.b, Is.EqualTo(blue).Within(.001f));
        }

        [Test]
        public void BuildConeVertices_StaysInsideConfiguredAngleAndRange()
        {
            const float range = 8f;
            const float angle = 100f;
            Vector3[] vertices = GuardianVisionCone.BuildConeVertices(range, angle, 20);

            Assert.That(vertices[0], Is.EqualTo(Vector3.zero));
            for (int index = 1; index < vertices.Length; index++)
            {
                Vector3 ray = vertices[index];
                Assert.That(ray.magnitude, Is.LessThanOrEqualTo(range + .001f));
                Assert.That(Vector3.Angle(Vector3.forward, ray), Is.LessThanOrEqualTo(angle * .5f + .001f));
            }
        }

        [Test]
        public void ConfigureTransparentMaterial_StandardShaderUsesFadeBlending()
        {
            Shader shader = Shader.Find("Standard");
            Assert.That(shader, Is.Not.Null, "The editor must provide Unity's built-in Standard shader.");
            var material = new Material(shader);
            try
            {
                GuardianVisionCone.ConfigureTransparentMaterial(material);

                Assert.That(material.GetFloat("_Mode"), Is.EqualTo(2f));
                Assert.That(material.GetInt("_SrcBlend"), Is.EqualTo((int)BlendMode.SrcAlpha));
                Assert.That(material.GetInt("_DstBlend"), Is.EqualTo((int)BlendMode.OneMinusSrcAlpha));
                Assert.That(material.GetInt("_ZWrite"), Is.Zero);
                Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
                Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void ConfigureTransparentMaterial_UrpShaderRetainsTransparentSurfaceContract()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Assert.That(shader, Is.Not.Null, "The production renderer requires URP/Lit.");
            var material = new Material(shader);
            try
            {
                GuardianVisionCone.ConfigureTransparentMaterial(material);

                Assert.That(material.GetFloat("_Surface"), Is.EqualTo(1f));
                Assert.That(material.GetFloat("_Blend"), Is.Zero);
                Assert.That(material.GetInt("_SrcBlend"), Is.EqualTo((int)BlendMode.SrcAlpha));
                Assert.That(material.GetInt("_DstBlend"), Is.EqualTo((int)BlendMode.OneMinusSrcAlpha));
                Assert.That(material.GetInt("_ZWrite"), Is.Zero);
                Assert.That(material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"), Is.True);
                Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }
    }
}
