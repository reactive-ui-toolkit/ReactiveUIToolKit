using UnityEngine.UI;

namespace Ruitk.Ugui
{
    /// <summary>Shared Graphic prop application (color, material, raycast, mask).</summary>
    internal static class UguiGraphicApplier
    {
        internal static void ApplyFull(Graphic g, UguiGraphicProps props)
        {
            if (g == null || props == null)
                return;
            if (props.Color.HasValue)
                g.color = props.Color.Value;
            if (props.Material != null)
                g.material = props.Material;
            if (props.RaycastTarget.HasValue)
                g.raycastTarget = props.RaycastTarget.Value;
            if (props.RaycastPadding.HasValue)
                g.raycastPadding = props.RaycastPadding.Value;
            if (props.Maskable.HasValue && g is MaskableGraphic mg)
                mg.maskable = props.Maskable.Value;
        }

        internal static void ApplyDiff(Graphic g, UguiGraphicProps prev, UguiGraphicProps next)
        {
            if (g == null || next == null)
                return;
            if (prev == null)
            {
                ApplyFull(g, next);
                return;
            }
            if (next.Color != prev.Color && next.Color.HasValue)
                g.color = next.Color.Value;
            if (!ReferenceEquals(next.Material, prev.Material))
                g.material = next.Material;
            if (next.RaycastTarget != prev.RaycastTarget && next.RaycastTarget.HasValue)
                g.raycastTarget = next.RaycastTarget.Value;
            if (next.RaycastPadding != prev.RaycastPadding && next.RaycastPadding.HasValue)
                g.raycastPadding = next.RaycastPadding.Value;
            if (next.Maskable != prev.Maskable && next.Maskable.HasValue && g is MaskableGraphic mg)
                mg.maskable = next.Maskable.Value;
        }
    }
}
