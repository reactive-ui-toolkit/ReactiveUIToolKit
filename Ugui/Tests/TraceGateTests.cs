using System.Collections.Generic;
using NUnit.Framework;
using Ruitk.Core;
using Ruitk.Core.Diagnostics;
using Ruitk.Core.Fiber;
using Ruitk.Elements;
using UnityEngine;

namespace Ruitk.Ugui.Tests
{
    /// <summary>
    /// M5 (U-08/§6) pins: the trace ladder's derived gates and <c>diff_tracing</c>
    /// independence. The gate matrix is §6's truth table in executable form —
    /// <b>structural</b> ⇒ <c>CurrentTraceLevel != None</c> (Basic RESTORED to its legacy
    /// meaning: structural events), <b>detail</b> ⇒ <c>== Verbose</c>, <b>diff</b> ⇒
    /// <c>EnableDiffTracing || == Verbose</c> (the exact legacy OR, `Reconciler.cs:1669` at
    /// `2d8b50a7~1`). The behavioral tests capture real reconciler output per configuration:
    /// Basic produces structural placement/deletion/replacement/commit lines and NO diff
    /// detail; diff_tracing alone (trace none) produces diff lines and NO structural lines;
    /// Verbose alone lights both.
    /// </summary>
    public class TraceGateTests
    {
        private GameObject _canvasGo;
        private RectTransform _mountRect;
        private FiberRenderer _renderer;
        private readonly List<string> _logs = new List<string>();
        private Application.LogCallback _capture;

        [SetUp]
        public void SetUp()
        {
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.None;
            DiagnosticsConfig.EnableDiffTracing = false;
            InternalLogOptions.EnableInternalLogs = false;

            _canvasGo = new GameObject("TraceCanvas", typeof(Canvas));
            var mount = new GameObject("Mount", typeof(RectTransform));
            _mountRect = (RectTransform)mount.transform;
            _mountRect.SetParent(_canvasGo.transform, false);

            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(UguiElementRegistryProvider.GetDefaultRegistry())
            );
            _renderer = new FiberRenderer((object)_mountRect.gameObject, context);

            _logs.Clear();
            _capture = (message, stackTrace, type) => _logs.Add(message);
            Application.logMessageReceived += _capture;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= _capture;
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.None;
            DiagnosticsConfig.EnableDiffTracing = false;

            _renderer?.Clear();
            if (_canvasGo != null)
                Object.DestroyImmediate(_canvasGo);
            var staging = GameObject.Find("Ruitk.Ugui.Staging");
            if (staging != null)
                Object.DestroyImmediate(staging);
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

        /// <summary>
        /// A one-child panel whose child swaps element type (Image ↔ Text) while
        /// keeping its tree position — the node-replacement decision. Keyed pins
        /// the same-key path; unkeyed pins the by-index occupied-slot path.
        /// </summary>
        private static VirtualNode Swap(bool image, bool keyed)
        {
            string key = keyed ? "slot" : null;
            VirtualNode child;
            if (image)
            {
                var box = UguiBaseProps.__Rent<UguiImageProps>();
                box.SizeDelta = new Vector2(8f, 8f);
                child = U.Image(box, key);
            }
            else
            {
                child = U.Text("swapped", key);
            }
            var area = UguiBaseProps.__Rent<UguiPanelProps>();
            return U.Panel(area, "area", child);
        }

        private int Count(string prefix)
        {
            int n = 0;
            foreach (var line in _logs)
            {
                if (line.StartsWith(prefix, System.StringComparison.Ordinal))
                    n++;
            }
            return n;
        }

        // ── §6 gate matrix (pure logic — the truth table, spelled row by row) ─

        [Test]
        public void GateMatrix_MatchesSection6TruthTable()
        {
            // (level, diff) -> (structural, detail, diffGate)
            var rows = new (DiagnosticsConfig.TraceLevel level, bool diff, bool structural, bool detail, bool diffGate)[]
            {
                (DiagnosticsConfig.TraceLevel.None, false, false, false, false),
                (DiagnosticsConfig.TraceLevel.None, true, false, false, true),
                (DiagnosticsConfig.TraceLevel.Basic, false, true, false, false),
                (DiagnosticsConfig.TraceLevel.Basic, true, true, false, true),
                (DiagnosticsConfig.TraceLevel.Verbose, false, true, true, true),
                (DiagnosticsConfig.TraceLevel.Verbose, true, true, true, true),
            };

            foreach (var row in rows)
            {
                DiagnosticsConfig.CurrentTraceLevel = row.level;
                DiagnosticsConfig.EnableDiffTracing = row.diff;

                bool structural =
                    DiagnosticsConfig.CurrentTraceLevel != DiagnosticsConfig.TraceLevel.None;
                bool detail =
                    DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose;
                bool diffGate =
                    DiagnosticsConfig.EnableDiffTracing
                    || DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose;

                Assert.AreEqual(row.structural, structural, $"structural @ {row.level}/{row.diff}");
                Assert.AreEqual(row.detail, detail, $"detail @ {row.level}/{row.diff}");
                Assert.AreEqual(row.diffGate, diffGate, $"diff @ {row.level}/{row.diff}");
            }
        }

        // ── Behavioral pins against the real reconciler output ────────────────

        [Test]
        public void TraceNone_DiffOff_IsSilent()
        {
            _renderer.Render(Boxes(3));
            _renderer.Render(Boxes(1));
            _renderer.Render(Swap(image: true, keyed: true));
            _renderer.Render(Swap(image: false, keyed: true)); // a replacement decision
            Assert.AreEqual(0, Count("[Fiber]"), "trace none + diff off must log nothing");
        }

        [Test]
        public void TraceBasic_ProducesStructuralEvents_AndNoDiffDetail()
        {
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.Basic;

            _renderer.Render(Boxes(3));
            Assert.GreaterOrEqual(
                Count("[Fiber] AppendChild") + Count("[Fiber] InsertBefore"),
                4,
                "basic must log placements (area + 3 boxes)"
            );
            Assert.GreaterOrEqual(Count("[Fiber] Commit #"), 1, "basic must log the commit summary");
            Assert.AreEqual(
                0,
                Count("[Fiber] Applying"),
                "props application is diff detail — absent at basic"
            );

            _logs.Clear();
            _renderer.Render(Boxes(1));
            Assert.AreEqual(
                2,
                Count("[Fiber] Delete"),
                "basic must log one line per removed subtree"
            );
        }

        [Test]
        public void TraceBasic_KeyedReplacement_EmitsDistinctReplaceLine()
        {
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.Basic;

            _renderer.Render(Swap(image: true, keyed: true));
            _logs.Clear();
            _renderer.Render(Swap(image: false, keyed: true));

            Assert.AreEqual(
                1,
                Count("[Fiber] Replace"),
                "same key, new type => one structural Replace line (family Basic set)"
            );
            Assert.AreEqual(
                1,
                Count("[Fiber] Delete"),
                "the replaced subtree still logs its own Delete at commit"
            );
        }

        [Test]
        public void TraceBasic_UnkeyedReplacement_EmitsReplaceLine()
        {
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.Basic;

            _renderer.Render(Swap(image: true, keyed: false));
            _logs.Clear();
            _renderer.Render(Swap(image: false, keyed: false));

            Assert.AreEqual(
                1,
                Count("[Fiber] Replace"),
                "occupied slot, new type => one structural Replace line"
            );
        }

        [Test]
        public void DiffTracingAlone_ProducesDiffDetail_AndNoStructuralEvents()
        {
            // §0.1 row 9: diff_tracing is INDEPENDENT — trace none, diff on.
            DiagnosticsConfig.EnableDiffTracing = true;

            _renderer.Render(Boxes(2));
            Assert.GreaterOrEqual(
                Count("[Fiber] Applying"),
                1,
                "diff_tracing alone must produce diff logs (independence)"
            );
            Assert.AreEqual(0, Count("[Fiber] AppendChild") + Count("[Fiber] InsertBefore"));
            Assert.AreEqual(0, Count("[Fiber] Commit #"));
            Assert.AreEqual(0, Count("[Fiber] Delete"));

            _renderer.Render(Swap(image: true, keyed: true));
            _renderer.Render(Swap(image: false, keyed: true)); // a replacement decision
            Assert.AreEqual(
                0,
                Count("[Fiber] Replace"),
                "Replace is structural — absent under diff_tracing alone"
            );
        }

        [Test]
        public void TraceVerbose_LightsStructuralAndDiff()
        {
            // The legacy OR: Verbose alone also lights the diff sites.
            DiagnosticsConfig.CurrentTraceLevel = DiagnosticsConfig.TraceLevel.Verbose;

            _renderer.Render(Boxes(2));
            Assert.GreaterOrEqual(Count("[Fiber] AppendChild") + Count("[Fiber] InsertBefore"), 3);
            Assert.GreaterOrEqual(Count("[Fiber] Applying"), 1);
            Assert.GreaterOrEqual(Count("[Fiber] Commit #"), 1);

            _renderer.Render(Swap(image: true, keyed: true));
            _logs.Clear();
            _renderer.Render(Swap(image: false, keyed: true));
            Assert.AreEqual(
                1,
                Count("[Fiber] Replace"),
                "verbose includes the structural Replace event"
            );
        }
    }
}
