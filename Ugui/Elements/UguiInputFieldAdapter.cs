using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// TMP_InputField with the exact GameObject &gt; UI &gt; Input Field
    /// template (via TMP_DefaultControls): Text Area/Placeholder/Text.
    /// Controlled-component semantics: the Text prop writes via
    /// SetTextWithoutNotify so state-driven updates never echo through
    /// onValueChanged.
    /// </summary>
    public sealed class UguiInputFieldAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = TMP_DefaultControls.CreateInputField(UguiDefaultResources.GetTmpResources());
            var tag = UguiNodeTag.GetOrAdd(go);
            tag.Selectable = go.GetComponent<TMP_InputField>();
            tag.Control = tag.Selectable;
            tag.Image = go.GetComponent<Image>();
            tag.Graphic = tag.Image;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiInputFieldProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiInputFieldProps, next as UguiInputFieldProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiInputFieldProps prev, UguiInputFieldProps next)
        {
            if (next == null)
                return;
            var tag = UguiNodeTag.Find(go);
            var input = tag != null ? tag.Selectable as TMP_InputField : go.GetComponent<TMP_InputField>();
            if (input == null)
                return;
            bool full = prev == null;

            UguiImageApplier.ApplyDiffOrFull(
                tag != null ? tag.Image : go.GetComponent<Image>(),
                prev,
                next
            );
            UguiSelectableApplier.Apply(input, prev, next);

            if ((full || next.CharacterLimit != prev.CharacterLimit) && next.CharacterLimit.HasValue)
                input.characterLimit = next.CharacterLimit.Value;
            if ((full || next.ContentType != prev.ContentType) && next.ContentType.HasValue)
                input.contentType = next.ContentType.Value;
            if ((full || next.LineType != prev.LineType) && next.LineType.HasValue)
                input.lineType = next.LineType.Value;
            if ((full || next.InputType != prev.InputType) && next.InputType.HasValue)
                input.inputType = next.InputType.Value;
            if ((full || next.KeyboardType != prev.KeyboardType) && next.KeyboardType.HasValue)
                input.keyboardType = next.KeyboardType.Value;
            if (
                (full || next.CharacterValidation != prev.CharacterValidation)
                && next.CharacterValidation.HasValue
            )
                input.characterValidation = next.CharacterValidation.Value;
            if ((full || next.ReadOnly != prev.ReadOnly) && next.ReadOnly.HasValue)
                input.readOnly = next.ReadOnly.Value;
            if ((full || next.CaretBlinkRate != prev.CaretBlinkRate) && next.CaretBlinkRate.HasValue)
                input.caretBlinkRate = next.CaretBlinkRate.Value;
            if ((full || next.CaretWidth != prev.CaretWidth) && next.CaretWidth.HasValue)
                input.caretWidth = next.CaretWidth.Value;
            if (
                (full || next.CustomCaretColor != prev.CustomCaretColor)
                && next.CustomCaretColor.HasValue
            )
                input.customCaretColor = next.CustomCaretColor.Value;
            if ((full || next.CaretColor != prev.CaretColor) && next.CaretColor.HasValue)
                input.caretColor = next.CaretColor.Value;
            if ((full || next.SelectionColor != prev.SelectionColor) && next.SelectionColor.HasValue)
                input.selectionColor = next.SelectionColor.Value;

            var text = input.textComponent;
            if (text != null)
            {
                if ((full || next.FontSize != prev.FontSize) && next.FontSize.HasValue)
                {
                    input.pointSize = next.FontSize.Value;
                }
                if ((full || next.TextColor != prev.TextColor) && next.TextColor.HasValue)
                    text.color = next.TextColor.Value;
            }

            if (input.placeholder is TMP_Text placeholder)
            {
                if ((full || next.Placeholder != prev.Placeholder) && next.Placeholder != null)
                    placeholder.text = next.Placeholder;
                if (
                    (full || next.PlaceholderColor != prev.PlaceholderColor)
                    && next.PlaceholderColor.HasValue
                )
                    placeholder.color = next.PlaceholderColor.Value;
            }

            if (
                full
                || next.OnValueChanged != prev.OnValueChanged
                || next.OnEndEdit != prev.OnEndEdit
                || next.OnSubmit != prev.OnSubmit
                || next.OnSelect != prev.OnSelect
                || next.OnDeselect != prev.OnDeselect
            )
            {
                var binding = UguiInputFieldBinding.GetOrAdd(go);
                binding.ValueChanged = next.OnValueChanged;
                binding.EndEdit = next.OnEndEdit;
                binding.Submit = next.OnSubmit;
                binding.Selected = next.OnSelect;
                binding.Deselected = next.OnDeselect;
            }

            if ((full || next.Text != prev.Text) && next.Text != null && input.text != next.Text)
                input.SetTextWithoutNotify(next.Text);
        }
    }
}
