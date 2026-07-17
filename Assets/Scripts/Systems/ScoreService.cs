using UnityEngine;

namespace EchoesOfTheRuins
{
    public static class ScoreService
    {
        public static ScoreResult CalculateFinalScore(RunStats stats)
        {
            if (stats == null) stats = new RunStats();
            var score = 600;
            score += Mathf.Max(0, 300 - Mathf.RoundToInt(stats.CompletionSeconds));
            score += Mathf.Max(0, stats.RelicsCollected) * 100;
            score -= Mathf.Max(0, stats.Alerts) * 75;
            score -= Mathf.Max(0, stats.Captures) * 150;
            score -= Mathf.Max(0, stats.EchoStonesUsed) * 20;
            score = Mathf.Max(0, score);
            var rank = score >= 900 ? "S" : score >= 700 ? "A" : score >= 500 ? "B" : "C";
            return new ScoreResult(score, rank);
        }
    }
}
