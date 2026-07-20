using NUnit.Framework;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace EchoesOfTheRuins.Tests
{
    public sealed class StaticProductionSceneTests
    {
        [Test]
        public void BuildProductionScene_CreatesCompletePlayableHierarchy()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject manager = RuinSceneBootstrap.BuildProductionScene();
            ReleaseSceneBindings.Ensure(manager.GetComponent<GameManager>());

            Assert.That(manager, Is.Not.Null);
            Assert.That(scene.GetRootGameObjects().Length, Is.GreaterThan(20));
            Assert.That(Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None), Has.Length.EqualTo(3));
            Assert.That(Object.FindObjectsByType<GuardianAI>(FindObjectsSortMode.None), Has.Length.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<PlayerInteractor>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CanvasHud>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<GuardianVisionCone>(FindObjectsSortMode.None), Has.Length.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<PlayerHitResponse>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<ObjectiveDirector>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<ThreatCoordinator>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<WorldObjectiveMarker>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<GuardianAI>(FindObjectsSortMode.None).All(guardian => guardian.GetComponent<UnityEngine.AI.NavMeshAgent>() != null), Is.True);
            Assert.That(Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>(), Is.Not.Null);
            Assert.That(RenderSettings.skybox, Is.Not.Null);
            int backdropCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name.StartsWith("Backdrop ")) backdropCount++;
            Assert.That(backdropCount, Is.GreaterThanOrEqualTo(16));
        }


        [Test]
        public void SavedProductionScene_IsAnEmptyRuntimeBootstrap()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/ProductionRuins.unity", OpenSceneMode.Single);
            int missingScripts = scene.GetRootGameObjects()
                .Sum(root => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root));

            Assert.That(missingScripts, Is.Zero, "The runtime bootstrap scene must not contain stale script references.");
            Assert.That(scene.GetRootGameObjects(), Is.Empty,
                "Gameplay must be generated after loading so Windows level1 remains small and readable.");
        }
    }
}
