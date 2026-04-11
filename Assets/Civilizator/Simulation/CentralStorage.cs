namespace Civilizator.Simulation
{
    /// <summary>
    /// The central building's resource storage.
    /// Stores four integer stocks: Logs, Ore, Meat, PlantFood.
    /// </summary>
    public class CentralStorage
    {
        private int[] stocks = new int[(int)ResourceKind.PlantFood + 1];

        /// <summary>
        /// Gets the current stock for the given resource kind.
        /// </summary>
        public int GetStock(ResourceKind kind) => stocks[(int)kind];

        /// <summary>
        /// Deposits resources into storage (instant).
        /// Adds the amount to the current stock for the given resource kind.
        /// </summary>
        public void Deposit(ResourceKind kind, int amount)
        {
            if (amount < 0)
                throw new System.ArgumentException("Deposit amount must be non-negative.");
            stocks[(int)kind] += amount;
        }

        /// <summary>
        /// Attempts to withdraw resources from storage.
        /// Returns the amount actually withdrawn (up to requested amount, limited by current stock).
        /// </summary>
        public int Withdraw(ResourceKind kind, int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new System.ArgumentException("Withdraw amount must be non-negative.");
            
            int available = stocks[(int)kind];
            int withdrawn = System.Math.Min(requestedAmount, available);
            stocks[(int)kind] -= withdrawn;
            return withdrawn;
        }

        /// <summary>
        /// Gets all four stocks as a snapshot.
        /// </summary>
        public (int logs, int ore, int meat, int plantFood) GetAllStocks()
        {
            return (
                stocks[(int)ResourceKind.Logs],
                stocks[(int)ResourceKind.Ore],
                stocks[(int)ResourceKind.Meat],
                stocks[(int)ResourceKind.PlantFood]
            );
        }

        /// <summary>
        /// Resets all stocks to zero.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < stocks.Length; i++)
                stocks[i] = 0;
        }
    }
}
