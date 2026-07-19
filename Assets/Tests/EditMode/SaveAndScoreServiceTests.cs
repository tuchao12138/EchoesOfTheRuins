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
        public void Load_ReturnsDefaults_WhenNoSaveExists()
        {
            SaveData loaded = new SaveService(temporaryDirectory).Load();

            Assert.That(loaded.Version, Is.EqualTo(SaveData.CurrentVersion));
            Assert.That(loaded.CheckpointId, Is.EqualTo(SaveData.StartCheckpointId));
            Assert.That(loaded.ObjectiveStage, Is.EqualTo(ObjectiveStage.Briefing.ToString()));
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
        public void Load_PreservesCorruptSaveAsRecoveryBackup()
        {
            Directory.CreateDirectory(temporaryDirectory);
            File.WriteAllText(Path.Combine(temporaryDirectory, "echoes-save.json"), "not json");

            new SaveService(temporaryDirectory).Load();

            Assert.That(Directory.GetFiles(temporaryDirectory, "echoes-save.corrupt-*.json"), Has.Length.EqualTo(1));
        }

        [Test]
        public void Load_MigratesVersionTwoDataToCurrentSchema()
        {
            Directory.CreateDirectory(temporaryDirectory);
            File.WriteAllText(
                Path.Combine(temporaryDirectory, "echoes-save.json"),
                "{\"Version\":2,\"CheckpointId\":\"altar\",\"CollectedCoreIds\":[\"courtyard-core\"],\"Settings\":null}");

            SaveData loaded = new SaveService(temporaryDirectory).Load();

            Assert.That(SaveData.CurrentVersion, Is.EqualTo(4));
            Assert.That(loaded.Version, Is.EqualTo(4));
            Assert.That(loaded.CheckpointId, Is.EqualTo("altar"));
            Assert.That(loaded.CollectedCoreIds, Is.EquivalentTo(new[] { "courtyard-core" }));
            Assert.That(loaded.Settings, Is.Not.Null);
            Assert.That(loaded.RecentRuns, Is.Not.Null);
            Assert.That(loaded.ObjectiveStage, Is.EqualTo(ObjectiveStage.CollectCores.ToString()));
        }

        [Test]
        public void Load_MigratesVersionThreeWithoutDiscardingProgressOrHistory()
        {
            Directory.CreateDirectory(temporaryDirectory);
            File.WriteAllText(
                Path.Combine(temporaryDirectory, "echoes-save.json"),
                "{\"Version\":3,\"CheckpointId\":\"courtyard\",\"CollectedCoreIds\":[\"core-1\"],\"CollectedRelicIds\":[\"relic-1\"],\"BestScore\":975,\"BestRank\":\"A\",\"BestCompletionSeconds\":91.5,\"RecentRuns\":[{\"Score\":975,\"Rank\":\"A\",\"CompletionSeconds\":91.5,\"Alerts\":1,\"Captures\":0,\"EchoStonesUsed\":2,\"RelicsCollected\":1,\"CompletedUtc\":\"2026-07-19T00:00:00Z\"}],\"Settings\":{\"MasterVolume\":0.6,\"MusicVolume\":0.5,\"SfxVolume\":0.4,\"MouseSensitivity\":1.2,\"QualityPreset\":2}}");

            SaveData loaded = new SaveService(temporaryDirectory).Load();

            Assert.That(loaded.Version, Is.EqualTo(4));
            Assert.That(loaded.ObjectiveStage, Is.EqualTo(ObjectiveStage.CollectCores.ToString()));
            Assert.That(loaded.CheckpointId, Is.EqualTo("courtyard"));
            Assert.That(loaded.CollectedCoreIds, Is.EquivalentTo(new[] { "core-1" }));
            Assert.That(loaded.CollectedRelicIds, Is.EquivalentTo(new[] { "relic-1" }));
            Assert.That(loaded.BestScore, Is.EqualTo(975));
            Assert.That(loaded.BestRank, Is.EqualTo("A"));
            Assert.That(loaded.BestCompletionSeconds, Is.EqualTo(91.5f));
            Assert.That(loaded.RecentRuns, Has.Count.EqualTo(1));
            Assert.That(loaded.RecentRuns[0].CompletedUtc, Is.EqualTo("2026-07-19T00:00:00Z"));
            Assert.That(loaded.Settings.MouseSensitivity, Is.EqualTo(1.2f));
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

        [Test]
        public void RecordCompletedRun_UpdatesBestAndKeepsDetailedHistory()
        {
            SaveData data = SaveData.CreateDefault();
            var stats = new RunStats
            {
                CompletionSeconds = 92f,
                Alerts = 1,
                Captures = 0,
                EchoStonesUsed = 1,
                RelicsCollected = 2
            };

            ScoreResult result = RunHistoryService.RecordCompletedRun(data, stats, "2026-07-18T00:00:00Z");

            Assert.That(data.RecentRuns, Has.Count.EqualTo(1));
            Assert.That(data.RecentRuns[0].Alerts, Is.EqualTo(1));
            Assert.That(data.RecentRuns[0].EchoStonesUsed, Is.EqualTo(1));
            Assert.That(data.BestScore, Is.EqualTo(result.Score));
            Assert.That(data.BestCompletionSeconds, Is.EqualTo(92f));
            Assert.That(data.BestRank, Is.EqualTo(result.Rank));
        }
    }
}
