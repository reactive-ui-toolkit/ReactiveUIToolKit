using ReactiveUITK.Elements;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Registers the cross-backend island elements: "UguiHost" into the
    /// UI Toolkit default registry and "UitkHost" into the uGUI default
    /// registry. Runs automatically before scene load (and on editor load),
    /// so island tags resolve in any mount without user bootstrapping.
    /// </summary>
    public static class UguiIslands
    {
        private static bool s_registered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureRegistered()
        {
            if (s_registered)
            {
                return;
            }
            s_registered = true;
            ElementRegistryProvider.GetDefaultRegistry().Register("UguiHost", new UguiHostAdapter());
            UguiElementRegistryProvider
                .GetDefaultRegistry()
                .Register("UitkHost", new UguiUitkHostAdapter());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EnsureRegisteredInEditor()
        {
            EnsureRegistered();
        }
#endif
    }
}
