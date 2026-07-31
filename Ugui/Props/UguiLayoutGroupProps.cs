using UnityEngine;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Shared props for the layout-group container elements. Names mirror the
    /// HorizontalOrVerticalLayoutGroup Inspector.
    /// </summary>
    public abstract class UguiLayoutGroupProps : UguiBaseProps
    {
        public int? PaddingLeft { get; set; }
        public int? PaddingRight { get; set; }
        public int? PaddingTop { get; set; }
        public int? PaddingBottom { get; set; }
        public TextAnchor? ChildAlignment { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiLayoutGroupProps o))
                return false;
            if (PaddingLeft != o.PaddingLeft)
                return false;
            if (PaddingRight != o.PaddingRight)
                return false;
            if (PaddingTop != o.PaddingTop)
                return false;
            if (PaddingBottom != o.PaddingBottom)
                return false;
            if (ChildAlignment != o.ChildAlignment)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            PaddingLeft = null;
            PaddingRight = null;
            PaddingTop = null;
            PaddingBottom = null;
            ChildAlignment = null;
            base.__ResetFields();
        }
    }

    public abstract class UguiHvLayoutGroupProps : UguiLayoutGroupProps
    {
        public float? Spacing { get; set; }
        public bool? ReverseArrangement { get; set; }
        public bool? ChildControlWidth { get; set; }
        public bool? ChildControlHeight { get; set; }
        public bool? ChildScaleWidth { get; set; }
        public bool? ChildScaleHeight { get; set; }
        public bool? ChildForceExpandWidth { get; set; }
        public bool? ChildForceExpandHeight { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiHvLayoutGroupProps o))
                return false;
            if (Spacing != o.Spacing)
                return false;
            if (ReverseArrangement != o.ReverseArrangement)
                return false;
            if (ChildControlWidth != o.ChildControlWidth)
                return false;
            if (ChildControlHeight != o.ChildControlHeight)
                return false;
            if (ChildScaleWidth != o.ChildScaleWidth)
                return false;
            if (ChildScaleHeight != o.ChildScaleHeight)
                return false;
            if (ChildForceExpandWidth != o.ChildForceExpandWidth)
                return false;
            if (ChildForceExpandHeight != o.ChildForceExpandHeight)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Spacing = null;
            ReverseArrangement = null;
            ChildControlWidth = null;
            ChildControlHeight = null;
            ChildScaleWidth = null;
            ChildScaleHeight = null;
            ChildForceExpandWidth = null;
            ChildForceExpandHeight = null;
            base.__ResetFields();
        }
    }

    public sealed class UguiHorizontalLayoutGroupProps : UguiHvLayoutGroupProps
    {
        internal override void __ReturnToPool()
        {
            Pool<UguiHorizontalLayoutGroupProps>.Return(this);
        }
    }

    public sealed class UguiVerticalLayoutGroupProps : UguiHvLayoutGroupProps
    {
        internal override void __ReturnToPool()
        {
            Pool<UguiVerticalLayoutGroupProps>.Return(this);
        }
    }

    public sealed class UguiGridLayoutGroupProps : UguiLayoutGroupProps
    {
        public Vector2? CellSize { get; set; }
        public Vector2? Spacing { get; set; }
        public UnityEngine.UI.GridLayoutGroup.Corner? StartCorner { get; set; }
        public UnityEngine.UI.GridLayoutGroup.Axis? StartAxis { get; set; }
        public UnityEngine.UI.GridLayoutGroup.Constraint? Constraint { get; set; }
        public int? ConstraintCount { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiGridLayoutGroupProps o))
                return false;
            if (CellSize != o.CellSize)
                return false;
            if (Spacing != o.Spacing)
                return false;
            if (StartCorner != o.StartCorner)
                return false;
            if (StartAxis != o.StartAxis)
                return false;
            if (Constraint != o.Constraint)
                return false;
            if (ConstraintCount != o.ConstraintCount)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            CellSize = null;
            Spacing = null;
            StartCorner = null;
            StartAxis = null;
            Constraint = null;
            ConstraintCount = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiGridLayoutGroupProps>.Return(this);
        }
    }
}
