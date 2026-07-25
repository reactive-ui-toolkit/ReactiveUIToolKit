using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Applies the shared prop groups: non-null group adds/updates the
    /// component, non-null → null removes it. Setter writes go through the
    /// components' own properties, so uGUI's native layout dirtying applies
    /// exactly as it does for hand-written code.
    /// </summary>
    internal static class UguiPropGroupApplier
    {
        internal static void ApplyFull(GameObject go, UguiBaseProps props)
        {
            if (props == null)
                return;
            if (props.LayoutElement != null)
                ApplyLayoutElement(go, props.LayoutElement);
            if (props.ContentSizeFitter != null)
                ApplyContentSizeFitter(go, props.ContentSizeFitter);
            if (props.AspectRatioFitter != null)
                ApplyAspectRatioFitter(go, props.AspectRatioFitter);
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

            if (!UguiLayoutElement.ValueEquals(prev.LayoutElement, next.LayoutElement))
            {
                if (next.LayoutElement != null)
                    ApplyLayoutElement(go, next.LayoutElement);
                else
                    RemoveComponent<LayoutElement>(go);
            }
            if (!UguiContentSizeFitter.ValueEquals(prev.ContentSizeFitter, next.ContentSizeFitter))
            {
                if (next.ContentSizeFitter != null)
                    ApplyContentSizeFitter(go, next.ContentSizeFitter);
                else
                    RemoveComponent<ContentSizeFitter>(go);
            }
            if (!UguiAspectRatioFitter.ValueEquals(prev.AspectRatioFitter, next.AspectRatioFitter))
            {
                if (next.AspectRatioFitter != null)
                    ApplyAspectRatioFitter(go, next.AspectRatioFitter);
                else
                    RemoveComponent<AspectRatioFitter>(go);
            }
        }

        private static void ApplyLayoutElement(GameObject go, UguiLayoutElement g)
        {
            var le = GetOrAdd<LayoutElement>(go);
            if (g.IgnoreLayout.HasValue)
                le.ignoreLayout = g.IgnoreLayout.Value;
            le.minWidth = g.MinWidth ?? -1f;
            le.minHeight = g.MinHeight ?? -1f;
            le.preferredWidth = g.PreferredWidth ?? -1f;
            le.preferredHeight = g.PreferredHeight ?? -1f;
            le.flexibleWidth = g.FlexibleWidth ?? -1f;
            le.flexibleHeight = g.FlexibleHeight ?? -1f;
            if (g.LayoutPriority.HasValue)
                le.layoutPriority = g.LayoutPriority.Value;
        }

        private static void ApplyContentSizeFitter(GameObject go, UguiContentSizeFitter g)
        {
            var fitter = GetOrAdd<ContentSizeFitter>(go);
            fitter.horizontalFit = g.HorizontalFit ?? ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = g.VerticalFit ?? ContentSizeFitter.FitMode.Unconstrained;
        }

        private static void ApplyAspectRatioFitter(GameObject go, UguiAspectRatioFitter g)
        {
            var fitter = GetOrAdd<AspectRatioFitter>(go);
            fitter.aspectMode = g.AspectMode ?? AspectRatioFitter.AspectMode.None;
            if (g.AspectRatio.HasValue)
                fitter.aspectRatio = g.AspectRatio.Value;
        }

        private static T GetOrAdd<T>(GameObject go)
            where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static void RemoveComponent<T>(GameObject go)
            where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(c);
            else
                Object.DestroyImmediate(c);
        }
    }
}
