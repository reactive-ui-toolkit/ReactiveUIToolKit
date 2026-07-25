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
            var go = TMP_DefaultControls.CreateDropdown(UguiDefaultResources.GetTmpResources());
            var tag = UguiNodeTag.GetOrAdd(go);
            tag.Selectable = go.GetComponent<TMP_Dropdown>();
            tag.Control = tag.Selectable;
            tag.Image = go.GetComponent<Image>();
            tag.Graphic = tag.Image;
            return go;
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
            var tag = UguiNodeTag.Find(go);
            var dropdown = tag != null ? tag.Selectable as TMP_Dropdown : go.GetComponent<TMP_Dropdown>();
            if (dropdown == null)
                return;
            bool full = prev == null;

            UguiImageApplier.ApplyDiffOrFull(
                tag != null ? tag.Image : go.GetComponent<Image>(),
                prev,
                next
            );
            UguiSelectableApplier.Apply(dropdown, prev, next);

            if (
                (
                    full
                    || !UguiDropdownProps.OptionsEqual(next.Options, prev.Options)
                    || !UguiDropdownProps.SpritesEqual(next.OptionSprites, prev.OptionSprites)
                )
                && next.Options != null
            )
            {
                var data = new List<TMP_Dropdown.OptionData>(next.Options.Count);
                for (int i = 0; i < next.Options.Count; i++)
                {
                    Sprite icon =
                        next.OptionSprites != null && i < next.OptionSprites.Count
                            ? next.OptionSprites[i]
                            : null;
                    data.Add(new TMP_Dropdown.OptionData(next.Options[i], icon, UnityEngine.Color.white));
                }
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

            if ((full || next.AlphaFadeSpeed != prev.AlphaFadeSpeed) && next.AlphaFadeSpeed.HasValue)
                dropdown.alphaFadeSpeed = next.AlphaFadeSpeed.Value;

            var arrow = go.transform.Find("Arrow");
            UguiSliderAdapter.ApplyPart(
                arrow,
                next.ArrowSprite,
                next.ArrowColor,
                full,
                prev?.ArrowSprite,
                prev?.ArrowColor
            );
            if (dropdown.template != null)
            {
                UguiSliderAdapter.ApplyPart(
                    dropdown.template,
                    next.TemplateSprite,
                    next.TemplateColor,
                    full,
                    prev?.TemplateSprite,
                    prev?.TemplateColor
                );
            }
            if (
                dropdown.itemText != null
                && (
                    full
                    || next.ItemFontSize != prev.ItemFontSize
                    || next.ItemColor != prev.ItemColor
                )
            )
            {
                if (next.ItemFontSize.HasValue)
                    dropdown.itemText.fontSize = next.ItemFontSize.Value;
                if (next.ItemColor.HasValue)
                    dropdown.itemText.color = next.ItemColor.Value;
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
