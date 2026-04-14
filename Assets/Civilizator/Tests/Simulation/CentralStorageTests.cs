using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class CentralStorageTests
    {
        private CentralStorage storage;

        [SetUp]
        public void Setup()
        {
            storage = new CentralStorage();
        }

        [Test]
        public void InitialStocksAreZero()
        {
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Logs));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Ore));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Meat));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.PlantFood));
        }

        [Test]
        public void DepositIncreasesStock()
        {
            storage.Deposit(ResourceKind.Logs, 50);
            Assert.AreEqual(50, storage.GetStock(ResourceKind.Logs));

            storage.Deposit(ResourceKind.Logs, 30);
            Assert.AreEqual(80, storage.GetStock(ResourceKind.Logs));
        }

        [Test]
        public void DepositMultipleResourceKinds()
        {
            storage.Deposit(ResourceKind.Logs, 10);
            storage.Deposit(ResourceKind.Ore, 20);
            storage.Deposit(ResourceKind.Meat, 5);
            storage.Deposit(ResourceKind.PlantFood, 15);

            Assert.AreEqual(10, storage.GetStock(ResourceKind.Logs));
            Assert.AreEqual(20, storage.GetStock(ResourceKind.Ore));
            Assert.AreEqual(5, storage.GetStock(ResourceKind.Meat));
            Assert.AreEqual(15, storage.GetStock(ResourceKind.PlantFood));
        }

        [Test]
        public void WithdrawDecreasesStock()
        {
            storage.Deposit(ResourceKind.Logs, 100);
            
            int withdrawn = storage.Withdraw(ResourceKind.Logs, 30);
            
            Assert.AreEqual(30, withdrawn);
            Assert.AreEqual(70, storage.GetStock(ResourceKind.Logs));
        }

        [Test]
        public void WithdrawMoreThanAvailableReturnsAvailable()
        {
            storage.Deposit(ResourceKind.Ore, 50);
            
            int withdrawn = storage.Withdraw(ResourceKind.Ore, 100);
            
            Assert.AreEqual(50, withdrawn);
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Ore));
        }

        [Test]
        public void WithdrawFromEmptyReturnsZero()
        {
            int withdrawn = storage.Withdraw(ResourceKind.Meat, 10);
            
            Assert.AreEqual(0, withdrawn);
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Meat));
        }

        [Test]
        public void GetAllStocksReturnsSnapshot()
        {
            storage.Deposit(ResourceKind.Logs, 10);
            storage.Deposit(ResourceKind.Ore, 20);
            storage.Deposit(ResourceKind.Meat, 5);
            storage.Deposit(ResourceKind.PlantFood, 15);

            var (logs, ore, meat, plantFood) = storage.GetAllStocks();

            Assert.AreEqual(10, logs);
            Assert.AreEqual(20, ore);
            Assert.AreEqual(5, meat);
            Assert.AreEqual(15, plantFood);
        }

        [Test]
        public void ResetClearsAllStocks()
        {
            storage.Deposit(ResourceKind.Logs, 100);
            storage.Deposit(ResourceKind.Ore, 50);
            storage.Deposit(ResourceKind.Meat, 30);
            storage.Deposit(ResourceKind.PlantFood, 20);

            storage.Reset();

            Assert.AreEqual(0, storage.GetStock(ResourceKind.Logs));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Ore));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.Meat));
            Assert.AreEqual(0, storage.GetStock(ResourceKind.PlantFood));
        }

        [Test]
        public void NegativeDepositThrows()
        {
            Assert.Throws<System.ArgumentException>(() => storage.Deposit(ResourceKind.Logs, -10));
        }

        [Test]
        public void NegativeWithdrawThrows()
        {
            Assert.Throws<System.ArgumentException>(() => storage.Withdraw(ResourceKind.Logs, -10));
        }
    }
}
