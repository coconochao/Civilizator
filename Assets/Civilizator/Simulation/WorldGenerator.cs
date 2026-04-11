using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Generates a world with natural resource nodes.
    /// Divides the 100×100 map into 10×10 regions, placing one node of each type per region.
    /// </summary>
    public static class WorldGenerator
    {
        public const int RegionGridSize = 10;
        public const int TilesPerRegion = GridPos.MapWidth / RegionGridSize;

        /// <summary>
        /// Generate all nodes for the world using a deterministic seed.
        /// Returns a list of 400 nodes (4 types × 100 regions).
        /// </summary>
        public static List<NaturalNode> GenerateNodes(int seed)
        {
            var nodes = new List<NaturalNode>();
            var usedPositions = new HashSet<GridPos>();
            var rng = new Random(seed);

            var nodeTypes = new[] { NaturalNodeType.Tree, NaturalNodeType.Plant, NaturalNodeType.Animal, NaturalNodeType.Ore };

            for (int regionX = 0; regionX < RegionGridSize; regionX++)
            {
                for (int regionY = 0; regionY < RegionGridSize; regionY++)
                {
                    foreach (var nodeType in nodeTypes)
                    {
                        GridPos nodePos = FindAvailablePositionInRegion(regionX, regionY, usedPositions, rng);
                        usedPositions.Add(nodePos);
                        nodes.Add(new NaturalNode(nodeType, nodePos));
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Find a random, unused position within a specific region.
        /// </summary>
        private static GridPos FindAvailablePositionInRegion(int regionX, int regionY, HashSet<GridPos> usedPositions, Random rng)
        {
            int regionStartX = regionX * TilesPerRegion;
            int regionStartY = regionY * TilesPerRegion;
            int regionEndX = regionStartX + TilesPerRegion;
            int regionEndY = regionStartY + TilesPerRegion;

            var candidates = new List<GridPos>();
            for (int x = regionStartX; x < regionEndX; x++)
            {
                for (int y = regionStartY; y < regionEndY; y++)
                {
                    var pos = new GridPos(x, y);
                    if (!usedPositions.Contains(pos))
                    {
                        candidates.Add(pos);
                    }
                }
            }

            if (candidates.Count == 0)
                throw new InvalidOperationException($"No available positions in region ({regionX}, {regionY})");

            int randomIndex = rng.Next(candidates.Count);
            return candidates[randomIndex];
        }
    }
}
