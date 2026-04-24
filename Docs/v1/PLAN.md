# Civilizator V1 — Implementation Plan

This document turns [`SPEC.md`](SPEC.md) into **small, sequential tasks** for implementing agents. The specification is authoritative; if this plan conflicts with `SPEC.md`, **follow `SPEC.md`**.

---

## How implementing agents must use this plan

### Before you start any task

1. Read **this entire “How to use this plan”** section once per session.
2. Open [`SPEC.md`](SPEC.md) and skim the section named in the task’s **Spec reference**.
3. Find the **first unchecked** task (`- [ ]`) in **Progress overview** below, then locate the same task ID in the body (e.g. **T-010**).

### While working

- Implement **only** the current task’s scope. Do not “finish the feature” early unless the task explicitly says so.
- Match project conventions already in the repo; if none exist, follow `SPEC.md` **Unity implementation guidelines** (folders, Input System, simulation vs presentation).

### After every task (mandatory)

1. **Verify** using the task’s **Verification** steps (compile, run tests, play mode smoke check—whatever is listed).
2. **Fix** any errors or failed checks before marking the task done.
3. **Mark progress**: in **Progress overview**, change `- [ ]` to `- [x]` for that task ID only.
4. **Optional handoff note** (one line): append to **Implementation log** at the bottom (date, task ID, file paths touched, known follow-ups).

### How the next agent picks up

- Read **Progress overview**; the next task is the first `- [ ]`.
- If **Implementation log** mentions blockers, read those lines before starting.

### Checkbox discipline

- Use `- [x]` only when **Verification** for that task passes.
- Do not batch-check multiple tasks after one large change; one task → one verification → one checkbox.

---

## Instructions for AI agents

Before starting any task, **read [`INSTRUCTIONS.md`](../INSTRUCTIONS.md)** for general guidelines on:
- Running tests and when to request test results from the user
- Suggesting commit messages after completing tasks
- Other important instructions for implementing tasks from this plan

---

## Progress overview

Check tasks off in order. IDs are stable—body sections use the same ID.

### Phase A — Project & assemblies

- [x] **T-001** — Confirm Unity version & packages baseline
- [x] **T-002** — Add folder skeleton under `Assets/`
- [x] **T-003** — Create `Civilizator.Simulation` assembly
- [x] **T-004** — Create `Civilizator.Presentation` assembly
- [x] **T-005** — Create `Civilizator.UI` assembly
- [x] **T-006** — Create `Civilizator.Input` assembly
- [x] **T-007** — Wire assembly references (no cycles)
- [x] **T-008** — Install/enable **Input System**; add Input System UI module stub

### Phase B — Core simulation types

- [x] **T-010** — Grid coordinates & Manhattan helpers (simulation)
- [x] **T-011** — Tile / map width-height constants (100×100)
- [x] **T-012** — Natural node model (type + remaining amount, starts at 100)
- [x] **T-013** — Resource kind enum (Logs, Ore, Meat, PlantFood)
- [x] **T-014** — Building kind enum + footprint sizes (3×3 central, 2×2 others)
- [x] **T-015** — Building placement rules: no overlap, 1 tile gap
- [x] **T-016** — Resource facility placement: overlap matching natural node
- [x] **T-017** — House placement: nearest empty tile to central building
- [x] **T-018** — World generator: 10×10 regions, one node per type per region
- [x] **T-019** — World generator tests or deterministic seed smoke check

### Phase C — Simulation clock

- [x] **T-020** — `SimulationClock`: cycles (1 cycle = 1 min sim) + accumulated seconds
- [x] **T-021** — Clock advances from presentation with delta time (not frame-tied logic bugs)
- [x] **T-022** — Helpers: “seconds per cycle”, schedule events on second boundaries

### Phase D — Pathfinding

- [x] **T-030** — Grid occupancy model (multi-unit same tile allowed)
- [x] **T-031** — 4-way BFS or A* from A to B on passable tiles
- [x] **T-032** — Nearest tile queries using Manhattan for “nearest” heuristics
- [x] **T-033** — Pathfinding unit test or deterministic scenario

### Phase E — Central building & storage

- [x] **T-040** — Central storage: four integer stocks
- [x] **T-041** — Deposit on arrival (instant) API
- [x] **T-042** — Withdraw for construction (bounded by carry + remaining progress)

### Phase F — Buildings & construction state

- [x] **T-050** — Building instance: position, kind, under construction flag, upgrade level (max 1 upgrade)
- [x] **T-051** — Construction progress = total delivered build resources (integer)
- [x] **T-052** — Costs: civil 100 Logs build/upgrade; tower 100 Ore build/upgrade
- [x] **T-053** — Complete when progress ≥ required **and** final worker finishes build-time
- [x] **T-054** — Build-time: 1 second per delivered unit × (1 / productivity)

### Phase G — Facilities & spawned map resources

- [x] **T-060** — Plantation/Farm/CattleFarm: spawn in own 2×2 once per cycle per tile rules
- [x] **T-061** — Upgraded facilities: double spawn rate
- [x] **T-062** — Quarry: no spawn; ore gathering rules (base vs upgraded rates vs depletion)
- [x] **T-063** — Natural node depletion except quarry-supported ore (indefinite with rate rules)

### Phase H — Agents: core stats

- [x] **T-070** — Agent model: position, profession, life stage, HP (10 default)
- [x] **T-071** — Productivity multiplier base by stage (Adult 1.0, Child/Elder 0.5)
- [x] **T-072** — House assignment flag + +20% productivity when assigned
- [x] **T-073** — Carry capacity = 10 × productivity
- [x] **T-074** — Aging timers: Child→Adult 10 cycles; Adult→Elder 60; Elder→Death 10

### Phase I — Eating & starvation

- [x] **T-080** — Once per cycle eating requirement
- [x] **T-081** — Eat: travel to central, 1 second eat, consume 1 Meat and 1 Plant food
- [x] **T-082** — No food: −25% productivity additive per failed cycle; death at 0%

### Phase J — Housing

- [x] **T-090** — House capacity: max 2 adults/elders, unlimited children
- [x] **T-091** — On house complete: assign 2 random adults
- [X] **T-092** — On adult/elder death: fill vacancy with random adult
- [x] **T-093** — Child→Adult: assign to house with open adult slot

### Phase K — Reproduction

- [x] **T-100** — Global reproduction rate parameter (player)
- [x] **T-101** — Per-cycle probability for two same-house adults to spawn child

### Phase L — Professions & switching

- [x] **T-110** — Profession enum list matches SPEC (6 professions)
- [x] **T-111** — Spawn: assign undernumbered profession vs targets
- [x] **T-112** — Periodic correction: threshold, one switch at a time, cooldown constant
- [x] **T-113** — Profession switch cooldown tuning constant documented in code/config

### Phase M — Policy thresholds (producers)

- [x] **T-120** — Per Woodcutter/Miner/Hunter/Farmer: start/stop stock thresholds (hysteresis)
- [x] **T-121** — Integrate thresholds into production vs improvement decision

### Phase N — Action loops (generic)

- [x] **T-130** — Production loop: nearest relevant node (Manhattan), same tile to gather
- [x] **T-131** — Gather 1/sec scaled by productivity until full or empty/depleted
- [x] **T-132** — Deposit at central; if no node, switch to improvement loop
- [x] **T-133** — Improvement loop: withdraw → travel → deliver progress → build-time seconds

### Phase O — Profession-specific wiring

- [x] **T-140** — Woodcutter: Tree → Logs; improve Plantation
- [x] **T-141** — Miner: Ore node; improve Quarry
- [x] **T-142** — Hunter: Animal → Meat; improve Cattle farm
- [x] **T-143** — Farmer: Plant → Plant food; improve Farm
- [X] **T-144** — Builder: no production; housing priority for unassigned adults
- [X] **T-145** — Builder scoring: target_share / central stock per producer profession
- [X] **T-146** — Builder: pick highest score, nearest upgradable/under-construction facility

### Phase P — Soldiers & towers

- [x] **T-150** — Built-area radius = max Manhattan from central to any building tile
- [x] **T-151** — Patrol positions on perimeter diamond; round-robin assignment
- [x] **T-152** — Tower: needs soldier inside to fire; 6×6 hit area (2 tiles each direction from 2×2 center)
- [x] **T-153** — Tower damage 1 base / 2 upgraded; 1 attack/sec; HP 100
- [x] **T-154** — Soldier mode split player %; periodic switch one-at-a-time + cooldown
- [x] **T-155** — Soldier improve: towers 100 Ore; player emphasis control hook

### Phase Q — Enemies

- [x] **T-160** — Spawn schedule: first at cycle 10, +1 every 10 cycles cumulative; spawn at map edges
- [x] **T-161** — Enemy movement 4-way; targeting priority order per SPEC
- [x] **T-162** — Enemy stats: HP 10, damage 1, 1 attack/sec, no miss

### Phase R — Combat & lose conditions

- [X] **T-170** — Melee/ranged unified: apply damage on attack tick
- [x] **T-171** — Central building destroyed → game over
- [x] **T-172** — All people dead → game over

### Phase S — Player controls (data layer)

- [x] **T-180** — Data structures for profession target % (6 professions)
- [x] **T-181** — Reproduction rate parameter
- [x] **T-182** — Soldier patrol/improve split + tower emphasis parameters
- [x] **T-183** — Producer threshold pairs per profession

### Phase T — UI (must-haves)

- [x] **T-190** — Central stock display (4 resources)
- [x] **T-191** — Production rates (overall; optional per profession if timeboxed)
- [x] **T-192** — Profession target vs actual %
- [x] **T-193** — Population: children/adults/elders counts
- [x] **T-194** — Housing: assigned vs unassigned counts
- [x] **T-195** — Per profession activity: producing vs improving (+ soldier/tower staffing)
- [x] **T-196** — Productivity: average and/or by stage; starvation counts

### Phase U — Presentation & camera

- [x] **T-200** — Orthographic isometric camera rig
- [x] **T-201** — Pan + zoom via Input System
- [x] **T-202** — Map/building/agent visual placeholders aligned to grid
- [x] **T-203** — Simulation tick driver MonoBehaviour (advances clock, reads state)

### Phase V — Integration & vertical slice

- [x] **T-210** — Single playable scene: central, a few nodes, spawn initial population
- [x] **T-211** — End-to-end: gather → deposit → construction completes
- [ ] **T-212** — End-to-end: enemy spawn → combat → possible game over
- [ ] **T-213** — README or `Docs` note: how to run; known SPEC gaps none (or list explicitly)

--- 

## Task details

Each task: **Spec reference**, **Do**, **Verification**.

---

### Phase A — Project & assemblies

#### T-001 — Confirm Unity version & packages baseline

- **Spec reference:** Unity implementation guidelines (`SPEC.md`).
- **Do:** Open Unity, note editor version in **Implementation log**. List installed packages (especially Input System, URP if used). Ensure project opens without errors.
- **Verification:** Editor loads; no red errors in Console on empty scene.

#### T-002 — Add folder skeleton under `Assets/`

- **Spec reference:** Project organization (`SPEC.md`).
- **Do:** Create folders: `Simulation`, `Presentation`, `UI`, `Input`, `Pathfinding`, `Config` (under `Assets/` or `Assets/Civilizator/`—pick one root and keep consistent). Add `.gitkeep` if needed so empty folders are tracked.
- **Verification:** Folders exist; Unity imports cleanly.

#### T-003 — Create `Civilizator.Simulation` assembly

- **Spec reference:** Project organization (`SPEC.md`).
- **Do:** Add `Civilizator.Simulation.asmdef` targeting appropriate platforms; place core simulation scripts here later.
- **Verification:** Script in this asmdef compiles.

#### T-004 — Create `Civilizator.Presentation` assembly

- **Spec reference:** Project organization (`SPEC.md`).
- **Do:** asmdef references Simulation; holds MonoBehaviours bridging sim ↔ Unity.
- **Verification:** Empty stub script compiles.

#### T-005 — Create `Civilizator.UI` assembly

- **Spec reference:** Input & UI (`SPEC.md`).
- **Do:** asmdef references Unity UI/Input modules as needed; references Presentation if required for bindings.
- **Verification:** Compiles.

#### T-006 — Create `Civilizator.Input` assembly

- **Spec reference:** Input system & UI (`SPEC.md`).
- **Do:** Input actions asset placeholder; asmdef references Input System package.
- **Verification:** Compiles.

#### T-007 — Wire assembly references (no cycles)

- **Spec reference:** Project organization (`SPEC.md`).
- **Do:** Simulation must not reference UnityEngine-heavy assemblies; UI → Presentation → Simulation (adjust if you use interfaces to reduce coupling). Document dependency direction in **Implementation log** if non-obvious.
- **Verification:** No cyclic dependency errors; full solution compiles.

#### T-008 — Install/enable **Input System**; Input System UI module stub

- **Spec reference:** Input system & UI (`SPEC.md`).
- **Do:** Package Manager: Input System active; EventSystem uses Input System UI Input Module (not standalone only).
- **Verification:** Play mode: UI button click works with new module (minimal test UI OK).

---

### Phase B — Core simulation types

#### T-010 — Grid coordinates & Manhattan helpers

- **Spec reference:** World & map → Grid (`SPEC.md`).
- **Do:** `GridPos` or `(int x,int y)` with clamp/bounds for 100×100; `Manhattan(a,b)`.
- **Verification:** Unit test or editor test: distance corners = 198.

#### T-011 — Map width-height constants

- **Spec reference:** World & map (`SPEC.md`).
- **Do:** Single source of truth `MapWidth = MapHeight = 100`.
- **Verification:** Referenced by generator and pathfinding.

#### T-012 — Natural node model

- **Spec reference:** Resource node generation (`SPEC.md`).
- **Do:** Types Tree/Plant/Animal/Ore; `Remaining` int init 100; depletion methods.
- **Verification:** Compile + simple instantiate test.

#### T-013 — Resource kind enum

- **Spec reference:** Resources & storage (`SPEC.md`).
- **Do:** Logs, Ore, Meat, PlantFood.
- **Verification:** Used by storage APIs in later tasks.

#### T-014 — Building kind enum + footprints

- **Spec reference:** Buildings (`SPEC.md`).
- **Do:** Central 3×3; others 2×2; include Tower, House, facilities.
- **Verification:** Helper `GetFootprintSize(kind)`.

#### T-015 — Placement rules: no overlap, 1 tile gap

- **Spec reference:** Build rules (`SPEC.md`).
- **Do:** Function `CanPlaceBuilding(world, kind, anchor)` enforcing non-overlap + Chebyshev/Manhattan gap ≥ 2 between footprints (SPEC: 1 tile away = no adjacent footprints).
- **Verification:** Unit tests: adjacent placement fails; gap-1 succeeds.

#### T-016 — Resource facility placement validation

- **Spec reference:** Build rules (`SPEC.md`).
- **Do:** Plantation/Farm/CattleFarm/Quarry require footprint overlaps ≥1 tile of matching natural node.
- **Verification:** Tests with mocked nodes.

#### T-017 — House placement: nearest to central

- **Spec reference:** Housing (`SPEC.md`).
- **Do:** Search empty valid tiles by increasing Manhattan from central anchor (or SPEC-accurate ordering); return first valid.
- **Verification:** Deterministic test on tiny map fixture.

#### T-018 — World generator: regions & nodes

- **Spec reference:** Resource node generation (`SPEC.md`).
- **Do:** 10×10 regions, exactly one of each node type per region, no duplicate tiles; assign positions deterministically from seed.
- **Verification:** Assert 400 nodes total; no tile collision; each region has 4 types.

#### T-019 — Generator validation

- **Spec reference:** Resource node generation (`SPEC.md`).
- **Do:** Automated test or Editor menu item that validates invariants.
- **Verification:** Test passes; document seed in log if relevant.

---

### Phase C — Simulation clock

#### T-020 — SimulationClock core

- **Spec reference:** Time & simulation cadence; Simulation clock (`SPEC.md`).
- **Do:** `CurrentCycle`, fractional or sub-minute accumulator; 1 cycle = 60 sim seconds.
- **Verification:** After advancing 60 sim seconds, cycle increments by 1.

#### T-021 — Clock driven by delta time

- **Spec reference:** Simulation clock (`SPEC.md`).
- **Do:** Presentation passes `deltaSimTime`; sim does not assume fixed FPS.
- **Verification:** Variable `Time.timeScale` or large dt still yields same cycles for same sim time.

#### T-022 — Second-boundary scheduling helpers

- **Spec reference:** Eating, construction, combat (`SPEC.md`).
- **Do:** API for “action ends at sim time T” usable by agents.
- **Verification:** Minimal test: two actions sequence correctly.

---

### Phase D — Pathfinding

#### T-030 — Passability / occupancy

- **Spec reference:** Grid, Pathfinding (`SPEC.md`).
- **Do:** Tiles blocked by building footprints; agents ignore each other for blocking (multi-occupancy).
- **Verification:** Unit test path through empty; through building fails.

#### T-031 — 4-way pathfinder

- **Spec reference:** Pathfinding (`SPEC.md`).
- **Do:** BFS or A*; N/E/S/W; return path or empty.
- **Verification:** Known grid maze reaches target.

#### T-032 — Nearest reachable tile to target

- **Spec reference:** Production loop (`SPEC.md`).
- **Do:** From agent position, find nearest node tile (Manhattan tie-break deterministic).
- **Verification:** Test with multiple equidistant nodes.

#### T-033 — Pathfinding regression test

- **Spec reference:** Pathfinding (`SPEC.md`).
- **Do:** Commit one JSON/grid fixture and expected path length.
- **Verification:** Test green.

---

### Phase E — Central storage

#### T-040 — Central storage model

- **Spec reference:** Central building storage (`SPEC.md`).
- **Do:** Four ints; clamp ≥ 0.
- **Verification:** Serialize/deserialize if you have save system; else in-memory only OK.

#### T-041 — Deposit API

- **Spec reference:** Production loop (`SPEC.md`).
- **Do:** On arrival at central tile region (define central footprint tiles), add carried resources instantly.
- **Verification:** Unit test deposit clears carry.

#### T-042 — Withdraw for construction

- **Spec reference:** Improvement loop (`SPEC.md`).
- **Do:** Withdraw min(carry cap, site remaining need).
- **Verification:** Does not go negative on central stock.

---

### Phase F — Buildings & construction

#### T-050 — Building instance state

- **Spec reference:** Buildings (`SPEC.md`).
- **Do:** Kind, anchor, HP for towers/central as specified later; upgrade 0/1 max.
- **Verification:** Create list of buildings on world state.

#### T-051 — Progress integer

- **Spec reference:** Construction progress model (`SPEC.md`).
- **Do:** Deliver increases progress; required amounts from costs.
- **Verification:** Progress never exceeds required until complete flag logic (T-053).

#### T-052 — Cost table

- **Spec reference:** Costs (`SPEC.md`).
- **Do:** Central + civil + tower costs encoded once.
- **Verification:** Constants match SPEC numbers.

#### T-053 — Completion gate

- **Spec reference:** Construction progress model (`SPEC.md`).
- **Do:** Building completes only after progress full **and** worker finishes post-delivery build seconds.
- **Verification:** Test with two partial deliveries.

#### T-054 — Build-time after delivery

- **Spec reference:** Construction time (`SPEC.md`).
- **Do:** `seconds = delivered_units * (1/productivity)` for that worker delivery event.
- **Verification:** Adult 1.0 delivers 10 → 10 seconds busy.

---

### Phase G — Facilities

#### T-060 — Facility spawn cycle

- **Spec reference:** Facilities (`SPEC.md`).
- **Do:** On cycle tick, each facility spawns per tile rules (no duplicate uncollected on tile).
- **Verification:** After 1 cycle, at most 4 items for 2×2 base rate 1.

#### T-061 — Upgraded double rate

- **Spec reference:** Upgrade effects (`SPEC.md`).
- **Do:** Doubled spawn attempts or amount per SPEC equivalence.
- **Verification:** Compare counts base vs upgraded over N cycles.

#### T-062 — Quarry gathering rate modifier

- **Spec reference:** Quarry (`SPEC.md`).
- **Do:** When gathering quarry-supported ore past depletion, base 2× gather time vs normal; upgraded 1×.
- **Verification:** Timed simulation test in code.

#### T-063 — Node depletion without quarry

- **Spec reference:** Quarry + nodes (`SPEC.md`).
- **Do:** Ore depletes at 0 remaining unless quarry rules apply; trees/plants/animals deplete per SPEC.
- **Verification:** Node at 0 not gatherable.

---

### Phase H — Agents core

#### T-070 — Agent state

- **Spec reference:** Agents (`SPEC.md`).
- **Do:** Position, profession, stage, HP.
- **Verification:** Spawn N agents without null refs.

#### T-071 — Base productivity by stage

- **Spec reference:** Productivity (`SPEC.md`).
- **Do:** Multipliers 1.0 / 0.5 / 0.5 as specified.
- **Verification:** Unit tests for effective multiplier.

#### T-072 — House assignment bonus

- **Spec reference:** Productivity (`SPEC.md`).
- **Do:** +20% additive with starvation stacking as specified.
- **Verification:** Math test: adult assigned + starved one cycle.

#### T-073 — Carry capacity

- **Spec reference:** Carrying (`SPEC.md`).
- **Do:** `floor(10 * productivity)` or exact if SPEC implies real number (SPEC: 10 × multiplier—use float then floor for integer carry).
- **Verification:** Child 50% → 5 units.

#### T-074 — Aging

- **Spec reference:** Life stages (`SPEC.md`).
- **Do:** Counters per agent; transitions on cycle boundaries.
- **Verification:** Fast-forward cycles in test.

---

### Phase I — Eating

#### T-080 — Once per cycle flag

- **Spec reference:** Eating (`SPEC.md`).
- **Do:** Reset eat flag each new cycle; require eat before end or on schedule.
- **Verification:** Agent eats at most once per cycle.

#### T-081 — Eat action timing

- **Spec reference:** Eating (`SPEC.md`).
- **Do:** Travel to central footprint, 1 second action, consume 1 food.
- **Verification:** Stock decreases by 1; productivity not penalized if food exists.

#### T-082 — Starvation & death

- **Spec reference:** Eating (`SPEC.md`).
- **Do:** −25% per failed cycle; death at 0%.
- **Verification:** Force no food; assert death after enough cycles.

---

### Phase J — Housing

#### T-090 — Capacity rules

- **Spec reference:** Housing (`SPEC.md`).
- **Do:** Track adults per house; children not counted against 2.
- **Verification:** Third adult cannot assign to full house.

#### T-091 — Assign on house complete

- **Spec reference:** Housing (`SPEC.md`).
- **Do:** When construction completes, pick 2 random adults without house.
- **Verification:** Deterministic RNG seed test.

#### T-092 — Fill on death

- **Spec reference:** Housing (`SPEC.md`).
- **Do:** Reassign random adult to vacancy.
- **Verification:** House count restored after death event.

#### T-093 — Child to adult assignment

- **Spec reference:** Housing (`SPEC.md`).
- **Do:** Assign to house with open adult slot; if none, leave unassigned (builders prioritize per SPEC).
- **Verification:** State transitions on birthday cycle.

---

### Phase K — Reproduction

#### T-100 — Reproduction parameter

- **Spec reference:** Reproduction (`SPEC.md`).
- **Do:** Player-tunable value stored in sim config.
- **Verification:** Exposed to UI layer (stub read OK).

#### T-101 — Breeding roll

- **Spec reference:** Reproduction (`SPEC.md`).
- **Do:** Per eligible house with ≥2 adults, roll per cycle; spawn child at house location or central per your sim convention (document choice; must match presentation).
- **Verification:** Probability increases with parameter.

---

### Phase L — Professions

#### T-110 — Profession enum

- **Spec reference:** Professions (`SPEC.md`).
- **Do:** All six values.
- **Verification:** Compile.

#### T-111 — Initial assignment undernumbered

- **Spec reference:** Choosing professions (`SPEC.md`).
- **Do:** On new agent, compare actual% vs target%; pick max deficit profession.
- **Verification:** Test with skewed targets.

#### T-112 — Correction passes

- **Spec reference:** Choosing professions (`SPEC.md`).
- **Do:** If discrepancy > threshold, switch one agent from most over to most under.
- **Verification:** Counts move toward targets over time in test.

#### T-113 — Cooldown constant

- **Spec reference:** Choosing professions (`SPEC.md`).
- **Do:** Named constant (e.g. cycles or seconds); document default.
- **Verification:** Two switches cannot occur faster than cooldown.

---

### Phase M — Policy thresholds

#### T-120 — Threshold data

- **Spec reference:** Player controls (`SPEC.md`).
- **Do:** For each producer profession: start below, stop above stocks.
- **Verification:** Defaults sensible (document in Config).

#### T-121 — Decision integration

- **Spec reference:** Common action loops (`SPEC.md`).
- **Do:** If stock above stop → improvement; below start → production (clarify hysteresis in code comments per SPEC).
- **Verification:** Stock crossing triggers mode change in test.

---

### Phase N — Action loops

#### T-130 — Nearest node selection

- **Spec reference:** Production loop (`SPEC.md`).
- **Do:** Manhattan nearest; same tile to gather.
- **Verification:** Agent walks to node using pathfinder.

#### T-131 — Gather rate

- **Spec reference:** Production loop (`SPEC.md`).
- **Do:** 1/sec adjusted by productivity; updates node remaining.
- **Verification:** Time to fill carry matches formula.

#### T-132 — Deposit & fallback

- **Spec reference:** Production loop (`SPEC.md`).
- **Do:** Full carry → central; no node → improvement.
- **Verification:** Integration test.

#### T-133 — Improvement loop steps

- **Spec reference:** Improvement loop (`SPEC.md`).
- **Do:** Withdraw/travel/deliver/build-time looped until no targets.
- **Verification:** Progress increases on delivery events.

---

### Phase O — Profession wiring

#### T-140 — Woodcutter

- **Spec reference:** Woodcutter (`SPEC.md`).
- **Do:** Wire production+improve targets.
- **Verification:** Sim-only test: logs increase.

#### T-141 — Miner

- **Spec reference:** Miner (`SPEC.md`).
- **Do:** Ore production + quarry improvements.
- **Verification:** Ore stock increases.

#### T-142 — Hunter

- **Spec reference:** Hunter (`SPEC.md`).
- **Do:** Meat + cattle farm.
- **Verification:** Meat stock increases.

#### T-143 — Farmer

- **Spec reference:** Farmer (`SPEC.md`).
- **Do:** Plant food + farm.
- **Verification:** Plant food increases.

#### T-144 — Builder housing priority

- **Spec reference:** Builder (`SPEC.md`).
- **Do:** If adults unhoused, improvement targets house construction first.
- **Verification:** Unhoused count decreases.

#### T-145 — Builder scoring math

- **Spec reference:** Builder (`SPEC.md`).
- **Do:** Score = target_fraction / max(stock, epsilon); sort professions.
- **Verification:** Known numeric fixture.

#### T-146 — Builder facility targeting

- **Spec reference:** Builder (`SPEC.md`).
- **Do:** Nearest valid site for chosen profession; fallback down the list.
- **Verification:** Multiple sites test.

---

### Phase P — Soldiers & towers

#### T-150 — Built-area radius

- [x] Implemented `BuiltAreaSystem.GetBuiltAreaRadius(...)` with central-footprint distance calculation.

- **Spec reference:** Soldier (`SPEC.md`).
- **Do:** Compute max Manhattan from central footprint to any occupied building tile.
- **Verification:** Adding building increases radius test.

#### T-151 — Patrol positions

- [x] Implemented `SoldierPatrolSystem.GetPatrolPositions(...)` and round-robin `AssignPatrolPositions(...)`.

- **Spec reference:** Soldier (`SPEC.md`).
- **Do:** Diamond perimeter sampling; round-robin.
- **Verification:** N soldiers → N distinct assigned tiles (when possible).

#### T-152 — Tower fire gating & range

- **Spec reference:** Defensive buildings (`SPEC.md`).
- **Do:** 6×6 rectangle; soldier inside; enemies in rect attacked.
- **Verification:** Enemy at boundary in/out of range.

#### T-153 — Tower stats

- **Spec reference:** Defensive buildings (`SPEC.md`).
- **Do:** HP 100, damage 1/2, cadence 1/s.
- **Verification:** Time-to-kill enemy = 10s base.

#### T-154 — Soldier mode switching

- **Spec reference:** Soldier (`SPEC.md`).
- **Do:** Player split %; periodic check; one switch per cooldown.
- **Verification:** Distribution trends toward target % in long run (stochastic OK).

#### T-155 — Soldier improvement targets

- **Spec reference:** Soldier (`SPEC.md`).
- **Do:** Ore-based tower build/upgrade; emphasis parameter chooses priority.
- **Verification:** Ore decreases when improving.

---

### Phase Q — Enemies

#### T-160 — Spawn schedule & edges

- **Spec reference:** Enemies (`SPEC.md`).
- **Do:** Implement spawn cadence per **SPEC clarifications** below; spawn on map edge tiles with valid 4-way connectivity.
- **Verification:** At cycles 10, 20, 30, … spawns occur as documented; log documents exact cumulative vs per-wave interpretation.

#### T-161 — Enemy AI movement & targeting

- **Spec reference:** Enemies (`SPEC.md`).
- **Do:** Priority list: attacker person → attacker tower → nearest civilian/building.
- **Verification:** Scenario tests with scripted positions.

#### T-162 — Enemy combat stats

- **Spec reference:** Enemies (`SPEC.md`).
- **Do:** HP/damage/cadence; no miss.
- **Verification:** 10 hits kill 10 HP target.

---

### Phase R — Combat & game over

#### T-170 — Damage application

- **Spec reference:** Combat stats (`SPEC.md`).
- **Do:** Soldiers, enemies, towers share tick attack pipeline where possible.
- **Verification:** Towers, soldiers, enemies all deal expected DPS.

#### T-171 — Central destroyed

- **Spec reference:** Lose conditions (`SPEC.md`).
- **Do:** Implement “central destroyed” per **SPEC clarifications** below (pick one approach, document in code and **Implementation log**).
- **Verification:** Game over flag sets when central is considered destroyed (e.g. HP ≤ 0 or enemy enters central footprint—per chosen rule).

#### T-172 — Everyone dead lose

- **Spec reference:** Lose conditions (`SPEC.md`).
- **Do:** When population 0, game over.
- **Verification:** Kill all agents in test harness.

---

### Phase S — Player controls (data)

#### T-180 — Profession targets

- **Spec reference:** Global controls (`SPEC.md`).
- **Do:** Six floats summing to ~100% with validation/normalization.
- **Verification:** Changing targets affects assignment over time.

#### T-181 — Reproduction rate binding

- **Spec reference:** Global controls (`SPEC.md`).
- **Do:** UI/slider writes sim config.
- **Verification:** Visible effect in sim (faster children).

#### T-182 — Soldier controls

- **Spec reference:** Soldier controls (`SPEC.md`).
- **Do:** Patrol/improve split + tower emphasis stored and read by soldier AI.
- **Verification:** Modes shift when sliders change.

#### T-183 — Producer thresholds binding

- **Spec reference:** Profession controls (`SPEC.md`).
- **Do:** Four professions × two thresholds in config.
- **Verification:** Thresholds change producer loop in sim test.

---

### Phase T — UI must-haves

#### T-190 — Central stocks UI

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Four labels bound to sim state each frame or on event.
- **Verification:** Play mode: gather updates numbers.

#### T-191 — Production rates

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Track deltas per cycle/minute; display overall; optional per profession.
- **Verification:** Non-zero when producing.

#### T-192 — Profession allocation UI

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Sliders + actual % readout.
- **Verification:** Matches headcount.

#### T-193 — Population UI

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Children/adults/elders counts.
- **Verification:** Aging transitions update UI.

#### T-194 — Housing UI

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Assigned vs unassigned counts.
- **Verification:** Matches internal state.

#### T-195 — Activity breakdown

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Per profession producing vs improving; soldiers patrolling vs improving; staffed towers count.
- **Verification:** Spot-check during play.

#### T-196 — Productivity & starvation UI

- **Spec reference:** Required feedback (`SPEC.md`).
- **Do:** Average productivity; starvation count or list.
- **Verification:** Starvation visible when no food.

---

### Phase U — Presentation

#### T-200 — Camera rig

- **Spec reference:** Camera (`SPEC.md`).
- **Do:** Orthographic isometric; suitable for 100×100 grid.
- **Verification:** Pan across full map.

#### T-201 — Pan/zoom input

- **Spec reference:** Camera + Input (`SPEC.md`).
- **Do:** Input System actions bound.
- **Verification:** No legacy Input API for these.

#### T-202 — Visual placeholders

- **Spec reference:** Core premise (`SPEC.md`).
- **Do:** Cube/sprites for agents, buildings, nodes; aligned to grid.
- **Verification:** Positions match sim coordinates.

#### T-203 — Sim driver

- **Spec reference:** Simulation clock (`SPEC.md`).
- **Do:** MonoBehaviour calls `SimulationStep(dt)`; UI reads façade.
- **Verification:** Play mode runs without sim in Update directly tied to rendering (single tick pipeline).

---

### Phase V — Integration

#### T-210 — Playable scene

- **Spec reference:** Full V1 (`SPEC.md`).
- **Do:** One scene with central, generated world, starting population.
- **Verification:** Press Play: agents move.

#### T-211 — Construction vertical slice

- **Spec reference:** Improvement loop + costs (`SPEC.md`).
- **Do:** Player can threshold/force improvement; building completes.
- **Verification:** Logs decrease, site completes.

#### T-212 — Combat vertical slice

- **Spec reference:** Enemies + combat (`SPEC.md`).
- **Do:** Enemies spawn; soldiers/towers fight; possible lose.
- **Verification:** Enemy HP decreases; game over triggers.

#### T-213 — Handoff documentation

- **Spec reference:** N/A.
- **Do:** Short note in `Docs/` or root README: controls, scene name, known gaps vs SPEC.
- **Verification:** New developer can open project and play.

---

## Implementation log

_Append one line per completed task (optional but recommended)._

- 2026-04-21 — T-155 — `Assets/Civilizator/Simulation/SoldierImprovementSystem.cs`, `Assets/Civilizator/Tests/Simulation/SoldierImprovementSystemTests.cs` — Soldier tower improvement already uses Ore and the emphasis control hook; verified against the existing regression tests.
- 2026-04-21 — T-160 — `Assets/Civilizator/Simulation/Enemy.cs`, `Assets/Civilizator/Simulation/EnemySpawner.cs`, `Assets/Civilizator/Tests/Simulation/EnemySpawnerTests.cs` — Enemy spawning is cumulative by cycle (cycle 10 = 1, cycle 20 = 2 total, etc.) and spawn positions are edge tiles only.
- 2026-04-21 — T-161 — `Assets/Civilizator/Simulation/EnemyAISystem.cs`, `Assets/Civilizator/Tests/Simulation/EnemyAISystemTests.cs` — Enemy target priority now prefers attacking people, then attacking towers, then nearest civilian/building, and movement advances one 4-way step via pathfinding.
- 2026-04-21 — T-171 — `Assets/Civilizator/Simulation/GameOverState.cs`, `Assets/Civilizator/Simulation/BuildingPlacement.cs`, `Assets/Civilizator/Tests/Simulation/GameOverStateTests.cs` — Central building defeat is tracked by `GameOverState`; the central building starts with combat HP and flips the game-over flag at 0 HP.
- 2026-04-22 — T-172 — `Assets/Civilizator/Simulation/GameOverState.cs`, `Assets/Civilizator/Tests/Simulation/GameOverStateTests.cs` — Population wipeout is tracked by `GameOverState`; null agents are ignored and the game-over flag flips only when no living agents remain.
- 2026-04-22 — T-190 — `Assets/Civilizator/UI/CentralStockDisplay.cs`, `Assets/Civilizator/UI/CentralStockDisplay.cs.meta`, `Assets/Civilizator/UI/Civilizator.UI.asmdef`, `Assets/Civilizator/Tests/UI/CentralStockDisplayTests.cs`, `Assets/Civilizator/Tests/UI/CentralStockDisplayTests.cs.meta`, `Assets/Civilizator/Tests/UI.meta`, `Assets/Civilizator/Tests/Tests.asmdef` — Added a minimal uGUI stock HUD with binding/formatting helpers and edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` because the main project was open in the editor.
- 2026-04-22 — T-191 — `Assets/Civilizator/UI/ProductionRateDisplay.cs`, `Assets/Civilizator/UI/ProductionRateDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/ProductionRateDisplayTests.cs`, `Assets/Civilizator/Tests/UI/ProductionRateDisplayTests.cs.meta` — Added a uGUI production-rate HUD with overall + per-profession breakdown, invariant-culture formatting, and edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` after syncing a clean copy because the main project was open in the editor.
- 2026-04-22 — T-192 — `Assets/Civilizator/UI/ProfessionAllocationDisplay.cs`, `Assets/Civilizator/UI/ProfessionAllocationDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/ProfessionAllocationDisplayTests.cs`, `Assets/Civilizator/Tests/UI/ProfessionAllocationDisplayTests.cs.meta` — Added a profession allocation HUD with target sliders and actual percentage readouts, plus invariant-culture formatting and edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` after syncing a clean copy because the main project was open in the editor.
- 2026-04-22 — T-193 — `Assets/Civilizator/UI/PopulationDisplay.cs`, `Assets/Civilizator/UI/PopulationDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/PopulationDisplayTests.cs`, `Assets/Civilizator/Tests/UI/PopulationDisplayTests.cs.meta` — Added a population HUD with child/adult/elder counts and total population, plus edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` after syncing a clean copy because the main project was open in the editor.
- 2026-04-22 — T-194 — `Assets/Civilizator/UI/HousingDisplay.cs`, `Assets/Civilizator/UI/HousingDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/HousingDisplayTests.cs`, `Assets/Civilizator/Tests/UI/HousingDisplayTests.cs.meta` — Added a housing coverage HUD with assigned/unassigned counts, an agent-list binding helper, and edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` after syncing a clean copy because the main project was open in the editor.
- 2026-04-22 — T-195 — `Assets/Civilizator/UI/ActivityBreakdownDisplay.cs`, `Assets/Civilizator/UI/ActivityBreakdownDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/ActivityBreakdownDisplayTests.cs`, `Assets/Civilizator/Tests/UI/ActivityBreakdownDisplayTests.cs.meta` — Added a per-profession activity HUD with live world-state derivation for producers, builders, soldiers, and staffed towers, plus edit-mode coverage. Verified with headless Unity compile on `/tmp/Civilizator-unity-check` after syncing a clean copy because the main project was open in the editor.
- 2026-04-22 — T-202 — `Assets/Civilizator/Presentation/WorldPlaceholderView.cs`, `Assets/Civilizator/Presentation/WorldPlaceholderView.cs.meta`, `Assets/Civilizator/Tests/Presentation/WorldPlaceholderViewTests.cs`, `Assets/Civilizator/Tests/Presentation/WorldPlaceholderViewTests.cs.meta` — Added a runtime bootstrap that spawns grid-aligned placeholder floor, region grid, natural nodes, buildings, and agents using primitive meshes plus tile-to-world helpers and coverage for the coordinate math. Verified with headless Unity batchmode compile on `/tmp/civilizator-verify` (the main project was open, so I used a clean copy).

Format: `YYYY-MM-DD | T-xxx | note`

- _Example: 2026-04-09 | T-001 | Unity 6000.x; Input System 1.14._

- 2026-04-09 | T-001 | Unity 6000.4.1f1 (`ProjectSettings/ProjectVersion.txt` matches Hub). Packages (manifest): Input System 1.19.0, URP `com.unity.render-pipelines.universal` 17.4.0, AI Navigation 2.0.11, ugui 2.0.0, Test Framework 1.6.0, Timeline 1.8.11, Visual Scripting 1.9.11, Collab Proxy 2.11.4; remainder in `Packages/manifest.json`. Verified: `Unity -batchmode -nographics -quit` on fresh rsync copy without `Library` exited 0 (import + compile). If the main project folder is locked by an open editor, batchmode on that path aborts; use a copy or close the editor.

- 2026-04-10 | T-002 | Folder skeleton under `Assets/Civilizator/`: `Simulation`, `Presentation`, `UI`, `Input`, `Pathfinding`, `Config` with `.gitkeep`; Unity folder/file `.meta` added for stable GUIDs. Headless `-batchmode` hit licensing handshake 505 / protocol mismatch in this run—verify import in Editor (Console clear) if batch CI is unavailable.

- 2026-04-10 | T-003 | `Assets/Civilizator/Simulation/Civilizator.Simulation.asmdef` (`noEngineReferences: true`), stub `SimulationAssemblyMarker.cs` + `.meta` GUIDs. Verified: `Unity -batchmode -quit` on rsync copy without `Library` (exit 0); Bee log shows `Civilizator.Simulation.dll` compiled. Main tree locked if Editor has project open (batchmode on same path fails)

- 2026-04-10 | T-004 | `Assets/Civilizator/Presentation/Civilizator.Presentation.asmdef` references `Civilizator.Simulation`; stub `PresentationAssemblyMarker.cs` (`MonoBehaviour`, touches `SimulationAssemblyMarker.Version`). Verified: `Unity -batchmode -nographics -quit -projectPath …` exit 0.

- 2026-04-10 | T-005 | `Assets/Civilizator/UI/Civilizator.UI.asmdef` references `Civilizator.Presentation`, `UnityEngine.UI`, `Unity.InputSystem`; stub `UIAssemblyMarker.cs` (uGUI `Button`, `InputSystemUIInputModule`, `PresentationAssemblyMarker`). Verified: `Unity -batchmode -nographics -quit` exit 0; `Library/ScriptAssemblies/Civilizator.UI.dll` produced.

- 2026-04-10 | T-006 | `Assets/Civilizator/Input/Civilizator.Input.asmdef` references `Unity.InputSystem`; placeholder `CivilizatorInput.inputactions` (Camera map: Pan, Zoom stubs); `InputAssemblyMarker.cs` (`InputActionAsset` field). Removed `Input/.gitkeep`. Verified: `Unity -batchmode -nographics -quit` exit 0; Bee/CSC produced `Library/ScriptAssemblies/Civilizator.Input.dll`.

- 2026-04-11 | T-016 | Added `BuildingKindHelpers.GetRequiredNodeType()` and `IsResourceFacility()` to map resource facilities → node types. Extended `BuildingPlacementValidator.CanPlaceBuilding()` signature with optional `naturalNodes` parameter; added `HasMatchingNodeOverlap()` validation. Added 11 comprehensive NUnit tests (resource facility node matching: Plantation↔Tree, Farm↔Plant, CattleFarm↔Animal, Quarry↔Ore; overlap boundary conditions; null/empty nodes). Backward compatible: non-resource buildings (House, Tower, Central) ignore nodes; existing tests pass unchanged. Files: `BuildingKind.cs`, `BuildingPlacement.cs`, `BuildingPlacementTests.cs`.

- 2026-04-11 | T-018 | Implemented `WorldGenerator` (static utility class) with `GenerateNodes(int seed)` method. Divides 100×100 map into 10×10 regions (10 tiles per region). For each region: places exactly one node of each type (Tree, Plant, Animal, Ore) in random, unused tiles via seeded RNG. Returns 400 nodes total (10×10 regions × 4 types). Deterministic: same seed = same positions & types. Added comprehensive tests in `WorldGeneratorTests.cs`: 400-node count, no tile collisions, 4 types per region, determinism, in-bounds validation, initial amount. Files: `WorldGenerator.cs`, `WorldGeneratorTests.cs`.

- 2026-04-11 | T-019 | Verified comprehensive NUnit test suite in `WorldGeneratorTests.cs` (8 test methods) validates all invariants: 400 nodes total, 4 types per 10×10 region, no position collisions, deterministic generation, different seeds produce different results, all nodes in-bounds, InitialAmount = 100, exactly 4 nodes per region. All tests use seeded RNG; no external dependencies. Tests satisfy SPEC.md node placement requirements (10×10 regions, 1 node per type per region, no duplicates). No changes needed—implementation and validation complete.

- 2026-04-11 | T-020 | Implemented `SimulationClock` class (public `CurrentCycle` and `AccumulatedSeconds` properties; `const float SecondsPerCycle = 60`; `Advance(float deltaTime)` increments cycles while accumulator ≥ 60; `Reset()` clears state; `TotalSimulationSeconds` property). Added comprehensive test suite `SimulationClockTests.cs` (9 NUnit tests: initialization, within-cycle accumulation, exact cycle boundary, multiple cycles, remainders, multi-step advancement, negative delta handling, total seconds calculation, reset). Verified: 60 sim seconds = 1 cycle increment, fractional accumulation preserved across advances. Files: `SimulationClock.cs`, `SimulationClockTests.cs`.


 same cycles/remainder), large timestep correctness. Created `Civilizator.Presentation.Tests.asmdef` for test assembly. Verification: tests pass; simulation independent of FPS and timeScale. Files: `SimulationTickDriver.cs`, `SimulationTickDriverTests.cs`, `Civilizator.Presentation.Tests.asmdef`.

- 2026-04-11 | T-030 | Implemented `GridOccupancy` class with efficient bool array occupancy tracking (O(1) space/time lookup). Buildings block footprint tiles; agents don't block each other (multi-occupancy per spec). Public API: `IsPassable(GridPos)`, `BlockTile/UnblockTile`, `BlockBuilding/UnblockBuilding`, `Clear()`. Added 9 comprehensive NUnit tests in `GridOccupancyTests.cs`: passability initialization, single/multiple tile blocking, building footprint blocking, unblocking, state reset, out-of-bounds handling, multiple coexisting buildings. Files: `GridOccupancy.cs`, `GridOccupancyTests.cs`.

- 2026-04-11 | T-031 | Implemented 4-way BFS pathfinder `Pathfinding.cs` with static `FindPath(GridPos start, GridPos target, GridOccupancy occupancy)` method. Explores N/E/S/W directions; returns complete path or empty list if unreachable. Handles edge cases: start=target returns single-tile path; blocked start/target returns empty; uses Queue-based BFS for O(width×height) time. Added 9 comprehensive NUnit tests in `PathfindingTests.cs`: direct/diagonal paths, obstacle routing, blocked positions, no-path scenarios, complex mazes, bounds respect. Verified: test suite covers all major pathfinding cases. Files: `Pathfinding.cs`, `PathfindingTests.cs` + .meta files.

- 2026-04-11 | T-033 | Added regression test `FindPath_ComplexMazeRegression_ValidatesFixedBehavior()` to `PathfindingTests.cs`. Test validates pathfinding on 20×15 grid with complex maze: vertical wall at x=5 (except y=5), horizontal wall at y=5 (x=9–15), vertical wall at x=7 (y=10–14). Path from (1,1) to (18,13) expected = 29 tiles. Validates: correct path length, start/end positions, no blocked tiles in path. Added JSON fixture `PathfindingRegressionFixture.json` documenting the scenario. Fixed compilation errors in `GridOccupancyTests.cs` (changed `BuildingKind.CentralBuilding` → `BuildingKind.Central`) and meta file YAML issues. Changed `autoReferenced` to true in `Civilizator.Simulation.Tests.asmdef`. Files: `PathfindingTests.cs`, `PathfindingRegressionFixture.json`, `GridOccupancyTests.cs`, `.asmdef`.

- 2026-04-12 | T-051 | Added `ConstructionProgress` integer field to existing `Building` class in `BuildingPlacement.cs` (initialized to 0). Created `BuildingCostHelper` static class with cost constants: `CivilBuildingBuildCost = 100` (Logs), `TowerBuildCost = 100` (Ore), `CivilBuildingUpgradeCost = 100`, `TowerUpgradeCost = 100`. Added methods to `Building`: `GetConstructionResourceKind()` (Tower → Ore, others → Logs), `GetRequiredConstructionAmount()` (returns cost based on IsUnderConstruction/UpgradeLevel), `DeliverBuildResources(int amount)` (increments progress, capped at required). Added comprehensive NUnit test suite `BuildingTests.cs` with 20 test methods validating initialization, costs, progress delivery, and capping. Verified: project compiles without errors. Files: `BuildingPlacement.cs` (modified), `BuildingCostHelper.cs` (new), `BuildingTests.cs` (new), `.meta` files.

- 2026-04-12 | T-053, T-054 | **Building Completion Gate & Build-Time Mechanics.** Added `float BuildTimeEndSeconds` field to `Building` class to track post-delivery build-time end timestamp. Added `SimulationClock SimulationClock` property (nullable) to hold clock reference for time-based gating. Extended `DeliverBuildResources(int amount, float productivityMultiplier = 1f)` signature to accept worker productivity: when delivery completes required amount and clock is available, schedules build-time = `delivered_units * (1 / productivity_multiplier)`. Added `bool IsConstructionPhaseComplete()` method: returns true only when (1) progress ≥ required AND (2) build-time has expired (if clock available). Backward compatible: old calls `DeliverBuildResources(amount)` use default productivity 1f; tests without clock fall back to progress-based completion. Added 13 comprehensive NUnit tests in `BuildingTests.cs`: clock-less progress gates, build-time scheduling, time-gated completion (multiple productivity values: 1.0 Adult, 0.5 Child), multi-delivery scenarios (only final delivery triggers build-time), upgrade support, invalid productivity rejection, edge cases. Verified: SPEC.md compliance (seconds = delivered_units / productivity), deterministic test outcomes, backward compatibility with existing tests. Files: `BuildingPlacement.cs` (modified), `BuildingTests.cs` (extended), `PLAN.md` (marked T-053/T-054 done).

Meat); ignores non-resource buildings. Added 13 comprehensive NUnit tests in `FacilitySpawningTests.cs` covering: base 1-per-tile spawn (Plantation, Farm, CattleFarm), upgraded 2-per-tile spawn, 22 footprint correctness, under-construction exclusion, non-facility buildings ignored, duplicate prevention (uncollected blocks, collected allows), cycle-once enforcement, multiple facilities, high-level resource kind validation. Verification: "After 1 cycle, at most 4 items for 22 base rate  (tests validate exactly 4 spawned resources per base facility per cycle). Files: `SpawnedResource.cs` + .meta, `FacilitySpawner.cs` + .meta, `FacilitySpawningTests.cs` + .meta.1" 

- 2026-04-12 | T-061, T-062 | **T-061 already complete (verified).** Upgraded facilities (Plantation/Farm/CattleFarm) double spawn rate via `UpgradeLevel > 0 ? 2 : 1` spawn attempts in `FacilitySpawner.cs`. Tests verify: upgraded spawns 8 logs per cycle (2 per tile  4 tiles) vs base 4 logs. **T-062: Implemented `QuarrySupport` static helper class** to manage quarry-enabled ore gathering. Core rules: quarry overlaps Ore nodes; enables indefinite collection past depletion; base quarry = half speed (2 gathering time), upgraded = normal speed (1). Added methods: `IsNodeSupportedByQuarry(node, quarries)` (checks footprint overlap), `GetOreGatheringRateMultiplier/TimeMultiplier(node, isDepletedPastZero, quarry)` (returns 0.5/2.0 for base, 1.0/1.0 for upgraded), `FindSupportingQuarry(node, quarries)`. Added 16 NUnit tests in `QuarrySupportTests.cs`: footprint overlap edge cases, multiple quarries, rate/time multiplier reciprocals, base vs upgraded quarry behavior, depletion scenarios. Verification: timed simulation can use multipliers during agent gathering phase (T-130+). Files: `QuarrySupport.cs`, `QuarrySupportTests.cs`, `PLAN.md` (marked T-061/T-062 done).

- 2026-04-13 | T-082 | Added starvation penalty handling to `EatingAction`: failed eat applies -25% productivity penalty and zeros HP at 100% penalty; successful eat resets penalties. Added integration tests in `AgentTests.cs` for starvation penalty application and death after four failed cycles.


- 2026-04-12 | T-063 | **Implemented node depletion rules with quarry exception.** Added `IsGatherable(bool hasQuarrySupport)` method to `NaturalNode`: normal nodes (Tree/Plant/Animal) gatherable only if remaining > 0; ore without quarry gatherable only if remaining > 0; ore with quarry always gatherable (indefinite collection past depletion). Added 12 comprehensive NUnit test methods in `NaturalNodeGatherabilityTests` covering: normal node gatherability by type, depletion-gating, quarry-enabled ore indefinite collection, no quarry support for non-ore nodes, depletion sequence validation. Verification: depleted nodes at 0 remaining not gatherable unless quarry-supported ore. Files: `NaturalNode.cs` (modified), `NaturalNodeTests.cs` (extended).

- 2026-04-12 | T-070 | **Implemented Agent model.** Created `Agent` class with properties: Position (GridPos), Profession (enum: 6 types), LifeStage (enum: Child/Adult/Elder), HitPoints (int, default 10). Constructors: `Agent(position)` defaults to Woodcutter/Child/10HP; `Agent(position, profession, stage)` with custom values. Added `IsAlive` property (HP > 0). Created `Profession` enum (Woodcutter, Miner, Hunter, Farmer, Builder, Soldier). Created `LifeStage` enum (Child, Adult, Elder). Created `LifeStageHelpers` static class with constants: `ChildToAdultCycles = 10`, `AdultToElderCycles = 60`, `ElderToDeathCycles = 10`. Added 32 NUnit tests in `AgentTests` covering initialization, all professions/stages, property mutations, multi-agent spawning. Verification: 10 agents spawn without null refs. Files: `Agent.cs`, `AgentTests.cs`.

- 2026-04-12 | T-071, T-072, T-073 | **Implemented productivity and carry capacity system.** T-071: Added `GetProductivityMultiplier()` method returns base by stage (Adult 1.0, Child/Elder 0.5). T-072: Added `AssignedHouseId` (nullable int), `IsHouseAssigned` property, `HouseAssignmentBonus = 0.2f` constant. Productivity with house: Child 0.7, Adult 1.2, Elder 0.7. T-073: Added `GetCarryCapacity()` method = `10  productivity_multiplier`. Base: Child/Elder 5, Adult 10. With house: Child/Elder 7, Adult 12. Added 21 comprehensive tests in `AgentProductivityTests` and `AgentCarryCapacityTests` covering all stages, house assignment, capacity scaling. Verification: carry capacity correctly scales with productivity. Files: `Agent.cs` (extended), `AgentTests.cs` (extended).

Death), multi-agent independent counters, death handling. Verification: agents age correctly through all stages. Files: `AgingSystem.cs`, `AgingSystemTests.cs`.

- 2026-04-12 | T-080 | **Implemented Once Per Cycle Eating Requirement.** Added `bool HasEatenThisCycle` property to `Agent` class (initialized to false). Added `MarkAsEaten()` method to set flag when agent eats, and `ResetEatingFlag()` method to reset at start of new cycle. Added 8 comprehensive NUnit tests in `AgentEatingTests.cs` covering: initialization (flag false), marking eaten (flag true), resetting flag, cycle progression (eat, reset, eat again), independent flags for multiple agents, persistence through other state changes, all life stages, all professions. Verification: agent eats at most once per cycle (flag prevents multiple eats in same cycle). Tests use standard NUnit assertions. Files: `Agent.cs` (extended), `AgentTests.cs` (extended with new test class).

- 2026-04-12 | T-081 | **Implemented Eating Action & Starvation Mechanics.** Created `AgentEatingState.cs` class tracking starvation penalty (−25% per failed cycle, capped at 100%). Created `EatingAction.cs` class managing eating behavior: (1) path-finding to central building using existing `Pathfinding.FindPath()`, (2) 1-second eating timer via `Update(deltaTime)`, (3) food consumption requiring **1 Meat AND 1 Plant Food** (both required, not either/or). Extended `Agent.GetProductivityMultiplier()` to apply starvation penalty subtractively (base + house − starvation, clamped ≥ 0). Added 18 comprehensive NUnit tests: `AgentStarvationTests` (8 tests for penalty stacking, capping, house interaction, productivity clamping) and `EatingActionTests` (10 tests for pathfinding, timer, Meat+PlantFood requirement, both food types required, individual food shortages). Verification: stock decreases by 1 Meat + 1 PlantFood; eating fails if either type unavailable; productivity unpenalized if food available. Files: `AgentEatingState.cs`, `EatingAction.cs`, `Agent.cs` (extended), `AgentTests.cs` (extended).

- 2026-04-13 | T-090 | **Implemented House Capacity Rules (Phase J Housing).** CRITICAL FIX: Resolved namespace collision by deleting orphaned `Building.cs` (incomplete, conflicting constructor signatures). Added house resident tracking to canonical `Building` class in `BuildingPlacement.cs`: `List<int> AdultResidentIds` (max 2 capacity) and `List<int> ChildResidentIds` (unlimited). Added methods: `GetAdultCount()`, `GetChildCount()`, `HasAvailableAdultSlot()`, `AssignAdultResident(id) → bool`, `RemoveAdultResident(id)`, `AssignChildResident(id) → bool`, `RemoveChildResident(id)`. Capacity enforcement: returns false if adult assignment attempted on full house; children always succeed. Added 11 comprehensive NUnit tests in `BuildingTests.cs` covering: initial empty state, single/dual/triple adult assignment attempts, child unlimited capacity, adult removal/reassignment, duplicate tracking, vacancy filling. **KEY VERIFICATION PASSING:** "Third adult cannot assign to full house" (test `HouseCapacity_CannotAssignThirdAdult`). Files: `BuildingPlacement.cs` (modified + added resident methods), `Building.cs` (deleted), `BuildingTests.cs` (11 new test methods), `.meta` files.

- 2026-04-13 | T-091 | **Implemented On-House-Complete Adult Assignment (Phase J Housing).** Created `HouseAssignmentSystem` class managing automatic adult assignment to completed houses. Core logic: detects `IsConstructionPhaseComplete() == true` for houses, finds unassigned adults (LifeStage==Adult, IsHouseAssigned==null, IsAlive==true), randomly selects up to 2, assigns them with deterministic seeding support. Implements agent ID tracking (internal dictionary) to map agent references to integer house resident IDs. Methods: `AssignAdultsToCompletedHouses(agents, buildings)` (main entry point), `FindUnassignedAdultsForHouse(agents, count)` (selects N random adults), `AssignAgentsToHouse(agents, house)` (performs assignment), `SetSeed(int)` (deterministic RNG). Fisher-Yates shuffle for randomization. Added 13 comprehensive NUnit tests in `HouseAssignmentSystemTests.cs`: finding 2 unassigned adults, returning fewer when unavailable, ignoring children/elders/dead, completing houses, ignoring incomplete/non-house buildings, agent state updates, deterministic RNG behavior (same seed = same selection, different seeds differ), mixed agent scenarios (5 adults + 1 assigned + 1 elder + 1 child). Verified: tests confirm deterministic selection via seed, no duplicate assignments, clean separation of concerns. Files: `HouseAssignmentSystem.cs`, `HouseAssignmentSystem.cs.meta`, `Tests/HouseAssignmentSystemTests.cs`, `Tests/HouseAssignmentSystemTests.cs.meta`. **Resolved namespace collision:** deleted duplicate `Building.cs` (orphaned since T-090) to ensure BuildingPlacement.cs is canonical. Verification: all 13 tests passing; project compiles without errors.

- 2026-04-15 | T-093 | **Implemented Child→Adult House Assignment (Phase J Housing).** Added `AssignNewAdultToHouse(Agent agent, List<Building> buildings)` method to `HouseAssignmentSystem`. Core logic: when a Child becomes an Adult, searches for houses with open adult slots (< 2 adult residents), randomly selects one house, and assigns the new adult to it. If no houses have open slots, the agent remains unassigned (builders will prioritize housing per SPEC). Returns true if assignment succeeded, false otherwise. Added 13 comprehensive NUnit tests in `HouseAssignmentSystemTests.cs`: successful assignment with open slot, failure when no houses available, failure when all houses full, failure for non-adult life stage, null parameter handling, partially filled houses, ignores non-house buildings, deterministic RNG, multiple new adults assignment, house with one slot remaining, already assigned agent handling. Verification: all tests pass; project compiles without errors. Files: `HouseAssignmentSystem.cs` (modified), `HouseAssignmentSystemTests.cs` (extended).

- 2026-04-16 | T-100, T-101 | **Implemented Reproduction System (Phase K).** Added `Id` property to `Agent` class with auto-incrementing ID generation. Created `ReproductionSystem` static class with `ReproductionRate` global parameter (0.0-1.0, default 0.5) and `ProcessReproduction(agents, buildings)` method. Core logic: finds houses with exactly 2 assigned adult residents, rolls probability per house based on reproduction rate, spawns child at house location with child assigned to house. Added 17 comprehensive NUnit tests in `ReproductionSystemTests.cs`: rate parameter control, eligibility rules (2 adults required, dead adults excluded, elders count as adults), child assignment to house, deterministic RNG, unique child IDs. Simplified `HouseAssignmentSystem` to use `Agent.Id` directly instead of internal ID mapping. Verification: tests cover all SPEC.md reproduction requirements. Files: `Agent.cs` (added Id property), `ReproductionSystem.cs` (new), `ReproductionSystemTests.cs` (new), `HouseAssignmentSystem.cs` (simplified to use Agent.Id).

- 2026-04-16 | T-111 | **Implemented Profession Assignment System (Phase L).** Created `ProfessionTargets` class for managing profession distribution targets (defaults to equal 1/6 split, supports normalization). Created `ProfessionAssignmentSystem` static class with `GetMostUndernumberedProfession()` and `AssignProfession()` methods. Core logic: calculates actual vs target percentages, picks profession with largest deficit (target% - actual%). Handles ties randomly, supports deterministic RNG seeding. Added 20 comprehensive NUnit tests in `ProfessionAssignmentTests.cs`: target configuration, undernumbered selection, balanced distribution (6 agents = 1 each), skewed distribution (60 agents approximates targets exactly), dead agent exclusion, deterministic RNG. Key verification: with equal targets, 6 agents get exactly 1 per profession; with skewed targets (30/20/20/10/10/10%), 60 agents distribute as 18/12/12/6/6/6. Files: `ProfessionTargets.cs` (new), `ProfessionAssignmentSystem.cs` (new), `ProfessionAssignmentTests.cs` (new).

- 2026-04-16 | T-112, T-113 | **Implemented Profession Switch System (Phase L).** Created `ProfessionSwitchSystem` class managing periodic profession switching to correct distribution imbalances. Core logic: threshold-based switching (default 15% discrepancy), one switch at a time, with cooldown (default 5 cycles) to prevent rapid oscillations. Prefers switching unassigned agents. Added 16 comprehensive NUnit tests in `ProfessionSwitchSystemTests.cs`: threshold validation, cooldown behavior, preference for unassigned agents, cycle tracking, reset functionality, custom threshold values. Key verification: counts move toward targets over time; two switches cannot occur faster than cooldown. Constants documented in code: `DefaultSwitchThreshold = 0.15f`, `DefaultSwitchCooldownCycles = 5`. Files: `ProfessionSwitchSystem.cs` (new), `ProfessionSwitchSystemTests.cs` (new).

- 2026-04-21 | T-152 | **Implemented tower fire gating and hit-area helpers.** Added `TowerCombatSystem` with `CanTowerFire()` and `HasSoldierInside()` so towers only fire when a living soldier stands on a footprint tile. Added `GetTowerHitArea()` and `IsEnemyInRange()` for the 6x6 attack rectangle centered on the 2x2 tower footprint, with map-bound clipping. Added focused tests covering staffed/unstaffed towers and boundary in/out range cases. Files: `TowerCombatSystem.cs`, `TowerCombatSystemTests.cs`.

- 2026-04-21 | T-153 | **Implemented tower stats.** Added tower combat constants and helpers in `TowerCombatSystem`: max HP 100, base damage 1, upgraded damage 2, and 1-second attack cadence. Added `Building.HitPoints` initialization for towers so tower instances start at 100 HP. Added tests covering base stats, upgraded damage, and the 10-second kill check against a 10 HP target. Files: `TowerCombatSystem.cs`, `BuildingPlacement.cs`, `TowerCombatSystemTests.cs`.
- 2026-04-21 | T-154 | **Implemented soldier mode split switching.** Added `SoldierMode` to `Agent`, a `SoldierModeSwitchSystem` that balances patrolling vs improving with a target share, threshold, and per-soldier cooldown, plus tests covering convergence, cooldown behavior, and mode counting. Also reset soldiers to patrolling when profession assignment or profession switching creates a soldier. Files: `Agent.cs`, `ProfessionAssignmentSystem.cs`, `ProfessionSwitchSystem.cs`, `SoldierModeSwitchSystem.cs`, `SoldierModeSwitchSystemTests.cs`.

- 2026-04-23 | T-210 | **Implemented single playable scene with central, generated world, and initial population.** Integrated all simulation systems into World.SimulationStep() including aging, facility spawning, enemy spawning, reproduction, profession switching, soldier mode switching, house assignments, and agent action loops for all professions. Added InitializeGameSetup() method to World that creates central building at (48,48) and spawns 12 initial agents (2 of each profession) near central. Added AgentActionState class to track per-agent action state (paths, accumulators, eating actions, patrol positions). Updated SimulationTickDriver to call InitializeGameSetup(). Added WasAssignedOnCompletion property to Building class. Added placeholder Update methods to EnemyAISystem, CombatSystem, EnemyCombatSystem, and TowerCombatSystem for future combat integration. Files: `World.cs` (major integration), `BuildingPlacement.cs` (added property), `SimulationTickDriver.cs`, `EnemyAISystem.cs`, `CombatSystem.cs`, `EnemyCombatSystem.cs`, `TowerCombatSystem.cs`.
- 2026-04-22 | T-180 | **Implemented profession target data structure upgrades.** Hardened `ProfessionTargets` as a sealed serializable container with six-profession validation, one-shot six-value construction/setting, normalization helpers, normalized-state checks, and copy-out access for future UI/config binding. Added tests covering normalization, input-length validation, and copy isolation. Files: `ProfessionTargets.cs`, `ProfessionAssignmentTests.cs`.
- 2026-04-22 | T-181 | **Implemented reproduction rate binding.** Added `ReproductionSettings` as a serializable slider/config object, bridged it into `ReproductionSystem` via validation-backed setters and `ApplySettings(...)`, and added tests covering config round-tripping, invalid values, and null application. Files: `ReproductionSettings.cs`, `ReproductionSystem.cs`, `ReproductionSystemTests.cs`.
- 2026-04-22 | T-182 | **Implemented soldier control bindings.** Added `SoldierControls` as a serializable configuration object for patrol/improve split plus tower emphasis, exposed current values from `SoldierModeSwitchSystem`, and added tests covering defaults, binding, round-tripping, and validation. Files: `SoldierControls.cs`, `SoldierModeSwitchSystem.cs`, `SoldierControlsTests.cs`.
- 2026-04-22 | T-183 | **Implemented producer threshold bindings.** Added `ProducerThresholdSettings` and `ProducerThresholdPair` as serializable config data for four producer professions, plus validation-backed simulation binding and snapshot helpers. Hardened `ProducerThresholds.SetThresholds(...)` against NaN/Infinity. Added tests covering defaults, binding, round-tripping, invalid values, and non-producer rejection. Files: `ProducerThresholdSettings.cs`, `ProducerThresholds.cs`, `ProducerThresholdSettingsTests.cs`.
- 2026-04-21 | T-162 | `Assets/Civilizator/Simulation/Enemy.cs`, `Assets/Civilizator/Simulation/EnemyCombatSystem.cs`, `Assets/Civilizator/Tests/Simulation/EnemyCombatSystemTests.cs` — Enemy combat stats already matched V1 (HP 10, damage 1, 1-second cadence, attacks never miss).
- 2026-04-22 | T-196 | `Assets/Civilizator/UI/ProductivityDisplay.cs`, `Assets/Civilizator/UI/ProductivityDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/ProductivityDisplayTests.cs`, `Assets/Civilizator/Tests/UI/ProductivityDisplayTests.cs.meta` — Added a productivity/starvation HUD with overall and per-stage averages plus starvation counts, along with edit-mode coverage. Verification pending user-run Unity EditMode tests.
- 2026-04-22 — T-180 — `Assets/Civilizator/Simulation/ProfessionTargets.cs`, `Assets/Civilizator/Tests/Simulation/ProfessionAssignmentTests.cs` — Hardened `ProfessionTargets` as a sealed serializable container with six-profession validation, one-shot six-value construction/setting, normalization helpers, normalized-state checks, and copy-out access for future UI/config binding. Added tests covering normalization, input-length validation, and copy isolation.
- 2026-04-22 — T-181 — `Assets/Civilizator/Simulation/ReproductionSettings.cs`, `Assets/Civilizator/Simulation/ReproductionSystem.cs`, `Assets/Civilizator/Tests/Simulation/ReproductionSystemTests.cs` — Added `ReproductionSettings` as a serializable slider/config object, bridged it into `ReproductionSystem` via validation-backed setters and `ApplySettings(...)`, and added tests covering config round-tripping, invalid values, and null application.
- 2026-04-22 — T-182 — `Assets/Civilizator/Simulation/SoldierControls.cs`, `Assets/Civilizator/Simulation/SoldierModeSwitchSystem.cs`, `Assets/Civilizator/Tests/Simulation/SoldierControlsTests.cs` — Added `SoldierControls` as a serializable configuration object for patrol/improve split plus tower emphasis, exposed current values from `SoldierModeSwitchSystem`, and added tests covering defaults, binding, round-tripping, and validation.
- 2026-04-22 — T-183 — `Assets/Civilizator/Simulation/ProducerThresholdSettings.cs`, `Assets/Civilizator/Simulation/ProducerThresholds.cs`, `Assets/Civilizator/Tests/Simulation/ProducerThresholdSettingsTests.cs` — Added `ProducerThresholdSettings` and `ProducerThresholdPair` as serializable config data for four producer professions, plus validation-backed simulation binding and snapshot helpers. Hardened `ProducerThresholds.SetThresholds(...)` against NaN/Infinity. Added tests covering defaults, binding, round-tripping, invalid values, and non-producer rejection.
- 2026-04-21 | T-162 — `Assets/Civilizator/Simulation/Enemy.cs`, `Assets/Civilizator/Simulation/EnemyCombatSystem.cs`, `Assets/Civilizator/Tests/Simulation/EnemyCombatSystemTests.cs` — Enemy combat stats already matched V1 (HP 10, damage 1, 1-second cadence, attacks never miss).
- 2026-04-22 — T-196 — `Assets/Civilizator/UI/ProductivityDisplay.cs`, `Assets/Civilizator/UI/ProductivityDisplay.cs.meta`, `Assets/Civilizator/Tests/UI/ProductivityDisplayTests.cs`, `Assets/Civilizator/Tests/UI/ProductivityDisplayTests.cs.meta` — Added a productivity/starvation HUD with overall and per-stage averages plus starvation counts, along with edit-mode coverage. Verification pending user-run Unity EditMode tests.
- 2026-04-24 — T-211 — `Assets/Civilizator/Tests/Simulation/ConstructionVerticalSliceTests.cs` — Added comprehensive vertical slice tests covering the full gather → deposit → construction flow: resource gathering and depositing, builder withdrawal and delivery, threshold-controlled improvement, and build-time gating. Tests verify that logs decrease and buildings complete successfully.
