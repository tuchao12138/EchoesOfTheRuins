using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class MasonryWallBuilderTests
    {
        [Test]
        public void Create_ProducesDetailedCombinedWallWithoutGameplayColliders()
        {
            Material material = RuntimeMaterialLibrary.Create("Test Stone", Color.gray, false);
            GameObject wall = MasonryWallBuilder.Create("Test Masonry", Vector3.zero, new Vector3(8f, 4f, 1f), material);
            try
            {
                MeshFilter filter = wall.GetComponent<MeshFilter>();
                Assert.That(filter, Is.Not.Null);
                Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThan(150));
                Assert.That(wall.GetComponent<Renderer>(), Is.Not.Null);
                Assert.That(wall.GetComponentsInChildren<Collider>(), Is.Empty);
                Assert.That(filter.sharedMesh.bounds.size.x, Is.InRange(7.5f, 8.8f));
                Assert.That(filter.sharedMesh.bounds.size.y, Is.InRange(3.7f, 4.6f));
            }
            finally
            {
                Object.DestroyImmediate(wall);
                Object.DestroyImmediate(material);
            }
        }
    }
}
