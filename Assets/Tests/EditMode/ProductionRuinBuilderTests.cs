using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ProductionRuinBuilderTests
    {
        [Test]
        public void CreateModule_CreatesAVisualOnlyRuinInstance()
        {
            Material material = new Material(Shader.Find("Standard"));
            GameObject module = ProductionRuinBuilder.CreateModule(
                "Visual Test",
                QuaterniusRuinAssetId.ArchGothic,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one,
                material);

            Assert.That(module, Is.Not.Null);
            Assert.That(module.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0));
            Assert.That(System.Array.FindAll(module.GetComponentsInChildren<Collider>(true), collider => collider.enabled), Is.Empty);

            Object.DestroyImmediate(module);
            Object.DestroyImmediate(material);
        }
    }
}
