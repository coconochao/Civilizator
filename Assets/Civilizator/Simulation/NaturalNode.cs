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
    /// Normal nodes (Tree, Plant, Animal, Ore without quarry support) are permanently unavailable once depleted.
    /// Ore nodes with quarry support can be gathered indefinitely even when remaining ≤ 0.
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
        /// For normal nodes (Tree, Plant, Animal): depleted when remaining ≤ 0.
        /// For ore nodes: check IsGatherable() which considers quarry support.
        /// </summary>
        public bool IsDepleted => Remaining <= 0;

        /// <summary>
        /// Checks if this node is gatherable, considering quarry support.
        /// - Normal nodes (Tree, Plant, Animal): gatherable only if remaining > 0.
        /// - Ore nodes without quarry support: gatherable only if remaining > 0.
        /// - Ore nodes with quarry support: gatherable even if remaining ≤ 0 (indefinite collection).
        /// </summary>
        public bool IsGatherable(bool hasQuarrySupport = false)
        {
            // Normal nodes always require remaining > 0
            if (Type != NaturalNodeType.Ore)
                return Remaining > 0;

            // Ore without quarry support requires remaining > 0
            if (!hasQuarrySupport)
                return Remaining > 0;

            // Ore with quarry support is always gatherable (indefinite)
            return true;
        }

        public override string ToString()
        {
            return $"NaturalNode({Type} at {Position}, {Remaining} remaining)";
        }
    }
}
