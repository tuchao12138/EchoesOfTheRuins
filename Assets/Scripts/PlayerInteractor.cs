using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public interface IInteractable
    {
        void Interact(PlayerInteractor player);
    }

    public interface IInteractionPromptProvider
    {
        string InteractionPrompt { get; }
    }

    public interface IHoldInteractable
    {
        float HoldDuration { get; }
        bool IsHoldInterrupted { get; }
        void CompleteHold(PlayerInteractor player);
    }

    public enum InteractionState
    {
        None,
        Available,
        Holding,
        Interrupted,
        Completed
    }

    public readonly struct InteractionViewData
    {
        public InteractionViewData(string prompt, float progress01, InteractionState state, Transform target)
        {
            Prompt = prompt ?? string.Empty;
            Progress01 = Mathf.Clamp01(progress01);
            State = state;
            Target = target;
        }

        public string Prompt { get; }
        public float Progress01 { get; }
        public InteractionState State { get; }
        public Transform Target { get; }
    }

    /// <summary>Selects the closest nearby interaction and exposes one consistent E-key prompt.</summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        // Slightly forgiving radius makes prompt discovery reliable around detailed core and gate meshes.
        [SerializeField, Min(.5f)] private float interactionRange = 3.75f;
        private readonly Collider[] overlapBuffer = new Collider[32];
        private readonly List<IInteractable> nearby = new List<IInteractable>();
        private IInteractable current;
        private IHoldInteractable activeHold;
        private CoreActivationModel holdProgress;

        public event Action<string> PromptChanged;
        public event Action<float, string> HoldProgressChanged;
        public event Action<InteractionViewData> InteractionChanged;
        public InteractionViewData CurrentInteraction { get; private set; }
        public string CurrentPrompt => current == null ? string.Empty :
            current is IInteractionPromptProvider provider ? provider.InteractionPrompt : $"E  INTERACT  /  {GetDisplayName(current)}";

        private void Update()
        {
            RefreshCurrent();
            if (current is IHoldInteractable hold)
            {
                UpdateHold(hold);
                return;
            }

            CancelHold();
            if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        }

        public void Register(IInteractable interactable)
        {
            if (interactable == null || nearby.Contains(interactable)) return;
            nearby.Add(interactable);
            RefreshCurrent();
        }

        public void Unregister(IInteractable interactable)
        {
            if (interactable == null) return;
            nearby.Remove(interactable);
            RefreshCurrent();
        }

        public bool TryInteract()
        {
            RefreshCurrent();
            if (current == null) return false;
            if (current is IHoldInteractable hold)
            {
                Publish(CurrentPrompt, 0f, InteractionState.Available, current);
                return false;
            }

            current.Interact(this);
            RefreshCurrent();
            return true;
        }

        private void UpdateHold(IHoldInteractable hold)
        {
            if (!ReferenceEquals(activeHold, hold))
            {
                activeHold = hold;
                holdProgress = new CoreActivationModel(hold.HoldDuration);
            }

            bool held = Input.GetKey(KeyCode.E);
            bool interrupted = hold.IsHoldInterrupted;
            float previousProgress = holdProgress.Progress01;
            bool completed = holdProgress.Tick(Time.deltaTime, held, interrupted);
            HoldProgressChanged?.Invoke(holdProgress.Progress01, CurrentPrompt);
            InteractionState state = interrupted || (!held && previousProgress > 0f)
                ? InteractionState.Interrupted
                : held ? InteractionState.Holding : InteractionState.Available;
            Publish(CurrentPrompt, holdProgress.Progress01, state, current);
            if (!completed) return;
            hold.CompleteHold(this);
            Publish(CurrentPrompt, 1f, InteractionState.Completed, current);
            CancelHold();
            RefreshCurrent();
        }

        private void CancelHold()
        {
            if (activeHold == null) return;
            activeHold = null;
            holdProgress = null;
            HoldProgressChanged?.Invoke(0f, string.Empty);
        }

        private void RefreshCurrent()
        {
            DiscoverNearbyColliders();
            IInteractable previous = current;
            current = null;
            float closestDistance = interactionRange;
            for (int index = nearby.Count - 1; index >= 0; index--)
            {
                IInteractable candidate = nearby[index];
                Component component = candidate as Component;
                if (component == null)
                {
                    nearby.RemoveAt(index);
                    continue;
                }

                float distance = Vector3.Distance(transform.position, component.transform.position);
                if (distance > closestDistance) continue;
                closestDistance = distance;
                current = candidate;
            }

            if (!ReferenceEquals(previous, current))
            {
                CancelHold();
                PromptChanged?.Invoke(CurrentPrompt);
                Publish(CurrentPrompt, 0f, current == null ? InteractionState.None : InteractionState.Available, current);
            }
        }

        private void DiscoverNearbyColliders()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRange, overlapBuffer, ~0, QueryTriggerInteraction.Collide);
            for (int index = 0; index < count; index++)
            {
                Collider collider = overlapBuffer[index];
                if (collider == null) continue;
                foreach (MonoBehaviour behaviour in collider.GetComponentsInParent<MonoBehaviour>(true))
                    if (behaviour is IInteractable interactable && !nearby.Contains(interactable)) nearby.Add(interactable);
                overlapBuffer[index] = null;
            }
        }

        private void Publish(string prompt, float progress, InteractionState state, IInteractable interactable)
        {
            Transform target = (interactable as Component)?.transform;
            CurrentInteraction = new InteractionViewData(prompt, progress, state, target);
            InteractionChanged?.Invoke(CurrentInteraction);
        }

        private static string GetDisplayName(IInteractable interactable)
        {
            Component component = interactable as Component;
            return component == null ? "OBJECT" : component.gameObject.name.ToUpperInvariant();
        }
    }
}
