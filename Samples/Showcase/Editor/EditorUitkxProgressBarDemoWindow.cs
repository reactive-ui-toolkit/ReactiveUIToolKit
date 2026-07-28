#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.ProgressBarDemoFunc;
using Ruitk.Samples.Components.ProgressBarDemoFunc.ProgressBarDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxProgressBarDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Progress Bar")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxProgressBarDemoWindow>("Progress Bar Demo");
            window.minSize = new Vector2(420, 240);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ProgressBarDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
