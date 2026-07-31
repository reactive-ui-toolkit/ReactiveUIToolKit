using Ruitk.Core.Config;
using Ruitk.Core.Diagnostics;
using UnityEngine;

namespace Ruitk.Core
{
    /// <summary>
    /// The bootstrap-facing resolvers the root renderers call once at mount. Resolution order for
    /// every value: the JSON settings store (<see cref="RuitkSettings"/> —
    /// <c>Assets/Resources/ReactiveUIToolkit/config.json</c>, edited via the settings window,
    /// <i>Reactive UI Toolkit ▸ Settings</i>; ships into player builds via <c>Resources</c>) → the
    /// legacy <c>config.json</c> (<see cref="RuitkConfig"/>, kept for store customers with an
    /// edited file) → compiled defaults (which <see cref="RuitkConfig"/> returns when no file
    /// exists).
    /// </summary>
    public static class BuildDefinesConfig
    {
        /// <summary>
        /// The canonical tri-state mapping: <c>On</c>/<c>Off</c> are literal; <c>Auto</c> means ON
        /// in the editor and in development player builds, OFF in release players.
        /// </summary>
        public static bool MapTriState(RuitkTriState value)
        {
            switch (value)
            {
                case RuitkTriState.On:
                    return true;
                case RuitkTriState.Off:
                    return false;
                default:
                    return Application.isEditor || Debug.isDebugBuild;
            }
        }

        public static string ResolveEnvironment()
        {
            var settings = RuitkSettings.ActiveOrNull;
            if (settings != null)
            {
                return settings.ResolveEnvironmentLabel();
            }
            return RuitkConfig.Current.EnvironmentLabel ?? "production";
        }

        public static DiagnosticsConfig.TraceLevel ResolveTraceLevel()
        {
            var settings = RuitkSettings.ActiveOrNull;
            if (settings != null)
            {
                return settings.traceLevel;
            }
            return RuitkConfig.Current.TraceLevel;
        }

        public static bool ResolveEnableDiffTracing()
        {
            var settings = RuitkSettings.ActiveOrNull;
            if (settings != null)
            {
                return settings.diffTracing;
            }
            return RuitkConfig.Current.EnableDiffTracing;
        }

        // ── Reconciler knobs (0.13 additions) ─────────────────────────────────
        // The legacy config.json only ever carried env/traceLevel/diffTracing, so these
        // resolvers have no legacy hop: JSON store → compiled default (the §3 defaults,
        // which reproduce the pre-knob constants byte-for-byte).

        /// <summary>
        /// <c>time_slicing</c> — whether renders may be time-sliced through an installed
        /// scheduler; <c>false</c> forces the synchronous work loop (explicit scheduler
        /// bypass). Default <c>true</c>.
        /// </summary>
        public static bool ResolveTimeSlicing()
        {
            var settings = RuitkSettings.ActiveOrNull;
            return settings != null ? settings.timeSlicing : true;
        }

        /// <summary>
        /// <c>time_slice_ms</c> — the render-phase slice budget in milliseconds. Default
        /// <c>2.0</c> (the former <c>FiberReconciler</c> constant).
        /// </summary>
        public static float ResolveTimeSliceMs()
        {
            var settings = RuitkSettings.ActiveOrNull;
            return settings != null ? settings.timeSliceMs : 2.0f;
        }

        /// <summary>
        /// <c>frame_budget_ms</c> — the play-mode/player <c>RenderScheduler</c> per-frame
        /// queue budget in milliseconds. Default <c>4.0</c>. The editor scheduler is
        /// unbudgeted by design and does not read this.
        /// </summary>
        public static float ResolveFrameBudgetMs()
        {
            var settings = RuitkSettings.ActiveOrNull;
            return settings != null ? settings.frameBudgetMs : 4.0f;
        }

        /// <summary>
        /// <c>host_node_pool</c> — gates the uGUI host-element pool (adapter-gated
        /// <c>TryResetForPool</c> reuse); <c>false</c> destroys unmounted elements instead.
        /// Default <c>true</c>. The UI Toolkit path is never pooled.
        /// </summary>
        public static bool ResolveHostNodePool()
        {
            var settings = RuitkSettings.ActiveOrNull;
            return settings != null ? settings.hostNodePool : true;
        }
    }
}
