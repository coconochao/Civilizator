using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages the eating action for an agent.
    /// Includes traveling to central building and consuming food.
    /// </summary>
    public class EatingAction
    {
        /// <summary>
        /// Duration of eating in simulation seconds.
        /// </summary>
        public const float EatingDurationSeconds = 1.0f;

        /// <summary>
        /// The agent performing the eating action.
        /// </summary>
        public Agent Agent { get; }

        /// <summary>
        /// The central building location.
        /// </summary>
        public GridPos CentralBuildingLocation { get; }

        /// <summary>
        /// Current path the agent is following to reach the central building.
        /// </summary>
        public List<GridPos> PathToCenter { get; set; }

        /// <summary>
        /// Current index in the path (0 = current position, length-1 = destination).
        /// </summary>
        public int PathIndex { get; set; } = 0;

        /// <summary>
        /// Elapsed time during the eating action (in simulation seconds).
        /// </summary>
        public float EatingElapsedTime { get; set; } = 0f;

        /// <summary>
        /// Whether the agent has reached the central building.
        /// </summary>
        public bool HasReachedCenter { get; set; } = false;

        /// <summary>
        /// Whether the eating action is complete (food consumed or failed due to no food).
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Whether eating succeeded (food was available and consumed).
        /// </summary>
        public bool WasSuccessful { get; set; } = false;

        public EatingAction(Agent agent, GridPos centralBuildingLocation)
        {
            Agent = agent;
            CentralBuildingLocation = centralBuildingLocation;
            PathToCenter = new List<GridPos>();
        }

        /// <summary>
        /// Initializes the path from the agent's current position to the central building.
        /// Called when the action starts.
        /// </summary>
        public void InitializePath(GridOccupancy occupancy)
        {
            if (Agent.Position == CentralBuildingLocation)
            {
                HasReachedCenter = true;
                PathToCenter = new List<GridPos> { Agent.Position };
            }
            else
            {
                PathToCenter = Pathfinding.FindPathToOccupiedTarget(Agent.Position, CentralBuildingLocation, occupancy);
                if (PathToCenter.Count == 0)
                {
                    // No path found; action fails
                    CompleteEating(false);
                }
            }
        }

        /// <summary>
        /// Completes the eating action and applies starvation effects if needed.
        /// </summary>
        private void CompleteEating(bool success)
        {
            if (IsComplete)
                return;

            IsComplete = true;
            WasSuccessful = success;

            if (success)
            {
                Agent.MarkAsEaten();
            }
            else
            {
                Agent.EatingState.ApplyStarvationPenalty();
                if (Agent.EatingState.IsDeadFromStarvation)
                {
                    Agent.HitPoints = 0;
                }
            }
        }

        /// <summary>
        /// Advances the agent along the path toward the central building.
        /// </summary>
        public void AdvancePath()
        {
            if (HasReachedCenter || PathToCenter.Count == 0)
                return;

            if (PathIndex < PathToCenter.Count - 1)
            {
                PathIndex++;
                Agent.Position = PathToCenter[PathIndex];

                if (Agent.Position == CentralBuildingLocation)
                {
                    HasReachedCenter = true;
                }
            }
        }

        /// <summary>
        /// Updates the eating action with delta time.
        /// Handles path movement and eating timer.
        /// </summary>
        public void Update(float deltaTime, CentralStorage storage)
        {
            if (IsComplete)
                return;

            // If not at center, advance along path
            if (!HasReachedCenter)
            {
                AdvancePath();
                return;
            }

            // At center: accumulate eating time
            EatingElapsedTime += deltaTime;

            if (EatingElapsedTime >= EatingDurationSeconds)
            {
                // Eating complete; attempt to consume food
                bool foodConsumed = TryConsumeFood(storage);
                CompleteEating(foodConsumed);
            }
        }

        /// <summary>
        /// Attempts to consume 1 Meat and 1 Plant Food from central storage.
        /// Both types are required; returns true only if both can be withdrawn.
        /// </summary>
        private bool TryConsumeFood(CentralStorage storage)
        {
            // Check if both Meat and PlantFood are available
            if (storage.GetStock(ResourceKind.Meat) >= 1 && storage.GetStock(ResourceKind.PlantFood) >= 1)
            {
                storage.Withdraw(ResourceKind.Meat, 1);
                storage.Withdraw(ResourceKind.PlantFood, 1);
                return true;
            }

            // Not enough of one or both food types
            return false;
        }
    }
}
