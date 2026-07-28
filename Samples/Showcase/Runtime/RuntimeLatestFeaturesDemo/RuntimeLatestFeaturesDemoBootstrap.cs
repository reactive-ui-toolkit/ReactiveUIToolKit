using Ruitk.Core;
using Ruitk.Samples.Components.LatestFeaturesDemoFunc;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.LatestFeaturesDemoFunc.LatestFeaturesDemoFunc;

namespace Ruitk.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeLatestFeaturesDemoBootstrap : MonoBehaviour
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
                    "RuntimeLatestFeaturesDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }

            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(LatestFeaturesDemoFunc.Render));
        }
    }
}
