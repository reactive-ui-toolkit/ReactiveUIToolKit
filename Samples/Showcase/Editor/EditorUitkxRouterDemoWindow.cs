#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.RouterDemoFunc;
using Ruitk.Samples.Components.RouterDemoFunc.RouterDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxRouterDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Router")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxRouterDemoWindow>("Router Demo");
            window.minSize = new Vector2(500, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RouterDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
