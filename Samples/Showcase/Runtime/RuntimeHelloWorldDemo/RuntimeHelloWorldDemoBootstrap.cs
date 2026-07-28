using Ruitk.Core;
using Ruitk.Samples.Components.HelloWorldFunc;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.HelloWorldFunc.HelloWorldFunc;

namespace Ruitk.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeHelloWorldDemoBootstrap : MonoBehaviour
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
                    "RuntimeHelloWorldDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(HelloWorldFunc.Render));
        }
    }
}
