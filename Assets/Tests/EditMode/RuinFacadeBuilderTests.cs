using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RuinFacadeBuilderTests
    {
        [Test]
        public void CreateFacade_CreatesVisibleRenderersWithoutEnabledNavigationColliders()
        {
            Material material = RuntimeMaterialLibrary.Create("facade-test", Color.gray, false);
            GameObject facade = RuinFacadeBuilder.CreateFacade("facade-test", Vector3.zero, Quaternion.identity, Vector3.one, material);

            Assert.That(facade.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0));
            Assert.That(System.Array.FindAll(facade.GetComponentsInChildren<Collider>(true), collider => collider.enabled), Is.Empty);

            Object.DestroyImmediate(facade);
            Object.DestroyImmediate(material);
        }
    }
}
