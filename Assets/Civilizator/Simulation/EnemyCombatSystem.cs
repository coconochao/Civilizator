using System;

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
        public const float AttackIntervalSeconds = 1f;

        /// <summary>
        /// Enemy attacks are deterministic and cannot miss in V1.
        /// </summary>
        public const bool AttacksAlwaysHit = true;

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
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.HitPoints = Math.Max(0, target.HitPoints - EnemyDamage);
        }

        /// <summary>
        /// Applies the enemy's fixed attack damage to the given target.
        /// </summary>
        public static void ApplyAttack(Enemy target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.HitPoints = Math.Max(0, target.HitPoints - EnemyDamage);
        }
    }
}
