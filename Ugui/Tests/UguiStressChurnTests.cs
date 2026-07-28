using System.Collections.Generic;
using NUnit.Framework;
using Ruitk.Core;
using Ruitk.Core.Fiber;
using Ruitk.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ruitk.Ugui.Tests
{
    /// <summary>
    /// The automated counterpart of Samples/Components/StressTest for the
    /// uGUI backend: many host elements, many re-render cycles with moving
    /// positions and churning membership. Asserts structural correctness
    /// after every cycle — the reconciler and pool must stay coherent under
    /// sustained load, not just in single-step scenarios.
    /// </summary>
    public class UguiStressChurnTests
    {
        private const int BoxCount = 300;
        private const int Cycles = 20;

        private GameObject _canvasGo;
        private RectTransform _mountRect;
        private FiberRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("StressCanvas", typeof(Canvas));
            var mount = new GameObject("Mount", typeof(RectTransform));
            _mountRect = (RectTransform)mount.transform;
            _mountRect.SetParent(_canvasGo.transform, false);

            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(UguiElementRegistryProvider.GetDefaultRegistry())
            );
            _renderer = new FiberRenderer((object)_mountRect.gameObject, context);
        }

        [TearDown]
        public void TearDown()
        {
            _renderer?.Clear();
            if (_canvasGo != null)
                Object.DestroyImmediate(_canvasGo);
            var staging = GameObject.Find("Ruitk.Ugui.Staging");
            if (staging != null)
                Object.DestroyImmediate(staging);
        }

        private static VirtualNode BoxField(int count, float t)
        {
            var children = new VirtualNode[count + 1];
            var status = UguiBaseProps.__Rent<UguiTextProps>();
            status.Text = $"boxes: {count} t: {t:F2}";
            children[0] = U.Text(status, "status");

            for (int i = 0; i < count; i++)
            {
                var box = UguiBaseProps.__Rent<UguiImageProps>();
                box.Anchors = UguiAnchorPreset.BottomLeft;
                box.SizeDelta = new Vector2(16f, 16f);
                box.AnchoredPosition = new Vector2(
                    Mathf.Abs(Mathf.Sin(t + i * 0.37f)) * 400f,
                    Mathf.Abs(Mathf.Cos(t * 1.3f + i * 0.11f)) * 300f
                );
                box.Color = new Color(0.2f, 0.4f + (i % 5) * 0.1f, 0.9f, 1f);
                children[i + 1] = U.Image(box, $"box-{i}");
            }

            var area = UguiBaseProps.__Rent<UguiPanelProps>();
            return U.Panel(area, "area", children);
        }

        [Test]
        public void StressLoop_MovingBoxes_StructureStaysCoherent()
        {
            for (int cycle = 0; cycle < Cycles; cycle++)
            {
                float t = cycle * 0.16f;
                _renderer.Render(BoxField(BoxCount, t));

                var area = _mountRect.GetChild(0);
                Assert.AreEqual(BoxCount + 1, area.childCount, $"cycle {cycle}");
                Assert.AreEqual(
                    $"boxes: {BoxCount} t: {t:F2}",
                    area.GetChild(0).GetComponent<TextMeshProUGUI>().text,
                    $"cycle {cycle}"
                );
            }

            var lastArea = _mountRect.GetChild(0);
            var sampleRt = (RectTransform)lastArea.GetChild(1).transform;
            Assert.AreNotEqual(Vector2.zero, sampleRt.anchoredPosition);
        }

        [Test]
        public void StressLoop_ChurningMembership_ReusesPooledVisuals()
        {
            var seenBoxes = new HashSet<int>();

            for (int cycle = 0; cycle < Cycles; cycle++)
            {
                // Membership oscillates so unmounted boxes flow through the
                // pool and come back on later cycles. The churn amplitude
                // (100) stays under the per-adapter pool capacity (128) so
                // every unmounted box is poolable — the reuse bound below is
                // then exact, not capacity-diluted.
                int count = cycle % 2 == 0 ? BoxCount : BoxCount - 100;
                _renderer.Render(BoxField(count, cycle * 0.25f));

                var area = _mountRect.GetChild(0);
                Assert.AreEqual(count + 1, area.childCount, $"cycle {cycle}");
                for (int i = 1; i < area.childCount; i++)
                {
                    seenBoxes.Add(area.GetChild(i).gameObject.GetInstanceID());
                }
            }

            // Pooling bound: with churn amplitude under the pool capacity,
            // every removed box returns to the pool and comes back — distinct
            // Image instances across the whole run must stay at peak-live
            // plus a small slack (the no-pooling worst case would be
            // Cycles/2 * 100 extra instances).
            Assert.LessOrEqual(
                seenBoxes.Count,
                BoxCount + 50,
                "membership churn must reuse pooled instances instead of leaking new ones"
            );
        }
    }
}
