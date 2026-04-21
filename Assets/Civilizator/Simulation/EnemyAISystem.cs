using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Enemy targeting and movement helpers.
    /// 
    /// V1 target priority:
    /// 1. nearest person that is attacking the enemy
    /// 2. nearest tower that is attacking the enemy
    /// 3. nearest civilian/building
    /// 
    /// Movement uses the same 4-way pathfinding rules as citizens.
    /// </summary>
    public static class EnemyAISystem
    {
        /// <summary>
        /// Describes the chosen enemy target.
        /// </summary>
        public sealed class EnemyTarget
        {
            public EnemyTargetKind Kind { get; }
            public Agent Agent { get; }
            public Building Building { get; }

            public GridPos Position => Agent != null ? Agent.Position : Building.Anchor;

            public EnemyTarget(EnemyTargetKind kind, Agent agent, Building building)
            {
                Kind = kind;
                Agent = agent;
                Building = building;
            }
        }

        /// <summary>
        /// Target categories used by the enemy AI.
        /// </summary>
        public enum EnemyTargetKind
        {
            AttackingPerson,
            AttackingTower,
            Civilian,
            Building
        }

        /// <summary>
        /// Finds the best target for an enemy using the SPEC priority order.
        /// </summary>
        public static EnemyTarget FindBestTarget(
            Enemy enemy,
            IEnumerable<Agent> attackingPeople,
            IEnumerable<Building> attackingTowers,
            IEnumerable<Agent> civilians,
            IEnumerable<Building> buildings)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));
            if (attackingPeople == null)
                throw new ArgumentNullException(nameof(attackingPeople));
            if (attackingTowers == null)
                throw new ArgumentNullException(nameof(attackingTowers));
            if (civilians == null)
                throw new ArgumentNullException(nameof(civilians));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));

            var attackingPerson = FindNearestAgent(enemy.Position, attackingPeople);
            if (attackingPerson != null)
                return new EnemyTarget(EnemyTargetKind.AttackingPerson, attackingPerson, null);

            var attackingTower = FindNearestBuilding(enemy.Position, attackingTowers, requireTower: true);
            if (attackingTower != null)
                return new EnemyTarget(EnemyTargetKind.AttackingTower, null, attackingTower);

            var nearestCivilian = FindNearestAgent(enemy.Position, civilians);
            var nearestBuilding = FindNearestBuilding(enemy.Position, buildings, requireTower: false);

            if (nearestCivilian == null && nearestBuilding == null)
                return null;

            if (nearestCivilian == null)
                return new EnemyTarget(EnemyTargetKind.Building, null, nearestBuilding);

            if (nearestBuilding == null)
                return new EnemyTarget(EnemyTargetKind.Civilian, nearestCivilian, null);

            int civilianDistance = GridPos.Manhattan(enemy.Position, nearestCivilian.Position);
            int buildingDistance = GridPos.Manhattan(enemy.Position, nearestBuilding.Anchor);

            if (civilianDistance <= buildingDistance)
                return new EnemyTarget(EnemyTargetKind.Civilian, nearestCivilian, null);

            return new EnemyTarget(EnemyTargetKind.Building, null, nearestBuilding);
        }

        /// <summary>
        /// Moves an enemy one 4-way step toward its best available target.
        /// Returns true when the enemy moved.
        /// </summary>
        public static bool AdvanceEnemy(
            Enemy enemy,
            GridOccupancy occupancy,
            IEnumerable<Agent> attackingPeople,
            IEnumerable<Building> attackingTowers,
            IEnumerable<Agent> civilians,
            IEnumerable<Building> buildings)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));
            if (occupancy == null)
                throw new ArgumentNullException(nameof(occupancy));

            var target = FindBestTarget(enemy, attackingPeople, attackingTowers, civilians, buildings);
            if (target == null)
                return false;

            GridPos? destination = GetMovementDestination(enemy.Position, target, occupancy);
            if (!destination.HasValue)
                return false;

            if (destination.Value == enemy.Position)
                return false;

            var path = Pathfinding.FindPath(enemy.Position, destination.Value, occupancy);
            if (path.Count < 2)
                return false;

            enemy.Position = path[1];
            return true;
        }

        /// <summary>
        /// Gets the grid position the enemy should path toward for the selected target.
        /// </summary>
        public static GridPos? GetMovementDestination(
            GridPos enemyPosition,
            EnemyTarget target,
            GridOccupancy occupancy)
        {
            if (target == null)
                return null;
            if (occupancy == null)
                throw new ArgumentNullException(nameof(occupancy));

            if (target.Agent != null)
                return target.Agent.Position;

            return Pathfinding.FindNearestReachableTile(enemyPosition, target.Building.Anchor, occupancy);
        }

        private static Agent FindNearestAgent(GridPos origin, IEnumerable<Agent> agents)
        {
            Agent nearest = null;
            int minDistance = int.MaxValue;

            foreach (var agent in agents)
            {
                if (agent == null || !agent.IsAlive)
                    continue;

                int distance = GridPos.Manhattan(origin, agent.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = agent;
                }
            }

            return nearest;
        }

        private static Building FindNearestBuilding(GridPos origin, IEnumerable<Building> buildings, bool requireTower)
        {
            Building nearest = null;
            int minDistance = int.MaxValue;

            foreach (var building in buildings)
            {
                if (building == null)
                    continue;

                if (requireTower && building.Kind != BuildingKind.Tower)
                    continue;

                int distance = GridPos.Manhattan(origin, building.Anchor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = building;
                }
            }

            return nearest;
        }
    }
}
