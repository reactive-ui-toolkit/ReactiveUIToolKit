# Migrating to 0.12.0 — the "Reactive UI Toolkit" rename

0.12.0 is the family-rebrand release: the library is now **Reactive UI Toolkit — Unity**,
part of the Reactive UI Toolkit family (Godot, Unity, Unreal), hosted at
`https://github.com/reactive-ui-toolkit/ruitk-unity`. It is a **BREAKING** release —
nothing changed functionally, but the code identity did:

| Deliverable | 0.11.x | 0.12 wave |
|---|---|---|
| Unity package (UPM) | 0.11.0 | **0.12.0** |
| VS Code extension | 1.7.0 | **1.8.0** |
| VS2022 extension | 1.7.0 | **1.8.0** |
| Rider plugin | 1.4.0 | **1.5.0** |

What stays: the UPM package id `com.reactiveuitoolkit`, the `.uitkx` language and the
`UITKX` tooling brand, the extension marketplace identities, the docs domain, and the
`RUITK` abbreviation (window titles / pref keys) — it abbreviates the new name too.

## What renamed

1. **Namespace / assembly root: `ReactiveUITK` → `Ruitk`** — every `namespace`, `using`,
   and `global::` reference, all 10 asmdefs (`ReactiveUITK.*` → `Ruitk.*`; the file
   `ReactiveUITK.Examples.asmdef` is now `Ruitk.Samples.asmdef`, finally matching its
   assembly name), and the analyzer DLLs (`Analyzers/Ruitk.Language.dll`,
   `Analyzers/Ruitk.SourceGenerator.dll` — same GUIDs, references survive).
2. **Composites:** `ReactiveUITKConfig` → `RuitkConfig`.
3. **Hidden runtime object names:** the media subsystem's internal hosts renamed —
   `__ReactiveUITK_MediaHost` / `__ReactiveUITK_VideoPeer` / `__ReactiveUITK_AudioPeer` /
   `__ReactiveUITK_Sfx` / `__ReactiveUITK_RT_<w>x<h>` are now `__Ruitk_*`. Only relevant
   if you `GameObject.Find` them (you shouldn't — they're internals).
4. **Define:** `REACTIVEUITK_HAS_TEST_FRAMEWORK` → `RUITK_HAS_TEST_FRAMEWORK`.
5. **Install folder casing:** `Assets/ReactiveUIToolKit` → `Assets/ReactiveUIToolkit`
   (lowercase k, matching the package id).
6. **Editor menu root:** `ReactiveUITK/…` → `Reactive UI Toolkit/…`.

## Upgrade steps

1. **Delete the old package folder first**, then import 0.12.0:
   remove `Assets/ReactiveUIToolKit` (and its `.meta`) before adding the new
   `Assets/ReactiveUIToolkit`.
   **Linux note:** Windows/macOS filesystems merge the two casings, so an in-place
   upgrade collapses into one folder. On Linux (case-sensitive) you end up with **BOTH**
   `ReactiveUIToolKit` and `ReactiveUIToolkit` side by side — delete the old capital-K
   folder or every type exists twice.
2. **Run the codemod** over your own code (it never edits inside the package folder):

   ```bash
   dotnet run --project Assets/ReactiveUIToolkit/SourceGenerator~/Tools/RuitkMigrateBrand -- Assets
   # then prove idempotence / CI-gate it:
   dotnet run --project Assets/ReactiveUIToolkit/SourceGenerator~/Tools/RuitkMigrateBrand -- Assets --check
   ```

   It rewrites `.cs`, `.uitkx`, and `.asmdef` files with per-file counts; a second run
   reports 0.
3. Let Unity recompile. Done.

## Hand-migration rules (what the codemod does, verbatim)

Apply in this order:

- **C1 — composites (enumerated):**
  `ReactiveUITKConfig` → `RuitkConfig`,
  `__ReactiveUITK_MediaHost` → `__Ruitk_MediaHost`,
  `__ReactiveUITK_VideoPeer` → `__Ruitk_VideoPeer`,
  `__ReactiveUITK_AudioPeer` → `__Ruitk_AudioPeer`,
  `__ReactiveUITK_Sfx` → `__Ruitk_Sfx`,
  `__ReactiveUITK_RT_` → `__Ruitk_RT_`.
- **C2 — bare token (regex, do not widen):** `ReactiveUITK(?![A-Za-z_])` → `Ruitk`.
  Covers `using`/`namespace`/`global::` lines and asmdef `references` entries like
  `ReactiveUITK.Runtime` → `Ruitk.Runtime`.
- **Define:** `REACTIVEUITK_HAS_TEST_FRAMEWORK` → `RUITK_HAS_TEST_FRAMEWORK`.
- **Path strings:** `Assets/ReactiveUIToolKit` → `Assets/ReactiveUIToolkit`
  (both slash forms).

## Cosmetic notes

- The editor-prefs key `ReactiveUITK.UitkxNavVerbose` is now `Ruitk.UitkxNavVerbose`;
  your saved value resets once. `RUITK_*` and `UITKX_HMR_*` keys are unchanged.
- Old versions keep the license and terms they shipped with; 0.12.0 ships under the
  Reactive UI Toolkit Community License 1.1 (see `LICENSE.md`).
