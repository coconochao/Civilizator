using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class GridOccupancyTests
    {
        private GridOccupancy occupancy;

        [SetUp]
        public void SetUp()
        {
            occupancy = new GridOccupancy();
        }

        [Test]
        public void IsPassable_AllTilesStartAsPassable()
        {
            Assert.IsTrue(occupancy.IsPassable(new GridPos(0, 0)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(50, 50)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(99, 99)));
        }

        [Test]
        public void BlockTile_MarksTileAsImpassable()
        {
            var tile = new GridPos(10, 10);
            occupancy.BlockTile(tile);
            
            Assert.IsFalse(occupancy.IsPassable(tile));
        }

        [Test]
        public void UnblockTile_MarksTileAsPassable()
        {
            var tile = new GridPos(10, 10);
            occupancy.BlockTile(tile);
            occupancy.UnblockTile(tile);
            
            Assert.IsTrue(occupancy.IsPassable(tile));
        }

        [Test]
        public void BlockBuilding_BlocksAllFootprintTiles()
        {
            var building = new Building(BuildingKind.Central, new GridPos(5, 5));
            occupancy.BlockBuilding(building);

            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);

            foreach (var tile in occupiedTiles)
            {
                Assert.IsFalse(occupancy.IsPassable(tile), $"Tile {tile} should be blocked");
            }
        }

        [Test]
        public void BlockBuilding_DoesNotBlockAdjacentTiles()
        {
            var building = new Building(BuildingKind.Central, new GridPos(5, 5));
            occupancy.BlockBuilding(building);

            Assert.IsTrue(occupancy.IsPassable(new GridPos(5, 3)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(5, 8)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(3, 5)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(8, 5)));
        }

        [Test]
        public void UnblockBuilding_UnblocksAllFootprintTiles()
        {
            var building = new Building(BuildingKind.Central, new GridPos(5, 5));
            occupancy.BlockBuilding(building);
            occupancy.UnblockBuilding(building);

            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);

            foreach (var tile in occupiedTiles)
            {
                Assert.IsTrue(occupancy.IsPassable(tile), $"Tile {tile} should be passable");
            }
        }

        [Test]
        public void Clear_ResetsAllTilesToPassable()
        {
            occupancy.BlockTile(new GridPos(10, 10));
            var building = new Building(BuildingKind.Tower, new GridPos(20, 20));
            occupancy.BlockBuilding(building);

            occupancy.Clear();

            Assert.IsTrue(occupancy.IsPassable(new GridPos(10, 10)));
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                Assert.IsTrue(occupancy.IsPassable(tile));
            }
        }

        [Test]
        public void IsPassable_ReturnsFalseForOutOfBounds()
        {
            Assert.IsFalse(occupancy.IsPassable(new GridPos(-1, 0)));
            Assert.IsFalse(occupancy.IsPassable(new GridPos(0, -1)));
            Assert.IsFalse(occupancy.IsPassable(new GridPos(GridPos.MapWidth, 0)));
            Assert.IsFalse(occupancy.IsPassable(new GridPos(0, GridPos.MapHeight)));
        }

        [Test]
        public void MultipleBuildings_BlockTheirRespectiveFootprints()
        {
            var building1 = new Building(BuildingKind.Central, new GridPos(5, 5));
            var building2 = new Building(BuildingKind.Tower, new GridPos(15, 15));

            occupancy.BlockBuilding(building1);
            occupancy.BlockBuilding(building2);

            Assert.IsFalse(occupancy.IsPassable(new GridPos(5, 5)));
            Assert.IsFalse(occupancy.IsPassable(new GridPos(15, 15)));
            Assert.IsFalse(occupancy.IsPassable(new GridPos(16, 16)));
            Assert.IsTrue(occupancy.IsPassable(new GridPos(10, 10)));
        }
    }
}
