using Ruitk;
using Ruitk.Core;
using Ruitk.Ugui;
using UnityEngine;

namespace Ruitk.Samples.UguiDemo
{
    /// <summary>
    /// Minimal uGUI backend demo: attach to a GameObject with a
    /// UguiRootRenderer under a Canvas (with an EventSystem in the scene).
    /// Renders a counter panel built entirely from uGUI-native props —
    /// anchor presets, LayoutGroups, TMP text, a wired Button.
    /// </summary>
    [RequireComponent(typeof(UguiRootRenderer))]
    public sealed class UguiCounterDemoBootstrap : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<UguiRootRenderer>().Render(V.Func(Counter));
        }

        private static VirtualNode Counter(
            IProps props,
            System.Collections.Generic.IReadOnlyList<VirtualNode> children
        )
        {
            var (count, setCount) = Hooks.UseState(0);

            var panel = UguiBaseProps.__Rent<UguiVerticalLayoutGroupProps>();
            panel.Anchors = UguiAnchorPreset.MiddleCenter;
            panel.SizeDelta = new Vector2(320f, 0f);
            panel.Spacing = 12f;
            panel.PaddingLeft = 16;
            panel.PaddingRight = 16;
            panel.PaddingTop = 16;
            panel.PaddingBottom = 16;
            panel.ChildControlWidth = true;
            panel.ChildControlHeight = true;
            panel.ContentSizeFitter = new UguiContentSizeFitter
            {
                VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize,
            };

            var label = UguiBaseProps.__Rent<UguiTextProps>();
            label.Text = $"Count: {count}";
            label.FontSize = 28f;
            label.Alignment = TMPro.TextAlignmentOptions.Center;

            var buttonProps = UguiBaseProps.__Rent<UguiButtonProps>();
            buttonProps.OnClick = () => setCount(count + 1);
            buttonProps.LayoutElement = new UguiLayoutElement { MinHeight = 40f };

            return U.VerticalLayoutGroup(
                panel,
                null,
                U.Text(label),
                U.Button(buttonProps, null, U.Text("Increment"))
            );
        }
    }
}
