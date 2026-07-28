export const LABEL_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;

public static class LabelExamples
{
  private static readonly Style LabelStyle = new Style { (StyleKeys.FontSize, 16f) };

  // Function component – pass LabelExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Label(
      new LabelProps
      {
        Text = "Hello label",
        Style = LabelStyle,
      }
    );
  }
}`
