using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ExplorerVisualTests
    {
        [Test]
        public void Create_BuildsNamedExplorerSilhouette()
        {
            GameObject player = new GameObject("Player");
            Material cloak = new Material(Shader.Find("Standard"));
            Material trim = new Material(Shader.Find("Standard"));

            GameObject explorer = ExplorerVisual.Create(player.transform, cloak, trim);

            Assert.That(explorer.name, Is.EqualTo("Explorer Visual"));
            Assert.That(explorer.transform.Find("Cloak"), Is.Not.Null);
            Assert.That(explorer.transform.Find("Torso"), Is.Not.Null);
            Assert.That(explorer.transform.Find("Head"), Is.Not.Null);
            Assert.That(explorer.transform.Find("Shoulder Light"), Is.Not.Null);

            Object.DestroyImmediate(explorer);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cloak);
            Object.DestroyImmediate(trim);
        }

        [Test]
        public void Create_DisablesPrimitiveColliders()
        {
            GameObject player = new GameObject("Player");
            Material cloak = new Material(Shader.Find("Standard"));
            Material trim = new Material(Shader.Find("Standard"));

            GameObject explorer = ExplorerVisual.Create(player.transform, cloak, trim);

            foreach (Collider collider in explorer.GetComponentsInChildren<Collider>())
                Assert.That(collider.enabled, Is.False, collider.name + " should not block player movement");

            Object.DestroyImmediate(explorer);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cloak);
            Object.DestroyImmediate(trim);
        }
    }
}
