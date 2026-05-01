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

        [Test]
        public void FindNearestReachableTile_SingleTarget_ReturnsClosestReachable()
        {
            // Agent at (0,0), target at (3,3)
            var start = new GridPos(0, 0);
            var target = new GridPos(3, 3);

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            Assert.IsNotNull(nearest, "Should find a reachable tile");
            // The nearest tile to (3,3) reachable from (0,0) should be (3,3) itself
            Assert.AreEqual(target, nearest.Value, "Nearest reachable tile should be target center if passable");
        }

        [Test]
        public void FindNearestReachableTile_EquidistantTiles_UsesDeterministicTieBreak()
        {
            // Create a setup with multiple equidistant tiles
            // Agent at (0,2), target at (5,2)
            // Tiles (2,2), (3,2), (4,2) should all be at distance 3, 2, 1 from target respectively
            // With target at (5,2), closest reachable should be as close as possible
            var start = new GridPos(0, 2);
            var target = new GridPos(5, 2);

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            Assert.IsNotNull(nearest);
            // (5,2) is directly reachable and closest
            Assert.AreEqual(new GridPos(5, 2), nearest.Value);
        }

        [Test]
        public void FindNearestReachableTile_WithObstacle_FindsNearestAroundIt()
        {
            // Block direct approach to target
            // Agent at (0,5), target at (6,5)
            // Block tiles (3,5) and (4,5)
            occupancy.BlockTile(new GridPos(3, 5));
            occupancy.BlockTile(new GridPos(4, 5));

            var start = new GridPos(0, 5);
            var target = new GridPos(6, 5);

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            Assert.IsNotNull(nearest, "Should find nearest tile even with obstacles");
            // The target itself is reachable, so should return it
            Assert.AreEqual(target, nearest.Value);
        }

        [Test]
        public void FindNearestReachableTile_MultipleEquidistantTiles_PicksLowerXFirst()
        {
            // Create a scenario where multiple tiles are equidistant from target
            // Block the target center so we can't reach it directly
            // Target at (5,5), but it's blocked
            // Tiles at distance 1: (4,5), (6,5), (5,4), (5,6)
            // Should deterministically pick (4,5) (lowest X)
            var start = new GridPos(0, 5);
            var target = new GridPos(5, 5);
            occupancy.BlockTile(target);

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            Assert.IsNotNull(nearest);
            // Distance 1 tiles: (4,5), (6,5), (5,4), (5,6)
            // Lowest X is (4,5), but we reach from (0,5) moving right
            // All are equidistant to (5,5), so pick by X-first rule: (4,5)
            Assert.AreEqual(new GridPos(4, 5), nearest.Value, "Should pick tile with lowest X when equidistant");
        }

        [Test]
        public void FindNearestReachableTile_StartBlocked_ReturnsNull()
        {
            var start = new GridPos(5, 5);
            var target = new GridPos(8, 8);

            occupancy.BlockTile(start);

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            Assert.IsNull(nearest, "Should return null when start is blocked");
        }

        [Test]
        public void FindNearestReachableTile_CompletelyIsolated_ReturnsNull()
        {
            // Isolate a small area with walls
            var start = new GridPos(5, 5);
            var target = new GridPos(8, 8);

            // Create walls around start (except don't block start itself)
            occupancy.BlockTile(new GridPos(4, 5));
            occupancy.BlockTile(new GridPos(6, 5));
            occupancy.BlockTile(new GridPos(5, 4));
            occupancy.BlockTile(new GridPos(5, 6));

            var nearest = Pathfinding.FindNearestReachableTile(start, target, occupancy);

            // start itself should be returned as reachable
            Assert.IsNotNull(nearest);
            Assert.AreEqual(start, nearest.Value, "At minimum should find start position itself");
        }

        [Test]
        public void FindPath_ComplexMazeRegression_ValidatesFixedBehavior()
        {
            // Regression test using complex maze fixture
            // Verifies pathfinding behavior remains consistent across versions
            // Grid: 20x15 with walls creating maze pattern
            // Start: (1,1), Target: (18,13), Expected path length: 29

            var occupancy20x15 = new GridOccupancy(20, 15);

            // Block vertical wall at x=5 (except at y=5 passage)
            for (int y = 0; y < 15; y++)
            {
                if (y != 5) // Leave opening at y=5
                {
                    occupancy20x15.BlockTile(new GridPos(5, y));
                }
            }

            // Block horizontal wall at y=5 (x=9-15, except passage at x=8)
            for (int x = 9; x < 16; x++)
            {
                occupancy20x15.BlockTile(new GridPos(x, 5));
            }

            // Block vertical wall at x=7 (y=10-14)
            for (int y = 10; y < 15; y++)
            {
                occupancy20x15.BlockTile(new GridPos(7, y));
            }

            var start = new GridPos(1, 1);
            var target = new GridPos(18, 13);

            var path = Pathfinding.FindPath(start, target, occupancy20x15);

            Assert.AreEqual(30, path.Count, "Regression: maze path should have exactly 30 tiles");
            Assert.AreEqual(start, path[0], "Path should start at (1,1)");
            Assert.AreEqual(target, path[path.Count - 1], "Path should end at (18,13)");

            // Verify no blocked tiles are in the path
            var blockedSet = new HashSet<GridPos>();
            for (int y = 0; y < 15; y++)
            {
                if (y != 5) blockedSet.Add(new GridPos(5, y));
            }
            for (int x = 9; x < 16; x++)
            {
                blockedSet.Add(new GridPos(x, 5));
            }
            for (int y = 10; y < 15; y++)
            {
                blockedSet.Add(new GridPos(7, y));
            }

            foreach (var tile in path)
            {
                Assert.IsFalse(blockedSet.Contains(tile), $"Path should not contain blocked tile {tile}");
            }
        }

        [Test]
        public void FindPathToOccupiedTarget_TargetBlocked_ReturnsPathEndingAtTarget()
        {
            var start = new GridPos(0, 0);
            var target = new GridPos(3, 3);
            occupancy.BlockTile(target);

            var path = Pathfinding.FindPathToOccupiedTarget(start, target, occupancy);

            Assert.Greater(path.Count, 0, "Path should exist to a blocked destination");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }

        [Test]
        public void FindPathToOccupiedTarget_StartBlocked_ReturnsPath()
        {
            var start = new GridPos(2, 2);
            var target = new GridPos(5, 5);
            occupancy.BlockTile(start);

            var path = Pathfinding.FindPathToOccupiedTarget(start, target, occupancy);

            Assert.Greater(path.Count, 0, "Path should exist even when starting inside a blocked tile");
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(target, path[path.Count - 1]);
        }
    }
}
