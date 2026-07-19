using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    /// <summary>Display-only four-zone runtime HUD.</summary>
    public sealed class CanvasHud : MonoBehaviour
    {
        private Text objectiveText, coreText, threatText, directionText, tutorialText, interactionText;
        private Image threatFill;
        private Image[] echoPips;
        private GameObject tutorialPanel, interactionPanel;
        private Coroutine telegraphPulse;
        private ObjectiveDirector objectives;
        private ThreatCoordinator threat;
        private TutorialDirector tutorial;
        private PlayerController player;
        private PlayerInteractor interactor;

        public void Configure(GuardianAI _) { }
        private void Awake()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();
            GameObject objective = Panel("Objective Zone", new Vector2(28, -28), new Vector2(HudLayoutSpec.ObjectivePanelWidth, HudLayoutSpec.ObjectivePanelHeight), new Vector2(0, 1));
            objectiveText = Label(objective.transform, "Objective", "BRIEFING", new Vector2(16, -12), new Vector2(328, 32), 18, TextAnchor.UpperLeft);
            coreText = Label(objective.transform, "Core Progress", "CORES  0 / 3", new Vector2(16, -52), new Vector2(328, 28), 17, TextAnchor.UpperLeft);
            GameObject danger = Panel("Threat Zone", new Vector2(0, -28), new Vector2(440, 64), new Vector2(.5f, 1));
            threatText = Label(danger.transform, "Threat State", "HIDDEN", new Vector2(16, -8), new Vector2(356, 25), 17, TextAnchor.MiddleCenter);
            directionText = Label(danger.transform, "Threat Direction", "", new Vector2(376, -8), new Vector2(48, 25), 15, TextAnchor.MiddleCenter);
            threatFill = Panel(danger.transform, "Threat Bar", new Vector2(16, -46), Vector2.zero, new Vector2(0, 1)).GetComponent<Image>();
            tutorialPanel = Panel("Tutorial Strip", new Vector2(0, -102), new Vector2(HudLayoutSpec.TutorialPanelWidth, HudLayoutSpec.TutorialPanelHeight), new Vector2(.5f, 1));
            tutorialText = Label(tutorialPanel.transform, "Tutorial", "", new Vector2(16, -8), new Vector2(668, 40), 17, TextAnchor.MiddleCenter);
            interactionPanel = Panel("Interaction Zone", new Vector2(0, 92), new Vector2(520, 48), new Vector2(.5f, 0)); interactionText = Label(interactionPanel.transform, "Interaction", "", new Vector2(16, -8), new Vector2(488, 32), 18, TextAnchor.MiddleCenter); interactionPanel.SetActive(false);
            GameObject echoes = Panel("Echo Zone", new Vector2(-28, 28), new Vector2(156, 52), new Vector2(1, 0)); echoPips = new Image[3];
            for (int i = 0; i < 3; i++) echoPips[i] = Panel(echoes.transform, "Echo Pip " + (i + 1), new Vector2(16 + i * 44, -12), new Vector2(28, 28), new Vector2(0, 1)).GetComponent<Image>();
        }
        private void Start()
        {
            objectives = ObjectiveDirector.Active ?? FindFirstObjectByType<ObjectiveDirector>(); threat = FindFirstObjectByType<ThreatCoordinator>(); if (threat == null) threat = new GameObject("Threat Coordinator").AddComponent<ThreatCoordinator>();
            if (objectives != null) { objectives.ObjectiveChanged += OnObjective; if (objectives.Tracker != null) OnObjective(objectives.Tracker.Current); }
            threat.Changed += OnThreat; OnThreat(threat.Current);
            Transform transformPlayer = GameManager.Instance == null ? null : GameManager.Instance.PlayerTransform; player = transformPlayer == null ? null : transformPlayer.GetComponent<PlayerController>(); interactor = transformPlayer == null ? null : transformPlayer.GetComponent<PlayerInteractor>();
            if (player != null) { player.EchoStoneCountChanged += OnEchoes; OnEchoes(player.EchoStoneCount); }
            if (interactor != null) { interactor.PromptChanged += OnPrompt; OnPrompt(interactor.CurrentPrompt); }
            tutorial = TutorialDirector.Active ?? FindFirstObjectByType<TutorialDirector>();
            if (tutorial != null) { tutorial.PresentationChanged += OnTutorialPresentation; OnTutorialPresentation(tutorial.Stage, tutorial.SafeEntryRemaining); }
        }
        private void OnTutorialPresentation(TutorialStage stage, float safeEntryRemaining) { tutorialText.text = safeEntryRemaining > 0f ? "SAFE ENTRY  " + Mathf.CeilToInt(safeEntryRemaining) + "s  |  " + TutorialCopy.Get(stage) : TutorialCopy.Get(stage); tutorialPanel.SetActive(stage != TutorialStage.Complete || safeEntryRemaining > 0f); }
        private void OnObjective(ObjectiveData data) { objectiveText.text = data.Stage == ObjectiveStage.ReachExit ? "REACH THE UNSEALED EXIT" : data.Title.ToUpperInvariant(); coreText.text = "CORES  " + data.ProgressCurrent + " / " + data.ProgressRequired; }
        private void OnThreat(ThreatSnapshot snapshot) { bool telegraph = snapshot.AttackPhase == GuardianAttackPhase.Telegraph; Color color = telegraph ? Color.red : snapshot.Suspicion >= .7f ? new Color(1, .55f, .15f) : new Color(.25f, .85f, 1); threatText.text = (snapshot.AttackPhase == GuardianAttackPhase.None ? snapshot.State.ToString() : snapshot.AttackPhase.ToString()).ToUpperInvariant() + "  " + Mathf.RoundToInt(snapshot.Suspicion * 100) + "%"; threatText.color = directionText.color = threatFill.color = color; directionText.text = Direction(snapshot.Direction); threatFill.rectTransform.sizeDelta = new Vector2(408 * snapshot.Suspicion, 5); if (telegraph) { if (telegraphPulse != null) StopCoroutine(telegraphPulse); telegraphPulse = StartCoroutine(PulseTelegraph()); } else if (telegraphPulse != null) { StopCoroutine(telegraphPulse); telegraphPulse = null; threatText.enabled = directionText.enabled = true; } }
        private IEnumerator PulseTelegraph() { while (true) { threatText.enabled = directionText.enabled = !threatText.enabled; yield return new WaitForSeconds(.16f); } }
        private void OnPrompt(string value) { interactionText.text = value; interactionPanel.SetActive(!string.IsNullOrWhiteSpace(value)); }
        private void OnEchoes(int count) { for (int i = 0; i < 3; i++) echoPips[i].color = i < count ? new Color(.3f, .9f, 1) : new Color(.15f, .2f, .25f); }
        private static string Direction(Vector3 d) => d.sqrMagnitude < .01f ? "" : Mathf.Abs(d.x) > Mathf.Abs(d.z) ? (d.x < 0 ? "LEFT" : "RIGHT") : (d.z > 0 ? "UP" : "DOWN");
        private void OnDestroy() { if (objectives != null) objectives.ObjectiveChanged -= OnObjective; if (threat != null) threat.Changed -= OnThreat; if (tutorial != null) tutorial.PresentationChanged -= OnTutorialPresentation; if (player != null) player.EchoStoneCountChanged -= OnEchoes; if (interactor != null) interactor.PromptChanged -= OnPrompt; }
        private GameObject Panel(string n, Vector2 p, Vector2 s, Vector2 a) => Panel(transform, n, p, s, a);
        private static GameObject Panel(Transform parent, string n, Vector2 p, Vector2 s, Vector2 a) { GameObject go = new GameObject(n, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); RectTransform r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = a; r.anchoredPosition = p; r.sizeDelta = s; go.GetComponent<Image>().color = new Color(.015f, .04f, .075f, .84f); return go; }
        private static Text Label(Transform parent, string n, string v, Vector2 p, Vector2 s, int size, TextAnchor a) { GameObject go = new GameObject(n, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); RectTransform r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = Vector2.up; r.anchoredPosition = p; r.sizeDelta = s; Text text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = v; text.fontSize = size; text.color = Color.white; text.fontStyle = FontStyle.Bold; text.alignment = a; return text; }
    }
}
