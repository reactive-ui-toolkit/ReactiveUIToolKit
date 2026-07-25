using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Applies the shared RectTransform/GameObject prop block. Application
    /// order: anchor preset first, then explicit anchor/pivot overrides, then
    /// offsets (OffsetMin/OffsetMax win over AnchoredPosition/SizeDelta when
    /// both encodings are present, matching how the Inspector's fields
    /// interact).
    /// </summary>
    internal static class UguiRectApplier
    {
        internal static void ApplyFull(GameObject go, UguiBaseProps props)
        {
            if (props == null)
                return;

            if (props.Name != null)
                go.name = props.Name;
            if (props.Layer.HasValue)
                go.layer = props.Layer.Value;

            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                if (props.Anchors.HasValue)
                {
                    UguiAnchorPresets.Resolve(
                        props.Anchors.Value,
                        out var min,
                        out var max,
                        out var pivot
                    );
                    rt.anchorMin = min;
                    rt.anchorMax = max;
                    rt.pivot = pivot;
                }
                if (props.AnchorMin.HasValue)
                    rt.anchorMin = props.AnchorMin.Value;
                if (props.AnchorMax.HasValue)
                    rt.anchorMax = props.AnchorMax.Value;
                if (props.Pivot.HasValue)
                    rt.pivot = props.Pivot.Value;

                if (props.AnchoredPosition.HasValue)
                    rt.anchoredPosition = props.AnchoredPosition.Value;
                if (props.SizeDelta.HasValue)
                    rt.sizeDelta = props.SizeDelta.Value;
                if (props.OffsetMin.HasValue)
                    rt.offsetMin = props.OffsetMin.Value;
                if (props.OffsetMax.HasValue)
                    rt.offsetMax = props.OffsetMax.Value;

                if (props.RotationZ.HasValue)
                    rt.localRotation = Quaternion.Euler(0f, 0f, props.RotationZ.Value);
                if (props.Scale.HasValue)
                    rt.localScale = props.Scale.Value;
            }

            if (props.Active.HasValue)
                go.SetActive(props.Active.Value);
        }

        internal static void ApplyDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            if (next == null)
                return;
            if (prev == null)
            {
                ApplyFull(go, next);
                return;
            }

            if (next.Name != prev.Name && next.Name != null)
                go.name = next.Name;
            if (next.Layer != prev.Layer && next.Layer.HasValue)
                go.layer = next.Layer.Value;

            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                bool anchorsChanged = next.Anchors != prev.Anchors;
                if (anchorsChanged && next.Anchors.HasValue)
                {
                    UguiAnchorPresets.Resolve(
                        next.Anchors.Value,
                        out var min,
                        out var max,
                        out var pivot
                    );
                    rt.anchorMin = min;
                    rt.anchorMax = max;
                    rt.pivot = pivot;
                }
                if ((next.AnchorMin != prev.AnchorMin || anchorsChanged) && next.AnchorMin.HasValue)
                    rt.anchorMin = next.AnchorMin.Value;
                if ((next.AnchorMax != prev.AnchorMax || anchorsChanged) && next.AnchorMax.HasValue)
                    rt.anchorMax = next.AnchorMax.Value;
                if ((next.Pivot != prev.Pivot || anchorsChanged) && next.Pivot.HasValue)
                    rt.pivot = next.Pivot.Value;

                if (next.AnchoredPosition != prev.AnchoredPosition && next.AnchoredPosition.HasValue)
                    rt.anchoredPosition = next.AnchoredPosition.Value;
                if (next.SizeDelta != prev.SizeDelta && next.SizeDelta.HasValue)
                    rt.sizeDelta = next.SizeDelta.Value;
                if (next.OffsetMin != prev.OffsetMin && next.OffsetMin.HasValue)
                    rt.offsetMin = next.OffsetMin.Value;
                if (next.OffsetMax != prev.OffsetMax && next.OffsetMax.HasValue)
                    rt.offsetMax = next.OffsetMax.Value;

                if (next.RotationZ != prev.RotationZ && next.RotationZ.HasValue)
                    rt.localRotation = Quaternion.Euler(0f, 0f, next.RotationZ.Value);
                if (next.Scale != prev.Scale && next.Scale.HasValue)
                    rt.localScale = next.Scale.Value;
            }

            if (next.Active != prev.Active)
                go.SetActive(next.Active ?? true);
        }
    }
}
