using System.Collections.Generic;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// UI Toolkit host backend. Host handles are <see cref="VisualElement"/>s;
    /// element creation and property application resolve through the
    /// <see cref="ElementRegistry"/> adapters.
    /// </summary>
    public sealed class UitkHostConfig : FiberHostConfig
    {
        private readonly ElementRegistry _registry;

        public UitkHostConfig(ElementRegistry registry)
        {
            _registry = registry;
        }

        public override object CreateElement(string elementType)
        {
            var adapter = _registry.Resolve(elementType);
            VisualElement element;
            if (adapter != null)
            {
                element = adapter.Create();
            }
            else
            {
                // Fallback to generic VisualElement
                element = new VisualElement { name = elementType };
            }
            return element;
        }

        public override void ApplyProperties(
            object element,
            string elementType,
            IReadOnlyDictionary<string, object> oldProps,
            IReadOnlyDictionary<string, object> newProps
        )
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter != null && newProps != null)
            {
                var ve = (VisualElement)element;
                if (oldProps != null)
                {
                    adapter.ApplyPropertiesDiff(ve, oldProps, newProps);
                }
                else
                {
                    adapter.ApplyProperties(ve, newProps);
                }
            }
        }

        public override void ApplyTypedProperties(
            object element,
            string elementType,
            BaseProps oldProps,
            BaseProps newProps
        )
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter is ITypedElementAdapter typed && newProps != null)
            {
                var ve = (VisualElement)element;
                if (oldProps != null)
                    typed.ApplyTypedDiff(ve, oldProps, newProps);
                else
                    typed.ApplyTypedFull(ve, newProps);
            }
        }

        public override void AppendChild(object parent, object child)
        {
            ((VisualElement)parent).Add((VisualElement)child);
        }

        public override void InsertBefore(object parent, object child, object beforeChild)
        {
            var parentVe = (VisualElement)parent;
            var childVe = (VisualElement)child;
            if (beforeChild == null)
            {
                parentVe.Add(childVe);
            }
            else
            {
                int index = parentVe.IndexOf((VisualElement)beforeChild);
                if (index >= 0)
                {
                    parentVe.Insert(index, childVe);
                }
                else
                {
                    parentVe.Add(childVe);
                }
            }
        }

        public override void RemoveChild(object parent, object child)
        {
            var parentVe = (VisualElement)parent;
            var childVe = (VisualElement)child;
            if (childVe != null && childVe.parent == parentVe)
            {
                parentVe.Remove(childVe);
            }
        }

        public override object GetParent(object element)
        {
            return ((VisualElement)element)?.parent;
        }

        public override void ClearChildren(object element)
        {
            ((VisualElement)element)?.Clear();
        }

        public override int GetChildCount(object element)
        {
            return ((VisualElement)element)?.childCount ?? 0;
        }

        public override object GetChildAt(object element, int index)
        {
            return ((VisualElement)element)[index];
        }

        public override void OnHostRemoved(object element)
        {
            // Clean up previousStyles tracking to prevent memory leak (P0-5)
            ReactiveUITK.Props.PropsApplier.NotifyElementRemoved((VisualElement)element);
        }

        public override string GetDebugName(object element)
        {
            return (element as VisualElement)?.name ?? element?.GetType().Name;
        }
    }
}
