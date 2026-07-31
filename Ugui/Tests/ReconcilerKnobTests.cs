using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruitk.Core;
using Ruitk.Core.Fiber;
using Ruitk.Elements;
using TMPro;
using UnityEngine;

namespace Ruitk.Ugui.Tests
{
    /// <summary>
    /// EditMode plumbing tests for the M3 reconciler knobs (<c>time_slicing</c>,
    /// <c>time_slice_ms</c>): the compiled defaults reproduce the former constants, the
    /// default dispatch routes updates through an installed scheduler unchanged, the
    /// explicit bypass (<c>time_slicing=false</c>) renders synchronously without touching
    /// the scheduler, the slice budget controls yielding, and a state update deferred
    /// during a bypassed commit still replays synchronously (no slice callback exists to
    /// pick it up). Resolution order for all four knobs is covered in
    /// <see cref="RuitkSettingsJsonTests"/>; the <c>host_node_pool</c> gate is covered in
    /// <see cref="UguiStressChurnTests"/>.
    /// </summary>
    public class ReconcilerKnobTests
    {
        /// <summary>
        /// Records enqueues without running them, so tests can observe the dispatch decision
        /// and pump slices one at a time.
        /// </summary>
        private sealed class RecordingScheduler : IScheduler
        {
            public readonly Queue<Action> Actions = new Queue<Action>();
            public readonly List<Action> BatchedEffects = new List<Action>();
            public int EnqueueCount;

            public void Enqueue(
                Action action,
                IScheduler.Priority priority = IScheduler.Priority.Normal
            )
            {
                EnqueueCount++;
                Actions.Enqueue(action);
            }

            public void EnqueueBatchedEffect(Action effect)
            {
                BatchedEffects.Add(effect);
            }

            public void BeginBatch() { }

            public void EndBatch() { }

            public void PumpNow()
            {
                while (Actions.Count > 0)
                {
                    Actions.Dequeue()();
                }
                var effects = BatchedEffects.ToArray();
                BatchedEffects.Clear();
                foreach (var effect in effects)
                {
                    effect();
                }
            }
        }

        private GameObject _canvasGo;
        private RectTransform _mountRect;
        private FiberRenderer _renderer;
        private RecordingScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("KnobCanvas", typeof(Canvas));
            var mount = new GameObject("Mount", typeof(RectTransform));
            _mountRect = (RectTransform)mount.transform;
            _mountRect.SetParent(_canvasGo.transform, false);

            _scheduler = new RecordingScheduler();
            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(UguiElementRegistryProvider.GetDefaultRegistry())
            );
            context.Environment["scheduler"] = _scheduler;
            _renderer = new FiberRenderer((object)_mountRect.gameObject, context);
        }

        [TearDown]
        public void TearDown()
        {
            // Restore the compiled defaults so knob state never leaks across tests.
            FiberConfig.TimeSlicingEnabled = true;
            FiberConfig.TimeSliceMs = 2.0f;

            // Let any parked slices finish before unmounting.
            _scheduler?.PumpNow();
            _renderer?.Clear();
            _scheduler?.PumpNow();
            if (_canvasGo != null)
                UnityEngine.Object.DestroyImmediate(_canvasGo);
            var staging = GameObject.Find("Ruitk.Ugui.Staging");
            if (staging != null)
                UnityEngine.Object.DestroyImmediate(staging);
        }

        private static VirtualNode Boxes(int count)
        {
            var children = new VirtualNode[count];
            for (int i = 0; i < count; i++)
            {
                var box = UguiBaseProps.__Rent<UguiImageProps>();
                box.SizeDelta = new Vector2(8f, 8f);
                children[i] = U.Image(box, $"box-{i}");
            }
            var area = UguiBaseProps.__Rent<UguiPanelProps>();
            return U.Panel(area, "area", children);
        }

        private Transform Area => _mountRect.GetChild(0);

        /// <summary>Runs parked slices one at a time until none remain; returns how many ran.</summary>
        private int DrainSlices(int maxActions = 10000)
        {
            int ran = 0;
            while (_scheduler.Actions.Count > 0 && ran < maxActions)
            {
                _scheduler.Actions.Dequeue()();
                ran++;
            }
            Assert.Less(ran, maxActions, "slice drain did not terminate");
            return ran;
        }

        [Test]
        public void Defaults_ReproduceTheFormerConstants()
        {
            // M3 acceptance: at defaults the knobs short-circuit to the old code paths —
            // slicing enabled, 2.0 ms slice (the deleted FiberReconciler const).
            Assert.IsTrue(FiberConfig.TimeSlicingEnabled);
            Assert.AreEqual(2.0f, FiberConfig.TimeSliceMs);
        }

        [Test]
        public void TimeSlicing_Default_RoutesUpdatesThroughScheduler()
        {
            // Initial mount is always synchronous, scheduler or not (React 18 semantics).
            _renderer.Render(Boxes(3));
            Assert.AreEqual(3, Area.childCount);
            Assert.AreEqual(0, _scheduler.EnqueueCount, "mount must not go through the scheduler");

            // A state-driven update goes through the scheduler and commits only when the
            // slice runs — today's behavior, untouched at the default.
            _renderer.Render(Boxes(5));
            Assert.AreEqual(3, Area.childCount, "update must not commit before its slice runs");
            Assert.GreaterOrEqual(_scheduler.EnqueueCount, 1);

            DrainSlices();
            Assert.AreEqual(5, Area.childCount);
        }

        [Test]
        public void TimeSlicing_Bypass_RendersSynchronously_WithoutTouchingScheduler()
        {
            _renderer.Render(Boxes(3));
            Assert.AreEqual(3, Area.childCount);

            FiberConfig.TimeSlicingEnabled = false;
            _renderer.Render(Boxes(5));

            Assert.AreEqual(5, Area.childCount, "bypass must commit synchronously");
            Assert.AreEqual(
                0,
                _scheduler.EnqueueCount,
                "bypass must not schedule render work even though a scheduler is installed"
            );
        }

        [Test]
        public void TimeSliceMs_ControlsSliceYielding()
        {
            _renderer.Render(Boxes(1));
            Assert.AreEqual(1, Area.childCount);

            // A zero budget yields after every unit of work: a multi-node update must take
            // multiple slices (each slice re-enqueues the next).
            FiberConfig.TimeSliceMs = 0f;
            _renderer.Render(Boxes(40));
            int slices = DrainSlices();
            Assert.AreEqual(40, Area.childCount);
            Assert.GreaterOrEqual(slices, 2, "a zero slice budget must force multiple slices");

            // An effectively-unbounded budget completes any update in a single slice.
            FiberConfig.TimeSliceMs = 100000f;
            _renderer.Render(Boxes(80));
            Assert.AreEqual(1, _scheduler.Actions.Count);
            _scheduler.Actions.Dequeue()();
            Assert.AreEqual(80, Area.childCount);
            Assert.AreEqual(
                0,
                _scheduler.Actions.Count,
                "an unbounded slice budget must finish without rescheduling"
            );
        }

        [Test]
        public void TimeSlicingBypass_StateUpdateDuringCommit_ReplaysSynchronously()
        {
            // A layout effect runs in the commit phase; its setState is deferred until the
            // commit completes. With the bypass active there is no slice callback to pick
            // the deferred work up, so the replay must restart the synchronous work loop —
            // the committed UI reflects the follow-up render before Render() returns.
            FiberConfig.TimeSlicingEnabled = false;

            VirtualNode Component(IProps props, IReadOnlyList<VirtualNode> children)
            {
                var (value, setValue) = Hooks.UseState(0);
                Hooks.UseLayoutEffect(
                    () =>
                    {
                        if (value == 0)
                        {
                            setValue(1);
                        }
                        return null;
                    },
                    value
                );
                var tp = UguiBaseProps.__Rent<UguiTextProps>();
                tp.Text = $"v:{value}";
                return U.Text(tp);
            }

            _renderer.Render(V.Func(Component));

            var tmp = _mountRect.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.AreEqual(
                "v:1",
                tmp.text,
                "the deferred commit-phase update must replay synchronously under the bypass"
            );
        }
    }
}
