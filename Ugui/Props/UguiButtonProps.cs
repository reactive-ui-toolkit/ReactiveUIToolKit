using System;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Props for the Button element — a Selectable Button whose background is
    /// its own Image (the GameObject &gt; UI &gt; Button shape). Inherits the
    /// full Image prop surface for the background graphic.
    /// </summary>
    public sealed class UguiButtonProps : UguiImageProps
    {
        public Action OnClick { get; set; }
        public bool? Interactable { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiButtonProps o))
                return false;
            if (OnClick != o.OnClick)
                return false;
            if (Interactable != o.Interactable)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            OnClick = null;
            Interactable = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiButtonProps>.Return(this);
        }
    }
}
