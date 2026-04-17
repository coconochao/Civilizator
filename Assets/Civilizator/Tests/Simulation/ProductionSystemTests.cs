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

        [TestCase(49, 49, true)]
        [TestCase(50, 50, true)]
        [TestCase(51, 51, true)]
        [TestCase(48, 50, false)]
        [TestCase(50, 48, false)]
        [TestCase(52, 50, false)]
        [TestCase(50, 52, false)]
        public void IsAtCentralStorage_CorrectlyIdentifiesCentralFootprint(int x, int y, bool expected)
        {
            // Arrange
            var agent = new Agent(new GridPos(x, y), Profession.Woodcutter, LifeStage.Adult);
            
            // Act
            bool result = ProductionSystem.IsAtCentralStorage(agent);
            
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DepositCarriedResources_AtCentral_DepositsCorrectResourceType()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 7 };
            var storage = new CentralStorage();

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage);

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

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage);

            // Assert
            Assert.That(deposited, Is.EqualTo(0));
            Assert.That(agent.CarriedResources, Is.EqualTo(7));
            Assert.That(storage.GetStock(ResourceKind.Logs), Is.EqualTo(0));
        }

        [Test]
        public void DepositCarriedResources_EmptyCarry_ReturnsZero()
        {
            // Arrange
            var agent = new Agent(new GridPos(50, 50), Profession.Woodcutter, LifeStage.Adult) { CarriedResources = 0 };
            var storage = new CentralStorage();

            // Act
            int deposited = ProductionSystem.DepositCarriedResources(agent, storage);

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

            // 700 / 1000 = 0.7 which is below 0.8 stop threshold
            bool shouldSwitch = ProductionSystem.ShouldSwitchToImprovement(agent, node, 700, 1000);

            // Assert
            Assert.That(shouldSwitch, Is.False);
        }

        #endregion
    }
}
