// GUIDField is a runtime control from Unity 6.4. Its value type is UnityEngine.GUID, so this
// compiles in players - it is not an editor-only control despite the name.
#if UNITY_6000_4_OR_NEWER
using System.Collections.Generic;
using Ruitk.Props;
using Ruitk.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ruitk.Elements
{
    public sealed class GUIDFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new GUIDField();

        // Markup carries a GUID as its dashed/undashed hex string, matching UXML's delayed-string
        // round-trip, so accept both a real GUID and a parseable string.
        private static bool TryReadGuid(object raw, out GUID value)
        {
            if (raw is GUID g)
            {
                value = g;
                return true;
            }
            if (raw is string s && GUID.TryParse(s, out var parsed))
            {
                value = parsed;
                return true;
            }
            value = default;
            return false;
        }

        private static void ApplySlots(
            GUIDField field,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (
                properties.TryGetValue("label", out var labelObj)
                && labelObj is Dictionary<string, object> labelMap
                && field.labelElement != null
            )
            {
                PropsApplier.Apply(field.labelElement, labelMap);
            }
            if (
                properties.TryGetValue("input", out var inputObj)
                && inputObj is Dictionary<string, object> inputMap
            )
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, inputMap);
            }
        }

        private static void ApplySlotsDiff(
            GUIDField field,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous.TryGetValue("label", out var prevLabel);
            next.TryGetValue("label", out var nextLabel);
            if (
                !ReferenceEquals(prevLabel, nextLabel)
                && nextLabel is Dictionary<string, object> labelMap
                && field.labelElement != null
            )
            {
                PropsApplier.Apply(field.labelElement, labelMap);
            }
            previous.TryGetValue("input", out var prevInput);
            next.TryGetValue("input", out var nextInput);
            if (
                !ReferenceEquals(prevInput, nextInput)
                && nextInput is Dictionary<string, object> inputMap
            )
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, inputMap);
            }
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not GUIDField field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            if (properties.TryGetValue("value", out var rawValue) && TryReadGuid(rawValue, out var g))
            {
                field.value = g;
            }
            TryApplyProp<bool>(properties, "readOnly", v => field.isReadOnly = v);
            TryApplyProp<bool>(properties, "isDelayed", v => field.isDelayed = v);
            TryApplyProp<int>(properties, "maxLength", v => field.maxLength = v);
            TryApplyProp<bool>(properties, "selectAllOnFocus", v => field.selectAllOnFocus = v);
            TryApplyProp<string>(properties, "labelText", v => field.label = v);
            ApplySlots(field, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is GUIDField field)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;

                previous.TryGetValue("value", out var prevValue);
                next.TryGetValue("value", out var nextValue);
                if (!Equals(prevValue, nextValue) && TryReadGuid(nextValue, out var g))
                {
                    field.value = g;
                }

                TryDiffProp<bool>(previous, next, "readOnly", v => field.isReadOnly = v);
                TryDiffProp<bool>(previous, next, "isDelayed", v => field.isDelayed = v);
                TryDiffProp<int>(previous, next, "maxLength", v => field.maxLength = v);
                TryDiffProp<bool>(
                    previous,
                    next,
                    "selectAllOnFocus",
                    v => field.selectAllOnFocus = v
                );
                TryDiffProp<string>(previous, next, "labelText", v => field.label = v);
                ApplySlotsDiff(field, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is GUIDField field && props is GUIDFieldProps tp)
            {
                if (tp.Value.HasValue)
                    field.value = tp.Value.Value;
                if (tp.ReadOnly.HasValue)
                    field.isReadOnly = tp.ReadOnly.Value;
                if (tp.IsDelayed.HasValue)
                    field.isDelayed = tp.IsDelayed.Value;
                if (tp.MaxLength.HasValue)
                    field.maxLength = tp.MaxLength.Value;
                if (tp.SelectAllOnFocus.HasValue)
                    field.selectAllOnFocus = tp.SelectAllOnFocus.Value;
                if (tp.LabelText != null)
                    field.label = tp.LabelText;
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
                if (tp.Label != null && field.labelElement != null)
                    PropsApplier.Apply(field.labelElement, tp.Label);
                if (tp.Input != null)
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                        PropsApplier.Apply(input, tp.Input);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is GUIDField field
                && prev is GUIDFieldProps tp
                && next is GUIDFieldProps tn
            )
            {
                if (tp.Value != tn.Value && tn.Value.HasValue)
                    field.value = tn.Value.Value;
                if (tp.ReadOnly != tn.ReadOnly && tn.ReadOnly.HasValue)
                    field.isReadOnly = tn.ReadOnly.Value;
                if (tp.IsDelayed != tn.IsDelayed && tn.IsDelayed.HasValue)
                    field.isDelayed = tn.IsDelayed.Value;
                if (tp.MaxLength != tn.MaxLength && tn.MaxLength.HasValue)
                    field.maxLength = tn.MaxLength.Value;
                if (tp.SelectAllOnFocus != tn.SelectAllOnFocus && tn.SelectAllOnFocus.HasValue)
                    field.selectAllOnFocus = tn.SelectAllOnFocus.Value;
                if (tp.LabelText != tn.LabelText && tn.LabelText != null)
                    field.label = tn.LabelText;

                if (tp.OnChange != tn.OnChange)
                {
                    if (tn.OnChange != null)
                        PropsApplier.ApplySingle(element, tp.OnChange, "onChange", tn.OnChange);
                    else if (tp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", tp.OnChange);
                }
                if (tp.OnChangeCapture != tn.OnChangeCapture)
                {
                    if (tn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            tp.OnChangeCapture,
                            "onChangeCapture",
                            tn.OnChangeCapture
                        );
                    else if (tp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", tp.OnChangeCapture);
                }
                if (!ReferenceEquals(tp.Label, tn.Label) && field.labelElement != null)
                {
                    if (tp.Label != null && tn.Label != null)
                        PropsApplier.ApplyDiff(field.labelElement, tp.Label, tn.Label);
                    else if (tn.Label != null)
                        PropsApplier.Apply(field.labelElement, tn.Label);
                }
                if (!ReferenceEquals(tp.Input, tn.Input))
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                    {
                        if (tp.Input != null && tn.Input != null)
                            PropsApplier.ApplyDiff(input, tp.Input, tn.Input);
                        else if (tn.Input != null)
                            PropsApplier.Apply(input, tn.Input);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
#endif
