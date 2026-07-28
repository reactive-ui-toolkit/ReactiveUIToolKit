using System;
using System.Reflection;
using Ruitk.Bench;
using Ruitk.Core;
using Ruitk.Samples.Components.ShowcaseDemoPage;
using Ruitk.Samples.Components.ShowcaseDemoPage.ShowcaseDemoPage;
using UnityEngine;

namespace Ruitk.Benchmark
{
    [DefaultExecutionOrder(-1000)]
    public sealed class BenchmarkSetup : MonoBehaviour
    {
        private void Awake()
        {
            try
            {
                BenchSharedHost.SharedDemoRenderer = () => V.Func(ShowcaseDemoPage.Render);
                Debug.Log("[BenchEditorHost] SharedDemo hook set.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BenchmarkSetup] Failed to set SharedDemo hook: " + e.Message);
            }
        }
    }
}
