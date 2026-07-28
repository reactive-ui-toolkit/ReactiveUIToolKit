export const TWO_PANE_SPLIT_VIEW_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;
using UnityEngine.UIElements;

// Editor-only usage
public static class TwoPaneSplitViewExamples
{
  // Function component – pass TwoPaneSplitViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.TwoPaneSplitView(
      new TwoPaneSplitViewProps
      {
        FixedPaneIndex = 0,
        FixedPaneInitialDimension = 220f,
        Orientation = "horizontal",
      },
      V.Box(new BoxProps(), V.Label(new LabelProps { Text = "Pane 1" })),
      V.Box(new BoxProps(), V.Label(new LabelProps { Text = "Pane 2" }))
    );
  }
}`

