using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// One adapter per uGUI element type: creates the backing GameObject
    /// (RectTransform + primary components) and applies typed props. The
    /// counterpart of the UI Toolkit BaseElementAdapter for the uGUI backend.
    /// </summary>
    public abstract class UguiElementAdapter
    {
        public abstract GameObject Create();

        public virtual void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            UguiRectApplier.ApplyFull(go, props);
            UguiPropGroupApplier.ApplyFull(go, props);
            if (props?.Ref != null)
            {
                UguiNodeTag.GetOrAdd(go).AssignedRef = props.Ref;
                UguiRefUtility.Assign(props.Ref, go);
            }
        }

        public virtual void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            UguiRectApplier.ApplyDiff(go, prev, next);
            UguiPropGroupApplier.ApplyDiff(go, prev, next);
            if (!ReferenceEquals(prev?.Ref, next?.Ref))
            {
                if (prev?.Ref != null)
                    UguiRefUtility.Assign(prev.Ref, null);
                var tag = UguiNodeTag.GetOrAdd(go);
                tag.AssignedRef = next?.Ref;
                if (next?.Ref != null)
                    UguiRefUtility.Assign(next.Ref, go);
            }
        }

        /// <summary>
        /// Dictionary-props path — only V.Text nodes (lowered to the "Label"
        /// element with a {"text": ...} dictionary) reach uGUI adapters this
        /// way.
        /// </summary>
        public virtual void ApplyProperties(
            GameObject go,
            IReadOnlyDictionary<string, object> properties
        ) { }

        public virtual void ApplyPropertiesDiff(
            GameObject go,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            ApplyProperties(go, next);
        }

        /// <summary>
        /// The GameObject children of this element mount under. Compound
        /// elements (ScrollRect content, dropdown template slots) override.
        /// </summary>
        public virtual GameObject ResolveChildHost(GameObject go) => go;

        /// <summary>
        /// Restore the host to the pristine state <see cref="Create"/>
        /// produces and return true to allow pooling. Only stateless visual
        /// elements opt in — stateful controls (Selectables, compounds)
        /// return false and are destroyed, so pooled reuse can never leak
        /// toggle/input/scroll state between mounts.
        /// </summary>
        public virtual bool TryResetForPool(GameObject go) => false;

        /// <summary>Shared pristine-reset for poolable adapters.</summary>
        protected static void ResetCommonState(GameObject go)
        {
            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(100f, 100f);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
                var lp = rt.localPosition;
                lp.z = 0f;
                rt.localPosition = lp;
            }
            go.layer = 0;
            go.tag = "Untagged";
            go.SetActive(true);

            RemoveIfPresent<UnityEngine.UI.LayoutElement>(go);
            RemoveIfPresent<UnityEngine.UI.ContentSizeFitter>(go);
            RemoveIfPresent<UnityEngine.UI.AspectRatioFitter>(go);
            RemoveIfPresent<CanvasGroup>(go);
            RemoveIfPresent<UnityEngine.UI.Mask>(go);
            RemoveIfPresent<UnityEngine.UI.RectMask2D>(go);
            RemoveShadowEffects(go);

            var bridge = go.GetComponent<UguiPointerBridge>();
            if (bridge != null)
                bridge.Current = null;
            var tag = go.GetComponent<UguiNodeTag>();
            if (tag != null)
                tag.AssignedRef = null;
        }

        private static void RemoveIfPresent<T>(GameObject go)
            where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(c);
            else
                Object.DestroyImmediate(c);
        }

        private static void RemoveShadowEffects(GameObject go)
        {
            var effects = go.GetComponents<UnityEngine.UI.Shadow>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (Application.isPlaying)
                    Object.Destroy(effects[i]);
                else
                    Object.DestroyImmediate(effects[i]);
            }
        }
    }
}
