using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RuinGameStateTests
    {
        [Test]
        public void CollectCore_AddsANewCoreToTheCount()
        {
            var gameState = new RuinGameState();

            gameState.CollectCore("core-a");

            Assert.That(gameState.CollectedCoreCount, Is.EqualTo(1));
        }

        [Test]
        public void CollectCore_DoesNotCountTheSameCoreTwice()
        {
            var gameState = new RuinGameState();

            gameState.CollectCore("core-a");
            gameState.CollectCore("core-a");

            Assert.That(gameState.CollectedCoreCount, Is.EqualTo(1));
        }

        [Test]
        public void ExitRemainsLockedUntilAllThreeUniqueCoresAreCollected()
        {
            var gameState = new RuinGameState();

            gameState.CollectCore("core-a");
            gameState.CollectCore("core-b");

            Assert.That(gameState.IsExitUnlocked, Is.False);
        }

        [Test]
        public void ExitUnlocksAfterTheThirdUniqueCoreIsCollected()
        {
            var gameState = new RuinGameState();

            gameState.CollectCore("core-a");
            gameState.CollectCore("core-b");
            gameState.CollectCore("core-c");

            Assert.That(gameState.IsExitUnlocked, Is.True);
        }
    }
}
