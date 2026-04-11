using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    /// <summary>
    /// Tests for the 4-way BFS pathfinder.
    /// Verifies correct path finding on various grid configurations.
    /// </summary>
    [TestFixture]
    public class PathfindingTests
    {
        private GridOccupancy occupancy;

        [SetUp]
        public void SetUp()
        {
            // Create a fresh 10x10 grid for testing (smaller than full map for clarity)
            occupancy = new GridOccupancy(10, 10);
        }

        [Test]
        public void FindPath_DirectPath_ReturnsCorrectPath()
        {
            // Simple horizontal path with no obstacles
            var start = new GridPos(0, 5);
            var target = new GridPos(3, 5);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.AreEqual(4, path.Count, "Path should have 4 tiles (0-1-2-3)");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }

        [Test]
        public void FindPath_NoObstacles_ReturnsShortestPath()
        {
            // Vertical path
            var start = new GridPos(5, 0);
            var target = new GridPos(5, 4);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.AreEqual(5, path.Count, "Vertical path of 5 tiles");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }

        [Test]
        public void FindPath_WithObstacle_RoutesAround()
        {
            // Create a simple wall that blocks straight path
            // Block the middle tile of a horizontal path
            occupancy.BlockTile(new GridPos(2, 5));

            var start = new GridPos(0, 5);
            var target = new GridPos(4, 5);

            var path = Pathfinding.FindPath(start, target, occupancy);

            // Path should exist (going around the obstacle)
            Assert.Greater(path.Count, 0, "Path should exist around obstacle");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);

            // Verify blocked tile is not in the path
            Assert.IsFalse(path.Contains(new GridPos(2, 5)), "Path should not contain blocked tile");
        }

        [Test]
        public void FindPath_StartEqualsTarget_ReturnsSingleTile()
        {
            var pos = new GridPos(3, 3);

            var path = Pathfinding.FindPath(pos, pos, occupancy);

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(pos, path[0]);
        }

        [Test]
        public void FindPath_StartIsBlocked_ReturnsEmptyPath()
        {
            var start = new GridPos(2, 2);
            var target = new GridPos(5, 5);

            occupancy.BlockTile(start);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.AreEqual(0, path.Count, "No path when start is blocked");
        }

        [Test]
        public void FindPath_TargetIsBlocked_ReturnsEmptyPath()
        {
            var start = new GridPos(2, 2);
            var target = new GridPos(5, 5);

            occupancy.BlockTile(target);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.AreEqual(0, path.Count, "No path when target is blocked");
        }

        [Test]
        public void FindPath_NoPath_ReturnsEmptyList()
        {
            // Create a complete wall blocking the path
            for (int x = 0; x < 10; x++)
            {
                occupancy.BlockTile(new GridPos(x, 5));
            }

            var start = new GridPos(3, 3);
            var target = new GridPos(3, 7);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.AreEqual(0, path.Count, "No path when target is completely blocked");
        }

        [Test]
        public void FindPath_Diagonal_RequiresMovesThroughIntermediateTiles()
        {
            // Moving from (0,0) to (3,3) requires path through intermediate tiles
            var start = new GridPos(0, 0);
            var target = new GridPos(3, 3);

            var path = Pathfinding.FindPath(start, target, occupancy);

            // Shortest path is 7 tiles long (3 right + 3 down + start)
            Assert.AreEqual(7, path.Count, "Diagonal path should be 7 tiles");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }

        [Test]
        public void FindPath_ComplexMaze_FindsPath()
        {
            // Create a simple maze pattern
            // Block a vertical corridor except at bottom
            for (int y = 0; y < 8; y++)
            {
                if (y != 7) // Leave one opening
                {
                    occupancy.BlockTile(new GridPos(5, y));
                }
            }

            var start = new GridPos(3, 3);
            var target = new GridPos(7, 3);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.Greater(path.Count, 0, "Path should exist through maze opening");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }

        [Test]
        public void FindPath_RespectsBounds()
        {
            // Test with positions near grid bounds (GridPos clamps to bounds)
            var start = new GridPos(0, 0);
            var target = new GridPos(8, 8);

            var path = Pathfinding.FindPath(start, target, occupancy);

            Assert.Greater(path.Count, 0, "Path should exist");
            // All tiles in path should be within bounds
            foreach (var tile in path)
            {
                Assert.IsTrue(tile.X >= 0 && tile.X < 10);
                Assert.IsTrue(tile.Y >= 0 && tile.Y < 10);
            }
        }
    }
}
