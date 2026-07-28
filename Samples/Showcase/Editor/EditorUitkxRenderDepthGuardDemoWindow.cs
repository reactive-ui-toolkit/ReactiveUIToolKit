#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.RenderDepthGuardDemoFunc;
using Ruitk.Samples.Components.RenderDepthGuardDemoFunc.RenderDepthGuardDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxRenderDepthGuardDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Tests-(Core-Fixes)/Render Depth Guard")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxRenderDepthGuardDemoWindow>("Render Depth Guard");
            window.minSize = new Vector2(540, 480);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RenderDepthGuardDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
