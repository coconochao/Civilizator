using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class BuilderSystemTests
    {
        [Test]
        public void IsBuilderProfession_OnlyBuilderReturnsTrue()
        {
            Assert.That(BuilderSystem.IsBuilderProfession(Profession.Builder), Is.True);
            Assert.That(BuilderSystem.IsBuilderProfession(Profession.Woodcutter), Is.False);
            Assert.That(BuilderSystem.IsBuilderProfession(Profession.Soldier), Is.False);
        }

        [Test]
        public void GetPriorityScore_ZeroStock_ReturnsPositiveInfinity()
        {
            var targets = new ProfessionTargets();
            var storage = new CentralStorage();

            float score = BuilderSystem.GetPriorityScore(Profession.Woodcutter, targets, storage);

            Assert.That(score, Is.EqualTo(float.PositiveInfinity));
        }

        [Test]
        public void FindBestImprovementTarget_PrioritizesHousingWhenAdultsAreUnassigned()
        {
            var builder = new Agent(new GridPos(50, 50), Profession.Builder, LifeStage.Adult);
            var agents = new List<Agent>
            {
                new Agent(new GridPos(40, 40), Profession.Woodcutter, LifeStage.Adult)
            };

            var buildings = new List<Building>
            {
                new Building(BuildingKind.House, new GridPos(52, 52))
                {
                    IsUnderConstruction = true
                },
                new Building(BuildingKind.Plantation, new GridPos(55, 55))
                {
                    IsUnderConstruction = true
                }
            };

            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Logs, 100);
            var targets = new ProfessionTargets();

            var target = BuilderSystem.FindBestImprovementTarget(builder, agents, buildings, storage, targets);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Kind, Is.EqualTo(BuildingKind.House));
        }

        [Test]
        public void FindBestImprovementTarget_SelectsHighestScoringProfessionAndNearestTarget()
        {
            var builder = new Agent(new GridPos(50, 50), Profession.Builder, LifeStage.Adult);
            var agents = new List<Agent>();

            var buildings = new List<Building>
            {
                new Building(BuildingKind.Quarry, new GridPos(60, 60))
                {
                    IsUnderConstruction = true
                },
                new Building(BuildingKind.Quarry, new GridPos(54, 54))
                {
                    IsUnderConstruction = true
                },
                new Building(BuildingKind.Plantation, new GridPos(52, 52))
                {
                    IsUnderConstruction = true
                }
            };

            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Logs, 100);
            storage.Deposit(ResourceKind.Meat, 100);
            storage.Deposit(ResourceKind.PlantFood, 100);
            storage.Deposit(ResourceKind.Ore, 1);

            var targets = new ProfessionTargets();
            targets.SetTarget(Profession.Woodcutter, 0.1f);
            targets.SetTarget(Profession.Miner, 0.5f);
            targets.SetTarget(Profession.Hunter, 0.2f);
            targets.SetTarget(Profession.Farmer, 0.2f);
            targets.SetTarget(Profession.Builder, 0.0f);
            targets.SetTarget(Profession.Soldier, 0.0f);
            targets.Normalize();

            var target = BuilderSystem.FindBestImprovementTarget(builder, agents, buildings, storage, targets);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Kind, Is.EqualTo(BuildingKind.Quarry));
            Assert.That(target.Anchor, Is.EqualTo(new GridPos(54, 54)));
        }

        [Test]
        public void FindBestImprovementTarget_SkipsMissingTopProfessionTargets()
        {
            var builder = new Agent(new GridPos(50, 50), Profession.Builder, LifeStage.Adult);
            var agents = new List<Agent>();

            var buildings = new List<Building>
            {
                new Building(BuildingKind.Quarry, new GridPos(54, 54))
                {
                    IsUnderConstruction = true
                }
            };

            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Logs, 100);
            storage.Deposit(ResourceKind.Meat, 100);
            storage.Deposit(ResourceKind.PlantFood, 100);
            storage.Deposit(ResourceKind.Ore, 100);

            var targets = new ProfessionTargets();
            targets.SetTarget(Profession.Woodcutter, 0.9f);
            targets.SetTarget(Profession.Miner, 0.1f);
            targets.SetTarget(Profession.Hunter, 0.0f);
            targets.SetTarget(Profession.Farmer, 0.0f);
            targets.SetTarget(Profession.Builder, 0.0f);
            targets.SetTarget(Profession.Soldier, 0.0f);
            targets.Normalize();

            var target = BuilderSystem.FindBestImprovementTarget(builder, agents, buildings, storage, targets);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Kind, Is.EqualTo(BuildingKind.Quarry));
        }
    }
}
