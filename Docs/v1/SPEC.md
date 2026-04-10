# Civilizator — V1 Specification (Locked)

This document fully specifies **V1** of the game simulation. It is intended to be sufficient for an AI agent (or developer) to produce a detailed development plan and implement the vertical slice **without making additional design decisions**.

## Core premise

- The game is an **isometric-view** simulation of a small civilization.
- The player does **not** directly command units.
- The player can:
  - move the camera, and
  - change **control parameters** that influence how citizens behave.

## V1 scope constraints

- **Population scale**: tens of people (not hundreds/thousands).
- **Movement is real**: agents physically move on the map using grid/pathfinding.
- **Resources are global numbers in the central building** (no per-item physical hauling), but agents still move to gather and deposit.
- **No monetary economy** in V1.
- **No roads** in V1 (even though roads were considered).
- **Minimal UI**: only must-have feedback panels/metrics (defined below).
- **Story**: none; enemies spawn periodically.

## World & map

### Grid

- Map is a **tile grid**.
- Map size: **100 × 100** tiles.
- Movement: **4-way** (N/E/S/W only).
- Distance metric for "nearest" and radii: **Manhattan distance**.
  - Note: Manhattan distance is used here as a simple, grid-friendly metric. It is **not a design pillar** of V1; using it is primarily a convenience for implementers.
- Tile occupancy: **multiple units can occupy the same tile**.

### Pathfinding (implementation freedom)

- Pathfinding can be the **simplest possible** approach that produces believable agent movement on a grid.
- Manhattan distance being specified above is **arbitrary** (chosen for simplicity); implementations may use any simple nearest/range logic as long as behavior remains consistent and readable.
- Recommended baseline for V1: a basic grid search (e.g., BFS / A* on a 4-way grid) with no special costs.

### Build rules

- Buildings may be placed on **any empty tiles**.
- Buildings **must not overlap**.
- When choosing a placement tile for a new building, use:
  - at least **1 tile away from other buildings** (no adjacent footprints).
- **Resource facilities** (Plantation, Farm, Cattle farm, Quarry): In addition to the rules above, placement is valid only if **at least one tile** of the building’s footprint overlaps an existing **natural node** of the matching type:
  - **Plantation** → **Tree** node
  - **Farm** → **Plant** node
  - **Cattle farm** → **Animal** node
  - **Quarry** → **Ore** node

### Resource node generation (natural)

The world contains natural resource nodes on tiles. Nodes do not move.

- Each node has:
  - a **type** (Tree / Plant / Animal / Ore), and
  - a **remaining amount** (integer), starting at **100**.
- Nodes are **finite** and **deplete** when gathered from; once depleted, they are unavailable permanently.
- Natural nodes **do not respawn** in V1 (only facility-spawned resources appear after improvements).
- Node placement:
  - Divide the map into **10 × 10 regions**.
  - In each region, place **exactly one node of each type**: Tree, Plant, Animal, Ore.
  - No two nodes may occupy the same tile.

## Time & simulation cadence

- Time is discretized into **cycles**.
- **1 cycle = 1 minute** of simulated time.
- Some actions are expressed in **seconds** (e.g., gathering, attacking, eating). These are real-time simulation seconds within the minute/cycle.

## Unity implementation guidelines (non-design requirements)

This section records implementation expectations for the Unity project. These do **not** add gameplay design requirements beyond what is already specified above; they exist to reduce rework and ensure V1 remains readable and debuggable.

### Camera (isometric readability)

- Use an **orthographic** camera for the default isometric view.
- Camera controls required for V1:
  - **Pan** (move across the map)
  - **Zoom** (in/out)
- The game is hands-off: do **not** rely on click-to-command interactions as core input.

### Input system & UI

- Use Unity's **Input System** package for input.
- Ensure UI is driven by the Input System (e.g., Input System UI module), not legacy input, to avoid mismatches between gameplay and UI input.

### Project organization (folder/assembly hygiene)

- Keep simulation logic and Unity presentation separated so the core rules are testable and not frame-rate dependent.
  - Example split: `Simulation/`, `Presentation/`, `UI/`, `Input/`, `Pathfinding/`, `Config/`.
- Prefer `.asmdef` files to keep compile times manageable as systems grow.

### Simulation clock (determinism & debugging)

- Implement a central **simulation clock** that advances cycles and supports second-based actions.
- Do not tie simulation correctness to frame rate; the simulation should remain stable across varying FPS.

## Resources & storage

### Resource types (V1)

- **Logs** (from trees and plantations)
- **Ore** (from mines and quarries)
- **Meat** (from animals and cattle farms)
- **Plant food** (from plants and farms)

### Central building storage

- There is a single **central building**.
- All global resources are stored in the central building as **numbers**.
- Depositing at the central building is **instant** on arrival.

## Buildings (footprints, costs, upgrades)

### Footprints

- Central building: **3 × 3**
- Most buildings (houses, profession facilities, towers, etc.): **2 × 2**
- (Roads are out of scope for V1.)

### Construction progress model (all buildings/upgrades)

- When a building is under construction or being upgraded, it tracks **progress = total delivered build resources** (integer).
- A building/upgrade is **complete** when:
  - the required amount has been delivered, and
  - the worker who delivered the final amount finishes their build-time work (see "Construction time").

### Construction time

When a worker delivers build resources to a construction site:

- They then spend time building: **1 second per delivered resource unit**, adjusted by productivity:
  - build time \(seconds\) = delivered_units × (1 / productivity_multiplier)
- Example: adult (100%) delivers 10 wood → 10 seconds build work.
- Multiple workers may work on the same construction simultaneously (tile multi-occupancy).

### Costs

- Civil building construction cost: **100 Logs**
  - Applies to: houses, plantations, farms, cattle farms, quarries, towers (unless overridden below).
- Civil building upgrade cost: **100 Logs**
- Tower construction cost: **100 Ore**
- Tower upgrade cost: **100 Ore**

### Upgrade limits and effects

- Any upgradable building can be upgraded **at most once** in V1.
- Upgrade effects:
  - **Plantation / Farm / Cattle farm**: upgrade **doubles spawn rate**.
  - **Quarry**: upgrade **doubles ore production rate** (see quarry rules below).
  - **Tower**: upgrade **doubles tower damage** (from 1 to 2 per attack).

## Facilities & spawn rules (improvements)

General rule: facilities **do not store resources** in V1.

### Plantation (wood)

- Footprint: 2×2.
- **Placement**: Must overlap at least one **Tree** natural node.
- Once built, it **spawns Logs** in its own 2×2 area:
  - Base: once per cycle, each tile in the 2×2 area spawns **1 Log** if that tile has no uncollected spawned log already.
  - Upgraded: spawns **2 Logs per cycle per tile** (equivalently doubles spawn rate; implement as two spawn attempts).

### Farm (plant food)

- Footprint: 2×2.
- **Placement**: Must overlap at least one **Plant** natural node.
- Once built, it **spawns Plant food** in its own 2×2 area:
  - Base: once per cycle, each tile spawns **1 Plant food** if not already present.
  - Upgraded: doubles spawn rate (same behavior as plantation).

### Cattle farm (meat)

- Footprint: 2×2.
- **Placement**: Must overlap at least one **Animal** natural node.
- Once built, it **spawns Meat** in its own 2×2 area:
  - Base: once per cycle, each tile spawns **1 Meat** if not already present.
  - Upgraded: doubles spawn rate (same behavior as plantation).

### Quarry (ore)

Quarry changes how ore depletion works.

- Footprint: 2×2.
- **Placement**: Must overlap at least one **Ore** natural node.
- Natural ore nodes:
  - Without quarry support, they deplete after 100 ore gathered (node remaining reaches 0).
  - With a quarry built, **ore can be collected indefinitely** (beyond depletion).
- A quarry **does not spawn ore**. A worker must still go to an ore location and gather.
- Base quarry effect:
  - Allows indefinite ore collection at an effective rate **half** that of a normal worker gathering loop.
  - (Implementation intent: gathering time is doubled while quarry-enabled ore is being gathered.)
- Upgraded quarry effect:
  - Doubles production rate relative to base quarry (i.e. upgraded quarry returns to **normal** worker gathering rate).

## Agents (citizens)

### Life stages and aging

Life stages: Child → Adult → Elder → Death.

- Child to Adult: **10 cycles**
- Adult to Elder: **60 cycles**
- Elder to Death: **10 cycles**

### Productivity

Productivity is a multiplier that affects:

- Carry capacity (directly)
- Action durations (inversely)

Base productivity:

- Adult: **100%** (multiplier 1.0)
- Child: **50%** (multiplier 0.5)
- Elder: **50%** (multiplier 0.5)

Modifiers (stack additively unless otherwise specified):

- Assigned to a house: **+20%**
- Starvation: **-25%** each cycle they fail to eat (additive)
  - Death occurs when productivity reaches **0%**.

### Carrying

- Carry capacity = **10 × productivity_multiplier**
  - Adult at 100% → 10 units
  - Child/Elder at 50% → 5 units

### Eating & starvation

- Each person must eat **once per cycle**.
- One unit of food is either:
  - 1 Meat, or
  - 1 Plant food
  - (Equivalent nutrition in V1.)
- Eating behavior:
  - Travel to central building (if not already there).
  - Spend **1 second** eating.
  - Consume 1 food from central stock.
- If no food is available at eating time:
  - Apply starvation penalty: productivity **-25%** (additive).
  - Continue normal behavior with reduced productivity.
  - If productivity reaches **0%**, the agent dies.

### Reproduction

- There is a global **reproduction rate control** (player-controlled).
- Breeding rule:
  - Periodically, two people assigned to the **same house** can generate a child.
  - (Exact cadence/probability is controlled by the reproduction rate parameter; implementers should treat it as a per-cycle probability.)

### Housing and assignment

Houses provide the "assigned to a house" productivity bonus.

- House footprint: 2×2.
- **Placement**: **nearest empty tile to the central building**.
- House capacity:
  - **2 Adults/Elders** maximum
  - **Unlimited children**
- House assignment rules:
  - When a builder finishes building a house, randomly assign **2 Adults** to that house.
  - When an assigned Adult/Elder dies, assign a **random Adult** to fill that vacancy.
  - When a Child becomes an Adult, assign them randomly to an **available house** (one with an adult slot open).

### Professions (V1 list)

- Woodcutter
- Miner
- Hunter
- Farmer
- Builder
- Soldier

### Choosing and switching professions

Profession targets are controlled by player-set percentages.

- On spawn (new adult/child), an agent chooses an **undernumbered** profession (relative to the targets).
- Periodic profession correction:
  - Compute discrepancies between target % and actual %.
  - If discrepancy magnitude is above a threshold:
    - pick an agent from the **most overrepresented** profession,
    - switch them to the **most underrepresented** profession.
  - Switches happen **one agent at a time**.
- There is a **cooldown** on profession switching.
  - (Exact cooldown duration is a tuning constant in implementation; V1 requires that a cooldown exists.)

## Common action loops

All professions follow a repeating loop:

1. Evaluate policy thresholds.
2. Choose between:
   - **Production loop**, or
   - **Improvement loop**
3. Execute loop.
4. Repeat.

Exceptions are explicitly defined (e.g. builders have no production loop).

### Production loop (generic)

- Find the **nearest available node** relevant to the profession.
  - Nearest uses Manhattan distance.
  - Gathering requires the agent to be on the **same tile** as the node/resource.
- Gather at a base rate of **1 resource per second** (all node types).
  - Action time scales inversely with productivity.
  - Continue until:
    - carry capacity is full, or
    - the node/resource is depleted/empty.
- Travel to central building and deposit instantly.

If no relevant natural node/resource is available:

- The agent switches to the **improvement loop**.

### Improvement loop (generic)

- Travel to central building.
- Withdraw build resources from central (up to carry capacity and up to remaining required progress).
- Travel to target construction site.
- Deliver resources (increase construction progress).
- Spend build-time: **1 second per delivered unit**, scaled by productivity.
- Repeat until no longer instructed to improve or no improvement targets exist.

## Profession-specific behavior

### Woodcutter

- Production:
  - Target: nearest Tree node.
  - Action: gather Logs, bring to central.
- Improvement:
  - Build/upgrade a **Plantation**.

### Miner

- Production:
  - Target: nearest Ore node.
  - Action: gather Ore, bring to central.
- Improvement:
  - Build/upgrade a **Quarry**.

### Hunter

- Production:
  - Target: nearest Animal node.
  - Action: gather Meat, bring to central.
- Improvement:
  - Build/upgrade a **Cattle farm**.

### Farmer

- Production:
  - Target: nearest Plant node.
  - Action: gather Plant food, bring to central.
- Improvement:
  - Build/upgrade a **Farm**.

### Builder

Builders have **no production loop**. They only improve/build.

Builder priority decision:

- For each profession that produces resources (Woodcutter, Miner, Hunter, Farmer), compute:
  - **score = target_worker_share / current_resource_amount**
    - target_worker_share is the player-set percentage for that profession (as a fraction).
    - current_resource_amount is the current stock in the central building of that profession’s main resource.
- Builders select the profession with the **highest score** and attempt to help that profession by:
  - finding the **nearest building of that profession** (facility) that can be upgraded or is under construction,
  - performing an improvement loop to build/upgrade it.
- Builders **do not start** building other professions’ buildings.
  - (They can contribute to ongoing constructions/upgrades if such a site exists and is selected via the rule above.)
- If no actions are available for the top-scoring profession, builders try the next-highest score, etc.

Builders also build housing:

- Houses are built via the same improvement system and cost rules (100 Logs).
- Builders prioritize building houses if there are adults with no house assigned.

### Soldier

Soldiers have two modes in V1:

- **Patrolling**
- **Improving** (building defensive structures)

Training/barracks are out of scope for V1.

Patrol distribution:

- Define the built area radius as:
  - **radius = max Manhattan distance from central to any building tile**.
- Patrolling soldiers should occupy standing positions **evenly distributed** around the built area.
  - (Implementation simplification: sample points on the perimeter of the radius diamond and assign soldiers round-robin.)

Tower operation:

- A tower requires a soldier to be **inside** to operate.
- Patrolling soldiers prioritize operating a **vacant tower** (one without a soldier inside).

Mode switching:

- There is a player-controlled percentage allocation for soldiers between patrolling and improving.
- Soldiers periodically check whether they should change function, similar to profession discrepancy checks (one-at-a-time switching, with cooldown).

Combat behavior:

- Patrolling soldiers:
  - attack nearby enemies when in range (melee assumed unless otherwise implemented),
  - otherwise hold position / move to assigned patrol position.

Improvement behavior:

- Build/upgrade defensive structures using ore costs:
  - Towers: 100 Ore build, 100 Ore upgrade
  - (Decision rules for what to build are player-controlled; V1 requires a control for tower building/upgrade emphasis.)

## Defensive buildings

### Tower

- Footprint: 2×2.
- **Placement**: In the current location of patroling soldier, that is the furthest away from other existing towers.
- HP: **100**
- Damage:
  - Base: **1**
  - Upgraded: **2**
- Attack cadence: **1 attack per second**
- Range:
  - Range is 2 tiles in each direction, interpreted as a **6×6 area** with the 2×2 tower footprint centered.
  - (Implementation: treat any enemy whose tile coordinate falls within this rectangle as in range.)
- Requires a soldier inside to fire.
- Does not consume resources to fire.

## Enemies

### Spawning

- Enemy spawns occur periodically:
  - First spawn: **1 enemy after 10 cycles**
  - Then: **+1 additional enemy every 10 cycles** (cumulative)
- Spawn locations: **map edges**.

### Movement and targeting

- Enemies use the same **4-way pathing** rules as citizens.
- Target priority (in order):
  1. nearest person that is attacking them, else
  2. nearest tower that is attacking them, else
  3. nearest civilian/building

### Combat stats (V1 defaults)

- HP:
  - Civilians: 10
  - Soldiers: 10
  - Enemies: 10
  - Towers: 100
- Damage:
  - Soldiers: 1 per attack
  - Enemies: 1 per attack
  - Towers: 1 per attack (2 if upgraded)
- Attack cadence: 1 attack per second.
- Attacks cannot miss.

## Lose conditions

Game over occurs if:

- the central building is destroyed, or
- everyone dies.

## Player controls (V1)

### Global controls

- **Profession distribution targets (%)** for:
  - Woodcutter, Miner, Hunter, Farmer, Builder, Soldier
- **Reproduction rate** (probability/strength of breeding)

### Profession controls

For each of: Woodcutter, Miner, Hunter, Farmer

- Two thresholds (hysteresis):
  - **Start producing below** stock threshold
  - **Stop producing above** stock threshold

Builders:

- Builder uses the fixed smart scoring rule defined above (score = target_share / resource_amount).

Soldiers:

- Percentage split between:
  - Patrolling
  - Improving
- Control(s) that determine what is built during improving:
  - tower building/upgrade emphasis (exact UI form is implementation-defined).

## Required feedback / UI (must-haves)

V1 must expose at least the following to the player:

- Central stock amounts:
  - Logs, Ore, Meat, Plant food
- Resource production rates (per cycle or per minute):
  - overall, and optionally per profession
- Profession allocation:
  - target % and actual % per profession
- Population breakdown:
  - # children, # adults, # elders
- Housing coverage:
  - # people with a house assignment
  - # people without a house assignment
- Per-profession activity breakdown:
  - for each profession, how many are currently:
    - producing
    - improving/building
    - (and for soldiers: patrolling vs improving; and how many towers are staffed)
- Productivity:
  - average productivity, and/or distribution by life stage
  - starvation status indicators (at least counts)

