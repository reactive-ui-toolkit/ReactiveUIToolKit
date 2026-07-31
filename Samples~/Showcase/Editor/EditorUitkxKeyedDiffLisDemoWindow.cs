#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.KeyedDiffLisDemoFunc;
using Ruitk.Samples.Components.KeyedDiffLisDemoFunc.KeyedDiffLisDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxKeyedDiffLisDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Tests-(13-17-18-26)/Keyed Diff (LIS)")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxKeyedDiffLisDemoWindow>("Keyed Diff (LIS)");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(KeyedDiffLisDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
