// MaskField became a runtime control in Unity 6.5 (moved out of UnityEditor.UIElements).
#if UNITY_6000_5_OR_NEWER
using System;
using System.Collections.Generic;
using Ruitk.Props;
using Ruitk.Props.Typed;
using UnityEngine.UIElements;

namespace Ruitk.Elements
{
    public sealed class MaskFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new MaskField();

        // choicesMasks is indexed against choices, so choices must land first or Unity rebuilds
        // the list and drops the custom masks.
        private static void ApplyChoices(
            MaskField field,
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
                if (cm is IList<int> masks)
                    field.choicesMasks = new List<int>(masks);
                else if (cm is IEnumerable<int> maskEn)
                    field.choicesMasks = new List<int>(maskEn);
            }
        }

        private static void ApplySlots(
            MaskField field,
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
            MaskField field,
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
            if (element is not MaskField field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            ApplyChoices(field, properties);
            TryApplyProp<int>(properties, "value", v => field.value = v);
            TryApplyProp<string>(properties, "labelText", v => field.label = v);
            TryApplyProp<Func<string, string>>(
                properties,
                "formatSelectedValue",
                v => field.formatSelectedValueCallback = v
            );
            TryApplyProp<Func<string, string>>(
                properties,
                "formatListItem",
                v => field.formatListItemCallback = v
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
            if (element is MaskField field)
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

                TryDiffProp<int>(previous, next, "value", v => field.value = v);
                TryDiffProp<string>(previous, next, "labelText", v => field.label = v);
                TryDiffProp<Func<string, string>>(
                    previous,
                    next,
                    "formatSelectedValue",
                    v => field.formatSelectedValueCallback = v
                );
                TryDiffProp<Func<string, string>>(
                    previous,
                    next,
                    "formatListItem",
                    v => field.formatListItemCallback = v
                );
                ApplySlotsDiff(field, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is MaskField field && props is MaskFieldProps tp)
            {
                if (tp.Choices != null)
                    field.choices = new List<string>(tp.Choices);
                if (tp.ChoicesMasks != null)
                    field.choicesMasks = new List<int>(tp.ChoicesMasks);
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
                element is MaskField field
                && prev is MaskFieldProps tp
                && next is MaskFieldProps tn
            )
            {
                bool choicesChanged = !ReferenceEquals(tp.Choices, tn.Choices);
                bool masksChanged = !ReferenceEquals(tp.ChoicesMasks, tn.ChoicesMasks);
                if (choicesChanged && tn.Choices != null)
                    field.choices = new List<string>(tn.Choices);
                if ((choicesChanged || masksChanged) && tn.ChoicesMasks != null)
                    field.choicesMasks = new List<int>(tn.ChoicesMasks);

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
