using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    /// <summary>Pure gameplay state for the three unique energy cores.</summary>
    public sealed class RuinGameState
    {
        public const int RequiredCoreCount = 3;
        private readonly HashSet<string> collectedCoreIds = new HashSet<string>();

        public int CollectedCoreCount => collectedCoreIds.Count;
        public bool IsExitUnlocked => CollectedCoreCount >= RequiredCoreCount;

        /// <returns>True only when this is the first collection of <paramref name="coreId"/>.</returns>
        public bool CollectCore(string coreId)
        {
            return !string.IsNullOrWhiteSpace(coreId) && collectedCoreIds.Add(coreId);
        }
    }
}
