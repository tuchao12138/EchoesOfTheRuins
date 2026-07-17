using System;

namespace EchoesOfTheRuins
{
    public enum TutorialStage
    {
        Objective,
        Movement,
        Shadow,
        EchoStone,
        Awareness,
        Complete
    }

    public sealed class TutorialProgress
    {
        public TutorialStage Stage { get; private set; } = TutorialStage.Objective;
        public event Action<TutorialStage> StageChanged;

        public void ContinueFromObjective() => Advance(TutorialStage.Objective, TutorialStage.Movement);
        public void NotifyMoved() => Advance(TutorialStage.Movement, TutorialStage.Shadow);
        public void NotifyCrouchedInShadow() => Advance(TutorialStage.Shadow, TutorialStage.EchoStone);
        public void NotifyEchoStoneUsed() => Advance(TutorialStage.EchoStone, TutorialStage.Awareness);
        public void ContinueFromAwareness() => Advance(TutorialStage.Awareness, TutorialStage.Complete);

        public void Skip()
        {
            if (Stage == TutorialStage.Complete) return;
            Stage = TutorialStage.Complete;
            StageChanged?.Invoke(Stage);
        }

        private void Advance(TutorialStage expected, TutorialStage next)
        {
            if (Stage != expected) return;
            Stage = next;
            StageChanged?.Invoke(Stage);
        }
    }

    public sealed class SpawnProtection
    {
        private float remaining;

        public bool CanBeDetected => remaining <= 0f;
        public float RemainingSeconds => Math.Max(0f, remaining);

        public SpawnProtection(float durationSeconds)
        {
            remaining = Math.Max(0f, durationSeconds);
        }

        public void Tick(float deltaTime)
        {
            remaining = Math.Max(0f, remaining - Math.Max(0f, deltaTime));
        }
    }

    public sealed class ObjectiveTrackerModel
    {
        private readonly int requiredCores;

        public int CollectedCores { get; private set; }
        public bool ExitUnlocked => CollectedCores >= requiredCores;
        public string CurrentObjective => ExitUnlocked
            ? "前往封印出口 / Reach the sealed exit"
            : $"收集能量核心 {CollectedCores}/{requiredCores} / Collect energy cores";

        public event Action<string> Changed;

        public ObjectiveTrackerModel(int requiredCores)
        {
            this.requiredCores = Math.Max(1, requiredCores);
        }

        public void SetCollectedCores(int count)
        {
            int clamped = Math.Max(0, Math.Min(requiredCores, count));
            if (clamped == CollectedCores) return;
            CollectedCores = clamped;
            Changed?.Invoke(CurrentObjective);
        }
    }
}
