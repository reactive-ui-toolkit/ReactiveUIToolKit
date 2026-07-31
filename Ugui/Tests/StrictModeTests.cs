using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Ruitk.Core;
using Ruitk.Core.Fiber;
using Ruitk.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Ruitk.Ugui.Tests
{
    /// <summary>
    /// EditMode pins for <c>strict_mode</c> (U-07): with the knob on, a function component's
    /// render body runs exactly TWICE per logical render — the first result discarded, the
    /// second reconciled — while every hook family keeps its index-keyed slot (overwrite, never
    /// append, never re-fire side-effectful registration): effects and layout effects run ONCE
    /// per commit with the SECOND invoke's captures, memo/imperative factories run once when
    /// deps are unchanged between the invokes, refs keep one instance, and the committed UI is
    /// identical to strict-off. Strict-diagnostics warnings count the logical render once
    /// (<c>StrictDiagnosticsKeys</c> dedup) under the fixed <c>[Hooks][Strict]</c> prefix.
    /// The resolver-level release force-off is pinned in <see cref="RuitkSettingsJsonTests"/>;
    /// the pool interaction under churn in <see cref="UguiStressChurnTests"/>.
    /// </summary>
    public class StrictModeTests
    {
        private const string ContextKey = "strict-mode-tests/ctx";
        private const string SignalKey = "strict-mode-tests/counter";

        private GameObject _canvasGo;
        private RectTransform _mountRect;
        private FiberRenderer _renderer;

        // ── Per-family observability counters (reset per test / per phase) ────
        private int _renderCount;
        private int _mountEffectRuns;
        private int _mountEffectCapturedRender;
        private int _mountEffectCleanupRuns;
        private int _valueEffectRuns;
        private int _valueEffectCleanupRuns;
        private int _layoutEffectRuns;
        private int _memoFactoryRuns;
        private int _imperativeFactoryRuns;
        private readonly HashSet<object> _refInstances = new HashSet<object>();
        private int _contextSeen;
        private int _signalSeen;
        private int _deferredSeen;
        private int _reducerSeen;
        private Hooks.StateSetter<int> _setValue;

        [SetUp]
        public void SetUp()
        {
            // Pin the global diagnostics state these tests depend on (LogAssert-based
            // assertions require a silent trace configuration).
            Ruitk.Core.Diagnostics.DiagnosticsConfig.CurrentTraceLevel =
                Ruitk.Core.Diagnostics.DiagnosticsConfig.TraceLevel.None;
            Ruitk.Core.Diagnostics.InternalLogOptions.EnableInternalLogs = false;

            _canvasGo = new GameObject("StrictCanvas", typeof(Canvas));
            var mount = new GameObject("Mount", typeof(RectTransform));
            _mountRect = (RectTransform)mount.transform;
            _mountRect.SetParent(_canvasGo.transform, false);
            CreateRenderer();
            ResetCounters();
        }

        [TearDown]
        public void TearDown()
        {
            // Restore the compiled defaults so knob state never leaks across tests.
            FiberConfig.StrictModeEnabled = false;
            Hooks.EnableHookValidation = true;
            Hooks.EnableStrictDiagnostics = false;

            DestroyRenderer();
            if (_canvasGo != null)
                UnityEngine.Object.DestroyImmediate(_canvasGo);
            var staging = GameObject.Find("Ruitk.Ugui.Staging");
            if (staging != null)
                UnityEngine.Object.DestroyImmediate(staging);
        }

        private void CreateRenderer()
        {
            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(UguiElementRegistryProvider.GetDefaultRegistry())
            );
            context.Environment[ContextKey] = 42;
            _renderer = new FiberRenderer((object)_mountRect.gameObject, context);
        }

        private void DestroyRenderer()
        {
            _renderer?.Clear();
            _renderer = null;
        }

        private void ResetCounters()
        {
            _renderCount = 0;
            _mountEffectRuns = 0;
            _mountEffectCapturedRender = 0;
            _mountEffectCleanupRuns = 0;
            _valueEffectRuns = 0;
            _valueEffectCleanupRuns = 0;
            _layoutEffectRuns = 0;
            _memoFactoryRuns = 0;
            _imperativeFactoryRuns = 0;
            _refInstances.Clear();
            _contextSeen = 0;
            _signalSeen = 0;
            _deferredSeen = 0;
            _reducerSeen = 0;
            _setValue = null;
        }

        private string CommittedText =>
            _mountRect.GetChild(0).GetComponent<TextMeshProUGUI>().text;

        /// <summary>
        /// One component exercising every hook family that is live on the fiber path (the
        /// metadata-gated hooks — UseSafeArea, UseStable*, element UseRef() — are slot-neutral
        /// there and cannot be exercised without legacy metadata): state, reducer, memo,
        /// callback, ref, context, deferred, transition, imperative handle, signal,
        /// animate (the tween/animate family), layout effect, and two effects (mount-only +
        /// deps-gated).
        /// </summary>
        private VirtualNode KitchenSink(IProps props, IReadOnlyList<VirtualNode> children)
        {
            _renderCount++;
            int renderNumber = _renderCount;

            var (value, setValue) = Hooks.UseState(0);
            _setValue = setValue;

            var (reduced, _) = Hooks.UseReducer<int, int>((s, a) => s + a, 10);
            _reducerSeen = reduced;

            int memoized = Hooks.UseMemo(
                () =>
                {
                    _memoFactoryRuns++;
                    return value * 2;
                },
                value
            );

            var callback = Hooks.UseCallback(() => value, value);

            var reference = Hooks.UseRef(0);
            _refInstances.Add(reference);

            _contextSeen = Hooks.UseContext<int>(ContextKey);

            _deferredSeen = Hooks.UseDeferredValue(value, value);

            var (_, startTransition) = Hooks.UseTransition();
            startTransition(null);

            Hooks.UseImperativeHandle<object>(() =>
            {
                _imperativeFactoryRuns++;
                return new object();
            });

            _signalSeen = Hooks.UseSignal<int>(SignalKey, 7);

            Hooks.UseAnimate(null, autoplay: false);

            Hooks.UseLayoutEffect(() =>
            {
                _layoutEffectRuns++;
                return null;
            });

            Hooks.UseEffect(() =>
            {
                _mountEffectRuns++;
                _mountEffectCapturedRender = renderNumber;
                return () => _mountEffectCleanupRuns++;
            });

            Hooks.UseEffect(
                () =>
                {
                    _valueEffectRuns++;
                    return () => _valueEffectCleanupRuns++;
                },
                value
            );

            var tp = UguiBaseProps.__Rent<UguiTextProps>();
            tp.Text =
                $"v:{value} m:{memoized} cb:{callback()} r:{_reducerSeen} "
                + $"c:{_contextSeen} s:{_signalSeen} d:{_deferredSeen}";
            return U.Text(tp);
        }

        private VirtualNode SetStateDuringRender(IProps props, IReadOnlyList<VirtualNode> children)
        {
            _renderCount++;
            var (value, setValue) = Hooks.UseState(0);
            if (value == 0)
            {
                setValue(1);
            }
            var tp = UguiBaseProps.__Rent<UguiTextProps>();
            tp.Text = $"v:{value}";
            return U.Text(tp);
        }

        [Test]
        public void StrictOff_Mount_RendersOnce_Baseline()
        {
            FiberConfig.StrictModeEnabled = false;
            _renderer.Render(V.Func(KitchenSink));

            Assert.AreEqual(1, _renderCount);
            Assert.AreEqual(1, _mountEffectRuns);
            Assert.AreEqual(1, _mountEffectCapturedRender);
            Assert.AreEqual(1, _layoutEffectRuns);
            Assert.AreEqual(1, _memoFactoryRuns);
            Assert.AreEqual(1, _imperativeFactoryRuns);
        }

        [Test]
        public void StrictMode_Mount_RenderTwice_EffectsOnce_SlotsOverwritten()
        {
            FiberConfig.StrictModeEnabled = true;
            _renderer.Render(V.Func(KitchenSink));

            // The render body ran twice; every side-effectful registration fired once.
            Assert.AreEqual(2, _renderCount, "strict mount must double-invoke the render body");
            Assert.AreEqual(1, _mountEffectRuns, "effects must NOT be double-invoked");
            Assert.AreEqual(
                2,
                _mountEffectCapturedRender,
                "the committed effect must hold the SECOND invoke's captures (slot overwritten)"
            );
            Assert.AreEqual(1, _valueEffectRuns);
            Assert.AreEqual(1, _layoutEffectRuns, "layout effects must NOT be double-invoked");
            Assert.AreEqual(
                1,
                _memoFactoryRuns,
                "UseMemo must reuse the first invoke's slot (deps unchanged between invokes)"
            );
            Assert.AreEqual(
                1,
                _imperativeFactoryRuns,
                "UseImperativeHandle must reuse the first invoke's slot"
            );
            Assert.AreEqual(
                1,
                _refInstances.Count,
                "UseRef must hand out one instance across both invokes"
            );

            // Non-slot families stayed coherent on the reconciled (second) result.
            Assert.AreEqual(42, _contextSeen, "UseContext must resolve on both invokes");
            Assert.AreEqual(7, _signalSeen);
            Assert.AreEqual(0, _deferredSeen);
            Assert.AreEqual(10, _reducerSeen);
            Assert.AreEqual("v:0 m:0 cb:0 r:10 c:42 s:7 d:0", CommittedText);

            // Unmount: cleanups run once.
            DestroyRenderer();
            Assert.AreEqual(1, _mountEffectCleanupRuns, "cleanup must run once on unmount");
            Assert.AreEqual(1, _valueEffectCleanupRuns);
        }

        [Test]
        public void StrictMode_Update_RendersTwicePerPass_CommittedUiMatchesStrictOff()
        {
            // Baseline: strict OFF, mount + state update.
            FiberConfig.StrictModeEnabled = false;
            _renderer.Render(V.Func(KitchenSink));
            _setValue(5);

            string offText = CommittedText;
            int offValueEffectRuns = _valueEffectRuns;
            int offValueEffectCleanups = _valueEffectCleanupRuns;
            int offMountEffectRuns = _mountEffectRuns;
            int offMemoRuns = _memoFactoryRuns;
            Assert.AreEqual(2, _renderCount, "strict-off baseline: one render per pass");

            // Same sequence, strict ON, on a fresh renderer.
            DestroyRenderer();
            ResetCounters();
            CreateRenderer();
            FiberConfig.StrictModeEnabled = true;

            _renderer.Render(V.Func(KitchenSink));
            _setValue(5);

            Assert.AreEqual(4, _renderCount, "strict: two invokes per logical render");
            Assert.AreEqual(offText, CommittedText, "committed UI must be identical to strict-off");
            Assert.AreEqual(
                offMountEffectRuns,
                _mountEffectRuns,
                "mount-only effect count must match strict-off"
            );
            Assert.AreEqual(
                offValueEffectRuns,
                _valueEffectRuns,
                "deps-gated effect count must match strict-off"
            );
            Assert.AreEqual(offValueEffectCleanups, _valueEffectCleanupRuns);
            Assert.AreEqual(
                offMemoRuns,
                _memoFactoryRuns,
                "memo recomputes once per deps change, not per invoke"
            );
        }

        [Test]
        public void StrictMode_StateUpdateDuringRender_WarnsOnce_WithStrictPrefix()
        {
            FiberConfig.StrictModeEnabled = true;
            Hooks.EnableStrictDiagnostics = true;

            LogAssert.Expect(
                LogType.Warning,
                new Regex(@"\[Hooks\]\[Strict\] State update scheduled during render")
            );

            _renderer.Render(V.Func(SetStateDuringRender));

            // The mid-render update replayed synchronously; the offending render warned ONCE
            // across both strict invokes and the replay pass (StrictDiagnosticsKeys dedup).
            LogAssert.NoUnexpectedReceived();
            Assert.AreEqual("v:1", CommittedText);
            Assert.AreEqual(
                4,
                _renderCount,
                "mount pass (2 invokes) + deferred replay pass (2 invokes)"
            );
        }

        [Test]
        public void StrictMode_CompiledDefault_IsOff()
        {
            // §0.1 row 7 / M4 acceptance: strict_mode defaults OFF — the compiled initializer
            // and the no-store resolver agree, so an untouched project never double-invokes.
            Assert.IsFalse(FiberConfig.StrictModeEnabled);
        }
    }
}
