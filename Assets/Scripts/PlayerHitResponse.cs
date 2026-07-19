using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    public sealed class PlayerHitResponse : MonoBehaviour
    {
        private readonly HashSet<GuardianAI> guardians = new HashSet<GuardianAI>();
        private readonly HashSet<GuardianAI> subscribedGuardians = new HashSet<GuardianAI>();
        private PlayerController player;
        private GameManager gameManager;
        private Image vignette;
        private bool handlingHit;
        private Coroutine hitRoutine;

        public void Configure(PlayerController controller, GameManager manager)
        {
            player = controller;
            gameManager = manager;
        }

        public void TrackGuardian(GuardianAI guardian)
        {
            if (guardian == null || !guardians.Add(guardian)) return;
            Subscribe(guardian);
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

        private void OnEnable()
        {
            foreach (GuardianAI guardian in guardians) Subscribe(guardian);
        }

        private void OnDisable()
        {
            if (hitRoutine != null) StopCoroutine(hitRoutine);
            hitRoutine = null;
            ClearHitState();
            UnsubscribeAll();
        }

        private void OnPlayerStruck(string guardianId)
        {
            if (!isActiveAndEnabled || handlingHit) return;
            hitRoutine = StartCoroutine(HandleHit(guardianId));
        }

        private IEnumerator HandleHit(string guardianId)
        {
            handlingHit = true;
            player?.SetInputLocked(true);
            player?.GetComponent<CharacterMotionAnimator>()?.Play(AnimationRole.Hit, restart: true);
            if (vignette != null) vignette.enabled = true;
            yield return new WaitForSecondsRealtime(1f);
            gameManager?.ResetPlayerToCheckpoint($"Struck by {guardianId}");
            hitRoutine = null;
            ClearHitState();
        }

        private void OnDestroy()
        {
            if (hitRoutine != null) StopCoroutine(hitRoutine);
            ClearHitState();
            UnsubscribeAll();
        }

        private void Subscribe(GuardianAI guardian)
        {
            if (!isActiveAndEnabled || guardian == null || !subscribedGuardians.Add(guardian)) return;
            guardian.PlayerStruck += OnPlayerStruck;
        }

        private void UnsubscribeAll()
        {
            foreach (GuardianAI guardian in subscribedGuardians)
                if (guardian != null) guardian.PlayerStruck -= OnPlayerStruck;
            subscribedGuardians.Clear();
        }

        private void ClearHitState()
        {
            player?.SetInputLocked(false);
            if (vignette != null) vignette.enabled = false;
            handlingHit = false;
        }
    }
}
