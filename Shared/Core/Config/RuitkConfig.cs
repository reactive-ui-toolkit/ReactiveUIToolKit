using System;
using System.IO;
using Ruitk.Core.Diagnostics;
using UnityEngine;

namespace Ruitk.Core.Config
{
    /// <summary>
    /// LEGACY fallback, superseded by <see cref="RuitkSettings"/> (the settings window, <i>Reactive
    /// UI Toolkit ▸ Settings</i>). <see cref="Ruitk.Core.BuildDefinesConfig"/> only consults this reader when no
    /// settings asset is active, so store customers who edited their shipped
    /// <c>Assets/ReactiveUIToolkit/config.json</c> keep their values across the upgrade. Do not
    /// add new settings here — add them to <see cref="RuitkSettings"/>; this type stays only for
    /// back-compatibility and still parses the old <c>envVariables</c> shape if a user's file
    /// carries one (the shipped <c>config.json</c> no longer does).
    /// </summary>
    public sealed class RuitkConfig
    {
        [Serializable]
        private sealed class EnvVariables
        {
            public string env;
            public string traceLevel;
            public bool diffTracing;
        }

        [Serializable]
        private sealed class Root
        {
            public EnvVariables envVariables;
        }

        public string EnvironmentLabel { get; private set; } = "production";
        public DiagnosticsConfig.TraceLevel TraceLevel { get; private set; } =
            DiagnosticsConfig.TraceLevel.None;
        public bool EnableDiffTracing { get; private set; } = false;

        private static RuitkConfig instance;
        public static RuitkConfig Current
        {
            get
            {
                if (instance == null)
                {
                    instance = Load();
                }
                return instance;
            }
        }

        private static RuitkConfig Load()
        {
            var cfg = new RuitkConfig();
            try
            {
                string candidate = GetDefaultProjectConfigPath();
                if (File.Exists(candidate))
                {
                    string json = File.ReadAllText(candidate);
                    var parsed = JsonUtility.FromJson<Root>(json);
                    if (parsed != null && parsed.envVariables != null)
                    {
                        if (!string.IsNullOrEmpty(parsed.envVariables.env))
                        {
                            cfg.EnvironmentLabel = parsed.envVariables.env;
                        }
                        if (!string.IsNullOrEmpty(parsed.envVariables.traceLevel))
                        {
                            cfg.TraceLevel = ParseTraceLevel(parsed.envVariables.traceLevel);
                        }
                        cfg.EnableDiffTracing = parsed.envVariables.diffTracing;
                    }
                }
            }
            catch { }
            return cfg;
        }

        /// <summary>
        /// The user-editable <c>config.json</c>, looked for at <c>&lt;Assets&gt;/ReactiveUIToolkit/config.json</c>.
        ///
        /// <para><b>Deliberately NOT routed through <c>RuitkPackagePaths</c>, and it must not be.</b> This
        /// type compiles into <c>Ruitk.Shared</c>, which has no <c>includePlatforms</c> — it is a RUNTIME
        /// assembly that ships into player builds. <c>RuitkPackagePaths</c> resolves via
        /// <c>UnityEditor.PackageManager.PackageInfo</c> and <c>AssetDatabase</c>, neither of which exists
        /// outside the editor, so referencing it from here would break every player build. Do not "fix"
        /// this by adding the resolver; see the note below for the actual fix.</para>
        ///
        /// <para><b>DESIGN QUESTION RESOLVED (unified settings).</b> Reading a *user-editable* file
        /// from inside the package was only ever coherent in the <c>Assets/</c> layout — a UPM
        /// PackageCache install is read-only AND re-created under a new hash on every upgrade, so
        /// for such users this path simply does not exist, <c>File.Exists</c> is false, and every
        /// setting keeps its compiled-in default. The fix chosen is not the ProjectSettings-JSON
        /// variant once suggested here but a project-owned <see cref="RuitkSettings"/>
        /// ScriptableObject asset (created from the settings window, <i>Reactive UI Toolkit ▸
        /// Settings</i>, preloaded into player builds), which is writable and upgrade-stable in
        /// all four layouts. This
        /// in-package path stays honoured as the legacy fallback only.</para>
        /// </summary>
        private static string GetDefaultProjectConfigPath()
        {
            string assets = Application.dataPath;
            return Path.Combine(assets, "ReactiveUIToolkit", "config.json");
        }

        private static DiagnosticsConfig.TraceLevel ParseTraceLevel(string value)
        {
            switch (value)
            {
                case "Verbose":
                case "verbose":
                    return DiagnosticsConfig.TraceLevel.Verbose;
                case "Basic":
                case "basic":
                    return DiagnosticsConfig.TraceLevel.Basic;
                case "None":
                case "none":
                default:
                    return DiagnosticsConfig.TraceLevel.None;
            }
        }
    }
}
