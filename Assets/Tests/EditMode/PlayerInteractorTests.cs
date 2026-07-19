using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class PlayerInteractorTests
    {
        private sealed class TestInteractable : MonoBehaviour, IInteractable
        {
            public int Interactions { get; private set; }
            public void Interact(PlayerInteractor player) => Interactions++;
        }

        private sealed class TestHoldInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IInteractionPromptProvider
        {
            public int Completions { get; private set; }
            public float HoldDuration => 1.5f;
            public bool IsHoldInterrupted => false;
            public string InteractionPrompt => "HOLD E 1.5s — ATTUNE CORE / 按住 E 激活核心";
            public void Interact(PlayerInteractor player) => Completions++;
            public void CompleteHold(PlayerInteractor player) => Completions++;
        }

        [Test]
        public void TryInteract_ChoosesClosestRegisteredInteractable()
        {
            GameObject playerObject = new GameObject("Player");
            PlayerInteractor player = playerObject.AddComponent<PlayerInteractor>();
            TestInteractable near = new GameObject("Near").AddComponent<TestInteractable>();
            near.transform.position = Vector3.forward;
            TestInteractable far = new GameObject("Far").AddComponent<TestInteractable>();
            far.transform.position = Vector3.forward * 2f;
            player.Register(far);
            player.Register(near);

            bool interacted = player.TryInteract();

            Assert.That(interacted, Is.True);
            Assert.That(near.Interactions, Is.EqualTo(1));
            Assert.That(far.Interactions, Is.Zero);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(near.gameObject);
            Object.DestroyImmediate(far.gameObject);
        }

        [Test]
        public void Unregister_PreventsInteraction()
        {
            GameObject playerObject = new GameObject("Player");
            PlayerInteractor player = playerObject.AddComponent<PlayerInteractor>();
            TestInteractable target = new GameObject("Target").AddComponent<TestInteractable>();
            target.transform.position = Vector3.forward;
            player.Register(target);
            player.Unregister(target);

            Assert.That(player.TryInteract(), Is.False);
            Assert.That(target.Interactions, Is.Zero);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(target.gameObject);
        }


        [Test]
        public void TryInteract_DiscoversNearbyColliderWithoutTriggerRegistration()
        {
            GameObject playerObject = new GameObject("Player");
            PlayerInteractor player = playerObject.AddComponent<PlayerInteractor>();
            GameObject targetObject = new GameObject("Nearby Target", typeof(BoxCollider));
            TestInteractable target = targetObject.AddComponent<TestInteractable>();
            targetObject.transform.position = Vector3.forward;
            try
            {
                Assert.That(player.TryInteract(), Is.True);
                Assert.That(target.Interactions, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void TryInteract_DoesNotBypassHoldRequirement()
        {
            GameObject playerObject = new GameObject("Player");
            PlayerInteractor player = playerObject.AddComponent<PlayerInteractor>();
            TestHoldInteractable target = new GameObject("Core").AddComponent<TestHoldInteractable>();
            target.transform.position = Vector3.forward;
            player.Register(target);
            try
            {
                Assert.That(player.TryInteract(), Is.False);
                Assert.That(target.Completions, Is.Zero);
                Assert.That(player.CurrentInteraction.State, Is.EqualTo(InteractionState.Available));
                Assert.That(player.CurrentInteraction.Prompt, Does.Contain("1.5s"));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(target.gameObject);
            }
        }
    }
}
