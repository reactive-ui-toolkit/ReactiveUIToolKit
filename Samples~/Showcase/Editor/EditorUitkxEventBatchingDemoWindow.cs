#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.EventBatchingDemoFunc;
using Ruitk.Samples.Components.EventBatchingDemoFunc.EventBatchingDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxEventBatchingDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Tests-(13-17-18-26)/Event Batching")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxEventBatchingDemoWindow>("Event Batching Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(EventBatchingDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
