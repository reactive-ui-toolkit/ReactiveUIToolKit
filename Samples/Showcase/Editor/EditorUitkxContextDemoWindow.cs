#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.ContextDemoFunc;
using Ruitk.Samples.Components.ContextDemoFunc.ContextDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxContextDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Context Demo")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxContextDemoWindow>("Context Demo");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ContextDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
