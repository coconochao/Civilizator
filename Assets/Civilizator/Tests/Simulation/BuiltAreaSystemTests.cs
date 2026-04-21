using System.Collections.Generic;
using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class BuiltAreaSystemTests
    {
        [Test]
        public void GetBuiltAreaRadius_OnlyCentralBuilding_ReturnsZero()
        {
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Central, new GridPos(10, 10))
            };

            Assert.AreEqual(0, BuiltAreaSystem.GetBuiltAreaRadius(buildings));
        }

        [Test]
        public void GetBuiltAreaRadius_UsesCentralFootprintNotAnchor()
        {
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Central, new GridPos(10, 10)),
                new Building(BuildingKind.House, new GridPos(14, 10))
            };

            Assert.AreEqual(3, BuiltAreaSystem.GetBuiltAreaRadius(buildings));
        }

        [Test]
        public void GetBuiltAreaRadius_IncreasesWhenAddingFartherBuilding()
        {
            var central = new Building(BuildingKind.Central, new GridPos(10, 10));
            var buildings = new List<Building> { central };

            int initialRadius = BuiltAreaSystem.GetBuiltAreaRadius(buildings);
            Assert.AreEqual(0, initialRadius);

            buildings.Add(new Building(BuildingKind.House, new GridPos(14, 10)));
            int afterFirstHouse = BuiltAreaSystem.GetBuiltAreaRadius(buildings);
            Assert.AreEqual(3, afterFirstHouse);

            buildings.Add(new Building(BuildingKind.Tower, new GridPos(20, 10)));
            int afterFartherBuilding = BuiltAreaSystem.GetBuiltAreaRadius(buildings);
            Assert.AreEqual(9, afterFartherBuilding);
            Assert.Greater(afterFartherBuilding, afterFirstHouse);
        }
    }
}
