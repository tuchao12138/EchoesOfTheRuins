using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RuinPropVisualBuilderTests
    {
        [Test]
        public void RitualDais_UsesLayeredOctagonalMeshes()
        {
            Material material = RuntimeMaterialLibrary.Create("Dais", Color.gray, false);
            GameObject root = RuinPropVisualBuilder.CreateRitualDais("Test Dais", Vector3.zero, 3f, material);
            try
            {
                Assert.That(root.GetComponentsInChildren<MeshRenderer>(), Has.Length.GreaterThanOrEqualTo(3));
                Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void Brazier_UsesSculptedBowlAndWarmLight()
        {
            Material stone = RuntimeMaterialLibrary.Create("Stone", Color.gray, false);
            Material fire = RuntimeMaterialLibrary.Create("Fire", Color.yellow, true);
            GameObject root = RuinPropVisualBuilder.CreateBrazier("Test Brazier", Vector3.zero, stone, fire);
            try
            {
                Assert.That(root.GetComponentsInChildren<MeshRenderer>(), Has.Length.GreaterThanOrEqualTo(2));
                Assert.That(root.GetComponentInChildren<Light>(), Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(stone);
                Object.DestroyImmediate(fire);
            }
        }
    }
}
