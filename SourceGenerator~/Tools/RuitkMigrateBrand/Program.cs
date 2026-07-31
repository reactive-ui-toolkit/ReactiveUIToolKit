using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ruitk.SourceGenerator.Tools
{
    /// <summary>
    /// CLI wrapper around <see cref="BrandMigrator"/> — the only layer that touches the filesystem.
    ///
    /// <code>
    ///   dotnet run --project SourceGenerator~/Tools/RuitkMigrateBrand -- &lt;dir&gt; [--check]
    /// </code>
    ///
    /// Walks <c>&lt;dir&gt;</c> for user sources (see <see cref="ScanRules.Extensions"/>), applies
    /// the 0.12.0 rebrand rules, and writes changed files back with per-file replacement counts.
    /// <c>--check</c> makes it a dry run that exits non-zero if anything would change — the
    /// idempotence gate (run once for real, then <c>--check</c> must be clean).
    ///
    /// <para><b>Skipping</b> is <see cref="ScanRules.IsSkipped"/>. <b>Encoding round-trip</b> is
    /// <see cref="FileEncodings"/>: a BOM is preserved, and a file that is not valid UTF-8 is
    /// skipped with a warning instead of being corrupted (audit finding M3).</para>
    ///
    /// <para><b>Failure handling</b> (audit finding H5). Every file is read, migrated and written
    /// inside its own try/catch. One unwritable file — Perforce and Plastic Cloud keep unopened
    /// files read-only, and both are mainstream in Unity teams — no longer aborts the run with a
    /// stack trace part-way through. The error is recorded, the remaining files are still written,
    /// and the tool prints every failure PLUS the exact list of files it did write, so a partial
    /// run is recoverable rather than an untracked mixed state.</para>
    ///
    /// <para><b>Exit codes.</b> 0 = clean; 1 = <c>--check</c> found pending changes;
    /// 2 = usage error; 3 = one or more files could not be read or written.</para>
    /// </summary>
    public static class Program
    {
        private const int ExitOk = 0;
        private const int ExitCheckDirty = 1;
        private const int ExitUsage = 2;
        private const int ExitFileErrors = 3;

        public static int Main(string[] args)
        {
            // ── L8: tolerant arg parsing. Flags may appear in any position, so the natural
            //    "--check Assets" ordering works as well as "Assets --check"; --help is explicit.
            string? root = null;
            bool check = false;
            foreach (string a in args)
            {
                switch (a)
                {
                    case "--help":
                    case "-h":
                    case "-?":
                    case "/?":
                        PrintUsage(Console.Out);
                        return ExitOk;
                    case "--check":
                    case "-c":
                        check = true;
                        continue;
                }

                if (a.Length > 1 && a[0] == '-')
                {
                    Console.Error.WriteLine($"error: unknown option: {a}");
                    PrintUsage(Console.Error);
                    return ExitUsage;
                }

                if (root != null)
                {
                    Console.Error.WriteLine($"error: more than one directory given ('{root}' and '{a}')");
                    PrintUsage(Console.Error);
                    return ExitUsage;
                }

                root = a;
            }

            if (root == null)
            {
                Console.Error.WriteLine("error: no directory given");
                PrintUsage(Console.Error);
                return ExitUsage;
            }

            root = Path.GetFullPath(root);
            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine($"error: directory not found: {root}");
                return ExitUsage;
            }

            // ── L9: the documented invariant is "never edits inside the package folder", but
            //    IsSkipped tests the path RELATIVE to the scan root, so pointing the tool AT the
            //    package folder bypasses it. Warn loudly rather than silently skipping everything
            //    (a user's project directory may legitimately carry this name).
            if (ScanRules.IsPackageFolderSegment(new DirectoryInfo(root).Name))
            {
                Console.Error.WriteLine(
                    $"warning: the scan root '{root}' is itself an installed-package folder. "
                        + "Point this tool at YOUR project's Assets folder — running it here "
                        + "rewrites the package's own sources.");
            }

            int scanned = 0;
            var changed = new List<(string Path, string Text, FileEncodings.BomKind Bom, int Count)>();
            var errors = new List<(string Path, string Message)>();
            var skippedEncoding = new List<string>();

            foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (!ScanRules.IsScannedExtension(path)) continue;
                if (ScanRules.IsSkipped(Path.GetRelativePath(root, path))) continue;
                scanned++;

                string text;
                FileEncodings.BomKind bom;
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    if (!FileEncodings.TryDecode(bytes, out string? decoded, out bom))
                    {
                        skippedEncoding.Add(Path.GetFullPath(path));
                        continue;
                    }
                    text = decoded!;
                }
                catch (Exception ex)
                {
                    errors.Add((Path.GetFullPath(path), $"read failed: {ex.GetType().Name}: {ex.Message}"));
                    continue;
                }

                try
                {
                    string migrated = BrandMigrator.Migrate(text, out int count);
                    if (count > 0)
                        changed.Add((Path.GetFullPath(path), migrated, bom, count));
                }
                catch (Exception ex)
                {
                    errors.Add((Path.GetFullPath(path), $"migrate failed: {ex.GetType().Name}: {ex.Message}"));
                }
            }

            foreach (string p in skippedEncoding)
                Console.Error.WriteLine(
                    "warning: skipped (not valid UTF-8 — re-save it as UTF-8 and re-run, or "
                        + $"migrate it by hand): {p}");

            foreach (var (p, _, _, count) in changed)
                Console.Error.WriteLine($"{(check ? "would change" : "rewrote")}: {p} ({count} replacement(s))");

            var written = new List<string>();
            if (!check)
            {
                foreach (var (p, text, bom, _) in changed)
                {
                    try
                    {
                        File.WriteAllBytes(p, FileEncodings.Encode(text, bom));
                        written.Add(p);
                    }
                    catch (Exception ex)
                    {
                        errors.Add((p, $"write failed: {ex.GetType().Name}: {ex.Message}"));
                    }
                }
            }

            int total = changed.Sum(c => c.Count);
            Console.WriteLine(
                $"{scanned} file(s) scanned; {changed.Count} {(check ? "would change" : "rewritten")}; "
                    + $"{total} replacement(s).");

            if (skippedEncoding.Count > 0)
                Console.WriteLine($"{skippedEncoding.Count} file(s) skipped (not valid UTF-8).");

            if (errors.Count > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"ERROR: {errors.Count} file(s) failed:");
                foreach (var (p, message) in errors)
                    Console.Error.WriteLine($"  {p}: {message}");

                if (written.Count > 0)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"{written.Count} file(s) WERE written before the failures:");
                    foreach (string p in written)
                        Console.Error.WriteLine($"  {p}");
                    Console.Error.WriteLine(
                        "The project is in a MIXED state. Revert the files above (VCS), fix the "
                            + "errors — files kept read-only by Perforce/Plastic must be checked "
                            + "out first — then re-run. The tool is idempotent, so re-running "
                            + "after a partial pass is safe.");
                }
                return ExitFileErrors;
            }

            return check && changed.Count > 0 ? ExitCheckDirty : ExitOk;
        }

        private static void PrintUsage(TextWriter w)
        {
            w.WriteLine("usage: RuitkMigrateBrand <dir> [--check]");
            w.WriteLine();
            w.WriteLine("  Rewrites user sources for the 0.12.0 rename:");
            w.WriteLine("    ReactiveUITK -> Ruitk (namespaces, usings, global::, asmdef references)");
            w.WriteLine("    ReactiveUITKConfig -> RuitkConfig; __ReactiveUITK_* -> __Ruitk_* media names");
            w.WriteLine("    \"ReactiveUITK/...\" menu paths -> \"Reactive UI Toolkit/...\"");
            w.WriteLine("    REACTIVEUITK_HAS_TEST_FRAMEWORK -> RUITK_HAS_TEST_FRAMEWORK");
            w.WriteLine("    ReactiveUIToolKit -> ReactiveUIToolkit install-path segments");
            w.WriteLine();
            w.WriteLine("  <dir>     directory to scan (normally your project's Assets folder)");
            w.WriteLine("  --check   dry run; exit 1 if anything would change (-c)");
            w.WriteLine("  --help    show this help (-h)");
            w.WriteLine();
            w.WriteLine("  Scans " + string.Join(" ", ScanRules.Extensions) + ".");
            w.WriteLine("  Frozen extension marketplace IDs (ReactiveUITK.uitkx,");
            w.WriteLine("  UitkxVsix.ReactiveUITK, ...) are preserved verbatim.");
            w.WriteLine("  Requires the .NET 8 SDK (Unity does not ship one).");
            w.WriteLine("  Exit: 0 clean, 1 --check dirty, 2 usage, 3 file read/write errors.");
        }
    }
}
