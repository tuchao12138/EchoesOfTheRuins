using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    /// <summary>Collects score-relevant events without coupling AI or player code to persistence.</summary>
    public sealed class RunMetricsTracker
    {
        private readonly Dictionary<string, GuardianState> guardianStates = new Dictionary<string, GuardianState>();

        public int Alerts { get; private set; }
        public int Captures { get; private set; }
        public int EchoStonesUsed { get; private set; }

        public void RecordGuardianState(string guardianId, GuardianState state)
        {
            guardianId = string.IsNullOrWhiteSpace(guardianId) ? "guardian" : guardianId;
            guardianStates.TryGetValue(guardianId, out GuardianState previous);
            if (state == GuardianState.Chase && previous != GuardianState.Chase) Alerts++;
            guardianStates[guardianId] = state;
        }

        public void RecordCapture() => Captures++;
        public void RecordEchoStone() => EchoStonesUsed++;

        public void ApplyTo(RunStats stats)
        {
            if (stats == null) return;
            stats.Alerts = Alerts;
            stats.Captures = Captures;
            stats.EchoStonesUsed = EchoStonesUsed;
        }
    }
}
