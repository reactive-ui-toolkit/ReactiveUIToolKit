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

#if UNITY_EDITOR
        private int _lastReportedSecond = -1;

        private void Update()
        {
            int second = Mathf.FloorToInt(Time.unscaledTime);
            if (second == _lastReportedSecond)
            {
                return;
            }
            _lastReportedSecond = second;
            int ops =
                UguiHostConfig.DebugCreated
                + UguiHostConfig.DebugAppended
                + UguiHostConfig.DebugInserted
                + UguiHostConfig.DebugRemoved;
            if (ops == 0)
            {
                return;
            }
            int staging =
                UguiHostConfig.DebugStagingRoot != null
                    ? UguiHostConfig.DebugStagingRoot.childCount
                    : -1;
            Debug.Log(
                $"[UguiStress] staging={staging} created={UguiHostConfig.DebugCreated} "
                    + $"appended={UguiHostConfig.DebugAppended} inserted={UguiHostConfig.DebugInserted} "
                    + $"removed={UguiHostConfig.DebugRemoved} pooled={UguiHostConfig.DebugPooled} "
                    + $"destroyed={UguiHostConfig.DebugDestroyed}"
            );
            UguiHostConfig.DebugCreated = 0;
            UguiHostConfig.DebugAppended = 0;
            UguiHostConfig.DebugInserted = 0;
            UguiHostConfig.DebugRemoved = 0;
            UguiHostConfig.DebugPooled = 0;
            UguiHostConfig.DebugDestroyed = 0;
        }
#endif
    }
}
