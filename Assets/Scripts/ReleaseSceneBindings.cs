using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Runtime-safe release bindings shared by scene generation and validation tests.</summary>
    public static class ReleaseSceneBindings
    {
        public static void Ensure(GameManager manager)
        {
            if (manager == null) return;
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            GuardianAI[] guardians = Object.FindObjectsByType<GuardianAI>(FindObjectsSortMode.None);
            foreach (GuardianAI guardian in guardians)
                if (guardian != null && guardian.GetComponent<GuardianVisionCone>() == null)
                    guardian.gameObject.AddComponent<GuardianVisionCone>();

            if (manager.GetComponent<ThreatCoordinator>() == null) manager.gameObject.AddComponent<ThreatCoordinator>();
            if (Object.FindFirstObjectByType<ObjectiveDirector>() == null) manager.gameObject.AddComponent<ObjectiveDirector>();
            if (Object.FindFirstObjectByType<WorldObjectiveMarker>() == null) new GameObject("World Objective Marker").AddComponent<WorldObjectiveMarker>();

            PlayerHitResponse hitResponse = player.GetComponent<PlayerHitResponse>() ?? player.gameObject.AddComponent<PlayerHitResponse>();
            hitResponse.Configure(player, manager);
            foreach (GuardianAI guardian in guardians) hitResponse.TrackGuardian(guardian);
        }
    }
}
