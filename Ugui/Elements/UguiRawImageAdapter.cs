using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiRawImageAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("RawImage");
            var raw = go.AddComponent<RawImage>();
            raw.raycastTarget = false;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go.GetComponent<RawImage>(), null, props as UguiRawImageProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go.GetComponent<RawImage>(), prev as UguiRawImageProps, next as UguiRawImageProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(RawImage raw, UguiRawImageProps prev, UguiRawImageProps next)
        {
            if (raw == null || next == null)
                return;
            if (prev == null || !ReferenceEquals(next.Texture, prev.Texture))
                raw.texture = next.Texture;
            if ((prev == null || next.UvRect != prev.UvRect) && next.UvRect.HasValue)
                raw.uvRect = next.UvRect.Value;
            if (prev == null)
                UguiGraphicApplier.ApplyFull(raw, next);
            else
                UguiGraphicApplier.ApplyDiff(raw, prev, next);
        }
    }
}
