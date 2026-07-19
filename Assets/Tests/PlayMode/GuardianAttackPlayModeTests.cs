using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianAttackPlayModeTests
    {
        private GuardianTestEncounter encounter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            encounter = GuardianTestEncounter.Create();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return encounter.Dispose();
        }

        [UnityTest]
        public IEnumerator GuardianAttack_TelegraphsBeforePlayerReset()
        {
            Vector3 checkpoint = encounter.Player.transform.position;

            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSeconds(.3f);

            Assert.That(Vector3.Distance(encounter.Player.transform.position, checkpoint), Is.LessThan(.01f));
            Assert.That(encounter.Guardian.AttackPhase, Is.EqualTo(GuardianAttackPhase.Telegraph));
        }

        [UnityTest]
        public IEnumerator GuardianAttack_ClearStrikeResetsPlayerExactlyOnceAfterHitDelay()
        {
            int struckCount = 0;
            int resetCount = 0;
            encounter.Guardian.PlayerStruck += _ => struckCount++;
            encounter.Manager.PlayerReset += _ => resetCount++;

            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSecondsRealtime(1.85f);

            Assert.That(struckCount, Is.EqualTo(1), "A strike must query and publish a hit only once.");
            Assert.That(resetCount, Is.EqualTo(1), "One strike must cause exactly one delayed checkpoint reset.");
        }

        [UnityTest]
        public IEnumerator GuardianAttack_WallObstructionProducesMiss()
        {
            int struckCount = 0;
            int resetCount = 0;
            encounter.Guardian.PlayerStruck += _ => struckCount++;
            encounter.Manager.PlayerReset += _ => resetCount++;
            encounter.CreateWall();

            encounter.Guardian.DebugBeginAttack(encounter.Player.transform);
            yield return new WaitForSecondsRealtime(1.85f);

            Assert.That(struckCount, Is.Zero);
            Assert.That(resetCount, Is.Zero);
        }
    }

    internal sealed class GuardianTestEncounter
    {
        private readonly GameManager previousManager;
        private readonly GameObject testRoot;

        public PlayerController Player { get; private set; }
        public GameManager Manager { get; private set; }
        public GuardianAI Guardian { get; private set; }
        public PlayerHitResponse Response { get; private set; }

        private GuardianTestEncounter(
            GameManager savedManager,
            GameObject root)
        {
            previousManager = savedManager;
            testRoot = root;
        }

        public static GuardianTestEncounter Create()
        {
            GameManager savedManager = GameManager.Instance;
            SetGameManagerInstance(null);
            GameObject testRoot = new GameObject($"Guardian Test {Guid.NewGuid():N}");
            var encounter = new GuardianTestEncounter(savedManager, testRoot);

            GameObject checkpoint = new GameObject("Test Checkpoint");
            checkpoint.transform.SetParent(testRoot.transform);
            checkpoint.transform.position = new Vector3(4f, 0f, 0f);

            GameObject playerObject = new GameObject("Test Player");
            playerObject.transform.SetParent(testRoot.transform);
            playerObject.tag = "Player";
            playerObject.AddComponent<CharacterController>();
            encounter.Player = playerObject.AddComponent<PlayerController>();
            encounter.Player.enabled = false;

            GameObject managerObject = new GameObject("Test Game Manager");
            managerObject.transform.SetParent(testRoot.transform);
            encounter.Manager = managerObject.AddComponent<GameManager>();
            encounter.Manager.Configure(playerObject.transform, checkpoint.transform);

            GameObject guardianObject = new GameObject("Test Guardian");
            guardianObject.transform.SetParent(testRoot.transform);
            guardianObject.transform.SetPositionAndRotation(new Vector3(0f, 0f, -1.2f), Quaternion.identity);
            encounter.Guardian = guardianObject.AddComponent<GuardianAI>();
            encounter.Guardian.Configure(playerObject.transform, Array.Empty<Transform>());
            encounter.Response = playerObject.GetComponent<PlayerHitResponse>();

            Physics.SyncTransforms();
            return encounter;
        }

        public void CreateWall()
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Attack Test Wall";
            wall.transform.SetParent(testRoot.transform);
            wall.transform.position = new Vector3(0f, 1f, -.6f);
            wall.transform.localScale = new Vector3(2f, 2f, .2f);
            Physics.SyncTransforms();
        }

        public IEnumerator Dispose()
        {
            if (testRoot != null) UnityEngine.Object.Destroy(testRoot);
            yield return null;
            SetGameManagerInstance(previousManager);
        }

        private static void SetGameManagerInstance(GameManager manager)
        {
            PropertyInfo property = typeof(GameManager).GetProperty(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            property.SetValue(null, manager);
        }
    }
}
