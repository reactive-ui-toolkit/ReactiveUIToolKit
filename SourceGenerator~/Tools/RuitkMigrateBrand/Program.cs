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
    /// Walks <c>&lt;dir&gt;</c> for user sources (<c>.cs</c>, <c>.uitkx</c>, <c>.asmdef</c>),
    /// applies the 0.12.0 rebrand rules, and writes changed files back with per-file
    /// replacement counts. <c>--check</c> makes it a dry run that exits non-zero if anything
    /// would change — the idempotence gate (run once for real, then <c>--check</c> must be clean).
    ///
    /// Skipped entirely: the installed package folder itself (any path segment named
    /// <c>ReactiveUIToolKit</c> or <c>ReactiveUIToolkit</c>), <c>~</c>-suffixed tooling folders,
    /// and <c>.git</c>/<c>Library</c>/<c>Temp</c>/<c>obj</c>/<c>bin</c>.
    /// </summary>
    public static class Program
    {
        private static readonly string[] s_extensions = { ".cs", ".uitkx", ".asmdef" };

        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: RuitkMigrateBrand <dir> [--check]");
                Console.Error.WriteLine("  Rewrites user sources for the 0.12.0 rename (ReactiveUITK -> Ruitk,");
                Console.Error.WriteLine("  RuitkConfig, __Ruitk_* media object names, RUITK_HAS_TEST_FRAMEWORK,");
                Console.Error.WriteLine("  Assets/ReactiveUIToolkit paths). --check = dry run, exit 1 if dirty.");
                return 2;
            }

            string root = Path.GetFullPath(args[0]);
            bool check = args.Contains("--check");
            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine($"error: directory not found: {root}");
                return 2;
            }

            int scanned = 0;
            var changed = new List<(string Path, string Text, int Count)>();
            foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (!s_extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) continue;
                if (IsSkipped(Path.GetRelativePath(root, path))) continue;
                scanned++;

                string text = File.ReadAllText(path);
                string migrated = BrandMigrator.Migrate(text, out int count);
                if (count > 0)
                    changed.Add((Path.GetFullPath(path), migrated, count));
            }

            foreach (var (p, _, count) in changed)
                Console.Error.WriteLine($"{(check ? "would change" : "rewrote")}: {p} ({count} replacement(s))");

            if (!check)
                foreach (var (p, text, _) in changed)
                    File.WriteAllText(p, text);

            int total = changed.Sum(c => c.Count);
            Console.WriteLine($"{scanned} file(s) scanned; {changed.Count} {(check ? "would change" : "rewritten")}; {total} replacement(s).");
            return check && changed.Count > 0 ? 1 : 0;
        }

        /// <param name="relativePath">Path RELATIVE to the scan root — segments above the root
        /// (e.g. a parent folder that happens to be named Temp) must not trigger skips.</param>
        private static bool IsSkipped(string relativePath)
        {
            foreach (string part in relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                // Never edit inside the installed package folder itself (either casing era).
                if (part.Equals("ReactiveUIToolKit", StringComparison.Ordinal)) return true;
                if (part.Equals("ReactiveUIToolkit", StringComparison.Ordinal)) return true;
                if (part.EndsWith("~", StringComparison.Ordinal)) return true;
                if (part is ".git" or "Library" or "Temp" or "obj" or "bin") return true;
            }
            return false;
        }
    }
}
