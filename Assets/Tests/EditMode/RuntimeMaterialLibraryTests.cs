using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RuntimeMaterialLibraryTests
    {
        [Test]
        public void Create_ReturnsAUsableMaterialForTheRuntimeScene()
        {
            Material material = RuntimeMaterialLibrary.Create("test", Color.cyan, true);

            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader, Is.Not.Null);
            Object.DestroyImmediate(material);
        }
    }
}
