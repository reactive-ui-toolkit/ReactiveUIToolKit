#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.HookStateQueueDemoFunc;
using Ruitk.Samples.Components.HookStateQueueDemoFunc.HookStateQueueDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxHookQueueDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Tests-(13-17-18-26)/Hook Queue Merge")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxHookQueueDemoWindow>("Hook Queue Merge Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(HookStateQueueDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
