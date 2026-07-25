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
            if (props.Pointer != null)
                UguiPointerBridge.GetOrAdd(go).Current = props.Pointer;
            if (props.CanvasGroup != null)
                ApplyCanvasGroup(go, props.CanvasGroup);
            if (props.Mask != null)
                ApplyMask(go, props.Mask);
            if (props.RectMask2D != null)
                ApplyRectMask2D(go, props.RectMask2D);
            if (props.Shadow != null)
                ApplyEffect<Shadow>(go, props.Shadow);
            if (props.Outline != null)
                ApplyEffect<Outline>(go, props.Outline);
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
            if (!UguiPointerEvents.ValueEquals(prev.Pointer, next.Pointer))
            {
                if (next.Pointer != null)
                {
                    UguiPointerBridge.GetOrAdd(go).Current = next.Pointer;
                }
                else
                {
                    var bridge = go.GetComponent<UguiPointerBridge>();
                    if (bridge != null)
                        bridge.Current = null;
                }
            }
            if (!UguiCanvasGroup.ValueEquals(prev.CanvasGroup, next.CanvasGroup))
            {
                if (next.CanvasGroup != null)
                    ApplyCanvasGroup(go, next.CanvasGroup);
                else
                    RemoveComponent<CanvasGroup>(go);
            }
            if (!UguiMask.ValueEquals(prev.Mask, next.Mask))
            {
                if (next.Mask != null)
                    ApplyMask(go, next.Mask);
                else
                    RemoveComponent<Mask>(go);
            }
            if (!UguiRectMask2D.ValueEquals(prev.RectMask2D, next.RectMask2D))
            {
                if (next.RectMask2D != null)
                    ApplyRectMask2D(go, next.RectMask2D);
                else
                    RemoveComponent<RectMask2D>(go);
            }
            if (!UguiShadowEffect.ValueEquals(prev.Shadow, next.Shadow))
            {
                if (next.Shadow != null)
                    ApplyEffect<Shadow>(go, next.Shadow);
                else
                    RemoveEffect<Shadow>(go);
            }
            if (!UguiShadowEffect.ValueEquals(prev.Outline, next.Outline))
            {
                if (next.Outline != null)
                    ApplyEffect<Outline>(go, next.Outline);
                else
                    RemoveEffect<Outline>(go);
            }
        }

        private static void ApplyCanvasGroup(GameObject go, UguiCanvasGroup g)
        {
            var group = GetOrAdd<CanvasGroup>(go);
            if (g.Alpha.HasValue)
                group.alpha = g.Alpha.Value;
            if (g.Interactable.HasValue)
                group.interactable = g.Interactable.Value;
            if (g.BlocksRaycasts.HasValue)
                group.blocksRaycasts = g.BlocksRaycasts.Value;
            if (g.IgnoreParentGroups.HasValue)
                group.ignoreParentGroups = g.IgnoreParentGroups.Value;
        }

        private static void ApplyMask(GameObject go, UguiMask g)
        {
            var mask = GetOrAdd<Mask>(go);
            if (g.ShowMaskGraphic.HasValue)
                mask.showMaskGraphic = g.ShowMaskGraphic.Value;
        }

        private static void ApplyRectMask2D(GameObject go, UguiRectMask2D g)
        {
            var mask = GetOrAdd<RectMask2D>(go);
            if (g.Padding.HasValue)
                mask.padding = g.Padding.Value;
            if (g.Softness.HasValue)
                mask.softness = g.Softness.Value;
        }

        // Outline derives from Shadow, so effect lookup must be exact-typed —
        // a generic GetComponent<Shadow> would return an Outline.
        private static void ApplyEffect<T>(GameObject go, UguiShadowEffect g)
            where T : Shadow
        {
            T effect = FindExact<T>(go);
            if (effect == null)
                effect = go.AddComponent<T>();
            if (g.EffectColor.HasValue)
                effect.effectColor = g.EffectColor.Value;
            if (g.EffectDistance.HasValue)
                effect.effectDistance = g.EffectDistance.Value;
            if (g.UseGraphicAlpha.HasValue)
                effect.useGraphicAlpha = g.UseGraphicAlpha.Value;
        }

        private static void RemoveEffect<T>(GameObject go)
            where T : Shadow
        {
            var effect = FindExact<T>(go);
            if (effect == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(effect);
            else
                Object.DestroyImmediate(effect);
        }

        private static T FindExact<T>(GameObject go)
            where T : Shadow
        {
            var candidates = go.GetComponents<Shadow>();
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i].GetType() == typeof(T))
                    return (T)candidates[i];
            }
            return null;
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
