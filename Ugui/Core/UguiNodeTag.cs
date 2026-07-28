using System.Collections.Generic;
using UnityEngine;

namespace Ruitk.Ugui
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

        // Typed component cache, populated by the adapter at Create time so
        // the per-frame diff path writes properties directly — the uGUI
        // equivalent of UITK diffs writing on the typed VisualElement, with
        // no GetComponent on the hot path. Adapters fall back to
        // GetComponent only when a cache slot is empty.
        internal UnityEngine.UI.Graphic Graphic;
        internal UnityEngine.UI.Image Image;
        internal UnityEngine.UI.Selectable Selectable;
        internal Component Control;

        private static readonly Dictionary<GameObject, UguiNodeTag> s_byGo =
            new Dictionary<GameObject, UguiNodeTag>();

        internal static UguiNodeTag GetOrAdd(GameObject go)
        {
            if (s_byGo.TryGetValue(go, out var cached) && cached != null)
            {
                return cached;
            }
            var tag = go.GetComponent<UguiNodeTag>();
            if (tag == null)
            {
                tag = go.AddComponent<UguiNodeTag>();
                tag.hideFlags = HideFlags.HideInInspector;
            }
            s_byGo[go] = tag;
            return tag;
        }

        /// <summary>Cache lookup without adding — null for untagged hosts.</summary>
        internal static UguiNodeTag Find(GameObject go)
        {
            if (s_byGo.TryGetValue(go, out var cached) && cached != null)
            {
                return cached;
            }
            var tag = go.GetComponent<UguiNodeTag>();
            if (tag != null)
            {
                s_byGo[go] = tag;
            }
            return tag;
        }

        private void OnDestroy()
        {
            s_byGo.Remove(gameObject);
        }
    }
}
