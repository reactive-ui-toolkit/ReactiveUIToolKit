#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.ToggleButtonGroupDemoFunc;
using Ruitk.Samples.Components.ToggleButtonGroupDemoFunc.ToggleButtonGroupDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxToggleButtonGroupDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Toggle Button Group")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxToggleButtonGroupDemoWindow>("Toggle Button Group Demo");
            window.minSize = new Vector2(420, 260);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ToggleButtonGroupDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
