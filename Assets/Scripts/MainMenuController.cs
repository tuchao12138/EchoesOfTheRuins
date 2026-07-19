using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    public static class MainMenuCopy
    {
        public const string Controls =
            "WASD  MOVE\nMOUSE  LOOK\nSPACE  JUMP\nC  CROUCH / HIDE\nSHIFT  SPRINT\nQ  THROW ECHO STONE\nE  INTERACT\nESC  PAUSE";
        public const string Footer = "MOONLIT CITADEL";
    }

    /// <summary>Self-contained production menu; static scene generation keeps it editable and build-safe.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private SaveService saveService;
        private SaveData saveData;
        private Button continueButton;
        private GameObject modal;
        private Text modalTitle;
        private Text modalBody;
        private GameObject modalActions;
        private Text qualityText;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            saveService = new SaveService();
            saveData = saveService.Load();
            AudioListener.volume = saveData.Settings.MasterVolume;
            BuildInterface();
        }

        private void BuildInterface()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Canvas canvas = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;

            GameObject veil = CreatePanel(canvas.transform, "Moonlit Veil", Vector2.zero, Vector2.zero, new Color(.005f, .015f, .035f, .58f));
            Stretch(veil.GetComponent<RectTransform>());

            GameObject brand = CreatePanel(canvas.transform, "Brand Panel", new Vector2(72f, -74f), new Vector2(820f, 340f), new Color(.008f, .025f, .055f, .86f));
            AnchorTopLeft(brand.GetComponent<RectTransform>());
            CreateText(brand.transform, "ECHOES", new Vector2(44f, -38f), new Vector2(730f, 76f), 54, new Color(.25f, .9f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);
            CreateText(brand.transform, "OF THE RUINS", new Vector2(44f, -108f), new Vector2(730f, 66f), 43, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            CreateText(brand.transform, "A MOONLIT STEALTH EXPEDITION", new Vector2(48f, -192f), new Vector2(700f, 38f), 19, new Color(.62f, .75f, .9f), FontStyle.Normal, TextAnchor.MiddleLeft);
            CreateText(brand.transform, "Recover three energy cores. Outsmart the stone guardians. Escape before the citadel wakes.",
                new Vector2(48f, -242f), new Vector2(690f, 64f), 20, new Color(.86f, .9f, .96f), FontStyle.Normal, TextAnchor.UpperLeft);

            GameObject navigation = CreatePanel(canvas.transform, "Navigation", new Vector2(-76f, -92f), new Vector2(430f, 690f), new Color(.008f, .022f, .048f, .9f));
            AnchorTopRight(navigation.GetComponent<RectTransform>());
            float y = -60f;
            CreateButton(navigation.transform, "NEW GAME", y, ShowNewGameConfirmation); y -= 92f;
            continueButton = CreateButton(navigation.transform, "CONTINUE", y, StartRun); y -= 92f;
            continueButton.interactable = saveService.HasSave;
            CreateButton(navigation.transform, "CONTROLS", y, ShowControls); y -= 92f;
            CreateButton(navigation.transform, "SETTINGS", y, ShowSettings); y -= 92f;
            CreateButton(navigation.transform, "QUIT", y, Quit); y -= 100f;
            CreateText(navigation.transform, MainMenuCopy.Footer, new Vector2(34f, y), new Vector2(360f, 28f), 14,
                new Color(.42f, .56f, .7f), FontStyle.Normal, TextAnchor.MiddleCenter);

            modal = CreatePanel(canvas.transform, "Modal", Vector2.zero, new Vector2(760f, 610f), new Color(.006f, .02f, .045f, .97f));
            AnchorCenter(modal.GetComponent<RectTransform>());
            modalTitle = CreateText(modal.transform, "Title", new Vector2(48f, -48f), new Vector2(664f, 70f), 36,
                new Color(.3f, .9f, 1f), FontStyle.Bold, TextAnchor.MiddleCenter);
            modalBody = CreateText(modal.transform, "Body", new Vector2(74f, -142f), new Vector2(612f, 300f), 22,
                Color.white, FontStyle.Normal, TextAnchor.UpperCenter);
            modalActions = new GameObject("Actions", typeof(RectTransform));
            modalActions.transform.SetParent(modal.transform, false);
            RectTransform actionsRect = modalActions.GetComponent<RectTransform>();
            actionsRect.anchorMin = actionsRect.anchorMax = new Vector2(.5f, 0f);
            actionsRect.pivot = new Vector2(.5f, 0f);
            actionsRect.anchoredPosition = new Vector2(0f, 38f);
            actionsRect.sizeDelta = new Vector2(650f, 88f);
            modal.SetActive(false);
        }

        private void ShowNewGameConfirmation()
        {
            ShowModal("BEGIN A NEW EXPEDITION?", "Current checkpoint and collected cores will be cleared.\nSettings and best scores are preserved.");
            CreateModalButton("BEGIN", -150f, () =>
            {
                GameSettings settings = saveData.Settings;
                int bestScore = saveData.BestScore;
                string bestRank = saveData.BestRank;
                float bestTime = saveData.BestCompletionSeconds;
                saveService.Delete();
                SaveData fresh = SaveData.CreateDefault();
                fresh.Settings = settings;
                fresh.BestScore = bestScore;
                fresh.BestRank = bestRank;
                fresh.BestCompletionSeconds = bestTime;
                saveService.Save(fresh);
                StartRun();
            });
            CreateModalButton("CANCEL", 150f, CloseModal);
        }

        private void ShowControls()
        {
            ShowModal("CONTROLS", MainMenuCopy.Controls);
            CreateModalButton("BACK", 0f, CloseModal);
        }

        private void ShowSettings()
        {
            ShowModal("SETTINGS", "MASTER VOLUME\n\n\nMOUSE SENSITIVITY\n\n\nQUALITY");
            Slider volume = CreateSlider(modal.transform, new Vector2(0f, -226f), 0f, 1f, saveData.Settings.MasterVolume);
            volume.onValueChanged.AddListener(value => { saveData.Settings.MasterVolume = value; AudioListener.volume = value; saveService.Save(saveData); });
            Slider sensitivity = CreateSlider(modal.transform, new Vector2(0f, -322f), .2f, 3f, saveData.Settings.MouseSensitivity);
            sensitivity.onValueChanged.AddListener(value => { saveData.Settings.MouseSensitivity = value; saveService.Save(saveData); });
            Button quality = CreateModalButton("QUALITY", -120f, CycleQuality);
            quality.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120f, 120f);
            qualityText = quality.GetComponentInChildren<Text>();
            UpdateQualityLabel();
            CreateModalButton("BACK", 170f, CloseModal);
        }

        private void CycleQuality()
        {
            int count = Mathf.Max(1, QualitySettings.names.Length);
            saveData.Settings.QualityPreset = (saveData.Settings.QualityPreset + 1) % count;
            QualitySettings.SetQualityLevel(saveData.Settings.QualityPreset, true);
            saveService.Save(saveData);
            UpdateQualityLabel();
        }

        private void UpdateQualityLabel()
        {
            if (qualityText == null) return;
            int index = Mathf.Clamp(saveData.Settings.QualityPreset, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            qualityText.text = QualitySettings.names.Length > 0 ? $"QUALITY  {QualitySettings.names[index].ToUpperInvariant()}" : "QUALITY";
        }

        private void ShowModal(string title, string body)
        {
            foreach (Transform child in modalActions.transform) Destroy(child.gameObject);
            foreach (Slider slider in modal.GetComponentsInChildren<Slider>(true)) Destroy(slider.gameObject);
            modalTitle.text = title;
            modalBody.text = body;
            modal.SetActive(true);
        }

        private void CloseModal() => modal.SetActive(false);
        private void StartRun() => SceneManager.LoadScene("ProductionRuins");
        private static void Quit() { if (Application.isEditor) Debug.Log("Quit requested"); else Application.Quit(); }

        private Button CreateModalButton(string text, float x, UnityEngine.Events.UnityAction action)
        {
            Button button = CreateButton(modalActions.transform, text, 0f, action);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(250f, 64f);
            return button;
        }

        private static Button CreateButton(Transform parent, string text, float y, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = CreatePanel(parent, text + " Button", new Vector2(35f, y), new Vector2(360f, 68f), new Color(.045f, .11f, .18f, .94f));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            Button button = obj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.35f, .92f, 1f);
            colors.pressedColor = new Color(.15f, .62f, .8f);
            colors.disabledColor = new Color(.35f, .4f, .45f, .6f);
            button.colors = colors;
            button.onClick.AddListener(action);
            CreateText(obj.transform, "Label", new Vector2(12f, -8f), new Vector2(336f, 52f), 21, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            obj.transform.Find("Label").GetComponent<Text>().text = text;
            return button;
        }

        private static Slider CreateSlider(Transform parent, Vector2 position, float min, float max, float value)
        {
            GameObject root = new GameObject("Setting Slider", typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500f, 28f);
            Image background = CreatePanel(root.transform, "Background", Vector2.zero, Vector2.zero, new Color(.09f, .16f, .23f, 1f)).GetComponent<Image>();
            Stretch(background.rectTransform);
            Image fill = CreatePanel(root.transform, "Fill", Vector2.zero, Vector2.zero, new Color(.25f, .9f, 1f, 1f)).GetComponent<Image>();
            Stretch(fill.rectTransform);
            Slider slider = root.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = fill;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            return slider;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string content, Vector2 position, Vector2 size, int fontSize, Color color, FontStyle style, TextAnchor alignment)
        {
            GameObject label = new GameObject(content, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.alignment = alignment;
            return text;
        }

        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void AnchorTopLeft(RectTransform rect) { rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f); }
        private static void AnchorTopRight(RectTransform rect) { rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one; }
        private static void AnchorCenter(RectTransform rect) { rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); }
    }
}
