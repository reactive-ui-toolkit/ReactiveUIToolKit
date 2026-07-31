using Ruitk;
using Ruitk.Core;
using Ruitk.Props.Typed;
using Ruitk.Samples.Components.HmrTests;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.HmrTests.HmrTests;

namespace Ruitk.Samples.FunctionalComponents
{
    public class HmrTestUIBootstrap : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null)
            {
                Debug.LogError("HmrTestUIBootstrap: RootRenderer component missing on GameObject");
                return;
            }
            if (uiDocument == null)
            {
                Debug.LogError("HmrTestUIBootstrap: UIDocument not assigned");
                return;
            }
            rootRenderer.Initialize(uiDocument);
            var hostProps = new VisualElementProps { PickingMode = PickingMode.Ignore };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(HmrTests.Render)));
        }
    }
}
