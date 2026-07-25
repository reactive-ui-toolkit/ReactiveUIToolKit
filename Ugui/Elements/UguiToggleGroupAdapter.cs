using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiToggleGroupProps : UguiBaseProps
    {
        public bool? AllowSwitchOff { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiToggleGroupProps o))
                return false;
            if (AllowSwitchOff != o.AllowSwitchOff)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            AllowSwitchOff = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiToggleGroupProps>.Return(this);
        }
    }

    /// <summary>
    /// Container element carrying a ToggleGroup. Child Toggles with
    /// <c>JoinGroup</c> attach to their nearest ancestor group
    /// automatically; an explicit <c>Group</c> prop always wins.
    /// </summary>
    public sealed class UguiToggleGroupAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("ToggleGroup", typeof(RectTransform), typeof(ToggleGroup));
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiToggleGroupProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiToggleGroupProps, next as UguiToggleGroupProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiToggleGroupProps prev, UguiToggleGroupProps next)
        {
            if (next == null)
                return;
            var group = go.GetComponent<ToggleGroup>();
            if (group == null)
                return;
            if ((prev == null || next.AllowSwitchOff != prev.AllowSwitchOff) && next.AllowSwitchOff.HasValue)
                group.allowSwitchOff = next.AllowSwitchOff.Value;
        }
    }
}
