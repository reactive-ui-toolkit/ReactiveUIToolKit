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

        public override bool TryResetForPool(GameObject go)
        {
            var image = go.GetComponent<Image>();
            if (image == null)
                return false;
            ResetCommonState(go);
            go.name = "Image";
            ResetImageState(image);
            return true;
        }

        internal static void ResetImageState(Image image)
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillAmount = 1f;
            image.fillOrigin = 0;
            image.fillClockwise = true;
            image.fillCenter = true;
            image.preserveAspect = false;
            image.pixelsPerUnitMultiplier = 1f;
            image.useSpriteMesh = false;
            image.alphaHitTestMinimumThreshold = 0f;
            image.color = Color.white;
            image.material = null;
            image.raycastTarget = false;
            image.raycastPadding = Vector4.zero;
            image.maskable = true;
        }
    }
}
