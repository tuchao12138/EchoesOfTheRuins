using System;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum ObjectiveStage { Briefing, Move, Observe, Hide, Distract, CollectCores, ReachExit, Complete }
    public enum ObjectiveSignal { BriefingFinished, MovedAndSprinted, ObservedGuardian, CrouchedInShadow, UsedEchoStone, CoreCountChanged, ExitUnlocked, Escaped }

    [Serializable]
    public sealed class ObjectiveData
    {
        public ObjectiveStage Stage;
        public string Title;
        public string Body;
        public int ProgressCurrent;
        public int ProgressRequired;
        public int RemainingRequired;
        public string TargetLabel;
        public bool HighPriority;
        public Vector3 WorldPosition;
        public bool ShowDistance;
        public bool InputLocked;
    }

    public sealed class ObjectiveTracker
    {
        private const int RequiredCores = 3;
        public ObjectiveData Current { get; private set; }
        public event Action<ObjectiveData> Changed;

        public ObjectiveTracker(ObjectiveStage initialStage = ObjectiveStage.Briefing, int collectedCores = 0)
        {
            Current = Create(initialStage, collectedCores);
        }

        public void Notify(ObjectiveSignal signal, int coreCount = 0, Vector3 worldPosition = default)
        {
            ObjectiveStage next = Current.Stage;
            switch (Current.Stage)
            {
                case ObjectiveStage.Briefing when signal == ObjectiveSignal.BriefingFinished: next = ObjectiveStage.Move; break;
                case ObjectiveStage.Move when signal == ObjectiveSignal.MovedAndSprinted: next = ObjectiveStage.Observe; break;
                case ObjectiveStage.Observe when signal == ObjectiveSignal.ObservedGuardian: next = ObjectiveStage.Hide; break;
                case ObjectiveStage.Hide when signal == ObjectiveSignal.CrouchedInShadow: next = ObjectiveStage.Distract; break;
                case ObjectiveStage.Distract when signal == ObjectiveSignal.UsedEchoStone: next = ObjectiveStage.CollectCores; break;
                case ObjectiveStage.CollectCores when (signal == ObjectiveSignal.CoreCountChanged && coreCount >= RequiredCores) || signal == ObjectiveSignal.ExitUnlocked: next = ObjectiveStage.ReachExit; break;
                case ObjectiveStage.ReachExit when signal == ObjectiveSignal.Escaped: next = ObjectiveStage.Complete; break;
            }

            if (next != Current.Stage)
                Advance(next, coreCount, worldPosition);
            else if (Current.Stage == ObjectiveStage.CollectCores && signal == ObjectiveSignal.CoreCountChanged && coreCount != Current.ProgressCurrent)
                Advance(ObjectiveStage.CollectCores, coreCount, worldPosition);
        }

        public void Advance(ObjectiveStage stage, int coreCount = 0, Vector3 worldPosition = default)
        {
            Current = Create(stage, coreCount, worldPosition);
            Changed?.Invoke(Current);
        }

        private static ObjectiveData Create(ObjectiveStage stage, int coreCount, Vector3 worldPosition = default)
        {
            int cores = Mathf.Clamp(coreCount, 0, RequiredCores);
            return stage switch
            {
                ObjectiveStage.Briefing => new ObjectiveData { Stage = stage, Title = "YOUR ROUTE", Body = "Watch the route to the first core.", InputLocked = true },
                ObjectiveStage.Move => new ObjectiveData { Stage = stage, Title = "MOVE", Body = "Move, then sprint to build momentum.", ShowDistance = true, WorldPosition = worldPosition },
                ObjectiveStage.Observe => new ObjectiveData { Stage = stage, Title = "OBSERVE", Body = "Watch the guardian before crossing.", ShowDistance = true, WorldPosition = worldPosition },
                ObjectiveStage.Hide => new ObjectiveData { Stage = stage, Title = "HIDE", Body = "Crouch in deep shadow to stay hidden.", ShowDistance = true, WorldPosition = worldPosition },
                ObjectiveStage.Distract => new ObjectiveData { Stage = stage, Title = "DISTRACT", Body = "Use an echo stone to draw the guardian away.", ShowDistance = true, WorldPosition = worldPosition },
                ObjectiveStage.CollectCores => CreateCoreRouteObjective(cores, worldPosition),
                ObjectiveStage.ReachExit => new ObjectiveData { Stage = stage, Title = "ESCAPE NORTH", Body = "All cores recovered. Reach the unsealed gate and press E.", ProgressCurrent = RequiredCores, ProgressRequired = RequiredCores, RemainingRequired = 0, TargetLabel = "EXIT", HighPriority = true, ShowDistance = true, WorldPosition = worldPosition },
                _ => new ObjectiveData { Stage = ObjectiveStage.Complete, Title = "ESCAPED", Body = "You escaped the ruins." }
            };
        }

        private static ObjectiveData CreateCoreRouteObjective(int collectedCores, Vector3 worldPosition)
        {
            (string title, string body, string targetLabel) = collectedCores switch
            {
                0 => ("CORE I - COURTYARD", "Reach the courtyard core and hold E for 1.5 seconds.", "COURTYARD"),
                1 => ("CORE II - SHADOW GALLERY", "Follow the cyan marker through the shadow gallery.", "SHADOW GALLERY"),
                _ => ("CORE III - ALTAR", "Follow the cyan beacon to the altar core.", "ALTAR")
            };
            return new ObjectiveData
            {
                Stage = ObjectiveStage.CollectCores,
                Title = title,
                Body = body,
                ProgressCurrent = collectedCores,
                ProgressRequired = RequiredCores,
                RemainingRequired = RequiredCores - collectedCores,
                TargetLabel = targetLabel,
                ShowDistance = true,
                WorldPosition = worldPosition
            };
        }
    }
}
