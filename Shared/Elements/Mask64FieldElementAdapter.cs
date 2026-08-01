// Mask64Field became a runtime control in Unity 6.5 (moved out of UnityEditor.UIElements).
#if UNITY_6000_5_OR_NEWER
using System;
using System.Collections.Generic;
using Ruitk.Props;
using Ruitk.Props.Typed;
using UnityEngine.UIElements;

namespace Ruitk.Elements
{
    public sealed class Mask64FieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new Mask64Field();

        // A 64-bit mask written in markup arrives as whatever numeric type the literal parsed to
        // (int for small values, long for larger ones), so widen rather than requiring ulong.
        private static bool TryReadUlong(object raw, out ulong value)
        {
            switch (raw)
            {
                case ulong u:
                    value = u;
                    return true;
                case long l:
                    value = unchecked((ulong)l);
                    return true;
                case int i:
                    value = unchecked((ulong)i);
                    return true;
                case uint ui:
                    value = ui;
                    return true;
                default:
                    value = 0UL;
                    return false;
            }
        }

        private static List<ulong> ReadMaskList(object raw)
        {
            if (raw is IList<ulong> exact)
                return new List<ulong>(exact);
            if (raw is not System.Collections.IEnumerable seq)
                return null;
            var result = new List<ulong>();
            foreach (var item in seq)
            {
                if (!TryReadUlong(item, out var v))
                    return null;
                result.Add(v);
            }
            return result;
        }

        // choicesMasks is indexed against choices, so choices must land first.
        private static void ApplyChoices(
            Mask64Field field,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties.TryGetValue("choices", out var ch))
            {
                if (ch is IList<string> list)
                    field.choices = new List<string>(list);
                else if (ch is IEnumerable<string> en)
                    field.choices = new List<string>(en);
            }
            if (properties.TryGetValue("choicesMasks", out var cm))
            {
                var masks = ReadMaskList(cm);
                if (masks != null)
                    field.choicesMasks = masks;
            }
        }

        private static void ApplySlots(
            Mask64Field field,
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
                properties.TryGetValue("visualInput", out var viObj)
                && viObj is Dictionary<string, object> viMap
            )
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, viMap);
            }
        }

        private static void ApplySlotsDiff(
            Mask64Field field,
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
            previous.TryGetValue("visualInput", out var prevVi);
            next.TryGetValue("visualInput", out var nextVi);
            if (!ReferenceEquals(prevVi, nextVi) && nextVi is Dictionary<string, object> viMap)
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, viMap);
            }
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not Mask64Field field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            ApplyChoices(field, properties);
            if (
                properties.TryGetValue("value", out var rawValue)
                && TryReadUlong(rawValue, out var v)
            )
            {
                field.value = v;
            }
            TryApplyProp<string>(properties, "labelText", t => field.label = t);
            TryApplyProp<Func<string, string>>(
                properties,
                "formatSelectedValue",
                f => field.formatSelectedValueCallback = f
            );
            TryApplyProp<Func<string, string>>(
                properties,
                "formatListItem",
                f => field.formatListItemCallback = f
            );
            ApplySlots(field, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Mask64Field field)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;

                previous.TryGetValue("choices", out var prevChoices);
                next.TryGetValue("choices", out var nextChoices);
                previous.TryGetValue("choicesMasks", out var prevMasks);
                next.TryGetValue("choicesMasks", out var nextMasks);
                if (
                    !ReferenceEquals(prevChoices, nextChoices)
                    || !ReferenceEquals(prevMasks, nextMasks)
                )
                {
                    ApplyChoices(field, next);
                }

                previous.TryGetValue("value", out var prevValue);
                next.TryGetValue("value", out var nextValue);
                if (!Equals(prevValue, nextValue) && TryReadUlong(nextValue, out var v))
                {
                    field.value = v;
                }

                TryDiffProp<string>(previous, next, "labelText", t => field.label = t);
                TryDiffProp<Func<string, string>>(
                    previous,
                    next,
                    "formatSelectedValue",
                    f => field.formatSelectedValueCallback = f
                );
                TryDiffProp<Func<string, string>>(
                    previous,
                    next,
                    "formatListItem",
                    f => field.formatListItemCallback = f
                );
                ApplySlotsDiff(field, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Mask64Field field && props is Mask64FieldProps tp)
            {
                if (tp.Choices != null)
                    field.choices = new List<string>(tp.Choices);
                if (tp.ChoicesMasks != null)
                    field.choicesMasks = new List<ulong>(tp.ChoicesMasks);
                if (tp.Value.HasValue)
                    field.value = tp.Value.Value;
                if (tp.LabelText != null)
                    field.label = tp.LabelText;
                if (tp.FormatSelectedValue != null)
                    field.formatSelectedValueCallback = tp.FormatSelectedValue;
                if (tp.FormatListItem != null)
                    field.formatListItemCallback = tp.FormatListItem;
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
                if (tp.Label != null && field.labelElement != null)
                    PropsApplier.Apply(field.labelElement, tp.Label);
                if (tp.VisualInput != null)
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                        PropsApplier.Apply(input, tp.VisualInput);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is Mask64Field field
                && prev is Mask64FieldProps tp
                && next is Mask64FieldProps tn
            )
            {
                bool choicesChanged = !ReferenceEquals(tp.Choices, tn.Choices);
                bool masksChanged = !ReferenceEquals(tp.ChoicesMasks, tn.ChoicesMasks);
                if (choicesChanged && tn.Choices != null)
                    field.choices = new List<string>(tn.Choices);
                if ((choicesChanged || masksChanged) && tn.ChoicesMasks != null)
                    field.choicesMasks = new List<ulong>(tn.ChoicesMasks);

                if (tp.Value != tn.Value && tn.Value.HasValue)
                    field.value = tn.Value.Value;
                if (tp.LabelText != tn.LabelText && tn.LabelText != null)
                    field.label = tn.LabelText;
                if (tp.FormatSelectedValue != tn.FormatSelectedValue)
                    field.formatSelectedValueCallback = tn.FormatSelectedValue;
                if (tp.FormatListItem != tn.FormatListItem)
                    field.formatListItemCallback = tn.FormatListItem;

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
                if (!ReferenceEquals(tp.VisualInput, tn.VisualInput))
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                    {
                        if (tp.VisualInput != null && tn.VisualInput != null)
                            PropsApplier.ApplyDiff(input, tp.VisualInput, tn.VisualInput);
                        else if (tn.VisualInput != null)
                            PropsApplier.Apply(input, tn.VisualInput);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
#endif
