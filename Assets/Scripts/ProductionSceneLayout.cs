using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum RuinZone
    {
        SafeEntry,
        Courtyard,
        ShadowGallery,
        EchoPassage,
        Altar,
        Exit
    }

    public sealed class CorePlacement
    {
        public CorePlacement(string id, RuinZone zone, Vector3 position)
        {
            Id = id;
            Zone = zone;
            Position = position;
        }

        public string Id { get; }
        public RuinZone Zone { get; }
        public Vector3 Position { get; }
    }

    public sealed class GuardianRoute
    {
        public GuardianRoute(string id, Vector3 spawn, IReadOnlyList<Vector3> waypoints)
        {
            Id = id;
            Spawn = spawn;
            Waypoints = waypoints;
        }

        public string Id { get; }
        public Vector3 Spawn { get; }
        public IReadOnlyList<Vector3> Waypoints { get; }
    }

    /// <summary>Single source of truth for the authored vertical-slice route.</summary>
    public sealed class ProductionSceneLayout
    {
        private ProductionSceneLayout(
            Vector3 playerSpawn,
            Vector3 exitPosition,
            IReadOnlyList<RuinZone> zoneOrder,
            IReadOnlyList<CorePlacement> cores,
            IReadOnlyList<GuardianRoute> guardians)
        {
            PlayerSpawn = playerSpawn;
            ExitPosition = exitPosition;
            ZoneOrder = zoneOrder;
            Cores = cores;
            Guardians = guardians;
        }

        public Vector3 PlayerSpawn { get; }
        public Vector3 ExitPosition { get; }
        public IReadOnlyList<RuinZone> ZoneOrder { get; }
        public IReadOnlyList<CorePlacement> Cores { get; }
        public IReadOnlyList<GuardianRoute> Guardians { get; }
        public float MainRouteLength
        {
            get
            {
                float length = 0f;
                Vector3 previous = PlayerSpawn;
                foreach (CorePlacement core in Cores)
                {
                    length += Vector3.Distance(previous, core.Position);
                    previous = core.Position;
                }
                return length + Vector3.Distance(previous, ExitPosition);
            }
        }

        public static ProductionSceneLayout CreateDefault()
        {
            var zoneOrder = new[]
            {
                RuinZone.SafeEntry,
                RuinZone.Courtyard,
                RuinZone.ShadowGallery,
                RuinZone.EchoPassage,
                RuinZone.Altar,
                RuinZone.Exit
            };
            var cores = new[]
            {
                new CorePlacement("courtyard-core", RuinZone.Courtyard, new Vector3(0f, 1.25f, -27f)),
                new CorePlacement("shadow-gallery-core", RuinZone.ShadowGallery, new Vector3(-16f, 1.25f, 2f)),
                new CorePlacement("altar-core", RuinZone.Altar, new Vector3(16f, 1.25f, 12f))
            };
            var guardians = new[]
            {
                new GuardianRoute("courtyard-guardian", new Vector3(6f, 0f, -24f), new[]
                {
                    new Vector3(-7f, 0f, -31f),
                    new Vector3(7f, 0f, -31f),
                    new Vector3(7f, 0f, -20f),
                    new Vector3(-7f, 0f, -20f)
                }),
                new GuardianRoute("gallery-guardian", new Vector3(-13f, 0f, 2f), new[]
                {
                    new Vector3(-18f, 0f, -6f),
                    new Vector3(-10f, 0f, -6f),
                    new Vector3(-10f, 0f, 12f),
                    new Vector3(-18f, 0f, 12f)
                }),
                new GuardianRoute("altar-guardian", new Vector3(13f, 0f, 28f), new[]
                {
                    new Vector3(9f, 0f, 25f),
                    new Vector3(19f, 0f, 25f),
                    new Vector3(19f, 0f, 39f),
                    new Vector3(9f, 0f, 39f)
                })
            };
            return new ProductionSceneLayout(
                new Vector3(0f, 0f, -49f),
                new Vector3(0f, 2f, 55f),
                zoneOrder,
                cores,
                guardians);
        }
    }
}
