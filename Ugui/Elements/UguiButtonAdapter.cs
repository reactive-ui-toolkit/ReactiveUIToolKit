using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// The GameObject &gt; UI &gt; Button shape: one GO carrying an Image
    /// (the background, also the Selectable's target graphic) and a Button.
    /// Children (e.g. a Text label) mount inside as usual.
    /// </summary>
    public sealed class UguiButtonAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("Button");
            var image = go.AddComponent<Image>();
            image.raycastTarget = true;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            var bp = props as UguiButtonProps;
            UguiImageApplier.ApplyFull(go.GetComponent<Image>(), bp);
            ApplyButton(go, null, bp);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            var bp = prev as UguiButtonProps;
            var bn = next as UguiButtonProps;
            UguiImageApplier.ApplyDiff(go.GetComponent<Image>(), bp, bn);
            ApplyButton(go, bp, bn);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void ApplyButton(GameObject go, UguiButtonProps prev, UguiButtonProps next)
        {
            if (next == null)
                return;
            var button = go.GetComponent<Button>();
            if (button == null)
                return;

            UguiSelectableApplier.Apply(button, prev, next);
            if (prev == null || next.OnClick != prev.OnClick)
            {
                UguiButtonBinding.GetOrAdd(go).Current = next.OnClick;
            }
        }
    }
}
