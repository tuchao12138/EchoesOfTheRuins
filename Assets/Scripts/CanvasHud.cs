using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    /// <summary>Event-driven gameplay HUD, interaction progress and completed-run summary.</summary>
    public sealed class CanvasHud : MonoBehaviour
    {
        private Text objectiveText, coreText, threatText, directionText, targetText, tutorialText, interactionText, feedbackText, resultText;
        private Image threatFill, holdFill;
        private Image[] echoPips;
        private GameObject tutorialPanel, interactionPanel, feedbackPanel, resultPanel;
        private Coroutine telegraphPulse, feedbackRoutine;
        private bool escapePhaseActive;
        private ObjectiveDirector objectives;
        private ThreatCoordinator threat;
        private TutorialDirector tutorial;
        private PlayerController player;
        private PlayerInteractor interactor;
        private ObjectiveData currentObjective;

        public void Configure(GuardianAI _) { }

        private void Awake()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject objective = Panel("Objective Zone", new Vector2(28, -28), new Vector2(520, 152), new Vector2(0, 1));
            objectiveText = Label(objective.transform, "Objective", "MISSION / 任务", new Vector2(16, -12), new Vector2(488, 62), 21, TextAnchor.UpperLeft);
            coreText = Label(objective.transform, "Core Progress", "CORES  0 / 3", new Vector2(16, -82), new Vector2(488, 28), 19, TextAnchor.UpperLeft);
            Label(objective.transform, "Mission Rule", "3 CORES  →  NORTH EXIT  →  PRESS E", new Vector2(16, -118), new Vector2(488, 22), 14, TextAnchor.UpperLeft).color = new Color(.4f, .9f, 1f);

            GameObject danger = Panel("Threat Zone", new Vector2(0, -28), new Vector2(440, 64), new Vector2(.5f, 1));
            threatText = Label(danger.transform, "Threat State", "HIDDEN", new Vector2(16, -8), new Vector2(356, 25), 17, TextAnchor.MiddleCenter);
            directionText = Label(danger.transform, "Threat Direction", "", new Vector2(376, -8), new Vector2(48, 25), 15, TextAnchor.MiddleCenter);
            threatFill = Panel(danger.transform, "Threat Bar", new Vector2(16, -46), Vector2.zero, new Vector2(0, 1)).GetComponent<Image>();

            GameObject target = Panel("Target Direction", new Vector2(-28, 0), new Vector2(230, 78), new Vector2(1, .5f));
            targetText = Label(target.transform, "Target", "NEXT CORE", new Vector2(12, -8), new Vector2(206, 60), 17, TextAnchor.MiddleCenter);
            targetText.color = new Color(.3f, .95f, 1f);

            tutorialPanel = Panel("Tutorial Strip", new Vector2(0, -102), new Vector2(700, 56), new Vector2(.5f, 1));
            tutorialText = Label(tutorialPanel.transform, "Tutorial", "", new Vector2(16, -8), new Vector2(668, 40), 17, TextAnchor.MiddleCenter);

            feedbackPanel = Panel("Mission Feedback", new Vector2(0, -174), new Vector2(760, 64), new Vector2(.5f, 1));
            feedbackText = Label(feedbackPanel.transform, "Feedback", "", new Vector2(18, -8), new Vector2(724, 48), 18, TextAnchor.MiddleCenter);
            feedbackText.color = new Color(.3f, .95f, 1f);
            feedbackPanel.SetActive(false);

            interactionPanel = Panel("Interaction Zone", new Vector2(0, 92), new Vector2(620, 62), new Vector2(.5f, 0));
            interactionText = Label(interactionPanel.transform, "Interaction", "", new Vector2(16, -7), new Vector2(588, 32), 18, TextAnchor.MiddleCenter);
            holdFill = Panel(interactionPanel.transform, "Hold Progress", new Vector2(16, -50), Vector2.zero, new Vector2(0, 1)).GetComponent<Image>();
            holdFill.color = new Color(.2f, .9f, 1f);
            interactionPanel.SetActive(false);

            GameObject echoes = Panel("Echo Zone", new Vector2(-28, 28), new Vector2(156, 52), new Vector2(1, 0));
            echoPips = new Image[3];
            for (int i = 0; i < 3; i++)
                echoPips[i] = Panel(echoes.transform, "Echo Pip " + (i + 1), new Vector2(16 + i * 44, -12), new Vector2(28, 28), new Vector2(0, 1)).GetComponent<Image>();

            CreateResultsPanel();
        }

        private void Start()
        {
            objectiveText.text = "MISSION";
            Text missionRule = transform.Find("Objective Zone/Mission Rule")?.GetComponent<Text>();
            if (missionRule != null) missionRule.text = HudCopy.MissionRule;
            objectives = ObjectiveDirector.Active ?? FindFirstObjectByType<ObjectiveDirector>();
            threat = FindFirstObjectByType<ThreatCoordinator>();
            if (threat == null) threat = new GameObject("Threat Coordinator").AddComponent<ThreatCoordinator>();
            if (objectives != null)
            {
                objectives.ObjectiveChanged += OnObjective;
                objectives.RouteHint += OnRouteHint;
                if (objectives.Tracker != null) OnObjective(objectives.Tracker.Current);
            }
            threat.Changed += OnThreat;
            OnThreat(threat.Current);

            Transform playerTransform = GameManager.Instance == null ? null : GameManager.Instance.PlayerTransform;
            player = playerTransform == null ? null : playerTransform.GetComponent<PlayerController>();
            interactor = playerTransform == null ? null : playerTransform.GetComponent<PlayerInteractor>();
            if (player != null)
            {
                player.EchoStoneCountChanged += OnEchoes;
                OnEchoes(player.EchoStoneCount);
            }
            if (interactor != null)
            {
                interactor.InteractionChanged += OnInteraction;
                OnInteraction(interactor.CurrentInteraction);
            }

            tutorial = TutorialDirector.Active ?? FindFirstObjectByType<TutorialDirector>();
            if (tutorial != null)
            {
                tutorial.PresentationChanged += OnTutorialPresentation;
                OnTutorialPresentation(tutorial.Stage, tutorial.SafeEntryRemaining);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged += OnCoreCount;
                GameManager.Instance.CoreFeedback += OnCoreFeedback;
                GameManager.Instance.RunPhaseChanged += OnRunPhase;
                GameManager.Instance.Victory += OnVictory;
                OnCoreCount(GameManager.Instance.GameState.CollectedCoreCount, GameManager.Instance.RequiredCoreCount);
                OnRunPhase(GameManager.Instance.CurrentRunPhase);
            }
        }

        private void OnTutorialPresentation(TutorialStage stage, float safeEntryRemaining)
        {
            if (escapePhaseActive)
            {
                tutorialPanel.SetActive(false);
                return;
            }
            tutorialText.text = safeEntryRemaining > 0f
                ? "SAFE ENTRY  " + Mathf.CeilToInt(safeEntryRemaining) + "s  |  " + TutorialCopy.Get(stage)
                : TutorialCopy.Get(stage);
            tutorialPanel.SetActive(stage != TutorialStage.Complete && (safeEntryRemaining > 0f || stage != TutorialStage.Objective));
        }

        private void OnObjective(ObjectiveData data)
        {
            currentObjective = data;
            objectiveText.text = HudCopy.Objective(data);
            if (data.ProgressRequired > 0)
                coreText.text = "CORES  " + data.ProgressCurrent + " / " + data.ProgressRequired;
            if (data.Stage == ObjectiveStage.CollectCores || data.Stage == ObjectiveStage.ReachExit || data.Stage == ObjectiveStage.Complete)
                tutorialPanel.SetActive(false);
        }

        private void Update()
        {
            if (targetText == null || currentObjective == null || player == null || !currentObjective.ShowDistance)
            {
                if (targetText != null) targetText.transform.parent.gameObject.SetActive(false);
                return;
            }

            targetText.transform.parent.gameObject.SetActive(true);
            Vector3 offset = currentObjective.WorldPosition - player.transform.position;
            string label = string.IsNullOrWhiteSpace(currentObjective.TargetLabel) ? "OBJECTIVE" : currentObjective.TargetLabel;
            targetText.text = label + "  " + Direction(offset) + "\n" + Mathf.RoundToInt(offset.magnitude) + " m";
        }

        private void OnCoreCount(int count, int required) => coreText.text = "CORES  " + count + " / " + required;

        private void OnRouteHint(string message) => ShowFeedback(message, 5f);

        private void OnCoreFeedback(int count, string zone)
        {
            if (Application.isPlaying)
            {
                string clearMessage = count switch
                {
                    1 => "CORE 1/3 - COURTYARD SEAL WEAKENED",
                    2 => "CORE 2/3 - ALTAR ROUTE OPEN. GUARDIANS SEARCHING.",
                    _ => "CORE 3/3 - EXIT UNSEALED. ESCAPE NORTH."
                };
                ShowFeedback(clearMessage + "  /  " + zone.Replace('_', ' '), count == 3 ? 4.5f : 3f);
                return;
            }
            string message = count switch
            {
                1 => "CORE 1/3 — COURTYARD SEAL WEAKENED",
                2 => "CORE 2/3 — ALTAR ROUTE OPEN. GUARDIANS SEARCHING.",
                _ => "CORE 3/3 — EXIT UNSEALED. ESCAPE NORTH."
            };
            ShowFeedback(message + "  /  " + zone.Replace('_', ' '), count == 3 ? 4.5f : 3f);
        }

        private void OnRunPhase(RunPhase phase)
        {
            if (phase == RunPhase.Escape)
            {
                escapePhaseActive = true;
                tutorialPanel.SetActive(false);
                objectiveText.text = HudCopy.EscapeObjective;
                ShowFeedback("ESCAPE PHASE - BOTH GUARDIANS ARE INTERCEPTING", 5f);
                return;
            }
            if (phase != RunPhase.Escape) return;
            escapePhaseActive = true;
            tutorialPanel.SetActive(false);
            objectiveText.text = "ESCAPE NORTH  /  前往北门\nREACH THE CYAN BEACON AND PRESS E";
            ShowFeedback("ESCAPE PHASE — BOTH GUARDIANS ARE INTERCEPTING", 5f);
        }

        private void OnThreat(ThreatSnapshot snapshot)
        {
            bool telegraph = snapshot.AttackPhase == GuardianAttackPhase.Telegraph;
            Color color = telegraph ? Color.red : snapshot.Suspicion >= .7f ? new Color(1, .55f, .15f) : new Color(.25f, .85f, 1);
            threatText.text = (snapshot.AttackPhase == GuardianAttackPhase.None ? snapshot.State.ToString() : snapshot.AttackPhase.ToString()).ToUpperInvariant() + "  " + Mathf.RoundToInt(snapshot.Suspicion * 100) + "%";
            threatText.color = directionText.color = threatFill.color = color;
            directionText.text = Direction(snapshot.Direction);
            threatFill.rectTransform.sizeDelta = new Vector2(408 * snapshot.Suspicion, 5);
            if (telegraph && telegraphPulse == null) telegraphPulse = StartCoroutine(PulseTelegraph());
            else if (!telegraph && telegraphPulse != null)
            {
                StopCoroutine(telegraphPulse);
                telegraphPulse = null;
                threatText.enabled = directionText.enabled = true;
            }
        }

        private IEnumerator PulseTelegraph()
        {
            while (true)
            {
                threatText.enabled = directionText.enabled = !threatText.enabled;
                yield return new WaitForSeconds(.16f);
            }
        }

        private void OnPrompt(string value)
        {
            interactionText.text = value;
            interactionPanel.SetActive(!string.IsNullOrWhiteSpace(value));
        }

        private void OnHoldProgress(float progress, string prompt)
        {
            if (!string.IsNullOrWhiteSpace(prompt)) OnPrompt(prompt);
            holdFill.rectTransform.sizeDelta = new Vector2(588f * Mathf.Clamp01(progress), 6f);
        }

        private void OnInteraction(InteractionViewData view)
        {
            if (view.State == InteractionState.Interrupted)
            {
                OnPrompt(view.Prompt + "\n" + HudCopy.InteractionInterrupted);
                holdFill.rectTransform.sizeDelta = new Vector2(588f * view.Progress01, 6f);
                holdFill.color = new Color(1f, .3f, .2f);
                return;
            }
            string prompt = view.State == InteractionState.Interrupted
                ? view.Prompt + "\nINTERRUPTED — BREAK LINE OF SIGHT / 已中断"
                : view.Prompt;
            OnPrompt(prompt);
            holdFill.rectTransform.sizeDelta = new Vector2(588f * view.Progress01, 6f);
            holdFill.color = view.State == InteractionState.Interrupted ? new Color(1f, .3f, .2f) : new Color(.2f, .9f, 1f);
        }

        private void OnEchoes(int count)
        {
            for (int i = 0; i < 3; i++)
                echoPips[i].color = i < count ? new Color(.3f, .9f, 1) : new Color(.15f, .2f, .25f);
        }

        private void ShowFeedback(string message, float seconds)
        {
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(message, seconds));
        }

        private IEnumerator ShowFeedbackRoutine(string message, float seconds)
        {
            feedbackText.text = message;
            feedbackPanel.SetActive(true);
            yield return new WaitForSeconds(seconds);
            feedbackPanel.SetActive(false);
            feedbackRoutine = null;
        }

        private void OnVictory()
        {
            if (Application.isPlaying)
            {
                GameManager liveManager = GameManager.Instance;
                RunStats liveStats = liveManager.LastRunStats ?? new RunStats();
                resultText.text = HudCopy.ResultSummary(liveManager.LastScoreResult.Rank, liveManager.LastScoreResult.Score, liveStats);
                resultPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }
            GameManager manager = GameManager.Instance;
            RunStats stats = manager.LastRunStats ?? new RunStats();
            resultText.text = "ESCAPED THE RUINS  /  成功逃离\n\n" +
                $"RANK  {manager.LastScoreResult.Rank}     SCORE  {manager.LastScoreResult.Score}\n" +
                $"TIME  {FormatTime(stats.CompletionSeconds)}\n" +
                $"ALERTS  {stats.Alerts}     CAPTURES  {stats.Captures}\n" +
                $"ECHO STONES  {stats.EchoStonesUsed}     RELICS  {stats.RelicsCollected}/2";
            resultPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void CreateResultsPanel()
        {
            resultPanel = Panel("Run Results", Vector2.zero, new Vector2(760, 520), new Vector2(.5f, .5f));
            resultPanel.GetComponent<Image>().color = new Color(.01f, .025f, .055f, .96f);
            resultText = Label(resultPanel.transform, "Results Text", "", new Vector2(40, -38), new Vector2(680, 300), 24, TextAnchor.UpperCenter);
            CreateButton(resultPanel.transform, "RETRY", new Vector2(54, 54), () => SceneManager.LoadScene("ProductionRuins"));
            CreateButton(resultPanel.transform, "MAIN MENU", new Vector2(394, 54), () => SceneManager.LoadScene("MainMenu"));
            resultPanel.SetActive(false);
            if (EventSystem.current == null)
            {
                GameObject events = new GameObject("Gameplay Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                events.transform.SetParent(transform, false);
            }
        }

        private static string FormatTime(float seconds) => $"{Mathf.FloorToInt(seconds / 60f):00}:{Mathf.FloorToInt(seconds % 60f):00}";
        private static string LocalizedObjective(ObjectiveData data) => data.Stage switch
        {
            ObjectiveStage.Move => "MOVE & SPRINT / 移动并疾跑\nWASD + LEFT SHIFT",
            ObjectiveStage.Observe => "OBSERVE THE GUARDIAN / 观察守卫\nAVOID ITS VISION CONE",
            ObjectiveStage.Hide => "ENTER DEEP SHADOW / 进入阴影\nHOLD C TO CROUCH",
            ObjectiveStage.Distract => "DISTRACT THE GUARDIAN / 调离守卫\nPRESS Q TO THROW AN ECHO STONE",
            ObjectiveStage.CollectCores => $"COLLECT 3 CORES / 收集核心\n{data.ProgressCurrent}/3 — HOLD E 1.5s AT EACH CORE",
            ObjectiveStage.ReachExit => "ESCAPE NORTH / 从北门逃离\nFOLLOW THE CYAN MARKER AND PRESS E",
            ObjectiveStage.Complete => "ESCAPED / 已逃离",
            _ => "MISSION BRIEFING / 任务简报\nCOLLECT 3 CORES, THEN ESCAPE NORTH"
        };
        private static string Direction(Vector3 d) => d.sqrMagnitude < .01f ? "" : Mathf.Abs(d.x) > Mathf.Abs(d.z) ? (d.x < 0 ? "LEFT" : "RIGHT") : (d.z > 0 ? "UP" : "DOWN");

        private void OnDestroy()
        {
            if (objectives != null) objectives.ObjectiveChanged -= OnObjective;
            if (objectives != null) objectives.RouteHint -= OnRouteHint;
            if (threat != null) threat.Changed -= OnThreat;
            if (tutorial != null) tutorial.PresentationChanged -= OnTutorialPresentation;
            if (player != null) player.EchoStoneCountChanged -= OnEchoes;
            if (interactor != null)
            {
                interactor.InteractionChanged -= OnInteraction;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged -= OnCoreCount;
                GameManager.Instance.CoreFeedback -= OnCoreFeedback;
                GameManager.Instance.RunPhaseChanged -= OnRunPhase;
                GameManager.Instance.Victory -= OnVictory;
            }
        }

        private GameObject Panel(string name, Vector2 position, Vector2 size, Vector2 anchor) => Panel(transform, name, position, size, anchor);

        private static GameObject Panel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(.015f, .04f, .075f, .84f);
            return go;
        }

        private static Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            return text;
        }

        private static void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject go = Panel(parent, label + " Button", position, new Vector2(312, 64), Vector2.zero);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);
            Label(go.transform, "Label", label, new Vector2(0, 0), new Vector2(312, 64), 19, TextAnchor.MiddleCenter);
        }
    }
}
