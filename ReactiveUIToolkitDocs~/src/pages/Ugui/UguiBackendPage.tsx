import type { FC } from 'react'
import {
  Box,
  List,
  ListItem,
  ListItemText,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './UguiBackendPage.style'
import {
  UGUI_CSHARP_COUNTER,
  UGUI_UITKX_COUNTER,
  UGUI_PREFAB_MARKUP,
  UGUI_PREFAB_BINDING,
  UGUI_ISLANDS,
} from './UguiBackendPage.example'

const Section: FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <Box>
    <Typography variant="h5" component="h2" gutterBottom>
      {title}
    </Typography>
    {children}
  </Box>
)

export const UguiBackendPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      uGUI Backend
    </Typography>
    <Typography variant="body1" paragraph>
      The library renders classic <strong>Unity UI (uGUI)</strong> through the same fiber
      reconciler as UI Toolkit — the same components, hooks, signals, context, router, and Hot
      Module Replacement, mounted under a <code>RectTransform</code> in an existing Canvas instead
      of a <code>UIDocument</code>. Use it to bring the reactive model to projects that live on
      uGUI, or to mix both worlds via islands.
    </Typography>
    <Typography variant="body1" paragraph>
      The design principle: <strong>uGUI keeps its own mental model.</strong> Positioning is
      RectTransform anchors/pivots — with an <code>anchors</code> preset prop mirroring the
      Inspector's anchor widget (<code>MiddleCenter</code>, <code>TopStretch</code>,{' '}
      <code>Stretch</code>, ...). Styling is sprites, colors, and materials. Stacking is
      LayoutGroups. There is <strong>deliberately no <code>Style</code>/USS surface</strong> on
      uGUI elements: a uGUI tree reads like the uGUI Inspector, not like a flexbox document.
    </Typography>

    <Section title="Getting Started">
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Add a <code>UguiRootRenderer</code> component to a GameObject under your Canvas. An <strong>EventSystem</strong> is required in the scene (the renderer warns once at first render if it is missing).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Assign the target <code>RectTransform</code> in the Inspector, call <code>Initialize(rectTransform)</code>, or simply place the component on a RectTransform — the mount resolves in that order.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Call <code>Render(V.Func(...))</code> with your component. <code>U.*</code> factories are the uGUI siblings of <code>V.*</code>.</>} />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        Props come from the ugui pool via <code>UguiBaseProps.__Rent&lt;T&gt;()</code> (or object
        initializers for prop-group values). A complete counter, mirroring the bundled{' '}
        <code>Samples/UguiDemo</code> sample:
      </Typography>
      <CodeBlock language="jsx" code={UGUI_CSHARP_COUNTER} />
      <Typography variant="body1" paragraph>
        <code>UguiRootRenderer</code> does not own or create the Canvas or the EventSystem — it
        slots into the scene structure you already have, so an existing uGUI screen can adopt a
        reactive subtree one rect at a time.
      </Typography>
    </Section>

    <Section title="Using .uitkx Markup">
      <Typography variant="body1" paragraph>
        A <code>.uitkx</code> file opts into the uGUI element vocabulary with the{' '}
        <code>@backend ugui</code> directive in the preamble. Everything else — hooks, imports,
        exports, control flow, HMR — works exactly as in a UI Toolkit file:
      </Typography>
      <CodeBlock language="jsx" code={UGUI_UITKX_COUNTER} />
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>UITKX2111</code> (error) — unknown backend value (<code>@backend</code> accepts <code>ugui</code> or <code>uitk</code>, the default), or a duplicate <code>@backend</code> directive.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>UITKX2112</code> (warning) — <code>@uss</code> in a <code>@backend ugui</code> file has no effect: uGUI elements are styled with sprites, colors, and materials, not USS.</>} />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        Files without the directive are untouched — UI Toolkit emission is byte-identical whether
        or not the uGUI backend is in the project.
      </Typography>
    </Section>

    <Section title="Element Reference">
      <TableContainer component={Paper} variant="outlined" sx={Styles.table}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Element</strong></TableCell>
              <TableCell><strong>What it renders</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>Canvas</code></TableCell>
              <TableCell>A nested Canvas — its own sorting and raycast scope.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Panel</code></TableCell>
              <TableCell>Plain container rect — the uGUI counterpart of <code>VisualElement</code>.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Image</code></TableCell>
              <TableCell><code>UnityEngine.UI.Image</code> — sprite, color, material, image type, fill.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>RawImage</code></TableCell>
              <TableCell><code>RawImage</code> — raw texture (render textures, atlases) with UV rect.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Text</code></TableCell>
              <TableCell>TextMeshPro (<code>TextMeshProUGUI</code>) label — text, font size, alignment, color.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Button</code></TableCell>
              <TableCell>uGUI <code>Button</code> with <code>onClick</code>; children compose the button face.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>HorizontalLayoutGroup</code> / <code>VerticalLayoutGroup</code></TableCell>
              <TableCell>Stacking containers — spacing, padding, child control/expand flags.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>GridLayoutGroup</code></TableCell>
              <TableCell>Grid container — cell size, spacing, constraint.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Toggle</code> / <code>ToggleGroup</code></TableCell>
              <TableCell>Checkbox (controlled <code>isOn</code>) and the group that makes toggles exclusive.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Slider</code></TableCell>
              <TableCell>uGUI <code>Slider</code> — controlled value, min/max, direction.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Scrollbar</code></TableCell>
              <TableCell>Standalone <code>Scrollbar</code> — controlled value, handle size, steps.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>ScrollRect</code></TableCell>
              <TableCell>Scrolling viewport — <strong>the first child IS the content</strong> (typically a LayoutGroup with a <code>contentSizeFitter</code> prop group); the binding keeps <code>ScrollRect.content</code> pointed at it across child swaps.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Dropdown</code></TableCell>
              <TableCell>TextMeshPro <code>TMP_Dropdown</code> — options list, controlled value.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>InputField</code></TableCell>
              <TableCell>TextMeshPro <code>TMP_InputField</code> — controlled text, placeholder, content type.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Prefab</code></TableCell>
              <TableCell>Migration bridge — mounts an existing uGUI prefab with <code>IReactivePrefab</code> prop binding (see below).</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>UitkHost</code></TableCell>
              <TableCell>Island — a UI Toolkit panel inside the uGUI tree (see Islands below).</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <Typography variant="body1" paragraph>
        <strong>Prop groups</strong> are the declarative twin of "Add Component" and work on any
        element: <code>layoutElement</code>, <code>contentSizeFitter</code>,{' '}
        <code>aspectRatioFitter</code>, <code>canvasGroup</code>, <code>mask</code>,{' '}
        <code>rectMask2D</code>, <code>shadow</code>, <code>outline</code>, and{' '}
        <code>pointer</code> (the IPointer*/drag/scroll bridge). A non-null group adds or updates
        the component; a group that transitions to null removes it. Field names mirror the
        Inspector.
      </Typography>
    </Section>

    <Section title="Prefab Migration Bridge">
      <Typography variant="body1" paragraph>
        <code>&lt;Prefab source={'{...}'} bind={'{...}'}/&gt;</code> mounts an existing uGUI
        prefab inside the reactive tree — the incremental-adoption path for a project full of
        hand-built prefabs. The holder rect is full-stretch, so the prefab's own anchors behave
        exactly as they would under the mount parent. Changing <code>source</code> swaps the
        instance; <code>onInstantiated</code> fires with the fresh instance root.
      </Typography>
      <CodeBlock language="jsx" code={UGUI_PREFAB_MARKUP} />
      <Typography variant="body1" paragraph>
        Any component on the prefab root that implements <code>IReactivePrefab</code> receives the{' '}
        <code>bind</code> value whenever it changes — a one-method contract
        (<code>void Bind(object props)</code>) that keeps the prefab's internals untouched:
      </Typography>
      <CodeBlock language="jsx" code={UGUI_PREFAB_BINDING} />
    </Section>

    <Section title="Islands — Mixing Both Backends">
      <Typography variant="body1" paragraph>
        One mount owns exactly one backend for its lifetime: <code>RootRenderer</code> renders UI
        Toolkit, <code>UguiRootRenderer</code> renders uGUI — never a mixed tree. Crossing over is
        explicit, via island elements (registered automatically, no bootstrapping):
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>U.UguiHost</code> — inside a <strong>UI Toolkit</strong> tree: embeds a uGUI subtree on a screen-synced overlay canvas with native uGUI input.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>U.UitkHost</code> — inside a <strong>uGUI</strong> tree: embeds a UI Toolkit panel rendered to a render texture (via <code>PanelSettings</code>) with forwarded pointer input.</>} />
        </ListItem>
      </List>
      <CodeBlock language="jsx" code={UGUI_ISLANDS} />
    </Section>

    <Section title="Notes & Behavior">
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><strong>Controlled components.</strong> Value writes go through <code>SetValueWithoutNotify</code> / <code>SetTextWithoutNotify</code>, so re-applying props never echoes a change event back into your handlers. UnityEvents are subscribed exactly once, with delegate-field diffs.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><strong>Raycast hygiene.</strong> Visual elements default <code>raycastTarget = false</code>; interactive ones default <code>true</code> — decorative graphics never swallow clicks unless you opt in.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><strong>Driven-rect hint.</strong> Writing rect props (size, anchors) on a RectTransform that a LayoutGroup already drives is a silent no-op in raw uGUI; the backend logs an editor-only hint so the conflict is visible while you develop.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><strong>Default sprites ship with the package.</strong> Controls reference Unity's builtin UI sprites (UISprite, Background, Knob, ...), so runtime-created elements look identical to the GameObject &gt; UI menu ones — in the editor and in player builds.</>} />
        </ListItem>
      </List>
    </Section>
  </Box>
)
