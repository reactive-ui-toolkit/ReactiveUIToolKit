using Ruitk.Core;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Ruitk.Samples.Components.PanelRendererDemos.PanelNestedChildFunc;
using Ruitk.Samples.Components.PanelRendererDemos.PanelNestedParentFunc;
using UnityEngine.UIElements;
#endif

namespace Ruitk.Samples.Showcase.Runtime
{
    /// <summary>
    /// Sample 3 - nested PanelRenderers (Unity 6.5+): the live regression for
    /// the shipped 6000.5.x workarounds. Scene setup: a parent GameObject with
    /// a PanelRenderer (Panel Settings assigned) and a CHILD GameObject under
    /// it with its own PanelRenderer - Unity auto-adopts it (parentUI).
    /// Put this bootstrap on the parent, assign both fields, and add TWO
    /// RootRenderers (one on each GameObject).
    ///
    /// What to watch for in the editor: the child mounting at all exercises
    /// the mount watchdog (case IN-150082 - without it the child's UI can
    /// silently never appear); the one-time parentUI warning names the
    /// limitation; and if a parent rebuild releases the child's tree
    /// (UUM-148452), the child repairs itself - its counter resetting to 0 is
    /// the visible proof a repair-and-remount happened.
    /// </summary>
    public class PanelNestedDemoBootstrap : MonoBehaviour
    {
#if UNITY_6000_5_OR_NEWER
        [SerializeField]
        private PanelRenderer parentRenderer;

        [SerializeField]
        private PanelRenderer childRenderer;

        [SerializeField]
        private RootRenderer parentMount;

        [SerializeField]
        private RootRenderer childMount;

        private void Awake()
        {
            if (
                parentRenderer == null
                || childRenderer == null
                || parentMount == null
                || childMount == null
            )
            {
                Debug.LogError(
                    "PanelNestedDemoBootstrap: assign the parent and child PanelRenderers "
                        + "and their RootRenderers in the Inspector.",
                    this
                );
                return;
            }
            parentMount.Initialize(parentRenderer);
            parentMount.Render(V.Func(PanelNestedParentFunc.Render));
            childMount.Initialize(childRenderer);
            childMount.Render(V.Func(PanelNestedChildFunc.Render));
        }
#else
        private void Awake()
        {
            Debug.LogWarning(
                "PanelNestedDemoBootstrap: the PanelRenderer host requires Unity 6000.5 or newer.",
                this
            );
        }
#endif
    }
}
