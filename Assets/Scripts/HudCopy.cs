namespace EchoesOfTheRuins
{
    /// <summary>Short, unambiguous English copy used by the gameplay HUD.</summary>
    public static class HudCopy
    {
        public const string MissionRule = "COLLECT 3 CORES  >  UNLOCK NORTH EXIT  >  PRESS E";
        public const string EscapeObjective = "ESCAPE NORTH\nFOLLOW THE CYAN BEACON AND PRESS E";
        public const string InteractionInterrupted = "INTERRUPTED - BREAK LINE OF SIGHT";

        public static string Objective(ObjectiveData data) => data.Stage switch
        {
            ObjectiveStage.Move => "MOVE AND SPRINT\nWASD + LEFT SHIFT",
            ObjectiveStage.Observe => "OBSERVE THE GUARDIAN\nAVOID ITS VISION CONE",
            ObjectiveStage.Hide => "ENTER DEEP SHADOW\nHOLD C TO CROUCH",
            ObjectiveStage.Distract => "DISTRACT THE GUARDIAN\nPRESS Q TO THROW AN ECHO STONE",
            ObjectiveStage.CollectCores => $"{data.Title}\n{data.ProgressCurrent}/3 - HOLD E FOR 1.5 SECONDS",
            ObjectiveStage.ReachExit => EscapeObjective,
            ObjectiveStage.Complete => "ESCAPED THE RUINS",
            _ => "MISSION BRIEFING\nCOLLECT 3 CORES, THEN ESCAPE NORTH"
        };

        public static string ResultSummary(string rank, int score, RunStats stats)
        {
            stats ??= new RunStats();
            return "ESCAPED THE RUINS\n\n" +
                $"RANK  {rank}     SCORE  {score}\n" +
                $"TIME  {FormatTime(stats.CompletionSeconds)}\n" +
                $"ALERTS  {stats.Alerts}     CAPTURES  {stats.Captures}\n" +
                $"ECHO STONES  {stats.EchoStonesUsed}     RELICS  {stats.RelicsCollected}/2";
        }

        private static string FormatTime(float seconds) =>
            $"{UnityEngine.Mathf.FloorToInt(seconds / 60f):00}:{UnityEngine.Mathf.FloorToInt(seconds % 60f):00}";
    }
}
