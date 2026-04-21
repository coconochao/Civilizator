using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Player-facing control values for soldier improvement work.
    /// The tower build emphasis slider determines whether soldiers prefer
    /// constructing new towers or upgrading existing ones.
    /// </summary>
    public static class SoldierImprovementControls
    {
        /// <summary>
        /// Default emphasis: balanced between tower construction and upgrades.
        /// </summary>
        public const float DefaultTowerBuildEmphasis = 0.5f;

        private static float _towerBuildEmphasis = DefaultTowerBuildEmphasis;

        /// <summary>
        /// Gets the current tower build emphasis value.
        /// 0 = always prefer upgrades, 1 = always prefer construction.
        /// </summary>
        public static float GetTowerBuildEmphasis() => _towerBuildEmphasis;

        /// <summary>
        /// Sets the tower build emphasis value.
        /// </summary>
        public static void SetTowerBuildEmphasis(float value)
        {
            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(nameof(value), "Tower build emphasis must be between 0 and 1");

            _towerBuildEmphasis = value;
        }

        /// <summary>
        /// Resets the control back to its default balanced state.
        /// </summary>
        public static void ResetToDefaults()
        {
            _towerBuildEmphasis = DefaultTowerBuildEmphasis;
        }
    }

    /// <summary>
    /// Soldier-specific improvement helpers.
    /// Soldiers only improve towers in V1, and the player can bias them toward
    /// new tower construction or tower upgrades via the emphasis control.
    /// </summary>
    public static class SoldierImprovementSystem
    {
        private const float BalancedEmphasisTolerance = 0.0001f;

        /// <summary>
        /// Finds the best tower improvement target for a soldier using the current
        /// player control value.
        /// </summary>
        public static Building FindBestImprovementTarget(Agent soldier, IEnumerable<Building> buildings)
        {
            return FindBestImprovementTarget(
                soldier,
                buildings,
                SoldierImprovementControls.GetTowerBuildEmphasis());
        }

        /// <summary>
        /// Finds the best tower improvement target for a soldier using an explicit
        /// build-vs-upgrade emphasis value.
        /// </summary>
        public static Building FindBestImprovementTarget(
            Agent soldier,
            IEnumerable<Building> buildings,
            float towerBuildEmphasis)
        {
            if (soldier == null)
                throw new ArgumentNullException(nameof(soldier));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));
            if (towerBuildEmphasis < 0f || towerBuildEmphasis > 1f)
                throw new ArgumentOutOfRangeException(nameof(towerBuildEmphasis), "Tower build emphasis must be between 0 and 1");
            if (soldier.Profession != Profession.Soldier)
                throw new ArgumentException("Soldier improvement targets are only selected for soldier agents", nameof(soldier));

            var towerTargets = buildings
                .Where(b => b != null && b.Kind == BuildingKind.Tower && !b.IsConstructionPhaseComplete())
                .ToList();

            if (towerTargets.Count == 0)
                return null;

            if (Math.Abs(towerBuildEmphasis - SoldierImprovementControls.DefaultTowerBuildEmphasis) <= BalancedEmphasisTolerance)
            {
                return FindNearestTowerTarget(soldier, towerTargets);
            }

            bool preferConstruction = towerBuildEmphasis > SoldierImprovementControls.DefaultTowerBuildEmphasis;

            var preferredTargets = preferConstruction
                ? towerTargets.Where(b => b.IsUnderConstruction).ToList()
                : towerTargets.Where(b => !b.IsUnderConstruction).ToList();

            if (preferredTargets.Count > 0)
                return FindNearestTowerTarget(soldier, preferredTargets);

            return FindNearestTowerTarget(soldier, towerTargets);
        }

        /// <summary>
        /// Returns the current tower improvement resource kind for a soldier target.
        /// Towers use Ore for both construction and upgrades.
        /// </summary>
        public static ResourceKind GetTowerImprovementResourceKind()
        {
            return ResourceKind.Ore;
        }

        private static Building FindNearestTowerTarget(Agent soldier, IReadOnlyList<Building> towers)
        {
            Building nearest = null;
            int minDistance = int.MaxValue;

            foreach (var tower in towers)
            {
                int distance = GridPos.Manhattan(soldier.Position, tower.Anchor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = tower;
                }
            }

            return nearest;
        }
    }
}
