namespace Civilizator.Simulation
{
    /// <summary>
    /// Represents an enemy unit in the simulation.
    /// Enemy combat defaults live in EnemyCombatSystem.
    /// </summary>
    public class Enemy
    {
        /// <summary>
        /// Default hit points for newly spawned enemies.
        /// </summary>
        public const int DefaultHitPoints = EnemyCombatSystem.EnemyMaxHitPoints;

        /// <summary>
        /// Unique identifier for this enemy.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Current grid position for the enemy.
        /// </summary>
        public GridPos Position { get; set; }

        /// <summary>
        /// Current hit points.
        /// </summary>
        public int HitPoints { get; set; }

        private static int NextId { get; set; } = 1;

        public Enemy(GridPos position)
        {
            Id = NextId++;
            Position = position;
            HitPoints = DefaultHitPoints;
        }

        public Enemy(GridPos position, int id)
        {
            Id = id;
            Position = position;
            HitPoints = DefaultHitPoints;
        }

        /// <summary>
        /// Returns true when the enemy still has HP remaining.
        /// </summary>
        public bool IsAlive => HitPoints > 0;
    }
}
