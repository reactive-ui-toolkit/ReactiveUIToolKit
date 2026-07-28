namespace Ruitk.EditorSupport
{
    /// <summary>
    /// Top-level Ruitk editor menu items.
    /// </summary>
    public static class FiberMenu
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Reactive UI Toolkit/UI Toolkit Debugger", priority = 9000)]
        private static void OpenUIToolkitDebugger()
        {
            UnityEditor.EditorApplication.ExecuteMenuItem("Window/UI Toolkit/Debugger");
        }
#endif
    }
}
