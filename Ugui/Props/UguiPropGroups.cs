using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Prop groups — the declarative twin of "Add Component". A non-null
    /// group on an element's props adds/updates the component; a group that
    /// goes from non-null to null removes it. Field names mirror the
    /// Inspector.
    /// </summary>
    public sealed class UguiLayoutElement
    {
        public bool? IgnoreLayout { get; set; }
        public float? MinWidth { get; set; }
        public float? MinHeight { get; set; }
        public float? PreferredWidth { get; set; }
        public float? PreferredHeight { get; set; }
        public float? FlexibleWidth { get; set; }
        public float? FlexibleHeight { get; set; }
        public int? LayoutPriority { get; set; }

        internal static bool ValueEquals(UguiLayoutElement a, UguiLayoutElement b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.IgnoreLayout == b.IgnoreLayout
                && a.MinWidth == b.MinWidth
                && a.MinHeight == b.MinHeight
                && a.PreferredWidth == b.PreferredWidth
                && a.PreferredHeight == b.PreferredHeight
                && a.FlexibleWidth == b.FlexibleWidth
                && a.FlexibleHeight == b.FlexibleHeight
                && a.LayoutPriority == b.LayoutPriority;
        }
    }

    public sealed class UguiContentSizeFitter
    {
        public ContentSizeFitter.FitMode? HorizontalFit { get; set; }
        public ContentSizeFitter.FitMode? VerticalFit { get; set; }

        internal static bool ValueEquals(UguiContentSizeFitter a, UguiContentSizeFitter b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.HorizontalFit == b.HorizontalFit && a.VerticalFit == b.VerticalFit;
        }
    }

    public sealed class UguiAspectRatioFitter
    {
        public AspectRatioFitter.AspectMode? AspectMode { get; set; }
        public float? AspectRatio { get; set; }

        internal static bool ValueEquals(UguiAspectRatioFitter a, UguiAspectRatioFitter b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.AspectMode == b.AspectMode && a.AspectRatio == b.AspectRatio;
        }
    }
}
