using TMPro;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiTextProps : UguiGraphicProps
    {
        public string Text { get; set; }
        public TMP_FontAsset Font { get; set; }
        public float? FontSize { get; set; }
        public bool? AutoSize { get; set; }
        public float? FontSizeMin { get; set; }
        public float? FontSizeMax { get; set; }
        public FontStyles? FontStyle { get; set; }
        public TextAlignmentOptions? Alignment { get; set; }
        public TextWrappingModes? Wrapping { get; set; }
        public TextOverflowModes? Overflow { get; set; }
        public bool? RichText { get; set; }
        public float? CharacterSpacing { get; set; }
        public float? WordSpacing { get; set; }
        public float? LineSpacing { get; set; }
        public float? ParagraphSpacing { get; set; }
        public Vector4? Margin { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiTextProps o))
                return false;
            if (Text != o.Text)
                return false;
            if (!ReferenceEquals(Font, o.Font))
                return false;
            if (FontSize != o.FontSize)
                return false;
            if (AutoSize != o.AutoSize)
                return false;
            if (FontSizeMin != o.FontSizeMin)
                return false;
            if (FontSizeMax != o.FontSizeMax)
                return false;
            if (FontStyle != o.FontStyle)
                return false;
            if (Alignment != o.Alignment)
                return false;
            if (Wrapping != o.Wrapping)
                return false;
            if (Overflow != o.Overflow)
                return false;
            if (RichText != o.RichText)
                return false;
            if (CharacterSpacing != o.CharacterSpacing)
                return false;
            if (WordSpacing != o.WordSpacing)
                return false;
            if (LineSpacing != o.LineSpacing)
                return false;
            if (ParagraphSpacing != o.ParagraphSpacing)
                return false;
            if (Margin != o.Margin)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Text = null;
            Font = null;
            FontSize = null;
            AutoSize = null;
            FontSizeMin = null;
            FontSizeMax = null;
            FontStyle = null;
            Alignment = null;
            Wrapping = null;
            Overflow = null;
            RichText = null;
            CharacterSpacing = null;
            WordSpacing = null;
            LineSpacing = null;
            ParagraphSpacing = null;
            Margin = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiTextProps>.Return(this);
        }
    }
}
