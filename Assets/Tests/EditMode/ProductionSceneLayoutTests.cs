using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ProductionSceneLayoutTests
    {
        [Test]
        public void DefaultLayout_DefinesACompleteSixZoneJourney()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();

            CollectionAssert.AreEqual(
                new[] { RuinZone.SafeEntry, RuinZone.Courtyard, RuinZone.ShadowGallery, RuinZone.EchoPassage, RuinZone.Altar, RuinZone.Exit },
                layout.ZoneOrder);
        }

        [Test]
        public void DefaultLayout_HasThreeUniqueCoresAndThreeGuardians()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();

            Assert.That(layout.Cores.Count, Is.EqualTo(3));
            Assert.That(layout.Cores.Select(core => core.Id).Distinct().Count(), Is.EqualTo(3));
            Assert.That(layout.Guardians.Count, Is.EqualTo(3));
            Assert.That(layout.Guardians.All(guardian => guardian.Waypoints.Count >= 3), Is.True);
        }


        [Test]
        public void DefaultLayout_ProvidesAnExtendedRouteAndVisibleNorthExit()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();

            Assert.That(layout.MainRouteLength, Is.GreaterThanOrEqualTo(100f));
            Assert.That(layout.ExitPosition.z, Is.GreaterThan(layout.PlayerSpawn.z + 80f));
        }

        [Test]
        public void DefaultLayout_KeepsGuardiansAwayFromProtectedSpawn()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();

            foreach (GuardianRoute guardian in layout.Guardians)
                foreach (Vector3 waypoint in guardian.Waypoints)
                    Assert.That(Vector3.Distance(layout.PlayerSpawn, waypoint), Is.GreaterThan(8f));
        }

        [Test]
        public void DefaultLayout_PresentsFirstCoreFromEntryApproach()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();
            CorePlacement firstCore = layout.Cores.Single(core => core.Zone == RuinZone.Courtyard);

            Assert.That(Vector3.Distance(layout.PlayerSpawn, firstCore.Position), Is.InRange(20f, 32f));
        }

        [Test]
        public void DefaultLayout_PlacesAltarCoreGroundedInsideTheReachableAltarRoom()
        {
            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();
            CorePlacement altarCore = layout.Cores.Single(core => core.Zone == RuinZone.Altar);

            Assert.That(altarCore.Position.y, Is.LessThanOrEqualTo(1.25f));
            Assert.That(altarCore.Position.x, Is.EqualTo(16f));
            Assert.That(altarCore.Position.z, Is.EqualTo(12f));
        }
    }
}
