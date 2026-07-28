#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.SignalCounterDemoFunc;
using Ruitk.Samples.Components.SignalCounterDemoFunc.SignalCounterDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxSignalCounterDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Signal Counter")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxSignalCounterDemoWindow>("Signal Counter Demo");
            window.minSize = new Vector2(360, 260);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SignalCounterDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
