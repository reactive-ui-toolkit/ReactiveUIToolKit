using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Low-level pointer events as a prop group on any element. Delegate
    /// fields compare by reference in ShallowEquals, so stable handlers
    /// (component methods, cached lambdas) diff cleanly.
    /// </summary>
    public sealed class UguiPointerEvents
    {
        public Action<PointerEventData> OnPointerEnter { get; set; }
        public Action<PointerEventData> OnPointerExit { get; set; }
        public Action<PointerEventData> OnPointerDown { get; set; }
        public Action<PointerEventData> OnPointerUp { get; set; }
        public Action<PointerEventData> OnPointerClick { get; set; }
        public Action<PointerEventData> OnBeginDrag { get; set; }
        public Action<PointerEventData> OnDrag { get; set; }
        public Action<PointerEventData> OnEndDrag { get; set; }
        public Action<PointerEventData> OnScroll { get; set; }

        internal static bool ValueEquals(UguiPointerEvents a, UguiPointerEvents b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.OnPointerEnter == b.OnPointerEnter
                && a.OnPointerExit == b.OnPointerExit
                && a.OnPointerDown == b.OnPointerDown
                && a.OnPointerUp == b.OnPointerUp
                && a.OnPointerClick == b.OnPointerClick
                && a.OnBeginDrag == b.OnBeginDrag
                && a.OnDrag == b.OnDrag
                && a.OnEndDrag == b.OnEndDrag
                && a.OnScroll == b.OnScroll;
        }
    }

    /// <summary>
    /// Attached lazily only when the element declares pointer handlers; holds
    /// the current handler set so prop diffs are plain field writes.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiPointerBridge
        : MonoBehaviour,
            IPointerEnterHandler,
            IPointerExitHandler,
            IPointerDownHandler,
            IPointerUpHandler,
            IPointerClickHandler,
            IBeginDragHandler,
            IDragHandler,
            IEndDragHandler,
            IScrollHandler
    {
        internal UguiPointerEvents Current;

        internal static UguiPointerBridge GetOrAdd(GameObject go)
        {
            var bridge = go.GetComponent<UguiPointerBridge>();
            if (bridge == null)
            {
                bridge = go.AddComponent<UguiPointerBridge>();
                bridge.hideFlags = HideFlags.HideInInspector;
            }
            return bridge;
        }

        public void OnPointerEnter(PointerEventData e) => Current?.OnPointerEnter?.Invoke(e);

        public void OnPointerExit(PointerEventData e) => Current?.OnPointerExit?.Invoke(e);

        public void OnPointerDown(PointerEventData e) => Current?.OnPointerDown?.Invoke(e);

        public void OnPointerUp(PointerEventData e) => Current?.OnPointerUp?.Invoke(e);

        public void OnPointerClick(PointerEventData e) => Current?.OnPointerClick?.Invoke(e);

        public void OnBeginDrag(PointerEventData e) => Current?.OnBeginDrag?.Invoke(e);

        public void OnDrag(PointerEventData e) => Current?.OnDrag?.Invoke(e);

        public void OnEndDrag(PointerEventData e) => Current?.OnEndDrag?.Invoke(e);

        public void OnScroll(PointerEventData e) => Current?.OnScroll?.Invoke(e);
    }
}
