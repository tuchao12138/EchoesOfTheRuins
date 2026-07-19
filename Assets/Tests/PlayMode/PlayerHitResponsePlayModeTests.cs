using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoesOfTheRuins.Tests
{
    public sealed class PlayerHitResponsePlayModeTests
    {
        private GuardianTestEncounter encounter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            encounter = GuardianTestEncounter.Create();
            Assert.That(encounter.Response, Is.Not.Null,
                "Guardian self-registration must install the player hit response.");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (encounter != null) yield return encounter.Dispose();
        }

        [UnityTest]
        public IEnumerator DisabledResponse_UnsubscribesAndPreventsNewHitHandling()
        {
            int resetCount = 0;
            encounter.Manager.PlayerReset += _ => resetCount++;
            Assert.That(SubscriberCount(encounter.Guardian), Is.EqualTo(1));

            encounter.Response.enabled = false;
            Assert.That(SubscriberCount(encounter.Guardian), Is.Zero);
            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSecondsRealtime(.75f);

            Assert.That(IsInputLocked(encounter.Player), Is.False);
            Assert.That(IsHandlingHit(encounter.Response), Is.False);
            Assert.That(GetVignette(encounter.Response).enabled, Is.False);
            Assert.That(resetCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DisableDuringHit_StopsResetAndAlwaysClearsPlayerFeedback()
        {
            int resetCount = 0;
            encounter.Manager.PlayerReset += _ => resetCount++;
            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSecondsRealtime(.75f);
            Assert.That(IsInputLocked(encounter.Player), Is.True);
            Assert.That(GetVignette(encounter.Response).enabled, Is.True);

            encounter.Response.enabled = false;
            yield return null;

            Assert.That(IsInputLocked(encounter.Player), Is.False);
            Assert.That(IsHandlingHit(encounter.Response), Is.False);
            Assert.That(GetVignette(encounter.Response).enabled, Is.False);
            yield return new WaitForSecondsRealtime(1.05f);
            Assert.That(resetCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyDuringHit_StopsResetAndAlwaysClearsPlayerFeedback()
        {
            int resetCount = 0;
            encounter.Manager.PlayerReset += _ => resetCount++;
            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSecondsRealtime(.75f);
            Image vignette = GetVignette(encounter.Response);
            Assert.That(IsInputLocked(encounter.Player), Is.True);
            Assert.That(vignette.enabled, Is.True);

            Object.Destroy(encounter.Response);
            yield return null;

            Assert.That(IsInputLocked(encounter.Player), Is.False);
            Assert.That(vignette == null || !vignette.enabled, Is.True);
            yield return new WaitForSecondsRealtime(1.05f);
            Assert.That(resetCount, Is.Zero);
        }

        [Test]
        public void RepeatedEnableCycles_DoNotDuplicateGuardianSubscription()
        {
            encounter.Response.enabled = false;
            encounter.Response.enabled = true;
            encounter.Response.enabled = false;
            encounter.Response.enabled = true;

            Assert.That(SubscriberCount(encounter.Guardian), Is.EqualTo(1));
        }

        private static int SubscriberCount(GuardianAI guardian)
        {
            FieldInfo field = typeof(GuardianAI).GetField("PlayerStruck", BindingFlags.Instance | BindingFlags.NonPublic);
            var callback = (System.Delegate)field?.GetValue(guardian);
            return callback?.GetInvocationList().Length ?? 0;
        }

        private static bool IsInputLocked(PlayerController player)
        {
            FieldInfo field = typeof(PlayerController).GetField("inputLocked", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)field.GetValue(player);
        }

        private static bool IsHandlingHit(PlayerHitResponse response)
        {
            FieldInfo field = typeof(PlayerHitResponse).GetField("handlingHit", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)field.GetValue(response);
        }

        private static Image GetVignette(PlayerHitResponse response) => response.GetComponentInChildren<Image>(true);
    }
}
