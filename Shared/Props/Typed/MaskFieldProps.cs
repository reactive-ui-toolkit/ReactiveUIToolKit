// MaskField became a runtime control in Unity 6.5; before that it lived in UnityEditor.UIElements.
#if UNITY_6000_5_OR_NEWER
using System;
using System.Collections.Generic;
using Ruitk.Core;

namespace Ruitk.Props.Typed
{
    public sealed class MaskFieldProps : BaseProps
    {
        public List<string> Choices { get; set; }

        // Overrides the default 1 << i mask per displayed choice, enabling composite flags.
        public List<int> ChoicesMasks { get; set; }

        // "Nothing" is 0 and "Everything" is ~0 (-1), NOT (1 << n) - 1. Never normalise between
        // the two: a user asking for "Everything" and a user ticking every defined bit are
        // different values to Unity, and collapsing them loses the distinction on the next diff.
        public int? Value { get; set; }

        public string LabelText { get; set; }
        public Func<string, string> FormatSelectedValue { get; set; }
        public Func<string, string> FormatListItem { get; set; }

        public ChangeEventHandler<int> OnChange { get; set; }
        public ChangeEventHandler<int> OnChangeCapture { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not MaskFieldProps o)
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
            Pool<MaskFieldProps>.Return(this);
        }
    }
}
#endif
