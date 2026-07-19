using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CameraCollisionSolverTests
    {
        [Test]
        public void ResolveDistance_StopsCameraBeforeWall()
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0f, 0f, -2f);
            wall.transform.localScale = new Vector3(4f, 4f, .4f);
            Physics.SyncTransforms();

            float distance = CameraCollisionSolver.ResolveDistance(
                Vector3.zero, Vector3.back, 5f, .2f, ~0, .12f, .65f);

            Assert.That(distance, Is.LessThan(2f));
            Assert.That(distance, Is.GreaterThanOrEqualTo(.65f));
            Object.DestroyImmediate(wall);
        }

        [Test]
        public void ResolveDistance_ReturnsDesiredDistanceWhenPathIsClear()
        {
            float distance = CameraCollisionSolver.ResolveDistance(
                Vector3.zero, Vector3.back, 5f, .2f, ~0, .12f, .65f);

            Assert.That(distance, Is.EqualTo(5f).Within(.001f));
        }
    }
}
