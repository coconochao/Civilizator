using System;

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
    }
}
