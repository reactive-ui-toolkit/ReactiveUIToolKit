#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.RefForwardingDemoFunc;
using Ruitk.Samples.Components.RefForwardingDemoFunc.RefForwardingDemoFunc;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxRefForwardingDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Ref Forwarding + useRef")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxRefForwardingDemoWindow>("Ref Forwarding Demo");
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RefForwardingDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
