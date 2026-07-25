using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Simple renderer using Fiber reconciler
    /// Drop-in replacement for VNodeHostRenderer
    /// </summary>
    public class FiberRenderer
    {
        private FiberRoot _root;
        private FiberReconciler _reconciler;
        private object _container;
        private FiberHostConfig _hostConfig;

#if UNITY_EDITOR
        /// <summary>HMR: exposes the root for fiber tree walking.</summary>
        internal FiberRoot Root => _root;
#endif

        public FiberRenderer(VisualElement container, HostContext context = null)
        {
            _container = container;

            if (context == null)
            {
                var registry = ElementRegistryProvider.GetDefaultRegistry();
                context = new HostContext(registry);
            }

            _hostConfig = context.HostConfig;
            _reconciler = new FiberReconciler(context);
        }

        /// <summary>
        /// Backend-agnostic entry: the container is an opaque host handle
        /// interpreted by <paramref name="context"/>'s
        /// <see cref="HostContext.HostConfig"/> (e.g. a RectTransform host
        /// under the uGUI backend).
        /// </summary>
        public FiberRenderer(object container, HostContext context)
        {
            if (context == null)
            {
                var registry = ElementRegistryProvider.GetDefaultRegistry();
                context = new HostContext(registry);
            }

            _container = container;
            _hostConfig = context.HostConfig;
            _reconciler = new FiberReconciler(context);
        }

        /// <summary>
        /// Render a virtual node tree (initial mount)
        /// </summary>
        public void Render(VirtualNode vnode)
        {
            if (_root == null)
            {
                // Initial mount - ensure container is clean
                _hostConfig.ClearChildren(_container);
                _root = _reconciler.CreateRoot(_container, vnode);
            }
            else
            {
                // Update
                _reconciler.ScheduleUpdateOnFiber(_root.Current, vnode);
            }
        }

        /// <summary>
        /// Clear and unmount the entire tree.
        ///
        /// <para>
        /// Runs every effect cleanup (UseEffect / UseLayoutEffect) and
        /// disposes every signal subscription before clearing the host
        /// container. This is critical for components that own external
        /// resources (pooled <c>VideoPlayer</c>, <c>AudioSource</c>,
        /// timers, native listeners, etc.) \u2014 without this they leak,
        /// e.g. an &lt;Audio&gt; element keeps playing forever after its
        /// owning EditorWindow is closed.
        /// </para>
        /// </summary>
        public void Clear()
        {
            _reconciler?.UnmountRoot();
            _hostConfig.ClearChildren(_container);
            _root = null;
        }

        /// <summary>
        /// Repoint the renderer at a new container VisualElement without
        /// tearing down the fiber tree. Moves all currently-mounted
        /// children from the old container to the new one (preserving
        /// child VisualElement identity, hooks, refs, animations) and
        /// updates the FiberRoot / root FiberNode host pointers so
        /// subsequent reconciliations write to the correct element.
        ///
        /// Called by <see cref="VNodeHostRenderer.RetargetHost"/> in
        /// response to UIDocument panel rebuilds where the parent
        /// rootVisualElement was replaced but the user's logical UI tree
        /// is unchanged.
        /// </summary>
        public void RetargetContainer(object nextContainer)
        {
            if (nextContainer == null || ReferenceEquals(nextContainer, _container))
            {
                return;
            }
            // Snapshot before moving — Add() removes from current parent and
            // mutates the source collection.
            int childCount = _hostConfig.GetChildCount(_container);
            if (childCount > 0)
            {
                var moved = new object[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    moved[i] = _hostConfig.GetChildAt(_container, i);
                }
                for (int i = 0; i < childCount; i++)
                {
                    _hostConfig.AppendChild(nextContainer, moved[i]);
                }
            }
            _container = nextContainer;
            if (_root != null)
            {
                _root.ContainerElement = nextContainer;
                if (_root.Current != null)
                {
                    _root.Current.HostElement = nextContainer;
                }
            }
        }
    }
}
