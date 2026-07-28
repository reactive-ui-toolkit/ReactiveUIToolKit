using UnityEngine;
using UnityEngine.UI;

namespace Ruitk.Ugui
{
    /// <summary>
    /// ScrollRect with a RectMask2D viewport. Children mount into the
    /// viewport, and the FIRST child IS the scroll content — exactly the
    /// object a uGUI developer would build by hand (typically a
    /// VerticalLayoutGroup with a contentSizeFitter prop group). The binding
    /// keeps ScrollRect.content pointed at it across child swaps. The
    /// viewport carries a clear, raycastable Image so drag works anywhere in
    /// the scroll area.
    /// </summary>
    public sealed class UguiScrollRectAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var root = new GameObject("ScrollRect", typeof(RectTransform));
            var rootImage = root.AddComponent<Image>();
            rootImage.raycastTarget = false;
            var scroll = root.AddComponent<ScrollRect>();

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            var viewportRt = (RectTransform)viewport.transform;
            viewportRt.SetParent(root.transform, false);
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportRt.pivot = new Vector2(0f, 1f);
            viewport.AddComponent<RectMask2D>();
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;

            scroll.viewport = viewportRt;
            var tag = UguiNodeTag.GetOrAdd(root);
            tag.Control = scroll;
            tag.Image = rootImage;
            tag.Graphic = rootImage;
            return root;
        }

        public override GameObject ResolveChildHost(GameObject go)
        {
            var scroll = CachedScroll(go);
            return scroll != null && scroll.viewport != null ? scroll.viewport.gameObject : go;
        }

        private static ScrollRect CachedScroll(GameObject go)
        {
            var tag = UguiNodeTag.Find(go);
            return tag != null && tag.Control is ScrollRect s ? s : go.GetComponent<ScrollRect>();
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiScrollRectProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiScrollRectProps, next as UguiScrollRectProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiScrollRectProps prev, UguiScrollRectProps next)
        {
            if (next == null)
                return;
            var scroll = CachedScroll(go);
            if (scroll == null)
                return;
            bool full = prev == null;

            var tag = UguiNodeTag.Find(go);
            UguiImageApplier.ApplyDiffOrFull(
                tag != null ? tag.Image : go.GetComponent<Image>(),
                prev,
                next
            );

            if ((full || next.Horizontal != prev.Horizontal) && next.Horizontal.HasValue)
                scroll.horizontal = next.Horizontal.Value;
            if ((full || next.Vertical != prev.Vertical) && next.Vertical.HasValue)
                scroll.vertical = next.Vertical.Value;
            if ((full || next.MovementType != prev.MovementType) && next.MovementType.HasValue)
                scroll.movementType = next.MovementType.Value;
            if ((full || next.Elasticity != prev.Elasticity) && next.Elasticity.HasValue)
                scroll.elasticity = next.Elasticity.Value;
            if ((full || next.Inertia != prev.Inertia) && next.Inertia.HasValue)
                scroll.inertia = next.Inertia.Value;
            if (
                (full || next.DecelerationRate != prev.DecelerationRate)
                && next.DecelerationRate.HasValue
            )
                scroll.decelerationRate = next.DecelerationRate.Value;
            if (
                (full || next.ScrollSensitivity != prev.ScrollSensitivity)
                && next.ScrollSensitivity.HasValue
            )
                scroll.scrollSensitivity = next.ScrollSensitivity.Value;

            if (
                (full || next.HorizontalScrollbarSpacing != prev.HorizontalScrollbarSpacing)
                && next.HorizontalScrollbarSpacing.HasValue
            )
                scroll.horizontalScrollbarSpacing = next.HorizontalScrollbarSpacing.Value;
            if (
                (full || next.VerticalScrollbarSpacing != prev.VerticalScrollbarSpacing)
                && next.VerticalScrollbarSpacing.HasValue
            )
                scroll.verticalScrollbarSpacing = next.VerticalScrollbarSpacing.Value;
            if (
                (full || next.ShowVerticalScrollbar != prev.ShowVerticalScrollbar)
                && next.ShowVerticalScrollbar.HasValue
            )
                SetScrollbar(scroll, vertical: true, next.ShowVerticalScrollbar.Value);
            if (
                (full || next.ShowHorizontalScrollbar != prev.ShowHorizontalScrollbar)
                && next.ShowHorizontalScrollbar.HasValue
            )
                SetScrollbar(scroll, vertical: false, next.ShowHorizontalScrollbar.Value);

            var binding = UguiScrollRectBinding.GetOrAdd(go, scroll.viewport);
            if (full || next.OnValueChanged != prev.OnValueChanged)
                binding.Current = next.OnValueChanged;
        }

        private static void SetScrollbar(ScrollRect scroll, bool vertical, bool show)
        {
            var existing = vertical ? scroll.verticalScrollbar : scroll.horizontalScrollbar;
            if (!show)
            {
                if (existing != null)
                {
                    existing.gameObject.SetActive(false);
                    if (vertical)
                        scroll.verticalScrollbar = null;
                    else
                        scroll.horizontalScrollbar = null;
                }
                return;
            }

            string name = vertical ? "Scrollbar Vertical" : "Scrollbar Horizontal";
            Scrollbar bar = existing;
            if (bar == null)
            {
                var found = scroll.transform.Find(name);
                bar = found != null ? found.GetComponent<Scrollbar>() : null;
            }
            if (bar == null)
            {
                var barGo = DefaultControls.CreateScrollbar(UguiDefaultResources.GetLegacyResources());
                barGo.name = name;
                var rt = (RectTransform)barGo.transform;
                rt.SetParent(scroll.transform, false);
                if (vertical)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = Vector2.one;
                    rt.pivot = Vector2.one;
                    rt.sizeDelta = new Vector2(20f, 0f);
                    barGo.GetComponent<Scrollbar>().direction = Scrollbar.Direction.BottomToTop;
                }
                else
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = Vector2.zero;
                    rt.sizeDelta = new Vector2(0f, 20f);
                }
                bar = barGo.GetComponent<Scrollbar>();
            }
            bar.gameObject.SetActive(true);
            if (vertical)
            {
                scroll.verticalScrollbar = bar;
                scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            }
            else
            {
                scroll.horizontalScrollbar = bar;
                scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            }
        }
    }
}
