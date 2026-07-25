using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// TMP_Dropdown with the exact GameObject &gt; UI &gt; Dropdown template
    /// (via TMP_DefaultControls): Label/Arrow/Template/Viewport/Content/Item.
    /// Value writes use SetValueWithoutNotify.
    /// </summary>
    public sealed class UguiDropdownAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiDropdownProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiDropdownProps, next as UguiDropdownProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiDropdownProps prev, UguiDropdownProps next)
        {
            if (next == null)
                return;
            var dropdown = go.GetComponent<TMP_Dropdown>();
            if (dropdown == null)
                return;
            bool full = prev == null;

            UguiImageApplier.ApplyDiffOrFull(go.GetComponent<Image>(), prev, next);
            UguiSelectableApplier.Apply(dropdown, prev, next);

            if (
                (full || !UguiDropdownProps.OptionsEqual(next.Options, prev.Options))
                && next.Options != null
            )
            {
                var data = new List<TMP_Dropdown.OptionData>(next.Options.Count);
                for (int i = 0; i < next.Options.Count; i++)
                    data.Add(new TMP_Dropdown.OptionData(next.Options[i]));
                dropdown.options = data;
            }

            var label = dropdown.captionText;
            if (label != null)
            {
                if ((full || next.LabelFontSize != prev.LabelFontSize) && next.LabelFontSize.HasValue)
                    label.fontSize = next.LabelFontSize.Value;
                if ((full || next.LabelColor != prev.LabelColor) && next.LabelColor.HasValue)
                    label.color = next.LabelColor.Value;
            }

            if (full || next.OnValueChanged != prev.OnValueChanged)
                UguiDropdownBinding.GetOrAdd(go).Current = next.OnValueChanged;
            if ((full || next.Value != prev.Value) && next.Value.HasValue)
            {
                dropdown.SetValueWithoutNotify(next.Value.Value);
                dropdown.RefreshShownValue();
            }
        }
    }
}
