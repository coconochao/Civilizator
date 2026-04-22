using Civilizator.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace Civilizator.Presentation.Tests
{
    [TestFixture]
    public class WorldPlaceholderViewTests
    {
        [Test]
        public void TileToWorldMapsGridCoordinatesToXZSpace()
        {
            Vector3 world = WorldPlaceholderView.TileToWorld(new GridPos(12, 34), 2f, 1.5f);

            Assert.That(world, Is.EqualTo(new Vector3(24f, 1.5f, 68f)));
        }

        [Test]
        public void FootprintCenterCenters2x2FootprintsBetweenTiles()
        {
            Vector3 center = WorldPlaceholderView.FootprintCenter(new GridPos(10, 10), 2, 1f);

            Assert.That(center, Is.EqualTo(new Vector3(10.5f, 0f, 10.5f)));
        }

        [Test]
        public void FootprintCenterCenters3x3FootprintsOnTheAnchorMiddle()
        {
            Vector3 center = WorldPlaceholderView.FootprintCenter(new GridPos(10, 10), 3, 1f, 0.25f);

            Assert.That(center, Is.EqualTo(new Vector3(11f, 0.25f, 11f)));
        }
    }
}
