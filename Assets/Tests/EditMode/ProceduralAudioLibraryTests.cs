using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ProceduralAudioLibraryTests
    {
        [Test]
        public void GameplayCues_AreNonEmptyAndShort()
        {
            AudioClip pickup = ProceduralAudioLibrary.CreatePickup();
            AudioClip alert = ProceduralAudioLibrary.CreateAlert();
            AudioClip unlock = ProceduralAudioLibrary.CreateUnlock();
            AudioClip telegraph = ProceduralAudioLibrary.CreateAttackTelegraph();
            AudioClip strike = ProceduralAudioLibrary.CreateSwordStrike();
            AudioClip hit = ProceduralAudioLibrary.CreatePlayerHit();
            AudioClip pulse = ProceduralAudioLibrary.CreateObjectivePulse();
            try
            {
                Assert.That(pickup.samples, Is.GreaterThan(1000));
                Assert.That(alert.samples, Is.GreaterThan(1000));
                Assert.That(unlock.samples, Is.GreaterThan(1000));
                Assert.That(telegraph.samples, Is.GreaterThan(1000));
                Assert.That(strike.samples, Is.GreaterThan(1000));
                Assert.That(hit.samples, Is.GreaterThan(1000));
                Assert.That(pulse.samples, Is.GreaterThan(1000));
                Assert.That(pickup.length, Is.LessThan(2f));
                Assert.That(alert.length, Is.LessThan(2f));
                Assert.That(unlock.length, Is.LessThan(3f));
            }
            finally
            {
                Object.DestroyImmediate(pickup);
                Object.DestroyImmediate(alert);
                Object.DestroyImmediate(unlock);
                Object.DestroyImmediate(telegraph);
                Object.DestroyImmediate(strike);
                Object.DestroyImmediate(hit);
                Object.DestroyImmediate(pulse);
            }
        }
    }
}
