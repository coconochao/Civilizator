using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Implements builder-specific improvement selection.
    /// Builders do not gather resources; they prioritize housing, then help the
    /// producer profession with the highest score according to the current stock mix.
    /// </summary>
    public static class BuilderSystem
    {
        private static readonly Profession[] ProducerProfessions =
        {
            Profession.Woodcutter,
            Profession.Miner,
            Profession.Hunter,
            Profession.Farmer
        };

        /// <summary>
        /// Returns true when the profession is Builder.
        /// </summary>
        public static bool IsBuilderProfession(Profession profession)
        {
            return profession == Profession.Builder;
        }

        /// <summary>
        /// Returns true when at least one living adult does not yet have a house assignment.
        /// </summary>
        public static bool HasUnassignedAdults(IEnumerable<Agent> agents)
        {
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));

            return agents.Any(a => a.IsAlive && a.LifeStage == LifeStage.Adult && !a.IsHouseAssigned);
        }

        /// <summary>
        /// Computes the builder priority score for a producer profession.
        /// Score = target share / current central stock of that profession's main resource.
        /// Zero stock is treated as the highest priority when the target share is positive.
        /// </summary>
        public static float GetPriorityScore(
            Profession profession,
            ProfessionTargets targets,
            CentralStorage storage)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (!ProductionSystem.IsProducerProfession(profession))
                throw new ArgumentException($"Profession {profession} is not a producer profession", nameof(profession));

            float targetShare = targets.GetTarget(profession);
            if (targetShare <= 0f)
                return 0f;

            ResourceKind resourceKind = ProductionSystem.GetRequiredResourceForProfession(profession);
            int currentStock = storage.GetStock(resourceKind);
            if (currentStock <= 0)
                return float.PositiveInfinity;

            return targetShare / currentStock;
        }

        /// <summary>
        /// Orders producer professions from highest to lowest builder priority score.
        /// Ties are broken by the enum order to keep the result deterministic.
        /// </summary>
        public static IReadOnlyList<Profession> GetPrioritizedProducerProfessions(
            ProfessionTargets targets,
            CentralStorage storage)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            return ProducerProfessions
                .OrderByDescending(profession => GetPriorityScore(profession, targets, storage))
                .ThenBy(profession => (int)profession)
                .ToArray();
        }

        /// <summary>
        /// Finds the nearest house that still requires construction or upgrade work.
        /// </summary>
        public static Building FindHousingTarget(Agent builder, IEnumerable<Building> buildings)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (!IsBuilderProfession(builder.Profession))
                throw new ArgumentException("Housing targets are only selected for builder agents", nameof(builder));

            var validTargets = buildings
                .Where(b => b.Kind == BuildingKind.House)
                .Where(b => b.IsUnderConstruction)
                .Where(b => !b.IsConstructionPhaseComplete())
                .ToList();

            if (validTargets.Count == 0)
                return null;

            Building nearest = null;
            int minDistance = int.MaxValue;

            foreach (var building in validTargets)
            {
                int distance = GridPos.Manhattan(builder.Position, building.Anchor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = building;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds the best improvement target for a builder.
        /// Builders prioritize housing when there are unassigned adults.
        /// Otherwise, they inspect producer professions in score order and
        /// choose the nearest available improvement target for the first profession with work.
        /// </summary>
        public static Building FindBestImprovementTarget(
            Agent builder,
            IEnumerable<Agent> agents,
            IEnumerable<Building> buildings,
            CentralStorage storage,
            ProfessionTargets targets)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (!IsBuilderProfession(builder.Profession))
                throw new ArgumentException("Best improvement targets are only selected for builder agents", nameof(builder));

            if (agents != null && HasUnassignedAdults(agents))
            {
                var housingTarget = FindHousingTarget(builder, buildings);
                if (housingTarget != null)
                    return housingTarget;
            }

            foreach (var profession in GetPrioritizedProducerProfessions(targets, storage))
            {
                var requiredBuilding = ProductionSystem.GetRequiredBuildingForProfession(profession);
                var target = ProductionSystem.FindNearestImprovementTarget(builder, buildings, requiredBuilding);
                if (target != null)
                    return target;
            }

            return null;
        }
    }
}
