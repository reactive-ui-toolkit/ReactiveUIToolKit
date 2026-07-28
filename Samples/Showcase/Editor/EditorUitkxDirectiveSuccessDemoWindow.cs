#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.DirectiveSuccessDemo;
using Ruitk.Samples.Components.DirectiveSuccessDemo.DirectiveSuccessDemo;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxDirectiveSuccessDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Directive Success")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxDirectiveSuccessDemoWindow>("Directive Success");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(DirectiveSuccessDemo.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
