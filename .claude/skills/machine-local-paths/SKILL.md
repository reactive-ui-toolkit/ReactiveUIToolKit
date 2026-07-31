---
name: machine-local-paths
description: The machine-local path invariant and its CI gate (scripts/check-machine-paths.mjs) — what it forbids, how to run it CORRECTLY (untracked files are invisible to it), the four legitimate ways to answer a violation, where machine facts live (.ruitk-local.json), the published-install-path exception that must NOT be derived away, and the copy-to-a-differently-named-folder portability test. Use when the gate fails, when adding or editing a .vscode config / script / workflow / csproj, after any rename or repo-wide sweep, when wiring a new external tool, or when moving/renaming the checkout.
---

# Machine-local paths

## The invariant

**No tracked file may name a path that exists only on one machine.** Repo locations are DERIVED, never
written down. External tools are PROBED, with an override chain. The irreducible machine values live in
one gitignored file. A CI gate enforces it.

Why it exists: the rebrand sweep rewrote a repo-folder segment *inside* a hardcoded absolute path in a
sibling repo's `.vscode/launch.json`, silently breaking F5 for every clone whose folder name differed
from the author's. Three independent audits read that line, classified it "owner machine path — leave
it", and moved on. Judgment missed it three times, so this is a gate, not a note.

## The two rules

- **R1 — personal roots.** A drive-absolute path (`<drive>:\…`) or an explicit user-home POSIX path
  (`/home/<u>/`, `/Users/<u>/`, `/mnt/<d>/`) whose root is not in `ALLOWED_ROOTS`. Shared platform and
  CI roots (`C:\Program Files\…`, `/usr/…`) are **deliberately legal** — the Unity Hub / MSBuild /
  dotnet probing in `ReferenceAssemblyLocator.cs`, `publish-vsix.ps1` and `install.ps1` is correct,
  cross-platform-aware code and must keep naming them.
- **R2 — portability-critical files.** `.vscode/*.json` (any depth), `*.csproj`, `*.sln`,
  `*.code-workspace`, `Directory.Build.props|targets` must contain **zero** drive-absolute paths, even
  standard ones — they run on other people's machines and have substitution available.

## THE EXCEPTION THAT IS NOT A LEAK

`Assets/ReactiveUIToolkit` is the **published install path** — a contract with every customer, baked
into `AssetStoreExport.PackageRoot`, `publish.yml`'s `STORE_DIR`, and the migration docs. It is a
*product* fact, not a machine fact. **Never "derive it away" or rename it to match the GitHub slug
(`ruitk-unity`).** The single source for handling both install channels is
`ScanRules.IsPackageFolderSegment`, which matches `ReactiveUIToolKit` / `ReactiveUIToolkit` /
`com.reactiveuitoolkit`.

### …but `Application.dataPath + "/ReactiveUIToolkit"` is NOT how you find the package

That the install path is a product contract does **not** mean tools may hardcode it. The package root
is a *different absolute path* in each of four shipping layouts: `Assets/ReactiveUIToolkit` (store
`.unitypackage` + today's dev tree), `Library/PackageCache/com.reactiveuitoolkit@<hash>` (the UPM
git-URL channel — the PRIMARY install per `MIGRATION-0.12.md`), `Packages/com.reactiveuitoolkit`
(embedded), and any absolute path (a local `file:` reference).

**Use `Editor/RuitkPackagePaths.cs` (`Ruitk.EditorSupport.RuitkPackagePaths`) — never a fresh literal.**
`TryGetRoot(out root)` to degrade or log, `GetRootOrThrow()` when you cannot continue,
`FailureMessage` for the rung-by-rung diagnostic. It probes `PackageInfo.FindForAssembly` (covers UPM /
embedded / `file:`) → an AssetDatabase sentinel walk-up (covers `Assets/`, and is folder-NAME-agnostic
so a renamed install still resolves) → the legacy `dataPath` path as the LAST rung only.

This closed a real customer bug: `UitkxHmrCompiler.FindAnalyzersDirectory` threw
`DirectoryNotFoundException` for every UPM customer, so Hot Module Reload could not run at all.
Converted with it: `PublishUtility.cs` (×4), `UitkxTestRunnerWindow.cs`, `BenchResultsViewer.cs`.

Two sites keep the literal **on purpose** — do not "fix" them:
- `AssetStoreExport.PackageRoot` is an *asset-database* prefix (`AssetDatabase.FindAssets`
  `searchInFolders` + `StartsWith` guard rails), and a contract with `publish.yml`'s hand-built
  `STORE_DIR`. Asserting the exact store layout is that file's whole job.
- `Shared/Core/Config/RuitkConfig.cs` is in **`Ruitk.Shared`, a RUNTIME assembly** (no
  `includePlatforms`). `RuitkPackagePaths` is editor-only by construction — `PackageInfo` lives in
  `UnityEditor.PackageManager` and `AssetDatabase` is editor-only, so referencing it from any runtime
  assembly (`Ruitk.Shared`, `Ruitk.Runtime`, `Ruitk.Diagnostics`) breaks every player build.

**Open design question, not a path bug** (owner decision — see the note in `RuitkConfig.cs`): a
*user-editable* file read from inside the package is incoherent for UPM installs, where the folder is
read-only and re-created under a new hash on each upgrade. Same shape in two package-root **write**
sites, `Diagnostics/Benchmark/BenchLogging/BenchPerSecondLogger.cs` and
`Diagnostics/Logs/ReactiveLogCapture.cs` — resolving their root correctly would only convert a
wrong-path bug into a write-to-read-only bug, so they were deliberately left alone. HMR itself is
clean here: every byte it emits goes to `%TEMP%/UitkxHmr`, never under the package root.

## Running it

```bash
node scripts/check-machine-paths.mjs          # the gate (exit 1 on violation)
node scripts/check-machine-paths.mjs --list   # every absolute path found, with a verdict each
```

**THE TRAP — new files are invisible.** The gate enumerates `git ls-files`, i.e. tracked files only. A
brand-new file is untracked, so the gate skips it and reports green — then turns red on the commit
that adds it. When your change ADDS files, test post-commit reality:

```bash
git add -N <the new files>
node scripts/check-machine-paths.mjs
git reset
```

## A violation has exactly four legitimate answers

1. **Derive it** — repo root via `git rev-parse --show-toplevel` or a script's own `..`; worktrees via
   `git worktree list`; `${workspaceFolder}` in VS Code configs.
2. **Probe + override** — `$ENV_VAR` → `.ruitk-local.json` → PATH / standard install roots → an error
   naming all three rungs. `PublishUtility.ResolveNpmPath` is the worked example (`RUITK_NPM` →
   `.ruitk-local.json` `npmPath` → PATH).
3. **Exempt it, with a reason** — `EXEMPT` entries carry a `why`. Earned by frozen tiers
   (`Plans~/archive/**`, shipped `CHANGELOG.md` / `DISCORD_CHANGELOG.md` / `changelog.json` bodies, and
   the marketplace pages generated from them) and test trees (`SourceGenerator~/Tests`,
   `ide-extensions~/lsp-server/Tests` hold ~80 lines of deliberate Windows-absolute fixtures).
4. **Mark the line** — trailing `path-gate-allow: <reason>`; in Markdown use an HTML comment so it
   doesn't render (see `MIGRATION-0.12.md`'s teaching placeholder).

**Never widen `ALLOWED_ROOTS` to make a violation pass.**

## Machine facts: `.ruitk-local.json`

Gitignored, beside `publisher-secrets.json` in `.gitignore`; copy `.ruitk-local.example.json`.
**Nothing may require it** — discovery must work without it. Keys here: `npmPath`, `unityEditor`.

**Unity `.meta` rule:** every new tracked NON-dot file needs a `.meta` sibling with a fresh 32-hex
guid (`scripts/check-machine-paths.mjs.meta` has one). Dot-prefixed files are skipped by Unity's
importer and must NOT get one — `.gitignore` and `.csharpierignore` have none, so `.ruitk-local.example.json`
has none either.

## The portability acceptance test

Proves the tree works elsewhere, including pending uncommitted edits a `git clone` cannot see:

```bash
mkdir -p <scratch>/qqq-renamed
tar --exclude=node_modules --exclude=Library --exclude=Temp --exclude=obj --exclude=bin -cf - . \
  | tar -C <scratch>/qqq-renamed -xf -      # keep .git — the gate needs it
cd <scratch>/qqq-renamed && git add -N scripts/check-machine-paths.mjs .ruitk-local.example.json
node scripts/check-machine-paths.mjs        # must be green HERE
```

Confirm `pwd` is inside the copy before trusting the result (see scar tissue).

## Scar tissue

- **A failed copy produced a fake green.** When `robocopy` failed, the following `cd` failed too, so
  the gate ran in the ORIGINAL folder and printed ✓. Always verify `pwd`. `tar | tar` works in Git Bash.
- **The gate scans itself.** Specimens in it are written `<drive>:` on purpose; a self-exemption would
  be a permanent blind spot. Its own `/home/<route>` comment had to be written that way too, because
  the PrettyUi sample's React-Router route strings are indistinguishable from Linux home dirs (hence
  that tree's scoped exemption).
- **Escaped backslashes.** JS/TS/JSON spell a separator `\\`; each allowed root is admitted twice
  (plain + doubled), derived in the engine section, never hand-typed.
- **Space-containing roots.** `C:\Program Files (x86)\…` — the check tests the raw line from the match
  offset, because captured hits stop at the first space.
- **`config.json` was the sharpest leak** — it is *runtime-read* and carried the owner's nvm path.
  Tracked config files that feed code are the highest-value place to look, not the docs.
- **`dotnet build` in `CICD/` does not work here** (pre-existing): the generated
  `ReactiveUITK.Shared.csproj` still references the pre-rebrand `ReactiveUITKConfig.cs`. Compile such
  edits against `UnityEditor.dll`/`UnityEngine.dll` with a scratch csproj instead.
