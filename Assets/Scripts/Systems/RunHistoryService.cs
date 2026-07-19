namespace EchoesOfTheRuins
{
    /// <summary>Applies a completed run to local history and best-result fields.</summary>
    public static class RunHistoryService
    {
        public static ScoreResult RecordCompletedRun(SaveData data, RunStats stats, string completedUtc)
        {
            if (data == null) data = SaveData.CreateDefault();
            if (stats == null) stats = new RunStats();
            if (data.RecentRuns == null) data.RecentRuns = new System.Collections.Generic.List<RunRecord>();

            ScoreResult result = ScoreService.CalculateFinalScore(stats);
            data.RecentRuns.Insert(0, new RunRecord
            {
                Score = result.Score,
                Rank = result.Rank,
                CompletionSeconds = stats.CompletionSeconds,
                Alerts = stats.Alerts,
                Captures = stats.Captures,
                EchoStonesUsed = stats.EchoStonesUsed,
                RelicsCollected = stats.RelicsCollected,
                CompletedUtc = completedUtc ?? string.Empty
            });
            if (data.RecentRuns.Count > 5)
                data.RecentRuns.RemoveRange(5, data.RecentRuns.Count - 5);

            if (result.Score > data.BestScore)
            {
                data.BestScore = result.Score;
                data.BestRank = result.Rank;
            }
            if (data.BestCompletionSeconds <= 0f || stats.CompletionSeconds < data.BestCompletionSeconds)
                data.BestCompletionSeconds = stats.CompletionSeconds;
            return result;
        }
    }
}
