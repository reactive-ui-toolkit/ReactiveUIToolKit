using System.Collections.Generic;
using Ruitk.Core.Config;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ruitk.EditorSupport
{
    /// <summary>
    /// Injects the active <see cref="RuitkSettings"/> asset into
    /// <c>PlayerSettings.preloadedAssets</c> for the duration of a player build, so the asset's
    /// <c>OnEnable</c> self-registers it at player startup — mirroring the Input System's
    /// <c>InputSettingsBuildProvider</c>. Preprocess adds (deduped), postprocess removes exactly
    /// what preprocess added, so a user's own preloaded-assets list round-trips untouched.
    ///
    /// <para>Both passes also strip null entries first (the ISXB-296 lesson: deleted assets leave
    /// <c>{fileID: 0}</c> junk in ProjectSettings.asset that otherwise accumulates forever). That
    /// strip doubles as the crash guard — if a build dies between pre and post and our entry goes
    /// stale later, the next build's preprocess sweeps it out instead of it dirtying
    /// ProjectSettings permanently.</para>
    /// </summary>
    internal sealed class RuitkSettingsBuildInjection
        : IPreprocessBuildWithReport,
            IPostprocessBuildWithReport
    {
        // Distinct from UitkxHmrBuildProcessor (0) so relative order is explicit; the two hooks
        // are independent, this one just runs first.
        public int callbackOrder => -100;

        private RuitkSettings _addedAsset;

        public void OnPreprocessBuild(BuildReport report)
        {
            _addedAsset = null;
            var settings = RuitkSettingsBootstrap.Resolve();

            var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());
            bool changed = preloaded.RemoveAll(asset => asset == null) > 0;

            if (settings != null && !preloaded.Contains(settings))
            {
                preloaded.Add(settings);
                _addedAsset = settings;
                changed = true;
            }

            if (changed)
            {
                PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            var added = _addedAsset;
            _addedAsset = null;

            var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());
            bool changed = preloaded.RemoveAll(asset => asset == null) > 0;

            if (added != null)
            {
                changed |= preloaded.Remove(added);
            }

            if (changed)
            {
                PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
            }
        }
    }
}
