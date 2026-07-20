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
            ObjectiveStage.CollectCores => $"COLLECT 3 CORES\n{data.ProgressCurrent}/3 - HOLD E FOR 1.5 SECONDS",
            ObjectiveStage.ReachExit => EscapeObjective,
            ObjectiveStage.Complete => "ESCAPED THE RUINS",
            _ => "MISSION BRIEFING\nCOLLECT 3 CORES, THEN ESCAPE NORTH"
        };
    }
}
