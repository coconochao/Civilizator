using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class QuarrySupportTests
    {
        [Test]
        public void IsNodeSupportedByQuarry_OreNodeOverlappingQuarry_ReturnsTrue()
        {
            // Arrange: Quarry at (0, 0) with 2×2 footprint covers (0,0), (1,0), (0,1), (1,1)
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            quarry.IsUnderConstruction = false;
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            var quarries = new List<Building> { quarry };

            // Act
            bool isSupported = QuarrySupport.IsNodeSupportedByQuarry(oreNode, quarries);

            // Assert
            Assert.IsTrue(isSupported);
        }

        [Test]
        public void IsNodeSupportedByQuarry_OreNodeOverlappingQuarryEdge_ReturnsTrue()
        {
            // Arrange: Quarry at (5, 5) covers (5,5), (6,5), (5,6), (6,6)
            var quarry = new Building(new GridPos(5, 5), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(6, 6), 50);
            var quarries = new List<Building> { quarry };

            // Act
            bool isSupported = QuarrySupport.IsNodeSupportedByQuarry(oreNode, quarries);

            // Assert
            Assert.IsTrue(isSupported);
        }

        [Test]
        public void IsNodeSupportedByQuarry_OreNodeNotOverlappingQuarry_ReturnsFalse()
        {
            // Arrange: Quarry at (0, 0), ore node at (2, 0) - no overlap
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(2, 0), 50);
            var quarries = new List<Building> { quarry };

            // Act
            bool isSupported = QuarrySupport.IsNodeSupportedByQuarry(oreNode, quarries);

            // Assert
            Assert.IsFalse(isSupported);
        }

        [Test]
        public void IsNodeSupportedByQuarry_MultipleQuarries_FirstSupporting_ReturnsTrue()
        {
            // Arrange: Multiple quarries, second one supports the ore node
            var quarry1 = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            var quarry2 = new Building(new GridPos(5, 5), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(5, 5), 50);
            var quarries = new List<Building> { quarry1, quarry2 };

            // Act
            bool isSupported = QuarrySupport.IsNodeSupportedByQuarry(oreNode, quarries);

            // Assert
            Assert.IsTrue(isSupported);
        }

        [Test]
        public void IsNodeSupportedByQuarry_NoQuarries_ReturnsFalse()
        {
            // Arrange: No quarries
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            var quarries = new List<Building>();

            // Act
            bool isSupported = QuarrySupport.IsNodeSupportedByQuarry(oreNode, quarries);

            // Assert
            Assert.IsFalse(isSupported);
        }

        [Test]
        public void GetOreGatheringRateMultiplier_NormalGathering_Returns1()
        {
            // Arrange: Node not depleted
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);

            // Act
            float multiplier = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, false, quarry);

            // Assert
            Assert.AreEqual(1.0f, multiplier);
        }

        [Test]
        public void GetOreGatheringRateMultiplier_DepletedNoQuarry_Returns1()
        {
            // Arrange: Node depleted but no quarry support
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);

            // Act
            float multiplier = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, true, null);

            // Assert
            Assert.AreEqual(1.0f, multiplier);
        }

        [Test]
        public void GetOreGatheringRateMultiplier_DepletedWithBaseQuarry_Returns0_5()
        {
            // Arrange: Node depleted with base (non-upgraded) quarry support
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            var baseQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            baseQuarry.UpgradeLevel = 0;

            // Act
            float multiplier = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, true, baseQuarry);

            // Assert
            Assert.AreEqual(0.5f, multiplier);
        }

        [Test]
        public void GetOreGatheringRateMultiplier_DepletedWithUpgradedQuarry_Returns1()
        {
            // Arrange: Node depleted with upgraded quarry support
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            var upgradedQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            upgradedQuarry.UpgradeLevel = 1;

            // Act
            float multiplier = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, true, upgradedQuarry);

            // Assert
            Assert.AreEqual(1.0f, multiplier);
        }

        [Test]
        public void GetOreGatheringTimeMultiplier_NormalGathering_Returns1()
        {
            // Arrange: Node not depleted
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);

            // Act
            float multiplier = QuarrySupport.GetOreGatheringTimeMultiplier(oreNode, false, quarry);

            // Assert
            Assert.AreEqual(1.0f, multiplier);
        }

        [Test]
        public void GetOreGatheringTimeMultiplier_DepletedWithBaseQuarry_Returns2()
        {
            // Arrange: Node depleted with base quarry - should take 2× time
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            var baseQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            baseQuarry.UpgradeLevel = 0;

            // Act
            float multiplier = QuarrySupport.GetOreGatheringTimeMultiplier(oreNode, true, baseQuarry);

            // Assert
            Assert.AreEqual(2.0f, multiplier);
        }

        [Test]
        public void GetOreGatheringTimeMultiplier_DepletedWithUpgradedQuarry_Returns1()
        {
            // Arrange: Node depleted with upgraded quarry - normal time
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            var upgradedQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            upgradedQuarry.UpgradeLevel = 1;

            // Act
            float multiplier = QuarrySupport.GetOreGatheringTimeMultiplier(oreNode, true, upgradedQuarry);

            // Assert
            Assert.AreEqual(1.0f, multiplier);
        }

        [Test]
        public void FindSupportingQuarry_WithSupportingQuarry_ReturnsQuarry()
        {
            // Arrange: Quarry that overlaps ore node
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(1, 1), 50);
            var quarries = new List<Building> { quarry };

            // Act
            var supportingQuarry = QuarrySupport.FindSupportingQuarry(oreNode, quarries);

            // Assert
            Assert.AreEqual(quarry, supportingQuarry);
        }

        [Test]
        public void FindSupportingQuarry_NoSupportingQuarry_ReturnsNull()
        {
            // Arrange: Quarry that doesn't overlap ore node
            var quarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(5, 5), 50);
            var quarries = new List<Building> { quarry };

            // Act
            var supportingQuarry = QuarrySupport.FindSupportingQuarry(oreNode, quarries);

            // Assert
            Assert.IsNull(supportingQuarry);
        }

        [Test]
        public void FindSupportingQuarry_MultipleQuarries_ReturnsFirst()
        {
            // Arrange: Multiple quarries that support ore node
            var quarry1 = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            var quarry2 = new Building(new GridPos(1, 0), BuildingKind.Quarry);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(1, 1), 50);
            var quarries = new List<Building> { quarry1, quarry2 };

            // Act
            var supportingQuarry = QuarrySupport.FindSupportingQuarry(oreNode, quarries);

            // Assert
            Assert.IsNotNull(supportingQuarry);
            Assert.IsTrue(quarries.Contains(supportingQuarry));
        }

        [Test]
        public void RateAndTimeMultipliers_AreReciprocals()
        {
            // Arrange: Various scenarios
            var baseQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            baseQuarry.UpgradeLevel = 0;
            var upgradedQuarry = new Building(new GridPos(0, 0), BuildingKind.Quarry);
            upgradedQuarry.UpgradeLevel = 1;
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);

            // Test base quarry
            float baseRate = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, true, baseQuarry);
            float baseTime = QuarrySupport.GetOreGatheringTimeMultiplier(oreNode, true, baseQuarry);

            // Test upgraded quarry
            float upgradedRate = QuarrySupport.GetOreGatheringRateMultiplier(oreNode, true, upgradedQuarry);
            float upgradedTime = QuarrySupport.GetOreGatheringTimeMultiplier(oreNode, true, upgradedQuarry);

            // Assert: multipliers are reciprocals (within floating point tolerance)
            Assert.AreEqual(1.0f / baseTime, baseRate, 0.0001f);
            Assert.AreEqual(1.0f / upgradedTime, upgradedRate, 0.0001f);
        }
    }
}
