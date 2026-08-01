// Minimal stand-ins for the UnityEngine surface that Shared/Core touches.
//
// Why a shim instead of referencing Unity's real assemblies: those DLLs reference
// native ECall implementations that only exist inside the Unity runtime. Probed on
// 6000.5.6f1 outside the editor, Debug.Log / Debug.LogWarning / Debug.LogError,
// Time.realtimeSinceStartup, GUID.Generate and `new VisualElement()` all throw
// SecurityException: "ECall methods must be packaged into a system module". The
// reconciler logs warnings on exactly the paths these tests exercise, so referencing
// the real assemblies would make the suite unrunnable.
//
// The compile-time check against the REAL Unity assemblies is a separate harness
// (UnityCompileCheck~) that compiles but never executes.
//
// Debug here CAPTURES instead of no-opping, so tests can assert on what the
// reconciler logged - warnings that used to be invisible are now assertable.

using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public enum LogKind
    {
        Log,
        Warning,
        Error,
    }

    public readonly struct LogEntry
    {
        public readonly LogKind Kind;
        public readonly string Message;

        public LogEntry(LogKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        public override string ToString() => $"[{Kind}] {Message}";
    }

    public static class Debug
    {
        private static readonly List<LogEntry> s_entries = new List<LogEntry>();

        public static IReadOnlyList<LogEntry> Entries => s_entries;

        public static void Clear() => s_entries.Clear();

        public static void Log(object message) =>
            s_entries.Add(new LogEntry(LogKind.Log, message?.ToString() ?? ""));

        public static void LogWarning(object message) =>
            s_entries.Add(new LogEntry(LogKind.Warning, message?.ToString() ?? ""));

        public static void LogError(object message) =>
            s_entries.Add(new LogEntry(LogKind.Error, message?.ToString() ?? ""));

        public static void LogException(Exception exception) =>
            s_entries.Add(new LogEntry(LogKind.Error, exception?.ToString() ?? ""));
    }

    public static class Mathf
    {
        public const float PI = (float)Math.PI;

        public static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        public static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float Max(float a, float b) => a > b ? a : b;

        public static float Min(float a, float b) => a < b ? a : b;

        public static float Pow(float f, float p) => (float)Math.Pow(f, p);

        public static float Sin(float f) => (float)Math.Sin(f);

        public static float Cos(float f) => (float)Math.Cos(f);

        public static float Abs(float f) => Math.Abs(f);

        public static float Sqrt(float f) => (float)Math.Sqrt(f);
    }

    public static class Time
    {
        // Tests drive time explicitly rather than reading a wall clock, so animation
        // and scheduling assertions stay deterministic.
        public static float realtimeSinceStartup { get; set; }

        public static double realtimeSinceStartupAsDouble { get; set; }

        public static float deltaTime { get; set; }

        public static float unscaledDeltaTime { get; set; }

        public static void Reset()
        {
            realtimeSinceStartup = 0f;
            realtimeSinceStartupAsDouble = 0d;
            deltaTime = 0f;
            unscaledDeltaTime = 0f;
        }
    }
}

namespace UnityEngine.Profiling
{
    public static class Profiler
    {
        public static void BeginSample(string name) { }

        public static void EndSample() { }
    }
}

namespace UnityEngine.Profiling
{
    public sealed class CustomSampler
    {
        private CustomSampler() { }

        public static CustomSampler Create(string name, bool collectGpuData = false) =>
            new CustomSampler();

        public void Begin() { }

        public void End() { }
    }
}

namespace UnityEngine
{
    // Present only so linked sources that mention these types compile. Nothing in the
    // core under test constructs or drives them; the mock host uses POCO handles.
    public class Object
    {
        public string name { get; set; } = "";

        public static void Destroy(Object obj) { }

        public static void DestroyImmediate(Object obj) { }

        public static void DontDestroyOnLoad(Object target) { }
    }

    public class Component : Object
    {
        private GameObject _go;

        public GameObject gameObject => _go ??= new GameObject(name);
    }

    public class MonoBehaviour : Component { }
}

namespace UnityEngine.UIElements
{
    // A structural stand-in only. The reconciler never touches VisualElement in these
    // tests - the mock FiberHostConfig hands it POCO handles - but a few linked files
    // name the type in signatures, so it has to exist to compile.
    public class VisualElement
    {
        public string name { get; set; } = "";

        public VisualElement parent { get; internal set; }

        private readonly System.Collections.Generic.List<VisualElement> _children =
            new System.Collections.Generic.List<VisualElement>();

        public int childCount => _children.Count;

        public VisualElement ElementAt(int index) => _children[index];

        public void Add(VisualElement child)
        {
            child.parent = this;
            _children.Add(child);
        }

        public void Remove(VisualElement child)
        {
            if (_children.Remove(child))
                child.parent = null;
        }

        public void Clear()
        {
            foreach (var c in _children)
                c.parent = null;
            _children.Clear();
        }
    }
}

namespace UnityEngine
{
    [System.Flags]
    public enum HideFlags
    {
        None = 0,
        HideInHierarchy = 1,
        HideInInspector = 2,
        DontSaveInEditor = 4,
        NotEditable = 8,
        DontSaveInBuild = 16,
        DontUnloadUnusedAsset = 32,
        DontSave = 52,
        HideAndDontSave = 61,
    }

    public class GameObject : Object
    {
        public GameObject() { }

        public GameObject(string name) => this.name = name;

        public HideFlags hideFlags { get; set; }

        public T AddComponent<T>() where T : Component, new() => new T();
    }
}
