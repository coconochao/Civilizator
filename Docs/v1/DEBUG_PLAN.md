# Debug Plan — ConstructionVerticalSliceTests Failures

## Status Legend
`[PENDING]` `[IN_PROGRESS]` `[DONE]`

---

## Root Cause Summary

**Production code bugs:**
1. `World.AddBuilding` never sets `IsUnderConstruction = true` for non-Central buildings, so builders never find houses to build.
2. `World.AddBuilding` never assigns `Building.SimulationClock`, so build-time gating never fires.
3. `ProductionSystem.IsAtCentralStorage` hard-codes zone `[49,51]`, but actual Central footprint is `[48,50]` (anchor `48,48`, 3×3). Agents path to `48,48` and then cannot deposit or withdraw.

**Test code bugs:**
4. `ThresholdControl_ForcesImprovement_WhenStockHigh` passes stop threshold `0.8f` with stock `500/1000 = 0.5`. `0.5 >= 0.8` is false, so the agent never switches to improvement.
5. `BuildTime_AppliedAfterDelivery_BuildingNotCompleteUntilTimePasses` delivers only `10` resources but civil buildings require `100`. It then asserts the building completes after 10 s, which is impossible.

---

## Task 1 — Fix `World.AddBuilding`
**Status:** `[PENDING]`
**File:** `Assets/Civilizator/Simulation/World.cs`
**Change:** After creating the building, add:
```csharp
if (kind != BuildingKind.Central)
{
    building.IsUnderConstruction = true;
    building.SimulationClock = Clock;
}
```
**Verification:** Ask the user to run the tests. Builder/house and gather/construct tests should begin progressing.

## Task 2 — Fix `ProductionSystem.IsAtCentralStorage`
**Status:** `[PENDING]`
**File:** `Assets/Civilizator/Simulation/ProductionSystem.cs`
**Change:** Replace the hard-coded `49..51` check with the real Central footprint `48..50`:
```csharp
return agent.Position.X >= 48 && agent.Position.X <= 50 &&
       agent.Position.Y >= 48 && agent.Position.Y <= 50;
```
*(Better long-term: accept a `Building` or `GridPos` parameter so it is not fragile.)*
**Verification:** Ask the user to run the tests. `ProducerAgent_GathersFromNode_*` and `BuilderAgent_WithdrawsFromCentral_*` should now show logs moving.

## Task 3 — Fix test threshold values
**Status:** `[PENDING]`
**File:** `Assets/Civilizator/Tests/Simulation/ConstructionVerticalSliceTests.cs`
**Change:** Change the `SetThresholds` call to use a stop threshold below `0.5`, e.g.:
```csharp
ProducerThresholds.SetThresholds(Profession.Woodcutter, 0.01f, 0.05f);
```
**Verification:** Ask the user to run the `ThresholdControl_*` test; progress should become > 0.

## Task 4 — Fix test resource amount / expectation
**Status:** `[PENDING]`
**File:** `Assets/Civilizator/Tests/Simulation/ConstructionVerticalSliceTests.cs`
**Change:** In `BuildTime_*`, change `builder.CarriedResources = 10;` to `builder.CarriedResources = BuildingCostHelper.CivilBuildingBuildCost;` (100). Update the first `Assert.AreEqual(10, …)` to `100`.
**Verification:** Ask the user to run the `BuildTime_*` test; it should pass.

## Task 5 — Full regression run
**Status:** `[PENDING]`
**Action:** Ask the user to run the entire `ConstructionVerticalSliceTests` fixture. All five tests should pass. If any still fail, the assigned agent should capture the exact failure message and append it to this plan before proceeding.
