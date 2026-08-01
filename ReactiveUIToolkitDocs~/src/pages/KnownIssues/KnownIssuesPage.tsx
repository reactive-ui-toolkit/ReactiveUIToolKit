import type { FC } from 'react'
import { Alert, Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './KnownIssuesPage.style'

export const KnownIssuesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Known Issues
    </Typography>

    {/* ── Unity 6.5 text generation (ATG) ─────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Unity 6.5: Advanced Text Generator is now the default
    </Typography>
    <Typography variant="body1" paragraph>
      Unity 6.5 makes the Advanced Text Generator (ATG) the default text system for UI Toolkit at
      runtime. Unity states <em>feature</em> parity with the previous generator, but not{' '}
      <em>measurement</em> parity: ATG shapes text through a different stack, so measured text sizes
      and the exact points at which lines wrap can differ from Unity 6.4 and earlier.
    </Typography>
    <Typography variant="body1" paragraph>
      If a layout of yours depends on precise text metrics or on specific wrap points, compare it
      across 6.4 and 6.5 after upgrading. Community reports (English, Chinese and Hebrew) describe
      ATG breaking lines at commas and periods where the previous generator did not.
    </Typography>
    <Typography variant="body1" paragraph>
      The opt-out is a <strong>cascading USS property</strong>, not a project setting, so you can
      switch generators for a single subtree rather than the whole application:
    </Typography>
    <CodeBlock
      language="jsx"
      code={`// Typed Style API - already supported, no library changes needed
new Style {
    UnityTextGenerator = new StyleEnum<TextGeneratorType>(TextGeneratorType.Standard),
}

/* or in USS, inherited by everything below the selector */
:root {
    -unity-text-generator: standard;
}`}
    />
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Static font assets are not supported by ATG - migrate them to dynamic font assets." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={'Rich text parsing is stricter: <style=blue> must become <style="blue">, and <align=flush> is unsupported.'} />
      </ListItem>
    </List>
    <Alert severity="info" sx={{ mt: 1 }}>
      Unity has said it intends to remove the opt-out in a future release, so treat{' '}
      <code>-unity-text-generator: standard</code> as a migration runway rather than a permanent
      setting.
    </Alert>

    {/* ── Runtime ─────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Runtime
    </Typography>
    <Typography variant="body1" paragraph>
      There is a known issue where <code>MultiColumnListView</code> can briefly
      jump or snap when scrolling large data sets; this will be addressed in a
      future update.
    </Typography>

    {/* ── Burst AOT ───────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Burst AOT &amp; Assembly Resolution
    </Typography>
    <Typography variant="body1" paragraph>
      If you encounter the error:
    </Typography>
    <CodeBlock
      language="jsx"
      code="Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor"
    />
    <Typography variant="body1" paragraph>
      Go to <strong>Edit → Project Settings → Burst AOT Settings</strong> and
      add <code>Assembly-CSharp-Editor</code> to the exclusion list. This
      prevents Burst from trying to AOT-compile editor-only assemblies that
      reference UITKX types.
    </Typography>

    {/* ── HMR Limitations ─────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      HMR Limitations
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Old assemblies from HMR swaps cannot be unloaded by Mono. Each swap leaks approximately 10–30 KB. This is negligible for normal development sessions but accumulates over very long sessions." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="The first HMR compile is ~1–1.5 seconds due to Roslyn JIT warmup. Subsequent compiles are 25–100 ms." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Brand-new .uitkx files are auto-discovered while HMR runs (the watcher covers Assets/, and unknown components referenced from an edited file are found and compiled automatically) — no restart needed. The new component behaves like any other on subsequent saves." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="A file declaring MULTIPLE components hot-swaps only its FIRST component during an HMR session; edits to the later components in that file take effect on the next full compile. One component per file (the recommended convention) avoids this entirely." />
      </ListItem>
    </List>

    {/* ── Render Depth ────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Component Tree Depth
    </Typography>
    <Typography variant="body1" paragraph>
      The reconciler enforces a maximum render depth of <strong>25</strong>{' '}
      nested re-renders per component. If a component calls{' '}
      <code>setState</code> unconditionally during its setup code (creating an
      infinite render loop), the depth guard stops it and logs an error. This is
      not configurable — restructure your component to move state updates into
      event handlers or effects.
    </Typography>

    {/* ── Hooks ───────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Hook Constraints
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Hooks must be called unconditionally at the top of the component's setup code. Calling hooks inside @if, @for, or other control blocks breaks hook ordering and causes runtime errors." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Thread safety: hooks are NOT thread-safe. All hook calls must happen on the main thread during the render cycle. Signal values can be read/written from any thread, but UseSignal() itself is a hook and follows hook rules." />
      </ListItem>
    </List>

    {/* ── Editor vs Runtime ───────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Editor vs Runtime Differences
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<>Editor uses <code>EditorRenderScheduler</code> (tied to <code>EditorApplication.update</code>), while runtime uses <code>RenderScheduler</code> (tied to <code>MonoBehaviour.Update</code>). Scheduling timing may differ slightly.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<>Drag events (<code>onDragEnter</code>, <code>onDragLeave</code>, <code>onDragUpdated</code>, <code>onDragPerform</code>, <code>onDragExited</code>) are editor-only and require <code>UNITY_EDITOR</code>.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<>Some components are editor-only: <code>PropertyField</code>, <code>InspectorElement</code>, <code>ObjectField</code>, <code>ColorField</code>, <code>EnumFlagsField</code>, <code>Toolbar</code> and its children, <code>TwoPaneSplitView</code>, <code>HelpBox</code>, <code>IMGUIContainer</code>.</>} />
      </ListItem>
    </List>

    <Alert severity="info" sx={{ mt: 2 }}>
      For troubleshooting build or LSP issues, see the{' '}
      <strong>Debugging Guide</strong>.
    </Alert>
  </Box>
)
