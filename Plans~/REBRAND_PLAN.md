# Rebrand plan — "Reactive UI Toolkit" umbrella + org migration (Unity leg)

**Status: PLAN — awaiting the U-Q ratifications in §2.** Family context (owner, 2026-07-27):
everything rebrands under **Reactive UI Toolkit**, GitHub org **`reactive-ui-toolkit`**, repo
slug scheme **`ruitk-<engine>`** → this repo becomes **`reactive-ui-toolkit/ruitk-unity`**.
Sequencing: **org transfer FIRST, in-repo rename SECOND** (one URL sweep to final values;
transfer is content-free; CI verified on the org before the wave; redirects cover the gap).
The Godot leg's plan (ReactiveUI-Godot `plans/REBRAND_PLAN.md`) is the reference
implementation of this format; this plan stands alone — no cross-reading required.

**Key facts from the census (2026-07-27, measured):**
- The family-core scanner corpus (`_tiers.familyCore` of
  `ide-extensions~/lsp-server/test-fixtures/uitkx-scanner-cases.json`, prefix-normalized
  `UETKX|GUITKX|UITKX→TKX`) contains **ZERO brand tokens** (0 × "ReactiveUITK", 0 ×
  "VirtualNode") → **no family corpus re-pin, no sibling coordination, for ANY rename here.**
- Unlike Godot's `RUI` (which did not spell "Toolkit" and was renamed to `Ruitk`), Unity's
  identifier root **`ReactiveUITK` already encodes the umbrella** ("Reactive UI ToolKit") —
  the same "already-aligned" status as `com.reactiveuitoolkit`. §2 U-Q1 rules on it.
- Two defects found by the census, fixed by this wave: the UPM `displayName` is the broken
  string `Reactive UIToolKit` (missing space), and the VS Code extension's `repository.url`
  points at a WRONG org (`https://github.com/ReactiveUITK/ReactiveUIToolKit` — neither the
  current owner nor the new org).

**EXECUTOR CONTRACT (read first — written for mechanical execution):**
- Every step names exact file(s), exact OLD string, exact NEW string, and a verification
  command. Execute in order. OLD string not found EXACTLY → **STOP and report**; no fuzzy
  matching, no improvisation.
- Replacements are scoped as stated (per-file, or repo-wide WITH stated exclusions) — never
  invent a sweep. Identifier replaces are **whole-word** where stated.
- `.meta` files: NEVER edit their guids; folder renames go through `git mv` so Unity's
  `.meta` pairing survives (the folder's own `.meta` moves with it).
- After each group run its verification; after Phase 3 the FULL battery (§6.H). Push ONLY to
  the feature branch (`rebrand/umbrella`, branched from `dev`); the owner PRs. **[OWNER]**
  steps are console/browser actions — list, never attempt.

---

## 1. Name Registry (single source of truth)

| # | Context | OLD (exact) | NEW (exact) |
|---|---|---|---|
| U-N1 | Umbrella brand | — | `Reactive UI Toolkit` |
| U-N2 | Org / repo | `yanivkalfa/ReactiveUIToolKit` | `reactive-ui-toolkit/ruitk-unity` (RESOLVED — Scheme C) |
| U-N3 | Product display name | `ReactiveUIToolKit` (display uses) | **TBD-U-R2** (rec: `Reactive UI Toolkit — Unity`) |
| U-N4 | License product label | `ReactiveUIToolKit (Unity)` | rec: `Reactive UI Toolkit — Unity` |
| U-N5 | License credit line | `Made with ReactiveUI` | follows the family R4 ruling (rec: keep) |
| U-N6 | UPM id | `com.reactiveuitoolkit` | **UNCHANGED** (already umbrella-aligned; package ids are immutable-ish — changing = a new package) |
| U-N7 | UPM displayName | `Reactive UIToolKit` (defective) | `<U-N3>` — **bugfix** |
| U-N8 | UPM author.name | `ReactiveUIToolKit` | rec: `Reactive UI Toolkit` |
| U-N9 | Extension display names + IDs | `UITKX (Unity - VS Code)` / `UITKX (Unity - VS2022)`, publisher `ReactiveUITK`, ids `uitkx` / `UitkxVsix.ReactiveUITK`, Rider `pluginName = UITKX`, gradle group `com.reactiveuitk` | **ALL UNCHANGED** (family ruling: extension identity AND display stay; only content underneath changes) |
| U-N10 | Namespace/assembly root | `ReactiveUITK` (namespaces ×557, usings ×730/318 files, 10 asmdefs, 6 csprojs, 2 committed Analyzer DLLs, generator-emitted attributes/usings) | **U-Q1** (rec: **UNCHANGED** — already spells the umbrella; see §2) |
| U-N11 | Docs site title | `ReactiveUIToolKit Documentation` | `<U-N3> — Documentation` |
| U-N12 | Docs domain | `reactiveuitoolkit.info` (CNAME) | **UNCHANGED** (already umbrella-aligned; custom domain means the repo rename does NOT break docs URLs) |
| U-N13 | unitypackage + Asset Store staging folder | `ReactiveUIToolKit-<ver>.unitypackage`, `STORE_DIR="shell/Assets/ReactiveUIToolKit"` | **U-Q2** (rec: `ReactiveUIToolkit` casing — see §2) |
| U-N14 | Docs folder name | `ReactiveUIToolKitDocs~` | optional Annex A (rec: keep) |
| U-N15 | Wave version | UPM currently `0.11.0` (staged) | next free minor at execution (rec: fold into the staged 0.11.0 if unpublished, else 0.12.0) — **TBD at execution** |

**Frozen in every scenario:** `.uitkx` extension + `UITKX####` codes + `Uitkx*` tool/assembly
names (`UitkxLanguageServer`, `UitkxVsix`, `UitkxMigrateImports`, `uitkx` npm name) — the
LANGUAGE brand; marketplace publisher/extension IDs and display names (U-N9); `V`,
`VirtualNode`, `Hooks` (family API names); Tier-3 historical bodies (§3); `.meta` guids.

## 2. Phase 0 — ratifications **[OWNER]**

| # | Decision | Options + recommendation |
|---|---|---|
| U-R2 | Display strings | Ratify U-N3/U-N4/U-N8 exact strings (recs above) |
| **U-Q1** | Namespace/assembly root `ReactiveUITK` | **(a) KEEP (rec):** it already encodes "Reactive UI Toolkit" — the exact criterion that got Godot's `RUI` renamed (RUI did NOT spell Toolkit; ReactiveUITK does). Keeping = zero user breakage (their `using ReactiveUITK;` lines, asmdef references, and analyzer DLL references all survive), zero codemod, non-breaking wave. **(b) RENAME** (e.g. → `Ruitk` for cross-leg symmetry, or `ReactiveUIToolkit` full-word): 3,582 occurrences / 751 files + 10 asmdef names (user asmdefs reference them by name!) + 2 committed DLL filenames + generator-emitted `global::ReactiveUITK.*` attribute/using strings + HMR type-matching by FullName → BREAKING wave + user codemod (extend `UitkxMigrateImports`) + Rider/VSCode server vocabulary. A full campaign for a symbol that is already on-brand. If (b) is chosen, this plan needs a v2 with the class/namespace table — request it |
| **U-Q2** | Asset-Store install folder casing | The `.unitypackage` installs to `Assets/ReactiveUIToolKit`. **(a) normalize to `ReactiveUIToolkit`** (rec — "nothing stays" consistency): case-only rename; NOTE Windows/macOS default filesystems are case-insensitive → updates merge in place (no dup folder), Linux users get a side-by-side dup → MIGRATION note required ("delete the old ToolKit folder"); publish.yml `STORE_DIR` + artifact names update. **(b) keep `ToolKit`** in the install path (zero migration, the K lives on in one place) |
| U-R4 | Credit line | Family ruling applies (rec: keep `Made with ReactiveUI`) |
| U-N15 | Wave version | Confirm at execution |

## 3. Census (measured 2026-07-27) + tiers

| Surface | Count | Disposition |
|---|---|---|
| `ReactiveUITK` (namespaces/assemblies/attributes) | **3,582 × 751 files** (557 namespace decls, 730 usings in 318 files, 10 asmdefs, 645 in SourceGenerator~ incl. emitted strings) | **U-Q1** (rec: FROZEN) |
| `ReactiveUIToolKit` (display + repo + folder + product label) | 707 × 102 files | Tier 1 — display uses convert (§6.B); folder/casing per U-Q2/U-N14; historical bodies frozen |
| `ToolKit` vs `Toolkit` casing | 708 vs 214 | normalize DISPLAY uses to `Toolkit` via the U-N3/U-N4 replaces (never inside identifiers if U-Q1=keep) |
| `Reactive UI` display strings | 8 × 7 files | §6.B |
| `yanivkalfa` URLs | 10 × 5 files (LICENSE-COMMERCIAL.md, README.md, docs TopBar/Licensing/GettingStarted pages) | §6.A |
| `reactiveuitoolkit.info` | 11 refs incl. `ReactiveUIToolKitDocs~/public/CNAME` | UNCHANGED (aligned) |
| Family-core corpus | 0 brand tokens | unaffected — no re-pin |
| Extension identity fields | vscode `repository.url` points at WRONG org | §6.A bugfix |
| UPM manifest | displayName defective | §6.B bugfix |
| Committed generated outputs | 0 `*.uitkx.g.cs` committed | nothing to regenerate |

**Tier 3 — historical record (frozen except live URLs):** CHANGELOG.md entry BODIES,
`Plans~/DISCORD_CHANGELOG.md` past entries, `Plans~/archive/**`, MIGRATION docs' bodies.
Live URLs inside them DO update (§6.A).

## 4. Phase 1 — org **[OWNER]**

The org `reactive-ui-toolkit` is created once for the family (Godot leg's plan, Phase 1). No
Unity-specific org work. The docs domain (`reactiveuitoolkit.info`) already serves this leg —
after transfer, verify the Pages custom-domain setting survives (it transfers with the repo;
re-save DNS check if the dashboard flags it).

## 5. Phase 2 — transfer + rename **[OWNER]**

1. Transfer `yanivkalfa/ReactiveUIToolKit` → org; rename to `ruitk-unity`.
2. Verify post-transfer: Actions on; secrets present (store/marketplace PATs, `UNITY_*`
   credentials for the license-activation CI); rulesets/branch protection intact; Pages +
   CNAME (`reactiveuitoolkit.info`) still bound.
3. Executor: `git remote set-url origin https://github.com/reactive-ui-toolkit/ruitk-unity.git`;
   `git fetch origin` must succeed.
4. **Never reuse** the freed `yanivkalfa/ReactiveUIToolKit` name. UPM installs via git URL
   keep working through redirects, but docs must advertise the new URL (§6).

## 6. Phase 3 — the in-repo rename (branch `rebrand/umbrella` off `dev`; one commit per group)

### Group A — URL swap (10 occurrences, 5 files + the wrong-org bugfix)

Replace every `https://github.com/yanivkalfa/ReactiveUIToolKit` (with or without `.git`) →
`https://github.com/reactive-ui-toolkit/ruitk-unity`:
1. `LICENSE-COMMERCIAL.md`
2. `README.md`
3. `ReactiveUIToolKitDocs~/src/components/TopBar/TopBar.tsx` (line ~52, has `.git` suffix)
4. `ReactiveUIToolKitDocs~/src/pages/Licensing/LicensingPage.tsx` (2 URLs)
5. `ReactiveUIToolKitDocs~/src/pages/UITKX/GettingStarted/UitkxGettingStartedPage.example.ts`
6. **Bugfix:** `ide-extensions~/vscode/package.json` `repository.url`:
   OLD `https://github.com/ReactiveUITK/ReactiveUIToolKit` (wrong org) →
   NEW `https://github.com/reactive-ui-toolkit/ruitk-unity`.
Verify: `git grep -c "yanivkalfa"` → 0; `git grep -c "github.com/ReactiveUITK/"` → 0.

### Group B — display names, licenses, manifests

**B1 root `package.json` (UPM):** `"displayName": "Reactive UIToolKit"` → `"<U-N3>"`
(bugfix + rebrand in one); `"author": { "name": "ReactiveUIToolKit" }` → `"<U-N8>"`;
`"name": "com.reactiveuitoolkit"` — DO NOT TOUCH.
**B2 `README.md`:** H1 `# ReactiveUIToolKit` → `# <U-N3>`; opening sentence + body display
uses of `ReactiveUIToolKit` → `<U-N3>` (grep the file; skip code blocks, `using` samples, and
paths); the docs links to `reactiveuitoolkit.info` stay.
**B3 LICENSE set (4 files, 10 label occurrences):** in `LICENSE.md` replace all 3
`ReactiveUIToolKit (Unity)` → `<U-N4>`; per family R4 the credit line stays; then copy to the
shipped duplicates: `cp LICENSE.md ide-extensions~/vscode/LICENSE && cp LICENSE.md
ide-extensions~/visual-studio/UitkxVsix/LICENSE.txt` (byte-identical trio); update
`LICENSE-COMMERCIAL.md`'s one label.
**B4 docs site:** `ReactiveUIToolKitDocs~/index.html` title → `<U-N3> — Documentation`;
`TopBar.tsx` logo alt + header text → `<U-N3>`; sweep remaining display uses:
`git grep -n "ReactiveUIToolKit" -- "ReactiveUIToolKitDocs~/src" | grep -v ".meta"` — update
DISPLAY occurrences (FAQPage, RoadmapPage, UitkxComponentsPage, UitkxConceptsPage,
LicensingPage), keep code samples' `using ReactiveUITK` and paths verbatim.
**B5 CHANGELOG.md header:** intro line "All notable changes to the ReactiveUIToolKit Unity
package" → `<U-N3>` phrasing (the file starts with a UTF-8 BOM — preserve it; entry BODIES
frozen).
**B6 `Plans~/DISCORD_CHANGELOG.md`:** header/format prose only if it names the product;
past entries frozen.
**B7 extension manifests:** display names, ids, publisher — ALL UNCHANGED (U-N9). Only
descriptions' product-label phrases (e.g. `(ReactiveUIToolKit for Unity)` if present — grep
`git grep -n "ReactiveUIToolKit" ide-extensions~/vscode/package.json
ide-extensions~/visual-studio/UitkxVsix/source.extension.vsixmanifest`) → `<U-N3>`.
**B8 `CLAUDE.md` / `AUTOMATION.md`:** repo-description phrasing → umbrella wording + new URL;
commands/paths untouched.

### Group C — Asset Store packaging (per U-Q2)

`.github/workflows/publish.yml`:
- line ~450 comment + ~606/633 artifact paths + ~645 artifact name + ~663/668 release title:
  `ReactiveUIToolKit-${VERSION}.unitypackage` → per U-Q2 ((a): `ReactiveUIToolkit-…`;
  release title → `<U-N3> $VERSION`).
- line ~492 `STORE_DIR="shell/Assets/ReactiveUIToolKit"` → per U-Q2 ((a):
  `shell/Assets/ReactiveUIToolkit`).
- `CICD/Editor/AssetStoreExport.cs`: grep for the staging folder string and align with U-Q2.
If U-Q2=(a): add the Linux-user migration note to the wave's changelog + store description.

### Group D — U-Q1 branch point

If U-Q1 = KEEP (rec): **no identifier changes anywhere** — skip to Group E.
If U-Q1 = RENAME: STOP — this plan must be extended with the namespace/assembly/DLL/codemod
table first (request plan v2; do not improvise a 3,582-occurrence rename).

### Group E — release wave

Versions per U-N15. Lane structure per this repo's `changelog` skill: root CHANGELOG.md
section (rebrand story, org move, links redirect, UPM displayName fix, U-Q2 migration note if
(a), "no code changes" if U-Q1=keep) + `ide-extensions~/changelog.json` entries ONLY if
extension content shipped changed (descriptions did → patch bump vscode/vs2022 + extract +
`node scripts/changelog.mjs verify`) + `Plans~/DISCORD_CHANGELOG.md` entry (≤2000 chars).

### Group F — expected-leftovers audit (defines DONE)

- `git grep -c "yanivkalfa"` → 0.
- `git grep -c "github.com/ReactiveUITK/"` → 0.
- `git grep -l "ReactiveUIToolKit (Unity)"` → 0.
- `git grep -n "Reactive UIToolKit"` → 0 (the defective displayName is gone).
- `git grep -n "ReactiveUIToolKit"` → remaining ONLY: identifier/namespace contexts if
  U-Q1=keep is N/A (that's `ReactiveUITK`), the docs FOLDER name `ReactiveUIToolKitDocs~`
  (U-N14 keep), Tier-3 bodies, `Plans~/archive/**`, and U-Q2=(b) packaging strings. Each hit
  must match this list; anything else = missed step, report.
- `git grep -c "ReactiveUITK"` → UNCHANGED from baseline 3,582 (proves U-Q1=keep touched no
  identifiers).

### Group G — sync + regeneration

`node scripts/changelog.mjs verify` green; if extension descriptions changed, regenerate the
marketplace pages (`extract-overview --ide vscode` and `--ide vs2022` per the changelog
skill) and commit template+output together.

### Group H — full verification battery

1. `dotnet test SourceGenerator~/Tests` — NOTE the pre-existing 2 failures live in the
   uncommitted HmrTests sample debris (parallel-session leftovers, `JustFile.Utils
   copy.uitkx`); if the working tree still carries them, expected = 1711+ pass / 2 known
   fails on that file ONLY. Any OTHER failure = this wave broke something.
2. `dotnet test ide-extensions~/lsp-server/Tests` → 152/152 (or current baseline).
3. Docs: `npm run build` in `ReactiveUIToolKitDocs~` (lint has 2 pre-existing errors in
   SearchModal.tsx/VersionContext.tsx — not this wave's).
4. `node scripts/corpus-hash.mjs --check` → UNCHANGED.
5. Push `rebrand/umbrella` only; **[OWNER]** PR → dev → checks → merge → master
   fast-forward per house flow.

## 7. Phase 4 — consoles + stores **[OWNER]**

1. Publish run (unitypackage under the new name per U-Q2, GitHub release titled `<U-N3>`).
2. **Unity Asset Store dashboard:** listing title/description → `<U-N3>` + new repo URL;
   if U-Q2=(a) add the Linux migration note to the listing.
3. Marketplaces (VS Code/Open VSX/VS2022/JetBrains): NOTHING manual — identities and display
   names unchanged; new versions carry updated content.
4. Discord: post the wave entry; pins keep working (docs domain unchanged!).
5. Org/repo cosmetics: description, website=`reactiveuitoolkit.info`, topics.

## 8. Aftermath + rollback

- Memory/plan cross-refs updated by the assistant. The Godot plan's §8 sibling table marks
  this leg done when merged.
- Rollback: Phase 3 = one branch (don't merge); transfer reversible; store edits re-editable.

## Annex A (optional, rec: keep) — renaming `ReactiveUIToolKitDocs~`
Reference list before attempting: `git grep -n "ReactiveUIToolKitDocs~"` (publish.yml ×3,
CLAUDE.md, AUTOMATION.md, README) + `git mv`. Zero user impact either way — pure tidiness.
