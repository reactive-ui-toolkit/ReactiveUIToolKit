export const MIN_MAX_SLIDER_BASIC = `// Example namespace: Ruitk.Samples.Components

using System.Collections.Generic;
using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;

public static class MinMaxSliderExamples
{
  private static readonly Style SliderStyle = new Style { (StyleKeys.Width, 200f) };

  // Function component – pass MinMaxSliderExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (range, setRange) = Hooks.UseState((min: 20f, max: 80f));

    void Update(float min, float max)
    {
      setRange(_ => (min, max));
    }

    return V.MinMaxSlider(
      new MinMaxSliderProps
      {
        MinValue = range.min,
        MaxValue = range.max,
        LowLimit = 0f,
        HighLimit = 100f,
        Style = SliderStyle,
      }
    );
  }
}`
