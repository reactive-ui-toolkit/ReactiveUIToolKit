#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.GalagaGame;
using Ruitk.Samples.Components.GalagaGame.GalagaGame;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxGalagaGameDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Galaga Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxGalagaGameDemoWindow>("Galaga Game Demo");
            window.minSize = new Vector2(640, 700);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(GalagaGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
