using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// TMP-backed text element. Registered under both "Text" (the ugui tag)
    /// and "Label" (what V.Text/U.Text text nodes lower to), so plain text
    /// children work identically to the UI Toolkit backend.
    /// </summary>
    public sealed class UguiTextAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("Text");
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go.GetComponent<TextMeshProUGUI>(), null, props as UguiTextProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go.GetComponent<TextMeshProUGUI>(), prev as UguiTextProps, next as UguiTextProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        public override void ApplyProperties(
            GameObject go,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (
                properties != null
                && properties.TryGetValue("text", out var t)
                && t is string s
            )
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp != null && tmp.text != s)
                    tmp.text = s;
            }
        }

        private static void Apply(TextMeshProUGUI tmp, UguiTextProps prev, UguiTextProps next)
        {
            if (tmp == null || next == null)
                return;

            bool full = prev == null;
            if ((full || next.Text != prev.Text) && next.Text != null)
                tmp.text = next.Text;
            if ((full || !ReferenceEquals(next.Font, prev.Font)) && next.Font != null)
                tmp.font = next.Font;
            if ((full || next.FontSize != prev.FontSize) && next.FontSize.HasValue)
                tmp.fontSize = next.FontSize.Value;
            if ((full || next.AutoSize != prev.AutoSize) && next.AutoSize.HasValue)
                tmp.enableAutoSizing = next.AutoSize.Value;
            if ((full || next.FontSizeMin != prev.FontSizeMin) && next.FontSizeMin.HasValue)
                tmp.fontSizeMin = next.FontSizeMin.Value;
            if ((full || next.FontSizeMax != prev.FontSizeMax) && next.FontSizeMax.HasValue)
                tmp.fontSizeMax = next.FontSizeMax.Value;
            if ((full || next.FontStyle != prev.FontStyle) && next.FontStyle.HasValue)
                tmp.fontStyle = next.FontStyle.Value;
            if ((full || next.Alignment != prev.Alignment) && next.Alignment.HasValue)
                tmp.alignment = next.Alignment.Value;
            if ((full || next.Wrapping != prev.Wrapping) && next.Wrapping.HasValue)
                tmp.textWrappingMode = next.Wrapping.Value;
            if ((full || next.Overflow != prev.Overflow) && next.Overflow.HasValue)
                tmp.overflowMode = next.Overflow.Value;
            if ((full || next.RichText != prev.RichText) && next.RichText.HasValue)
                tmp.richText = next.RichText.Value;
            if (
                (full || next.CharacterSpacing != prev.CharacterSpacing)
                && next.CharacterSpacing.HasValue
            )
                tmp.characterSpacing = next.CharacterSpacing.Value;
            if ((full || next.WordSpacing != prev.WordSpacing) && next.WordSpacing.HasValue)
                tmp.wordSpacing = next.WordSpacing.Value;
            if ((full || next.LineSpacing != prev.LineSpacing) && next.LineSpacing.HasValue)
                tmp.lineSpacing = next.LineSpacing.Value;
            if (
                (full || next.ParagraphSpacing != prev.ParagraphSpacing)
                && next.ParagraphSpacing.HasValue
            )
                tmp.paragraphSpacing = next.ParagraphSpacing.Value;
            if ((full || next.Margin != prev.Margin) && next.Margin.HasValue)
                tmp.margin = next.Margin.Value;

            if (full)
                UguiGraphicApplier.ApplyFull(tmp, next);
            else
                UguiGraphicApplier.ApplyDiff(tmp, prev, next);
        }
    }
}
