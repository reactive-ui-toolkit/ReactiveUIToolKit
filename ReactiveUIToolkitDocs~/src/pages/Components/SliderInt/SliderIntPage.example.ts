export const SLIDER_INT_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;
using UnityEngine.UIElements;

public static class SliderIntExamples
{
  // Function component – pass SliderIntExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(5);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue(evt.newValue);
    }

    return V.SliderInt(
      new SliderIntProps
      {
        LowValue = 0,
        HighValue = 10,
        Value = value,
        Direction = "Horizontal",
      }
    );
  }
}`

