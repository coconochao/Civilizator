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
            var initialPosition = woodcutter.Position;
            
            // Act - Simulate enough time for full cycle
            // Move to tree (1 tile), gather (10 units at 1/sec), move to central (3 tiles), deposit (instant)
            // Move to central (from plantation area), withdraw, deliver
            // Total time needed: ~30 seconds for the full cycle
            
            float totalTime = 120f; // 2 minutes should be more than enough
            float deltaTime = 0.1f;
            int steps = (int)(totalTime / deltaTime);
            
            int stepsSinceLastLog = 0;
            int logInterval = 100; // Log every 100 steps
            
            bool agentMovedToTree = false;
            bool agentGathered = false;
            bool agentMovedToCentral = false;
            bool agentDeposited = false;
            bool agentMovedToBuilding = false;
            bool agentDelivered = false;
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                stepsSinceLastLog++;
                
                // Verification: Check if agent is moving towards tree
                if (!agentMovedToTree && woodcutter.Position == tree.Position)
                {
                    agentMovedToTree = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent reached tree at {tree.Position}");
                }
                
                // Verification: Check if agent gathered resources
                if (!agentGathered && woodcutter.CarriedResources > 0)
                {
                    agentGathered = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent gathered {woodcutter.CarriedResources} resources");
                }
                
                // Verification: Check if agent moved back to central
                var central = world.GetCentralBuilding();
                if (!agentMovedToCentral && central != null && woodcutter.Position.X >= central.Anchor.X && 
                    woodcutter.Position.X < central.Anchor.X + 2 && woodcutter.Position.Y >= central.Anchor.Y && 
                    woodcutter.Position.Y < central.Anchor.Y + 2)
                {
                    agentMovedToCentral = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent reached central storage");
                }
                
                // Verification: Check if resources were deposited
                int currentLogs = world.Storage.GetStock(ResourceKind.Logs);
                if (!agentDeposited && currentLogs > initialLogs)
                {
                    agentDeposited = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Resources deposited. Logs: {initialLogs} -> {currentLogs}");
                }
                
                // Verification: Check if agent moved to building
                if (!agentMovedToBuilding && woodcutter.Position.X >= plantation.Anchor.X && 
                    woodcutter.Position.X < plantation.Anchor.X + 2 && woodcutter.Position.Y >= plantation.Anchor.Y && 
                    woodcutter.Position.Y < plantation.Anchor.Y + 2)
                {
                    agentMovedToBuilding = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent reached building site");
                }
                
                // Verification: Check if resources were delivered to building
                if (!agentDelivered && plantation.ConstructionProgress > initialProgress)
                {
                    agentDelivered = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Resources delivered to building. Progress: {initialProgress} -> {plantation.ConstructionProgress}");
                }
                
                // Periodic logging for debugging
                if (stepsSinceLastLog >= logInterval)
                {
                    stepsSinceLastLog = 0;
                    var state = world.GetAgentActionState(woodcutter);
                    string pathInfo = state?.CurrentPath != null && state.CurrentPath.Count > 0 
                        ? $"Path to {state.CurrentPath[state.CurrentPath.Count - 1]} (step {state.PathIndex}/{state.CurrentPath.Count})"
                        : "No path";
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent at {woodcutter.Position}, Carried: {woodcutter.CarriedResources}, " +
                        $"{pathInfo}, Tree remaining: {tree.Remaining}, Storage logs: {currentLogs}, Building progress: {plantation.ConstructionProgress}");
                }
                
                // Stop if building is complete
                if (!plantation.IsUnderConstruction && plantation.IsConstructionPhaseComplete())
                {
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Building construction completed!");
                    break;
                }
            }
            
            // Assert
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            int finalProgress = plantation.ConstructionProgress;
            
            // Additional verification assertions
            TestContext.WriteLine($"=== Test Summary ===");
            TestContext.WriteLine($"Agent moved to tree: {agentMovedToTree}");
            TestContext.WriteLine($"Agent gathered resources: {agentGathered}");
            TestContext.WriteLine($"Agent moved to central: {agentMovedToCentral}");
            TestContext.WriteLine($"Agent deposited resources: {agentDeposited}");
            TestContext.WriteLine($"Agent moved to building: {agentMovedToBuilding}");
            TestContext.WriteLine($"Agent delivered resources: {agentDelivered}");
            TestContext.WriteLine($"Position changed: {initialPosition} -> {woodcutter.Position}");
            TestContext.WriteLine($"Logs: {initialLogs} -> {finalLogs}");
            TestContext.WriteLine($"Building progress: {initialProgress} -> {finalProgress}");
            TestContext.WriteLine($"Tree remaining: {tree.Remaining}");
            
            Assert.IsTrue(agentMovedToTree, "Agent should have moved to the tree");
            Assert.IsTrue(agentGathered, "Agent should have gathered resources from tree");
            Assert.IsTrue(agentMovedToCentral, "Agent should have moved to central storage");
            Assert.IsTrue(agentDeposited, "Agent should have deposited resources at central storage");
            
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
            var initialPosition = woodcutter.Position;
            
            // Act - Simulate gathering and depositing
            float timeToGatherAndDeposit = 30f;
            float deltaTime = 0.1f;
            int steps = (int)(timeToGatherAndDeposit / deltaTime);
            
            int stepsSinceLastLog = 0;
            int logInterval = 50;
            
            bool agentMovedToTree = false;
            bool agentGathered = false;
            bool agentMovedToCentral = false;
            bool agentDeposited = false;
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                stepsSinceLastLog++;
                
                // Verification: Check if agent moved to tree
                if (!agentMovedToTree && woodcutter.Position == tree.Position)
                {
                    agentMovedToTree = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent reached tree at {tree.Position}");
                }
                
                // Verification: Check if agent gathered resources
                if (!agentGathered && woodcutter.CarriedResources > 0)
                {
                    agentGathered = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent gathered {woodcutter.CarriedResources} resources");
                }
                
                // Verification: Check if agent moved to central
                var central = world.GetCentralBuilding();
                if (!agentMovedToCentral && central != null && woodcutter.Position.X >= central.Anchor.X && 
                    woodcutter.Position.X < central.Anchor.X + 2 && woodcutter.Position.Y >= central.Anchor.Y && 
                    woodcutter.Position.Y < central.Anchor.Y + 2)
                {
                    agentMovedToCentral = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent reached central storage");
                }
                
                // Verification: Check if resources were deposited
                int currentLogs = world.Storage.GetStock(ResourceKind.Logs);
                if (!agentDeposited && currentLogs > initialLogs)
                {
                    agentDeposited = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Resources deposited. Logs: {initialLogs} -> {currentLogs}");
                }
                
                // Periodic logging
                if (stepsSinceLastLog >= logInterval)
                {
                    stepsSinceLastLog = 0;
                    var state = world.GetAgentActionState(woodcutter);
                    string pathInfo = state?.CurrentPath != null && state.CurrentPath.Count > 0 
                        ? $"Path to {state.CurrentPath[state.CurrentPath.Count - 1]} (step {state.PathIndex}/{state.CurrentPath.Count})"
                        : "No path";
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Agent at {woodcutter.Position}, Carried: {woodcutter.CarriedResources}, " +
                        $"{pathInfo}, Tree remaining: {tree.Remaining}, Storage logs: {currentLogs}");
                }
                
                // Stop if agent has deposited at least once
                if (currentLogs > initialLogs)
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
            
            TestContext.WriteLine($"=== Test Summary ===");
            TestContext.WriteLine($"Agent moved to tree: {agentMovedToTree}");
            TestContext.WriteLine($"Agent gathered resources: {agentGathered}");
            TestContext.WriteLine($"Agent moved to central: {agentMovedToCentral}");
            TestContext.WriteLine($"Agent deposited resources: {agentDeposited}");
            TestContext.WriteLine($"Position changed: {initialPosition} -> {woodcutter.Position}");
            TestContext.WriteLine($"Logs: {initialLogs} -> {finalLogs}");
            TestContext.WriteLine($"Tree remaining: {initialNodeRemaining} -> {finalNodeRemaining}");
            
            Assert.IsTrue(agentMovedToTree, "Agent should have moved to the tree");
            Assert.IsTrue(agentGathered, "Agent should have gathered resources");
            Assert.IsTrue(agentMovedToCentral, "Agent should have moved to central storage");
            Assert.IsTrue(agentDeposited, "Agent should have deposited resources");
            
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
            var initialPosition = builder.Position;
            
            // Act - Simulate builder working
            float timeToComplete = 60f;
            float deltaTime = 0.1f;
            int steps = (int)(timeToComplete / deltaTime);
            
            int stepsSinceLastLog = 0;
            int logInterval = 50;
            
            bool agentMovedToCentral = false;
            bool agentWithdrew = false;
            bool agentMovedToBuilding = false;
            bool agentDelivered = false;
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                stepsSinceLastLog++;
                
                // Verification: Check if agent moved to central
                var central = world.GetCentralBuilding();
                if (!agentMovedToCentral && central != null && builder.Position.X >= central.Anchor.X && 
                    builder.Position.X < central.Anchor.X + 2 && builder.Position.Y >= central.Anchor.Y && 
                    builder.Position.Y < central.Anchor.Y + 2)
                {
                    agentMovedToCentral = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Builder reached central storage");
                }
                
                // Verification: Check if agent withdrew resources
                if (!agentWithdrew && builder.CarriedResources > 0)
                {
                    agentWithdrew = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Builder withdrew {builder.CarriedResources} resources");
                }
                
                // Verification: Check if agent moved to building
                if (!agentMovedToBuilding && builder.Position.X >= house.Anchor.X && 
                    builder.Position.X < house.Anchor.X + 2 && builder.Position.Y >= house.Anchor.Y && 
                    builder.Position.Y < house.Anchor.Y + 2)
                {
                    agentMovedToBuilding = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Builder reached building site");
                }
                
                // Verification: Check if resources were delivered to building
                int currentProgress = house.ConstructionProgress;
                if (!agentDelivered && currentProgress > initialProgress)
                {
                    agentDelivered = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Resources delivered to building. Progress: {initialProgress} -> {currentProgress}");
                }
                
                // Periodic logging
                if (stepsSinceLastLog >= logInterval)
                {
                    stepsSinceLastLog = 0;
                    var state = world.GetAgentActionState(builder);
                    string pathInfo = state?.CurrentPath != null && state.CurrentPath.Count > 0 
                        ? $"Path to {state.CurrentPath[state.CurrentPath.Count - 1]} (step {state.PathIndex}/{state.CurrentPath.Count})"
                        : "No path";
                    int currentLogs = world.Storage.GetStock(ResourceKind.Logs);
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Builder at {builder.Position}, Carried: {builder.CarriedResources}, " +
                        $"{pathInfo}, Storage logs: {currentLogs}, Building progress: {currentProgress}");
                }
                
                // Stop if building is complete
                if (!house.IsUnderConstruction && house.IsConstructionPhaseComplete())
                {
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Building construction completed!");
                    break;
                }
            }
            
            // Assert
            int finalProgress = house.ConstructionProgress;
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            
            TestContext.WriteLine($"=== Test Summary ===");
            TestContext.WriteLine($"Agent moved to central: {agentMovedToCentral}");
            TestContext.WriteLine($"Agent withdrew resources: {agentWithdrew}");
            TestContext.WriteLine($"Agent moved to building: {agentMovedToBuilding}");
            TestContext.WriteLine($"Agent delivered resources: {agentDelivered}");
            TestContext.WriteLine($"Position changed: {initialPosition} -> {builder.Position}");
            TestContext.WriteLine($"Logs: {initialLogs} -> {finalLogs}");
            TestContext.WriteLine($"Building progress: {initialProgress} -> {finalProgress}");
            
            Assert.IsTrue(agentMovedToCentral, "Builder should have moved to central storage");
            Assert.IsTrue(agentWithdrew, "Builder should have withdrawn resources");
            Assert.IsTrue(agentMovedToBuilding, "Builder should have moved to building site");
            Assert.IsTrue(agentDelivered, "Builder should have delivered resources to building");
            
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
            var initialPosition = woodcutter.Position;
            int initialLogs = world.Storage.GetStock(ResourceKind.Logs);
            
            // Act - Set low stop threshold to force improvement
            ProducerThresholds.SetThresholds(
                Profession.Woodcutter,
                0.01f,
                0.05f);
            
            float simulationTime = 60f;
            float deltaTime = 0.1f;
            int steps = (int)(simulationTime / deltaTime);
            
            int stepsSinceLastLog = 0;
            int logInterval = 50;
            
            bool agentMovedToCentral = false;
            bool agentWithdrew = false;
            bool agentMovedToBuilding = false;
            bool agentDelivered = false;
            
            for (int i = 0; i < steps; i++)
            {
                world.SimulationStep(deltaTime);
                stepsSinceLastLog++;
                
                // Verification: Check if agent moved to central (to withdraw)
                var central = world.GetCentralBuilding();
                if (!agentMovedToCentral && central != null && woodcutter.Position.X >= central.Anchor.X && 
                    woodcutter.Position.X < central.Anchor.X + 2 && woodcutter.Position.Y >= central.Anchor.Y && 
                    woodcutter.Position.Y < central.Anchor.Y + 2)
                {
                    agentMovedToCentral = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Woodcutter reached central storage");
                }
                
                // Verification: Check if agent withdrew resources
                if (!agentWithdrew && woodcutter.CarriedResources > 0)
                {
                    agentWithdrew = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Woodcutter withdrew {woodcutter.CarriedResources} resources");
                }
                
                // Verification: Check if agent moved to building
                if (!agentMovedToBuilding && woodcutter.Position.X >= plantation.Anchor.X && 
                    woodcutter.Position.X < plantation.Anchor.X + 2 && woodcutter.Position.Y >= plantation.Anchor.Y && 
                    woodcutter.Position.Y < plantation.Anchor.Y + 2)
                {
                    agentMovedToBuilding = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Woodcutter reached building site");
                }
                
                // Verification: Check if resources were delivered to building
                int currentProgress = plantation.ConstructionProgress;
                if (!agentDelivered && currentProgress > initialProgress)
                {
                    agentDelivered = true;
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Resources delivered to building. Progress: {initialProgress} -> {currentProgress}");
                }
                
                // Periodic logging
                if (stepsSinceLastLog >= logInterval)
                {
                    stepsSinceLastLog = 0;
                    var state = world.GetAgentActionState(woodcutter);
                    string pathInfo = state?.CurrentPath != null && state.CurrentPath.Count > 0 
                        ? $"Path to {state.CurrentPath[state.CurrentPath.Count - 1]} (step {state.PathIndex}/{state.CurrentPath.Count})"
                        : "No path";
                    string targetInfo = state?.CurrentTargetBuilding != null 
                        ? $"Target: {state.CurrentTargetBuilding.Kind}"
                        : "No target";
                    int currentLogs = world.Storage.GetStock(ResourceKind.Logs);
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Woodcutter at {woodcutter.Position}, Carried: {woodcutter.CarriedResources}, " +
                        $"{pathInfo}, {targetInfo}, Storage logs: {currentLogs}, Building progress: {currentProgress}");
                }
                
                if (plantation.IsConstructionPhaseComplete())
                {
                    TestContext.WriteLine($"[{i * deltaTime:F1}s] Building construction completed!");
                    break;
                }
            }
            
            // Assert
            int finalProgress = plantation.ConstructionProgress;
            int finalLogs = world.Storage.GetStock(ResourceKind.Logs);
            
            TestContext.WriteLine($"=== Test Summary ===");
            TestContext.WriteLine($"Agent moved to central: {agentMovedToCentral}");
            TestContext.WriteLine($"Agent withdrew resources: {agentWithdrew}");
            TestContext.WriteLine($"Agent moved to building: {agentMovedToBuilding}");
            TestContext.WriteLine($"Agent delivered resources: {agentDelivered}");
            TestContext.WriteLine($"Position changed: {initialPosition} -> {woodcutter.Position}");
            TestContext.WriteLine($"Logs: {initialLogs} -> {finalLogs}");
            TestContext.WriteLine($"Building progress: {initialProgress} -> {finalProgress}");
            
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
            
            TestContext.WriteLine($"Initial state: House at {house.Anchor}, Progress: {house.ConstructionProgress}, Under construction: {house.IsUnderConstruction}");
            
            // Manually deliver resources to test build-time gating
            builder.Position = new GridPos(46, 46); // At building site
            builder.CarriedResources = BuildingCostHelper.CivilBuildingBuildCost;
            
            TestContext.WriteLine($"Delivering {BuildingCostHelper.CivilBuildingBuildCost} resources to house");
            ProductionSystem.DeliverResourcesToBuilding(builder, house, world.Clock);
            
            TestContext.WriteLine($"After delivery: House Progress: {house.ConstructionProgress}, Under construction: {house.IsUnderConstruction}, Complete: {house.IsConstructionPhaseComplete()}");
            
            // Assert - Building should have progress but not be complete immediately
            Assert.AreEqual(BuildingCostHelper.CivilBuildingBuildCost, house.ConstructionProgress,
                "Progress should match delivered resources");
            Assert.IsFalse(house.IsConstructionPhaseComplete(), 
                "Building should not be complete immediately after delivery (build-time required)");
            
            // Act - Advance time past build-time
            // Adult productivity = 1.0, so 100 units = 100 seconds build time
            float buildTime = 100.1f;
            
            // Log progress during build time
            float checkInterval = 20f;
            float elapsed = 0f;
            while (elapsed < buildTime)
            {
                float step = System.Math.Min(checkInterval, buildTime - elapsed);
                world.SimulationStep(step);
                elapsed += step;
                TestContext.WriteLine($"[{elapsed:F1}s] House Progress: {house.ConstructionProgress}, Complete: {house.IsConstructionPhaseComplete()}");
            }
            
            // Assert - Building should now be complete
            TestContext.WriteLine($"Final state: House Progress: {house.ConstructionProgress}, Complete: {house.IsConstructionPhaseComplete()}");
            Assert.IsTrue(house.IsConstructionPhaseComplete(), 
                "Building should complete after build-time elapses");
        }
    }
}
