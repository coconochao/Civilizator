using NUnit.Framework;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class GridPosTests
    {
        [Test]
        public void Manhattan_CornerToCorner_Returns198()
        {
            var corner1 = new GridPos(0, 0);
            var corner2 = new GridPos(99, 99);
            int distance = GridPos.Manhattan(corner1, corner2);
            Assert.AreEqual(198, distance);
        }

        [Test]
        public void Manhattan_SamePosition_ReturnsZero()
        {
            var pos = new GridPos(50, 50);
            int distance = GridPos.Manhattan(pos, pos);
            Assert.AreEqual(0, distance);
        }

        [Test]
        public void Manhattan_Symmetric()
        {
            var pos1 = new GridPos(10, 20);
            var pos2 = new GridPos(30, 50);
            int dist1to2 = GridPos.Manhattan(pos1, pos2);
            int dist2to1 = GridPos.Manhattan(pos2, pos1);
            Assert.AreEqual(dist1to2, dist2to1);
        }

        [Test]
        public void Manhattan_DistanceTo_ConsistentWithStatic()
        {
            var pos1 = new GridPos(5, 15);
            var pos2 = new GridPos(25, 35);
            int staticDist = GridPos.Manhattan(pos1, pos2);
            int instanceDist = pos1.DistanceTo(pos2);
            Assert.AreEqual(staticDist, instanceDist);
        }

        [Test]
        public void Constructor_ClampsBelowZero()
        {
            var pos = new GridPos(-10, -5);
            Assert.AreEqual(0, pos.X);
            Assert.AreEqual(0, pos.Y);
        }

        [Test]
        public void Constructor_ClampsAboveMax()
        {
            var pos = new GridPos(150, 200);
            Assert.AreEqual(99, pos.X);
            Assert.AreEqual(99, pos.Y);
        }

        [Test]
        public void Equality_WorksCorrectly()
        {
            var pos1 = new GridPos(10, 20);
            var pos2 = new GridPos(10, 20);
            var pos3 = new GridPos(10, 21);
            
            Assert.AreEqual(pos1, pos2);
            Assert.AreNotEqual(pos1, pos3);
            Assert.IsTrue(pos1 == pos2);
            Assert.IsFalse(pos1 == pos3);
        }

        [Test]
        public void IsInBounds_ValidPosition_ReturnsTrue()
        {
            var pos = new GridPos(50, 50);
            Assert.IsTrue(pos.IsInBounds());
        }

        [Test]
        public void MapConstants_AreDefined()
        {
            Assert.AreEqual(100, GridPos.MapWidth);
            Assert.AreEqual(100, GridPos.MapHeight);
        }
    }
}
