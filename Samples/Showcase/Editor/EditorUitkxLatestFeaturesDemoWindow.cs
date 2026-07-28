#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.LatestFeaturesDemoFunc;
using Ruitk.Samples.Components.LatestFeaturesDemoFunc.LatestFeaturesDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxLatestFeaturesDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Latest Features Showcase")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxLatestFeaturesDemoWindow>("Latest Features Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(LatestFeaturesDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
