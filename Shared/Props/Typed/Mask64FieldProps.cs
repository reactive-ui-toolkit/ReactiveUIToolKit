// Mask64Field became a runtime control in Unity 6.5; the 64-bit sibling of MaskField.
#if UNITY_6000_5_OR_NEWER
using System;
using System.Collections.Generic;
using Ruitk.Core;

namespace Ruitk.Props.Typed
{
    public sealed class Mask64FieldProps : BaseProps
    {
        public List<string> Choices { get; set; }

        // Overrides the default 1UL << i mask per displayed choice, enabling composite flags.
        public List<ulong> ChoicesMasks { get; set; }

        // "Nothing" is 0 and "Everything" is ~0UL, NOT (1UL << n) - 1. See MaskFieldProps for why
        // the two must never be normalised into each other.
        public ulong? Value { get; set; }

        public string LabelText { get; set; }
        public Func<string, string> FormatSelectedValue { get; set; }
        public Func<string, string> FormatListItem { get; set; }

        public ChangeEventHandler<ulong> OnChange { get; set; }
        public ChangeEventHandler<ulong> OnChangeCapture { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not Mask64FieldProps o)
                return false;
            if (!ReferenceEquals(Choices, o.Choices))
                return false;
            if (!ReferenceEquals(ChoicesMasks, o.ChoicesMasks))
                return false;
            if (Value != o.Value)
                return false;
            if (LabelText != o.LabelText)
                return false;
            if (FormatSelectedValue != o.FormatSelectedValue)
                return false;
            if (FormatListItem != o.FormatListItem)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(VisualInput, o.VisualInput))
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Choices != null)
            {
                dict["choices"] = Choices;
            }
            if (ChoicesMasks != null)
            {
                dict["choicesMasks"] = ChoicesMasks;
            }
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (LabelText != null)
            {
                dict["labelText"] = LabelText;
            }
            if (FormatSelectedValue != null)
            {
                dict["formatSelectedValue"] = FormatSelectedValue;
            }
            if (FormatListItem != null)
            {
                dict["formatListItem"] = FormatListItem;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                dict["onChangeCapture"] = OnChangeCapture;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (VisualInput != null)
            {
                dict["visualInput"] = VisualInput;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Choices = null;
            ChoicesMasks = null;
            Value = null;
            LabelText = null;
            FormatSelectedValue = null;
            FormatListItem = null;
            OnChange = null;
            OnChangeCapture = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<Mask64FieldProps>.Return(this);
        }
    }
}
#endif
