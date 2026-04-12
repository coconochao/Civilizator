using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class AgentTests
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            var pos = new GridPos(5, 10);
            var agent = new Agent(pos);

            Assert.AreEqual(pos, agent.Position);
            Assert.AreEqual(Profession.Woodcutter, agent.Profession);
            Assert.AreEqual(LifeStage.Child, agent.LifeStage);
            Assert.AreEqual(Agent.DefaultHitPoints, agent.HitPoints);
        }

        [Test]
        public void Constructor_WithProfessionAndStage()
        {
            var pos = new GridPos(15, 20);
            var agent = new Agent(pos, Profession.Miner, LifeStage.Adult);

            Assert.AreEqual(pos, agent.Position);
            Assert.AreEqual(Profession.Miner, agent.Profession);
            Assert.AreEqual(LifeStage.Adult, agent.LifeStage);
            Assert.AreEqual(Agent.DefaultHitPoints, agent.HitPoints);
        }

        [Test]
        public void DefaultHitPoints_IsPublicConstant()
        {
            Assert.AreEqual(10, Agent.DefaultHitPoints);
        }

        [Test]
        public void IsAlive_WithPositiveHP_ReturnsTrue()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.HitPoints = 5;
            Assert.IsTrue(agent.IsAlive);
        }

        [Test]
        public void IsAlive_WithZeroHP_ReturnsFalse()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.HitPoints = 0;
            Assert.IsFalse(agent.IsAlive);
        }

        [Test]
        public void IsAlive_WithNegativeHP_ReturnsFalse()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.HitPoints = -5;
            Assert.IsFalse(agent.IsAlive);
        }

        [Test]
        public void Position_CanBeChanged()
        {
            var agent = new Agent(new GridPos(0, 0));
            var newPos = new GridPos(10, 10);
            agent.Position = newPos;
            Assert.AreEqual(newPos, agent.Position);
        }

        [Test]
        public void Profession_CanBeChanged()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            agent.Profession = Profession.Soldier;
            Assert.AreEqual(Profession.Soldier, agent.Profession);
        }

        [Test]
        public void LifeStage_CanBeChanged()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Hunter, LifeStage.Child);
            agent.LifeStage = LifeStage.Adult;
            Assert.AreEqual(LifeStage.Adult, agent.LifeStage);
        }

        [Test]
        public void HitPoints_CanBeChanged()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.HitPoints = 7;
            Assert.AreEqual(7, agent.HitPoints);
        }

        [Test]
        public void AllProfessions_CanBeAssigned()
        {
            var agent = new Agent(new GridPos(0, 0));

            var professions = new[]
            {
                Profession.Woodcutter,
                Profession.Miner,
                Profession.Hunter,
                Profession.Farmer,
                Profession.Builder,
                Profession.Soldier
            };

            foreach (var prof in professions)
            {
                agent.Profession = prof;
                Assert.AreEqual(prof, agent.Profession);
            }
        }

        [Test]
        public void AllLifeStages_CanBeAssigned()
        {
            var agent = new Agent(new GridPos(0, 0));

            var stages = new[] { LifeStage.Child, LifeStage.Adult, LifeStage.Elder };

            foreach (var stage in stages)
            {
                agent.LifeStage = stage;
                Assert.AreEqual(stage, agent.LifeStage);
            }
        }

        [Test]
        public void ToString_FormatsCorrectly()
        {
            var agent = new Agent(new GridPos(5, 10), Profession.Miner, LifeStage.Adult);
            agent.HitPoints = 7;
            string result = agent.ToString();

            Assert.IsTrue(result.Contains("Miner"));
            Assert.IsTrue(result.Contains("Adult"));
            Assert.IsTrue(result.Contains("5"));
            Assert.IsTrue(result.Contains("10"));
            Assert.IsTrue(result.Contains("7"));
        }
    }

    [TestFixture]
    public class LifeStageHelpersTests
    {
        [Test]
        public void GetProductivityMultiplier_Adult_Returns1()
        {
            Assert.AreEqual(1.0f, LifeStageHelpers.GetProductivityMultiplier(LifeStage.Adult));
        }

        [Test]
        public void GetProductivityMultiplier_Child_Returns0_5()
        {
            Assert.AreEqual(0.5f, LifeStageHelpers.GetProductivityMultiplier(LifeStage.Child));
        }

        [Test]
        public void GetProductivityMultiplier_Elder_Returns0_5()
        {
            Assert.AreEqual(0.5f, LifeStageHelpers.GetProductivityMultiplier(LifeStage.Elder));
        }

        [Test]
        public void GetNextStage_Child_ReturnsAdult()
        {
            Assert.AreEqual(LifeStage.Adult, LifeStageHelpers.GetNextStage(LifeStage.Child));
        }

        [Test]
        public void GetNextStage_Adult_ReturnsElder()
        {
            Assert.AreEqual(LifeStage.Elder, LifeStageHelpers.GetNextStage(LifeStage.Adult));
        }

        [Test]
        public void GetNextStage_Elder_ReturnsElder()
        {
            Assert.AreEqual(LifeStage.Elder, LifeStageHelpers.GetNextStage(LifeStage.Elder));
        }

        [Test]
        public void GetAgingDuration_Child_Returns10Cycles()
        {
            Assert.AreEqual(10, LifeStageHelpers.GetAgingDuration(LifeStage.Child));
        }

        [Test]
        public void GetAgingDuration_Adult_Returns60Cycles()
        {
            Assert.AreEqual(60, LifeStageHelpers.GetAgingDuration(LifeStage.Adult));
        }

        [Test]
        public void GetAgingDuration_Elder_Returns10Cycles()
        {
            Assert.AreEqual(10, LifeStageHelpers.GetAgingDuration(LifeStage.Elder));
        }

        [Test]
        public void AgingDurations_ArePublicConstants()
        {
            Assert.AreEqual(10, LifeStageHelpers.ChildToAdultCycles);
            Assert.AreEqual(60, LifeStageHelpers.AdultToElderCycles);
            Assert.AreEqual(10, LifeStageHelpers.ElderToDeathCycles);
        }

        [Test]
        public void ProductivityMultipliers_ChildAndElderEqual()
        {
            float childMult = LifeStageHelpers.GetProductivityMultiplier(LifeStage.Child);
            float elderMult = LifeStageHelpers.GetProductivityMultiplier(LifeStage.Elder);
            Assert.AreEqual(childMult, elderMult);
        }

        [Test]
        public void ProductivityMultipliers_AdultDoubleChildAndElder()
        {
            float adultMult = LifeStageHelpers.GetProductivityMultiplier(LifeStage.Adult);
            float childMult = LifeStageHelpers.GetProductivityMultiplier(LifeStage.Child);
            Assert.AreEqual(adultMult, childMult * 2);
        }

        [Test]
        public void LifeProgression_ChildAdultElderSequence()
        {
            var stage = LifeStage.Child;

            stage = LifeStageHelpers.GetNextStage(stage);
            Assert.AreEqual(LifeStage.Adult, stage);

            stage = LifeStageHelpers.GetNextStage(stage);
            Assert.AreEqual(LifeStage.Elder, stage);

            stage = LifeStageHelpers.GetNextStage(stage);
            Assert.AreEqual(LifeStage.Elder, stage); // Elder stays Elder (age to death handled elsewhere)
        }

        [Test]
        public void TotalAgingCycles_ChildToElder()
        {
            int totalCycles = LifeStageHelpers.ChildToAdultCycles + LifeStageHelpers.AdultToElderCycles;
            Assert.AreEqual(70, totalCycles); // 10 + 60
        }

        [Test]
        public void TotalLifespanCycles_ChildToDeath()
        {
            int totalCycles = LifeStageHelpers.ChildToAdultCycles + 
                             LifeStageHelpers.AdultToElderCycles + 
                             LifeStageHelpers.ElderToDeathCycles;
            Assert.AreEqual(80, totalCycles); // 10 + 60 + 10
        }
    }

    [TestFixture]
    public class AgentSpawningTests
    {
        [Test]
        public void SpawnAgents_CreatesMultipleWithoutNullRefs()
        {
            var agents = new List<Agent>();
            for (int i = 0; i < 10; i++)
            {
                var pos = new GridPos(i % 10, i / 10);
                agents.Add(new Agent(pos));
            }

            Assert.AreEqual(10, agents.Count);
            foreach (var agent in agents)
            {
                Assert.IsNotNull(agent);
                Assert.IsNotNull(agent.Position);
            }
        }

        [Test]
        public void SpawnAgents_WithVariedProfessions()
        {
            var professions = new[]
            {
                Profession.Woodcutter,
                Profession.Miner,
                Profession.Hunter,
                Profession.Farmer,
                Profession.Builder,
                Profession.Soldier
            };

            var agents = new List<Agent>();
            for (int i = 0; i < professions.Length; i++)
            {
                agents.Add(new Agent(new GridPos(i, 0), professions[i], LifeStage.Adult));
            }

            Assert.AreEqual(6, agents.Count);
            for (int i = 0; i < agents.Count; i++)
            {
                Assert.AreEqual(professions[i], agents[i].Profession);
            }
        }

        [Test]
        public void SpawnAgents_WithVariedLifeStages()
        {
            var stages = new[] { LifeStage.Child, LifeStage.Adult, LifeStage.Elder };

            var agents = new List<Agent>();
            for (int i = 0; i < stages.Length; i++)
            {
                agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, stages[i]));
            }

            Assert.AreEqual(3, agents.Count);
            for (int i = 0; i < agents.Count; i++)
            {
                Assert.AreEqual(stages[i], agents[i].LifeStage);
            }
        }

        [Test]
        public void SpawnAgents_AtVaryingPositions()
        {
            var positions = new[]
            {
                new GridPos(0, 0),
                new GridPos(10, 10),
                new GridPos(50, 50),
                new GridPos(99, 99),
                new GridPos(25, 75)
            };

            var agents = new List<Agent>();
            foreach (var pos in positions)
            {
                agents.Add(new Agent(pos));
            }

            Assert.AreEqual(5, agents.Count);
            for (int i = 0; i < agents.Count; i++)
            {
                Assert.AreEqual(positions[i], agents[i].Position);
            }
        }

        [Test]
        public void InitialAgentState_AllSpawnedChilden()
        {
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0));
                Assert.AreEqual(LifeStage.Child, agent.LifeStage);
                Assert.AreEqual(Agent.DefaultHitPoints, agent.HitPoints);
                Assert.IsTrue(agent.IsAlive);
            }
        }
    }
}
