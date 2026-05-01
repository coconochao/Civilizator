using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SoldierImprovementSystemTests
    {
        [SetUp]
        public void SetUp()
        {
            SoldierImprovementControls.ResetToDefaults();
        }

        [Test]
        public void SetTowerBuildEmphasis_InvalidValue_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => SoldierImprovementControls.SetTowerBuildEmphasis(-0.1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => SoldierImprovementControls.SetTowerBuildEmphasis(1.1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => SoldierImprovementControls.SetTowerBuildEmphasis(float.NaN));
        }

        [Test]
        public void FindBestImprovementTarget_ConstructionEmphasis_PrefersTowerBuild()
        {
            SoldierImprovementControls.SetTowerBuildEmphasis(0.9f);

            var soldier = new Agent(new GridPos(10, 10), Profession.Soldier, LifeStage.Adult);
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Tower, new GridPos(30, 30))
                {
                    IsUnderConstruction = true
                },
                new Building(BuildingKind.Tower, new GridPos(12, 12))
                {
                    UpgradeLevel = 0
                }
            };

            var target = SoldierImprovementSystem.FindBestImprovementTarget(soldier, buildings);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Anchor, Is.EqualTo(new GridPos(30, 30)));
            Assert.That(target.IsUnderConstruction, Is.True);
        }

        [Test]
        public void FindBestImprovementTarget_UpgradeEmphasis_PrefersTowerUpgrade()
        {
            SoldierImprovementControls.SetTowerBuildEmphasis(0.1f);

            var soldier = new Agent(new GridPos(10, 10), Profession.Soldier, LifeStage.Adult);
            var buildings = new List<Building>
            {
                new Building(BuildingKind.Tower, new GridPos(30, 30))
                {
                    IsUnderConstruction = true
                },
                new Building(BuildingKind.Tower, new GridPos(12, 12))
                {
                    UpgradeLevel = 0
                }
            };

            var target = SoldierImprovementSystem.FindBestImprovementTarget(soldier, buildings);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Anchor, Is.EqualTo(new GridPos(12, 12)));
            Assert.That(target.IsUnderConstruction, Is.False);
        }

        [Test]
        public void WithdrawResourcesForImprovement_TowerTarget_ConsumesOre()
        {
            SoldierImprovementControls.SetTowerBuildEmphasis(0.9f);

            var soldier = new Agent(new GridPos(49, 49), Profession.Soldier, LifeStage.Adult);
            var target = new Building(BuildingKind.Tower, new GridPos(50, 50))
            {
                IsUnderConstruction = true
            };
            var centralBuilding = new Building(BuildingKind.Central, new GridPos(47, 47));

            var buildings = new List<Building> { target };
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Ore, 100);

            var selected = SoldierImprovementSystem.FindBestImprovementTarget(soldier, buildings);
            Assert.That(selected, Is.SameAs(target));
            Assert.That(SoldierImprovementSystem.GetTowerImprovementResourceKind(), Is.EqualTo(ResourceKind.Ore));

            int oreBefore = storage.GetStock(ResourceKind.Ore);
            int withdrawn = ProductionSystem.WithdrawResourcesForImprovement(soldier, selected, storage, centralBuilding);
            int oreAfter = storage.GetStock(ResourceKind.Ore);

            Assert.That(withdrawn, Is.GreaterThan(0));
            Assert.That(oreAfter, Is.LessThan(oreBefore));
            Assert.That(oreAfter, Is.EqualTo(oreBefore - withdrawn));
        }
    }
}
