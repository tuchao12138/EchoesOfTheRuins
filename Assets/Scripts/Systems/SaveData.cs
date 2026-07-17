using System;
using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    [Serializable]
    public sealed class GameSettings
    {
        public const float DefaultMouseSensitivity = 1f;
        public float MasterVolume = 0.85f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 0.85f;
        public float MouseSensitivity = DefaultMouseSensitivity;
        public int QualityPreset = 1;
    }

    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;
        public const string StartCheckpointId = "entrance";

        public int Version = CurrentVersion;
        public string CheckpointId = StartCheckpointId;
        public List<string> CollectedCoreIds = new List<string>();
        public List<string> CollectedRelicIds = new List<string>();
        public string LastPlayedUtc = "";
        public int BestScore;
        public float BestCompletionSeconds;
        public GameSettings Settings = new GameSettings();

        public static SaveData CreateDefault() => new SaveData();
    }

    [Serializable]
    public sealed class RunStats
    {
        public float CompletionSeconds;
        public int Alerts;
        public int Captures;
        public int EchoStonesUsed;
        public int RelicsCollected;
    }

    public readonly struct ScoreResult
    {
        public readonly int Score;
        public readonly string Rank;

        public ScoreResult(int score, string rank)
        {
            Score = score;
            Rank = rank;
        }
    }
}
