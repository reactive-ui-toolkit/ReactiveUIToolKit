using System.IO;
using UnityEngine;

namespace Ruitk.Core.Config
{
    /// <summary>
    /// The single resolver for where diagnostics output (benchmark results, log captures) is
    /// written. Diagnostics used to write into the package folder itself
    /// (<c>Assets/ReactiveUIToolkit/Diagnostics/…</c>), which is wrong in every layout but the
    /// legacy Assets/ install: a UPM PackageCache folder is read-only and wiped on upgrade, and
    /// writing into shipped package content dirties the user's project either way. Output now goes
    /// OUTSIDE the package:
    ///
    /// <list type="bullet">
    /// <item>override — <see cref="RuitkSettings.diagnosticsOutputFolder"/> when set: absolute
    ///   paths as-is; relative paths resolve against the project root (editor) or
    ///   <c>Application.persistentDataPath</c> (player);</item>
    /// <item>editor default — <c>&lt;project&gt;/Logs/ReactiveUIToolkit</c> (Unity's own Logs/
    ///   folder, already gitignored in typical projects);</item>
    /// <item>player default — <c>&lt;persistentDataPath&gt;/ReactiveUIToolkit</c>.</item>
    /// </list>
    ///
    /// Runtime-safe by construction (no <c>UnityEditor</c>); main thread only
    /// (<c>Application.dataPath</c>/<c>persistentDataPath</c> require it).
    /// </summary>
    public static class RuitkDiagnosticsPaths
    {
        /// <summary>
        /// The root folder for all diagnostics output. Not created here — callers
        /// <c>Directory.CreateDirectory</c> the subfolder they actually write.
        /// </summary>
        public static string GetOutputRoot()
        {
            var settings = RuitkSettings.ActiveOrNull;
            string configured = settings != null ? settings.diagnosticsOutputFolder : null;
            if (!string.IsNullOrEmpty(configured))
            {
                configured = configured.Trim();
            }

            if (!string.IsNullOrEmpty(configured))
            {
                if (Path.IsPathRooted(configured))
                {
                    return Normalize(Path.GetFullPath(configured));
                }
                string baseFolder = Application.isEditor
                    ? GetProjectRoot()
                    : Application.persistentDataPath;
                return Normalize(Path.GetFullPath(Path.Combine(baseFolder, configured)));
            }

            if (Application.isEditor)
            {
                return Normalize(Path.Combine(GetProjectRoot(), "Logs", "ReactiveUIToolkit"));
            }
            return Normalize(Path.Combine(Application.persistentDataPath, "ReactiveUIToolkit"));
        }

        private static string GetProjectRoot()
        {
            return Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
        }

        private static string Normalize(string path)
        {
            string full = path.Replace('\\', '/');
            return full.Length > 1 && full.EndsWith("/") ? full.TrimEnd('/') : full;
        }
    }
}
