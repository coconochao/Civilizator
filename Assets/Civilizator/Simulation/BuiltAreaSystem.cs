using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Computes the built-area radius used by soldier patrol placement.
    /// The radius is the maximum Manhattan distance from the central building footprint
    /// to any occupied tile in the current set of buildings.
    /// </summary>
    public static class BuiltAreaSystem
    {
        /// <summary>
        /// Computes the built-area radius using the central building found in the list.
        /// Returns 0 when no central building is present.
        /// </summary>
        public static int GetBuiltAreaRadius(IEnumerable<Building> buildings)
        {
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));

            Building centralBuilding = buildings.FirstOrDefault(building => building != null && building.Kind == BuildingKind.Central);
            if (centralBuilding == null)
                return 0;

            return GetBuiltAreaRadius(centralBuilding, buildings);
        }

        /// <summary>
        /// Computes the built-area radius using an explicit central building reference.
        /// </summary>
        public static int GetBuiltAreaRadius(Building centralBuilding, IEnumerable<Building> buildings)
        {
            if (centralBuilding == null)
                throw new ArgumentNullException(nameof(centralBuilding));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));

            int maxRadius = 0;
            foreach (var building in buildings)
            {
                if (building == null)
                    continue;

                int buildingRadius = GetDistanceFromCentralFootprint(centralBuilding, building);
                if (buildingRadius > maxRadius)
                    maxRadius = buildingRadius;
            }

            return maxRadius;
        }

        /// <summary>
        /// Computes the Manhattan distance from the central footprint to a building footprint.
        /// The distance is zero when the footprints overlap.
        /// </summary>
        public static int GetDistanceFromCentralFootprint(Building centralBuilding, Building targetBuilding)
        {
            if (centralBuilding == null)
                throw new ArgumentNullException(nameof(centralBuilding));
            if (targetBuilding == null)
                throw new ArgumentNullException(nameof(targetBuilding));

            return GetDistanceFromFootprintToFootprint(
                centralBuilding.Anchor,
                centralBuilding.GetFootprintSize(),
                targetBuilding.Anchor,
                targetBuilding.GetFootprintSize());
        }

        /// <summary>
        /// Computes the Manhattan distance between two square footprints.
        /// Returns zero when the footprints touch or overlap.
        /// </summary>
        private static int GetDistanceFromFootprintToFootprint(
            GridPos anchorA,
            int sizeA,
            GridPos anchorB,
            int sizeB)
        {
            int aMinX = anchorA.X;
            int aMaxX = anchorA.X + sizeA - 1;
            int aMinY = anchorA.Y;
            int aMaxY = anchorA.Y + sizeA - 1;

            int bMinX = anchorB.X;
            int bMaxX = anchorB.X + sizeB - 1;
            int bMinY = anchorB.Y;
            int bMaxY = anchorB.Y + sizeB - 1;

            int distanceX = 0;
            if (aMaxX < bMinX)
            {
                distanceX = bMaxX - aMaxX;
            }
            else if (bMaxX < aMinX)
            {
                distanceX = aMinX - bMinX;
            }

            int distanceY = 0;
            if (aMaxY < bMinY)
            {
                distanceY = bMaxY - aMaxY;
            }
            else if (bMaxY < aMinY)
            {
                distanceY = aMinY - bMinY;
            }

            return distanceX + distanceY;
        }
    }
}
