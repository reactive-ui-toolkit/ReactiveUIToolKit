using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ruitk.SourceGenerator.Tools
{
    /// <summary>
    /// Pure text transform for the 0.12.0 rebrand (no filesystem access — see Program).
    ///
    /// Applies, in order, the exact REBRAND_PLAN §7.C rules:
    ///   F  — frozen-identity guard (marketplace IDs are masked so no later rule can touch them);
    ///   C1 — enumerated composite table (supervisor-ratified, 2026-07-28);
    ///   C1b— editor menu root, inside string literals (<c>"ReactiveUITK/</c> → <c>"Reactive UI Toolkit/</c>);
    ///   C2 — bare token via the normative regex <c>ReactiveUITK</c> with BOTH word boundaries;
    ///   C3 — the test-framework define;
    ///   D  — <c>ReactiveUIToolKit</c> install-path segment (capital-K → lowercase-k), any prefix.
    ///
    /// Deliberately dumb: ordered literal/regex replacements, no parsing. That is what
    /// makes it idempotent — after one pass no OLD token can remain, so a second pass
    /// finds nothing.
    ///
    /// <para><b>Rule F (frozen identities).</b> The 0.12.0 release froze every extension
    /// marketplace identity. Those strings embed the OLD brand token by design and must
    /// survive the codemod verbatim — rewriting <c>ReactiveUITK.uitkx</c> to
    /// <c>Ruitk.uitkx</c> in a user's file points them at an extension that does not
    /// exist. They are masked with a NUL-delimited sentinel before any rule runs and
    /// restored afterwards; masking is not counted as a replacement, so idempotency is
    /// unaffected.</para>
    ///
    /// <para><b>Rule C2 boundaries.</b> The right boundary was always normative; the LEFT
    /// boundary is required so text merely *ending* in the token — e.g. the frozen
    /// <c>UitkxVsix.ReactiveUITK</c> — is not rewritten. Dots are excluded on the left so
    /// a qualified tail never matches, while every real usage
    /// (<c>using ReactiveUITK.X</c>, <c>global::ReactiveUITK</c>, <c>"ReactiveUITK.Runtime"</c>)
    /// is preceded by whitespace, <c>:</c> or <c>"</c> and still matches.</para>
    ///
    /// <para><b>Rule D boundaries.</b> Matching the bare segment (not just an
    /// <c>Assets/</c>-prefixed literal) is what catches the package's own
    /// <c>Path.Combine(Application.dataPath, "ReactiveUIToolKit", …)</c> idiom, the
    /// <c>Packages/</c>-prefixed form, and both backslash spellings. Both boundaries are
    /// enforced so a user folder such as <c>ReactiveUIToolKitExtras</c> is left alone.</para>
    /// </summary>
    public static class BrandMigrator
    {
        /// <summary>
        /// Rule F — marketplace identities frozen by the 0.12.0 release. These must round-trip
        /// byte-identically. Ordered longest-first so a longer identity masks before a shorter
        /// one can claim part of it.
        /// </summary>
        private static readonly string[] s_frozenIdentities =
        {
            "UitkxVsix.ReactiveUITK",   // VS2022 <Identity Id>
            "itemName=ReactiveUITK",    // marketplace item URLs (VS Code + VS2022)
            "publishers/ReactiveUITK",  // publisher management URL
            "vsce login ReactiveUITK",  // publish runbook command
            "ReactiveUITK.uitkx",       // VS Code extension id (also the -visualstudio suffix form)
        };

        /// <summary>C1 — the enumerated composite table. Order matters: before the bare token.</summary>
        private static readonly (string Old, string New)[] s_composites =
        {
            ("ReactiveUITKConfig", "RuitkConfig"),
            ("__ReactiveUITK_MediaHost", "__Ruitk_MediaHost"),
            ("__ReactiveUITK_VideoPeer", "__Ruitk_VideoPeer"),
            ("__ReactiveUITK_AudioPeer", "__Ruitk_AudioPeer"),
            ("__ReactiveUITK_Sfx", "__Ruitk_Sfx"),
            ("__ReactiveUITK_RT_", "__Ruitk_RT_"),
        };

        /// <summary>
        /// C1b — editor menu root. Anchored on the opening quote so it only fires inside a
        /// string literal ([MenuItem("…")], ExecuteMenuItem("…")). MUST run before C2: '/'
        /// is not in [A-Za-z_], so the bare token would otherwise turn "ReactiveUITK/HMR Mode"
        /// into the plausible-looking but wrong "Ruitk/HMR Mode".
        /// </summary>
        private const string OldMenuRoot = "\"ReactiveUITK/";
        private const string NewMenuRoot = "\"Reactive UI Toolkit/";

        /// <summary>C2 — normative bare-token regex. Do not widen (contract §1.3).</summary>
        private static readonly Regex s_bareToken =
            new(@"(?<![A-Za-z0-9_.])ReactiveUITK(?![A-Za-z_])", RegexOptions.CultureInvariant);

        private const string OldDefine = "REACTIVEUITK_HAS_TEST_FRAMEWORK";
        private const string NewDefine = "RUITK_HAS_TEST_FRAMEWORK";

        /// <summary>
        /// D — install-folder casing. Boundaried bare segment, so it covers every spelling:
        /// Assets/…, Assets\…, Assets\\… (escaped C# literal), Packages/…, and the bare
        /// "ReactiveUIToolKit" Path.Combine segment.
        /// </summary>
        private static readonly Regex s_installPath =
            new(@"(?<![A-Za-z0-9_])ReactiveUIToolKit(?![A-Za-z0-9_])", RegexOptions.CultureInvariant);

        private const string NewInstallPath = "ReactiveUIToolkit";

        /// <summary>Sentinel wrapper for rule F. NUL cannot occur in the text files we scan.</summary>
        private const char SentinelMark = '\0';

        /// <summary>
        /// Rewrites one file's text. Returns the new text and the number of replacements
        /// (0 replacements always means text is reference-unchanged).
        /// </summary>
        public static string Migrate(string text, out int replacements)
        {
            int count = 0;

            // F — mask frozen marketplace identities so no rule below can touch them.
            List<string>? masked = null;
            for (int i = 0; i < s_frozenIdentities.Length; i++)
            {
                string frozen = s_frozenIdentities[i];
                if (text.IndexOf(frozen, StringComparison.Ordinal) < 0) continue;
                masked ??= new List<string>();
                string token = SentinelMark + "RUITKFROZEN" + masked.Count + SentinelMark;
                masked.Add(frozen);
                text = text.Replace(frozen, token, StringComparison.Ordinal);
            }

            foreach (var (oldToken, newToken) in s_composites)
                text = ReplaceCounting(text, oldToken, newToken, ref count);

            text = ReplaceCounting(text, OldMenuRoot, NewMenuRoot, ref count);

            text = s_bareToken.Replace(text, m => { count++; return "Ruitk"; });

            text = ReplaceCounting(text, OldDefine, NewDefine, ref count);

            text = s_installPath.Replace(text, m => { count++; return NewInstallPath; });

            // F — restore.
            if (masked != null)
            {
                for (int i = 0; i < masked.Count; i++)
                {
                    string token = SentinelMark + "RUITKFROZEN" + i + SentinelMark;
                    text = text.Replace(token, masked[i], StringComparison.Ordinal);
                }
            }

            replacements = count;
            return text;
        }

        private static string ReplaceCounting(string text, string oldToken, string newToken, ref int count)
        {
            int idx = 0;
            int found = 0;
            while ((idx = text.IndexOf(oldToken, idx, StringComparison.Ordinal)) >= 0)
            {
                found++;
                idx += oldToken.Length;
            }
            if (found == 0) return text;
            count += found;
            return text.Replace(oldToken, newToken, StringComparison.Ordinal);
        }
    }
}
