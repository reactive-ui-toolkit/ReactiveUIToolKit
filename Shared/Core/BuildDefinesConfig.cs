using Ruitk.Core.Config;
using Ruitk.Core.Diagnostics;

namespace Ruitk.Core
{
    /// <summary>
    /// The bootstrap-facing resolvers the root renderers call once at mount. Resolution order for
    /// every value: the active <see cref="RuitkSettings"/> asset (the settings window, <i>Reactive
    /// UI Toolkit ▸ Settings</i>; preloaded into player builds) → the legacy <c>config.json</c>
    /// (<see cref="RuitkConfig"/>, kept for store customers with an edited file) → compiled
    /// defaults (which <see cref="RuitkConfig"/> returns when no file exists).
    /// </summary>
    public static class BuildDefinesConfig
    {
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
    }
}
