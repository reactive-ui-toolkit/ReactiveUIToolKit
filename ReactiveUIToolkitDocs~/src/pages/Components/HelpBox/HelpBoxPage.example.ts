export const HELP_BOX_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;

public static class HelpBoxExamples
{
  // Function component – pass HelpBoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.HelpBox(
      new HelpBoxProps
      {
        Text = "Something went wrong.",
        MessageType = "Error",
      }
    );
  }
}`

