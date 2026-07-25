using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Hidden bookkeeping component the backend attaches to host GameObjects
    /// when needed: holds the currently-assigned ref target so it can be
    /// nulled on removal, and (later) pooling metadata.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiNodeTag : MonoBehaviour
    {
        internal object AssignedRef;

        /// <summary>
        /// The adapter that created this host — lets the backend route child
        /// operations through the element's ResolveChildHost (compound
        /// elements mount children into an inner rect, not the element root).
        /// </summary>
        internal UguiElementAdapter Adapter;

        internal static UguiNodeTag GetOrAdd(GameObject go)
        {
            var tag = go.GetComponent<UguiNodeTag>();
            if (tag == null)
            {
                tag = go.AddComponent<UguiNodeTag>();
                tag.hideFlags = HideFlags.HideInInspector;
            }
            return tag;
        }
    }
}
