using System;
using UnityEngine;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Contract a prefab root component can implement to receive declarative
    /// prop updates from a &lt;Prefab bind={...}&gt; element.
    /// </summary>
    public interface IReactivePrefab
    {
        void Bind(object props);
    }

    public sealed class UguiPrefabProps : UguiBaseProps
    {
        public GameObject Source { get; set; }
        public object Bind { get; set; }
        public Action<GameObject> OnInstantiated { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiPrefabProps o))
                return false;
            if (!ReferenceEquals(Source, o.Source))
                return false;
            if (!ReferenceEquals(Bind, o.Bind))
                return false;
            if (OnInstantiated != o.OnInstantiated)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Source = null;
            Bind = null;
            OnInstantiated = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiPrefabProps>.Return(this);
        }
    }

    [AddComponentMenu("")]
    public sealed class UguiPrefabHolder : MonoBehaviour
    {
        internal GameObject Source;
        internal GameObject Instance;
    }

    /// <summary>
    /// The migration bridge: mounts an existing uGUI prefab inside a reactive
    /// tree. The holder rect is full-stretch, so the prefab's own anchors
    /// behave exactly as they would under the mount parent. A Source change
    /// swaps the instance; Bind is delivered to every IReactivePrefab on the
    /// instance root on every apply where it changed.
    /// </summary>
    public sealed class UguiPrefabAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("Prefab", typeof(RectTransform), typeof(UguiPrefabHolder));
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiPrefabProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiPrefabProps, next as UguiPrefabProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiPrefabProps prev, UguiPrefabProps next)
        {
            if (next == null)
                return;
            var holder = go.GetComponent<UguiPrefabHolder>();
            if (holder == null)
                return;
            bool full = prev == null;

            bool sourceChanged = full || !ReferenceEquals(holder.Source, next.Source);
            if (sourceChanged)
            {
                if (holder.Instance != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(holder.Instance);
                    else
                        UnityEngine.Object.DestroyImmediate(holder.Instance);
                    holder.Instance = null;
                }
                holder.Source = next.Source;
                if (next.Source != null)
                {
                    holder.Instance = UnityEngine.Object.Instantiate(next.Source, go.transform, false);
                    next.OnInstantiated?.Invoke(holder.Instance);
                }
            }

            bool bindChanged = sourceChanged || !ReferenceEquals(prev?.Bind, next.Bind);
            if (bindChanged && next.Bind != null && holder.Instance != null)
            {
                var bindables = holder.Instance.GetComponents<IReactivePrefab>();
                for (int i = 0; i < bindables.Length; i++)
                {
                    bindables[i].Bind(next.Bind);
                }
            }
        }
    }
}
