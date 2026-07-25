using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Shared Selectable surface: interactability, the transition block, and
    /// navigation — including explicit targets, which accept a
    /// <c>Ref&lt;Selectable&gt;</c>, a <c>Ref&lt;GameObject&gt;</c>, or a
    /// direct <c>Selectable</c>. Inherits the Image surface for the control's
    /// background graphic.
    /// </summary>
    public abstract class UguiSelectableProps : UguiImageProps
    {
        public bool? Interactable { get; set; }
        public Selectable.Transition? Transition { get; set; }
        public ColorBlock? Colors { get; set; }
        public SpriteState? SpriteState { get; set; }
        public UguiAnimationTriggers AnimationTriggers { get; set; }
        public Navigation.Mode? NavigationMode { get; set; }
        public object SelectOnUp { get; set; }
        public object SelectOnDown { get; set; }
        public object SelectOnLeft { get; set; }
        public object SelectOnRight { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiSelectableProps o))
                return false;
            if (Interactable != o.Interactable)
                return false;
            if (Transition != o.Transition)
                return false;
            if (!ColorBlockEquals(Colors, o.Colors))
                return false;
            if (!SpriteStateEquals(SpriteState, o.SpriteState))
                return false;
            if (!UguiAnimationTriggers.ValueEquals(AnimationTriggers, o.AnimationTriggers))
                return false;
            if (NavigationMode != o.NavigationMode)
                return false;
            if (!ReferenceEquals(SelectOnUp, o.SelectOnUp))
                return false;
            if (!ReferenceEquals(SelectOnDown, o.SelectOnDown))
                return false;
            if (!ReferenceEquals(SelectOnLeft, o.SelectOnLeft))
                return false;
            if (!ReferenceEquals(SelectOnRight, o.SelectOnRight))
                return false;
            return base.ShallowEquals(other);
        }

        private static bool ColorBlockEquals(ColorBlock? a, ColorBlock? b)
        {
            if (a.HasValue != b.HasValue)
                return false;
            if (!a.HasValue)
                return true;
            var x = a.Value;
            var y = b.Value;
            return x.normalColor == y.normalColor
                && x.highlightedColor == y.highlightedColor
                && x.pressedColor == y.pressedColor
                && x.selectedColor == y.selectedColor
                && x.disabledColor == y.disabledColor
                && x.colorMultiplier == y.colorMultiplier
                && x.fadeDuration == y.fadeDuration;
        }

        private static bool SpriteStateEquals(SpriteState? a, SpriteState? b)
        {
            if (a.HasValue != b.HasValue)
                return false;
            if (!a.HasValue)
                return true;
            var x = a.Value;
            var y = b.Value;
            return ReferenceEquals(x.highlightedSprite, y.highlightedSprite)
                && ReferenceEquals(x.pressedSprite, y.pressedSprite)
                && ReferenceEquals(x.selectedSprite, y.selectedSprite)
                && ReferenceEquals(x.disabledSprite, y.disabledSprite);
        }

        internal override void __ResetFields()
        {
            Interactable = null;
            Transition = null;
            Colors = null;
            SpriteState = null;
            AnimationTriggers = null;
            NavigationMode = null;
            SelectOnUp = null;
            SelectOnDown = null;
            SelectOnLeft = null;
            SelectOnRight = null;
            base.__ResetFields();
        }
    }
}
