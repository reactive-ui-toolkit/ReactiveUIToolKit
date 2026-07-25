using ReactiveUITK.Core;
using ReactiveUITK.Samples.Components.UguiStressTest.UguiStressTest;
using ReactiveUITK.Ugui;
using UnityEngine;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
    /// <summary>
    /// Mounts the UguiStressTest component — the uGUI twin of
    /// Samples/Components/StressTest, written in .uitkx with the same
    /// hook-driven flow (useState boxes + a ticker effect integrating
    /// seed-42 physics). Scene setup: Canvas + EventSystem + a stretched
    /// RectTransform with a UguiRootRenderer and this component.
    /// </summary>
    [RequireComponent(typeof(UguiRootRenderer))]
    public sealed class RuntimeUguiStressTestDemoBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }

        private void Start()
        {
            GetComponent<UguiRootRenderer>().Render(V.Func(UguiStressTest.Render));
        }
    }
}
