export const INTEGER_FIELD_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;
using UnityEngine.UIElements;

public static class IntegerFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass IntegerFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(42);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue(evt.newValue);
    }

    return V.IntegerField(
      new IntegerFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Integer" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`
