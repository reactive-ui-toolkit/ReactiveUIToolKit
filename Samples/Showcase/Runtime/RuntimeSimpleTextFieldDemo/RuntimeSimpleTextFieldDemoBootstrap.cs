using Ruitk.Core;
using Ruitk.Samples.Components.SimpleTextFieldFunc;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.Samples.Components.SimpleTextFieldFunc.SimpleTextFieldFunc;

namespace Ruitk.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeSimpleTextFieldDemoBootstrap : MonoBehaviour
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
                    "RuntimeSimpleTextFieldDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(SimpleTextFieldFunc.Render));
        }
    }
}
