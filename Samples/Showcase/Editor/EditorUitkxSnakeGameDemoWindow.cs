#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ruitk.EditorSupport;
using Ruitk.Samples.Components.SnakeGame;
using Ruitk.Samples.Components.SnakeGame.SnakeGame;

namespace Ruitk.Samples.UITKX.Editor
{
    public sealed class EditorUitkxSnakeGameDemoWindow : EditorWindow
    {
        [MenuItem("Reactive UI Toolkit/Demos/Snake Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxSnakeGameDemoWindow>("Snake Game Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SnakeGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
