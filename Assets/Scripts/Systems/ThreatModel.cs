using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public readonly struct ThreatSnapshot : IEquatable<ThreatSnapshot>
    {
        public readonly string GuardianId;
        public readonly GuardianState State;
        public readonly float Suspicion;
        public readonly GuardianAttackPhase AttackPhase;
        public readonly Vector3 Direction;

        public ThreatSnapshot(string guardianId, GuardianState state, float suspicion, GuardianAttackPhase attackPhase, Vector3 direction)
        {
            GuardianId = guardianId ?? string.Empty;
            State = state;
            Suspicion = Mathf.Clamp01(suspicion);
            AttackPhase = attackPhase;
            Direction = direction.sqrMagnitude > .001f ? direction.normalized : Vector3.zero;
        }

        public bool Equals(ThreatSnapshot other) => GuardianId == other.GuardianId && State == other.State &&
            Mathf.Approximately(Suspicion, other.Suspicion) && AttackPhase == other.AttackPhase && Direction == other.Direction;
        public override bool Equals(object obj) => obj is ThreatSnapshot other && Equals(other);
        public override int GetHashCode() => (GuardianId, State, Suspicion, AttackPhase, Direction).GetHashCode();
    }

    public sealed class ThreatModel
    {
        private readonly Dictionary<string, ThreatSnapshot> guardians = new Dictionary<string, ThreatSnapshot>();
        public ThreatSnapshot Current { get; private set; }
        public event Action<ThreatSnapshot> Changed;

        public void Update(string guardianId, GuardianState state, float suspicion, GuardianAttackPhase attackPhase, Vector3 direction)
        {
            guardians[guardianId] = new ThreatSnapshot(guardianId, state, suspicion, attackPhase, direction);
            ThreatSnapshot selected = SelectCurrent();
            if (selected.Equals(Current)) return;
            Current = selected;
            Changed?.Invoke(Current);
        }

        public void Remove(string guardianId)
        {
            if (string.IsNullOrWhiteSpace(guardianId) || !guardians.Remove(guardianId)) return;
            ThreatSnapshot selected = SelectCurrent();
            if (selected.Equals(Current)) return;
            Current = selected;
            Changed?.Invoke(Current);
        }

        private ThreatSnapshot SelectCurrent()
        {
            ThreatSnapshot selected = default;
            bool hasSelection = false;
            foreach (ThreatSnapshot candidate in guardians.Values)
            {
                if (!hasSelection || IsMoreDangerous(candidate, selected))
                {
                    selected = candidate;
                    hasSelection = true;
                }
            }
            return selected;
        }

        private static bool IsMoreDangerous(ThreatSnapshot candidate, ThreatSnapshot current)
        {
            int attack = AttackPriority(candidate.AttackPhase).CompareTo(AttackPriority(current.AttackPhase));
            if (attack != 0) return attack > 0;
            int state = StatePriority(candidate.State).CompareTo(StatePriority(current.State));
            if (state != 0) return state > 0;
            return candidate.Suspicion > current.Suspicion;
        }

        private static int AttackPriority(GuardianAttackPhase phase) => phase switch
        {
            GuardianAttackPhase.HitConfirmed => 5,
            GuardianAttackPhase.Strike => 4,
            GuardianAttackPhase.Telegraph => 3,
            GuardianAttackPhase.Recovery => 2,
            _ => 1
        };

        private static int StatePriority(GuardianState state) => state switch
        {
            GuardianState.Capture => 8,
            GuardianState.Recovery => 7,
            GuardianState.Strike => 6,
            GuardianState.AttackTelegraph => 5,
            GuardianState.Chase => 4,
            GuardianState.Search => 3,
            GuardianState.Investigate => 2,
            _ => 1
        };
    }
}
