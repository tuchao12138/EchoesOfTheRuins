using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CitadelBackdropBuilderTests
    {
        [Test]
        public void Create_BuildsSplitNorthSouthCurtainsWithOpenRouteAndVariedSkyline()
        {
            Material material = RuntimeMaterialLibrary.Create("Backdrop Test Stone", Color.gray, false);
            GameObject root = CitadelBackdropBuilder.Create(material);

            try
            {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                Assert.That(renderers, Has.Length.EqualTo(14));
                Assert.That(root.transform.Find("North Curtain Left"), Is.Not.Null);
                Assert.That(root.transform.Find("North Curtain Right"), Is.Not.Null);
                Assert.That(root.transform.Find("South Curtain Left"), Is.Not.Null);
                Assert.That(root.transform.Find("South Curtain Right"), Is.Not.Null);
                Assert.That(root.transform.Find("East Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("West Curtain"), Is.Not.Null);
                Assert.That(root.transform.Find("North Crown Left"), Is.Not.Null);
                Assert.That(root.transform.Find("East Crown Right"), Is.Not.Null);

                Bounds northLeft = root.transform.Find("North Curtain Left").GetComponent<Renderer>().bounds;
                Bounds northRight = root.transform.Find("North Curtain Right").GetComponent<Renderer>().bounds;
                Bounds southLeft = root.transform.Find("South Curtain Left").GetComponent<Renderer>().bounds;
                Bounds southRight = root.transform.Find("South Curtain Right").GetComponent<Renderer>().bounds;
                Assert.That(northLeft.max.x, Is.LessThanOrEqualTo(-6.9f));
                Assert.That(northRight.min.x, Is.GreaterThanOrEqualTo(6.9f));
                Assert.That(southLeft.max.x, Is.LessThanOrEqualTo(-6.9f));
                Assert.That(southRight.min.x, Is.GreaterThanOrEqualTo(6.9f));

                foreach (string wall in new[] { "East Curtain", "West Curtain" })
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
