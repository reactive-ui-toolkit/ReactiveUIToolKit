using Ruitk.Core;
using Ruitk.Core.Config;
using Ruitk.EditorSupport.HMR;
using UnityEditor;
using UnityEngine;

namespace Ruitk.EditorSupport
{
    /// <summary>
    /// The one Reactive UI Toolkit settings window (<i>Reactive UI Toolkit ▸ Settings</i>):
    /// every user-facing knob on a single scrollable screen, in clearly labeled sections —
    /// <b>Configuration</b> (the project-scoped <see cref="RuitkSettings"/> asset),
    /// <b>Hot Reload (HMR)</b> (per-developer EditorPrefs, including the two keybind
    /// recorders), and <b>Console navigation</b>. It replaces the former Project Settings /
    /// Preferences pages; windows that used to embed settings (the HMR window) link here
    /// instead.
    ///
    /// <para>Without a settings asset the Configuration section is read-only: it shows the
    /// values currently in effect (compiled defaults, or a legacy <c>config.json</c> if the
    /// project still honours one) plus a "Create settings asset" button. Nothing is ever
    /// written to the project until the user clicks that button — the Input System pattern:
    /// merely opening the window must not dirty version control or force an asset on projects
    /// that are happy with the defaults.</para>
    /// </summary>
    internal sealed class RuitkSettingsWindow : EditorWindow
    {
        private Vector2 _scroll;
        private SerializedObject _serialized;
        private UitkxHmrController _prefsController;
        private int _recording; // 0=none, 1=toggle HMR, 2=open window

        [MenuItem("Reactive UI Toolkit/Settings", priority = 300)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<RuitkSettingsWindow>("Reactive UI Toolkit Settings");
            wnd.minSize = new Vector2(380, 440);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(8);
            DrawConfigurationSection();

            EditorGUILayout.Space(12);
            DrawSeparator();
            EditorGUILayout.Space(8);
            DrawHmrSection();

            EditorGUILayout.Space(12);
            DrawSeparator();
            EditorGUILayout.Space(8);
            DrawConsoleNavigationSection();

            EditorGUILayout.Space(12);
            EditorGUILayout.EndScrollView();
        }

        // ── Configuration (per-project: the RuitkSettings asset) ─────────────

        private void DrawConfigurationSection()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Per project — stored in the settings asset, applied to player builds.",
                EditorStyles.miniLabel
            );
            EditorGUILayout.Space(4);

            var settings = RuitkSettings.ActiveOrNull;
            if (settings == null)
            {
                settings = RuitkSettingsBootstrap.Resolve();
            }

            if (settings == null)
            {
                DrawNoAsset();
                return;
            }

            DrawAsset(settings);
        }

        private void DrawNoAsset()
        {
            _serialized = null;

            EditorGUILayout.HelpBox(
                "No Reactive UI Toolkit settings asset exists in this project, so the values "
                    + "below are in effect (compiled defaults, or a legacy "
                    + "Assets/ReactiveUIToolkit/config.json if the project still carries one). "
                    + "Create the asset to edit them; it is also preloaded into player builds.",
                MessageType.Info
            );

            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("Effective values", EditorStyles.boldLabel);
                EditorGUILayout.TextField("Environment", BuildDefinesConfig.ResolveEnvironment());
                EditorGUILayout.EnumPopup("Trace Level", BuildDefinesConfig.ResolveTraceLevel());
                EditorGUILayout.Toggle("Diff Tracing", BuildDefinesConfig.ResolveEnableDiffTracing());
                EditorGUILayout.Toggle(
                    "Exception Control Flow",
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow()
                );
                EditorGUILayout.TextField(
                    "Diagnostics Output",
                    RuitkDiagnosticsPaths.GetOutputRoot()
                );
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Create settings asset", GUILayout.Width(180)))
            {
                var created = RuitkSettingsBootstrap.CreateSettingsAsset();
                EditorGUIUtility.PingObject(created);
            }
        }

        private void DrawAsset(RuitkSettings settings)
        {
            if (_serialized == null || _serialized.targetObject != settings)
            {
                _serialized = new SerializedObject(settings);
            }
            _serialized.Update();

            EditorGUILayout.PropertyField(
                _serialized.FindProperty(nameof(RuitkSettings.environment)),
                new GUIContent(
                    "Environment",
                    "Environment label exposed as HostContext.Environment[\"env\"]. Auto resolves "
                        + "to development in the editor and development builds, production otherwise."
                )
            );
            EditorGUILayout.PropertyField(
                _serialized.FindProperty(nameof(RuitkSettings.traceLevel)),
                new GUIContent("Trace Level", "Reconciler trace level (None, Basic, Verbose).")
            );
            EditorGUILayout.PropertyField(
                _serialized.FindProperty(nameof(RuitkSettings.diffTracing)),
                new GUIContent("Diff Tracing", "Detailed Fiber diff/reconciliation tracing.")
            );
            EditorGUILayout.PropertyField(
                _serialized.FindProperty(nameof(RuitkSettings.exceptionControlFlow)),
                new GUIContent(
                    "Exception Control Flow",
                    "Route render exceptions through the exception-boundary control flow."
                )
            );
            var outputFolder = _serialized.FindProperty(
                nameof(RuitkSettings.diagnosticsOutputFolder)
            );
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(
                    outputFolder,
                    new GUIContent(
                        "Diagnostics Output Folder",
                        "Where benchmark results and log captures are written. Empty = "
                            + "<project>/Logs/ReactiveUIToolkit in the editor, "
                            + "<persistentDataPath>/ReactiveUIToolkit in players. Absolute paths "
                            + "are used as-is; relative paths resolve against the project root "
                            + "(editor) or persistentDataPath (player)."
                    )
                );
                if (GUILayout.Button("Browse…", GUILayout.Width(70)))
                {
                    string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
                    string start = string.IsNullOrEmpty(outputFolder.stringValue)
                        ? projectRoot
                        : outputFolder.stringValue;
                    string picked = EditorUtility.OpenFolderPanel(
                        "Diagnostics Output Folder",
                        start,
                        ""
                    );
                    if (!string.IsNullOrEmpty(picked))
                    {
                        // Folders inside the project are stored project-relative so the asset
                        // stays valid when the project moves or is shared between machines.
                        string normalizedRoot = projectRoot.Replace('\\', '/').TrimEnd('/') + "/";
                        string normalizedPick = picked.Replace('\\', '/');
                        outputFolder.stringValue = normalizedPick.StartsWith(
                            normalizedRoot,
                            System.StringComparison.OrdinalIgnoreCase
                        )
                            ? normalizedPick.Substring(normalizedRoot.Length)
                            : picked;
                        GUI.FocusControl(null);
                    }
                }
            }
            _serialized.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            string assetPath = AssetDatabase.GetAssetPath(settings);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Asset", assetPath);
                if (GUILayout.Button("Select", GUILayout.Width(70)))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Effective environment: " + BuildDefinesConfig.ResolveEnvironment()
                    + "\nDiagnostics output root: " + RuitkDiagnosticsPaths.GetOutputRoot()
                    + "\nThe asset is added to Preloaded Assets during player builds so these "
                    + "values apply in players too.",
                MessageType.None
            );
        }

        // ── Hot Reload (HMR) (per-developer: EditorPrefs) ────────────────────

        /// <summary>
        /// The live controller when HMR is running, else a dormant one — constructing a
        /// controller is side-effect free until <c>Start()</c>, and its pref-backed properties
        /// are how the EditorPrefs keys stay defined in exactly one place each.
        /// </summary>
        private UitkxHmrController Controller
        {
            get
            {
                var live = UitkxHmrController.Instance;
                if (live != null)
                {
                    return live;
                }
                return _prefsController ??= new UitkxHmrController();
            }
        }

        private void DrawHmrSection()
        {
            EditorGUILayout.LabelField("Hot Reload (HMR)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Per developer — stored in EditorPrefs on this machine, not in the project.",
                EditorStyles.miniLabel
            );
            EditorGUILayout.Space(4);

            var controller = Controller;

            bool autoStop = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto-stop on Play Mode",
                    "Stop HMR automatically when entering Play Mode."
                ),
                controller.AutoStopOnPlayMode
            );
            if (autoStop != controller.AutoStopOnPlayMode)
            {
                controller.AutoStopOnPlayMode = autoStop;
            }

            bool showNotify = EditorGUILayout.Toggle(
                new GUIContent(
                    "Show notifications",
                    "Show an in-window notification after each successful hot swap."
                ),
                controller.ShowNotifications
            );
            if (showNotify != controller.ShowNotifications)
            {
                controller.ShowNotifications = showNotify;
            }

            bool autoReload = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto-reload on rude edit",
                    "When a CLR rude edit is detected (newly added static readonly field), "
                        + "request a domain reload automatically."
                ),
                controller.AutoReloadOnRudeEdit
            );
            if (autoReload != controller.AutoReloadOnRudeEdit)
            {
                controller.AutoReloadOnRudeEdit = autoReload;
            }

            bool verboseWatcher = EditorGUILayout.Toggle(
                new GUIContent(
                    "Verbose watcher trace",
                    "Log every raw file-watcher event to the Console ([HMR][trace]). High noise; "
                        + "use when a save appears to do nothing."
                ),
                controller.VerboseWatcherTrace
            );
            if (verboseWatcher != controller.VerboseWatcherTrace)
            {
                controller.VerboseWatcherTrace = verboseWatcher;
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Keyboard shortcuts", EditorStyles.miniBoldLabel);
            DrawKeybindRow("Toggle HMR", 1, UitkxHmrKeybinds.ToggleHmrKey);
            DrawKeybindRow("Open HMR Window", 2, UitkxHmrKeybinds.ToggleWindowKey);
        }

        /// <summary>
        /// The keybind recorder (moved here from the HMR window — it needs an EditorWindow's
        /// key-capture loop): click the combo button, press a modifier+key, Escape cancels,
        /// × clears.
        /// </summary>
        private void DrawKeybindRow(string label, int id, KeyCombo current)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

                if (_recording == id)
                {
                    // Recording mode — capture next key combo
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.3f);
                    GUILayout.Button("Press keys...", EditorStyles.miniButton, GUILayout.Width(90));
                    GUI.backgroundColor = Color.white;

                    var e = Event.current;
                    if (e != null && e.type == EventType.KeyDown)
                    {
                        if (e.keyCode == KeyCode.Escape)
                        {
                            _recording = 0;
                            e.Use();
                        }
                        else if (
                            e.keyCode != KeyCode.None
                            && (e.control || e.alt || e.shift)
                            && e.keyCode != KeyCode.LeftControl
                            && e.keyCode != KeyCode.RightControl
                            && e.keyCode != KeyCode.LeftAlt
                            && e.keyCode != KeyCode.RightAlt
                            && e.keyCode != KeyCode.LeftShift
                            && e.keyCode != KeyCode.RightShift
                        )
                        {
                            var combo = new KeyCombo(e.control, e.alt, e.shift, e.keyCode);
                            if (id == 1)
                                UitkxHmrKeybinds.ToggleHmrKey = combo;
                            else
                                UitkxHmrKeybinds.ToggleWindowKey = combo;
                            _recording = 0;
                            e.Use();
                        }
                    }
                    Repaint();
                }
                else
                {
                    if (
                        GUILayout.Button(
                            current.ToDisplay(),
                            EditorStyles.miniButton,
                            GUILayout.Width(90)
                        )
                    )
                        _recording = id;

                    EditorGUI.BeginDisabledGroup(!current.IsValid);
                    if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (id == 1)
                            UitkxHmrKeybinds.ToggleHmrKey = default;
                        else
                            UitkxHmrKeybinds.ToggleWindowKey = default;
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        // ── Console navigation (per-developer: EditorPrefs) ──────────────────

        private void DrawConsoleNavigationSection()
        {
            EditorGUILayout.LabelField("Console navigation", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Per developer — stored in EditorPrefs on this machine, not in the project.",
                EditorStyles.miniLabel
            );
            EditorGUILayout.Space(4);

            bool navVerbose = EditorGUILayout.Toggle(
                new GUIContent(
                    "Verbose .uitkx navigation logs",
                    "Log every step of .uitkx console-link resolution ([UITKX Nav]). "
                        + "Takes effect immediately — no domain reload needed."
                ),
                Ruitk.Editor.UitkxConsoleNavigation.VerboseLogging
            );
            if (navVerbose != Ruitk.Editor.UitkxConsoleNavigation.VerboseLogging)
            {
                Ruitk.Editor.UitkxConsoleNavigation.VerboseLogging = navVerbose;
            }
        }

        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        }
    }
}
