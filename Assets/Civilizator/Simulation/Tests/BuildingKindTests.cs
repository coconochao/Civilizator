using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class BuildingKindTests
    {
        [Test]
        public void GetFootprintSize_CentralIs3x3()
        {
            Assert.AreEqual(3, BuildingKindHelpers.GetFootprintSize(BuildingKind.Central));
        }

        [Test]
        public void GetFootprintSize_HouseIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.House));
        }

        [Test]
        public void GetFootprintSize_TowerIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.Tower));
        }

        [Test]
        public void GetFootprintSize_PlantationIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.Plantation));
        }

        [Test]
        public void GetFootprintSize_FarmIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.Farm));
        }

        [Test]
        public void GetFootprintSize_CattleFarmIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.CattleFarm));
        }

        [Test]
        public void GetFootprintSize_QuarryIs2x2()
        {
            Assert.AreEqual(2, BuildingKindHelpers.GetFootprintSize(BuildingKind.Quarry));
        }

        [Test]
        public void AllBuildingKindsExist()
        {
            var kinds = new[]
            {
                BuildingKind.Central,
                BuildingKind.House,
                BuildingKind.Tower,
                BuildingKind.Plantation,
                BuildingKind.Farm,
                BuildingKind.CattleFarm,
                BuildingKind.Quarry
            };

            foreach (var kind in kinds)
            {
                // Should not throw
                var size = BuildingKindHelpers.GetFootprintSize(kind);
                Assert.IsTrue(size > 0);
            }
        }
    }
}
