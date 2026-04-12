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
    public class AgentProductivityTests
    {
        [Test]
        public void GetProductivityMultiplier_Child_NoHouse_Returns0_5()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            Assert.AreEqual(0.5f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void GetProductivityMultiplier_Adult_NoHouse_Returns1()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            Assert.AreEqual(1.0f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void GetProductivityMultiplier_Elder_NoHouse_Returns0_5()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            Assert.AreEqual(0.5f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void GetProductivityMultiplier_Child_WithHouse_Returns0_7()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            agent.AssignedHouseId = 1;
            Assert.AreEqual(0.5f + Agent.HouseAssignmentBonus, agent.GetProductivityMultiplier(), 0.0001f);
            Assert.AreEqual(0.7f, agent.GetProductivityMultiplier(), 0.0001f);
        }

        [Test]
        public void GetProductivityMultiplier_Adult_WithHouse_Returns1_2()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            agent.AssignedHouseId = 1;
            Assert.AreEqual(1.0f + Agent.HouseAssignmentBonus, agent.GetProductivityMultiplier(), 0.0001f);
            Assert.AreEqual(1.2f, agent.GetProductivityMultiplier(), 0.0001f);
        }

        [Test]
        public void GetProductivityMultiplier_Elder_WithHouse_Returns0_7()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            agent.AssignedHouseId = 1;
            Assert.AreEqual(0.5f + Agent.HouseAssignmentBonus, agent.GetProductivityMultiplier(), 0.0001f);
            Assert.AreEqual(0.7f, agent.GetProductivityMultiplier(), 0.0001f);
        }

        [Test]
        public void HouseAssignmentBonus_Is0_2()
        {
            Assert.AreEqual(0.2f, Agent.HouseAssignmentBonus);
        }

        [Test]
        public void IsHouseAssigned_WithNullId_ReturnsFalse()
        {
            var agent = new Agent(new GridPos(0, 0));
            Assert.IsNull(agent.AssignedHouseId);
            Assert.IsFalse(agent.IsHouseAssigned);
        }

        [Test]
        public void IsHouseAssigned_WithValidId_ReturnsTrue()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.AssignedHouseId = 5;
            Assert.IsTrue(agent.IsHouseAssigned);
        }
    }

    [TestFixture]
    public class AgentCarryCapacityTests
    {
        [Test]
        public void BaseCarryCapacity_Is10()
        {
            Assert.AreEqual(10, Agent.BaseCarryCapacity);
        }

        [Test]
        public void GetCarryCapacity_Child_NoHouse_Returns5()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            Assert.AreEqual(5, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_Adult_NoHouse_Returns10()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            Assert.AreEqual(10, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_Elder_NoHouse_Returns5()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            Assert.AreEqual(5, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_Child_WithHouse_Returns7()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            agent.AssignedHouseId = 1;
            // 0.7 * 10 = 7
            Assert.AreEqual(7, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_Adult_WithHouse_Returns12()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            agent.AssignedHouseId = 1;
            // 1.2 * 10 = 12
            Assert.AreEqual(12, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_Elder_WithHouse_Returns7()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            agent.AssignedHouseId = 1;
            // 0.7 * 10 = 7
            Assert.AreEqual(7, agent.GetCarryCapacity());
        }

        [Test]
        public void GetCarryCapacity_ScalesWithProductivityMultiplier()
        {
            var childNoHouse = new Agent(new GridPos(0, 0), Profession.Hunter, LifeStage.Child);
            var childWithHouse = new Agent(new GridPos(0, 0), Profession.Hunter, LifeStage.Child);
            childWithHouse.AssignedHouseId = 1;

            int noHouseCapacity = childNoHouse.GetCarryCapacity();
            int withHouseCapacity = childWithHouse.GetCarryCapacity();

            Assert.IsTrue(withHouseCapacity > noHouseCapacity);
            Assert.AreEqual(5, noHouseCapacity);
            Assert.AreEqual(7, withHouseCapacity);
        }

        [Test]
        public void GetCarryCapacity_MultipleAgents_CorrectByStage()
        {
            var agents = new[]
            {
                new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child),
                new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult),
                new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Elder)
            };

            Assert.AreEqual(5, agents[0].GetCarryCapacity()); // Child: 10 * 0.5
            Assert.AreEqual(10, agents[1].GetCarryCapacity()); // Adult: 10 * 1.0
            Assert.AreEqual(5, agents[2].GetCarryCapacity()); // Elder: 10 * 0.5
        }
    }
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

    [TestFixture]
    public class AgentEatingTests
    {
        [Test]
        public void HasEatenThisCycle_InitializesAsFalse()
        {
            var agent = new Agent(new GridPos(0, 0));
            Assert.IsFalse(agent.HasEatenThisCycle);
        }

        [Test]
        public void MarkAsEaten_SetsHasEatenThisCycleToTrue()
        {
            var agent = new Agent(new GridPos(0, 0));
            Assert.IsFalse(agent.HasEatenThisCycle);
            agent.MarkAsEaten();
            Assert.IsTrue(agent.HasEatenThisCycle);
        }

        [Test]
        public void ResetEatingFlag_ClearsHasEatenThisCycle()
        {
            var agent = new Agent(new GridPos(0, 0));
            agent.MarkAsEaten();
            Assert.IsTrue(agent.HasEatenThisCycle);
            agent.ResetEatingFlag();
            Assert.IsFalse(agent.HasEatenThisCycle);
        }

        [Test]
        public void AgentEatsAtMostOncePerCycle()
        {
            var agent = new Agent(new GridPos(0, 0));
            
            // Cycle 1: Agent eats
            Assert.IsFalse(agent.HasEatenThisCycle);
            agent.MarkAsEaten();
            Assert.IsTrue(agent.HasEatenThisCycle);
            
            // Try to mark as eaten again (should still be true, but flag doesn't allow multiple eats)
            agent.MarkAsEaten();
            Assert.IsTrue(agent.HasEatenThisCycle);
            
            // Cycle 2: Reset flag for new cycle
            agent.ResetEatingFlag();
            Assert.IsFalse(agent.HasEatenThisCycle);
            
            // Agent eats again in cycle 2
            agent.MarkAsEaten();
            Assert.IsTrue(agent.HasEatenThisCycle);
        }

        [Test]
        public void MultipleAgents_HaveIndependentEatingFlags()
        {
            var agent1 = new Agent(new GridPos(0, 0));
            var agent2 = new Agent(new GridPos(1, 1));
            
            agent1.MarkAsEaten();
            Assert.IsTrue(agent1.HasEatenThisCycle);
            Assert.IsFalse(agent2.HasEatenThisCycle);
            
            agent2.MarkAsEaten();
            Assert.IsTrue(agent1.HasEatenThisCycle);
            Assert.IsTrue(agent2.HasEatenThisCycle);
            
            agent1.ResetEatingFlag();
            Assert.IsFalse(agent1.HasEatenThisCycle);
            Assert.IsTrue(agent2.HasEatenThisCycle);
        }

        [Test]
        public void EatingFlag_PersistsThroughOtherStateChanges()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            agent.MarkAsEaten();
            
            // Change other properties
            agent.Position = new GridPos(5, 5);
            agent.Profession = Profession.Miner;
            agent.LifeStage = LifeStage.Adult;
            agent.HitPoints = 8;
            agent.AssignedHouseId = 2;
            
            // Eating flag should persist
            Assert.IsTrue(agent.HasEatenThisCycle);
            
            // Reset should still work
            agent.ResetEatingFlag();
            Assert.IsFalse(agent.HasEatenThisCycle);
        }

        [Test]
        public void EatingFlag_WorksForAllLifeStages()
        {
            var stages = new[] { LifeStage.Child, LifeStage.Adult, LifeStage.Elder };
            
            foreach (var stage in stages)
            {
                var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, stage);
                Assert.IsFalse(agent.HasEatenThisCycle);
                agent.MarkAsEaten();
                Assert.IsTrue(agent.HasEatenThisCycle);
                agent.ResetEatingFlag();
                Assert.IsFalse(agent.HasEatenThisCycle);
            }
        }

        [Test]
        public void EatingFlag_WorksForAllProfessions()
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
            
            foreach (var prof in professions)
            {
                var agent = new Agent(new GridPos(0, 0), prof, LifeStage.Adult);
                Assert.IsFalse(agent.HasEatenThisCycle);
                agent.MarkAsEaten();
                Assert.IsTrue(agent.HasEatenThisCycle);
                agent.ResetEatingFlag();
                Assert.IsFalse(agent.HasEatenThisCycle);
            }
        }
    }

    [TestFixture]
    public class AgentStarvationTests
    {
        [Test]
        public void EatingState_InitializesWithNoPenalty()
        {
            var agent = new Agent(new GridPos(0, 0));
            Assert.AreEqual(0f, agent.EatingState.StarvationPenalty);
        }

        [Test]
        public void ApplyStarvationPenalty_ReducesProductivity()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            // Base multiplier for Adult: 1.0
            Assert.AreEqual(1.0f, agent.GetProductivityMultiplier());

            // Apply one starvation penalty (-25%)
            agent.EatingState.ApplyStarvationPenalty();
            Assert.AreEqual(0.75f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void StarvationPenalty_StacksAdditively()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            
            // Apply 4 penalties (each -25%)
            for (int i = 0; i < 4; i++)
            {
                agent.EatingState.ApplyStarvationPenalty();
            }

            // Should reach 0% (1.0 - 1.0 = 0.0)
            Assert.AreEqual(0f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void StarvationPenalty_CapsAt100Percent()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            
            // Apply 10 penalties (way more than needed to cap)
            for (int i = 0; i < 10; i++)
            {
                agent.EatingState.ApplyStarvationPenalty();
            }

            // Penalty should be capped at 1.0
            Assert.AreEqual(1.0f, agent.EatingState.StarvationPenalty);
            Assert.AreEqual(0f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void ResetStarvationPenalty_ClearsStarvation()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            agent.EatingState.ApplyStarvationPenalty();
            agent.EatingState.ApplyStarvationPenalty();
            
            Assert.AreEqual(0.5f, agent.GetProductivityMultiplier());
            
            agent.EatingState.ResetStarvationPenalty();
            Assert.AreEqual(0f, agent.EatingState.StarvationPenalty);
            Assert.AreEqual(1.0f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void StarvationPenalty_InteractsWithHouseBonus()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            agent.AssignedHouseId = 1; // +20% bonus
            
            // Without starvation: 1.0 + 0.2 = 1.2
            Assert.AreEqual(1.2f, agent.GetProductivityMultiplier());
            
            // Apply starvation: 1.2 - 0.25 = 0.95
            agent.EatingState.ApplyStarvationPenalty();
            Assert.AreEqual(0.95f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void ProductivityMultiplier_NeverGoesNegative()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            // Base: 0.5, with starvation capping at 0.5 + 1.0 penalty = -0.5, should clamp to 0
            
            agent.EatingState.ApplyStarvationPenalty();
            agent.EatingState.ApplyStarvationPenalty();
            agent.EatingState.ApplyStarvationPenalty();
            
            Assert.AreEqual(0f, agent.GetProductivityMultiplier());
        }

        [Test]
        public void IsDeadFromStarvation_ReturnsTrue_At100Percent()
        {
            var agent = new Agent(new GridPos(0, 0));
            Assert.IsFalse(agent.EatingState.IsDeadFromStarvation);
            
            // Apply 4 penalties to reach 100%
            for (int i = 0; i < 4; i++)
            {
                agent.EatingState.ApplyStarvationPenalty();
            }
            
            Assert.IsTrue(agent.EatingState.IsDeadFromStarvation);
        }
    }

    [TestFixture]
    public class EatingActionTests
    {
        [Test]
        public void EatingAction_InitializeWithAgentAndLocation()
        {
            var agent = new Agent(new GridPos(5, 5));
            var center = new GridPos(50, 50);
            var action = new EatingAction(agent, center);
            
            Assert.AreEqual(agent, action.Agent);
            Assert.AreEqual(center, action.CentralBuildingLocation);
            Assert.IsFalse(action.IsComplete);
            Assert.IsFalse(action.WasSuccessful);
        }

        [Test]
        public void EatingAction_AtCenterImmediately_IfAlreadyThere()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            Assert.IsTrue(action.HasReachedCenter);
        }

        [Test]
        public void EatingAction_ConsumesMeatIfAvailable()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Meat, 5);
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            // Simulate eating
            action.Update(1.5f, storage); // Simulate 1.5 seconds (exceeds 1 second eating time)
            
            Assert.IsTrue(action.IsComplete);
            Assert.IsTrue(action.WasSuccessful);
            Assert.AreEqual(4, storage.GetStock(ResourceKind.Meat)); // One consumed
        }

        [Test]
        public void EatingAction_ConsumesPlantFoodIfMeatUnavailable()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.PlantFood, 3);
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            // Simulate eating
            action.Update(1.5f, storage);
            
            Assert.IsTrue(action.IsComplete);
            Assert.IsTrue(action.WasSuccessful);
            Assert.AreEqual(2, storage.GetStock(ResourceKind.PlantFood)); // One consumed
        }

        [Test]
        public void EatingAction_PrefersMeatOverPlantFood()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Meat, 2);
            storage.Deposit(ResourceKind.PlantFood, 5);
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            // Simulate eating
            action.Update(1.5f, storage);
            
            Assert.AreEqual(1, storage.GetStock(ResourceKind.Meat)); // Meat consumed
            Assert.AreEqual(5, storage.GetStock(ResourceKind.PlantFood)); // PlantFood untouched
        }

        [Test]
        public void EatingAction_FailsWhenNoFoodAvailable()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var storage = new CentralStorage(); // Empty storage
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            // Simulate eating
            action.Update(1.5f, storage);
            
            Assert.IsTrue(action.IsComplete);
            Assert.IsFalse(action.WasSuccessful);
        }

        [Test]
        public void EatingAction_RequiresFullSecond()
        {
            var center = new GridPos(50, 50);
            var agent = new Agent(center);
            var action = new EatingAction(agent, center);
            
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Meat, 1);
            
            var occupancy = new GridOccupancy();
            action.InitializePath(occupancy);
            
            // Simulate 0.5 seconds (less than eating time)
            action.Update(0.5f, storage);
            Assert.IsFalse(action.IsComplete);
            
            // Simulate another 0.4 seconds (total 0.9, still not complete)
            action.Update(0.4f, storage);
            Assert.IsFalse(action.IsComplete);
            
            // Simulate another 0.2 seconds (total 1.1, should complete)
            action.Update(0.2f, storage);
            Assert.IsTrue(action.IsComplete);
            Assert.IsTrue(action.WasSuccessful);
        }

        [Test]
        public void EatingAction_FailsWhenNoPathAvailable()
        {
            var agent = new Agent(new GridPos(0, 0));
            var center = new GridPos(50, 50);
            var action = new EatingAction(agent, center);
            
            // Create occupancy that blocks all paths
            var occupancy = new GridOccupancy();
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    occupancy.RegisterOccupant(new GridPos(x, y));
                }
            }
            
            action.InitializePath(occupancy);
            
            Assert.IsTrue(action.IsComplete);
            Assert.IsFalse(action.WasSuccessful);
        }
    }
}
