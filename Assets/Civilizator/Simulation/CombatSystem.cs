using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Shared combat resolution helpers for all attack sources.
    /// Attack sources call into this pipeline when their attack tick completes,
    /// so damage application behaves consistently for melee and ranged combat.
    /// </summary>
    public static class CombatSystem
    {
        /// <summary>
        /// Default attack cadence used throughout V1 combat.
        /// </summary>
        public const float AttackIntervalSeconds = 1f;

        private static readonly Dictionary<int, float> SoldierAttackAccumulators = new Dictionary<int, float>();

        /// <summary>
        /// Applies damage to an agent on an attack tick.
        /// </summary>
        public static void ApplyAttackTick(Agent target, int damage)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ApplyDamage(target, damage);
        }

        /// <summary>
        /// Applies damage to an enemy on an attack tick.
        /// </summary>
        public static void ApplyAttackTick(Enemy target, int damage)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ApplyDamage(target, damage);
        }

        /// <summary>
        /// Applies damage to a building on an attack tick.
        /// </summary>
        public static void ApplyAttackTick(Building target, int damage)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ApplyDamage(target, damage);
        }

        private static void ApplyDamage(Agent target, int damage)
        {
            ValidateDamage(damage);
            target.HitPoints = Math.Max(0, target.HitPoints - damage);
        }

        private static void ApplyDamage(Enemy target, int damage)
        {
            ValidateDamage(damage);
            target.HitPoints = Math.Max(0, target.HitPoints - damage);
        }

        private static void ApplyDamage(Building target, int damage)
        {
            ValidateDamage(damage);
            target.HitPoints = Math.Max(0, target.HitPoints - damage);
        }

        private static void ValidateDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be non-negative.");
        }

        /// <summary>
        /// Updates soldier combat against enemies.
        /// </summary>
        public static void UpdateSoldierCombat(IEnumerable<Agent> agents, IEnumerable<Enemy> enemies, SimulationClock clock, float deltaTime)
        {
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));
            if (deltaTime <= 0f)
                return;

            var livingEnemies = enemies.Where(enemy => enemy != null && enemy.IsAlive).ToList();
            if (livingEnemies.Count == 0)
                return;

            foreach (var soldier in agents)
            {
                if (soldier == null || !soldier.IsAlive || soldier.Profession != Profession.Soldier || soldier.SoldierMode != SoldierMode.Patrolling)
                    continue;

                float accumulator = GetSoldierAttackAccumulator(soldier.Id);
                accumulator += deltaTime;

                while (accumulator >= AttackIntervalSeconds)
                {
                    var target = FindNearestEnemyInMeleeRange(soldier, livingEnemies);
                    if (target == null)
                        break;

                    ApplyAttackTick(target, 1);
                    accumulator -= AttackIntervalSeconds;

                    livingEnemies = enemies.Where(enemy => enemy != null && enemy.IsAlive).ToList();
                    if (livingEnemies.Count == 0)
                        break;
                }

                SoldierAttackAccumulators[soldier.Id] = accumulator;
            }
        }

        private static float GetSoldierAttackAccumulator(int soldierId)
        {
            if (!SoldierAttackAccumulators.TryGetValue(soldierId, out var accumulator))
            {
                accumulator = 0f;
            }

            return accumulator;
        }

        private static Enemy FindNearestEnemyInMeleeRange(Agent soldier, IReadOnlyList<Enemy> enemies)
        {
            Enemy nearest = null;
            int bestDistance = int.MaxValue;

            foreach (var enemy in enemies)
            {
                int distance = GridPos.Manhattan(soldier.Position, enemy.Position);
                if (distance > 1)
                    continue;

                if (distance < bestDistance || (distance == bestDistance && (nearest == null || enemy.Id < nearest.Id)))
                {
                    nearest = enemy;
                    bestDistance = distance;
                }
            }

            return nearest;
        }
    }
}
