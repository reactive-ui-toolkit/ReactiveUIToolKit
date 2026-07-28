using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ruitk.SourceGenerator.Tools
{
    /// <summary>
    /// Pure text transform for the 0.12.0 rebrand (no filesystem access — see Program).
    ///
    /// Applies, in order, the exact REBRAND_PLAN §7.C rules:
    ///   C1 — enumerated composite table (supervisor-ratified, 2026-07-28);
    ///   C2 — bare token via the normative regex <c>ReactiveUITK(?![A-Za-z_])</c>;
    ///   C3 — the test-framework define;
    ///   D  — <c>Assets/ReactiveUIToolKit</c> install-path strings (both slash forms).
    ///
    /// Deliberately dumb: ordered literal/regex replacements, no parsing. That is what
    /// makes it idempotent — after one pass no OLD token can remain, so a second pass
    /// finds nothing.
    /// </summary>
    public static class BrandMigrator
    {
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

        /// <summary>C2 — normative bare-token regex. Do not widen (contract §1.3).</summary>
        private static readonly Regex s_bareToken =
            new(@"ReactiveUITK(?![A-Za-z_])", RegexOptions.CultureInvariant);

        private const string OldDefine = "REACTIVEUITK_HAS_TEST_FRAMEWORK";
        private const string NewDefine = "RUITK_HAS_TEST_FRAMEWORK";

        private static readonly (string Old, string New)[] s_paths =
        {
            ("Assets/ReactiveUIToolKit", "Assets/ReactiveUIToolkit"),
            (@"Assets\\ReactiveUIToolKit", @"Assets\\ReactiveUIToolkit"), // escaped form in C# string literals
            (@"Assets\ReactiveUIToolKit", @"Assets\ReactiveUIToolkit"),
        };

        /// <summary>
        /// Rewrites one file's text. Returns the new text and the number of replacements
        /// (0 replacements always means text is reference-unchanged).
        /// </summary>
        public static string Migrate(string text, out int replacements)
        {
            int count = 0;

            foreach (var (oldToken, newToken) in s_composites)
                text = ReplaceCounting(text, oldToken, newToken, ref count);

            text = s_bareToken.Replace(text, m => { count++; return "Ruitk"; });

            text = ReplaceCounting(text, OldDefine, NewDefine, ref count);

            foreach (var (oldPath, newPath) in s_paths)
                text = ReplaceCounting(text, oldPath, newPath, ref count);

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
