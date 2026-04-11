using NUnit.Framework;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class NaturalNodeTests
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            var pos = new GridPos(10, 20);
            var node = new NaturalNode(NaturalNodeType.Tree, pos);
            
            Assert.AreEqual(NaturalNodeType.Tree, node.Type);
            Assert.AreEqual(pos, node.Position);
            Assert.AreEqual(100, node.Remaining);
            Assert.IsFalse(node.IsDepleted);
        }

        [Test]
        public void Gather_ReducesRemaining()
        {
            var node = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0));
            int gathered = node.Gather(30);
            
            Assert.AreEqual(30, gathered);
            Assert.AreEqual(70, node.Remaining);
            Assert.IsFalse(node.IsDepleted);
        }

        [Test]
        public void Gather_MoreThanRemaining_ReturnsActualAmount()
        {
            var node = new NaturalNode(NaturalNodeType.Plant, new GridPos(5, 5));
            int gathered = node.Gather(150); // More than 100 remaining
            
            Assert.AreEqual(100, gathered);
            Assert.AreEqual(0, node.Remaining);
            Assert.IsTrue(node.IsDepleted);
        }

        [Test]
        public void Gather_FromDepleted_ReturnsZero()
        {
            var node = new NaturalNode(NaturalNodeType.Animal, new GridPos(15, 15), 0);
            int gathered = node.Gather(50);
            
            Assert.AreEqual(0, gathered);
            Assert.AreEqual(0, node.Remaining);
            Assert.IsTrue(node.IsDepleted);
        }

        [Test]
        public void Gather_NegativeAmount_ReturnsZero()
        {
            var node = new NaturalNode(NaturalNodeType.Tree, new GridPos(25, 25));
            int gathered = node.Gather(-10);
            
            Assert.AreEqual(0, gathered);
            Assert.AreEqual(100, node.Remaining); // Unchanged
        }

        [Test]
        public void Constructor_WithCustomRemaining()
        {
            var node = new NaturalNode(NaturalNodeType.Ore, new GridPos(50, 50), 45);
            Assert.AreEqual(45, node.Remaining);
        }

        [Test]
        public void Constructor_NegativeRemaining_ClampsToZero()
        {
            var node = new NaturalNode(NaturalNodeType.Plant, new GridPos(60, 60), -10);
            Assert.AreEqual(0, node.Remaining);
            Assert.IsTrue(node.IsDepleted);
        }

        [Test]
        public void GatherMultiple_DepletesCompletely()
        {
            var node = new NaturalNode(NaturalNodeType.Animal, new GridPos(70, 70));
            
            node.Gather(40);
            Assert.AreEqual(60, node.Remaining);
            Assert.IsFalse(node.IsDepleted);
            
            node.Gather(50);
            Assert.AreEqual(10, node.Remaining);
            Assert.IsFalse(node.IsDepleted);
            
            node.Gather(10);
            Assert.AreEqual(0, node.Remaining);
            Assert.IsTrue(node.IsDepleted);
        }

        [Test]
        public void AllNodeTypes_CanBeCreated()
        {
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(0, 0));
            var plant = new NaturalNode(NaturalNodeType.Plant, new GridPos(1, 1));
            var animal = new NaturalNode(NaturalNodeType.Animal, new GridPos(2, 2));
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(3, 3));
            
            Assert.AreEqual(NaturalNodeType.Tree, tree.Type);
            Assert.AreEqual(NaturalNodeType.Plant, plant.Type);
            Assert.AreEqual(NaturalNodeType.Animal, animal.Type);
            Assert.AreEqual(NaturalNodeType.Ore, ore.Type);
        }

        [Test]
        public void InitialAmount_IsPublicConstant()
        {
            Assert.AreEqual(100, NaturalNode.InitialAmount);
        }
    }

    [TestFixture]
    public class WorldGeneratorQuickTests
    {
        [Test]
        public void GenerateNodes_Returns400Nodes()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            Assert.AreEqual(400, nodes.Count);
        }

        [Test]
        public void GenerateNodes_NoTileCollisions()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            var positions = new System.Collections.Generic.HashSet<GridPos>();

            foreach (var node in nodes)
            {
                Assert.IsFalse(positions.Contains(node.Position), 
                    $"Duplicate position found: {node.Position}");
                positions.Add(node.Position);
            }

            Assert.AreEqual(400, positions.Count);
        }

        [Test]
        public void GenerateNodes_DeterministicWithSameSeed()
        {
            var nodes1 = WorldGenerator.GenerateNodes(123);
            var nodes2 = WorldGenerator.GenerateNodes(123);

            Assert.AreEqual(400, nodes1.Count);
            Assert.AreEqual(400, nodes2.Count);

            for (int i = 0; i < nodes1.Count; i++)
            {
                Assert.AreEqual(nodes1[i].Type, nodes2[i].Type);
                Assert.AreEqual(nodes1[i].Position, nodes2[i].Position);
            }
        }
    }
}
