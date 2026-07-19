using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianNavigationTests
    {
        [Test]
        public void NavigationReady_IsFalseWhenAgentIsNotOnBakedNavMesh()
        {
            GameObject guardian = new GameObject("Guardian", typeof(NavMeshAgent), typeof(GuardianAI));
            try
            {
                Assert.That(guardian.GetComponent<GuardianAI>().NavigationReady, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(guardian);
            }
        }
    }
}
