using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CourtyardSetDressingBuilderTests
    {
        [Test]
        public void Create_AddsEightReadableRuinClustersWithoutBlockingNavigation()
        {
            Material stone = RuntimeMaterialLibrary.Create("Dressing Stone", Color.gray, false);
            Material shadow = RuntimeMaterialLibrary.Create("Dressing Shadow", Color.black, false);
            GameObject root = CourtyardSetDressingBuilder.Create(stone, shadow);

            try
            {
                Assert.That(root.transform.childCount, Is.EqualTo(8));
                Assert.That(root.transform.Find("Collapsed Column West"), Is.Not.Null);
                Assert.That(root.transform.Find("Broken Wall Cover West"), Is.Not.Null);
                Assert.That(root.transform.Find("North Window Ruin East"), Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Renderer>(true), Is.Not.Empty);
                foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                    Assert.That(collider.enabled, Is.False, $"Decorative collider remained active on {collider.name}.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(stone);
                Object.DestroyImmediate(shadow);
            }
        }
    }
}
