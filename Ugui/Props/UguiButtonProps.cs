using System;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Props for the Button element — a Selectable Button whose background is
    /// its own Image (the GameObject &gt; UI &gt; Button shape). Inherits the
    /// full Image surface for the background graphic and the Selectable
    /// surface (transition block, navigation).
    /// </summary>
    public sealed class UguiButtonProps : UguiSelectableProps
    {
        public Action OnClick { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiButtonProps o))
                return false;
            if (OnClick != o.OnClick)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            OnClick = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiButtonProps>.Return(this);
        }
    }
}
