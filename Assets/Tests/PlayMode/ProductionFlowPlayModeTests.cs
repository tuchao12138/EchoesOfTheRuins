using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ProductionFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainMenu_LoadsWithActiveCameraAndNavigationCanvas()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;

            MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();
            Camera camera = Camera.main;

            Assert.That(menu, Is.Not.Null, "Main menu controller was not loaded.");
            Assert.That(camera, Is.Not.Null.And.Property("isActiveAndEnabled").True,
                "The main menu must always have an active camera.");
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null,
                "The main menu controller did not create its navigation canvas.");
            Assert.That(EventSystem.current, Is.Not.Null.And.Property("isActiveAndEnabled").True,
                "The menu cannot receive pointer clicks without an active EventSystem.");
        }

        [UnityTest]
        public IEnumerator MainMenu_NavigationRemainsInsideSmallWindow()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;
            Canvas.ForceUpdateCanvases();

            RectTransform navigation = GameObject.Find("Navigation").GetComponent<RectTransform>();
            var corners = new Vector3[4];
            navigation.GetWorldCorners(corners);

            Assert.That(corners[0].x, Is.GreaterThanOrEqualTo(0f), "Navigation is clipped on the left.");
            Assert.That(corners[2].x, Is.LessThanOrEqualTo(Screen.width + 1f), "Navigation is clipped on the right.");
            Assert.That(corners[0].y, Is.GreaterThanOrEqualTo(0f), "Navigation is clipped at the bottom.");
            Assert.That(corners[2].y, Is.LessThanOrEqualTo(Screen.height + 1f), "Navigation is clipped at the top.");
        }

        [UnityTest]
        public IEnumerator MainMenu_ControlsButtonRespondsToPointerClick()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;

            GameObject controls = GameObject.Find("CONTROLS Button");
            GameObject modal = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(rect => rect.name == "Modal").gameObject;
            Assert.That(controls, Is.Not.Null);
            Assert.That(modal, Is.Not.Null.And.Property("activeSelf").False);

            var eventData = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(controls, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.That(modal.activeSelf, Is.True, "CONTROLS click did not open the modal panel.");
            Assert.That(modal.transform.Find("Title").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("CONTROLS"));
        }

        [UnityTest]
        public IEnumerator MainMenu_NewGameStartsTheProtectedBriefing()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;

            ExecuteEvents.Execute(GameObject.Find("NEW GAME Button"), new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            yield return null;
            ExecuteEvents.Execute(GameObject.Find("BEGIN Button"), new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "ProductionRuins");
            yield return null;

            TutorialDirector tutorial = Object.FindFirstObjectByType<TutorialDirector>();
            Assert.That(tutorial, Is.Not.Null);
            Assert.That(tutorial.SafeEntryRemaining, Is.GreaterThan(0f));
            Assert.That(GameObject.Find("Tutorial Strip").activeSelf, Is.True);
            Assert.That(GameObject.Find("Objective").GetComponent<UnityEngine.UI.Text>().text,
                Does.Contain("MISSION"), "The production HUD must expose a visible mission heading during the briefing.");
        }

        [UnityTest]
        public IEnumerator ProductionRuins_LoadsCompletePlayableVerticalSlice()
        {
            new SaveService().Delete();
            yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindFirstObjectByType<GameManager>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PlayerController>(), Is.Not.Null);
            Assert.That(Camera.main, Is.Not.Null.And.Property("isActiveAndEnabled").True);
            Assert.That(Object.FindObjectsByType<GuardianAI>(FindObjectsSortMode.None), Has.Length.EqualTo(3));
            Assert.That(Object.FindObjectsByType<Collectible>(FindObjectsInactive.Include, FindObjectsSortMode.None),
                Has.Length.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<ExitGate>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CanvasHud>(), Is.Not.Null);
            Assert.That(GameObject.Find("Core Progress").GetComponent<UnityEngine.UI.Text>().text,
                Is.EqualTo("CORES  0 / 3"),
                "Briefing objectives must not overwrite the live three-core progress with 0 / 0.");

            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            new SaveService().Delete();
        }

        [UnityTest]
        public IEnumerator ProductionRuins_AdvancesWorldMarkerFromEachCoreToTheNorthGate()
        {
            new SaveService().Delete();
            yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
            yield return null;

            GameManager manager = Object.FindFirstObjectByType<GameManager>();
            ObjectiveDirector director = Object.FindFirstObjectByType<ObjectiveDirector>();
            WorldObjectiveMarker marker = Object.FindFirstObjectByType<WorldObjectiveMarker>();
            Assert.That(manager, Is.Not.Null);
            Assert.That(director, Is.Not.Null);
            Assert.That(marker, Is.Not.Null);

            director.Tracker.Advance(ObjectiveStage.CollectCores, 0);
            yield return null;
            Assert.That(marker.Target.name, Does.Contain("Courtyard"));

            Assert.That(manager.CollectCore("courtyard-core"), Is.True);
            yield return null;
            Assert.That(marker.Target.name, Does.Contain("ShadowGallery"));

            Assert.That(manager.CollectCore("shadow-gallery-core"), Is.True);
            yield return null;
            Assert.That(marker.Target.name, Does.Contain("Altar"));

            Assert.That(manager.CollectCore("altar-core"), Is.True);
            yield return null;
            Assert.That(marker.Target.name, Does.Contain("Sealed Exit Gate"));

            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            new SaveService().Delete();
        }

        [UnityTest]
        public IEnumerator ProductionRuins_HudExplainsSafeEntryAndRestoredExitObjective()
        {
            new SaveService().Delete();
            yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
            yield return null;

            TutorialDirector tutorial = Object.FindFirstObjectByType<TutorialDirector>();
            Assert.That(tutorial, Is.Not.Null);
            Assert.That(tutorial.SafeEntryRemaining, Is.GreaterThan(0f));
            Assert.That(GameObject.Find("Tutorial Strip").activeSelf, Is.True);

            GameManager manager = Object.FindFirstObjectByType<GameManager>();
            Assert.That(manager.CollectCore("courtyard-core"), Is.True);
            Assert.That(manager.CollectCore("shadow-gallery-core"), Is.True);
            Assert.That(manager.CollectCore("altar-core"), Is.True);
            yield return null;

            Assert.That(GameObject.Find("Objective").GetComponent<UnityEngine.UI.Text>().text,
                Does.Contain("ESCAPE NORTH"));
            Assert.That(GameObject.Find("Core Progress").GetComponent<UnityEngine.UI.Text>().text,
                Is.EqualTo("CORES  3 / 3"));
            GameObject tutorialStrip = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(rect => rect.name == "Tutorial Strip").gameObject;
            Assert.That(tutorialStrip.activeSelf, Is.False,
                "The tutorial must retire once the escape objective takes over.");
        }

        [UnityTest]
        public IEnumerator ProductionRuins_ThreeCoresRequireExitInteractionBeforeVictory()
        {
            new SaveService().Delete();
            yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
            yield return null;

            GameManager manager = Object.FindFirstObjectByType<GameManager>();
            ExitGate exitGate = Object.FindFirstObjectByType<ExitGate>();
            Assert.That(manager, Is.Not.Null);
            Assert.That(exitGate, Is.Not.Null);
            Assert.That(manager.HasWon, Is.False);

            Assert.That(manager.CollectCore("courtyard-core"), Is.True);
            Assert.That(manager.CollectCore("shadow-gallery-core"), Is.True);
            Assert.That(manager.CollectCore("altar-core"), Is.True);
            yield return null;

            Assert.That(manager.CurrentRunPhase, Is.EqualTo(RunPhase.Escape));
            Assert.That(manager.HasWon, Is.False, "Collecting cores unlocks escape but must not auto-complete the run.");
            Assert.That(exitGate.IsUnlocked, Is.True);
            Assert.That(exitGate.InteractionPrompt, Does.Contain("ESCAPE"));

            exitGate.Interact(null);
            yield return null;

            Assert.That(manager.HasWon, Is.True);
            Assert.That(manager.CurrentRunPhase, Is.EqualTo(RunPhase.Complete));
            Assert.That(manager.LastRunStats, Is.Not.Null);

            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            new SaveService().Delete();
        }
    }
}
