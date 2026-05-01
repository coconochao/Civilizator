using NUnit.Framework;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    /// <summary>
    /// Vertical slice coverage for enemy spawning, combat resolution, and lose conditions.
    /// </summary>
    [TestFixture]
    public class CombatVerticalSliceTests
    {
        [Test]
        public void EnemySpawn_CombatAndGameOver_WorkEndToEnd()
        {
            var world = new World();
            world.InitializeGameSetup();

            // Keep two soldiers alive so patrol/improve stays balanced and the tower can remain staffed.
            var soldiers = world.Agents.Where(agent => agent.Profession == Profession.Soldier).Take(2).ToList();
            Assert.That(soldiers, Has.Count.EqualTo(2));

            var patrolSoldier = soldiers[0];
            patrolSoldier.SoldierMode = SoldierMode.Patrolling;
            var improveSoldier = soldiers[1];
            improveSoldier.SoldierMode = SoldierMode.Improving;

            foreach (var agent in world.Agents.Where(agent => !soldiers.Contains(agent)).ToList())
            {
                agent.HitPoints = 0;
            }

            for (int cycle = 0; cycle < 10; cycle++)
            {
                world.SimulationStep(SimulationClock.SecondsPerCycle);
            }

            Assert.That(world.Enemies, Has.Count.EqualTo(1));

            var enemy = world.Enemies[0];
            var central = world.GetCentralBuilding();
            Assert.IsNotNull(central);

            // Create a staffed tower right before combat so it fires immediately without
            // carrying over a long pre-spawn attack accumulator.
            var tower = world.AddBuilding(BuildingKind.Tower, new GridPos(45, 48));
            tower.IsUnderConstruction = false;
            var soldierState = world.GetAgentActionState(patrolSoldier);
            patrolSoldier.Position = tower.Anchor;
            soldierState.PatrolPosition = tower.Anchor;
            improveSoldier.Position = new GridPos(0, 0);

            // Put the spawned enemy next to the central building so combat can resolve immediately.
            enemy.Position = new GridPos(central.Anchor.X - 1, central.Anchor.Y);
            central.HitPoints = 1;

            world.SimulationStep(1f);

            Assert.That(enemy.HitPoints, Is.LessThan(Enemy.DefaultHitPoints));
            Assert.That(central.HitPoints, Is.EqualTo(0));
            Assert.IsTrue(world.GameOver.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.CentralDestroyed, world.GameOver.Reason);
        }
    }
}
