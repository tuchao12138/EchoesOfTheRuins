using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CitadelBackdropBuilderTests
    {
        [Test]
        public void Create_BuildsContinuousCurtainWallsAndVariedSkyline()
        {
            Material material = RuntimeMaterialLibrary.Create("Backdrop Test Stone", Color.gray, false);
            GameObject root = CitadelBackdropBuilder.Create(material);

            try
            {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                Assert.That(renderers, Has.Length.EqualTo(12));
                Assert.That(root.transform.Find("North Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("South Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("East Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("West Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("North Crown Left"), Is.Not.Null);
                Assert.That(root.transform.Find("East Crown Right"), Is.Not.Null);

                foreach (string wall in new[] { "North Curtain", "South Curtain", "East Curtain", "West Curtain" })
                {
                    Bounds bounds = root.transform.Find(wall).GetComponent<Renderer>().bounds;
                    Assert.That(bounds.size.y, Is.GreaterThanOrEqualTo(5.8f));
                    Assert.That(Mathf.Max(bounds.size.x, bounds.size.z), Is.GreaterThanOrEqualTo(58f));
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(material);
            }
        }
    }
}
