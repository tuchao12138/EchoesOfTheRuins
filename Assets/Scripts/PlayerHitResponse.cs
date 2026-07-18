using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    public sealed class PlayerHitResponse : MonoBehaviour
    {
        private readonly HashSet<GuardianAI> guardians = new HashSet<GuardianAI>();
        private PlayerController player;
        private GameManager gameManager;
        private Image vignette;
        private bool handlingHit;

        public void Configure(PlayerController controller, GameManager manager)
        {
            player = controller;
            gameManager = manager;
        }

        public void TrackGuardian(GuardianAI guardian)
        {
            if (guardian == null || !guardians.Add(guardian)) return;
            guardian.PlayerStruck += OnPlayerStruck;
        }

        private void Awake()
        {
            GameObject canvasObject = new GameObject("Player Hit Vignette", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            GameObject imageObject = new GameObject("Red Vignette", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            vignette = imageObject.GetComponent<Image>();
            vignette.color = new Color(.75f, 0f, 0f, .42f);
            vignette.raycastTarget = false;
            vignette.enabled = false;
        }

        private void OnPlayerStruck(string guardianId)
        {
            if (handlingHit) return;
            StartCoroutine(HandleHit(guardianId));
        }

        private IEnumerator HandleHit(string guardianId)
        {
            handlingHit = true;
            player?.SetInputLocked(true);
            player?.GetComponent<CharacterMotionAnimator>()?.Play(AnimationRole.Hit, restart: true);
            if (vignette != null) vignette.enabled = true;
            yield return new WaitForSecondsRealtime(1f);
            gameManager?.ResetPlayerToCheckpoint($"Struck by {guardianId}");
            player?.SetInputLocked(false);
            if (vignette != null) vignette.enabled = false;
            handlingHit = false;
        }

        private void OnDestroy()
        {
            foreach (GuardianAI guardian in guardians)
                if (guardian != null) guardian.PlayerStruck -= OnPlayerStruck;
        }
    }
}
