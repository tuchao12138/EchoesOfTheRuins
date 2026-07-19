using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ObjectiveRuntimeIntegrationTests
    {
        [Test]
        public void TutorialDirector_WiresObjectiveRuntimeToExistingSceneObjects()
        {
            var player = new GameObject("Player").AddComponent<PlayerController>();
            var core = new GameObject("Core 1 - Courtyard").transform;
            var exit = new GameObject("Exit Gate").transform;
            var guardian = new GameObject("Guardian").transform;
            var camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            var tutorial = new GameObject("Tutorial").AddComponent<TutorialDirector>();

            tutorial.Configure(player, core, exit, guardian, camera);

            Assert.That(tutorial.RuntimeDirector, Is.Not.Null);
            Assert.That(tutorial.RuntimeMarker, Is.Not.Null);
            Assert.That(tutorial.RuntimeDirector.Player, Is.EqualTo(player));
            Assert.That(tutorial.RuntimeDirector.FirstCoreTarget, Is.EqualTo(core));
            Assert.That(tutorial.RuntimeDirector.ExitTarget, Is.EqualTo(exit));
            Assert.That(tutorial.RuntimeDirector.GuardianObservationTarget, Is.EqualTo(guardian));
        }
    }
}
