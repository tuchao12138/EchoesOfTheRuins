using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class PlayerLocomotionModelTests
    {
        [Test]
        public void CameraYawNinety_ForwardInputMovesWorldRight()
        {
            var model = new PlayerLocomotionModel();
            LocomotionFrame frame = model.Tick(.1f,
                new PlayerInputFrame(Vector2.up, 90f, false, false, false, true));

            Assert.That(frame.WorldDirection.x, Is.EqualTo(1f).Within(.02f));
            Assert.That(Mathf.Abs(frame.WorldDirection.z), Is.LessThan(.02f));
        }

        [Test]
        public void SpeedAcceleratesAndBrakesWithoutInstantChanges()
        {
            var model = new PlayerLocomotionModel(acceleration: 10f, braking: 12f);
            float first = model.Tick(.1f,
                new PlayerInputFrame(Vector2.up, 0f, false, false, false, true)).Speed;
            float second = model.Tick(.1f,
                new PlayerInputFrame(Vector2.up, 0f, false, false, false, true)).Speed;
            float braking = model.Tick(.1f,
                new PlayerInputFrame(Vector2.zero, 0f, false, false, false, true)).Speed;

            Assert.That(first, Is.GreaterThan(0f).And.LessThan(second));
            Assert.That(braking, Is.GreaterThan(0f).And.LessThan(second));
        }

        [Test]
        public void LateralInputFacesMovementDirection()
        {
            var model = new PlayerLocomotionModel(turnDegreesPerSecond: 720f);
            LocomotionFrame frame = model.Tick(.5f,
                new PlayerInputFrame(Vector2.right, 0f, false, false, false, true));

            Assert.That(Mathf.DeltaAngle(frame.FacingYaw, 90f), Is.EqualTo(0f).Within(1f));
        }
    }
}
