using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    /// <summary>Runtime Canvas presentation for the playable build; gameplay scripts publish state and this class renders it.</summary>
    public sealed class CanvasHud : MonoBehaviour
    {
        private GuardianAI guardian;
        private Text coresText;
        private Text objectiveText;
        private Text alertText;
        private Text tutorialText;
        private Text controlsText;
        private PlayerController player;
        private TutorialDirector tutorial;

        public void Configure(GuardianAI configuredGuardian) => guardian = configuredGuardian;

        private void Awake()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            var topLeft = CreatePanel(transform, "Objective Panel", new Vector2(18f, -18f), new Vector2(440f, 138f), new Vector2(0f, 1f), new Color(.025f, .06f, .12f, .84f));
            CreateText(topLeft.transform, "Game Title", "遗迹回响  /  ECHOES OF THE RUINS", new Vector2(16f, -12f), new Vector2(408f, 30f), 18, new Color(.33f, .9f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);
            coresText = CreateText(topLeft.transform, "Core Count", "能量核心  0 / 3", new Vector2(16f, -49f), new Vector2(408f, 27f), 17, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            objectiveText = CreateText(topLeft.transform, "Objective", "目标：收集能量核心", new Vector2(16f, -80f), new Vector2(408f, 24f), 15, new Color(.82f, .9f, 1f), FontStyle.Normal, TextAnchor.MiddleLeft);
            alertText = CreateText(topLeft.transform, "Alert", "安全 / SAFE", new Vector2(16f, -106f), new Vector2(408f, 23f), 15, new Color(.5f, .84f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);

            var tutorialPanel = CreatePanel(transform, "Tutorial Panel", new Vector2(0f, -28f), new Vector2(660f, 88f), new Vector2(.5f, 1f), new Color(.02f, .08f, .15f, .88f));
            tutorialText = CreateText(tutorialPanel.transform, "Tutorial Text", "", new Vector2(18f, -12f), new Vector2(624f, 64f), 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);

            var bottom = CreatePanel(transform, "Controls Panel", new Vector2(18f, 18f), new Vector2(760f, 40f), new Vector2(0f, 0f), new Color(.02f, .05f, .1f, .78f));
            controlsText = CreateText(bottom.transform, "Controls", "", new Vector2(14f, -7f), new Vector2(730f, 27f), 15, new Color(.88f, .95f, 1f), FontStyle.Normal, TextAnchor.MiddleLeft);
        }

        private void Update()
        {
            if (player == null && GameManager.Instance?.PlayerTransform != null) player = GameManager.Instance.PlayerTransform.GetComponent<PlayerController>();
            if (tutorial == null) tutorial = TutorialDirector.Active;
            if (GameManager.Instance != null)
            {
                int count = GameManager.Instance.GameState.CollectedCoreCount;
                int required = GameManager.Instance.RequiredCoreCount;
                coresText.text = $"能量核心  {count} / {required}";
                objectiveText.text = count >= required ? "目标：前往封印出口 / Reach the sealed exit" : "目标：收集发光的青色能量核心";
            }

            if (tutorial != null)
            {
                tutorialText.text = tutorial.Stage == TutorialStage.Complete ? "" : tutorial.Prompt;
                tutorialText.transform.parent.gameObject.SetActive(tutorial.Stage != TutorialStage.Complete);
            }

            if (guardian != null)
            {
                switch (guardian.CurrentState)
                {
                    case GuardianState.Chase:
                    case GuardianState.Capture:
                        alertText.text = "发现 / DETECTED — 立刻脱离视野！";
                        alertText.color = new Color(1f, .25f, .2f);
                        break;
                    case GuardianState.Investigate:
                    case GuardianState.Search:
                        alertText.text = "警戒 / SEARCHING — 保持阴影与掩体";
                        alertText.color = new Color(1f, .8f, .22f);
                        break;
                    default:
                        alertText.text = "安全 / SAFE — 观察蓝色守卫视野";
                        alertText.color = new Color(.5f, .84f, 1f);
                        break;
                }
            }

            if (player != null)
            {
                string stealth = player.IsInShadow ? "阴影 / SHADOW" : "暴露 / EXPOSED";
                if (player.IsCrouching) stealth += "   蹲伏 / CROUCH";
                controlsText.text = $"WASD 移动   鼠标观察   Space 跳跃   C 蹲伏   Shift 疾跑   Q 回响石      {stealth}      回响石 {player.EchoStoneCount}";
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
            panel.GetComponent<Image>().color = color;
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
    }
}
