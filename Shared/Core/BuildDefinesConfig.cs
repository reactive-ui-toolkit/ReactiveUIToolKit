using Ruitk.Core.Config;
using Ruitk.Core.Diagnostics;

namespace Ruitk.Core
{
    public static class BuildDefinesConfig
    {
        public static string ResolveEnvironment()
        {
            return RuitkConfig.Current.EnvironmentLabel ?? "production";
        }

        public static DiagnosticsConfig.TraceLevel ResolveTraceLevel()
        {
            return RuitkConfig.Current.TraceLevel;
        }

        public static bool ResolveEnableDiffTracing()
        {
            return RuitkConfig.Current.EnableDiffTracing;
        }

        public static bool ResolveExceptionBoundaryFlow()
        {
            return RuitkConfig.Current.UseExceptionBoundaryFlow;
        }
    }
}
