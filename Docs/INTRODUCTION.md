# Civilizator — Introduction

Civilizator is a civilization simulation game viewed in isometric perspective where the player acts as a **hands-off governor**. The player does not directly command individuals or click-to-order tasks; instead, the player shapes society using **policy-like controls** and watches the civilization adapt over time.

## Player role: indirect control

The player's influence is expressed through controls that guide how citizens behave:

- **Profession targets (%)**: the player sets desired population percentages for each profession. Citizens choose or switch professions to converge toward these targets over time.
- **Policy thresholds and priorities**: within each profession, the player sets high-level rules (for example, when to produce resources versus when to invest in improvements).
- **Societal tuning**: controls like reproduction rate influence population growth and long-term survival.

The design goal is that the player solves systems problems (balance, bottlenecks, resilience), not micromanagement problems.

## What makes the game fun

- **Readable cause and effect**: changing a control should quickly produce observable changes in behavior and outcomes.
- **Emergent outcomes**: simple individual rules create complex civilization behavior (shortages, surpluses, growth, collapse).
- **Strategic pressure**: periodic external threats force trade-offs between economy and defense.

## Simulation philosophy

- Citizens are autonomous agents with professions and basic needs.
- The world is a tile map; people move through it and physically perform work.
- Resources are tracked primarily as **stocks and rates**, not as individually simulated physical items.
- The UI must provide strong diagnostics so the player can answer “what’s happening and why?” quickly.

## Core systems (conceptual)

- **Population**: citizens age through life stages; productivity changes with age and conditions (for example, housing and starvation).
- **Professions**: citizens work in professions like gatherers (food/wood/ore), builders (infrastructure), and soldiers (defense).
- **Production vs improvement**: professions choose between producing goods now or improving long-term output by constructing/upgrading facilities.
- **Threats**: enemies spawn periodically and can damage citizens and structures, creating survival pressure.

## V1 focus (vertical slice)

V1 is intentionally small and explicit: tens of agents, real movement on a tile grid, a central storage building, finite natural resource nodes, a basic construction/upgrading model, minimal combat with towers and periodic enemies, and a minimal UI showing must-have metrics.

The detailed locked V1 rules live in `Docs/v1/SPEC.md`.

## Post-V1 direction (planned)

After V1 proves the core loop is fun and readable, future versions may add:

- A **monetary economy** (wages, private consumption, markets, inequality, investment)
- More professions and specialization
- Deeper objectives/achievements and longer-term progression (tech eras, monuments)
- More nuanced external threats and defenses
- Stronger planning/diagnostics UI (graphs, forecasts, bottleneck explanations)

