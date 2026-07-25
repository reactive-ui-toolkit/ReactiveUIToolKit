using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    internal static class UguiLayoutGroupApplier
    {
        internal static void ApplyShared(LayoutGroup group, UguiLayoutGroupProps prev, UguiLayoutGroupProps next)
        {
            if (group == null || next == null)
                return;
            bool full = prev == null;
            if (
                full
                || next.PaddingLeft != prev.PaddingLeft
                || next.PaddingRight != prev.PaddingRight
                || next.PaddingTop != prev.PaddingTop
                || next.PaddingBottom != prev.PaddingBottom
            )
            {
                var padding = group.padding;
                if (next.PaddingLeft.HasValue)
                    padding.left = next.PaddingLeft.Value;
                if (next.PaddingRight.HasValue)
                    padding.right = next.PaddingRight.Value;
                if (next.PaddingTop.HasValue)
                    padding.top = next.PaddingTop.Value;
                if (next.PaddingBottom.HasValue)
                    padding.bottom = next.PaddingBottom.Value;
                group.padding = padding;
            }
            if ((full || next.ChildAlignment != prev.ChildAlignment) && next.ChildAlignment.HasValue)
                group.childAlignment = next.ChildAlignment.Value;
        }

        internal static void ApplyHv(
            HorizontalOrVerticalLayoutGroup group,
            UguiHvLayoutGroupProps prev,
            UguiHvLayoutGroupProps next
        )
        {
            if (group == null || next == null)
                return;
            bool full = prev == null;
            if ((full || next.Spacing != prev.Spacing) && next.Spacing.HasValue)
                group.spacing = next.Spacing.Value;
            if (
                (full || next.ReverseArrangement != prev.ReverseArrangement)
                && next.ReverseArrangement.HasValue
            )
                group.reverseArrangement = next.ReverseArrangement.Value;
            if (
                (full || next.ChildControlWidth != prev.ChildControlWidth)
                && next.ChildControlWidth.HasValue
            )
                group.childControlWidth = next.ChildControlWidth.Value;
            if (
                (full || next.ChildControlHeight != prev.ChildControlHeight)
                && next.ChildControlHeight.HasValue
            )
                group.childControlHeight = next.ChildControlHeight.Value;
            if (
                (full || next.ChildScaleWidth != prev.ChildScaleWidth)
                && next.ChildScaleWidth.HasValue
            )
                group.childScaleWidth = next.ChildScaleWidth.Value;
            if (
                (full || next.ChildScaleHeight != prev.ChildScaleHeight)
                && next.ChildScaleHeight.HasValue
            )
                group.childScaleHeight = next.ChildScaleHeight.Value;
            if (
                (full || next.ChildForceExpandWidth != prev.ChildForceExpandWidth)
                && next.ChildForceExpandWidth.HasValue
            )
                group.childForceExpandWidth = next.ChildForceExpandWidth.Value;
            if (
                (full || next.ChildForceExpandHeight != prev.ChildForceExpandHeight)
                && next.ChildForceExpandHeight.HasValue
            )
                group.childForceExpandHeight = next.ChildForceExpandHeight.Value;
            ApplyShared(group, prev, next);
        }
    }

    public sealed class UguiHorizontalLayoutGroupAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return new GameObject("HorizontalLayoutGroup", typeof(HorizontalLayoutGroup));
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            UguiLayoutGroupApplier.ApplyHv(
                go.GetComponent<HorizontalLayoutGroup>(),
                null,
                props as UguiHorizontalLayoutGroupProps
            );
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            UguiLayoutGroupApplier.ApplyHv(
                go.GetComponent<HorizontalLayoutGroup>(),
                prev as UguiHorizontalLayoutGroupProps,
                next as UguiHorizontalLayoutGroupProps
            );
            base.ApplyTypedDiff(go, prev, next);
        }
    }

    public sealed class UguiVerticalLayoutGroupAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return new GameObject("VerticalLayoutGroup", typeof(VerticalLayoutGroup));
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            UguiLayoutGroupApplier.ApplyHv(
                go.GetComponent<VerticalLayoutGroup>(),
                null,
                props as UguiVerticalLayoutGroupProps
            );
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            UguiLayoutGroupApplier.ApplyHv(
                go.GetComponent<VerticalLayoutGroup>(),
                prev as UguiVerticalLayoutGroupProps,
                next as UguiVerticalLayoutGroupProps
            );
            base.ApplyTypedDiff(go, prev, next);
        }
    }

    public sealed class UguiGridLayoutGroupAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            return new GameObject("GridLayoutGroup", typeof(GridLayoutGroup));
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go.GetComponent<GridLayoutGroup>(), null, props as UguiGridLayoutGroupProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(
                go.GetComponent<GridLayoutGroup>(),
                prev as UguiGridLayoutGroupProps,
                next as UguiGridLayoutGroupProps
            );
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(
            GridLayoutGroup grid,
            UguiGridLayoutGroupProps prev,
            UguiGridLayoutGroupProps next
        )
        {
            if (grid == null || next == null)
                return;
            bool full = prev == null;
            if ((full || next.CellSize != prev.CellSize) && next.CellSize.HasValue)
                grid.cellSize = next.CellSize.Value;
            if ((full || next.Spacing != prev.Spacing) && next.Spacing.HasValue)
                grid.spacing = next.Spacing.Value;
            if ((full || next.StartCorner != prev.StartCorner) && next.StartCorner.HasValue)
                grid.startCorner = next.StartCorner.Value;
            if ((full || next.StartAxis != prev.StartAxis) && next.StartAxis.HasValue)
                grid.startAxis = next.StartAxis.Value;
            if ((full || next.Constraint != prev.Constraint) && next.Constraint.HasValue)
                grid.constraint = next.Constraint.Value;
            if (
                (full || next.ConstraintCount != prev.ConstraintCount)
                && next.ConstraintCount.HasValue
            )
                grid.constraintCount = next.ConstraintCount.Value;
            UguiLayoutGroupApplier.ApplyShared(grid, prev, next);
        }
    }
}
