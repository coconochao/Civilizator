using NUnit.Framework;
using Civilizator.Simulation;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class BuildingPlacementTests
    {
        private List<Building> buildings;

        [SetUp]
        public void Setup()
        {
            buildings = new List<Building>();
        }

        [Test]
        public void CanPlaceBuilding_EmptyWorld_ShouldSucceed()
        {
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(10, 10));
            Assert.IsTrue(canPlace);
        }

        [Test]
        public void CanPlaceBuilding_AdjacentPlacement_ShouldFail()
        {
            // Place a 2x2 house at (10, 10)
            buildings.Add(new Building(BuildingKind.House, new GridPos(10, 10)));

            // Try to place another 2x2 house adjacent to it (no gap)
            // Adjacent means touching edges. Gap 0 means Chebyshev distance < 1
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(12, 10));
            Assert.IsFalse(canPlace, "Should not allow adjacent placement (0 tile gap)");
        }

        [Test]
        public void CanPlaceBuilding_GapOne_ShouldSucceed()
        {
            // Place a 2x2 house at (10, 10) - occupies tiles (10,10), (10,11), (11,10), (11,11)
            buildings.Add(new Building(BuildingKind.House, new GridPos(10, 10)));

            // Place another 2x2 house with 1 tile gap - at (13, 10)
            // This occupies tiles (13,10), (13,11), (14,10), (14,11)
            // Chebyshev distance from (11, y) to (13, y) is 2 (gap of 1 tile)
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(13, 10));
            Assert.IsTrue(canPlace, "Should allow placement with 1 tile gap");
        }

        [Test]
        public void CanPlaceBuilding_OverlapWithExisting_ShouldFail()
        {
            // Place a 2x2 house at (10, 10)
            buildings.Add(new Building(BuildingKind.House, new GridPos(10, 10)));

            // Try to place another 2x2 house that overlaps
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(11, 11));
            Assert.IsFalse(canPlace, "Should not allow overlapping placement");
        }

        [Test]
        public void CanPlaceBuilding_OutsideMapBounds_ShouldFail()
        {
            // Try to place at the edge where footprint extends beyond bounds
            int mapWidth = GridPos.MapWidth;
            int mapHeight = GridPos.MapHeight;

            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(mapWidth - 1, mapHeight - 1));
            Assert.IsFalse(canPlace, "Should not allow placement extending outside map bounds");
        }

        [Test]
        public void CanPlaceBuilding_ValidCornerPlacement_ShouldSucceed()
        {
            // Place at (0, 0) - within bounds for a 2x2
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(0, 0));
            Assert.IsTrue(canPlace);
        }

        [Test]
        public void CanPlaceBuilding_CentralBuildingLargerFootprint_ShouldFail()
        {
            // Central building is 3x3, so it extends beyond the map at (98, 98)
            // 98 + 3 = 101, which exceeds map width of 100
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.Central, new GridPos(98, 98));
            Assert.IsFalse(canPlace);
        }

        [Test]
        public void CanPlaceBuilding_CentralBuildingValidPlacement_ShouldSucceed()
        {
            // Central building is 3x3, so place at (97, 97)
            // 97 + 3 = 100, which is valid
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.Central, new GridPos(97, 97));
            Assert.IsTrue(canPlace);
        }

        [Test]
        public void CanPlaceBuilding_MultipleBuildings_GapEnforced()
        {
            // Place house 1 at (10, 10)
            buildings.Add(new Building(BuildingKind.House, new GridPos(10, 10)));

            // Place house 2 at (15, 10) - gap of 2 (from 11 to 13)
            buildings.Add(new Building(BuildingKind.House, new GridPos(15, 10)));

            // Try to place house 3 between them with gap 0.5 (at 12, 10) - too close to house 1
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(12, 10));
            Assert.IsFalse(canPlace, "Should not allow placement too close to any existing building");
        }

        [Test]
        public void CanPlaceBuilding_DiagonalGap_ShouldSucceed()
        {
            // Place house at (10, 10)
            buildings.Add(new Building(BuildingKind.House, new GridPos(10, 10)));

            // Place another at (13, 13) - diagonally positioned with gap in both directions
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, new GridPos(13, 13));
            Assert.IsTrue(canPlace);
        }
    }
}
