// Run with: dotnet script FormatTest.csx [path-to-file.uitkx]   (or csi)
// With no argument it formats Samples/Components/UitkxCounterFunc/UitkxCounterFunc.uitkx from THIS
// checkout: the repo root is derived from the script's own location (CallerFilePath -> ../..), never
// written down, so the script works in any clone. See CLAUDE.md "Machine-local paths".
using Ruitk.Language.Formatter;

static string ThisScriptPath(
    [System.Runtime.CompilerServices.CallerFilePath] string path = ""
) => path;

var scriptDir = System.IO.Path.GetDirectoryName(ThisScriptPath());
var repoRoot = System.IO.Path.GetFullPath(
    System.IO.Path.Combine(string.IsNullOrEmpty(scriptDir) ? "." : scriptDir, "..", "..")
);
var target =
    Args.Count > 0
        ? Args[0]
        : System.IO.Path.Combine(
            repoRoot,
            "Samples",
            "Components",
            "UitkxCounterFunc",
            "UitkxCounterFunc.uitkx"
        );

if (!System.IO.File.Exists(target)) {
    System.Console.WriteLine($"ERROR: file not found: {target}");
    System.Console.WriteLine("Pass a .uitkx path as the first argument.");
    return;
}

var source = System.IO.File.ReadAllText(target);
try {
    var f = new AstFormatter(FormatterOptions.Default);
    var result = f.Format(source, System.IO.Path.GetFileName(target));
    System.Console.WriteLine($"SUCCESS: {result.Length} chars, first 200: {result.Substring(0, System.Math.Min(200, result.Length))}");
} catch (System.Exception ex) {
    System.Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
    System.Console.WriteLine(ex.StackTrace?.Substring(0, System.Math.Min(500, ex.StackTrace?.Length ?? 0)));
}
