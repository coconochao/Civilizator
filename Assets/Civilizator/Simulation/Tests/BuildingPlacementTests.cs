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

        // ===== Resource Facility Placement Tests =====

        [Test]
        public void CanPlaceBuilding_ResourceFacilityWithoutNodes_ShouldFail()
        {
            // Attempting to place a Plantation without providing natural nodes should fail
            var emptyNodes = new List<NaturalNode>();
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Plantation, new GridPos(10, 10), emptyNodes);
            Assert.IsFalse(canPlace, "Should not allow resource facility without matching node");
        }

        [Test]
        public void CanPlaceBuilding_PlantationOverlapsTreeNode_ShouldSucceed()
        {
            // Create a Tree node at (10, 10)
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 10))
            };

            // Place Plantation with anchor (10, 10) - footprint 2x2, overlaps the node
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Plantation, new GridPos(10, 10), nodes);
            Assert.IsTrue(canPlace, "Plantation should succeed when overlapping matching Tree node");
        }

        [Test]
        public void CanPlaceBuilding_PlantationNodesWrongType_ShouldFail()
        {
            // Create an Ore node (not a Tree)
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(10, 10))
            };

            // Try to place Plantation over the Ore node
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Plantation, new GridPos(10, 10), nodes);
            Assert.IsFalse(canPlace, "Plantation should fail when overlapping non-Tree node");
        }

        [Test]
        public void CanPlaceBuilding_FarmOverlapsPlanNode_ShouldSucceed()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Plant, new GridPos(11, 11))
            };

            // Anchor at (10, 10), footprint 2x2 covers (10,10), (10,11), (11,10), (11,11)
            // The Plant node at (11, 11) is within this footprint
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Farm, new GridPos(10, 10), nodes);
            Assert.IsTrue(canPlace, "Farm should succeed when overlapping matching Plant node");
        }

        [Test]
        public void CanPlaceBuilding_CattleFarmOverlapsAnimalNode_ShouldSucceed()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Animal, new GridPos(20, 20))
            };

            // Anchor at (19, 19), footprint 2x2 covers (19,19) to (20,20)
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.CattleFarm, new GridPos(19, 19), nodes);
            Assert.IsTrue(canPlace, "CattleFarm should succeed when overlapping matching Animal node");
        }

        [Test]
        public void CanPlaceBuilding_QuarryOverlapsOreNode_ShouldSucceed()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(30, 30))
            };

            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Quarry, new GridPos(30, 30), nodes);
            Assert.IsTrue(canPlace, "Quarry should succeed when overlapping matching Ore node");
        }

        [Test]
        public void CanPlaceBuilding_ResourceFacilityNodeOutsideFootprint_ShouldFail()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Tree, new GridPos(15, 15))
            };

            // Anchor at (10, 10), footprint 2x2 covers (10,10) to (11,11)
            // Node at (15, 15) is outside this footprint
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Plantation, new GridPos(10, 10), nodes);
            Assert.IsFalse(canPlace, "Plantation should fail when no node overlaps its footprint");
        }

        [Test]
        public void CanPlaceBuilding_ResourceFacilityMultipleNodes_AtLeastOneMatches_ShouldSucceed()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(10, 10)),
                new NaturalNode(NaturalNodeType.Plant, new GridPos(11, 11)),
                new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 11))
            };

            // Anchor at (10, 10), footprint 2x2
            // Ore node at (10, 10) overlaps - but we're placing a Farm
            // Plant node at (11, 11) overlaps - this matches!
            // Tree node at (10, 11) overlaps - doesn't match
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Farm, new GridPos(10, 10), nodes);
            Assert.IsTrue(canPlace, "Farm should succeed when at least one matching Plant node overlaps");
        }

        [Test]
        public void CanPlaceBuilding_NonResourceFacilityIgnoresNodes()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 10))
            };

            // House placement should not care about matching nodes
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.House, new GridPos(10, 10), nodes);
            Assert.IsTrue(canPlace, "Non-resource buildings should not require matching nodes");
        }

        [Test]
        public void CanPlaceBuilding_ResourceFacilityOnNodeBoundary_ShouldSucceed()
        {
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(11, 11))
            };

            // Anchor at (10, 10), footprint 2x2 covers (10,10), (10,11), (11,10), (11,11)
            // Node at (11, 11) is at the boundary of the footprint - should overlap
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Quarry, new GridPos(10, 10), nodes);
            Assert.IsTrue(canPlace, "Resource facility should succeed when node is on footprint boundary");
        }

        [Test]
        public void CanPlaceBuilding_ResourceFacilityNullNodesList_ShouldFail()
        {
            // Placing a resource facility with null nodes list should fail
            bool canPlace = BuildingPlacementValidator.CanPlaceBuilding(
                buildings, BuildingKind.Plantation, new GridPos(10, 10), null);
            Assert.IsFalse(canPlace, "Resource facility with null nodes list should fail");
        }

        // ===== Building State Tests (T-050) =====

        [Test]
        public void Building_CreatedWithDefaultState()
        {
            var building = new Building(BuildingKind.House, new GridPos(10, 10));

            Assert.AreEqual(BuildingKind.House, building.Kind);
            Assert.AreEqual(new GridPos(10, 10), building.Anchor);
            Assert.IsFalse(building.IsUnderConstruction, "Building should not be under construction by default");
            Assert.AreEqual(0, building.UpgradeLevel, "Building should start at upgrade level 0");
        }

        [Test]
        public void Building_CanSetUnderConstruction()
        {
            var building = new Building(BuildingKind.Tower, new GridPos(20, 20));

            building.IsUnderConstruction = true;
            Assert.IsTrue(building.IsUnderConstruction);

            building.IsUnderConstruction = false;
            Assert.IsFalse(building.IsUnderConstruction);
        }

        [Test]
        public void Building_CanSetUpgradeLevel()
        {
            var building = new Building(BuildingKind.Plantation, new GridPos(30, 30));

            Assert.AreEqual(0, building.UpgradeLevel);

            building.UpgradeLevel = 1;
            Assert.AreEqual(1, building.UpgradeLevel, "Building should support upgrade level 1");

            building.UpgradeLevel = 0;
            Assert.AreEqual(0, building.UpgradeLevel);
        }

        [Test]
        public void Building_UpgradeLevelMaxIsOne()
        {
            var building = new Building(BuildingKind.Central, new GridPos(0, 0));

            // The spec specifies max 1 upgrade in V1
            building.UpgradeLevel = 1;
            Assert.AreEqual(1, building.UpgradeLevel, "Max upgrade level should be 1 in V1");
        }
    }
}
