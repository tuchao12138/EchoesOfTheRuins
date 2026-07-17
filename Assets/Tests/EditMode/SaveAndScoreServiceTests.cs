using System;
using System.IO;
using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class SaveAndScoreServiceTests
    {
        private string temporaryDirectory;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = Path.Combine(Path.GetTempPath(), "EchoesSaveTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, true);
        }

        [Test]
        public void Load_ReturnsSafeDefaults_WhenSaveIsCorrupt()
        {
            Directory.CreateDirectory(temporaryDirectory);
            File.WriteAllText(Path.Combine(temporaryDirectory, "echoes-save.json"), "not json");

            var loaded = new SaveService(temporaryDirectory).Load();

            Assert.That(loaded.CheckpointId, Is.EqualTo(SaveData.StartCheckpointId));
            Assert.That(loaded.CollectedCoreIds, Is.Empty);
            Assert.That(loaded.Settings.MouseSensitivity, Is.EqualTo(GameSettings.DefaultMouseSensitivity));
        }

        [Test]
        public void SaveAndLoad_PreservesCheckpointProgressAndSettings()
        {
            var service = new SaveService(temporaryDirectory);
            var data = SaveData.CreateDefault();
            data.CheckpointId = "altar";
            data.CollectedCoreIds.Add("moon-core");
            data.CollectedRelicIds.Add("stone-tablet");
            data.BestScore = 960;
            data.BestCompletionSeconds = 84.5f;
            data.Settings.MasterVolume = 0.7f;
            data.Settings.MouseSensitivity = 1.35f;

            service.Save(data);
            var loaded = service.Load();

            Assert.That(loaded.CheckpointId, Is.EqualTo("altar"));
            Assert.That(loaded.CollectedCoreIds, Is.EquivalentTo(new[] { "moon-core" }));
            Assert.That(loaded.CollectedRelicIds, Is.EquivalentTo(new[] { "stone-tablet" }));
            Assert.That(loaded.BestScore, Is.EqualTo(960));
            Assert.That(loaded.BestCompletionSeconds, Is.EqualTo(84.5f).Within(0.001f));
            Assert.That(loaded.Settings.MasterVolume, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(loaded.Settings.MouseSensitivity, Is.EqualTo(1.35f).Within(0.001f));
        }

        [Test]
        public void CalculateFinalScore_RewardsQuietFastEscape()
        {
            var quiet = new RunStats { CompletionSeconds = 90f, Alerts = 0, Captures = 0, EchoStonesUsed = 0, RelicsCollected = 2 };
            var noisy = new RunStats { CompletionSeconds = 210f, Alerts = 4, Captures = 2, EchoStonesUsed = 4, RelicsCollected = 0 };

            var quietResult = ScoreService.CalculateFinalScore(quiet);
            var noisyResult = ScoreService.CalculateFinalScore(noisy);

            Assert.That(quietResult.Score, Is.GreaterThan(noisyResult.Score));
            Assert.That(quietResult.Rank, Is.EqualTo("S"));
            Assert.That(noisyResult.Rank, Is.Not.EqualTo("S"));
        }
    }
}
