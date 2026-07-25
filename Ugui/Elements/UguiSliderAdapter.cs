using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Menu-identical Slider skeleton via DefaultControls (Background /
    /// Fill Area/Fill / Handle Slide Area/Handle), with sprites supplied
    /// through props. Value writes use SetValueWithoutNotify.
    /// </summary>
    public sealed class UguiSliderAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return DefaultControls.CreateSlider(UguiDefaultResources.GetLegacyResources());
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiSliderProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiSliderProps, next as UguiSliderProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiSliderProps prev, UguiSliderProps next)
        {
            if (next == null)
                return;
            var slider = go.GetComponent<Slider>();
            if (slider == null)
                return;
            bool full = prev == null;

            UguiSelectableApplier.Apply(slider, prev, next);

            if ((full || next.MinValue != prev.MinValue) && next.MinValue.HasValue)
                slider.minValue = next.MinValue.Value;
            if ((full || next.MaxValue != prev.MaxValue) && next.MaxValue.HasValue)
                slider.maxValue = next.MaxValue.Value;
            if ((full || next.WholeNumbers != prev.WholeNumbers) && next.WholeNumbers.HasValue)
                slider.wholeNumbers = next.WholeNumbers.Value;
            if ((full || next.Direction != prev.Direction) && next.Direction.HasValue)
                slider.direction = next.Direction.Value;
            if (full || next.OnValueChanged != prev.OnValueChanged)
                UguiSliderBinding.GetOrAdd(go).Current = next.OnValueChanged;
            if ((full || next.Value != prev.Value) && next.Value.HasValue)
                slider.SetValueWithoutNotify(next.Value.Value);

            ApplyPart(
                go.transform.Find("Background"),
                next.BackgroundSprite,
                next.BackgroundColor,
                full,
                prev?.BackgroundSprite,
                prev?.BackgroundColor
            );
            ApplyPart(
                slider.fillRect,
                next.FillSprite,
                next.FillColor,
                full,
                prev?.FillSprite,
                prev?.FillColor
            );
            ApplyPart(
                slider.handleRect,
                next.HandleSprite,
                next.HandleColor,
                full,
                prev?.HandleSprite,
                prev?.HandleColor
            );
        }

        internal static void ApplyPart(
            Transform part,
            Sprite sprite,
            Color? color,
            bool full,
            Sprite prevSprite,
            Color? prevColor
        )
        {
            if (part == null)
                return;
            var image = part.GetComponent<Image>();
            if (image == null)
                return;
            if ((full || !ReferenceEquals(sprite, prevSprite)) && sprite != null)
                image.sprite = sprite;
            if ((full || color != prevColor) && color.HasValue)
                image.color = color.Value;
        }
    }

    public sealed class UguiScrollbarAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return DefaultControls.CreateScrollbar(UguiDefaultResources.GetLegacyResources());
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiScrollbarProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiScrollbarProps, next as UguiScrollbarProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiScrollbarProps prev, UguiScrollbarProps next)
        {
            if (next == null)
                return;
            var bar = go.GetComponent<Scrollbar>();
            if (bar == null)
                return;
            bool full = prev == null;

            UguiImageApplier.ApplyDiffOrFull(go.GetComponent<Image>(), prev, next);
            UguiSelectableApplier.Apply(bar, prev, next);

            if ((full || next.Size != prev.Size) && next.Size.HasValue)
                bar.size = next.Size.Value;
            if ((full || next.NumberOfSteps != prev.NumberOfSteps) && next.NumberOfSteps.HasValue)
                bar.numberOfSteps = next.NumberOfSteps.Value;
            if ((full || next.Direction != prev.Direction) && next.Direction.HasValue)
                bar.direction = next.Direction.Value;
            if (full || next.OnValueChanged != prev.OnValueChanged)
                UguiScrollbarBinding.GetOrAdd(go).Current = next.OnValueChanged;
            if ((full || next.Value != prev.Value) && next.Value.HasValue)
                bar.SetValueWithoutNotify(next.Value.Value);

            UguiSliderAdapter.ApplyPart(
                bar.handleRect,
                next.HandleSprite,
                next.HandleColor,
                full,
                prev?.HandleSprite,
                prev?.HandleColor
            );
        }
    }
}
