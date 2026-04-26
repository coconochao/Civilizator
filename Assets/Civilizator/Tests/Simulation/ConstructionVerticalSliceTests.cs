using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    /// <summary>
    /// Vertical slice tests for the construction flow:
    /// gather → deposit → withdraw → deliver → building completes
    /// </summary>
    [TestFixture]
    public class ConstructionVerticalSliceTests
    {
        [Test]
        public void Gather_Deposit_Construct_Complete_LogsDecrease_BuildingCompletes()
        {
            // Arrange
            var world = new World();
            world.InitializeGameSetup();
            
            // Add a tree node near central for gathering
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(52, 52));
            world.NaturalNodes.Add(tree);
            
            // Add a plantation under construction near central
            var plantation = world.AddBuilding(BuildingKind.Plantation, new GridPos(45, 45));
            
            // Give central storage some initial food so agent doesn't starve
            world.Storage.Deposit(ResourceKind.Meat, 100);
            world.Storage.Deposit(ResourceKind.PlantFood, 100);
            
            var woodcutter = world.Agents.FirstOrDefault(a => a.Profession == Profession.Woodcutter);
            Assert.IsNotNull(woodcutter, "Should have a woodcutter agent");
            
            int initialLogs = world.Storage.GetStock(ResourceKind.Logs);
            int initialProgress = plantation.ConstructionProgress;
            
            // Act - Simulate enough time for full cycle
            // Move to tree (1 tile), gather (10 units at 1/sec), move to central (3 tiles), deposit (instant)
            // Move to central (from plantation area), withdraw, deliver
            // Total time needed: ~30 seconds for the full cycle
            
            float totalTime = 120f; // 2 minutes should be more than enough
            float deltaTime = 0.1f;
            int steps = (int)(totalTime / deltaTime);
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                
                // Stop if building is complete
                if (!plantation.IsUnderConstruction && plantation.IsConstructionPhaseComplete())
                {
                    break;
                }
            }
            
            // Assert
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            int finalProgress = plantation.ConstructionProgress;
            
            // Logs should have decreased (used for construction)
            // Note: Logs might also have increased from gathering, but net should be negative or zero
            // since we started with 0 logs and used them for construction
            Assert.LessOrEqual(finalLogs, initialLogs, 
                "Logs should not increase net (gathered logs used for construction)");
            
            // Building should complete
            Assert.IsFalse(plantation.IsUnderConstruction, 
                "Plantation should no longer be under construction");
            Assert.IsTrue(plantation.IsConstructionPhaseComplete(), 
                "Plantation construction phase should be complete");
            Assert.Greater(finalProgress, initialProgress, 
                "Construction progress should have increased");
        }
        
        [Test]
        public void ProducerAgent_GathersFromNode_DepositsAtCentral_ResourcesAdded()
        {
            // Arrange
            var world = new World();
            world.InitializeGameSetup();
            
            // Add a tree node
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(52, 52));
            world.NaturalNodes.Add(tree);
            
            // Give food to prevent starvation
            world.Storage.Deposit(ResourceKind.Meat, 100);
            world.Storage.Deposit(ResourceKind.PlantFood, 100);
            
            var woodcutter = world.Agents.FirstOrDefault(a => a.Profession == Profession.Woodcutter);
            Assert.IsNotNull(woodcutter);
            
            int initialLogs = world.Storage.GetStock(ResourceKind.Logs);
            int initialNodeRemaining = tree.Remaining;
            
            // Act - Simulate gathering and depositing
            float timeToGatherAndDeposit = 30f;
            float deltaTime = 0.1f;
            int steps = (int)(timeToGatherAndDeposit / deltaTime);
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                
                // Stop if agent has deposited at least once
                if (world.Storage.GetStock(ResourceKind.Logs) > initialLogs)
                {
                    // Continue a bit more to ensure full deposit
                    for (int j = 0; j < 50; j++)
                    {
                        world.SimulationStep(deltaTime);
                    }
                    break;
                }
            }
            
            // Assert
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            int finalNodeRemaining = tree.Remaining;
            
            Assert.Greater(finalLogs, initialLogs, 
                "Logs in central storage should increase after gathering and depositing");
            Assert.Less(finalNodeRemaining, initialNodeRemaining, 
                "Tree should have fewer resources after gathering");
        }
        
        [Test]
        public void BuilderAgent_WithdrawsFromCentral_DeliversToBuilding_ProgressIncreases()
        {
            // Arrange
            var world = new World();
            world.InitializeGameSetup();
            
            // Add logs to central storage
            world.Storage.Deposit(ResourceKind.Logs, 200);
            world.Storage.Deposit(ResourceKind.Meat, 100);
            world.Storage.Deposit(ResourceKind.PlantFood, 100);
            
            // Add a house under construction
            var house = world.AddBuilding(BuildingKind.House, new GridPos(45, 45));
            
            var builder = world.Agents.FirstOrDefault(a => a.Profession == Profession.Builder);
            Assert.IsNotNull(builder);
            
            int initialProgress = house.ConstructionProgress;
            int initialLogs = world.Storage.GetStock(ResourceKind.Logs);
            
            // Act - Simulate builder working
            float timeToComplete = 60f;
            float deltaTime = 0.1f;
            int steps = (int)(timeToComplete / deltaTime);
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                
                // Stop if building is complete
                if (!house.IsUnderConstruction && house.IsConstructionPhaseComplete())
                {
                    break;
                }
            }
            
            // Assert
            int finalProgress = house.ConstructionProgress;
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            
            Assert.Greater(finalProgress, initialProgress, 
                "Construction progress should increase");
            Assert.Less(finalLogs, initialLogs, 
                "Logs should decrease as they're used for construction");
            Assert.IsTrue(house.IsConstructionPhaseComplete(), 
                "House should complete construction");
        }
        
        [Test]
        public void ThresholdControl_ForcesImprovement_WhenStockHigh()
        {
            // Arrange
            var world = new World();
            world.InitializeGameSetup();
            
            // Add tree and plantation
            var tree = new NaturalNode(NaturalNodeType.Tree, new GridPos(52, 52));
            world.NaturalNodes.Add(tree);
            
            var plantation = world.AddBuilding(BuildingKind.Plantation, new GridPos(45, 45));
            
            // Give food and lots of logs to trigger improvement threshold
            world.Storage.Deposit(ResourceKind.Meat, 100);
            world.Storage.Deposit(ResourceKind.PlantFood, 100);
            world.Storage.Deposit(ResourceKind.Logs, 500); // High stock
            
            var woodcutter = world.Agents.FirstOrDefault(a => a.Profession == Profession.Woodcutter);
            Assert.IsNotNull(woodcutter);
            
            int initialProgress = plantation.ConstructionProgress;
            
            // Act - Set low stop threshold to force improvement
            ProducerThresholds.SetThresholds(
                Profession.Woodcutter,
                0.01f,
                0.05f);
            
            float simulationTime = 60f;
            float deltaTime = 0.1f;
            int steps = (int)(simulationTime / deltaTime);
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                
                if (plantation.IsConstructionPhaseComplete())
                {
                    break;
                }
            }
            
            // Assert
            int finalProgress = plantation.ConstructionProgress;
            
            Assert.Greater(finalProgress, initialProgress, 
                "With high logs stock and low stop threshold, agent should improve building");
        }
        
        [Test]
        public void BuildTime_AppliedAfterDelivery_BuildingNotCompleteUntilTimePasses()
        {
            // Arrange
            var world = new World();
            world.InitializeGameSetup();
            
            // Add logs to storage
            world.Storage.Deposit(ResourceKind.Logs, 200);
            world.Storage.Deposit(ResourceKind.Meat, 100);
            world.Storage.Deposit(ResourceKind.PlantFood, 100);
            
            // Add a house
            var house = world.AddBuilding(BuildingKind.House, new GridPos(45, 45));
            
            var builder = world.Agents.FirstOrDefault(a => a.Profession == Profession.Builder);
            Assert.IsNotNull(builder);
            
            // Manually deliver resources to test build-time gating
            builder.Position = new GridPos(46, 46); // At building site
            builder.CarriedResources = BuildingCostHelper.CivilBuildingBuildCost;
            ProductionSystem.DeliverResourcesToBuilding(builder, house, world.Clock);
            
            // Assert - Building should have progress but not be complete immediately
            Assert.AreEqual(BuildingCostHelper.CivilBuildingBuildCost, house.ConstructionProgress,
                "Progress should match delivered resources");
            Assert.IsFalse(house.IsConstructionPhaseComplete(), 
                "Building should not be complete immediately after delivery (build-time required)");
            
            // Act - Advance time past build-time
            // Adult productivity = 1.0, so 100 units = 100 seconds build time
            float buildTime = 100.1f;
            world.SimulationStep(buildTime);
            
            // Assert - Building should now be complete
            Assert.IsTrue(house.IsConstructionPhaseComplete(), 
                "Building should complete after build-time elapses");
        }
    }
}
