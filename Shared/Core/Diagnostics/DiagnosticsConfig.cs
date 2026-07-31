using System;

namespace Ruitk.Core.Diagnostics
{
    /// <summary>
    /// Global diagnostics configuration, independent of any particular reconciler.
    /// Backed by RuitkConfig / BuildDefinesConfig at startup.
    /// </summary>
    public static class DiagnosticsConfig
    {
        public enum TraceLevel
        {
            None,
            Basic,
            Verbose,
        }

        /// <summary>
        /// Current trace level. Defaults to None.
        /// </summary>
        public static TraceLevel CurrentTraceLevel { get; set; } = TraceLevel.None;

        /// <summary>
        /// When true, additional diff / reconciliation tracing is enabled.
        /// </summary>
        public static bool EnableDiffTracing { get; set; } = false;
    }
}

