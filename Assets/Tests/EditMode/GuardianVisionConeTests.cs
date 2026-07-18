using NUnit.Framework;
using UnityEngine;

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
    }
}
