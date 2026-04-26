using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class ProductionSystemTests
    {
        [Test]
        public void FindNearestRelevantNode_WoodcutterFindsNearestTree()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 10)),    // Distance 10
                new NaturalNode(NaturalNodeType.Tree, new GridPos(7, 7)),      // Distance 4
                new NaturalNode(NaturalNodeType.Ore, new GridPos(3, 3)),       // Wrong type
                new NaturalNode(NaturalNodeType.Tree, new GridPos(2, 2))       // Distance 6
            };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest.Position, Is.EqualTo(new GridPos(7, 7)));
        }

        [Test]
        public void FindNearestRelevantNode_MinerFindsNearestOre()
        {
            // Arrange
            var agent = new Agent(new GridPos(0, 0), Profession.Miner, LifeStage.Adult);
            
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(5, 0)),
                new NaturalNode(NaturalNodeType.Ore, new GridPos(3, 0)),
                new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 0))
            };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest.Position, Is.EqualTo(new GridPos(3, 0)));
        }

        [Test]
        public void FindNearestRelevantNode_HunterFindsNearestMeat()
        {
            // Arrange
            var agent = new Agent(new GridPos(10, 10), Profession.Hunter, LifeStage.Adult);
            
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Animal, new GridPos(12, 12)),
                new NaturalNode(NaturalNodeType.Animal, new GridPos(15, 15))
            };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest.Position, Is.EqualTo(new GridPos(12, 12)));
        }

        [Test]
        public void FindNearestRelevantNode_FarmerFindsNearestPlantFood()
        {
            // Arrange
            var agent = new Agent(new GridPos(20, 20), Profession.Farmer, LifeStage.Adult);
            
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Plant, new GridPos(22, 22)),
                new NaturalNode(NaturalNodeType.Plant, new GridPos(21, 21))
            };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest.Position, Is.EqualTo(new GridPos(21, 21)));
        }

        [Test]
        public void FindNearestRelevantNode_IgnoresDepletedNodes()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            
            var depletedNode = new NaturalNode(NaturalNodeType.Tree, new GridPos(6, 6), 0);
            
            var validNode = new NaturalNode(NaturalNodeType.Tree, new GridPos(10, 10));

            var nodes = new List<NaturalNode> { depletedNode, validNode };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest, Is.EqualTo(validNode));
        }

        [Test]
        public void FindNearestRelevantNode_NoValidNodes_ReturnsNull()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            var nodes = new List<NaturalNode>
            {
                new NaturalNode(NaturalNodeType.Ore, new GridPos(6, 6)),
                new NaturalNode(NaturalNodeType.Animal, new GridPos(7, 7))
            };

            // Act
            var nearest = ProductionSystem.FindNearestRelevantNode(agent, nodes);

            // Assert
            Assert.That(nearest, Is.Null);
        }

        [Test]
        public void IsOnSameTileAsNode_WhenOnSameTile_ReturnsTrue()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5));
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5));

            // Act
            bool result = ProductionSystem.IsOnSameTileAsNode(agent, node);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsOnSameTileAsNode_WhenOnDifferentTile_ReturnsFalse()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5));
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(6, 6));

            // Act
            bool result = ProductionSystem.IsOnSameTileAsNode(agent, node);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsProducerProfession_CorrectlyIdentifiesProducers()
        {
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Woodcutter), Is.True);
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Miner), Is.True);
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Hunter), Is.True);
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Farmer), Is.True);
            
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Builder), Is.False);
            Assert.That(ProductionSystem.IsProducerProfession(Profession.Soldier), Is.False);
        }

        [Test]
        public void ProcessGathering_AdultAgent_GathersOneUnitPerSecond()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5), 10);
            float accumulator = 0f;

            // Act: 1 full second
            int gathered = ProductionSystem.ProcessGathering(agent, node, 1.0f, ref accumulator);

            // Assert
            Assert.That(gathered, Is.EqualTo(1));
            Assert.That(agent.CarriedResources, Is.EqualTo(1));
            Assert.That(node.Remaining, Is.EqualTo(9));
            Assert.That(accumulator, Is.EqualTo(0f));
        }

        [Test]
        public void ProcessGathering_ChildAgent_GathersHalfRate()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Child);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5), 10);
            float accumulator = 0f;

            // Act: 2 seconds needed for 1 unit at 0.5 rate
            ProductionSystem.ProcessGathering(agent, node, 1.0f, ref accumulator);
            int gathered = ProductionSystem.ProcessGathering(agent, node, 1.0f, ref accumulator);

            // Assert
            Assert.That(gathered, Is.EqualTo(1));
            Assert.That(agent.CarriedResources, Is.EqualTo(1));
            Assert.That(node.Remaining, Is.EqualTo(9));
        }

        [Test]
        public void ProcessGathering_FractionalDeltaTime_AccumulatesCorrectly()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5), 10);
            float accumulator = 0f;

            // Act: 10 ticks of 0.1s each
            int totalGathered = 0;
            for (int i = 0; i < 10; i++)
            {
                totalGathered += ProductionSystem.ProcessGathering(agent, node, 0.1f, ref accumulator);
            }

            // Assert
            Assert.That(totalGathered, Is.EqualTo(1));
            Assert.That(agent.CarriedResources, Is.EqualTo(1));
            Assert.That(node.Remaining, Is.EqualTo(9));
        }

        [Test]
        public void ProcessGathering_CarryFull_StopsGathering()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 10 };
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5), 10);
            float accumulator = 0f;

            // Act
            int gathered = ProductionSystem.ProcessGathering(agent, node, 10.0f, ref accumulator);

            // Assert
            Assert.That(gathered, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(10));
            Assert.That(node.Remaining, Is.EqualTo(10));
        }

        [Test]
        public void ProcessGathering_NodeDepleted_StopsGathering()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(5, 5), 3);
            float accumulator = 0f;

            // Act: Attempt to gather 5 units
            int gathered = ProductionSystem.ProcessGathering(agent, node, 5.0f, ref accumulator);

            // Assert
            Assert.That(gathered, Is.EqualTo(3));
            Assert.That(agent.CarriedResources, Is.EqualTo(3));
            Assert.That(node.Remaining, Is.EqualTo(0));
        }

        [Test]
        public void ProcessGathering_NotOnSameTile_ReturnsZero()
        {
            // Arrange
            var agent = new Agent(new GridPos(5, 5), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(6, 6), 10);
            float accumulator = 0f;

            // Act
            int gathered = ProductionSystem.ProcessGathering(agent, node, 10.0f, ref accumulator);

            // Assert
            Assert.That(gathered, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(0));
            Assert.That(node.Remaining, Is.EqualTo(10));
        }

        #region T-132 Deposit & Improvement Switch Tests

        [TestCase(48, 48, true)]
        [TestCase(49, 48, true)]
        [TestCase(48, 49, true)]
        [TestCase(49, 49, true)]
        [TestCase(50, 48, true)]
        [TestCase(48, 50, true)]
        [TestCase(50, 50, true)]
        [TestCase(47, 48, false)]
        [TestCase(48, 47, false)]
        [TestCase(51, 48, false)]
        [TestCase(48, 51, false)]
        public void IsAtCentralStorage_CorrectlyIdentifiesCentralFootprint(int x, int y, bool expected)
        {
            // Arrange
            var agent = new Agent(new GridPos(x, y), Profession.Woodcutter, LifeStage.Adult);
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48)); // 3x3 footprint at (48,48)
            
            // Act
            bool result = ProductionSystem.IsAtCentralStorage(agent, centralBuilding);
            
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DepositCarriedResources_AtCentral_DepositsCorrectResourceType()
        {
            // Arrange
            var agent = new Agent(new GridPos(49, 49), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 7 };
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage, centralBuilding);

            // Assert
            Assert.That(deposited, Is.EqualTo(7));
            Assert.That(agent.CarriedResources, Is.EqualTo(0));
            Assert.That(storage.GetStock(ResourceKind.Logs), Is.EqualTo(7));
            Assert.That(storage.GetStock(ResourceKind.Ore), Is.EqualTo(0));
        }

        [Test]
        public void DepositCarriedResources_NotAtCentral_ReturnsZero()
        {
            // Arrange
            var agent = new Agent(new GridPos(10, 10), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 7 };
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage, centralBuilding);

            // Assert
            Assert.That(deposited, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(7));
            Assert.That(storage.GetStock(ResourceKind.Logs), Is.EqualTo(0));
        }

        [Test]
        public void DepositCarriedResources_EmptyCarry_ReturnsZero()
        {
            // Arrange
            var agent = new Agent(new GridPos(49, 49), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 0 };
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage, centralBuilding);

            // Assert
            Assert.That(deposited, Is.EqualTo(0));
        }

        [Test]
        public void ShouldSwitchToImprovement_NoNodes_ReturnsTrue()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);

            // Act
            bool shouldSwitch = ProductionSystem.ShouldSwitchToImprovement(agent, null, 0, 1000);

            // Assert
            Assert.That(shouldSwitch, Is.True);
        }

        [Test]
        public void ShouldSwitchToImprovement_StockAboveStopThreshold_ReturnsTrue()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(51, 51), 100);

            // Default stop threshold for Woodcutter is 0.8
            // 900 / 1000 = 0.9 which is above 0.8
            bool shouldSwitch = ProductionSystem.ShouldSwitchToImprovement(agent, node, 900, 1000);

            // Assert
            Assert.That(shouldSwitch, Is.True);
        }

        [Test]
        public void ShouldSwitchToImprovement_StockBelowStopThreshold_ReturnsFalse()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(51, 51), 100);
            ProducerThresholds.ResetToDefaults();

            // 700 / 1000 = 0.7 which is below 0.8 stop threshold
            bool shouldSwitch = ProductionSystem.ShouldSwitchToImprovement(agent, node, 700, 1000);

            // Assert
            Assert.That(shouldSwitch, Is.False);
        }

        #endregion

        #region T-133 Improvement Loop Tests

        [Test]
        public void FindNearestImprovementTarget_WoodcutterFindsNearestPlantation()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);
            
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Plantation, new GridPos(60, 60)) { IsUnderConstruction = true }, // Distance 20
                new Building(BuildingKind.Plantation, new GridPos(55, 55)) { IsUnderConstruction = true }, // Distance 10
                new Building(BuildingKind.Quarry, new GridPos(40, 40)) { IsUnderConstruction = true },     // Wrong type
                new Building(BuildingKind.Plantation, new GridPos(45, 45)) { IsUnderConstruction = true }  // Distance 10
            };

            // Act
            var nearest = ProductionSystem.FindNearestImprovementTarget(agent, buildings);

            // Assert
            Assert.That(nearest.Anchor, Is.EqualTo(new GridPos(55, 55)));
        }

        [Test]
        public void FindNearestImprovementTarget_MinerFindsNearestQuarry()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Miner, LifeStage.Adult);
            
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Quarry, new GridPos(53, 53)) { IsUnderConstruction = true },
                new Building(BuildingKind.Quarry, new GridPos(56, 56)) { IsUnderConstruction = true }
            };

            // Act
            var nearest = ProductionSystem.FindNearestImprovementTarget(agent, buildings);

            // Assert
            Assert.That(nearest.Anchor, Is.EqualTo(new GridPos(53, 53)));
        }

        [Test]
        public void FindNearestImprovementTarget_IgnoresCompletedBuildings()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);
            
            var completedBuilding = new Building(BuildingKind.Plantation, new GridPos(51, 51)) 
            { 
                IsUnderConstruction = false, 
                UpgradeLevel = 1 
            };
            
            var validBuilding = new Building(BuildingKind.Plantation, new GridPos(60, 60)) 
            { 
                IsUnderConstruction = true 
            };

            var buildings = new List<Building> { completedBuilding, validBuilding };

            // Act
            var nearest = ProductionSystem.FindNearestImprovementTarget(agent, buildings);

            // Assert
            Assert.That(nearest, Is.EqualTo(validBuilding));
        }

        [Test]
        public void FindNearestImprovementTarget_NoValidTargets_ReturnsNull()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult);
            var buildings = new List<Building>
            {
                new Building(BuildingKind.House, new GridPos(51, 51)),
                new Building(BuildingKind.Central, new GridPos(49, 49))
            };

            // Act
            var nearest = ProductionSystem.FindNearestImprovementTarget(agent, buildings);

            // Assert
            Assert.That(nearest, Is.Null);
        }

        [Test]
        public void WithdrawResourcesForImprovement_AtCentral_WithdrawsCorrectAmount()
        {
            // Arrange
            var agent = new Agent(new GridPos(49, 49), Profession.Woodcutter, LifeStage.Adult);
            var target = new Building(BuildingKind.Plantation, new GridPos(60, 60)) { ConstructionProgress = 20 };
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));
            storage.Deposit(ResourceKind.Logs, 100);

            // Act
            int withdrawn = ProductionSystem.WithdrawResourcesForImprovement(agent, target, storage, centralBuilding);

            // Assert
            Assert.That(withdrawn, Is.EqualTo(10)); // Carry capacity 10
            Assert.That(agent.CarriedResources, Is.EqualTo(10));
            Assert.That(storage.GetStock(ResourceKind.Logs), Is.EqualTo(90));
        }

        [Test]
        public void WithdrawResourcesForImprovement_NotAtCentral_ReturnsZero()
        {
            // Arrange
            var agent = new Agent(new GridPos(10, 10), Profession.Woodcutter, LifeStage.Adult);
            var target = new Building(BuildingKind.Plantation, new GridPos(60, 60));
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));
            storage.Deposit(ResourceKind.Logs, 100);

            // Act
            int withdrawn = ProductionSystem.WithdrawResourcesForImprovement(agent, target, storage, centralBuilding);

            // Assert
            Assert.That(withdrawn, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(0));
            Assert.That(storage.GetStock(ResourceKind.Logs), Is.EqualTo(100));
        }

        [Test]
        public void WithdrawResourcesForImprovement_WithdrawsOnlyRemainingRequired()
        {
            // Arrange
            var agent = new Agent(new GridPos(49, 49), Profession.Woodcutter, LifeStage.Adult);
            var target = new Building(BuildingKind.Plantation, new GridPos(60, 60)) { ConstructionProgress = 95 }; // Need 5 more
            var storage = new CentralStorage();
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(48, 48));
            storage.Deposit(ResourceKind.Logs, 100);

            // Act
            int withdrawn = ProductionSystem.WithdrawResourcesForImprovement(agent, target, storage, centralBuilding);

            // Assert
            Assert.That(withdrawn, Is.EqualTo(5));
            Assert.That(agent.CarriedResources, Is.EqualTo(5));
        }

        [TestCase(50, 50, true)]
        [TestCase(51, 51, true)]
        [TestCase(49, 49, false)]
        [TestCase(48, 50, false)]
        [TestCase(50, 52, false)]
        public void IsAtBuildingSite_CorrectlyIdentifiesBuildingFootprint(int agentX, int agentY, bool expected)
        {
            // Arrange
            var agent = new Agent(new GridPos(agentX, agentY), Profession.Woodcutter, LifeStage.Adult);
            var building = new Building(BuildingKind.Plantation, new GridPos(50, 50)); // 2x2 footprint

            // Act
            bool result = ProductionSystem.IsAtBuildingSite(agent, building);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DeliverResourcesToBuilding_AtSite_DeliversResourcesAndSchedulesBuildTime()
        {
            // Arrange
            var clock = new SimulationClock();
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 10 };
            var target = new Building(BuildingKind.Plantation, new GridPos(50, 50)) { ConstructionProgress = 90, SimulationClock = clock };

            // Act
            int delivered = ProductionSystem.DeliverResourcesToBuilding(agent, target, clock);

            // Assert
            Assert.That(delivered, Is.EqualTo(10));
            Assert.That(agent.CarriedResources, Is.EqualTo(0));
            Assert.That(target.ConstructionProgress, Is.EqualTo(100)); // 90 + 10
            Assert.That(target.BuildTimeEndSeconds, Is.GreaterThan(0)); // Build time scheduled
        }

        [Test]
        public void DeliverResourcesToBuilding_NotAtSite_ReturnsZero()
        {
            // Arrange
            var clock = new SimulationClock();
            var agent = new Agent(new GridPos(10, 10), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 10 };
            var target = new Building(BuildingKind.Plantation, new GridPos(50, 50));

            // Act
            int delivered = ProductionSystem.DeliverResourcesToBuilding(agent, target, clock);

            // Assert
            Assert.That(delivered, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(10));
            Assert.That(target.ConstructionProgress, Is.EqualTo(0));
        }

        [Test]
        public void GetRequiredBuildingForProfession_CorrectMappings()
        {
            Assert.That(ProductionSystem.GetRequiredBuildingForProfession(Profession.Woodcutter), Is.EqualTo(BuildingKind.Plantation));
            Assert.That(ProductionSystem.GetRequiredBuildingForProfession(Profession.Miner), Is.EqualTo(BuildingKind.Quarry));
            Assert.That(ProductionSystem.GetRequiredBuildingForProfession(Profession.Hunter), Is.EqualTo(BuildingKind.CattleFarm));
            Assert.That(ProductionSystem.GetRequiredBuildingForProfession(Profession.Farmer), Is.EqualTo(BuildingKind.Farm));
        }

        #endregion
    }
}
