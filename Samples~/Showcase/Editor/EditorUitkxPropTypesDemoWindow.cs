#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.PropTypesDemoFunc;
using Ruitk.Samples.Components.PropTypesDemoFunc.PropTypesDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxPropTypesDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Tests-(35-37-40)/PropTypes Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxPropTypesDemoWindow>("PropTypes Demo");
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(PropTypesDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
