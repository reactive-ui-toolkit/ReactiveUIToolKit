using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Plain RectTransform container, created full-stretch by default (the
    /// GameObject &gt; UI &gt; Panel habit). No Graphic — zero draw cost.
    /// </summary>
    public sealed class UguiPanelAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }
    }
}
