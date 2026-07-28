import type { FC } from 'react'
import { Alert, Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './MigrationPage.style'

const MANIFEST_SNIPPET = `{
  "dependencies": {
    // was: "com.reactiveuitoolkit": "https://github.com/<old-org>/<old-repo>.git#dist"
    "com.reactiveuitoolkit": "https://github.com/reactive-ui-toolkit/ruitk-unity.git#dist"
  }
}`

const CODEMOD_COMMANDS = `# The tool ships in the REPOSITORY, not in the package — Asset Store exports and
# UPM installs do not contain SourceGenerator~ (Unity never imports "~" folders).
# So clone the repo and run it from there, against YOUR project.
git clone https://github.com/reactive-ui-toolkit/ruitk-unity.git
cd ruitk-unity

# rewrite your own code (never edits inside the package folder)
dotnet run --project SourceGenerator~/Tools/RuitkMigrateBrand -- /path/to/YourProject/Assets

# prove idempotence / gate it in CI: exits non-zero if anything would still change
dotnet run --project SourceGenerator~/Tools/RuitkMigrateBrand -- /path/to/YourProject/Assets --check`

export const MigrationPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Migrating to 0.12 — the Ruitk rename
    </Typography>

    <Typography variant="body1" paragraph>
      0.12.0 is the family-rebrand release: the library is now{' '}
      <strong>Reactive UI Toolkit — Unity</strong>, hosted at{' '}
      <a href="https://github.com/reactive-ui-toolkit/ruitk-unity" target="_blank" rel="noreferrer">
        github.com/reactive-ui-toolkit/ruitk-unity
      </a>
      . Nothing changed functionally — but the code identity did, so upgrading an existing
      project is a breaking (two-step) migration.
    </Typography>

    {/* ── What renamed ────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      What renamed
    </Typography>
    <List sx={Styles.list}>
      <ListItem>
        <ListItemText
          primary={<>Namespace / assembly root <code>ReactiveUITK</code> → <code>Ruitk</code></>}
          secondary={<>Every <code>using</code>, <code>namespace</code>, and <code>global::</code> reference; all asmdefs are now <code>Ruitk.*</code>.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<>Analyzer DLLs → <code>Analyzers/Ruitk.Language.dll</code> + <code>Analyzers/Ruitk.SourceGenerator.dll</code></>}
          secondary="Renamed with their GUIDs preserved, so existing references survive."
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<>Install folder casing: <code>Assets/ReactiveUIToolKit</code> → <code>Assets/ReactiveUIToolkit</code></>}
          secondary="Delete the old folder before importing 0.12.0 (see the Linux warning below)."
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<>Define <code>REACTIVEUITK_HAS_TEST_FRAMEWORK</code> → <code>RUITK_HAS_TEST_FRAMEWORK</code>; <code>ReactiveUITKConfig</code> → <code>RuitkConfig</code></>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<>Hidden runtime object names <code>__ReactiveUITK_*</code> → <code>__Ruitk_*</code></>}
          secondary={<>The media subsystem&apos;s internal hosts (<code>__Ruitk_MediaHost</code>, <code>__Ruitk_VideoPeer</code>, <code>__Ruitk_AudioPeer</code>, <code>__Ruitk_Sfx</code>, <code>__Ruitk_RT_&lt;w&gt;x&lt;h&gt;</code>) — only relevant if you <code>GameObject.Find</code> them.</>}
        />
      </ListItem>
      {/* M14 — these two were in MIGRATION-0.12.md but missing from this page. */}
      <ListItem>
        <ListItemText
          primary={<>Editor menu root: <code>ReactiveUITK/…</code> → <code>Reactive UI Toolkit/…</code></>}
          secondary={<>All 53 menu items moved. Update your own <code>[MenuItem(&quot;ReactiveUITK/…&quot;)]</code> attributes and any <code>ExecuteMenuItem</code> call — note the new root is the <em>display</em> name, not <code>Ruitk</code>.</>}
        />
      </ListItem>
      {/* M5 — the user-visible default-namespace change. */}
      <ListItem>
        <ListItemText
          primary={<>Default generated namespace for <em>your</em> <code>.uitkx</code> files: <code>ReactiveUITK.Uitkx</code> → <code>Ruitk.Uitkx</code> (and <code>ReactiveUITK.FunctionStyle</code> → <code>Ruitk.FunctionStyle</code>)</>}
          secondary={<>Applies to every component without an explicit <code>@namespace</code> / <code>namespacePrefix</code> — your own components change namespace. The codemod fixes compile-time references, but <strong>reflection and serialized type names break at runtime, not compile time</strong>: grep for <code>&quot;ReactiveUITK.Uitkx</code> and <code>&quot;ReactiveUITK.FunctionStyle</code> in <code>Type.GetType(…)</code> calls and serialized assembly-qualified names before shipping.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary="Licensing moved to Reactive UI Toolkit Community License 1.1"
          secondary="Versions you already shipped keep the license and terms they shipped with; 0.12.0 onwards is 1.1."
        />
      </ListItem>
    </List>

    {/* ── Before you start ────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Before you start
    </Typography>

    {/* M6 — config.json lives inside the folder step 1 tells you to delete. */}
    <Alert severity="warning" sx={{ mb: 2 }}>
      <strong>Back up your <code>config.json</code> first.</strong> The user-editable{' '}
      <code>Assets/ReactiveUIToolkit/config.json</code> (<code>env</code>,{' '}
      <code>traceLevel</code>, <code>diffTracing</code>, <code>exceptionControlFlow</code>)
      lives <em>inside</em> the package folder, so deleting the old folder deletes it and
      your settings silently reset to defaults. Copy it somewhere safe and merge your
      values back afterwards. The codemod also rewrites files in place and makes no
      backups — commit or back up your project before running it.
    </Alert>

    {/* L10 — the .NET SDK prerequisite was never stated. */}
    <Alert severity="info" sx={{ mb: 2 }}>
      <strong>The codemod needs the .NET 8 SDK.</strong> Unity does not ship one, so{' '}
      <code>dotnet</code> will not be on your PATH just because Unity is installed. Install
      it from{' '}
      <a href="https://dotnet.microsoft.com/download/dotnet/8.0" target="_blank" rel="noreferrer">
        dotnet.microsoft.com
      </a>{' '}
      — <code>dotnet --version</code> should print 8.x or newer.
    </Alert>

    <Alert severity="warning">
      <strong>Linux note:</strong> Windows and macOS merge the two folder casings, so an
      in-place upgrade collapses into one folder. On case-sensitive filesystems you end up
      with <em>both</em> <code>ReactiveUIToolKit</code> and <code>ReactiveUIToolkit</code>{' '}
      side by side — delete the old capital-K folder or every type exists twice. The
      casing matters even when you only see one folder: every path literal in the package
      now says <code>ReactiveUIToolkit</code>, so a surviving capital-K folder means{' '}
      <code>config.json</code> is not found and the library falls back to defaults —
      typically on a Linux build agent days later, not on your machine.
    </Alert>

    {/* ── UPM users ───────────────────────────────────────────────────────── */}
    {/* H4 — the UPM git URL changed and nothing told anyone. */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      UPM users: update the git URL
    </Typography>
    <Typography variant="body1" paragraph>
      If you installed via Package Manager you have no <code>Assets/ReactiveUIToolKit</code>{' '}
      folder to delete — but the repository moved, so the old URL will not deliver 0.12.0,
      and once it stops resolving your project will not open. Update{' '}
      <code>Packages/manifest.json</code>:
    </Typography>
    <CodeBlock language="json" code={MANIFEST_SNIPPET} />
    <Typography variant="body1" paragraph>
      The package id <code>com.reactiveuitoolkit</code> is unchanged. Pin a release with{' '}
      <code>…ruitk-unity.git#v0.12.0</code> if you prefer tags to the rolling{' '}
      <code>#dist</code> branch. Then let Package Manager re-resolve, or delete{' '}
      <code>Library/PackageCache/com.reactiveuitoolkit@*</code> to force it.
    </Typography>

    {/* ── The codemod ─────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Run the codemod
    </Typography>
    <Typography variant="body1" paragraph>
      After deleting the old folder and importing 0.12.0, rewrite your own{' '}
      <code>.cs</code> / <code>.uitkx</code> / <code>.asmdef</code> / <code>.asmref</code> /{' '}
      <code>.rsp</code> files with the <code>RuitkMigrateBrand</code> tool. It prints
      per-file replacement counts and a second run reports 0. It preserves each file&apos;s
      byte-order mark, skips (with a warning) any file that is not valid UTF-8 rather than
      corrupting it, and reports rather than crashes when a file cannot be written — but{' '}
      <strong>check your sources out of Perforce/Plastic first</strong>, since both keep
      unopened files read-only. <code>--help</code> prints the full rule list.
    </Typography>
    <CodeBlock language="bash" code={CODEMOD_COMMANDS} />

    {/* ── Delete the orphaned folder ──────────────────────────────────────── */}
    {/* H1 + M1 — the generated trigger/registry folder nothing cleans up. */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Then delete <code>Assets/ReactiveUITK/</code>
    </Typography>
    <Alert severity="error" sx={{ mb: 2 }}>
      This is <strong>not</strong> the package folder — it is the small folder the editor
      integration generates, which 0.12.0 moved to <code>Assets/Ruitk/</code>. Delete{' '}
      <code>Assets/ReactiveUITK/</code> and its <code>.meta</code> after migrating.
    </Alert>
    <List sx={Styles.list}>
      <ListItem>
        <ListItemText
          primary={<><code>Assets/ReactiveUITK/Resources/__uitkx_registry.asset</code></>}
          secondary={<>The asset registry is loaded <strong>by name</strong> (<code>Resources.Load(&quot;__uitkx_registry&quot;)</code>), so a leftover copy competes with the new one under <code>Assets/Ruitk/Resources/</code>. When the stale one wins, every <code>Asset&lt;T&gt;()</code> / <code>Ast&lt;T&gt;()</code> lookup and every <code>uss=</code> stylesheet added or changed after the upgrade returns null — in the editor <em>and</em> in player builds. Nothing self-heals it; the editor logs a warning if it spots this.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<><code>Assets/ReactiveUITK/UITKX_GeneratorTrigger.g.cs</code></>}
          secondary="An obsolete recompile trigger. The codemod deliberately skips this folder so the stale trigger keeps its old namespace and does not collide with the new one — deleting the folder is the clean end state."
        />
      </ListItem>
    </List>

    {/* ── Manual steps ────────────────────────────────────────────────────── */}
    {/* M4 — file kinds/settings the codemod cannot reach. */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Manual steps the codemod cannot do
    </Typography>
    <List sx={Styles.list}>
      <ListItem>
        <ListItemText
          primary={<>Project Settings scripting defines</>}
          secondary={<>If you added <code>REACTIVEUITK_HAS_TEST_FRAMEWORK</code> under <em>Project Settings ▸ Player ▸ Other Settings ▸ Scripting Define Symbols</em> (per build target!), rename it to <code>RUITK_HAS_TEST_FRAMEWORK</code> by hand — Unity stores it in <code>ProjectSettings/ProjectSettings.asset</code>, which is not scanned. A stale define fails <strong>silently</strong>: the guarded block just stops compiling in.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={<>Your own <code>uitkx.config.json</code>, if it names <code>ReactiveUIToolKit</code> paths</>}
          secondary={<><code>.json</code> is deliberately not scanned — a user&apos;s <code>.vscode/settings.json</code> legitimately contains the frozen <code>ReactiveUITK.uitkx</code> extension id, which must not be rewritten.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary="Reflection and serialized type-name strings"
          secondary="See the default-namespace item under “What renamed” — these break at runtime, not compile time."
        />
      </ListItem>
    </List>

    {/* ── Notes ───────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Notes
    </Typography>
    <List sx={Styles.list}>
      <ListItem>
        <ListItemText
          primary={<>The editor-prefs key <code>ReactiveUITK.UitkxNavVerbose</code> is now <code>Ruitk.UitkxNavVerbose</code></>}
          secondary="Your saved value resets once — cosmetic only."
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary="Unchanged on purpose"
          secondary={<>The UPM package id <code>com.reactiveuitoolkit</code>, the <code>.uitkx</code> language and UITKX tooling brand, the IDE extension marketplace identities (<code>ReactiveUITK.uitkx</code>, <code>UitkxVsix.ReactiveUITK</code> — these keep the old token deliberately and must never be rewritten), and <code>RUITK</code>-prefixed titles and pref keys.</>}
        />
      </ListItem>
      <ListItem>
        <ListItemText
          primary={
            <>
              Full hand-migration rules:{' '}
              <a
                href="https://github.com/reactive-ui-toolkit/ruitk-unity/blob/HEAD/MIGRATION-0.12.md"
                target="_blank"
                rel="noreferrer"
              >
                MIGRATION-0.12.md
              </a>{' '}
              in the repository.
            </>
          }
        />
      </ListItem>
    </List>
  </Box>
)
