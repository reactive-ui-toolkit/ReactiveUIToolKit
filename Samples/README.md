# Samples

## Directory Structure

### Components/

Declarative `.uitkx` components demonstrating hooks, fiber behavior,
and runtime features. Each component has a `.uitkx` file, with optional
companion `.hooks.uitkx` (hook exports) and `.style.uitkx`
(style value exports) files imported by the component.

Includes 25+ demos: counters, context, effects, portals, routing,
signals, synthetic events, keyed diffing, and more.

`UguiStressTest/` is the `@backend ugui` twin of `StressTest/` — the same
hook-driven flow rendered through classic Unity UI instead of UI Toolkit.

### Shared/

Shared data models, utility functions, and reusable `.uitkx` components
used across multiple demos (list views, tree views, animations).

### Showcase/

Multi-page demo application hosting all samples together.
Contains EditorWindow entries for in-editor preview.

- `Editor/` — Individual demo windows (one per component)
- `Both/` — Showcase All aggregated demo page
- `Runtime/` — Play-mode bootstraps, including the uGUI demos
  (`RuntimeUguiGalleryDemo`, `RuntimeUguiStressTestDemo`) — uGUI cannot
  render in editor windows, so these mount in a scene

### Shared/ (Common Utilities)

Reusable demo components used across categories:
animations, shared layouts, navigation bars.
