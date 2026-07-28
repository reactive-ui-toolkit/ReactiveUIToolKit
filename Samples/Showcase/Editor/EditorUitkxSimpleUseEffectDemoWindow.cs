#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.SimpleUseEffectFunc;
using Ruitk.Samples.Components.SimpleUseEffectFunc.SimpleUseEffectFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxSimpleUseEffectDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Simple UseEffect")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxSimpleUseEffectDemoWindow>("Simple UseEffect Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SimpleUseEffectFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
