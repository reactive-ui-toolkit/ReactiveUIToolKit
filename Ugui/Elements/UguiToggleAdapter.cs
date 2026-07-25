using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Toggle with the standard Background/Checkmark part structure. The
    /// background Image is the Selectable's target graphic; the checkmark is
    /// toggle.graphic. User children (labels) mount at the root after the
    /// parts. Value writes use SetIsOnWithoutNotify (controlled-component
    /// semantics — no event echo).
    /// </summary>
    public sealed class UguiToggleAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var root = new GameObject("Toggle", typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            rootRt.sizeDelta = new Vector2(160f, 20f);

            var background = new GameObject("Background");
            var bgImage = background.AddComponent<Image>();
            bgImage.raycastTarget = true;
            var bgRt = (RectTransform)background.transform;
            bgRt.SetParent(root.transform, false);
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.sizeDelta = new Vector2(20f, 20f);

            var checkmark = new GameObject("Checkmark");
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.raycastTarget = false;
            var checkRt = (RectTransform)checkmark.transform;
            checkRt.SetParent(background.transform, false);
            checkRt.anchorMin = new Vector2(0.5f, 0.5f);
            checkRt.anchorMax = new Vector2(0.5f, 0.5f);
            checkRt.sizeDelta = new Vector2(14f, 14f);

            var toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            return root;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiToggleProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiToggleProps, next as UguiToggleProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiToggleProps prev, UguiToggleProps next)
        {
            if (next == null)
                return;
            var toggle = go.GetComponent<Toggle>();
            if (toggle == null)
                return;
            bool full = prev == null;

            UguiImageApplier.ApplyDiffOrFull(toggle.targetGraphic as Image, prev, next);
            UguiSelectableApplier.Apply(toggle, prev, next);

            var checkmark = toggle.graphic as Image;
            if (checkmark != null)
            {
                if (
                    (full || !ReferenceEquals(next.CheckmarkSprite, prev.CheckmarkSprite))
                    && next.CheckmarkSprite != null
                )
                    checkmark.sprite = next.CheckmarkSprite;
                if ((full || next.CheckmarkColor != prev.CheckmarkColor) && next.CheckmarkColor.HasValue)
                    checkmark.color = next.CheckmarkColor.Value;
            }

            if ((full || !ReferenceEquals(next.Group, prev.Group)) && next.Group != null)
                toggle.group = next.Group;
            var binding = UguiToggleBinding.GetOrAdd(go);
            if (full || next.OnValueChanged != prev.OnValueChanged)
                binding.Current = next.OnValueChanged;
            binding.AutoJoinGroup = next.JoinGroup ?? false;
            if ((full || next.IsOn != prev.IsOn) && next.IsOn.HasValue)
                toggle.SetIsOnWithoutNotify(next.IsOn.Value);
        }
    }
}
