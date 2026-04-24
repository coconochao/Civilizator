using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Helper methods for tower combat behavior.
    /// Towers require a soldier inside their footprint to fire.
    /// Their attack area is a 6x6 rectangle centered on the 2x2 tower footprint.
    /// </summary>
    public static class TowerCombatSystem
    {
        /// <summary>
        /// Maximum hit points for a tower.
        /// </summary>
        public const int TowerMaxHitPoints = 100;

        /// <summary>
        /// Base damage per tower attack.
        /// </summary>
        public const int BaseTowerDamage = 1;

        /// <summary>
        /// Damage per tower attack after upgrade.
        /// </summary>
        public const int UpgradedTowerDamage = 2;

        /// <summary>
        /// Tower attack cadence in seconds.
        /// </summary>
        public const float AttackIntervalSeconds = CombatSystem.AttackIntervalSeconds;

        /// <summary>
        /// Returns true when the tower can fire.
        /// A tower can fire only if it has at least one living soldier standing inside its footprint.
        /// </summary>
        public static bool CanTowerFire(Building tower, IEnumerable<Agent> agents)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));
            if (tower.Kind != BuildingKind.Tower)
                return false;

            return HasSoldierInside(tower, agents);
        }

        /// <summary>
        /// Returns the tower's damage per attack.
        /// Base towers deal 1 damage; upgraded towers deal 2 damage.
        /// </summary>
        public static int GetTowerDamage(Building tower)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                throw new ArgumentException("Tower damage requires a tower building.", nameof(tower));

            return tower.UpgradeLevel >= 1 ? UpgradedTowerDamage : BaseTowerDamage;
        }

        /// <summary>
        /// Returns the tower's maximum hit points.
        /// </summary>
        public static int GetTowerMaxHitPoints(Building tower)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                throw new ArgumentException("Tower hit points require a tower building.", nameof(tower));

            return TowerMaxHitPoints;
        }

        /// <summary>
        /// Returns the tower attack cadence in seconds.
        /// </summary>
        public static float GetTowerAttackIntervalSeconds(Building tower)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                throw new ArgumentException("Tower cadence requires a tower building.", nameof(tower));

            return AttackIntervalSeconds;
        }

        /// <summary>
        /// Returns true if at least one living soldier occupies a tile inside the tower footprint.
        /// </summary>
        public static bool HasSoldierInside(Building tower, IEnumerable<Agent> agents)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));

            var towerTiles = new List<GridPos>();
            tower.GetOccupiedTiles(towerTiles);

            foreach (var agent in agents)
            {
                if (agent == null || !agent.IsAlive || agent.Profession != Profession.Soldier)
                    continue;

                if (towerTiles.Contains(agent.Position))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the tiles affected by the tower's attack.
        /// The area is a 6x6 rectangle that extends two tiles beyond each side of the 2x2 footprint.
        /// </summary>
        public static List<GridPos> GetTowerHitArea(Building tower)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                throw new ArgumentException("Tower hit areas require a tower building.", nameof(tower));

            int footprintSize = tower.GetFootprintSize();
            int minX = tower.Anchor.X - 2;
            int maxX = tower.Anchor.X + footprintSize + 1;
            int minY = tower.Anchor.Y - 2;
            int maxY = tower.Anchor.Y + footprintSize + 1;

            var area = new List<GridPos>(36);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (x < 0 || x >= GridPos.MapWidth || y < 0 || y >= GridPos.MapHeight)
                        continue;

                    area.Add(new GridPos(x, y));
                }
            }

            return area;
        }

        /// <summary>
        /// Returns true when the given enemy tile falls inside the tower's attack area.
        /// </summary>
        public static bool IsEnemyInRange(Building tower, GridPos enemyPosition)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                return false;

            int footprintSize = tower.GetFootprintSize();
            int minX = tower.Anchor.X - 2;
            int maxX = tower.Anchor.X + footprintSize + 1;
            int minY = tower.Anchor.Y - 2;
            int maxY = tower.Anchor.Y + footprintSize + 1;

            return enemyPosition.X >= minX && enemyPosition.X <= maxX &&
                   enemyPosition.Y >= minY && enemyPosition.Y <= maxY;
        }

        /// <summary>
        /// Applies the tower's attack damage to an enemy target.
        /// Call this when the tower's attack tick resolves.
        /// </summary>
        public static void ApplyAttack(Building tower, Enemy target)
        {
            if (tower == null)
                throw new ArgumentNullException(nameof(tower));
            if (tower.Kind != BuildingKind.Tower)
                throw new ArgumentException("Tower attacks require a tower building.", nameof(tower));

            CombatSystem.ApplyAttackTick(target, GetTowerDamage(tower));
        }

        /// <summary>
        /// Updates tower combat against enemies.
        /// </summary>
        public static void UpdateTowerCombat(IEnumerable<Building> buildings, IEnumerable<Agent> agents, IEnumerable<Enemy> enemies, SimulationClock clock, float deltaTime)
        {
            // Placeholder for tower combat logic
            // Will be implemented in later tasks
        }
    }
}
