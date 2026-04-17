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
                new NaturalNode(new GridPos(10, 10), ResourceKind.Logs),    // Distance 10
                new NaturalNode(new GridPos(7, 7), ResourceKind.Logs),      // Distance 4
                new NaturalNode(new GridPos(3, 3), ResourceKind.Ore),       // Wrong type
                new NaturalNode(new GridPos(2, 2), ResourceKind.Logs)       // Distance 6
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
                new NaturalNode(new GridPos(5, 0), ResourceKind.Ore),
                new NaturalNode(new GridPos(3, 0), ResourceKind.Ore),
                new NaturalNode(new GridPos(10, 0), ResourceKind.Logs)
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
                new NaturalNode(new GridPos(12, 12), ResourceKind.Meat),
                new NaturalNode(new GridPos(15, 15), ResourceKind.Meat)
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
                new NaturalNode(new GridPos(22, 22), ResourceKind.PlantFood),
                new NaturalNode(new GridPos(21, 21), ResourceKind.PlantFood)
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
            
            var depletedNode = new NaturalNode(new GridPos(6, 6), ResourceKind.Logs);
            depletedNode.Remaining = 0;
            
            var validNode = new NaturalNode(new GridPos(10, 10), ResourceKind.Logs);

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
                new NaturalNode(new GridPos(6, 6), ResourceKind.Ore),
                new NaturalNode(new GridPos(7, 7), ResourceKind.Meat)
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
            var node = new NaturalNode(new GridPos(5, 5), ResourceKind.Logs);

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
            var node = new NaturalNode(new GridPos(6, 6), ResourceKind.Logs);

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
    }
}