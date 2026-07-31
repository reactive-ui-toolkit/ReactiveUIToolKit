using System;
using System.IO;

namespace Ruitk.SourceGenerator.Tools
{
    /// <summary>
    /// Which files the rebrand codemod scans, and which folders it refuses to touch.
    /// Split out of <c>Program</c> so the rules are unit-testable (audit finding M9).
    /// </summary>
    public static class ScanRules
    {
        /// <summary>
        /// File kinds that can carry the old brand token.
        ///
        /// <para><c>.asmref</c> names an assembly (<c>ReactiveUITK.Runtime</c>) and <c>.rsp</c>
        /// carries <c>-define:REACTIVEUITK_HAS_TEST_FRAMEWORK</c>; both were missed by the
        /// original scan (audit finding M4), and a stale define fails SILENTLY — the conditional
        /// block just stops compiling in, with no diagnostic.</para>
        ///
        /// <para>Deliberately NOT scanned: <c>.json</c>. A user's <c>.vscode/settings.json</c>
        /// holds the frozen <c>ReactiveUITK.uitkx</c> formatter id, and Project Settings scripting
        /// defines are covered by a documented manual step in MIGRATION-0.12.md instead.</para>
        /// </summary>
        public static readonly string[] Extensions =
        {
            ".cs", ".uitkx", ".asmdef", ".asmref", ".rsp",
        };

        public static bool IsScannedExtension(string path)
        {
            string ext = Path.GetExtension(path);
            foreach (string e in Extensions)
                if (string.Equals(ext, e, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        /// <summary>
        /// True when a path must not be rewritten.
        /// </summary>
        /// <param name="relativePath">Path RELATIVE to the scan root — segments above the root
        /// (e.g. a parent folder that happens to be named Temp) must not trigger skips.</param>
        public static bool IsSkipped(string relativePath)
        {
            foreach (string part in relativePath.Split(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                // Never edit inside the installed package folder itself (either casing era, plus
                // the embedded-UPM folder name — an embedded install at
                // Packages/com.reactiveuitoolkit/ was not skipped before, audit finding L9).
                if (IsPackageFolderSegment(part)) return true;

                // Tool-generated folders: the recompile trigger + the asset registry live here.
                //
                // The 0.11.x folder is skipped so the stale trigger file KEEPS its old
                // ReactiveUITK.Generated namespace. Rewriting it to Ruitk.Generated would make it
                // declare the SAME type as the new trigger under Assets/Ruitk — two identical
                // types in Assembly-CSharp, i.e. CS0101 + CS0102 and a default assembly that no
                // longer compiles, pointing at a hidden auto-generated file (audit finding H1).
                // Deleting Assets/ReactiveUITK is a documented post-migration step.
                if (part.Equals("ReactiveUITK", StringComparison.Ordinal)) return true;
                if (part.Equals("Ruitk", StringComparison.Ordinal)) return true;

                if (part.EndsWith("~", StringComparison.Ordinal)) return true;
                if (part is ".git" or "Library" or "Temp" or "obj" or "bin") return true;
            }
            return false;
        }

        /// <summary>The installed-package folder, in every name it can have.</summary>
        public static bool IsPackageFolderSegment(string part) =>
            part.Equals("ReactiveUIToolKit", StringComparison.Ordinal)
            || part.Equals("ReactiveUIToolkit", StringComparison.Ordinal)
            || part.Equals("com.reactiveuitoolkit", StringComparison.OrdinalIgnoreCase);
    }
}
