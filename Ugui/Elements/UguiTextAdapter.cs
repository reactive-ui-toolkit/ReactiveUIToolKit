using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ruitk.Ugui
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
            var tag = UguiNodeTag.GetOrAdd(go);
            tag.Graphic = tmp;
            tag.Control = tmp;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(CachedText(go), null, props as UguiTextProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(CachedText(go), prev as UguiTextProps, next as UguiTextProps);
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
                var tmp = CachedText(go);
                if (tmp != null && tmp.text != s)
                    tmp.text = s;
            }
        }

        private static TextMeshProUGUI CachedText(GameObject go)
        {
            var tag = UguiNodeTag.Find(go);
            return tag != null && tag.Control is TextMeshProUGUI tmp
                ? tmp
                : go.GetComponent<TextMeshProUGUI>();
        }

        public override bool TryResetForPool(GameObject go)
        {
            var tmp = CachedText(go);
            if (tmp == null)
                return false;
            ResetCommonState(go);
            go.name = "Text";
            tmp.text = string.Empty;
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
            tmp.fontSize = 36f;
            tmp.enableAutoSizing = false;
            tmp.fontSizeMin = 18f;
            tmp.fontSizeMax = 72f;
            tmp.fontStyle = FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.richText = true;
            tmp.characterSpacing = 0f;
            tmp.wordSpacing = 0f;
            tmp.lineSpacing = 0f;
            tmp.paragraphSpacing = 0f;
            tmp.margin = UnityEngine.Vector4.zero;
            tmp.enableVertexGradient = false;
            tmp.colorGradient = new VertexGradient(UnityEngine.Color.white);
            tmp.colorGradientPreset = null;
            tmp.extraPadding = false;
            tmp.outlineWidth = 0f;
            tmp.color = UnityEngine.Color.white;
            tmp.material = null;
            tmp.raycastTarget = false;
            tmp.maskable = true;
            return true;
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
            if (next.Gradient.HasValue && (full || !next.Gradient.Equals(prev.Gradient)))
            {
                tmp.enableVertexGradient = true;
                tmp.colorGradient = next.Gradient.Value;
            }
            if (
                (full || !ReferenceEquals(next.GradientPreset, prev.GradientPreset))
                && next.GradientPreset != null
            )
                tmp.colorGradientPreset = next.GradientPreset;
            if ((full || next.ExtraPadding != prev.ExtraPadding) && next.ExtraPadding.HasValue)
                tmp.extraPadding = next.ExtraPadding.Value;
            if ((full || next.OutlineWidth != prev.OutlineWidth) && next.OutlineWidth.HasValue)
                tmp.outlineWidth = next.OutlineWidth.Value;
            if (next.OutlineColor.HasValue && (full || !next.OutlineColor.Equals(prev.OutlineColor)))
                tmp.outlineColor = next.OutlineColor.Value;
            if (
                (full || !ReferenceEquals(next.StyleSheet, prev.StyleSheet))
                && next.StyleSheet != null
            )
                tmp.styleSheet = next.StyleSheet;
            if (
                (full || !ReferenceEquals(next.SpriteAsset, prev.SpriteAsset))
                && next.SpriteAsset != null
            )
                tmp.spriteAsset = next.SpriteAsset;

            if (full)
                UguiGraphicApplier.ApplyFull(tmp, next);
            else
                UguiGraphicApplier.ApplyDiff(tmp, prev, next);
        }
    }
}
