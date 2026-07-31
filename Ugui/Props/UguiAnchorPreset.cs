using UnityEngine;

namespace Ruitk.Ugui
{
    /// <summary>
    /// The RectTransform anchor-preset widget as a value. Each preset expands
    /// to the exact (anchorMin, anchorMax, pivot) triple the Inspector widget
    /// applies. An explicit AnchorMin/AnchorMax/Pivot prop set alongside a
    /// preset overrides the preset's corresponding component.
    /// </summary>
    public enum UguiAnchorPreset
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        TopStretch,
        MiddleStretch,
        BottomStretch,
        StretchLeft,
        StretchCenter,
        StretchRight,
        Stretch,
    }

    public static class UguiAnchorPresets
    {
        public static void Resolve(
            UguiAnchorPreset preset,
            out Vector2 anchorMin,
            out Vector2 anchorMax,
            out Vector2 pivot
        )
        {
            switch (preset)
            {
                case UguiAnchorPreset.TopLeft:
                    anchorMin = anchorMax = new Vector2(0f, 1f);
                    pivot = new Vector2(0f, 1f);
                    break;
                case UguiAnchorPreset.TopCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 1f);
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case UguiAnchorPreset.TopRight:
                    anchorMin = anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(1f, 1f);
                    break;
                case UguiAnchorPreset.MiddleLeft:
                    anchorMin = anchorMax = new Vector2(0f, 0.5f);
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case UguiAnchorPreset.MiddleCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case UguiAnchorPreset.MiddleRight:
                    anchorMin = anchorMax = new Vector2(1f, 0.5f);
                    pivot = new Vector2(1f, 0.5f);
                    break;
                case UguiAnchorPreset.BottomLeft:
                    anchorMin = anchorMax = new Vector2(0f, 0f);
                    pivot = new Vector2(0f, 0f);
                    break;
                case UguiAnchorPreset.BottomCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 0f);
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case UguiAnchorPreset.BottomRight:
                    anchorMin = anchorMax = new Vector2(1f, 0f);
                    pivot = new Vector2(1f, 0f);
                    break;
                case UguiAnchorPreset.TopStretch:
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case UguiAnchorPreset.MiddleStretch:
                    anchorMin = new Vector2(0f, 0.5f);
                    anchorMax = new Vector2(1f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case UguiAnchorPreset.BottomStretch:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(1f, 0f);
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case UguiAnchorPreset.StretchLeft:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(0f, 1f);
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case UguiAnchorPreset.StretchCenter:
                    anchorMin = new Vector2(0.5f, 0f);
                    anchorMax = new Vector2(0.5f, 1f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case UguiAnchorPreset.StretchRight:
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(1f, 0.5f);
                    break;
                default:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }
    }
}
