# Project Overview

- **Game Title:** Solar System AR Mobile Application
- **High-Level Concept:** A mobile AR application for exploring the solar system — viewing planets, rovers, spaceflight, and an educational quiz.
- **Players:** Single player
- **Inspiration / Reference Games:** Educational AR / solar-system explorers
- **Tone / Art Direction:** Realistic space / planetary
- **Target Platform:** Android
- **Screen Orientation / Resolution:** Mobile (handled at runtime by `MobileDisplayBootstrap`)
- **Render Pipeline:** Built-in

> This plan is a **maintenance / cleanup task**, not a gameplay feature. Goal: remove genuinely dead C# scripts **without breaking the project**, using a reversible quarantine-first workflow.

# Cleanup Scope & Rationale

## Investigation method (already completed, read-only)
1. Enumerated all 76 `.cs` files under `Assets/`.
2. Used `AssetDatabase.GetDependencies` across **all 474** scenes/prefabs/ScriptableObjects to find every script referenced by an asset (transitively).
3. Cross-checked **code-to-code references** by extracting the real **type names** from each candidate file (not filenames) and scanning all other `.cs` files.
4. Verified runtime-only usage patterns: `[RuntimeInitializeOnLoadMethod]`, `AddComponent<>()`, `MenuItem`, `SendMessage`, `Invoke`, `CustomEditor`, `ContextMenu`.

## Scripts CONFIRMED alive — explicitly protected (do NOT touch)
- `Scripts/Mobile/MobileDisplayBootstrap.cs` — auto-runs via `[RuntimeInitializeOnLoadMethod]`.
- `Scripts/Mobile/UICanvasFitter.cs`, `CameraFovFitter.cs`, `TouchDriveInput.cs`, `TouchArrowControls.cs` — added at runtime via `AddComponent<>()` from the bootstrap / each other. **Invisible in scenes but essential.**
- `Core/Scripts/AdvancedRoverController.cs`, `RoverController.cs` — referenced by the live `RoverSceneController` (and bootstrap).
- All other scripts referenced by scenes/prefabs (ARViewController, ARLauncher, ShipFlightController, QuizManager, etc.).

## Scripts to REMOVE — 11 confirmed dead (no asset refs, no live code refs)
1. `Assets/Core/Scripts/InfiniteMartianLandscape.cs` — dead root (only refers to SurfaceFollow).
2. `Assets/Scripts/SurfaceFollow.cs` — only referenced by the dead InfiniteMartianLandscape.
3. `Assets/Core/Scripts/InfinitePlanetTerrain.cs` — no references.
4. `Assets/Core/Scripts/RoverDataCollection.cs` — no references.
5. `Assets/Scripts/AtmosphereController.cs` — no references.
6. `Assets/Scripts/AtmosphericWindFollow.cs` — no references.
7. `Assets/Scripts/CharonOrbit.cs` — no references.
8. `Assets/Scripts/InfiniteOrbitCenter.cs` — no references.
9. `Assets/Scripts/SpaceShipHUD.cs` — dead root (only refers to SpaceShipSurfaceFlight).
10. `Assets/Scripts/SpaceShipSurfaceFlight.cs` — only referenced by the dead SpaceShipHUD.
11. `Assets/Scripts/Editor/TerrainGenerator.cs` — static utility, never called from anywhere.

## Explicitly EXCLUDED from removal (per user decision)
- **`Assets/Scripts/SceneHelpers.cs`** — contains `DataPointPulse` and `InputMapEnabler`. Both are currently orphaned/unusable because their class names do not match the filename (Unity cannot attach them). `InputMapEnabler` appears to have been **intended** to enable the "Player" input action map on Start. **Flagged for repair, not deletion** (see "Follow-up notes").
- **TextMesh Pro `Examples & Extras` (16 scripts)** — package sample content, left untouched per user decision. (They cross-reference each other, so partial deletion would break the rest anyway.)

# Removal Approach: Quarantine-First (reversible)

Instead of deleting immediately, move the 11 dead scripts (and their `.cs.meta` files) into a quarantine folder, let the user confirm the project still compiles and runs, then hard-delete in a follow-up pass.

### Critical constraint — Editor assembly
`TerrainGenerator.cs` lives under an `Editor/` folder and uses the `UnityEditor` namespace. It must be quarantined into a path that is **still under an `Editor/` folder** (`Assets/_Deprecated/Editor/`). Moving it to a plain runtime folder would compile it into a runtime assembly and cause build/compile errors referencing `UnityEditor`.

### Quarantine layout
```
Assets/_Deprecated/
├── Core/
│   ├── InfiniteMartianLandscape.cs (+ .meta)
│   ├── InfinitePlanetTerrain.cs (+ .meta)
│   └── RoverDataCollection.cs (+ .meta)
├── SurfaceFollow.cs (+ .meta)
├── AtmosphereController.cs (+ .meta)
├── AtmosphericWindFollow.cs (+ .meta)
├── CharonOrbit.cs (+ .meta)
├── InfiniteOrbitCenter.cs (+ .meta)
├── SpaceShipHUD.cs (+ .meta)
├── SpaceShipSurfaceFlight.cs (+ .meta)
└── Editor/
    └── TerrainGenerator.cs (+ .meta)
```

> Moves must use `AssetDatabase.MoveAsset` (preserves GUIDs) so that if any hidden reference existed, it would remain resolvable and the move would be trivially reversible.

# Key Asset & Context

- Files to move: the 11 `.cs` files listed above **plus their `.cs.meta` files**.
- New folder: `Assets/_Deprecated/` (with `Core/` and `Editor/` subfolders).
- No scene, prefab, ScriptableObject, or ProjectSettings changes are expected.

# Implementation Steps

### Step 1 — Create quarantine folders
- **Description:** Create `Assets/_Deprecated/`, `Assets/_Deprecated/Core/`, and `Assets/_Deprecated/Editor/`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** No

### Step 2 — Move the 11 dead scripts via AssetDatabase.MoveAsset
- **Description:** Move each of the 11 `.cs` files (GUID-preserving) to the layout above. `TerrainGenerator.cs` → `Assets/_Deprecated/Editor/`. Core scripts → `Assets/_Deprecated/Core/`. Others → `Assets/_Deprecated/`. Unity handles `.meta` automatically with `MoveAsset`.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No

### Step 3 — Recompile & verify no errors
- **Description:** Trigger a recompile (AssetDatabase.Refresh) and read the Console. Confirm **zero** new compile errors caused by the moves.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

### Step 4 — Runtime smoke check
- **Description:** Confirm the two key scenes still load/behave: `Assets/Scenes/Screens/IntroScreen.unity` and `Assets/Scenes/Screens/ARViewScreen.unity`. Verify no "missing script" warnings appear on their GameObjects.
- **Assigned role:** developer
- **Dependencies:** Step 3
- **Parallelizable:** No

### Step 5 — Hard-delete follow-up (only after user confirmation)
- **Description:** After the user confirms the build/play is healthy, permanently delete `Assets/_Deprecated/`. This is a **separate, explicitly-confirmed** step — not performed automatically.
- **Assigned role:** developer
- **Dependencies:** Step 4 + explicit user go-ahead
- **Parallelizable:** No

# Verification & Testing

- **Compile check:** Unity Console shows no new errors/warnings after Step 2 (Step 3).
- **Missing-script check:** No "The referenced script is missing" warnings in IntroScreen or ARViewScreen (Step 4).
- **Mobile cluster intact:** Confirm `MobileDisplayBootstrap` still auto-installs its components (it was NOT removed).
- **Rollback:** If any error appears, move the offending file back with `AssetDatabase.MoveAsset` (GUID preserved → reference restored instantly).

# Follow-up notes (not part of this cleanup)

- **`SceneHelpers.cs` / `InputMapEnabler`:** Currently non-functional because class name ≠ filename. If enabling the "Player" input action map at scene start is desired, this needs repair (rename file to match a single class, or split into properly-named files, then attach to a GameObject). Left for a separate task.
- **TextMesh Pro Examples & Extras:** Left in place. Can be removed later as a whole folder via Package Manager if desired.
