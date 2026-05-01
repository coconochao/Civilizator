# Civilizator V1 Handoff

This note is the quick-start for opening the current V1 build in Unity.

## How to run

1. Open the project in Unity **6000.4.1f1**.
2. Open [`Assets/Scenes/SampleScene.unity`](/Users/caioneves/Civilizator/Assets/Scenes/SampleScene.unity).
3. Press **Play**.

The scene boots the current vertical slice automatically:

- `SimulationTickDriver` initializes the simulation and advances it every frame.
- `WorldPlaceholderView` creates the grid, natural nodes, buildings, and starter agents.
- The main camera is set up as the isometric presentation camera for the map.
- The HUD and control surface are created at runtime if the scene does not already contain them.

## Player controls

The V1 control surface is policy-based rather than direct unit orders:

- Adjust profession targets for Woodcutter, Miner, Hunter, Farmer, Builder, and Soldier.
- Adjust reproduction rate.
- Adjust the producer start/stop thresholds for Woodcutter, Miner, Hunter, and Farmer.
- Adjust soldier patrol vs improvement split, plus tower-building emphasis.

Camera input uses the Input System actions in the `Camera` map:

- `Pan`
- `Zoom`

The runtime HUD exposes the policy controls as sliders in the left-side panel. If you open the scene in Edit mode, those controls are not part of the authored hierarchy; they appear when the scene enters Play mode.

## What the player should see

- Central storage counts for Logs, Ore, Meat, and Plant food.
- Production-rate, profession-allocation, population, housing, activity, and productivity HUDs.
- A generated 100x100 world with region grid markers, natural nodes, starter buildings, and starter agents.

## Known SPEC gaps

None known for the implemented V1 checklist.

If a future task exposes a mismatch with [`SPEC.md`](/Users/caioneves/Civilizator/Docs/v1/SPEC.md), document it here explicitly instead of leaving it implicit.
