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
            var tag = UguiNodeTag.GetOrAdd(go);
            tag.Graphic = raw;
            tag.Control = raw;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(CachedRaw(go), null, props as UguiRawImageProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(CachedRaw(go), prev as UguiRawImageProps, next as UguiRawImageProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static RawImage CachedRaw(GameObject go)
        {
            var tag = UguiNodeTag.Find(go);
            return tag != null && tag.Control is RawImage raw ? raw : go.GetComponent<RawImage>();
        }

        public override bool TryResetForPool(GameObject go)
        {
            var raw = CachedRaw(go);
            if (raw == null)
                return false;
            ResetCommonState(go);
            go.name = "RawImage";
            raw.texture = null;
            raw.uvRect = new Rect(0f, 0f, 1f, 1f);
            raw.color = Color.white;
            raw.material = null;
            raw.raycastTarget = false;
            raw.maskable = true;
            return true;
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
