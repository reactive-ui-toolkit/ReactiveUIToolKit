using Ruitk.Core;
using Ruitk.Samples.Components.SimpleUseEffectFunc;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.SimpleUseEffectFunc.SimpleUseEffectFunc;

namespace Ruitk.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeSimpleUseEffectDemoBootstrap : MonoBehaviour
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
                    "RuntimeSimpleUseEffectDemoBootstrap Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(SimpleUseEffectFunc.Render));
        }
    }
}
