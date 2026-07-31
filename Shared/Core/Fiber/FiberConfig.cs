namespace Ruitk.Core.Fiber
{
    /// <summary>
    /// Global configuration for Fiber reconciler
    /// </summary>
    public static class FiberConfig
    {
        /// <summary>
        /// Whether the render phase may be time-sliced through an installed scheduler (the
        /// <c>time_slicing</c> setting; default <c>true</c>). <c>false</c> is the explicit
        /// scheduler bypass: state-driven updates run the existing synchronous
        /// <c>WorkLoop()</c> even when a scheduler is present (the initial mount is always
        /// synchronous either way, and passive effects keep their normal scheduler-deferred
        /// timing). Set once per mount from
        /// <c>BuildDefinesConfig.ResolveTimeSlicing()</c>.
        /// </summary>
        public static bool TimeSlicingEnabled { get; set; } = true;

        /// <summary>
        /// Maximum milliseconds the render phase runs per slice before yielding back to the
        /// scheduler (the <c>time_slice_ms</c> setting; default <c>2.0</c>). Applies wherever
        /// a scheduler slices — play-mode/player <c>RenderScheduler</c> and the editor
        /// scheduler alike. Set once per mount from
        /// <c>BuildDefinesConfig.ResolveTimeSliceMs()</c>.
        /// </summary>
        public static float TimeSliceMs { get; set; } = 2.0f;

        /// <summary>
        /// Enable verbose fiber logging
        /// </summary>
        public static bool EnableFiberLogging { get; set; } = false;

        /// <summary>
        /// Enable to see which reconciler is being used
        /// </summary>
        public static bool ShowReconcilerInfo { get; set; } = false;
    }
}
