#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.EditorControlsDemoFunc;
using Ruitk.Samples.Components.EditorControlsDemoFunc.EditorControlsDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxEditorControlsDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Editor Controls")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxEditorControlsDemoWindow>("RUITK Editor Controls");
            window.minSize = new Vector2(600, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(EditorControlsDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
