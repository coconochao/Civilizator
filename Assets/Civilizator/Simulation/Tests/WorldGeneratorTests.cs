using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class WorldGeneratorTests
    {
        [Test]
        public void GenerateNodes_Returns400Nodes()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            Assert.AreEqual(400, nodes.Count);
        }

        [Test]
        public void GenerateNodes_ProducesFourTypesPerRegion()
        {
            var nodes = WorldGenerator.GenerateNodes(42);

            var typesByRegion = new Dictionary<(int, int), HashSet<NaturalNodeType>>();

            foreach (var node in nodes)
            {
                int regionX = node.Position.X / WorldGenerator.TilesPerRegion;
                int regionY = node.Position.Y / WorldGenerator.TilesPerRegion;
                var regionKey = (regionX, regionY);

                if (!typesByRegion.ContainsKey(regionKey))
                {
                    typesByRegion[regionKey] = new HashSet<NaturalNodeType>();
                }
                typesByRegion[regionKey].Add(node.Type);
            }

            foreach (var region in typesByRegion.Values)
            {
                Assert.AreEqual(4, region.Count);
                Assert.IsTrue(region.Contains(NaturalNodeType.Tree));
                Assert.IsTrue(region.Contains(NaturalNodeType.Plant));
                Assert.IsTrue(region.Contains(NaturalNodeType.Animal));
                Assert.IsTrue(region.Contains(NaturalNodeType.Ore));
            }
        }

        [Test]
        public void GenerateNodes_NoTileCollisions()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            var positions = new HashSet<GridPos>();

            foreach (var node in nodes)
            {
                Assert.IsFalse(positions.Contains(node.Position), 
                    $"Duplicate position found: {node.Position}");
                positions.Add(node.Position);
            }

            Assert.AreEqual(400, positions.Count);
        }

        [Test]
        public void GenerateNodes_AllNodesStartWithInitialAmount()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            foreach (var node in nodes)
            {
                Assert.AreEqual(NaturalNode.InitialAmount, node.Remaining);
                Assert.IsFalse(node.IsDepleted);
            }
        }

        [Test]
        public void GenerateNodes_DeterministicWithSameSeed()
        {
            var nodes1 = WorldGenerator.GenerateNodes(123);
            var nodes2 = WorldGenerator.GenerateNodes(123);

            Assert.AreEqual(nodes1.Count, nodes2.Count);

            for (int i = 0; i < nodes1.Count; i++)
            {
                Assert.AreEqual(nodes1[i].Type, nodes2[i].Type);
                Assert.AreEqual(nodes1[i].Position, nodes2[i].Position);
                Assert.AreEqual(nodes1[i].Remaining, nodes2[i].Remaining);
            }
        }

        [Test]
        public void GenerateNodes_DifferentSeedsDifferentResults()
        {
            var nodes1 = WorldGenerator.GenerateNodes(42);
            var nodes2 = WorldGenerator.GenerateNodes(99);

            var positions1 = new HashSet<GridPos>(nodes1.Select(n => n.Position));
            var positions2 = new HashSet<GridPos>(nodes2.Select(n => n.Position));

            Assert.AreNotEqual(positions1, positions2, "Different seeds should produce different node positions");
        }

        [Test]
        public void GenerateNodes_AllPositionsInBounds()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            foreach (var node in nodes)
            {
                Assert.IsTrue(node.Position.IsInBounds(), 
                    $"Node at {node.Position} is out of bounds");
                Assert.GreaterOrEqual(node.Position.X, 0);
                Assert.Less(node.Position.X, GridPos.MapWidth);
                Assert.GreaterOrEqual(node.Position.Y, 0);
                Assert.Less(node.Position.Y, GridPos.MapHeight);
            }
        }

        [Test]
        public void GenerateNodes_EachRegionHasExactlyFourNodes()
        {
            var nodes = WorldGenerator.GenerateNodes(42);
            var nodesByRegion = new Dictionary<(int, int), int>();

            foreach (var node in nodes)
            {
                int regionX = node.Position.X / WorldGenerator.TilesPerRegion;
                int regionY = node.Position.Y / WorldGenerator.TilesPerRegion;
                var regionKey = (regionX, regionY);

                if (!nodesByRegion.ContainsKey(regionKey))
                {
                    nodesByRegion[regionKey] = 0;
                }
                nodesByRegion[regionKey]++;
            }

            foreach (var count in nodesByRegion.Values)
            {
                Assert.AreEqual(4, count);
            }

            Assert.AreEqual(100, nodesByRegion.Count, "Should have exactly 100 regions (10x10)");
        }
    }
}
