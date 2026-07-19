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

        [Test]
        public void CreateTextured_BindsTheProvidedTexture()
        {
            Texture2D texture = new Texture2D(2, 2);
            Material material = RuntimeMaterialLibrary.CreateTextured("textured-test", texture);

            Assert.That(material, Is.Not.Null);
            Assert.That(material.mainTexture, Is.SameAs(texture));

            Object.DestroyImmediate(material);
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void Create_UsesUrpLitWhenUrpIsInstalled()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            Assert.That(urpLit, Is.Not.Null);

            Material material = RuntimeMaterialLibrary.Create("urp-test", Color.white, false);

            Assert.That(material.shader, Is.EqualTo(urpLit));
            Object.DestroyImmediate(material);
        }
    }
}
