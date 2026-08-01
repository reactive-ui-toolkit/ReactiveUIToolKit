// GUIDField is a runtime control from Unity 6.4 (UnityEngine.UIElements.GUIDField).
// Its value type is UnityEngine.GUID, not UnityEditor.GUID, so this compiles in players.
#if UNITY_6000_4_OR_NEWER
using System.Collections.Generic;
using Ruitk.Core;
using UnityEngine;

namespace Ruitk.Props.Typed
{
    public sealed class GUIDFieldProps : BaseProps
    {
        public GUID? Value { get; set; }
        public bool? ReadOnly { get; set; }
        public bool? IsDelayed { get; set; }
        public int? MaxLength { get; set; }
        public bool? SelectAllOnFocus { get; set; }
        public string LabelText { get; set; }

        public ChangeEventHandler<GUID> OnChange { get; set; }
        public ChangeEventHandler<GUID> OnChangeCapture { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not GUIDFieldProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (ReadOnly != o.ReadOnly)
                return false;
            if (IsDelayed != o.IsDelayed)
                return false;
            if (MaxLength != o.MaxLength)
                return false;
            if (SelectAllOnFocus != o.SelectAllOnFocus)
                return false;
            if (LabelText != o.LabelText)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(Input, o.Input))
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (ReadOnly.HasValue)
            {
                dict["readOnly"] = ReadOnly.Value;
            }
            if (IsDelayed.HasValue)
            {
                dict["isDelayed"] = IsDelayed.Value;
            }
            if (MaxLength.HasValue)
            {
                dict["maxLength"] = MaxLength.Value;
            }
            if (SelectAllOnFocus.HasValue)
            {
                dict["selectAllOnFocus"] = SelectAllOnFocus.Value;
            }
            if (LabelText != null)
            {
                dict["labelText"] = LabelText;
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
            if (Input != null)
            {
                dict["input"] = Input;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Value = null;
            ReadOnly = null;
            IsDelayed = null;
            MaxLength = null;
            SelectAllOnFocus = null;
            LabelText = null;
            OnChange = null;
            OnChangeCapture = null;
            Label = null;
            Input = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<GUIDFieldProps>.Return(this);
        }
    }
}
#endif
