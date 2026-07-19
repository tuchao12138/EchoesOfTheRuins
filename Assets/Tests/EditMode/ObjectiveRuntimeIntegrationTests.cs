using System.Reflection;
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

        [Test]
        public void TutorialDirector_ConfigurePlayerOnly_ResolvesTargetsBeforeDeferredObjectiveStartup()
        {
            var playerObject = new GameObject("Player");
            var player = playerObject.AddComponent<PlayerController>();
            var coreObject = new GameObject("Core 1 - Courtyard");
            var core = coreObject.AddComponent<Collectible>().transform;
            var exitObject = new GameObject("Exit Gate");
            var exit = exitObject.AddComponent<ExitGate>().transform;
            var guardianObject = new GameObject("Guardian");
            var guardian = guardianObject.AddComponent<GuardianAI>().transform;
            var tutorialObject = new GameObject("Tutorial");
            var director = tutorialObject.AddComponent<ObjectiveDirector>();
            var tutorial = tutorialObject.AddComponent<TutorialDirector>();

            typeof(ObjectiveDirector).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(director, null);
            Assert.That(director.Tracker, Is.Null);

            tutorial.Configure(player);

            Assert.That(director.Player, Is.EqualTo(player));
            Assert.That(director.FirstCoreTarget, Is.EqualTo(core));
            Assert.That(director.ExitTarget, Is.EqualTo(exit));
            Assert.That(director.GuardianObservationTarget, Is.EqualTo(guardian));
            Assert.That(director.Tracker, Is.Not.Null);
            Assert.That(director.Tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Briefing));

            Object.DestroyImmediate(tutorialObject);
            Object.DestroyImmediate(guardianObject);
            Object.DestroyImmediate(exitObject);
            Object.DestroyImmediate(coreObject);
            Object.DestroyImmediate(playerObject);
        }
    }
}
