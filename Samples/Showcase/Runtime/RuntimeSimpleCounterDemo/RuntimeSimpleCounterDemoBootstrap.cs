using Ruitk.Core;
using Ruitk.Samples.Components.SimpleCounterFunc;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.SimpleCounterFunc.SimpleCounterFunc;

namespace Ruitk.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeSimpleCounterDemoBootstrap : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null || uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    "RuntimeSimpleCounterDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(SimpleCounterFunc.Render));
        }
    }
}
