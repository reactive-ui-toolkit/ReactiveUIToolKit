#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.HelloWorldFunc;
using Ruitk.Samples.Components.HelloWorldFunc.HelloWorldFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxHelloWorldDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Hello World")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxHelloWorldDemoWindow>("Hello World Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(HelloWorldFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
