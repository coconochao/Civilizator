using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Enemy combat defaults and helper methods.
    /// V1 enemies have fixed stats, attack once per second, and do not miss.
    /// </summary>
    public static class EnemyCombatSystem
    {
        /// <summary>
        /// Maximum hit points for an enemy.
        /// </summary>
        public const int EnemyMaxHitPoints = 10;

        /// <summary>
        /// Damage dealt by an enemy on each attack.
        /// </summary>
        public const int EnemyDamage = 1;

        /// <summary>
        /// Enemy attack cadence in seconds.
        /// </summary>
        public const float AttackIntervalSeconds = CombatSystem.AttackIntervalSeconds;

        /// <summary>
        /// Enemy attacks are deterministic and cannot miss in V1.
        /// </summary>
        public const bool AttacksAlwaysHit = true;

        private static readonly Dictionary<int, float> EnemyAttackAccumulators = new Dictionary<int, float>();

        /// <summary>
        /// Returns the enemy's maximum hit points.
        /// </summary>
        public static int GetEnemyMaxHitPoints(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            return EnemyMaxHitPoints;
        }

        /// <summary>
        /// Returns the enemy's damage per attack.
        /// </summary>
        public static int GetEnemyDamage(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            return EnemyDamage;
        }

        /// <summary>
        /// Returns the enemy's attack cadence in seconds.
        /// </summary>
        public static float GetEnemyAttackIntervalSeconds(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            return AttackIntervalSeconds;
        }

        /// <summary>
        /// Returns true because enemy attacks always hit in V1.
        /// </summary>
        public static bool DoesEnemyAttackMiss(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            return !AttacksAlwaysHit;
        }

        /// <summary>
        /// Applies the enemy's fixed attack damage to the given target.
        /// </summary>
        public static void ApplyAttack(Agent target)
        {
            CombatSystem.ApplyAttackTick(target, EnemyDamage);
        }

        /// <summary>
        /// Applies the enemy's fixed attack damage to the given target.
        /// </summary>
        public static void ApplyAttack(Enemy target)
        {
            CombatSystem.ApplyAttackTick(target, EnemyDamage);
        }

        /// <summary>
        /// Applies the enemy's fixed attack damage to the given building.
        /// </summary>
        public static void ApplyAttack(Building target)
        {
            CombatSystem.ApplyAttackTick(target, EnemyDamage);
        }

        /// <summary>
        /// Updates enemy combat against agents and buildings.
        /// </summary>
        public static void UpdateEnemyCombat(IEnumerable<Enemy> enemies, IEnumerable<Agent> agents, Building central, SimulationClock clock, float deltaTime)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));
            if (deltaTime <= 0f)
                return;

            var livingAgents = agents.Where(agent => agent != null && agent.IsAlive).ToList();
            var buildings = new List<Building>();
            if (central != null)
                buildings.Add(central);

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float accumulator = GetEnemyAttackAccumulator(enemy.Id);
                accumulator += deltaTime;

                while (accumulator >= AttackIntervalSeconds)
                {
                    var target = EnemyAISystem.FindBestTarget(
                        enemy,
                        Enumerable.Empty<Agent>(),
                        Enumerable.Empty<Building>(),
                        livingAgents.Where(agent => agent.Profession != Profession.Soldier),
                        buildings);

                    if (target == null || !IsTargetInMeleeRange(enemy, target))
                        break;

                    if (target.Agent != null)
                    {
                        ApplyAttack(target.Agent);
                    }
                    else if (target.Building != null)
                    {
                        ApplyAttack(target.Building);
                    }

                    accumulator -= AttackIntervalSeconds;

                    livingAgents = agents.Where(agent => agent != null && agent.IsAlive).ToList();
                    if (livingAgents.Count == 0)
                        break;
                }

                EnemyAttackAccumulators[enemy.Id] = accumulator;
            }
        }

        private static float GetEnemyAttackAccumulator(int enemyId)
        {
            if (!EnemyAttackAccumulators.TryGetValue(enemyId, out var accumulator))
            {
                accumulator = 0f;
            }

            return accumulator;
        }

        private static bool IsTargetInMeleeRange(Enemy enemy, EnemyAISystem.EnemyTarget target)
        {
            if (target == null)
                return false;

            if (target.Agent != null)
                return GridPos.Manhattan(enemy.Position, target.Agent.Position) <= 1;

            if (target.Building != null)
            {
                var occupiedTiles = new List<GridPos>();
                target.Building.GetOccupiedTiles(occupiedTiles);
                foreach (var tile in occupiedTiles)
                {
                    if (GridPos.Manhattan(enemy.Position, tile) <= 1)
                        return true;
                }
            }

            return false;
        }
    }
}
