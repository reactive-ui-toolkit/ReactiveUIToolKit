using System;

namespace Ruitk.Props.Typed
{
    /// <summary>
    /// Backend-neutral base for typed host-element props. The fiber stores and
    /// compares host props through this type only; each backend family
    /// (UI Toolkit's <see cref="BaseProps"/>, the uGUI props family) supplies
    /// its own fields, equality, and pooling underneath it.
    /// </summary>
    public abstract class HostPropsBase : global::Ruitk.Core.IProps
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Pool generation stamp (shared by all families)
        //  0 = user-created via new — never pooled
        //  >0 = rented from a family pool
        // ═══════════════════════════════════════════════════════════════════
        internal uint _generation;

        // Idempotent return guard: true when this instance is currently in a
        // family's pending-return list waiting to be moved to its pool.
        // Prevents the same instance from being scheduled twice in one flush
        // window (which would push it into the pool twice and let two future
        // Rents hand out the same instance).
        internal bool _isPendingReturn;

        /// <summary>Host element name (both backends expose a name).</summary>
        public string Name { get; set; }

        /// <summary>
        /// Ref target invoked by the owning backend when the host element is
        /// created or removed. Interpretation is backend-specific.
        /// </summary>
        public object Ref { get; set; }

        /// <summary>
        /// Field-by-field equality for host bailout, dispatched across
        /// families. Instances of different families are never equal.
        /// </summary>
        public abstract bool HostShallowEquals(HostPropsBase other);

        /// <summary>
        /// Schedule this instance (and any pooled resources it owns, e.g. its
        /// Style for the UI Toolkit family) for return to its family pool at
        /// the next flush. Called by the reconciler in the commit phase only.
        /// </summary>
        internal abstract void __ScheduleReturnToFamilyPool();

        // ═══════════════════════════════════════════════════════════════════
        //  Family flush registry — each family registers one flusher from its
        //  static constructor; the reconciler flushes all families once per
        //  commit. Flushing an unregistered (never-used) family is a no-op by
        //  construction, so registration-on-first-use is safe: an instance
        //  can only become pending after its family's type initializer ran.
        // ═══════════════════════════════════════════════════════════════════
        private static Action s_familyFlushers;

        internal static void __RegisterFamilyFlusher(Action flusher)
        {
            s_familyFlushers += flusher;
        }

        internal static void __FlushAllFamilies()
        {
            s_familyFlushers?.Invoke();
        }
    }
}
