using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class GameOverStateTests
    {
        [Test]
        public void EvaluateCentralBuilding_AliveCentralDoesNotEndGame()
        {
            var state = new GameOverState();
            var central = new Building(BuildingKind.Central, new GridPos(50, 50));

            Assert.AreEqual(TowerCombatSystem.TowerMaxHitPoints, central.HitPoints);

            state.EvaluateCentralBuilding(central);

            Assert.IsFalse(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.None, state.Reason);
        }

        [Test]
        public void EvaluateCentralBuilding_ZeroHpCentralEndsGame()
        {
            var state = new GameOverState();
            var central = new Building(BuildingKind.Central, new GridPos(50, 50));

            CombatSystem.ApplyAttackTick(central, 100);
            state.EvaluateCentralBuilding(central);

            Assert.IsTrue(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.CentralDestroyed, state.Reason);
        }

        [Test]
        public void MarkGameOver_SecondTerminalReasonDoesNotOverwriteFirst()
        {
            var state = new GameOverState();

            state.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);
            state.MarkGameOver(GameOverState.GameOverReason.EveryoneDead);

            Assert.IsTrue(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.CentralDestroyed, state.Reason);
        }

        [Test]
        public void Reset_ClearsGameOverFlag()
        {
            var state = new GameOverState();

            state.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);
            state.Reset();

            Assert.IsFalse(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.None, state.Reason);
        }

        [Test]
        public void EvaluatePopulation_AllAgentsDeadEndsGame()
        {
            var state = new GameOverState();
            var agents = new List<Agent>
            {
                new Agent(new GridPos(1, 1)) { HitPoints = 0 },
                new Agent(new GridPos(2, 2)) { HitPoints = 0 },
                null
            };

            state.EvaluatePopulation(agents);

            Assert.IsTrue(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.EveryoneDead, state.Reason);
        }

        [Test]
        public void EvaluatePopulation_AnyLivingAgentPreventsGameOver()
        {
            var state = new GameOverState();
            var agents = new List<Agent>
            {
                new Agent(new GridPos(1, 1)) { HitPoints = 0 },
                new Agent(new GridPos(2, 2)) { HitPoints = 1 },
                null
            };

            state.EvaluatePopulation(agents);

            Assert.IsFalse(state.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.None, state.Reason);
        }
    }
}
