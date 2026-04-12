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
    public class NaturalNodeGatherabilityTests
    {
        [Test]
        public void IsGatherable_NormalNodeWithRemaining_ReturnsTrue()
        {
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(0, 0), 50);
            Assert.IsTrue(tree.IsGatherable());
        }

        [Test]
        public void IsGatherable_NormalNodeDepleted_ReturnsFalse()
        {
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(0, 0), 0);
            Assert.IsFalse(tree.IsGatherable());
        }

        [Test]
        public void IsGatherable_AllNormalNodeTypes_DependOnRemaining()
        {
            // Plant with remaining
            var plant = new NaturalNode(NaturalNodeType.Plant, new GridPos(0, 0), 25);
            Assert.IsTrue(plant.IsGatherable());

            // Plant depleted
            var plantDepleted = new NaturalNode(NaturalNodeType.Plant, new GridPos(1, 1), 0);
            Assert.IsFalse(plantDepleted.IsGatherable());

            // Animal with remaining
            var animal = new NaturalNode(NaturalNodeType.Animal, new GridPos(2, 2), 10);
            Assert.IsTrue(animal.IsGatherable());

            // Animal depleted
            var animalDepleted = new NaturalNode(NaturalNodeType.Animal, new GridPos(3, 3), 0);
            Assert.IsFalse(animalDepleted.IsGatherable());
        }

        [Test]
        public void IsGatherable_OreWithoutQuarry_DepletedReturnsFalse()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            Assert.IsFalse(ore.IsGatherable(hasQuarrySupport: false));
        }

        [Test]
        public void IsGatherable_OreWithoutQuarry_WithRemainingReturnsTrue()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: false));
        }

        [Test]
        public void IsGatherable_OreWithQuarry_DepletedReturnsTrue()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void IsGatherable_OreWithQuarry_WithRemainingReturnsTrue()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void IsGatherable_OreWithQuarry_NegativeRemainingReturnsTrue()
        {
            // After multiple gathers, remaining might go slightly negative (clamped to 0)
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 0);
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void IsGatherable_TreeCannotUseQuarrySupport()
        {
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(0, 0), 0);
            // Quarry support is ignored for non-ore nodes
            Assert.IsFalse(tree.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void IsGatherable_PlantCannotUseQuarrySupport()
        {
            var plant = new NaturalNode(NaturalNodeType.Plant, new GridPos(0, 0), 0);
            Assert.IsFalse(plant.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void IsGatherable_AnimalCannotUseQuarrySupport()
        {
            var animal = new NaturalNode(NaturalNodeType.Animal, new GridPos(0, 0), 0);
            Assert.IsFalse(animal.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void GatherAfterDepletion_WithQuarry_StaysGatherable()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 50);

            // Gather all remaining
            ore.Gather(50);
            Assert.AreEqual(0, ore.Remaining);
            Assert.IsFalse(ore.IsGatherable(hasQuarrySupport: false));
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: true));

            // Continue gathering with quarry support (no-op on remaining, but node stays gatherable)
            ore.Gather(10);
            Assert.AreEqual(0, ore.Remaining);
            Assert.IsTrue(ore.IsGatherable(hasQuarrySupport: true));
        }

        [Test]
        public void GatherAfterDepletion_WithoutQuarry_BecomesUngatherable()
        {
            var ore = new NaturalNode(NaturalNodeType.Ore, new GridPos(0, 0), 30);

            // Gather all remaining
            ore.Gather(30);
            Assert.AreEqual(0, ore.Remaining);
            Assert.IsFalse(ore.IsGatherable(hasQuarrySupport: false));
        }
    }
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
