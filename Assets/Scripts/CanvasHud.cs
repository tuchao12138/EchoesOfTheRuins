using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    /// <summary>Build-safe, readable HUD. Gameplay publishes state; this view only renders it.</summary>
    public sealed class CanvasHud : MonoBehaviour
    {
        private GuardianAI guardian;
        private Text coresText;
        private Text objectiveText;
        private Text alertText;
        private Text tutorialText;
        private TutorialDirector tutorial;

        public void Configure(GuardianAI configuredGuardian) => guardian = configuredGuardian;

        private void Awake()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject topLeft = CreatePanel(transform, "Objective Panel", new Vector2(18f, -18f), new Vector2(440f, 138f), new Vector2(0f, 1f), new Color(.025f, .06f, .12f, .84f));
            CreateText(topLeft.transform, "Game Title", "ECHOES OF THE RUINS", new Vector2(16f, -12f), new Vector2(408f, 30f), 18, new Color(.33f, .9f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);
            coresText = CreateText(topLeft.transform, "Core Count", "CORES  0 / 3", new Vector2(16f, -49f), new Vector2(408f, 27f), 17, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            objectiveText = CreateText(topLeft.transform, "Objective", "OBJECTIVE: FIND THE ENERGY CORES", new Vector2(16f, -80f), new Vector2(408f, 24f), 15, new Color(.82f, .9f, 1f), FontStyle.Normal, TextAnchor.MiddleLeft);
            alertText = CreateText(topLeft.transform, "Alert", "SAFE - WATCH THE BLUE VISION CONE", new Vector2(16f, -106f), new Vector2(408f, 23f), 15, new Color(.38f, .86f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);

            GameObject tutorialPanel = CreatePanel(transform, "Tutorial Panel", new Vector2(0f, -28f), new Vector2(660f, 88f), new Vector2(.5f, 1f), new Color(.02f, .08f, .15f, .88f));
            tutorialText = CreateText(tutorialPanel.transform, "Tutorial Text", string.Empty, new Vector2(18f, -12f), new Vector2(624f, 64f), 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        private void Update()
        {
            if (tutorial == null) tutorial = TutorialDirector.Active;
            if (GameManager.Instance != null)
            {
                int count = GameManager.Instance.GameState.CollectedCoreCount;
                int required = GameManager.Instance.RequiredCoreCount;
                coresText.text = $"CORES  {count} / {required}";
                objectiveText.text = count >= required ? "OBJECTIVE: REACH THE SEALED EXIT" : "OBJECTIVE: FIND THE ENERGY CORES";
            }

            if (tutorial != null)
            {
                tutorialText.text = TutorialCopy(tutorial.Stage);
                tutorialText.transform.parent.gameObject.SetActive(tutorial.Stage != TutorialStage.Complete);
            }

            if (guardian != null)
            {
                HudAlert alert = HudTheme.GetAlert(guardian.CurrentState);
                alertText.text = alert.Message;
                alertText.color = alert.Color;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.material = Resources.Load<Material>("Materials/HudRuntime");
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color, FontStyle style, TextAnchor alignment)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static string TutorialCopy(TutorialStage stage) => stage switch
        {
            TutorialStage.Objective => "OBJECTIVE\nCOLLECT 3 ENERGY CORES AND ESCAPE.",
            TutorialStage.Movement => "MOVE\nWASD TO MOVE. MOUSE TO LOOK. SPACE TO JUMP.",
            TutorialStage.Shadow => "STEALTH\nPRESS C TO CROUCH IN THE SHADOWS.",
            TutorialStage.EchoStone => "ECHO STONE\nPRESS Q TO DISTRACT THE GUARD.",
            TutorialStage.Awareness => "WATCH THE GUARD\nAVOID THE GOLDEN VISION CONE.",
            _ => string.Empty
        };
    }
}
