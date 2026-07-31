#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ruitk.EditorSupport;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ruitk.Editor
{
    /// <summary>
    /// Editor window that runs the UITKX Source Generator unit tests
    /// (xUnit, <c>SourceGenerator~/Tests/</c>) via <c>dotnet test</c> and
    /// displays the output inline.
    ///
    /// Menu path: Reactive UI Toolkit / Diagnostics / Run Unit Tests
    /// </summary>
    public sealed class UitkxTestRunnerWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Diagnostics/Run Unit Tests")]
        public static void Open()
        {
            var window = GetWindow<UitkxTestRunnerWindow>("UITKX Unit Tests");
            window.minSize = new Vector2(520f, 420f);
            window.Show();
        }

        // ── State ─────────────────────────────────────────────────────────────

        private bool _running;
        private string _output = "Press  ▶  Run Tests  to execute the xUnit suite.";
        private bool _passed;

        // ── UI elements ───────────────────────────────────────────────────────

        private Button _runButton;
        private Label _statusLabel;
        private TextField _outputField;

        // ── GUI ───────────────────────────────────────────────────────────────

        private void CreateGUI()
        {
            rootVisualElement.style.flexGrow = 1f;
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.paddingTop = 8f;
            rootVisualElement.style.paddingLeft = 8f;
            rootVisualElement.style.paddingRight = 8f;
            rootVisualElement.style.paddingBottom = 8f;

            // ── Header row ───────────────────────────────────────────────────
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 8f,
                    alignItems = Align.Center,
                },
            };

            _runButton = new Button(OnRunClicked)
            {
                text = "▶  Run Tests",
                style = { minWidth = 110f, height = 28f },
            };
            header.Add(_runButton);

            _statusLabel = new Label("Idle")
            {
                style =
                {
                    marginLeft = 12f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1f,
                },
            };
            SetStatus(StatusKind.Idle);
            header.Add(_statusLabel);
            rootVisualElement.Add(header);

            // ── Path info ────────────────────────────────────────────────────
            string testDir = GetTestsDirectory();
            var pathLabel = new Label(
                testDir != null
                    ? $"Project: {testDir}"
                    : "Project: <package root not found — see the Console for the paths tried>"
            )
            {
                style =
                {
                    fontSize = 10f,
                    marginBottom = 6f,
                    whiteSpace = WhiteSpace.Normal,
                    color = new StyleColor(new Color(0.6f, 0.6f, 0.6f)),
                },
            };
            rootVisualElement.Add(pathLabel);

            // ── Output area ──────────────────────────────────────────────────
            _outputField = new TextField
            {
                multiline = true,
                isReadOnly = true,
                value = _output,
                style =
                {
                    flexGrow = 1f,
                    unityFontDefinition = StyleKeyword.Initial,
                    fontSize = 11f,
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            // Remove the default background tint on the inner text input element
            _outputField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var inner = _outputField.Q(className: "unity-text-field__input");
                if (inner != null)
                {
                    inner.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                    inner.style.color = new StyleColor(Color.white);
                    inner.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            });
            rootVisualElement.Add(_outputField);
        }

        // ── Run ───────────────────────────────────────────────────────────────

        private void OnRunClicked()
        {
            if (_running)
                return;

            string testDir = GetTestsDirectory();
            if (testDir == null)
            {
                SetOutput(RuitkPackagePaths.FailureMessage + "\n");
                SetStatus(StatusKind.Failed);
                return;
            }
            if (!Directory.Exists(testDir))
            {
                SetOutput(
                    "Test project not found:\n  " + testDir + "\n\n"
                    + "SourceGenerator~/ is a development-only folder and is not part of an Asset "
                    + "Store install — run these tests from a source checkout.\n"
                );
                SetStatus(StatusKind.Failed);
                return;
            }

            _running = true;
            _runButton.SetEnabled(false);
            SetOutput("Running dotnet test…\n");
            SetStatus(StatusKind.Running);

            _ = RunDotnetTestAsync(testDir);
        }

        private async Task RunDotnetTestAsync(string workingDir)
        {
            var sb = new StringBuilder();

            try
            {
                string dotnet = FindDotnet();
                if (!Directory.Exists(workingDir))
                {
                    AppendLine(sb, $"ERROR: test directory not found:\n  {workingDir}");
                    AppendLine(sb, "Ensure the SourceGenerator~/Tests folder exists.");
                    Finish(sb, success: false);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = dotnet,
                    Arguments = "test --verbosity normal",
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = new Process { StartInfo = psi };
                var outputDone = new TaskCompletionSource<bool>();
                var errorDone = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data == null)
                    {
                        outputDone.TrySetResult(true);
                        return;
                    }
                    lock (sb)
                        AppendLine(sb, e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data == null)
                    {
                        errorDone.TrySetResult(true);
                        return;
                    }
                    lock (sb)
                        AppendLine(sb, e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Stream output live to the window while the process runs
                var progressTask = Task.Run(async () =>
                {
                    while (!process.HasExited)
                    {
                        await Task.Delay(150);
                        string snapshot;
                        lock (sb)
                            snapshot = sb.ToString();
                        EditorApplication.delayCall += () => SetOutput(snapshot);
                    }
                });

                await Task.WhenAll(outputDone.Task, errorDone.Task, progressTask);
                process.WaitForExit();

                bool success = process.ExitCode == 0;
                Finish(sb, success);
            }
            catch (Exception ex)
            {
                AppendLine(sb, $"Exception launching dotnet: {ex.Message}");
                AppendLine(sb, "Make sure the .NET SDK is installed and on your PATH.");
                Finish(sb, success: false);
            }
        }

        private void Finish(StringBuilder sb, bool success)
        {
            string final = sb.ToString();
            EditorApplication.delayCall += () =>
            {
                _running = false;
                _passed = success;
                SetOutput(final);
                SetStatus(success ? StatusKind.Passed : StatusKind.Failed);
                _runButton.SetEnabled(true);
                Repaint();
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetOutput(string text)
        {
            _output = text;
            if (_outputField != null)
                _outputField.value = text;
        }

        private enum StatusKind
        {
            Idle,
            Running,
            Passed,
            Failed,
        }

        private void SetStatus(StatusKind kind)
        {
            if (_statusLabel == null)
                return;
            switch (kind)
            {
                case StatusKind.Idle:
                    _statusLabel.text = "Idle";
                    _statusLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                    break;
                case StatusKind.Running:
                    _statusLabel.text = "⏳  Running…";
                    _statusLabel.style.color = new StyleColor(new Color(0.9f, 0.8f, 0.2f));
                    break;
                case StatusKind.Passed:
                    _statusLabel.text = "✅  All tests passed";
                    _statusLabel.style.color = new StyleColor(new Color(0.3f, 0.9f, 0.35f));
                    break;
                case StatusKind.Failed:
                    _statusLabel.text = "❌  Tests failed";
                    _statusLabel.style.color = new StyleColor(new Color(0.95f, 0.3f, 0.2f));
                    break;
            }
        }

        /// <summary>
        /// Absolute path to the package's <c>SourceGenerator~/Tests/</c>, or <c>null</c> when the
        /// package root cannot be resolved.
        ///
        /// <para>Resolved through <see cref="RuitkPackagePaths"/> rather than assuming
        /// <c>Assets/ReactiveUIToolkit</c>, so this works in the UPM, embedded and <c>file:</c> layouts
        /// too. Note that <c>SourceGenerator~</c> is tilde-suffixed: it ships inside the UPM package but
        /// is absent from an Asset Store <c>.unitypackage</c> (the exporter walks the asset database,
        /// which ignores <c>~</c> folders), so a missing directory here is expected for store installs
        /// and is reported rather than run against.</para>
        ///
        /// <para>Must not throw — it is called while the window is being built.</para>
        /// </summary>
        private static string GetTestsDirectory()
        {
            if (!RuitkPackagePaths.TryGetRoot(out string packageRoot))
            {
                return null;
            }
            return Path.GetFullPath(Path.Combine(packageRoot, "SourceGenerator~", "Tests"));
        }

        private static string FindDotnet()
        {
            // On Windows, dotnet.exe is usually on PATH; try common locations too.
            foreach (
                string candidate in new[]
                {
                    "dotnet",
                    @"C:\Program Files\dotnet\dotnet.exe",
                    @"C:\Program Files (x86)\dotnet\dotnet.exe",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "dotnet",
                        "dotnet.exe"
                    ),
                }
            )
            {
                if (candidate == "dotnet")
                    return candidate; // let OS resolve
                if (File.Exists(candidate))
                    return candidate;
            }
            return "dotnet";
        }

        private static void AppendLine(StringBuilder sb, string line) => sb.AppendLine(line);
    }
}
#endif
