# Family Settings Parity — UNITY EXECUTION PLAN

**Status: EXECUTING** (campaign started 2026-07-31 on `feat/family-parity`; execution log at the
bottom of this file). Originally written 2026-07-31; every anchor below verified against the working
tree on `master` HEAD `758b1588` **plus the uncommitted unified-settings pile** — see §1.1, that
pile is the baseline, not noise. **BASELINE DRIFT (M0 finding):** the §1.1 pile has since been
COMMITTED (`ab46dc53`) and release-staged as `0.13.0` (`f2728b43`, merged to dev+master via
PRs #224–#226) but **NOT published** (latest GitHub release: v0.11.0; no package tag; no Publish
run since 2026-07-25). Consequences: §1.1's "uncommitted" framing is history; §5 row 15's
`[Unreleased]` section is now the committed-but-unshipped `[0.13.0]` body; U-01's "never shipped
or committed" is now "committed, never shipped"; M6 must resolve with the owner whether the
campaign reshapes the staged 0.13.0 in place or targets 0.14.0.
**Family contract:** §0 below is the normative embed of the family parity contract (owner rulings
2026-07-31). Sibling legs carry the same section; substance is family-frozen. This plan MAY add
Unity detail; it MAY NOT contradict §0. Any conflict = STOP AND ASK the owner.
**Reference-implementation rule:** THIS repo is the family reference. The campaign's job is
**exposure + cleanup** — making the reconciler's existing behavior configurable and restoring
lost diagnostics semantics — **never behavior change to the reconciler itself**. If a task seems
to require changing what `FiberReconciler` *does* (ordering, effects, bailout, commit), stop: you
misread the task.
**Release target:** Unity package **0.12.0 → 0.13.0** (`package.json:4`), one minor, folding in
the already-implemented (uncommitted) unified-settings work it reshapes.
**Branch:** create `feat/family-parity` off the current branch. Never push, never tag, never add a
`Co-Authored-By` trailer, never commit unless the milestone says so and the owner has not said
otherwise.

Every decision is pre-made below. All verification gates are HARD — do not close a milestone with
any gate red.

---

## 0. FAMILY PARITY CONTRACT (normative — owner rulings 2026-07-31; do NOT re-litigate)

Reference implementation: **this repo (ruitk-unity)**. Sibling legs port these semantics; this leg
defines them. Rulings:

- **All settings ship into builds.** Defaults are off/production — an untouched build behaves as
  today (the two sanctioned exceptions are recorded in the decision log, §10: the hook-validation
  release flip and the no-store environment default).
- **`strict_mode` on every leg defaults OFF**, is opt-in, and is **force-off in release builds**.
- **NO UI Toolkit pooling.** `Shared/Elements/Pools/GlobalVisualElementPool.cs` was removed
  deliberately — commit `a05f5c07` (2025-11-17, "removed cache and pool GlobalVisualElementPool";
  landed via merge `e54ccf54` 2025-10-24, removed from mainline via merge `baf2797b`): generic
  `VisualElement` reset = state bleed. Only **adapter-gated** pooling — the uGUI pattern where the
  element joins the pool only if its adapter's `TryResetForPool` provably restores the pristine
  `Create()` state (`Ugui/Core/UguiHostConfig.cs:21-26`) — is sanctioned.
- **`exceptionControlFlow` stays removed.** It was the selector for a legacy error-boundary
  strategy; the feature itself survived unconditionally (verified: `DiagnosticsConfig.
  UseExceptionBoundaryFlow` is written at three mount sites and **read by nothing** — §5). The
  knob is a lie today; it dies in this campaign (§5) and is never revived.
- **`Basic` trace level is RESTORED to its legacy meaning: structural events.** It was lost by
  accident in the fiber rewrite. Evidence: legacy `Shared/Core/Reconciler.cs:1811` gated
  `EnableDiffTracing || TraceLevel != DiffTraceLevel.None` on the `[ReplaceNode]` structural log,
  vs `EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose` on the `[Diff]` detail logs
  (`:1669, :1922, :2142, :2161`) — file deleted whole in commit `2d8b50a7` ("removed all legacy
  code", 2025-12-14). Today `TraceLevel.Basic` parses (`Shared/Core/Config/RuitkConfig.cs:115-117`)
  but **no code path anywhere checks for it** — every gate is `== Verbose` or `!= None`.
- **Pool caps stay per-leg constants** (`UguiHostConfig.cs:25` `PoolCapacityPerType = 128`) — the
  on/off knob is canonical, the capacity is not.

### 0.1 Canonical knobs (identical names/semantics/defaults across all legs)

| # | Knob | Type | Default | Unity anchor today | What changes here |
|---|---|---|---|---|---|
| 1 | `time_slicing` | bool | `true` | `Shared/Core/Fiber/FiberReconciler.cs:363-373` — "no scheduler installed" is currently the ONLY synchronous path | `false` = explicit scheduler bypass → synchronous `WorkLoop()` even when a scheduler exists (U-04) |
| 2 | `time_slice_ms` | float | `2.0` | `private const float TimeSliceMs = 2.0f;` `FiberReconciler.cs:31`, consumed `:450` | const → setting (U-04) |
| 3 | `frame_budget_ms` | float | `4.0` | `Runtime/Core/RenderScheduler.cs:19-20` `[SerializeField] frameBudgetMs = 4.0f`, read `:152, :177` — serialized-but-unreachable (the component is only ever `AddComponent`-created at runtime: `Runtime/Core/RootRenderer.cs:52-56`, `Ugui/Core/UguiRootRenderer.cs:44-48`) | field ← setting at `Awake` (U-04) |
| 4 | `host_node_pool` | bool | `true` | uGUI pool always-on: acquire `UguiHostConfig.cs:55-63`, release `:221-235` | gates the uGUI pool; the UITK path stays unpooled — this knob does NOT create a UITK pool (U-05) |
| 5 | `hook_validation` | tri-state | `auto` | `Shared/Core/Hooks.cs:21` `EnableHookValidation = true` in ALL builds | **the flip**: auto = editor/dev ON, release OFF. `EnableHookAutoRealign` (`Hooks.cs:24`) stays as-is, internal, untouched (U-06) |
| 6 | `strict_diagnostics` | tri-state | `auto` | `Hooks.cs:22` `EnableStrictDiagnostics = false` | becomes auto (same mapping as #5). Warnings already implemented: state-update-during-render `Hooks.cs:156-161` + `:603-608`, missing-deps `:551-575`, funnel `WarnStrict` `:523-549`. FIX the misleading `[Hooks][StrictMode]` prefix → `[Hooks][Strict]` (3 sites: `:160, :573, :607`) (U-06) |
| 7 | `strict_mode` | bool | `false`, force-off in release | does not exist | ADD: double-invoke render functions, first result discarded, effects NOT double-invoked, diagnostics count the render once. Semantics reference: `ruitk-unreal/Plugins/ReactiveUIToolkit/Source/RuitkCore/Private/RuitkReconciler.cpp:565-570` (`RunOnce(); if (IsStrictModeEnabled()) Result = RunOnce();`). Unity insertion point: the single render call `Shared/Core/Fiber/FiberFunctionComponent.cs:160-163` (U-07) |
| 8 | `trace_level` | enum `none/basic/verbose` | `none` | enum exists `Shared/Core/Diagnostics/DiagnosticsConfig.cs:11-16`; `Basic` is dead (see §0 ruling) | RESTORE Basic = structural events, mapped to the fiber reconciler's placement/deletion/commit sites (the `FiberConfig.EnableFiberLogging` sites `FiberReconciler.cs:1172-1216` are the natural structural-log locations). Verbose = Basic + per-element/per-hook detail (existing Verbose sites). Full mapping table §6 (U-08) |
| 9 | `diff_tracing` | bool | `false`, **INDEPENDENT** | wrongly AND-ed with trace level in 3 element adapters: `Shared/Elements/RadioButtonElementAdapter.cs:78-80`, `RadioButtonGroupElementAdapter.cs:130-132`, `ToggleElementAdapter.cs:80-82` (`EnableDiffTracing && CurrentTraceLevel != None`; legacy semantics were OR — `Reconciler.cs:1669` et al.) | restore independence; wire to the real fiber diff layer, absorbing `FiberConfig.EnableFiberLogging` (`Shared/Core/Fiber/FiberConfig.cs:11` — verified set by NOTHING anywhere), which becomes internal to this knob (U-08) |
| 10 | `environment` | enum `auto/development/production` | `auto` | `Shared/Core/Config/RuitkSettings.cs:11-16, :38, :80-93` — already correct | keep; storage moves with the rest (U-01) |

Leg-specific extras MUST be marked **"(Unity-only)"** wherever they surface (schema, window, docs).
Unity's one extra: `diagnostics_output_folder` (consumed by
`Shared/Core/Config/RuitkDiagnosticsPaths.cs:34`).

---

## 1. Where Unity starts (verified 2026-07-31 — trust it, re-verify only what you touch)

### 1.1 The baseline INCLUDES the uncommitted unified-settings pile

The working tree carries the settings campaign of 2026-07-30, implemented and green but
**uncommitted**. It is the floor this plan builds on — do not revert it, do not commit it
separately, do not "clean it up" first. The relevant untracked/modified files:

- `Shared/Core/Config/RuitkSettings.cs` (untracked) — the ScriptableObject store this plan
  **replaces** (U-01). Fields: `environment :38`, `traceLevel :41`, `diffTracing :44`,
  `exceptionControlFlow :46-47`, `diagnosticsOutputFolder :55`; `ActiveOrNull :63`;
  `ResolveEnvironmentLabel :80-93`.
- `Editor/RuitkSettingsBootstrap.cs` (untracked) — asset discovery + `CreateSettingsAsset`; **to
  be deleted** (U-01).
- `Editor/RuitkSettingsBuildInjection.cs` (untracked) — Preloaded-Assets build hook; **to be
  deleted** (U-01; the JSON store needs no injection — `Resources/` ships by itself).
- `Editor/RuitkSettingsWindow.cs` (untracked) — the unified window; **kept and retyped** over the
  JSON (U-02). Sections: Configuration (`:62-219`), HMR (`:241-306`), Console navigation
  (`:382-403`); read-only no-store block `:86-121`; create-on-demand button `:116-120`; Browse
  picker `:170-195`.
- `Shared/Core/BuildDefinesConfig.cs` (modified) — the four bootstrap resolvers (`:15-53`), each
  `RuitkSettings.ActiveOrNull → RuitkConfig.Current → compiled default`. This resolver SHAPE is
  kept; the first hop changes store (U-01) and four new resolvers join it (U-04..U-07).
- `Shared/Core/Config/RuitkConfig.cs` (modified) — the LEGACY `Assets/ReactiveUIToolkit/config.json`
  `envVariables` fallback reader (path derivation `:102-106`). Stays, as fallback hop 2, minus the
  `exceptionControlFlow` field (§5).
- `Shared/Core/Config/RuitkDiagnosticsPaths.cs` (untracked) — output-root resolution; consumer of
  `RuitkSettings.ActiveOrNull` (`:34`); keeps working through the storage swap.
- `CHANGELOG.md` (modified) — an `[Unreleased]` section describing the ScriptableObject design.
  **Rewritten in M6** (the asset never shipped; describe only what 0.13.0 actually ships).
- Also in the pile, NOT this campaign's to touch beyond what §5/§7 name: the generator disk-scan
  fix + `SourceGenerator~/Tests/PackageLayoutDiscoveryTests.cs`, machine-path gate files, `.vscode/`,
  publish-menu removal, docs page edits.

### 1.2 Bootstrap seams (where resolved settings are APPLIED — all three mount surfaces)

| Surface | File | Resolver-apply block today |
|---|---|---|
| UITK runtime (`MonoBehaviour`) | `Runtime/Core/RootRenderer.cs` | `EnsureSetup` `:44-75` (env `:63`, trace `:66`, diff `:67`, exception `:68-69`, internal-logs-from-Verbose `:72-73`) |
| uGUI runtime | `Ugui/Core/UguiRootRenderer.cs` | `EnsureSetup` `:40-69` (same shape; exception `:64-65`, internal-logs `:67-68`) |
| Editor mounts | `Editor/EditorRootRendererUtility.cs` | `Mount` `:35-70` (exception `:57-58`, internal-logs `:60-61`) |

New knobs are read at these same seams, bootstrap-style, like the existing keys. Note the editor
surface uses `EditorRenderScheduler` (`Editor/EditorRenderScheduler.cs`), which has **no frame
budget at all** — `ExecuteQueue` (`:159-173`) drains every queue fully each editor update. See
U-04 for the decision.

### 1.3 Trace/diagnostics sites (complete inventory — §6 maps each to its new gate)

- `FiberConfig.EnableFiberLogging` consumers, ALL in `Shared/Core/Fiber/FiberReconciler.cs`:
  `:1136` (apply typed props), `:1154` (apply props), `:1172` (no props), `:1187` (InsertBefore),
  `:1199` (AppendChild), `:1209` (no host parent), `:1291` (CommitUpdate Label old/new text dump).
  The flag is declared `FiberConfig.cs:11` and **never assigned anywhere** — a compile-time-only
  debug knob.
- Commit-phase methods (structural-event homes): `CommitRoot :784`, `CommitDeletions :916`,
  `CommitWork :943`, `CommitPlacement :1094`, `CommitDeletion :1370` (all `FiberReconciler.cs`).
- `InternalLogOptions.EnableInternalLogs` (`Shared/Core/Diagnostics/InternalLogOptions.cs:12`) —
  set from `CurrentTraceLevel == Verbose` at the three mount seams (§1.2); consumers:
  `Shared/Elements/BaseElementAdapter.cs:112`, `Shared/Core/Hooks.cs:219, :247, :638, :665`.
- Direct Verbose checks: `Hooks.cs:1241` (UseEffect capture log),
  `Editor/EditorRenderScheduler.cs:111-114` (queue depths), `:133-136` (effect flush).
- The three AND-bugged adapter sites (§0.1 row 9).

### 1.4 Verification infrastructure (what "green" means here)

- **Engine-free gates** (run from the repo root, no Unity needed):
  ```bash
  node scripts/check-machine-paths.mjs      # machine-local path gate (CI: a step of test.yml's `gates` job)
  node scripts/corpus-hash.mjs --check      # family corpus (untouched by this campaign — must STAY green)
  ```
- **Compile harness — the host Unity project.** This package is consumed as a UPM `file:`
  dependency by a host project whose `Packages/manifest.json` contains
  `"com.reactiveuitoolkit": "file:…/ruitk-unity"` (+ `testables`). The host project's location is
  a MACHINE FACT — derive it (find the manifest naming this checkout among the owner's project
  roots; if not found, ask the owner), never write it into a tracked file. All Unity compile
  gates are `dotnet build` runs against the host project's generated csprojs — **VERIFY-UNITY**:
  ```bash
  # run from the HOST PROJECT root; ls *.csproj first — Unity regenerates these
  dotnet build Ruitk.Shared.csproj -v q --nologo      # engine core — 0 errors
  dotnet build Ruitk.Runtime.csproj -v q --nologo
  dotnet build Ruitk.Ugui.csproj -v q --nologo
  dotnet build Ruitk.Editor.csproj -v q --nologo
  dotnet build Ruitk.Samples.csproj -v q --nologo
  dotnet build Ruitk.Diagnostics.csproj -v q --nologo
  ```
  Verified 2026-07-31: the `Ruitk.*` csproj set exists in the host project alongside a STALE
  `ReactiveUITK.*` set from before the 0.12 rename — never build the stale set.
- **Player-assembly proof** (required for every milestone that touches `Shared/`, `Runtime/`, or
  `Ugui/`): build the `.Player` variant of each touched runtime assembly (e.g.
  `Ruitk.Shared.Player.csproj`). Verified 2026-07-31: **`Ruitk.*.Player.csproj` do not currently
  exist** (only stale `ReactiveUITK.*.Player.csproj`). At M0, ask the owner to enable player-csproj
  generation (Edit ▸ Preferences ▸ External Tools) and regenerate, or to run a regeneration
  headlessly; if neither is possible, STOP AND ASK — do not substitute the stale set, and do not
  skip the proof (a stray `UnityEditor` reference outside `#if UNITY_EDITOR` is exactly what this
  gate exists to catch).
- **THE OWNER MAY HAVE THE UNITY EDITOR OPEN.** Never launch Unity in batchmode against the host
  project (single-instance lock; `CICD/Editor/AssetStoreExport.cs:17-23` documents the batchmode
  pattern for CI, not for this). The `dotnet build` harness above is the whole point: it compiles
  the same csprojs without touching the running editor. In-editor verification is the owner's
  (M8).
- **SG/LSP suites** (`dotnet test SourceGenerator~/Tests/…`, `ide-extensions~/lsp-server/Tests/…`)
  are UNTOUCHED by this campaign unless a milestone touches `SourceGenerator~`/`language-lib`
  (none does). Run them once at M0 for a green floor and once at M7; anything red that this
  campaign didn't cause = pre-existing, record and continue.
- **Functional smoke — the settings-campaign pattern**: resolution-order proof by seeding each
  layer and asserting the resolved value (§4 M1 test spec), plus the owner-run window smoke (M8).

---

## 2. Engine-local decisions (U-01..U-09) — pre-made; do not improvise

**U-01 — STORAGE REWORK: plain JSON in `Resources`, ScriptableObject stack deleted.**
The store becomes a project-owned JSON file at **`Assets/Resources/ReactiveUIToolkit/config.json`**
in the CONSUMER project (never inside this package): under `Resources/` it ships into every player
build automatically, and `Resources.Load<TextAsset>("ReactiveUIToolkit/config")` is synchronous on
all platforms. **Created on demand only** by the settings window's create button — never
auto-dropped into a user's project (the TMP/DOTween lesson; same rule the SO campaign followed,
`RuitkSettingsWindow.cs:86-121`).
- **DELETE:** `Editor/RuitkSettingsBootstrap.cs` (+`.meta`), `Editor/RuitkSettingsBuildInjection.cs`
  (+`.meta`) — no discovery (fixed path), no Preloaded-Assets injection (Resources ships itself).
- **REWRITE `Shared/Core/Config/RuitkSettings.cs` in place** (same file, same class name, same
  assembly): from `ScriptableObject` to a plain serializable settings model + static loader.
  Keep the consumer-facing surface so `BuildDefinesConfig` and `RuitkDiagnosticsPaths.cs:34`
  need only mechanical edits: `RuitkSettings.ActiveOrNull` remains the "store or null" accessor
  (null = no JSON file ⇒ fall through to legacy config ⇒ compiled defaults), now backed by a
  cached `Resources.Load` + parse, with an explicit `Invalidate()` for the editor window to call
  after writes. No `UnityEditor` references (player assembly — the proof gate catches this).
- **Resolution order (unchanged shape, new first hop):** JSON store → legacy
  `Assets/ReactiveUIToolkit/config.json` `envVariables` (`RuitkConfig`, `:102-106`) → compiled
  defaults. All resolvers stay on `BuildDefinesConfig`.
- **Parsing:** `JsonUtility.FromJson` into a DTO whose fields are INITIALIZED to the §3 defaults —
  JsonUtility leaves absent fields at their initializers, which is exactly missing-key = default.
  Tri-states and enums are lowercase strings (`""` ⇒ default); unknown keys are ignored by
  JsonUtility (forward compat). The parse core takes a `string` (not a path) so it is unit-testable
  without asset plumbing.
- **The window (U-02) is the only writer.** It writes the FULL canonical schema (§3, all keys
  explicit, 2-space indent, trailing newline) via `File.WriteAllText` +
  `AssetDatabase.ImportAsset`, then `RuitkSettings.Invalidate()`.
- **Migration story:** the ScriptableObject asset was never shipped or committed — drop silently;
  one changelog sentence (M6). The owner's host project holds a smoke-test
  `Assets/ReactiveUIToolkitSettings.asset` — flag it for deletion during the M8 smoke. The legacy
  `config.json` fallback keeps store customers with an edited file working, unchanged.

**U-02 — The settings window becomes a typed editor over the JSON.** `Editor/RuitkSettingsWindow.cs`
keeps its shell: three sections, the no-store read-only "effective values" view (`:86-121`
pattern), "Create settings file" (writes the full §3 schema at the U-01 path, creating
`Assets/Resources/ReactiveUIToolkit/` as needed), the Browse picker with project-relative
normalization (`:170-195`), HMR + Console sections untouched. The `SerializedObject` plumbing
(`:123-197`) is replaced by: parse JSON → typed controls (`EnumPopup` for environment/trace_level,
`Popup` for the two tri-states, `Toggle`/`FloatField` for the rest, each labeled with its §0.1
semantics; Unity-only keys suffixed "(Unity-only)") → on change, rewrite the file (U-01 writer).
Show the file path + a Select button (ping the TextAsset). The exceptionControlFlow rows
(`:105-108`, `:147-153`) die in §5.

**U-03 — `BuildDefinesConfig` grows one resolver per knob** (same shape as `:15-53`):
`ResolveTimeSlicing`, `ResolveTimeSliceMs`, `ResolveFrameBudgetMs`, `ResolveHostNodePool`,
`ResolveHookValidation`, `ResolveStrictDiagnostics`, `ResolveStrictMode`; existing
`ResolveEnvironment`/`ResolveTraceLevel`/`ResolveEnableDiffTracing` re-point their first hop to the
JSON store; `ResolveExceptionBoundaryFlow` is deleted (§5). Legacy-fallback note: `RuitkConfig`
only ever carried `env/traceLevel/diffTracing` (+ the dying key) — the NEW knobs have no legacy
hop; their chain is JSON → compiled default. Tri-state mapping (`auto`):
`Application.isEditor || Debug.isDebugBuild ? on : off`. `strict_mode` force-off:
`ResolveStrictMode()` returns `false` whenever `!Application.isEditor && !Debug.isDebugBuild`,
regardless of the stored value — release players cannot opt in.

**U-04 — Reconciler knob exposure (no behavior change at defaults).**
- `time_slice_ms`: delete the const `FiberReconciler.cs:31`; add
  `public static float TimeSliceMs = 2.0f;` to `FiberConfig` (`Shared/Core/Fiber/FiberConfig.cs`),
  consume at `FiberReconciler.cs:450`. Set from `ResolveTimeSliceMs()` at the three §1.2 seams.
- `time_slicing`: add `public static bool TimeSlicingEnabled = true;` to `FiberConfig`. At
  `FiberReconciler.cs:363-373` the dispatch becomes: scheduler present AND `TimeSlicingEnabled` →
  `ScheduleRootWork` (sliced, unchanged); otherwise → `WorkLoop()` (the existing synchronous
  path, `:380-400`). This is the contract's "explicit bypass": today "no scheduler installed" is
  the only sync route; the knob makes the bypass first-class without touching either loop's body.
- `frame_budget_ms`: in `RenderScheduler.Awake` (`Runtime/Core/RenderScheduler.cs:33-43`), after
  the singleton guard, `frameBudgetMs = BuildDefinesConfig.ResolveFrameBudgetMs();`. Keep the
  `[SerializeField]` (harmless; the resolver wins for the runtime-created instance).
- **Editor scheduler stays unbudgeted BY DESIGN** (investigated): `EditorRenderScheduler` has no
  budget field and drains fully every `EditorApplication.update` (`:159-173`) — editor preview
  favors immediacy, HMR depends on prompt flushes, and no owner ask exists to change it.
  `frame_budget_ms` therefore applies to play-mode/player `RenderScheduler` only; `time_slicing` /
  `time_slice_ms` apply everywhere a scheduler slices (`ProcessWorkUntilDeadline`,
  `FiberReconciler.cs:429-472`). Document this Unity-only note in the window tooltip + docs (§7).
- Defaults leave every path byte-equivalent to today: `true/2.0/4.0` reproduce current behavior
  exactly.

**U-05 — `host_node_pool` gates the uGUI pool only.** `UguiHostConfig` reads
`BuildDefinesConfig.ResolveHostNodePool()` ONCE in its constructor into a `readonly bool
_poolEnabled` (bootstrap-read discipline — no per-frame resolver calls). Gate the acquire
(`UguiHostConfig.cs:55-63` — skip pool lookup, go straight to `adapter.Create()`) and the release
(`:221-235` — skip `TryResetForPool`, `DestroySafely` directly). `PoolCapacityPerType` (`:25`)
stays a per-leg constant per §0. The UITK host path gains NOTHING — no pool, no flag, per the §0
ruling.

**U-06 — Hook validation + strict diagnostics.** At the three §1.2 seams:
`Hooks.EnableHookValidation = BuildDefinesConfig.ResolveHookValidation();` and
`Hooks.EnableStrictDiagnostics = BuildDefinesConfig.ResolveStrictDiagnostics();`. The compiled
initializers (`Hooks.cs:21-22`) stay as-is (`true`/`false`) — they only matter before first mount,
and pre-mount there are no hooks; the resolver overwrites at every mount. Net effect = the
contract's flip: release players resolve `auto` → OFF. `EnableHookAutoRealign` (`:24`) is
untouched, internal, and NOT in the schema. Prefix fix: the three `[Hooks][StrictMode]` message
sites (`:160, :573, :607`) become `[Hooks][Strict]` — these are strict-DIAGNOSTICS warnings and
the old prefix collides with knob #7's name.

**U-07 — strict_mode double-invoke.** Insertion: `FiberFunctionComponent.cs:160-163`, the single
`wipFiber.TypedRender(...)` call. Shape (mirroring
`ruitk-unreal/Plugins/ReactiveUIToolkit/Source/RuitkCore/Private/RuitkReconciler.cpp:565-570`):
when `FiberConfig.StrictModeEnabled` (new static, set from `ResolveStrictMode()` at the §1.2
seams), invoke the render function twice; the FIRST result is discarded, the SECOND is the one
reconciled. Rules, all load-bearing:
- **Per-render state must be re-prepared between invokes**: hook cursors and the context-dep clear
  (`FiberFunctionComponent.cs:130-136` and the state-reset code immediately above the render call)
  run before EACH invoke — extract the existing prep into a local and call it twice; do not
  duplicate the code.
- **Effects are NOT double-invoked**: hook effect registration is index-keyed and overwrites in
  place (`Hooks.cs:1230-1239` pattern), so the second pass replaces the first's captures — verify,
  don't assume, for EVERY hook family (state/effect/layout-effect/memo/callback/ref/context) in
  the M4 tests. Effects run at commit, which happens once.
- **Diagnostics count the render once**: any per-render counter/metric/trace incremented inside
  the render path must reflect one logical render — audit `_workUnitCount`/metrics
  (`FiberReconciler.cs:33-39`), hook-order priming (`FiberFunctionComponent.cs:172-174` — priming
  after the second invoke is correct and unchanged), and the §6 trace sites (log once, on the
  counted invoke).
- **The discarded tree**: `VirtualNode` is pooled (`__Rent`). Investigate the existing recycle
  path at execution time; if a safe explicit release exists, release the discarded tree, else
  document the per-render garbage as strict-mode-only cost. Acceptance: the Ugui stress suite
  (`Ugui/Tests/UguiStressChurnTests.cs`) green with strict_mode forced on (M4 test spec).
- **Force-off in release** is U-03's resolver job, not a `#if` — the code path compiles into
  players but cannot activate.
- MaxRenderDepth guard (`:144-155`): the double-invoke must not double-count depth — one logical
  render increments `s_renderDepth` once (the two invokes happen within it).

**U-08 — Trace ladder + diff_tracing rewire.** Full site mapping in §6. Principles:
- `trace_level` drives TWO derived flags at the §1.2 seams (plus everywhere §6 says inline):
  `Basic` ⇒ structural logging on; `Verbose` ⇒ structural + detail
  (`InternalLogOptions.EnableInternalLogs` becomes `>= Basic`? NO — it is per-hook/per-element
  DETAIL, so it stays `== Verbose`; §6 rows are authoritative).
- Structural events (Basic): placement (`InsertBefore :1187-1196`, `AppendChild :1199-1204`),
  deletion (`CommitDeletion :1370` — add the log, none exists), the no-host-parent anomaly
  (`:1209-1215`), and a one-line commit summary in `CommitRoot :784` (counts already tracked:
  `_commitCount`, `_effectsCommitted`). Gate: `DiagnosticsConfig.CurrentTraceLevel != None`.
  Replacement is deletion+placement in the fiber model — the two logs above cover the legacy
  `[ReplaceNode]` semantics.
- Diff detail (`diff_tracing`, independent): props application (`:1136-1141, :1154-1160,
  :1172-1177`), `CommitUpdate` old/new dump (`:1291-1320+` — and DROP the `== "Label"` filter?
  NO: keep the site's existing behavior, just re-gate it; widening the dump is behavior change),
  and the three element adapters (fix `&&` → drop the trace-level term entirely: gate on
  `EnableDiffTracing` alone — that is the "independent" ruling; the legacy OR also let Verbose
  alone light these, which §6 preserves by ALSO gating them on `== Verbose`, i.e.
  `EnableDiffTracing || CurrentTraceLevel == Verbose`, the exact legacy expression).
- `FiberConfig.EnableFiberLogging` DIES as public API: delete the property (`FiberConfig.cs:11`),
  replace every consumer per §6. Nothing sets it today (verified), so no caller breaks.
  `ShowReconcilerInfo` (`FiberConfig.cs:16`) is dead too but is a §9 decision item, not this
  campaign's.
- `DiagnosticsConfig.EnableDiffTracing` (`DiagnosticsConfig.cs:26`) remains the runtime flag;
  `RuitkConfig` legacy parsing (`:73`) and the resolver chain keep feeding it.

**U-09 — Schema/docs/changelog discipline.** §3 is the schema; §7 the sync surface. The
`[Unreleased]` changelog section is REWRITTEN, not appended to (it describes unshipped work this
campaign reshapes). House changelog style per `CHANGELOG.md` top entry + `scripts/changelog.mjs
verify` if touched-lanes require; Discord entry per the `discord-changelog` skill (ASCII, ≤2000
chars) staged in `Plans~/DISCORD_CHANGELOG.md` at M7, shipped by the owner at release.

---

## 3. The JSON schema (canonical; the window always writes ALL keys)

```json
{
  "environment": "auto",
  "time_slicing": true,
  "time_slice_ms": 2.0,
  "frame_budget_ms": 4.0,
  "host_node_pool": true,
  "hook_validation": "auto",
  "strict_diagnostics": "auto",
  "strict_mode": false,
  "trace_level": "none",
  "diff_tracing": false,
  "diagnostics_output_folder": ""
}
```

- Keys are the §0.1 canonical snake_case names, identical across legs;
  `diagnostics_output_folder` is **(Unity-only)** and must be labeled so in window + docs.
- Enum/tri-state values are lowercase strings: `environment` ∈ `auto|development|production`;
  `hook_validation`/`strict_diagnostics` ∈ `auto|on|off`; `trace_level` ∈ `none|basic|verbose`.
  Parsing is case-insensitive, unknown value ⇒ default + one editor-only warning.
- Missing key ⇒ default (DTO initializers, U-01). Unknown keys ⇒ ignored.
- File absent ⇒ `RuitkSettings.ActiveOrNull == null` ⇒ legacy `config.json` hop ⇒ compiled
  defaults. An untouched project has NO file and behaves per the defaults column.
- Platform notes: `Resources.Load<TextAsset>` is synchronous everywhere Unity runs (the `.json`
  extension imports as TextAsset); no streaming-assets async, no per-platform path logic, WebGL
  included. The load happens once, cached, at first resolver call per domain.

---

## 4. Milestones

House rules for EVERY milestone: re-verify the anchors you are about to edit (the tree moves);
develop; extend tests IN the milestone; run the milestone's verify block; NEVER weaken an existing
test/gate to get green (if a gate seems wrong — STOP AND ASK); commit at milestone end with
`feat(parity): M<n> — <summary>` ONLY if the owner's no-auto-commit standing rule has been lifted
for this campaign — otherwise leave the work uncommitted and note milestone completion in the
final report. No push, ever.

**VERIFY-GATES** (every milestone, engine-free, repo root):
```bash
node scripts/check-machine-paths.mjs
node scripts/corpus-hash.mjs --check
```

### M0 — Baseline audit (no product code)
1. `git status --short` — expect exactly the §1.1 pile (plus this plan file). Anything else dirty:
   STOP AND ASK.
2. Locate the host project (§1.4). `ls *.csproj` there; confirm the `Ruitk.*` set. Ask the owner
   to produce `Ruitk.*.Player.csproj` (player-csproj generation) — record the answer; if
   unavailable this session, the player proof runs on whatever milestone first touches `Shared/`
   and MUST be resolved by then.
3. Run VERIFY-GATES + VERIFY-UNITY + both `~`-world suites (SG, LSP) for the green floor. Record
   totals. Any red — STOP AND ASK (do not build on a red base).
4. Create branch `feat/family-parity`.

Gate: everything green; findings recorded at the top of the working notes.

### M1 — exceptionControlFlow removal (small, self-contained, shrinks every later surface)
Execute the §5 table top to bottom. Definition of done: `grep -ri "exceptionControlFlow\|
UseExceptionBoundaryFlow\|ResolveExceptionBoundaryFlow" --include="*.cs"` over `Shared/ Runtime/
Ugui/ Editor/ Diagnostics/ Samples/ CICD/` returns ZERO hits; the docs/changelog rows are edited
per their table verdicts.

Gate: VERIFY-GATES + VERIFY-UNITY green; player proof for `Ruitk.Shared.Player` +
`Ruitk.Runtime.Player` + `Ruitk.Ugui.Player` (first `Shared/` touch — M0 step 2 must be resolved).

### M2 — Storage rework (U-01, U-02, U-03 for the EXISTING keys)
1. Rewrite `Shared/Core/Config/RuitkSettings.cs` per U-01 (model + loader + `Invalidate`).
2. Delete `Editor/RuitkSettingsBootstrap.cs`, `Editor/RuitkSettingsBuildInjection.cs` (+metas).
3. Retype `Editor/RuitkSettingsWindow.cs` per U-02 (existing keys only: environment, trace_level,
   diff_tracing, diagnostics_output_folder — the new knobs join in their own milestones so each
   lands with its plumbing).
4. Re-point `BuildDefinesConfig` first hop; `RuitkDiagnosticsPaths` mechanical fix.
5. Tests (new file `Ugui/Tests/RuitkSettingsJsonTests.cs`, EditMode-safe, in the existing
   `Ruitk.Ugui.Tests` asmdef — no new asmdef): parse-string cases — empty JSON ⇒ all defaults;
   full §3 schema round-trip; unknown key ignored; bad enum value ⇒ default; tri-state mapping
   table (`auto` editor-context = on); resolution-order proof seeding each hop (JSON model
   injected → legacy `RuitkConfig` fixture string → defaults) — the settings-campaign functional
   smoke pattern, now pinned as a real test.

Gate: VERIFY-GATES + VERIFY-UNITY + player proof (`Shared/` touched); `Ruitk.Ugui.Tests` compiles
(owner runs it in-editor at M8; the compile IS this session's gate — §1.4 locked-editor rule).

### M3 — Reconciler knobs (U-04, U-05)
1. `FiberConfig`: add `TimeSliceMs`, `TimeSlicingEnabled`; `FiberReconciler.cs:31` const deleted,
   `:450` re-pointed, `:363-373` bypass added.
2. `RenderScheduler.Awake` budget read; `UguiHostConfig` `_poolEnabled` gates (U-05).
3. Resolvers + seam application (all three §1.2 seams); window rows + §3 keys for
   `time_slicing/time_slice_ms/frame_budget_ms/host_node_pool` with the U-04 editor-unbudgeted
   tooltip note.
4. Tests: extend `RuitkSettingsJsonTests` for the four new keys; stress suites
   (`UguiStressChurnTests`) must be re-read to confirm they do not assume pooling — if one does,
   parameterize it, never delete the assertion.

Gate: VERIFY-GATES + VERIFY-UNITY + player proof. Acceptance: with no JSON file present, a
diff of runtime behavior is IMPOSSIBLE by construction (defaults reproduce the constants —
re-read the three edited decision points and confirm each default short-circuits to the old code
path).

### M4 — hook_validation flip + strict_diagnostics + strict_mode (U-06, U-07)
1. U-06 seam wiring + the three-site prefix fix.
2. U-07 double-invoke at `FiberFunctionComponent.cs:160-163` + `FiberConfig.StrictModeEnabled` +
   force-off resolver.
3. Window rows + §3 keys (`hook_validation`, `strict_diagnostics`, `strict_mode` — the last with a
   "double-invokes renders in dev; forced off in release builds" tooltip).
4. Tests: `Ugui/Tests` additions — strict_mode on: a counting component proves render body runs
   2×, effect runs 1×, cleanup runs 1×, committed UI identical to strict-off; hook-order
   validation still primes correctly; `UguiStressChurnTests` green with strict on (pool
   interaction, U-07 discarded-tree rule); state-update-during-render warning fires once per
   offending render (dedup via `StrictDiagnosticsKeys`, `Hooks.cs:539-543`, unchanged).

Gate: VERIFY-GATES + VERIFY-UNITY + player proof. Acceptance: message prefix grep —
`grep -rn "StrictMode" Shared/Core/Hooks.cs` returns zero hits.

### M5 — Trace ladder restoration + diff_tracing independence (U-08, §6)
Execute the §6 table row by row; delete `FiberConfig.EnableFiberLogging`; fix the three adapters.
Tests: a gate-matrix test (pure logic, `Ugui/Tests`): for each (trace_level × diff_tracing)
combination assert the derived flags (`structural`, `detail`, `diff`) match §6's truth table —
this pins Basic's restoration and diff independence against regression.

Gate: VERIFY-GATES + VERIFY-UNITY + player proof. Acceptance grep:
`grep -rn "EnableFiberLogging" --include="*.cs" .` → zero hits outside `Plans~/`.

### M6 — Changelog + version
1. REWRITE `CHANGELOG.md` `[Unreleased]` → `## [0.13.0] - <date>`: unified settings (JSON store,
   window), the canonical knob set with the §0.1 defaults table, the hook-validation release
   flip (BEHAVIOR CHANGE callout, house style precedent: the config.json demotion entry), Basic
   trace restoration, diff_tracing independence, strict_mode, exceptionControlFlow removal, the
   generator disk-scan fix (already drafted — keep), publish-menu removal (already drafted —
   keep). One sentence: the interim ScriptableObject store existed only unreleased and was
   replaced before shipping.
2. `package.json:4` → `0.13.0`.
3. `Plans~/DISCORD_CHANGELOG.md` entry per the `discord-changelog` skill (ASCII, ≤2000 chars).

Gate: VERIFY-GATES; `node scripts/changelog.mjs verify` if the tooling lane was touched (it was
not — extensions unchanged; run it anyway, it must stay green).

### M7 — Docs site
Per §7. Gate: `cd ReactiveUIToolkitDocs~ && npm run build` → 0 errors; VERIFY-GATES.

### M8 — Owner smoke (manual; do not skip silently)
Ask the owner (editor open is fine — this is IN the editor): open Reactive UI Toolkit ▸ Settings;
no-store view shows effective defaults; Create writes `Assets/Resources/ReactiveUIToolkit/
config.json` with the full §3 body; toggling trace_level to `basic` produces structural `[Fiber]`
logs on a Samples interaction and NO per-hook detail; `verbose` adds detail; `diff_tracing` alone
(trace `none`) produces diff logs (independence proven live); strict_mode on shows double render
counts in a dev build and is inert in a release build; delete the stale
`Assets/ReactiveUIToolkitSettings.asset` from the host project; run `Ruitk.Ugui.Tests` in the
Test Runner. Record results; if the owner defers, record THAT in the changelog entry
("editor smoke pending") — never silently.

---

## 5. exceptionControlFlow — full touchpoint table (M1; verdict per row)

| # | Location | What is there | Action |
|---|---|---|---|
| 1 | `Shared/Core/Config/RuitkSettings.cs:46-47` | `exceptionControlFlow` field + tooltip | delete (file is rewritten in M2 anyway; M1 deletes the field so the M2 rewrite never carries it) |
| 2 | `Shared/Core/Config/RuitkConfig.cs:25` | legacy DTO field | delete — an old user `config.json` carrying the key is silently ignored by JsonUtility, which is the intended migration |
| 3 | `RuitkConfig.cs:38` | `UseExceptionBoundaryFlow` property | delete |
| 4 | `RuitkConfig.cs:74` | fallback assignment | delete |
| 5 | `Shared/Core/BuildDefinesConfig.cs:45-53` | `ResolveExceptionBoundaryFlow()` | delete |
| 6 | `Shared/Core/Diagnostics/DiagnosticsConfig.cs:28-32` | `UseExceptionBoundaryFlow` static (write-only — zero readers, verified 2026-07-31) | delete |
| 7 | `Runtime/Core/RootRenderer.cs:68-69` | seam assignment | delete |
| 8 | `Ugui/Core/UguiRootRenderer.cs:64-65` | seam assignment | delete |
| 9 | `Editor/EditorRootRendererUtility.cs:57-58` | seam assignment | delete |
| 10 | `Editor/RuitkSettingsWindow.cs:105-108` | read-only "Effective values" row | delete |
| 11 | `Editor/RuitkSettingsWindow.cs:147-153` | PropertyField row | delete |
| 12 | `ReactiveUIToolkitDocs~/src/pages/UITKX/Concepts/UitkxConceptsPage.tsx:117` | settings bullet claiming the toggle "routes render exceptions through the exception-boundary flow" — **currently false** (the flag reads nothing) | delete the bullet (the section is updated wholesale in M7 anyway; M1 may fold this into M7 — either is fine, it must be gone by M7's gate) |
| 13 | `ReactiveUIToolkitDocs~/src/pages/Migration/MigrationPage.tsx:102-111` (`:106`) | 0.12-migration backup warning listing the legacy key | annotate: append "(`exceptionControlFlow` was removed in 0.13.0; the legacy key is ignored)" — the backup advice itself stays, it is about a real old file |
| 14 | `MIGRATION-0.12.md:51-54` (`:53`) | same list, shipped doc | **leave as history** — shipped migration docs are a frozen record (the machine-path gate's own frozen-tier principle); it accurately describes 0.12-era files |
| 15 | `CHANGELOG.md` `[Unreleased]` (uncommitted; two mentions: the Added section's window row, the Changed section's shipped-block note) | describes the knob as live | rewritten at M6 — M1 just leaves a `TODO(M6)` marker; SHIPPED changelog bodies mentioning the key are frozen, untouched |

Rationale lock (echo of §0): the knob selected between error-boundary strategies in the legacy
reconciler; the strategy selector died with `Shared/Core/Reconciler.cs` (commit `2d8b50a7`) and
the surviving boundary behavior is unconditional. Do not resurrect the knob "for compatibility" —
there is nothing for it to select.

---

## 6. Trace-site mapping (M5 executes this table; §0.1 rows 8-9 are the law)

Derived gates after M5 — spell them exactly like this in code (no new abstraction layer; these are
inline conditions or the existing `InternalLogOptions` bridge):
- **structural** ⇒ `DiagnosticsConfig.CurrentTraceLevel != TraceLevel.None` (Basic and Verbose)
- **detail** ⇒ `DiagnosticsConfig.CurrentTraceLevel == TraceLevel.Verbose`
  (`InternalLogOptions.EnableInternalLogs` keeps this meaning — assignment at the three seams
  unchanged)
- **diff** ⇒ `DiagnosticsConfig.EnableDiffTracing || CurrentTraceLevel == TraceLevel.Verbose`
  (the exact legacy OR expression, `Reconciler.cs:1669` at `2d8b50a7~1`)

| Site (today) | Today's gate | Becomes |
|---|---|---|
| `FiberReconciler.cs:1187-1196` InsertBefore log | `EnableFiberLogging` | **structural** |
| `FiberReconciler.cs:1199-1204` AppendChild log | `EnableFiberLogging` | **structural** |
| `FiberReconciler.cs:1209-1215` no-host-parent warning | `EnableFiberLogging` | **structural** |
| `FiberReconciler.cs:1370` `CommitDeletion` (no log exists) | — | ADD one **structural** log: `[Fiber] Delete {ElementType}` at method entry (top-level per deleted subtree — inside `CommitDeletions :916`'s loop, not per recursive child; one line per removed subtree, matching legacy `[ReplaceNode]` granularity) |
| `FiberReconciler.cs:784` `CommitRoot` (no log exists) | — | ADD one **structural** summary at commit end: `[Fiber] Commit #{_commitCount} effects={_effectsCommitted}` |
| `FiberReconciler.cs:1136-1141` apply typed props | `EnableFiberLogging` | **diff** |
| `FiberReconciler.cs:1154-1160` apply props (+key list) | `EnableFiberLogging` | **diff** |
| `FiberReconciler.cs:1172-1177` NO-props warning | `EnableFiberLogging` | **diff** |
| `FiberReconciler.cs:1291+` CommitUpdate Label old/new dump | `EnableFiberLogging && == "Label"` | **diff** (keep the Label filter — re-gating, not widening) |
| `Shared/Elements/RadioButtonElementAdapter.cs:78-80` | `EnableDiffTracing && != None` (BUG) | **diff** |
| `Shared/Elements/RadioButtonGroupElementAdapter.cs:130-132` | same BUG | **diff** |
| `Shared/Elements/ToggleElementAdapter.cs:80-82` | same BUG | **diff** |
| `Shared/Elements/BaseElementAdapter.cs:112` | `EnableInternalLogs` | **detail** (unchanged) |
| `Hooks.cs:219, :247, :638, :665` | `EnableInternalLogs` | **detail** (unchanged) |
| `Hooks.cs:1241-1253` UseEffect capture log | `== Verbose` inline | **detail** (mechanically: leave as-is or route through `InternalLogOptions` — pick the file's existing majority style, which is `InternalLogOptions`) |
| `Editor/EditorRenderScheduler.cs:111-129` queue-depth log | `== Verbose` inline | **detail** (unchanged gate; editor-side) |
| `Editor/EditorRenderScheduler.cs:133-143` effect-flush log | `== Verbose` inline | **detail** (unchanged gate) |
| `FiberConfig.EnableFiberLogging` (`FiberConfig.cs:11`) | set by nothing | DELETED (absorbed; §0.1 row 9) |

Strict-mode interaction (U-07): structural/diff logs fire on the COUNTED (second) invoke only —
placement/commit sites are commit-phase so they are naturally single; nothing in the render phase
above logs per-invoke except hook detail (`Hooks.cs:1241`), which under strict double-invoke will
log twice at Verbose — accepted, it is truthful (two captures happened), note it in docs.

---

## 7. Docs + changelog sync surface (M6/M7 checklist)

- [ ] `ReactiveUIToolkitDocs~/src/pages/UITKX/Concepts/UitkxConceptsPage.tsx:102-120` — the
      settings bullets: rewrite to the §3 schema (all 10 canonical knobs + the Unity-only folder
      key, marked), the JSON path + create-on-demand flow replacing the asset story, the
      `auto` tri-state semantics, the trace ladder (`basic` = structural, `verbose` = +detail,
      `diff_tracing` independent), strict_mode (dev-only, double-invoke, release force-off), and
      the U-04 editor-unbudgeted note. Delete row 12 of §5 if M1 left it.
- [ ] `ReactiveUIToolkitDocs~/src/pages/Migration/MigrationPage.tsx:106` — §5 row 13 annotation.
- [ ] `CHANGELOG.md` — M6 rewrite (see milestone).
- [ ] `package.json:4` — `0.13.0`.
- [ ] `Plans~/DISCORD_CHANGELOG.md` — M6 entry.
- [ ] `CLAUDE.md` — if it gains/keeps any sentence about settings storage, it must say JSON store,
      not ScriptableObject (currently it says neither — only add if something there becomes wrong).
- [ ] Extension lanes (`ide-extensions~/changelog.json`, marketplace pages): UNTOUCHED — no
      extension change in this campaign; `node scripts/changelog.mjs verify` must remain green.
- [ ] `ReactiveUIToolkitDocs~` build: `npm run build` → 0 errors.

---

## 8. DO-NOT list (violating any = stop and ask)

1. **NO UI Toolkit pooling — not even "while we're in there".** History, quoted (§0): the one
   attempt, `Shared/Elements/Pools/GlobalVisualElementPool.cs`, was deliberately removed in
   `a05f5c07` ("removed cache and pool GlobalVisualElementPool", 2025-11-17) because resetting a
   generic `VisualElement` cannot be proven complete — leftover state bleeds into the next mount.
   Only adapter-gated pooling (uGUI `TryResetForPool`, where each adapter owns its reset proof)
   is sanctioned, and `host_node_pool` only GATES the existing uGUI pool — it creates nothing.
2. **NO exceptionControlFlow revival.** It was a strategy selector for a legacy reconciler path
   that no longer exists; the feature it "selected" runs unconditionally. A config key that
   selects nothing is worse than none (§5 rationale lock).
3. **Reconciler BEHAVIOR unchanged — this leg is the reference.** Every default must reproduce
   today's execution byte-for-byte (M3 acceptance). If parity with a sibling seems to require a
   Unity reconciler change, the sibling is wrong or the contract is — STOP AND ASK; do not edit
   `FiberReconciler`'s algorithm, effect ordering, bailout, or commit sequencing.
4. **Mount stays synchronous.** `time_slicing=false` routes through the EXISTING `WorkLoop()`;
   do not introduce async mount, coroutines, or deferred first paint anywhere.
5. **Never auto-create the settings file.** Window button only (TMP/DOTween lesson). Opening the
   window must not dirty the user's project.
6. **Never launch the Unity editor from automation while the owner may have it open** (§1.4) —
   the dotnet compile harness is the only sanctioned compile check; in-editor steps are M8, owner-run.
7. **Do not commit the uncommitted settings pile "to clean up" before M0** — it is the baseline;
   the owner's no-auto-commit rule stands unless explicitly lifted.
8. **Never weaken/delete an existing test assertion to get green** (parameterizing a
   pooling-assuming stress test per M3 is the sanctioned pattern; deleting its assert is not).
9. **No new asmdef, no SourceGenerator~/language-lib edits, no corpus/hash writes** — this
   campaign has zero `.uitkx`-language surface; `corpus-hash.mjs --check` green throughout.
10. **No machine-local paths in anything tracked** — host-project location and Unity editor
    location are derived or live in `.ruitk-local.json` (§1.4); `check-machine-paths.mjs` gates
    every milestone.
11. **Pool capacity stays a constant** (`PoolCapacityPerType = 128`) — do not promote it to a
    setting "for symmetry"; §0 pins caps per-leg.

---

## 9. Dead-code decision items (LISTED for the owner — recommendation only, NOT part of this campaign)

| Item | Evidence | Recommendation |
|---|---|---|
| `PropTypeValidator` subsystem | `Shared/Core/PropTypes.cs:131-180`; `internal static class` with `Enabled=true` and `Validate(...)` — **zero call sites repo-wide** (verified 2026-07-31: the only grep hit is its own declaration). The public `PropTypes`/`WithPropTypes` surface (`:182+`) attaches definitions that nothing ever validates. | Owner-gated ticket: **remove-or-wire**. If wired, it belongs behind `strict_diagnostics`; if removed, the public `WithPropTypes` API needs a deprecation minor first. Do NOT fold into this campaign. |
| `FiberConfig.ShowReconcilerInfo` | `Shared/Core/Fiber/FiberConfig.cs:16` — declared, never read, never set (verified). | Remove in the same future ticket; public static, so deprecation note in changelog. |

---

## 10. Decision log (campaign-local; §0 decisions are the family's, these are Unity's)

| # | Decision | Why |
|---|---|---|
| D-1 | Storage = `Assets/Resources/ReactiveUIToolkit/config.json` TextAsset in the CONSUMER project; package never carries one | Resources ships into every build with zero build hooks; synchronous load everywhere; project-owned = writable + upgrade-stable in all install layouts (the UPM PackageCache problem that killed the in-package file, documented at `RuitkConfig.cs:92-100`) |
| D-2 | `RuitkSettings` class name + `ActiveOrNull` accessor survive the SO→JSON rewrite | Minimizes churn at `BuildDefinesConfig` + `RuitkDiagnosticsPaths`; the type was never shipped, so no public-API compat concern |
| D-3 | Bootstrap + BuildInjection deleted rather than adapted | Both exist solely to solve SO problems (asset discovery, preloaded-assets injection) that JSON-in-Resources does not have |
| D-4 | JsonUtility DTO-with-initialized-defaults as the parser | No new dependency; absent-field = initializer is exactly missing-key = default; tri-states as strings sidestep JsonUtility's absent-bool blindness |
| D-5 | Editor scheduler stays unbudgeted (U-04) | Investigated: no budget exists today, editor preview + HMR favor immediate drain; aligning it would be a behavior change with no ask — documented instead |
| D-6 | Sanctioned untouched-build changes, exactly two | (a) hook_validation release flip — §0 ruling item 5 explicitly sanctions it; (b) no-store editor environment: `production` (legacy compiled default) → `auto`→`development` — this is §0.1 row 10's canonical default doing its job, and the (uncommitted) changelog already carries the BEHAVIOR CHANGE callout pattern to extend |
| D-7 | `[Hooks][StrictMode]` → `[Hooks][Strict]` rather than renaming the strict_diagnostics knob | The messages belong to strict_diagnostics; `strict_mode` (knob 7) now owns the "StrictMode" name family-wide — prefix must stop squatting it |
| D-8 | Legacy trace evidence pinned to `2d8b50a7~1:Shared/Core/Reconciler.cs` | Executors can re-derive every §6 "legacy" claim with `git show` — no trust required |
| D-9 | strict_mode gate is a resolver-level runtime force-off, not `#if` | Contract says settings ship into builds; the CODE ships, the ACTIVATION is denied in release — simplest proof of "cannot opt in" |
| D-10 | MIGRATION-0.12.md untouched (§5 row 14) | Shipped migration docs are frozen history — same principle the machine-path gate encodes for archived tiers |

---

## 11. Reference reading list (the files that DEFINE the family semantics — protect them)

Sibling-leg executors port FROM these; Unity executors must not casually reshape them. Read before
touching, cite line-exactly in commits:

- `Shared/Core/Fiber/FiberReconciler.cs` — work loop (`:360-472`), commit phase (`:784+`),
  placement (`:1094+`). THE reference reconciler. This campaign only re-gates its logs and
  parameterizes two constants.
- `Runtime/Core/RenderScheduler.cs` — the budgeted frame pump (`:150-180`), priority queues,
  batching. `frame_budget_ms` semantics live here.
- `Shared/Core/Fiber/FiberFunctionComponent.cs` — the render call (`:130-175`), hook-order
  priming, effect flag propagation. strict_mode's insertion point.
- `Shared/Core/Hooks.cs` — validation (`:21`), strict diagnostics (`:22, :156-161, :523-575,
  :603-608`), the 20+ hook implementations whose index-keyed re-registration makes double-invoke
  safe.
- `Shared/Core/BuildDefinesConfig.cs` + `Shared/Core/Config/RuitkConfig.cs` +
  `Shared/Core/Config/RuitkSettings.cs` — the three-hop resolution chain every leg mirrors.
- `Ugui/Core/UguiHostConfig.cs` — the sanctioned pooling pattern (`TryResetForPool`).
- `ruitk-unreal/Plugins/ReactiveUIToolkit/Source/RuitkCore/Private/RuitkReconciler.cpp:565-570` —
  the strict_mode double-invoke shape this leg adopts (sibling reference, read-only).
- History: `git show a05f5c07` (pool removal), `git show 2d8b50a7` (legacy reconciler deletion —
  the Basic-trace evidence base).

---

## 12. Error signatures / risks

| Signature | Meaning → action |
|---|---|
| CS0246 `UnityEditor` in a `.Player` build | An editor API leaked into `Shared/`/`Runtime/`/`Ugui/` — wrap in `#if UNITY_EDITOR` or move to `Editor/`; this is the player-proof gate doing its job |
| `Ruitk.*.Player.csproj` absent | M0 step 2 unresolved — STOP AND ASK; do not fake the proof with the stale `ReactiveUITK.*` set |
| Settings window edits do nothing at runtime | `RuitkSettings.Invalidate()` not called after write, or the TextAsset wasn't re-imported — U-01 writer contract |
| A default-config run behaves differently from `master` | M3 acceptance violated — a knob's default doesn't short-circuit to the old path; diff the decision point, not the symptom |
| Strict-mode double effects | U-07 rule 2 broken for some hook family — the index-keyed overwrite assumption failed; fix the prep-reset, never skip the second invoke |
| `check-machine-paths.mjs` red on the plan or code | A drive-absolute or personal-root path got written — derive it or move it to `.ruitk-local.json`; NEVER extend the allow-list to pass |
| `corpus-hash.mjs` red | Something touched the `.uitkx` corpus — this campaign must not; revert the touch |
| Unity editor "assembly locked" during a build attempt | You launched Unity or copied into `Analyzers/` against §1.4/DO-NOT 6 — stop, use the dotnet harness |
| Verbose logs appear at `basic` | A §6 row mis-gated (structural vs detail) — re-check against the truth-table test from M5 |

---

*End of plan. Companion documents: `Plans~/ES_MODULES_EXECUTION_PLAN.md` (house plan-style
precedent), `CHANGELOG.md` `[Unreleased]` (the uncommitted settings-campaign record this plan
absorbs), the family parity contract as embedded in §0 (sibling legs carry the same section).*

---

## EXECUTION LOG (running; newest milestone last)

### M0 — Baseline audit — DONE 2026-07-31
- **Tree state:** clean; the §1.1 pile is committed (`ab46dc53` settings, `f2728b43` 0.13.0
  release staging, `ca31886e` this plan) — see the BASELINE DRIFT note in the header. Branch
  `feat/family-parity` checked out at `392154d4` (== origin/dev; origin/master identical).
- **Host project:** located via `Packages/manifest.json` naming this checkout (`file:` dependency
  + `testables`). The **Unity editor IS OPEN on it** (editor process + `Temp/UnityLockfile`
  verified) → locked-editor mode for the whole round: dotnet harness only, no Unity launches.
- **Csproj sets:** current `Ruitk.*` set present (regenerated 2026-07-30 by the running editor);
  `Ruitk.*.Player.csproj` **absent** (only the stale pre-rename `ReactiveUITK.*.Player` set,
  from a months-old generation — never used, per plan). **Workaround executed:** the three
  Player csprojs are SYNTHESIZED outside the repo (scratchpad) from the current `Ruitk.*` set by
  the exact generator delta (drop `UNITY_EDITOR*` defines, drop `UnityEditor*` reference blocks,
  Player output dir, `.Player` project-reference chain, paths absolutized so nothing is written
  into the host project). Baseline proof: Shared/Runtime/Ugui Player builds **0 errors**.
  OWNER TOUCHPOINT (deferred): enable player-csproj generation (Edit ▸ Preferences ▸ External
  Tools) and regenerate, so later milestones can run the real artifact.
- **Green floor:** machine-paths gate ✓; corpus-hash ✓ (`917dd8cd…`); VERIFY-UNITY 6/6 csprojs
  0 errors (warnings pre-existing: Shared 5, Editor 1, Samples 11); SG suite **1828/1828**; LSP
  suite **152/152**. `dotnet test` churned `Analyzers/*.dll` — reverted via targeted checkout
  (watch this before every commit).
