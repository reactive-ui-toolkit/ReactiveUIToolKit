#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.UitkxCounterFunc;
using Ruitk.Samples.Components.UitkxCounterFunc.UitkxCounterFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxCounterDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Counter")]
        public static void ShowWindow()
        {
            EditorUitkxCounterDemoWindow window = GetWindow<EditorUitkxCounterDemoWindow>(
                "UITKX Counter Demo"
            );
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(UitkxCounterFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
