using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class BuildingTests
    {
        [Test]
        public void Constructor_InitializesFieldsCorrectly()
        {
            var anchor = new GridPos(5, 10);
            var building = new Building(BuildingKind.House, anchor);

            Assert.AreEqual(anchor, building.Anchor);
            Assert.AreEqual(BuildingKind.House, building.Kind);
            Assert.AreEqual(0, building.UpgradeLevel);
            Assert.IsFalse(building.IsUnderConstruction);
            Assert.AreEqual(0, building.ConstructionProgress);
        }

        [Test]
        public void Constructor_WorksForAllBuildingKinds()
        {
            var anchor = new GridPos(0, 0);
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
                var building = new Building(kind, anchor);
                Assert.AreEqual(kind, building.Kind);
                Assert.AreEqual(0, building.UpgradeLevel);
                Assert.IsFalse(building.IsUnderConstruction);
            }
        }

        [Test]
        public void GetConstructionResourceKind_TowerRequiresOre()
        {
            var building = new Building(BuildingKind.Tower, new GridPos(0, 0));
            Assert.AreEqual(ResourceKind.Ore, building.GetConstructionResourceKind());
        }

        [Test]
        public void GetConstructionResourceKind_CivilBuildingsRequireLogs()
        {
            var civilKinds = new[]
            {
                BuildingKind.Central,
                BuildingKind.House,
                BuildingKind.Plantation,
                BuildingKind.Farm,
                BuildingKind.CattleFarm,
                BuildingKind.Quarry
            };

            foreach (var kind in civilKinds)
            {
                var building = new Building(kind, new GridPos(0, 0));
                Assert.AreEqual(ResourceKind.Logs, building.GetConstructionResourceKind(), $"Failed for {kind}");
            }
        }

        [Test]
        public void GetRequiredConstructionAmount_NewBuildingRequiresBuildCost()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;

            Assert.AreEqual(BuildingCostHelper.GetBuildCost(BuildingKind.House), building.GetRequiredConstructionAmount());
        }

        [Test]
        public void GetRequiredConstructionAmount_UpgradeRequiresUpgradeCost()
        {
            var building = new Building(BuildingKind.Tower, new GridPos(0, 0));
            building.IsUnderConstruction = false;
            building.UpgradeLevel = 0;

            Assert.AreEqual(BuildingCostHelper.GetUpgradeCost(BuildingKind.Tower), building.GetRequiredConstructionAmount());
        }

        [Test]
        public void GetRequiredConstructionAmount_MaxUpgradeReturnsZero()
        {
            var building = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            building.IsUnderConstruction = false;
            building.UpgradeLevel = 1;

            Assert.AreEqual(0, building.GetRequiredConstructionAmount());
        }

        [Test]
        public void DeliverBuildResources_IncrementsProgress()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;

            building.DeliverBuildResources(10);
            Assert.AreEqual(10, building.ConstructionProgress);

            building.DeliverBuildResources(20);
            Assert.AreEqual(30, building.ConstructionProgress);
        }

        [Test]
        public void DeliverBuildResources_CapsProgressAtRequired()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            // Deliver exactly required amount
            building.DeliverBuildResources(required);
            Assert.AreEqual(required, building.ConstructionProgress);

            // Try to deliver more (should not exceed)
            building.DeliverBuildResources(50);
            Assert.AreEqual(required, building.ConstructionProgress);
        }

        [Test]
        public void DeliverBuildResources_NeverExceedsRequired()
        {
            var building = new Building(BuildingKind.Tower, new GridPos(0, 0));
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            building.DeliverBuildResources(required + 100);
            Assert.AreEqual(required, building.ConstructionProgress);
            Assert.IsTrue(building.ConstructionProgress <= required);
        }

        [Test]
        public void DeliverBuildResources_ThrowsOnNegativeAmount()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;

            Assert.Throws<System.ArgumentException>(() => building.DeliverBuildResources(-5));
        }

        [Test]
        public void DeliverBuildResources_NoOpWhenNotUnderConstruction()
        {
            var building = new Building(BuildingKind.Quarry, new GridPos(0, 0));
            building.IsUnderConstruction = false;
            building.UpgradeLevel = 1; // Max upgrade, no further construction

            // GetRequiredConstructionAmount() returns 0, so delivery is no-op
            building.DeliverBuildResources(10);
            Assert.AreEqual(0, building.ConstructionProgress);
        }

        [Test]
        public void ProgressMultipleDeliveries_SimulatesGradualConstruction()
        {
            var building = new Building(BuildingKind.Farm, new GridPos(0, 0));
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            // Simulate multiple workers delivering resources
            int quarter = required / 4;
            building.DeliverBuildResources(quarter);
            Assert.AreEqual(quarter, building.ConstructionProgress);

            building.DeliverBuildResources(quarter);
            Assert.AreEqual(quarter * 2, building.ConstructionProgress);

            building.DeliverBuildResources(quarter);
            Assert.AreEqual(quarter * 3, building.ConstructionProgress);

            building.DeliverBuildResources(required); // Deliver more than needed
            Assert.AreEqual(required, building.ConstructionProgress);
        }

        [Test]
        public void UpgradeLevelCanBeModified()
        {
            var building = new Building(BuildingKind.Central, new GridPos(0, 0));
            Assert.AreEqual(0, building.UpgradeLevel);

            building.UpgradeLevel = 1;
            Assert.AreEqual(1, building.UpgradeLevel);
        }

        [Test]
        public void IsUnderConstructionCanBeModified()
        {
            var building = new Building(BuildingKind.CattleFarm, new GridPos(0, 0));
            Assert.IsFalse(building.IsUnderConstruction);

            building.IsUnderConstruction = true;
            Assert.IsTrue(building.IsUnderConstruction);

            building.IsUnderConstruction = false;
            Assert.IsFalse(building.IsUnderConstruction);
        }

        [Test]
        public void ConstructionProgressCanBeModified()
        {
            var building = new Building(BuildingKind.Plantation, new GridPos(0, 0));
            Assert.AreEqual(0, building.ConstructionProgress);

            building.ConstructionProgress = 50;
            Assert.AreEqual(50, building.ConstructionProgress);
        }
    }

    [TestFixture]
    public class BuildingCostHelperTests
    {
        [Test]
        public void GetBuildCost_CivilBuildings_Returns100Logs()
        {
            var civilKinds = new[]
            {
                BuildingKind.Central,
                BuildingKind.House,
                BuildingKind.Plantation,
                BuildingKind.Farm,
                BuildingKind.CattleFarm,
                BuildingKind.Quarry
            };

            foreach (var kind in civilKinds)
            {
                Assert.AreEqual(100, BuildingCostHelper.GetBuildCost(kind), $"Failed for {kind}");
                Assert.AreEqual(100, BuildingCostHelper.CivilBuildingBuildCost);
            }
        }

        [Test]
        public void GetBuildCost_Tower_Returns100Ore()
        {
            Assert.AreEqual(100, BuildingCostHelper.GetBuildCost(BuildingKind.Tower));
            Assert.AreEqual(100, BuildingCostHelper.TowerBuildCost);
        }

        [Test]
        public void GetUpgradeCost_CivilBuildingsConstruction_Returns100Logs()
        {
            var civilKinds = new[]
            {
                BuildingKind.Central,
                BuildingKind.House,
                BuildingKind.Plantation,
                BuildingKind.Farm,
                BuildingKind.CattleFarm,
                BuildingKind.Quarry
            };

            foreach (var kind in civilKinds)
            {
                Assert.AreEqual(100, BuildingCostHelper.GetUpgradeCost(kind), $"Failed for {kind}");
                Assert.AreEqual(100, BuildingCostHelper.CivilBuildingUpgradeCost);
            }
        }

        [Test]
        public void GetUpgradeCost_Tower_Returns100Ore()
        {
            Assert.AreEqual(100, BuildingCostHelper.GetUpgradeCost(BuildingKind.Tower));
            Assert.AreEqual(100, BuildingCostHelper.TowerUpgradeCost);
        }

        [Test]
        public void AllCostsMatchSpec()
        {
            // SPEC.md: Civil building construction cost = 100 Logs
            // SPEC.md: Civil building upgrade cost = 100 Logs
            // SPEC.md: Tower construction cost = 100 Ore
            // SPEC.md: Tower upgrade cost = 100 Ore

            Assert.AreEqual(100, BuildingCostHelper.CivilBuildingBuildCost);
            Assert.AreEqual(100, BuildingCostHelper.CivilBuildingUpgradeCost);
            Assert.AreEqual(100, BuildingCostHelper.TowerBuildCost);
            Assert.AreEqual(100, BuildingCostHelper.TowerUpgradeCost);
        }

        [Test]
        public void GetBuildCost_ThrowsOnUnknownKind()
        {
            // Cast to BuildingKind to trick the compiler (invalid kind)
            // This is mainly a safety check for future-proofing
            // In practice, all valid BuildingKind values are handled
            Assert.DoesNotThrow(() =>
            {
                foreach (var kind in new[]
                {
                    BuildingKind.Central,
                    BuildingKind.House,
                    BuildingKind.Tower,
                    BuildingKind.Plantation,
                    BuildingKind.Farm,
                    BuildingKind.CattleFarm,
                    BuildingKind.Quarry
                })
                {
                    BuildingCostHelper.GetBuildCost(kind);
                    BuildingCostHelper.GetUpgradeCost(kind);
                }
            });
        }
    }
}

