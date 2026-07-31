#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ruitk.EditorSupport
{
    /// <summary>
    /// The single layout-agnostic resolver for the Reactive UI Toolkit package root.
    ///
    /// The package ships through four channels and its root is a DIFFERENT absolute path in each,
    /// so no tool may assume it lives under <c>Assets/</c>:
    ///
    /// <list type="bullet">
    /// <item>(a) <c>&lt;project&gt;/Assets/ReactiveUIToolkit</c> — Asset Store <c>.unitypackage</c>, and today's dev layout.</item>
    /// <item>(b) <c>&lt;project&gt;/Library/PackageCache/com.reactiveuitoolkit@&lt;hash&gt;</c> — the UPM git-URL channel, which the docs call the PRIMARY install.</item>
    /// <item>(c) <c>&lt;project&gt;/Packages/com.reactiveuitoolkit</c> — embedded package.</item>
    /// <item>(d) an arbitrary absolute path outside the project — a local <c>file:</c> package reference.</item>
    /// </list>
    ///
    /// Resolution order:
    /// <list type="number">
    /// <item><see cref="UnityEditor.PackageManager.PackageInfo.FindForAssembly"/> on THIS assembly.
    ///   Authoritative for (b), (c) and (d); returns <c>null</c> for (a), where our code is not in a package.</item>
    /// <item>Asset-database sentinel discovery — locate one of our own scripts, then walk UP until a
    ///   directory looks like our package root. Covers (a). Folder-NAME-agnostic, so it also survives
    ///   the install folder being renamed, and depth-agnostic, so moving the sentinel cannot break it.</item>
    /// <item>Legacy last resort: <c>Application.dataPath/ReactiveUIToolkit</c>. Kept so a tree with a
    ///   broken asset database still works — but ONLY as the final rung, never as the sole mechanism.</item>
    /// <item>Nothing matched: <see cref="FailureMessage"/> names every rung tried, with concrete paths.</item>
    /// </list>
    ///
    /// <para><b>EDITOR-ONLY BY CONSTRUCTION.</b> Rung 1 is <c>UnityEditor.PackageManager</c> and rung 2 is
    /// <c>AssetDatabase</c>. This type lives in the Editor-only <c>Ruitk.Editor</c> assembly and MUST NOT be
    /// referenced from a runtime assembly (<c>Ruitk.Shared</c>, <c>Ruitk.Runtime</c>, <c>Ruitk.Diagnostics</c>
    /// all compile into player builds, where neither API exists).</para>
    ///
    /// <para>The result is cached — the layout cannot change within a domain reload. Main thread only
    /// (<c>AssetDatabase</c> and <c>Application.dataPath</c> both require it), which is where every
    /// consumer already runs.</para>
    ///
    /// <para>Writing under the returned root is NOT safe in general: in layout (b) the package folder is
    /// read-only. Use this to READ package content; put generated output in <c>Temp/</c>,
    /// <c>Library/</c>, <c>Path.GetTempPath()</c>, or the user's own <c>Assets/</c>.</para>
    /// </summary>
    public static class RuitkPackagePaths
    {
        /// <summary>
        /// The published install folder under <c>Assets/</c>. This literal is a PRODUCT contract, not a
        /// machine path (see the <c>machine-local-paths</c> skill) — but it is only ever rung 3.
        /// </summary>
        private const string LegacyInstallFolder = "ReactiveUIToolkit";

        /// <summary>
        /// Scripts of ours used as rung-2 anchors, best first. These are ASSET NAMES (the file stem);
        /// renaming one of these files means updating this array. <c>UitkxHmrCompiler</c> is kept as a
        /// second anchor because it was the original probe and gives us redundancy for free.
        /// </summary>
        private static readonly string[] SentinelScripts =
        {
            nameof(RuitkPackagePaths),
            "UitkxHmrCompiler",
        };

        /// <summary>
        /// Subdirectories that together identify our package root in every layout. <c>Editor/</c> and
        /// <c>Shared/</c> are asmdef-bearing top-level folders that CI guard-rails already require to
        /// ship (see <c>AssetStoreExport.RequirePrefix</c>), so this test is stable — and it never looks
        /// at the root folder's NAME, which is what makes a renamed install resolve.
        /// </summary>
        private static readonly string[] RootMarkerDirectories = { "Editor", "Shared" };

        /// <summary>How far above a sentinel script the walk-up looks before giving up.</summary>
        private const int MaxWalkUp = 8;

        private static bool _resolved;
        private static string _root;
        private static string _failure;

        /// <summary>
        /// The absolute package root (forward-slashed, no trailing separator), or <c>null</c> when it
        /// cannot be resolved. Prefer <see cref="TryGetRoot"/>, which also hands you the diagnostic.
        /// </summary>
        public static string Root
        {
            get
            {
                Resolve();
                return _root;
            }
        }

        /// <summary>
        /// True and the absolute package root, or false and <c>null</c>. Never throws — the shape to use
        /// where a miss should degrade or log rather than abort. On false, log <see cref="FailureMessage"/>.
        /// </summary>
        public static bool TryGetRoot(out string root)
        {
            Resolve();
            root = _root;
            return root != null;
        }

        /// <summary>
        /// The rung-by-rung diagnostic for a failed resolution, or <c>null</c> while resolution succeeds.
        /// </summary>
        public static string FailureMessage
        {
            get
            {
                Resolve();
                return _failure;
            }
        }

        /// <summary>
        /// The absolute package root, or a <see cref="DirectoryNotFoundException"/> carrying
        /// <see cref="FailureMessage"/>. For callers that genuinely cannot continue without it.
        /// </summary>
        public static string GetRootOrThrow()
        {
            Resolve();
            if (_root != null)
            {
                return _root;
            }
            throw new DirectoryNotFoundException(_failure);
        }

        /// <summary>
        /// <see cref="GetRootOrThrow"/> combined with <paramref name="parts"/>. Throws when the root
        /// cannot be resolved; does NOT require the result to exist.
        /// </summary>
        public static string Combine(params string[] parts)
        {
            string acc = GetRootOrThrow();
            if (parts == null)
            {
                return acc;
            }
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    acc = Path.Combine(acc, part);
                }
            }
            return Normalize(acc);
        }

        /// <summary>
        /// Forgets the cached result so the next call re-probes. Only useful to tests and to a tool that
        /// has just repaired the asset database; the layout itself cannot change within a domain reload.
        /// </summary>
        public static void InvalidateCache()
        {
            _resolved = false;
            _root = null;
            _failure = null;
        }

        // ── Resolution ────────────────────────────────────────────────────────────

        private static void Resolve()
        {
            if (_resolved)
            {
                return;
            }
            _resolved = true;

            var probes = new List<string>();

            // ── Rung 1: the package manager — layouts (b), (c), (d) ───────────────
            try
            {
                var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(RuitkPackagePaths).Assembly
                );
                string resolvedPath = info?.resolvedPath;
                if (!string.IsNullOrEmpty(resolvedPath) && Directory.Exists(resolvedPath))
                {
                    _root = Normalize(resolvedPath);
                    return;
                }
                probes.Add(
                    "  1. PackageManager.PackageInfo.FindForAssembly(Ruitk.Editor) → "
                    + (info == null
                        ? "null — not installed as a package (expected for an Assets/ install)"
                        : "'" + resolvedPath + "' — does not exist on disk")
                );
            }
            catch (Exception ex)
            {
                probes.Add(
                    "  1. PackageManager.PackageInfo.FindForAssembly(Ruitk.Editor) → threw "
                    + ex.GetType().Name + ": " + ex.Message
                );
            }

            // ── Rung 2: asset-database sentinel discovery — layout (a) ────────────
            foreach (string sentinel in SentinelScripts)
            {
                string found = TryResolveFromSentinel(sentinel, probes);
                if (found != null)
                {
                    _root = found;
                    return;
                }
            }

            // ── Rung 3: the legacy published install path ─────────────────────────
            string legacy;
            try
            {
                legacy = Normalize(Path.Combine(Application.dataPath, LegacyInstallFolder));
                if (Directory.Exists(legacy))
                {
                    _root = legacy;
                    return;
                }
                probes.Add(
                    "  3. legacy Application.dataPath/" + LegacyInstallFolder + " → '" + legacy
                    + "' — does not exist"
                );
            }
            catch (Exception ex)
            {
                probes.Add(
                    "  3. legacy Application.dataPath/" + LegacyInstallFolder + " → threw "
                    + ex.GetType().Name + ": " + ex.Message
                );
            }

            // ── Rung 4: one clear error naming every rung ─────────────────────────
            _root = null;
            _failure =
                "Reactive UI Toolkit: cannot locate the package root. Tried, in order:\n"
                + string.Join("\n", probes.ToArray())
                + "\nThe package root is expected at Assets/" + LegacyInstallFolder
                + ", Packages/com.reactiveuitoolkit, Library/PackageCache/com.reactiveuitoolkit@<hash>,"
                + " or a 'file:' package path. If the install is intact, a stale asset database can"
                + " also cause this — try Assets ▸ Reimport All.";
        }

        /// <summary>
        /// Rung 2 for one sentinel: find the script asset by name, then walk UP from it until an
        /// ancestor satisfies <see cref="LooksLikePackageRoot"/>.
        /// </summary>
        private static string TryResolveFromSentinel(string sentinelName, List<string> probes)
        {
            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets(sentinelName + " t:MonoScript");
            }
            catch (Exception ex)
            {
                probes.Add(
                    "  2. AssetDatabase sentinel '" + sentinelName + "' → threw "
                    + ex.GetType().Name + ": " + ex.Message
                );
                return null;
            }

            if (guids == null || guids.Length == 0)
            {
                probes.Add(
                    "  2. AssetDatabase sentinel '" + sentinelName
                    + "' → no MonoScript by that name in the asset database"
                );
                return null;
            }

            string rejected = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                // FindAssets' name filter is a FUZZY match, so require an exact file-stem hit — a
                // future "RuitkPackagePathsTests.cs" must not be able to anchor the walk.
                if (!string.Equals(
                        Path.GetFileNameWithoutExtension(assetPath),
                        sentinelName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                string absolute = ToAbsolutePath(assetPath);
                if (absolute == null)
                {
                    continue;
                }

                string dir = Path.GetDirectoryName(absolute);
                for (int i = 0; i < MaxWalkUp && !string.IsNullOrEmpty(dir); i++)
                {
                    if (LooksLikePackageRoot(dir))
                    {
                        return Normalize(dir);
                    }
                    dir = Path.GetDirectoryName(dir);
                }
                rejected = absolute;
            }

            probes.Add(
                "  2. AssetDatabase sentinel '" + sentinelName + "' → "
                + (rejected == null
                    ? "no exact filename match among " + guids.Length + " fuzzy candidate(s)"
                    : "anchored at '" + rejected + "' but no ancestor within " + MaxWalkUp
                      + " level(s) contained "
                      + string.Join("/ + ", RootMarkerDirectories) + "/")
            );
            return null;
        }

        /// <summary>
        /// A directory is our package root when it holds every marker in
        /// <see cref="RootMarkerDirectories"/>. Deliberately blind to the directory's own NAME.
        /// </summary>
        private static bool LooksLikePackageRoot(string dir)
        {
            try
            {
                foreach (string marker in RootMarkerDirectories)
                {
                    if (!Directory.Exists(Path.Combine(dir, marker)))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Project-relative asset path (<c>Assets/…</c>, <c>Packages/…</c>) → absolute filesystem path.
        /// Correct for <c>Assets/…</c> (layout a) and for an embedded <c>Packages/…</c> (layout c). A
        /// PackageCache-backed <c>Packages/…</c> (layout b) has no such physical path, so the
        /// <c>File.Exists</c> guard rejects it — rung 1 already owns that layout.
        /// </summary>
        private static string ToAbsolutePath(string assetPath)
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
                if (string.IsNullOrEmpty(projectRoot))
                {
                    return null;
                }
                string absolute = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
                return File.Exists(absolute) ? absolute : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Absolute, forward-slashed, no trailing separator — so callers can string-compare.</summary>
        private static string Normalize(string path)
        {
            string full = Path.GetFullPath(path).Replace('\\', '/');
            return full.Length > 1 && full.EndsWith("/", StringComparison.Ordinal)
                ? full.TrimEnd('/')
                : full;
        }
    }
}
#endif
