namespace Ruitk.Core.Fiber
{
    /// <summary>
    /// Root of the fiber tree - matches React's FiberRoot
    /// </summary>
    public class FiberRoot
    {
        /// <summary>The root container — opaque host handle owned by the mount's backend.</summary>
        public object ContainerElement;

        /// <summary>Current fiber tree (what's on screen)</summary>
        public FiberNode Current;

        /// <summary>Work-in-progress fiber tree (being built)</summary>
        public FiberNode WorkInProgress;

        /// <summary>First effect in the effect list</summary>
        public FiberNode FirstEffect;

        /// <summary>Last effect in the effect list</summary>
        public FiberNode LastEffect;

        /// <summary>Pending work (for scheduling)</summary>
        public bool HasPendingWork;

        /// <summary>Host context</summary>
        public HostContext Context;

        /// <summary>Reconciler instance</summary>
        public FiberReconciler Reconciler;

        /// <summary>The last rendered root VirtualNode (for updates)</summary>
        public VirtualNode RootVNode;
    }
}
