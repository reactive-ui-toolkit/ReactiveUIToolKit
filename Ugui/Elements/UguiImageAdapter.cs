using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiImageAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("Image");
            var image = go.AddComponent<Image>();
            // Raycast hygiene: purely visual elements do not block raycasts
            // unless asked to (raycastTarget prop overrides).
            image.raycastTarget = false;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            UguiImageApplier.ApplyFull(go.GetComponent<Image>(), props as UguiImageProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            UguiImageApplier.ApplyDiff(
                go.GetComponent<Image>(),
                prev as UguiImageProps,
                next as UguiImageProps
            );
            base.ApplyTypedDiff(go, prev, next);
        }
    }
}
