using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class EnemySpawnerTests
    {
        private SimulationClock _clock;
        private EnemySpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _clock = new SimulationClock();
            _spawner = new EnemySpawner(_clock);
        }

        [Test]
        public void SpawnIfDue_BeforeCycle10_DoesNotSpawn()
        {
            _clock.Advance(9 * SimulationClock.SecondsPerCycle);

            var spawned = _spawner.SpawnIfDue();

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void SpawnIfDue_AtCycle10_SpawnsOneEnemyOnEdge()
        {
            _clock.Advance(10 * SimulationClock.SecondsPerCycle);

            var spawned = _spawner.SpawnIfDue();

            Assert.That(spawned, Has.Count.EqualTo(1));
            Assert.That(spawned[0].HitPoints, Is.EqualTo(Enemy.DefaultHitPoints));
            Assert.That(IsEdgeTile(spawned[0].Position), Is.True);
        }

        [Test]
        public void SpawnIfDue_Cycle20_SpawnsOneAdditionalEnemy()
        {
            _clock.Advance(10 * SimulationClock.SecondsPerCycle);
            var firstWave = _spawner.SpawnIfDue();

            _clock.Advance(10 * SimulationClock.SecondsPerCycle);
            var secondWave = _spawner.SpawnIfDue();

            Assert.That(firstWave, Has.Count.EqualTo(1));
            Assert.That(secondWave, Has.Count.EqualTo(1));
            Assert.That(secondWave.All(enemy => IsEdgeTile(enemy.Position)), Is.True);
            Assert.That(EnemySpawner.GetCumulativeSpawnCountForCycle(_clock.CurrentCycle), Is.EqualTo(2));
        }

        [Test]
        public void SpawnIfDue_SkippedCycles_CatchesUpToCumulativeCount()
        {
            _clock.Advance(30 * SimulationClock.SecondsPerCycle);

            var spawned = _spawner.SpawnIfDue();

            Assert.That(spawned, Has.Count.EqualTo(3));
            Assert.That(spawned.Select(enemy => enemy.Position).Distinct().Count(), Is.GreaterThan(1));
        }

        [Test]
        public void GetNextEdgeSpawnPosition_ReturnsOnlyEdgeTiles()
        {
            var positions = new HashSet<GridPos>();

            for (int i = 0; i < 8; i++)
            {
                positions.Add(_spawner.GetNextEdgeSpawnPosition());
            }

            Assert.That(positions.All(IsEdgeTile), Is.True);
        }

        private static bool IsEdgeTile(GridPos position)
        {
            return position.X == 0 ||
                   position.X == GridPos.MapWidth - 1 ||
                   position.Y == 0 ||
                   position.Y == GridPos.MapHeight - 1;
        }
    }
}
