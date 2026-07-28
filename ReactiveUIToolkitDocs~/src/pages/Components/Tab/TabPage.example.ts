export const TAB_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;

public static class TabExamples
{
  // Function component – pass TabExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Tab(
      new TabProps
      {
        Text = "Tab title",
      }
    );
  }
}`

