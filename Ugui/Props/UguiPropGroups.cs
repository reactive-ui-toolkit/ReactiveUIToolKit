using UnityEngine.UI;

namespace Ruitk.Ugui
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

    public sealed class UguiCanvasGroup
    {
        public float? Alpha { get; set; }
        public bool? Interactable { get; set; }
        public bool? BlocksRaycasts { get; set; }
        public bool? IgnoreParentGroups { get; set; }

        internal static bool ValueEquals(UguiCanvasGroup a, UguiCanvasGroup b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.Alpha == b.Alpha
                && a.Interactable == b.Interactable
                && a.BlocksRaycasts == b.BlocksRaycasts
                && a.IgnoreParentGroups == b.IgnoreParentGroups;
        }
    }

    public sealed class UguiMask
    {
        public bool? ShowMaskGraphic { get; set; }

        internal static bool ValueEquals(UguiMask a, UguiMask b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.ShowMaskGraphic == b.ShowMaskGraphic;
        }
    }

    public sealed class UguiRectMask2D
    {
        public UnityEngine.Vector4? Padding { get; set; }
        public UnityEngine.Vector2Int? Softness { get; set; }

        internal static bool ValueEquals(UguiRectMask2D a, UguiRectMask2D b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.Padding == b.Padding && a.Softness == b.Softness;
        }
    }

    /// <summary>Selectable Animation-transition trigger names (Inspector parity).</summary>
    public sealed class UguiAnimationTriggers
    {
        public string Normal { get; set; }
        public string Highlighted { get; set; }
        public string Pressed { get; set; }
        public string Selected { get; set; }
        public string Disabled { get; set; }

        internal static bool ValueEquals(UguiAnimationTriggers a, UguiAnimationTriggers b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.Normal == b.Normal
                && a.Highlighted == b.Highlighted
                && a.Pressed == b.Pressed
                && a.Selected == b.Selected
                && a.Disabled == b.Disabled;
        }
    }

    /// <summary>Shared shape for the Shadow and Outline vertex effects.</summary>
    public sealed class UguiShadowEffect
    {
        public UnityEngine.Color? EffectColor { get; set; }
        public UnityEngine.Vector2? EffectDistance { get; set; }
        public bool? UseGraphicAlpha { get; set; }

        internal static bool ValueEquals(UguiShadowEffect a, UguiShadowEffect b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.EffectColor == b.EffectColor
                && a.EffectDistance == b.EffectDistance
                && a.UseGraphicAlpha == b.UseGraphicAlpha;
        }
    }
}
