namespace Civilizator.Simulation
{
    /// <summary>
    /// Types of natural resource nodes that spawn on the map.
    /// </summary>
    public enum NaturalNodeType
    {
        Tree,
        Plant,
        Animal,
        Ore
    }

    /// <summary>
    /// A natural resource node on the map.
    /// Nodes have a type and deplete when gathered from.
    /// Once depleted (remaining = 0), they are permanently unavailable.
    /// </summary>
    public class NaturalNode
    {
        public NaturalNodeType Type { get; }
        public GridPos Position { get; }
        public int Remaining { get; private set; }

        /// <summary>
        /// Initial remaining amount for all natural nodes.
        /// </summary>
        public const int InitialAmount = 100;

        public NaturalNode(NaturalNodeType type, GridPos position, int remaining = InitialAmount)
        {
            Type = type;
            Position = position;
            Remaining = remaining < 0 ? 0 : remaining;
        }

        /// <summary>
        /// Gather the specified amount from this node.
        /// Returns the actual amount gathered (may be less if node depletes).
        /// </summary>
        public int Gather(int amount)
        {
            if (amount < 0)
                return 0;

            int amountGathered = amount > Remaining ? Remaining : amount;
            Remaining -= amountGathered;
            return amountGathered;
        }

        /// <summary>
        /// Whether this node is completely depleted.
        /// </summary>
        public bool IsDepleted => Remaining <= 0;

        public override string ToString()
        {
            return $"NaturalNode({Type} at {Position}, {Remaining} remaining)";
        }
    }
}
