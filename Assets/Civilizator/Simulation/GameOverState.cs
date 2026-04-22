using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Tracks whether the current run has ended and why.
    /// For V1, defeat occurs when:
    /// - the central building's HP reaches 0, or
    /// - all agents are dead.
    /// </summary>
    public sealed class GameOverState
    {
        /// <summary>
        /// Reasons the run can end in V1.
        /// </summary>
        public enum GameOverReason
        {
            None,
            CentralDestroyed,
            EveryoneDead
        }

        public bool IsGameOver { get; private set; }

        public GameOverReason Reason { get; private set; } = GameOverReason.None;

        /// <summary>
        /// Evaluates the central building and sets the game-over flag if it has been destroyed.
        /// </summary>
        public void EvaluateCentralBuilding(Building centralBuilding)
        {
            if (centralBuilding == null)
                throw new ArgumentNullException(nameof(centralBuilding));

            if (IsGameOver)
                return;

            if (centralBuilding.Kind != BuildingKind.Central)
                return;

            if (centralBuilding.HitPoints <= 0)
                MarkGameOver(GameOverReason.CentralDestroyed);
        }

        /// <summary>
        /// Evaluates the population and sets the game-over flag if no living agents remain.
        /// Null agents are ignored.
        /// </summary>
        public void EvaluatePopulation(System.Collections.Generic.IEnumerable<Agent> agents)
        {
            if (agents == null)
                throw new ArgumentNullException(nameof(agents));

            if (IsGameOver)
                return;

            foreach (var agent in agents)
            {
                if (agent != null && agent.IsAlive)
                    return;
            }

            MarkGameOver(GameOverReason.EveryoneDead);
        }

        /// <summary>
        /// Marks the run as over for the given reason.
        /// Repeated calls keep the first terminal reason.
        /// </summary>
        public void MarkGameOver(GameOverReason reason)
        {
            if (reason == GameOverReason.None)
                throw new ArgumentOutOfRangeException(nameof(reason), "Game over requires a terminal reason.");

            if (IsGameOver)
                return;

            IsGameOver = true;
            Reason = reason;
        }

        /// <summary>
        /// Clears the game-over state.
        /// Useful for resetting a test harness or starting a new run.
        /// </summary>
        public void Reset()
        {
            IsGameOver = false;
            Reason = GameOverReason.None;
        }
    }
}
