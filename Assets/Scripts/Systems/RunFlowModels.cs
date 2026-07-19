using System;
using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    public enum RunPhase
    {
        Infiltration,
        CoreHunt,
        Escape,
        Complete
    }

    /// <summary>Deterministic run progression shared by gameplay, UI and tests.</summary>
    public sealed class RunFlowState
    {
        public RunPhase Phase { get; private set; } = RunPhase.Infiltration;
        public bool HasCompleted => Phase == RunPhase.Complete;

        public event Action<RunPhase> Changed;

        public void BeginCoreHunt()
        {
            if (Phase == RunPhase.Infiltration) SetPhase(RunPhase.CoreHunt);
        }

        public void UpdateCoreProgress(int collected, int required)
        {
            if (HasCompleted) return;
            if (required > 0 && collected >= required)
                SetPhase(RunPhase.Escape);
            else if (Phase == RunPhase.Infiltration)
                SetPhase(RunPhase.CoreHunt);
        }

        public bool TryCompleteEscape()
        {
            if (Phase != RunPhase.Escape) return false;
            SetPhase(RunPhase.Complete);
            return true;
        }

        private void SetPhase(RunPhase phase)
        {
            if (Phase == phase) return;
            Phase = phase;
            Changed?.Invoke(phase);
        }
    }

    public static class ExitGatePresentation
    {
        public static string GetPrompt(bool unlocked, int remainingCores) => unlocked
            ? "E — ESCAPE / 逃离遗迹"
            : $"EXIT SEALED — {Math.Max(0, remainingCores)} CORES REQUIRED / 出口封印";
    }

    /// <summary>Pure hold-to-activate progress. Detection cancels the current attempt.</summary>
    public sealed class CoreActivationModel
    {
        private readonly float duration;
        private float elapsed;

        public CoreActivationModel(float durationSeconds)
        {
            duration = Math.Max(.1f, durationSeconds);
        }

        public float Progress01 => Math.Min(1f, elapsed / duration);

        public bool Tick(float deltaTime, bool held, bool interrupted)
        {
            if (interrupted || !held)
            {
                elapsed = 0f;
                return false;
            }

            elapsed = Math.Min(duration, elapsed + Math.Max(0f, deltaTime));
            return elapsed >= duration;
        }
    }

    public static class MotionClarityProfile
    {
        public const bool UseTemporalAntialiasing = false;
        public const bool UseMotionBlur = false;
        public const bool UseFilmGrain = false;
        public const float FloorTextureTiling = 7f;
        public const int TargetFrameRate = 60;
        public const int AnisotropicLevel = 16;
    }

    public readonly struct GuardianDifficultyProfile
    {
        public readonly float PatrolSpeedMultiplier;
        public readonly float ChaseSpeedMultiplier;
        public readonly float DetectionRangeMultiplier;
        public readonly float SuspicionGainMultiplier;
        public readonly float SuspicionDecayMultiplier;

        private GuardianDifficultyProfile(float patrol, float chase, float range, float gain, float decay)
        {
            PatrolSpeedMultiplier = patrol;
            ChaseSpeedMultiplier = chase;
            DetectionRangeMultiplier = range;
            SuspicionGainMultiplier = gain;
            SuspicionDecayMultiplier = decay;
        }

        public static GuardianDifficultyProfile For(RunPhase phase, int collectedCores)
        {
            int cores = Math.Max(0, Math.Min(3, collectedCores));
            if (phase == RunPhase.Escape)
                return new GuardianDifficultyProfile(1.3f, 1.18f, 1.2f, 1.3f, .65f);
            return new GuardianDifficultyProfile(
                1f + cores * .08f,
                1f + cores * .04f,
                1f + cores * .04f,
                1f + cores * .05f,
                1f);
        }
    }

    public sealed class RelicProgressState
    {
        private readonly HashSet<string> ids = new HashSet<string>();
        public int Count => ids.Count;

        public void Restore(IEnumerable<string> relicIds)
        {
            ids.Clear();
            if (relicIds == null) return;
            foreach (string id in relicIds)
                if (!string.IsNullOrWhiteSpace(id)) ids.Add(id);
        }

        public bool Collect(string id) => !string.IsNullOrWhiteSpace(id) && ids.Add(id);
        public bool Contains(string id) => !string.IsNullOrWhiteSpace(id) && ids.Contains(id);
        public List<string> ToList() => new List<string>(ids);
    }
}
