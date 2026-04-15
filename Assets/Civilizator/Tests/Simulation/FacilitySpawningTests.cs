using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class FacilitySpawnerTests
    {
        private SimulationClock _clock;
        private FacilitySpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _clock = new SimulationClock();
            _spawner = new FacilitySpawner(_clock);
        }

        [Test]
        public void SpawnIfNewCycle_WithPlantationBase_Spawns1LogPerTilePer2x2_AfterCycleBoundary()
        {
            // Arrange: Create a base (non-upgraded) plantation at (0, 0)
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = false;
            plantation.UpgradeLevel = 0;

            var buildings = new List<Building> { plantation };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Advance to just before cycle boundary
            _clock.Advance(59f);
            var spawnedBefore = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: No spawn yet
            Assert.AreEqual(0, spawnedBefore.Count);

            // Act: Cross cycle boundary
            existingSpawned.AddRange(spawnedBefore);
            _clock.Advance(2f);
            var spawnedAfter = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: Should spawn 4 logs (one per tile of 2×2 footprint)
            Assert.AreEqual(4, spawnedAfter.Count);
            Assert.IsTrue(spawnedAfter.All(r => r.Kind == ResourceKind.Logs));
            Assert.IsTrue(spawnedAfter.All(r => !r.IsCollected));

            // Verify tile positions (anchor at (0,0), so 2×2 covers (0,0), (1,0), (0,1), (1,1))
            var positions = spawnedAfter.Select(r => r.Position).OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            Assert.AreEqual(new GridPos(0, 0), positions[0]);
            Assert.AreEqual(new GridPos(0, 1), positions[1]);
            Assert.AreEqual(new GridPos(1, 0), positions[2]);
            Assert.AreEqual(new GridPos(1, 1), positions[3]);
        }

        [Test]
        public void SpawnIfNewCycle_WithFarmBase_Spawns1PlantFoodPerTile()
        {
            // Arrange: Create a base farm at (5, 5)
            var farm = new Building(BuildingKind.Farm, new GridPos(5, 5));
            farm.IsUnderConstruction = false;
            farm.UpgradeLevel = 0;

            var buildings = new List<Building> { farm };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: 4 plant foods
            Assert.AreEqual(4, spawned.Count);
            Assert.IsTrue(spawned.All(r => r.Kind == ResourceKind.PlantFood));
        }

        [Test]
        public void SpawnIfNewCycle_WithCattleFarmBase_Spawns1MeatPerTile()
        {
            // Arrange: Create a base cattle farm at (10, 10)
            var farm = new Building(BuildingKind.CattleFarm, new GridPos(10, 10));
            farm.IsUnderConstruction = false;
            farm.UpgradeLevel = 0;

            var buildings = new List<Building> { farm };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: 4 meats
            Assert.AreEqual(4, spawned.Count);
            Assert.IsTrue(spawned.All(r => r.Kind == ResourceKind.Meat));
        }

        [Test]
        public void SpawnIfNewCycle_WithUpgradedPlantation_Spawns2LogsPerTile()
        {
            // Arrange: Create an upgraded plantation
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = false;
            plantation.UpgradeLevel = 1;

            var buildings = new List<Building> { plantation };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: Should spawn 8 logs (2 per tile × 4 tiles)
            Assert.AreEqual(8, spawned.Count);
            Assert.IsTrue(spawned.All(r => r.Kind == ResourceKind.Logs));
        }

        [Test]
        public void SpawnIfNewCycle_DoesNotSpawnInUnderConstructionFacility()
        {
            // Arrange: Facility still under construction
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = true;
            plantation.UpgradeLevel = 0;

            var buildings = new List<Building> { plantation };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: No spawn
            Assert.AreEqual(0, spawned.Count);
        }

        [Test]
        public void SpawnIfNewCycle_IgnoresCentralAndHouses()
        {
            // Arrange: Create central and house buildings (non-spawning)
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Central, new GridPos(0, 0)),
                new Building(BuildingKind.House, new GridPos(5, 5)),
                new Building(BuildingKind.Tower, new GridPos(10, 10))
            };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: No spawn (no resource facilities)
            Assert.AreEqual(0, spawned.Count);
        }

        [Test]
        public void SpawnIfNewCycle_NoDuplicatesOnTile_NoUncollectedOnTile()
        {
            // Arrange: Plantation with one tile already occupied
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = false;
            plantation.UpgradeLevel = 0;

            var existingSpawned = new List<SpawnedResource>
            {
                new SpawnedResource(ResourceKind.Logs, new GridPos(0, 0))
            };

            var buildings = new List<Building> { plantation };

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: Should spawn only 3 (tiles (1,0), (0,1), (1,1); not (0,0))
            Assert.AreEqual(3, spawned.Count);
            Assert.IsFalse(spawned.Any(r => r.Position == new GridPos(0, 0)));
        }

        [Test]
        public void SpawnIfNewCycle_NoSpawnOnCollectedTile_ButSpawnsOnCollectedTile()
        {
            // Arrange: Plantation with one collected resource (should not block spawn)
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = false;
            plantation.UpgradeLevel = 0;

            var collected = new SpawnedResource(ResourceKind.Logs, new GridPos(0, 0));
            collected.IsCollected = true;
            var existingSpawned = new List<SpawnedResource> { collected };

            var buildings = new List<Building> { plantation };

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: Should spawn 4 (collected resource doesn't block)
            Assert.AreEqual(4, spawned.Count);
        }

        [Test]
        public void SpawnIfNewCycle_MultipleFacilities_AllSpawn()
        {
            // Arrange: Multiple facilities at different locations
            var plantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            plantation.IsUnderConstruction = false;
            plantation.UpgradeLevel = 0;

            var farm = new Building(BuildingKind.Farm, new GridPos(5, 5));
            farm.IsUnderConstruction = false;
            farm.UpgradeLevel = 0;

            var buildings = new List<Building> { plantation, farm };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: 4 logs + 4 plant foods = 8 total
            Assert.AreEqual(8, spawned.Count);
            Assert.AreEqual(4, spawned.Count(r => r.Kind == ResourceKind.Logs));
            Assert.AreEqual(4, spawned.Count(r => r.Kind == ResourceKind.PlantFood));
        }

        [Test]
        public void SpawnIfNewCycle_UpgradedFacility_SpawnsTwicePerTile()
        {
            // Arrange: Upgraded plantation at (0, 0) and base at (5, 5)
            var upgradedPlantation = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            upgradedPlantation.IsUnderConstruction = false;
            upgradedPlantation.UpgradeLevel = 1;

            var basePlantation = new Building(BuildingKind.Plantation, new GridPos(5, 5));
            basePlantation.IsUnderConstruction = false;
            basePlantation.UpgradeLevel = 0;

            var buildings = new List<Building> { upgradedPlantation, basePlantation };
            var existingSpawned = new List<SpawnedResource>();

            // Act: Complete first cycle
            _clock.Advance(60f);
            var spawned = _spawner.SpawnIfNewCycle(buildings, existingSpawned);

            // Assert: 8 (upgraded) + 4 (base) = 12
            Assert.AreEqual(12, spawned.Count);
        }

        [Test]
        public void SpawnedResource_InitiallyNotCollected()
        {
            var resource = new SpawnedResource(ResourceKind.Logs, new GridPos(0, 0));
            Assert.IsFalse(resource.IsCollected);
        }

        [Test]
        public void SpawnedResource_CanMarkAsCollected()
        {
            var resource = new SpawnedResource(ResourceKind.Logs, new GridPos(0, 0));
            resource.IsCollected = true;
            Assert.IsTrue(resource.IsCollected);
        }
    }
}
