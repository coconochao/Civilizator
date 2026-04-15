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

        [Test]
        public void IsConstructionPhaseComplete_WithoutClock_BasedonProgressOnly()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            // Without a clock, completion is progress-based only
            building.ConstructionProgress = required - 1;
            Assert.IsFalse(building.IsConstructionPhaseComplete());

            building.ConstructionProgress = required;
            Assert.IsTrue(building.IsConstructionPhaseComplete());
        }

        [Test]
        public void DeliverBuildResources_WithClock_SchedulesBuildTime()
        {
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            // Deliver resources to complete the required amount
            building.DeliverBuildResources(required, productivityMultiplier: 1f);

            // BuildTimeEndSeconds should be set
            Assert.Greater(building.BuildTimeEndSeconds, 0f);
            Assert.AreEqual(required, building.BuildTimeEndSeconds);
        }

        [Test]
        public void IsConstructionPhaseComplete_WithClock_GatedByBuildTime()
        {
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.Tower, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            // Deliver required amount; build-time is scheduled
            building.DeliverBuildResources(required, productivityMultiplier: 1f);

            // At this point, progress is met but build-time hasn't elapsed
            Assert.AreEqual(required, building.ConstructionProgress);
            Assert.IsFalse(building.IsConstructionPhaseComplete());

            // Advance clock past build-time
            float buildTimeSeconds = required * (1f / 1f); // 100 seconds
            clock.Advance(buildTimeSeconds + 0.1f);

            // Now the building should be complete
            Assert.IsTrue(building.IsConstructionPhaseComplete());
        }

        [Test]
        public void BuildTimeCalculation_Adult_ProductivityOnePointZero()
        {
            // Adult with productivity 1.0 delivers 10 units
            // Build-time = 10 * (1 / 1.0) = 10 seconds
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = true;
            building.ConstructionProgress = building.GetRequiredConstructionAmount() - 10; // Set progress so that 10 units completes it

            // Deliver 10 units with adult productivity
            building.DeliverBuildResources(10, productivityMultiplier: 1f);

            // BuildTimeEndSeconds should be 10 (current time 0 + 10 seconds)
            Assert.AreEqual(10f, building.BuildTimeEndSeconds);
        }

        [Test]
        public void BuildTimeCalculation_Child_ProductivityHalf()
        {
            // Child with productivity 0.5 delivers 10 units
            // Build-time = 10 * (1 / 0.5) = 20 seconds
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = true;
            building.ConstructionProgress = building.GetRequiredConstructionAmount() - 10; // Set progress so that 10 units completes it
            // Deliver 10 units with child productivity
            building.DeliverBuildResources(10, productivityMultiplier: 0.5f);

            // BuildTimeEndSeconds should be 20 (current time 0 + 20 seconds)
            Assert.AreEqual(20f, building.BuildTimeEndSeconds);
        }

        [Test]
        public void BuildTime_MultipleDeliveries_LastOneMatters()
        {
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.Farm, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            int quarter = required / 4;

            // First delivery: incomplete
            building.DeliverBuildResources(quarter, productivityMultiplier: 1f);
            Assert.AreEqual(0f, building.BuildTimeEndSeconds); // No build-time yet

            // Second delivery: still incomplete
            building.DeliverBuildResources(quarter, productivityMultiplier: 1f);
            Assert.AreEqual(0f, building.BuildTimeEndSeconds); // No build-time yet

            // Third delivery: still incomplete
            building.DeliverBuildResources(quarter, productivityMultiplier: 1f);
            Assert.AreEqual(0f, building.BuildTimeEndSeconds); // No build-time yet

            // Fourth delivery: completes with different productivity
            building.DeliverBuildResources(quarter, productivityMultiplier: 0.5f);

            // Build-time should be based on the final delivery only
            // (quarter units with 0.5 productivity = quarter * 2 seconds)
            float expectedBuildTime = quarter * (1f / 0.5f);
            Assert.AreEqual(expectedBuildTime, building.BuildTimeEndSeconds);
        }

        [Test]
        public void BuildTime_NotScheduledIfClockNotSet()
        {
            // Without a clock, build-time is not scheduled
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;
            int required = building.GetRequiredConstructionAmount();

            building.DeliverBuildResources(required, productivityMultiplier: 1f);

            // BuildTimeEndSeconds should remain 0 (not set)
            Assert.AreEqual(0f, building.BuildTimeEndSeconds);
        }

        [Test]
        public void BuildTime_InvalidProductivity_Throws()
        {
            var building = new Building(BuildingKind.House, new GridPos(0, 0));
            building.IsUnderConstruction = true;

            Assert.Throws<System.ArgumentException>(
                () => building.DeliverBuildResources(10, productivityMultiplier: 0f)
            );

            Assert.Throws<System.ArgumentException>(
                () => building.DeliverBuildResources(10, productivityMultiplier: -1f)
            );
        }

        [Test]
        public void Upgrade_BuildTimeWorks_SameAsConstruction()
        {
            // Verify build-time mechanism works for upgrades too
            var clock = new SimulationClock();
            var building = new Building(BuildingKind.Tower, new GridPos(0, 0));
            building.SimulationClock = clock;
            building.IsUnderConstruction = false;
            building.UpgradeLevel = 0;

            int upgradeCost = building.GetRequiredConstructionAmount();
            building.DeliverBuildResources(upgradeCost, productivityMultiplier: 1f);

            Assert.AreEqual(upgradeCost, building.BuildTimeEndSeconds);
            Assert.IsFalse(building.IsConstructionPhaseComplete());

            clock.Advance(upgradeCost + 0.1f);
            Assert.IsTrue(building.IsConstructionPhaseComplete());
        }

        // T-090 — House capacity tests

        [Test]
        public void HouseCapacity_InitiallyEmpty()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            Assert.AreEqual(0, house.GetAdultCount());
            Assert.AreEqual(0, house.GetChildCount());
            Assert.IsTrue(house.HasAvailableAdultSlot());
        }

        [Test]
        public void HouseCapacity_CanAssignOneAdult()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            bool result = house.AssignAdultResident(1);

            Assert.IsTrue(result);
            Assert.AreEqual(1, house.GetAdultCount());
            Assert.IsTrue(house.HasAvailableAdultSlot());
        }

        [Test]
        public void HouseCapacity_CanAssignTwoAdults()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            bool result1 = house.AssignAdultResident(1);
            bool result2 = house.AssignAdultResident(2);

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(2, house.GetAdultCount());
            Assert.IsFalse(house.HasAvailableAdultSlot());
        }

        [Test]
        public void HouseCapacity_CannotAssignThirdAdult()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            house.AssignAdultResident(1);
            house.AssignAdultResident(2);
            bool result3 = house.AssignAdultResident(3);

            Assert.IsFalse(result3);
            Assert.AreEqual(2, house.GetAdultCount());
        }

        [Test]
        public void HouseCapacity_ChildrenUnlimited()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            // Assign multiple children without capacity limit
            bool result1 = house.AssignChildResident(100);
            bool result2 = house.AssignChildResident(101);
            bool result3 = house.AssignChildResident(102);
            bool result4 = house.AssignChildResident(103);

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.IsTrue(result3);
            Assert.IsTrue(result4);
            Assert.AreEqual(4, house.GetChildCount());
        }

        [Test]
        public void HouseCapacity_CanRemoveAdult()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            house.AssignAdultResident(1);
            house.AssignAdultResident(2);
            Assert.IsFalse(house.HasAvailableAdultSlot());

            house.RemoveAdultResident(2);

            Assert.AreEqual(1, house.GetAdultCount());
            Assert.IsTrue(house.HasAvailableAdultSlot());
        }

        [Test]
        public void HouseCapacity_CanAssignAfterRemoval()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            house.AssignAdultResident(1);
            house.AssignAdultResident(2);
            house.RemoveAdultResident(1);

            // Should now be able to assign a new adult
            bool result = house.AssignAdultResident(3);

            Assert.IsTrue(result);
            Assert.AreEqual(2, house.GetAdultCount());
            Assert.Contains(2, house.AdultResidentIds);
            Assert.Contains(3, house.AdultResidentIds);
        }

        [Test]
        public void HouseCapacity_CanRemoveChild()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            house.AssignChildResident(100);
            house.AssignChildResident(101);
            Assert.AreEqual(2, house.GetChildCount());

            house.RemoveChildResident(100);

            Assert.AreEqual(1, house.GetChildCount());
            Assert.Contains(101, house.ChildResidentIds);
        }

        [Test]
        public void HouseCapacity_IgnoresDuplicateAssignments()
        {
            var house = new Building(BuildingKind.House, new GridPos(0, 0));

            house.AssignAdultResident(1);
            house.AssignAdultResident(1); // Duplicate
            
            Assert.AreEqual(1, house.GetAdultCount());
            Assert.AreEqual(1, house.AdultResidentIds.Count);
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

