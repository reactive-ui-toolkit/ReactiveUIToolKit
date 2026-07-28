#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.DoomGame;
using Ruitk.Samples.Components.DoomGame.DoomGame;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxDoomGameDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Doom Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxDoomGameDemoWindow>("Doom Game Demo");
            window.minSize = new Vector2(660, 500);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(DoomGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
