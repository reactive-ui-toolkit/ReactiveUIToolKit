#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.SimpleCounterFunc;
using Ruitk.Samples.Components.SimpleCounterFunc.SimpleCounterFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxSimpleCounterDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Simple Counter")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxSimpleCounterDemoWindow>("Simple Counter Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SimpleCounterFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
