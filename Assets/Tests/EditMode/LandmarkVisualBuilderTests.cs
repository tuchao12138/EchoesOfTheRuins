using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class LandmarkVisualBuilderTests
    {
        [Test]
        public void CreateCore_BuildsCrystalAndRuneRingsWithoutPrimitiveShapes()
        {
            GameObject root = new GameObject("Core");
            Material crystal = new Material(Shader.Find("Standard"));
            Material rune = new Material(Shader.Find("Standard"));

            LandmarkVisualBuilder.CreateCore(root.transform, crystal, rune);

            Assert.That(root.transform.Find("Crystal"), Is.Not.Null);
            Assert.That(root.transform.Find("Rune Ring A"), Is.Not.Null);
            Assert.That(root.transform.Find("Rune Ring B"), Is.Not.Null);
            Assert.That(root.GetComponentsInChildren<MeshFilter>(true).Length, Is.GreaterThan(0));
            Assert.That(root.GetComponentsInChildren<LineRenderer>(true).Length, Is.EqualTo(2));
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(crystal);
            Object.DestroyImmediate(rune);
        }

        [Test]
        public void CreateCheckpoint_BuildsGroundRunesWithoutCylinderRenderer()
        {
            GameObject root = new GameObject("Checkpoint");
            Material rune = new Material(Shader.Find("Standard"));

            LandmarkVisualBuilder.CreateCheckpoint(root.transform, rune);

            Assert.That(root.GetComponentsInChildren<LineRenderer>(true).Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(root.GetComponentsInChildren<MeshRenderer>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(rune);
        }

        [Test]
        public void CreateGateBarrier_UsesRuneLinesInsteadOfSolidDebugBox()
        {
            GameObject root = new GameObject("Gate");
            Material rune = new Material(Shader.Find("Standard"));

            LandmarkVisualBuilder.CreateGateBarrier(root.transform, rune);

            Assert.That(root.GetComponentsInChildren<LineRenderer>(true).Length, Is.GreaterThanOrEqualTo(7));
            Assert.That(root.GetComponentsInChildren<MeshRenderer>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(rune);
        }
    }
}
