#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Shared;
using Ruitk.Samples.Shared.AnimationsDemoPage;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxAnimationsDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Animations")]
        public static void ShowWindow()
        {
            EditorUitkxAnimationsDemoWindow window = GetWindow<EditorUitkxAnimationsDemoWindow>(
                "UITKX Animations Demo"
            );
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                hostElement,
                V.Func(AnimationsDemoPage.Render, key: "animations-demo")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
