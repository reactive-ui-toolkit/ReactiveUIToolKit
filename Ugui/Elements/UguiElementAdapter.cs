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
    }
}
