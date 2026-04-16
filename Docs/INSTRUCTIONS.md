# Instructions for AI Agents

This document provides general instructions for AI agents implementing tasks from implementation plans in this repository.

---

## Running Tests

**You cannot run tests autonomously.** When a task's **Verification** step requires tests to pass, you must **ask the user to run them and report results**.

### When to request test runs

- After implementing new code with test files
- Before marking a task done (if verification includes "tests pass")
- After fixing compilation errors

### Instructions to give the user

```
Please run the tests and tell me if they pass:

1. Open Unity Editor (6000.4.1f1)
2. Go to: Window → General → Test Runner
3. Click the "EditMode" tab
4. Click "Run all" to run all tests
5. Tell me if all tests show green checkmarks

If tests don't appear, try: Assets → Reimport All, then reload the Test Runner window.
```

### Test coverage by module

If the user reports test failures, they indicate:
- **GridPosTests**: Problem with position/Manhattan distance logic
- **BuildingPlacementTests**: Issue with overlap/gap rules or resource facility validation
- **WorldGeneratorTests**: World generation seeding or region distribution broken
- **SimulationClockTests**: Cycle advancement or delta time handling broken
- **PathfindingTests**: BFS pathfinder or occupancy blocking issue
- **GridOccupancyTests**: Tile passability or building footprint tracking broken
- **SimulationTickDriverTests**: Presentation driver timing issue

All tests run in EditMode only (no play mode tests yet).

---

## Suggesting Commit Messages

After finishing the implementation of one or more tasks, **suggest a short commit message to the user**. The commit message should start with the code(s) of the tasks that were implemented.

### Format

```
T-XXX, T-YYY: brief description of changes
```

### Examples

```
T-010, T-011: Add grid coordinates and map constants

T-020, T-021, T-022: Implement simulation clock with delta time support

T-090, T-091: Add house capacity rules and adult assignment on completion
```

### Guidelines

- List all task IDs that were completed in this commit, in numerical order
- Use a concise description (max ~72 characters for the subject line)
- Optionally add a body with more details if needed