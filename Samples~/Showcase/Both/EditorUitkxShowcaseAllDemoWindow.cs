#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.ShowcaseDemoPage;
using Ruitk.Samples.Components.ShowcaseDemoPage.ShowcaseDemoPage;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxShowcaseAllDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Showcase All")]
        public static void ShowWindow()
        {
            EditorUitkxShowcaseAllDemoWindow window = GetWindow<EditorUitkxShowcaseAllDemoWindow>(
                "Ruitk UITKX Showcase Demo"
            );
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                hostElement,
                V.Func(ShowcaseDemoPage.Render, key: "showcase-demo-page")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
