import type { FC } from 'react'
import { Alert, Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './MigrationPage.style'

const CODEMOD_COMMANDS = `# rewrite your own code (never edits inside the package folder)
dotnet run --project Assets/ReactiveUIToolkit/SourceGenerator~/Tools/RuitkMigrateBrand -- Assets

# prove idempotence / gate it in CI: exits non-zero if anything would still change
dotnet run --project Assets/ReactiveUIToolkit/SourceGenerator~/Tools/RuitkMigrateBrand -- Assets --check`

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
    </List>

    <Alert severity="warning">
      <strong>Linux note:</strong> Windows and macOS merge the two folder casings, so an
      in-place upgrade collapses into one folder. On case-sensitive filesystems you end up
      with <em>both</em> <code>ReactiveUIToolKit</code> and <code>ReactiveUIToolkit</code>{' '}
      side by side — delete the old capital-K folder or every type exists twice.
    </Alert>

    {/* ── The codemod ─────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Run the codemod
    </Typography>
    <Typography variant="body1" paragraph>
      After deleting the old folder and importing 0.12.0, rewrite your own{' '}
      <code>.cs</code> / <code>.uitkx</code> / <code>.asmdef</code> files with the bundled{' '}
      <code>RuitkMigrateBrand</code> tool. It prints per-file replacement counts and a
      second run reports 0.
    </Typography>
    <CodeBlock language="bash" code={CODEMOD_COMMANDS} />

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
          secondary={<>The UPM package id <code>com.reactiveuitoolkit</code>, the <code>.uitkx</code> language and UITKX tooling brand, the IDE extension identities, and <code>RUITK</code>-prefixed titles and pref keys.</>}
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
