using System;
using Ruitk.Core.Config;
using UnityEditor;
using UnityEngine;

namespace Ruitk.EditorSupport
{
    /// <summary>
    /// Editor-side discovery for the <see cref="RuitkSettings"/> asset. The asset class itself is
    /// runtime code (<c>Ruitk.Shared</c>) and must not touch <c>AssetDatabase</c>, so locating the
    /// asset and assigning <see cref="RuitkSettings.SetActive"/> lives here: once on editor load,
    /// and again whenever an asset import/delete/move could have changed the answer (the
    /// <see cref="AssetPostprocessor"/> below).
    /// </summary>
    [InitializeOnLoad]
    public static class RuitkSettingsBootstrap
    {
        /// <summary>
        /// Where "Create settings asset" puts the asset. Users may move it afterwards — discovery
        /// is by type, not by path.
        /// </summary>
        public const string DefaultAssetPath = "Assets/ReactiveUIToolkitSettings.asset";

        static RuitkSettingsBootstrap()
        {
            Resolve();
        }

        /// <summary>
        /// Finds the settings asset (<c>t:RuitkSettings</c>) and makes it the active instance;
        /// clears the active instance when none exists. First found (path order) wins; a warning
        /// lists every path when the project holds more than one. Returns the active instance or
        /// <c>null</c>.
        /// </summary>
        public static RuitkSettings Resolve()
        {
            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets("t:RuitkSettings");
            }
            catch (Exception)
            {
                return RuitkSettings.ActiveOrNull;
            }

            if (guids == null || guids.Length == 0)
            {
                RuitkSettings.SetActive(null);
                return null;
            }

            var paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }
            Array.Sort(paths, StringComparer.Ordinal);

            RuitkSettings chosen = null;
            foreach (string path in paths)
            {
                chosen = AssetDatabase.LoadAssetAtPath<RuitkSettings>(path);
                if (chosen != null)
                {
                    break;
                }
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning(
                    "Reactive UI Toolkit: multiple RuitkSettings assets found; using the first "
                    + "(path order). Delete the extras — only one is ever read:\n  "
                    + string.Join("\n  ", paths)
                );
            }

            RuitkSettings.SetActive(chosen);
            return chosen;
        }

        /// <summary>
        /// The "Create settings asset" code path of the settings window. Creates the asset at
        /// <see cref="DefaultAssetPath"/> and makes it active; returns the existing asset instead
        /// when one is already in the project (nothing is ever created or written until the user —
        /// or an explicit caller — asks; see <see cref="RuitkSettingsWindow"/> for the rationale).
        /// </summary>
        public static RuitkSettings CreateSettingsAsset()
        {
            var existing = Resolve();
            if (existing != null)
            {
                return existing;
            }

            var settings = ScriptableObject.CreateInstance<RuitkSettings>();
            AssetDatabase.CreateAsset(settings, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            RuitkSettings.SetActive(settings);
            return settings;
        }
    }

    /// <summary>
    /// Re-resolves the active settings asset when one is imported, deleted, or moved. Filtered to
    /// <c>.asset</c> paths so ordinary script/texture imports never pay for a FindAssets sweep.
    /// </summary>
    internal sealed class RuitkSettingsAssetWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            if (
                AnyAssetFile(importedAssets)
                || AnyAssetFile(deletedAssets)
                || AnyAssetFile(movedAssets)
            )
            {
                RuitkSettingsBootstrap.Resolve();
            }
        }

        private static bool AnyAssetFile(string[] paths)
        {
            if (paths == null)
            {
                return false;
            }
            foreach (string path in paths)
            {
                if (path != null && path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
