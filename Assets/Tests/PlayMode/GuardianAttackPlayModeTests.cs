using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianAttackPlayModeTests
    {
        [UnityTest]
        public IEnumerator GuardianAttack_TelegraphsBeforePlayerReset()
        {
            yield return LoadIsolatedEncounter();
            GuardianAI guardian = Object.FindFirstObjectByType<GuardianAI>();
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Vector3 checkpoint = player.transform.position;

            guardian.DebugBeginAttack(player.transform);
            yield return new WaitForSeconds(.3f);

            Assert.That(Vector3.Distance(player.transform.position, checkpoint), Is.LessThan(.01f));
            Assert.That(guardian.AttackPhase, Is.EqualTo(GuardianAttackPhase.Telegraph));
        }

        [UnityTest]
        public IEnumerator GuardianAttack_ClearStrikeResetsPlayerExactlyOnceAfterHitDelay()
        {
            yield return LoadIsolatedEncounter();
            GuardianAI guardian = Object.FindFirstObjectByType<GuardianAI>();
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            GameManager manager = Object.FindFirstObjectByType<GameManager>();
            int struckCount = 0;
            int resetCount = 0;
            guardian.PlayerStruck += _ => struckCount++;
            manager.PlayerReset += _ => resetCount++;

            guardian.DebugBeginAttack(player.transform);
            yield return new WaitForSecondsRealtime(1.85f);

            Assert.That(struckCount, Is.EqualTo(1), "A strike must query and publish a hit only once.");
            Assert.That(resetCount, Is.EqualTo(1), "One strike must cause exactly one delayed checkpoint reset.");
        }

        [UnityTest]
        public IEnumerator GuardianAttack_WallObstructionProducesMiss()
        {
            yield return LoadIsolatedEncounter();
            GuardianAI guardian = Object.FindFirstObjectByType<GuardianAI>();
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            GameManager manager = Object.FindFirstObjectByType<GameManager>();
            int struckCount = 0;
            int resetCount = 0;
            guardian.PlayerStruck += _ => struckCount++;
            manager.PlayerReset += _ => resetCount++;
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Attack Test Wall";
            wall.transform.position = new Vector3(1000f, 1f, 1000.6f);
            wall.transform.localScale = new Vector3(2f, 2f, .2f);
            Physics.SyncTransforms();

            guardian.DebugBeginAttack(player.transform);
            yield return new WaitForSecondsRealtime(1.85f);

            Assert.That(struckCount, Is.Zero);
            Assert.That(resetCount, Is.Zero);
            Object.Destroy(wall);
        }

        private static IEnumerator LoadIsolatedEncounter()
        {
            yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
            yield return null;
            GuardianAI guardian = Object.FindFirstObjectByType<GuardianAI>();
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Assert.That(guardian, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            player.enabled = false;
            guardian.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            guardian.transform.SetPositionAndRotation(new Vector3(1000f, 0f, 1000f), Quaternion.identity);
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.SetPositionAndRotation(new Vector3(1000f, 0f, 1001.2f), Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
            yield return null;
        }
    }
}
