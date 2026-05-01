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
            // Keep everyone fed through the warmup so the world reaches the enemy spawn without
            // accidentally ending via starvation.
            world.Storage.Deposit(ResourceKind.Meat, 200);
            world.Storage.Deposit(ResourceKind.PlantFood, 200);

            for (int cycle = 0; cycle < 10; cycle++)
            {
                world.SimulationStep(SimulationClock.SecondsPerCycle);
            }

            Assert.That(world.Enemies, Has.Count.EqualTo(1));

            var survivor = world.Agents.First(agent => agent.Profession == Profession.Builder);
            survivor.Position = new GridPos(0, 0);

            foreach (var agent in world.Agents.Where(agent => agent != survivor).ToList())
            {
                agent.HitPoints = 0;
            }

            var enemy = world.Enemies[0];
            var central = world.GetCentralBuilding();
            Assert.IsNotNull(central);

            var tower = world.AddBuilding(BuildingKind.Tower, new GridPos(45, 48));
            tower.IsUnderConstruction = false;
            var towerSoldier = world.AddAgent(tower.Anchor, Profession.Soldier, LifeStage.Adult);
            towerSoldier.SoldierMode = SoldierMode.Patrolling;
            towerSoldier.HasEatenThisCycle = true;

            world.SimulationStep(0f);
            var towerSoldierState = world.GetAgentActionState(towerSoldier);
            Assert.IsNotNull(towerSoldierState);
            towerSoldierState.PatrolPosition = tower.Anchor;

            // Put the spawned enemy next to the central building so combat can resolve immediately.
            enemy.Position = new GridPos(central.Anchor.X - 1, central.Anchor.Y);
            central.HitPoints = 1;

            TestContext.WriteLine($"Enemy at {enemy.Position}, central at {central.Anchor}, tower at {tower.Anchor}, survivor at {survivor.Position}");

            world.SimulationStep(1f);

            TestContext.WriteLine($"After step: enemy HP={enemy.HitPoints}, central HP={central.HitPoints}, gameOver={world.GameOver.IsGameOver}/{world.GameOver.Reason}");

            Assert.That(enemy.HitPoints, Is.LessThan(Enemy.DefaultHitPoints));
            Assert.That(central.HitPoints, Is.EqualTo(0));
            Assert.IsTrue(world.GameOver.IsGameOver);
            Assert.AreEqual(GameOverState.GameOverReason.CentralDestroyed, world.GameOver.Reason);
        }
    }
}
