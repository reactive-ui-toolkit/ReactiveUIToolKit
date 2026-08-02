using System;
using Ruitk.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ruitk.Core
{
    public sealed class RootRenderer : MonoBehaviour
    {
        public static RootRenderer Instance { get; private set; }
        private HostContext sharedHostContext;
        private ElementRegistry elementRegistry;
        private IRootSource rootSource;
        private VisualElement rootElement;
        private VNodeHostRenderer vnodeHostRenderer;

        // Deferred mount: a Render() that arrives before the host has built
        // its panel is held here and replayed when the root source produces a
        // root. Previously the vnode was silently discarded, which forced
        // callers to sequence their first Render after Unity's panel build.
        private VirtualNode pendingRootNode;

        private void EnsureSetup()
        {
            if (elementRegistry == null)
            {
                elementRegistry = ElementRegistryProvider.GetDefaultRegistry();
            }
            if (sharedHostContext == null)
            {
                if (RenderScheduler.Instance == null)
                {
                    var go = new GameObject("RenderScheduler");
                    go.hideFlags = HideFlags.DontSave;
                    go.AddComponent<RenderScheduler>();
                }
                sharedHostContext = RuitkBootstrap.CreateHostContext(
                    elementRegistry,
                    hostConfig: null,
                    scheduler: RenderScheduler.Instance,
                    isEditor: false
                );
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureSetup();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Unmount();
        }

        /// <summary>
        /// Configures the root VisualElement and optionally seeds named environment slots
        /// (portal targets, feature flags, etc.) into the shared <see cref="HostContext"/>.
        /// Must be called before the first <see cref="Render"/> call.
        /// </summary>
        /// <param name="uiRootElement">The VisualElement that acts as the React root.</param>
        /// <param name="env">
        /// Optional callback invoked with the <see cref="HostContext"/> after built-in keys
        /// (scheduler, env, etc.) are set.  Use this to seed named portal target slots:
        /// <code>
        /// rootRenderer.Initialize(uiDoc.rootVisualElement,
        ///     env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = overlayLayer);
        /// </code>
        /// </param>
        public void Initialize(VisualElement uiRootElement, Action<HostContext> env = null)
        {
            AdoptRootSource(new StaticRootSource(uiRootElement), env);
        }

        /// <summary>
        /// UIDocument-aware overload. In the <b>editor</b> this polls the
        /// document's <c>rootVisualElement</c> once per frame and reparents
        /// the mounted fiber tree onto the new root whenever Unity rebuilds
        /// the panel (undo, asset swap, disable/enable, HMR, and the 6.3
        /// <c>InspectorWindow</c> selection storm). Those are hookless,
        /// editor-only mutations with no callback to observe, so polling is
        /// the only correct detection mechanism; the cost is one reference
        /// compare per frame on a tick source already running.
        ///
        /// In <b>player builds</b> the poll is compiled out entirely: a
        /// running game has no hookless panel swaps (every runtime panel
        /// change is developer-initiated), so this overload simply seeds the
        /// initial root from <paramref name="hostDoc"/>, exactly like
        /// <see cref="Initialize(VisualElement, Action{HostContext})"/>. A
        /// build that deliberately rebuilds a UIDocument at runtime should
        /// re-call <see cref="Render"/> (or this overload) from the code that
        /// triggers the rebuild.
        /// </summary>
        public void Initialize(UIDocument hostDoc, Action<HostContext> env = null)
        {
            AdoptRootSource(new UIDocumentRootSource(hostDoc), env);
        }

        private void AdoptRootSource(IRootSource source, Action<HostContext> env)
        {
            EnsureSetup();
            rootSource?.Stop();
            rootSource = source;
            rootElement = source.CurrentRoot as VisualElement;
            env?.Invoke(sharedHostContext);
            source.Start(OnRootSourceChanged);
        }

        private void OnRootSourceChanged()
        {
            var next = rootSource?.CurrentRoot as VisualElement;
            rootElement = next;
            if (next == null)
            {
                return;
            }
            if (vnodeHostRenderer != null)
            {
                // Move the live tree onto the freshly-built root, preserving
                // hook, ref and animation state.
                vnodeHostRenderer.RetargetHost(next);
                return;
            }
            if (pendingRootNode != null)
            {
                var deferred = pendingRootNode;
                pendingRootNode = null;
                Render(deferred);
            }
        }

        public void Render(VirtualNode rootNode)
        {
            EnsureSetup();
            if (rootElement == null)
            {
                pendingRootNode = rootNode;
                return;
            }
            pendingRootNode = null;
            if (vnodeHostRenderer == null)
            {
                vnodeHostRenderer = new VNodeHostRenderer(sharedHostContext, rootElement);
            }
            vnodeHostRenderer.Render(rootNode);
        }

        public void Unmount()
        {
            rootSource?.Stop();
            pendingRootNode = null;
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.Unmount();
                vnodeHostRenderer = null;
            }
        }
    }
}
