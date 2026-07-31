using Ruitk.Core.Diagnostics;
using UnityEngine;

namespace Ruitk.Core.Config
{
    /// <summary>
    /// Environment selector for <see cref="RuitkSettings.environment"/>. <c>Auto</c> resolves at
    /// read time: editor or development player builds report <c>development</c>, release player
    /// builds report <c>production</c>.
    /// </summary>
    public enum RuitkEnvironment
    {
        Auto,
        Development,
        Production,
    }

    /// <summary>
    /// The unified project-wide settings asset for Reactive UI Toolkit, edited via the settings
    /// window (<i>Reactive UI Toolkit ▸ Settings</i>). One asset per project (create it from that
    /// window); it feeds <see cref="Ruitk.Core.BuildDefinesConfig"/>, which falls back to the legacy
    /// <c>config.json</c> (<see cref="RuitkConfig"/>) and then to compiled defaults when no asset
    /// exists.
    ///
    /// <para>Player builds: the editor build hook adds the active asset to
    /// <c>PlayerSettings.preloadedAssets</c>, so <see cref="OnEnable"/> runs at player startup and
    /// self-registers the instance — the Input System settings pattern. No <c>UnityEditor</c>
    /// references may appear here; discovery inside the editor is the bootstrap's job
    /// (<c>Editor/RuitkSettingsBootstrap.cs</c>).</para>
    /// </summary>
    public sealed class RuitkSettings : ScriptableObject
    {
        [Tooltip(
            "Environment label exposed as HostContext.Environment[\"env\"]. "
            + "Auto: development in the editor and in development player builds, "
            + "production in release player builds."
        )]
        public RuitkEnvironment environment = RuitkEnvironment.Auto;

        [Tooltip("Reconciler trace level. None (silent), Basic, or Verbose (also enables internal logs).")]
        public DiagnosticsConfig.TraceLevel traceLevel = DiagnosticsConfig.TraceLevel.None;

        [Tooltip("Detailed Fiber diff/reconciliation tracing (DiagnosticsConfig.EnableDiffTracing).")]
        public bool diffTracing = false;

        [Tooltip("Route render exceptions through the exception-boundary control flow.")]
        public bool exceptionControlFlow = false;

        [Tooltip(
            "Where diagnostics output (benchmark results, log captures) is written. "
            + "Empty = the per-context default: <project>/Logs/ReactiveUIToolkit in the editor, "
            + "<persistentDataPath>/ReactiveUIToolkit in players. Absolute paths are used as-is; "
            + "relative paths resolve against the project root (editor) or persistentDataPath (player)."
        )]
        public string diagnosticsOutputFolder = "";

        private static RuitkSettings s_active;

        /// <summary>The active settings instance, or <c>null</c> when no asset is registered.</summary>
        public static RuitkSettings Instance => s_active;

        /// <summary>Alias of <see cref="Instance"/> that makes the null case explicit at call sites.</summary>
        public static RuitkSettings ActiveOrNull => s_active;

        /// <summary>
        /// Makes <paramref name="settings"/> the active instance (pass <c>null</c> to clear). Called
        /// by the editor bootstrap after asset discovery; player builds never need it because the
        /// preloaded asset registers itself via <see cref="OnEnable"/>.
        /// </summary>
        public static void SetActive(RuitkSettings settings)
        {
            s_active = settings;
        }

        /// <summary>
        /// The effective environment string fed to <c>HostContext.Environment["env"]</c>:
        /// <c>"development"</c> or <c>"production"</c>, with <see cref="RuitkEnvironment.Auto"/>
        /// resolved from the running context.
        /// </summary>
        public string ResolveEnvironmentLabel()
        {
            switch (environment)
            {
                case RuitkEnvironment.Development:
                    return "development";
                case RuitkEnvironment.Production:
                    return "production";
                default:
                    return Application.isEditor || Debug.isDebugBuild
                        ? "development"
                        : "production";
            }
        }

        private void OnEnable()
        {
            if (s_active == null)
            {
                s_active = this;
            }
        }

        private void OnDisable()
        {
            if (s_active == this)
            {
                s_active = null;
            }
        }
    }
}
