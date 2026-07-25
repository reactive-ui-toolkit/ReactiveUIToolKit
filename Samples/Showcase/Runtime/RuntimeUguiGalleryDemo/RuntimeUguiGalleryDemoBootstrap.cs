using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Core;
using ReactiveUITK.Ugui;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
    /// <summary>
    /// uGUI interaction gallery: slider, mute toggle, dropdown, input field,
    /// and a keyed scroll list — every control state-driven through hooks.
    /// Scene setup: Canvas + EventSystem, a stretched RectTransform under the
    /// Canvas carrying a UguiRootRenderer, and this component beside it.
    /// </summary>
    [RequireComponent(typeof(UguiRootRenderer))]
    public sealed class RuntimeUguiGalleryDemoBootstrap : MonoBehaviour
    {
        private static readonly string[] s_flavors = { "Vanilla", "Chocolate", "Pistachio" };
        private static readonly Color s_ink = new Color(0.13f, 0.13f, 0.15f);
        private static readonly Color s_panel = new Color(0.92f, 0.93f, 0.95f);

        private void Start()
        {
            GetComponent<UguiRootRenderer>().Render(V.Func(Gallery));
        }

        private static VirtualNode ButtonLabel(string text)
        {
            var props = UguiBaseProps.__Rent<UguiTextProps>();
            props.Text = text;
            props.FontSize = 16f;
            props.Color = s_ink;
            props.Alignment = TMPro.TextAlignmentOptions.Center;
            props.Anchors = UguiAnchorPreset.Stretch;
            props.OffsetMin = Vector2.zero;
            props.OffsetMax = Vector2.zero;
            return U.Text(props);
        }

        private static VirtualNode Gallery(
            IProps props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (volume, setVolume) = Hooks.UseState(0.5f);
            var (muted, setMuted) = Hooks.UseState(false);
            var (flavor, setFlavor) = Hooks.UseState(0);
            var (nameText, setNameText) = Hooks.UseState(string.Empty);
            var (rows, setRows) = Hooks.UseState(3);

            var root = UguiBaseProps.__Rent<UguiVerticalLayoutGroupProps>();
            root.Anchors = UguiAnchorPreset.Stretch;
            root.OffsetMin = new Vector2(24f, 24f);
            root.OffsetMax = new Vector2(-24f, -24f);
            root.Spacing = 10f;
            root.ChildControlWidth = true;
            root.ChildControlHeight = true;
            root.ChildForceExpandHeight = false;

            var header = UguiBaseProps.__Rent<UguiTextProps>();
            header.Text =
                $"uGUI Gallery — volume {(muted ? "muted" : volume.ToString("F2"))}, "
                + $"flavor {s_flavors[flavor]}, name '{nameText}', rows {rows}";
            header.FontSize = 20f;
            header.LayoutElement = new UguiLayoutElement { MinHeight = 30f };

            var slider = UguiBaseProps.__Rent<UguiSliderProps>();
            slider.Value = volume;
            slider.OnValueChanged = v => setVolume(v);
            slider.Interactable = !muted;
            slider.LayoutElement = new UguiLayoutElement { MinHeight = 24f };

            var mute = UguiBaseProps.__Rent<UguiToggleProps>();
            mute.IsOn = muted;
            mute.OnValueChanged = v => setMuted(v);
            mute.LayoutElement = new UguiLayoutElement { MinHeight = 24f };

            var muteLabel = UguiBaseProps.__Rent<UguiTextProps>();
            muteLabel.Text = "Mute";
            muteLabel.FontSize = 16f;
            muteLabel.Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            muteLabel.Anchors = UguiAnchorPreset.Stretch;
            muteLabel.OffsetMin = new Vector2(28f, 0f);
            muteLabel.OffsetMax = Vector2.zero;

            var dropdown = UguiBaseProps.__Rent<UguiDropdownProps>();
            dropdown.Options = s_flavors;
            dropdown.Value = flavor;
            dropdown.OnValueChanged = v => setFlavor(v);
            dropdown.LayoutElement = new UguiLayoutElement { MinHeight = 32f };

            var input = UguiBaseProps.__Rent<UguiInputFieldProps>();
            input.Text = nameText;
            input.Placeholder = "Type a name...";
            input.OnValueChanged = v => setNameText(v);
            input.LayoutElement = new UguiLayoutElement { MinHeight = 32f };

            var addRow = UguiBaseProps.__Rent<UguiButtonProps>();
            addRow.OnClick = () => setRows(rows + 1);
            addRow.LayoutElement = new UguiLayoutElement { MinHeight = 32f };

            var removeRow = UguiBaseProps.__Rent<UguiButtonProps>();
            removeRow.OnClick = () => setRows(Mathf.Max(0, rows - 1));
            removeRow.Interactable = rows > 0;
            removeRow.LayoutElement = new UguiLayoutElement { MinHeight = 32f };

            var scroll = UguiBaseProps.__Rent<UguiScrollRectProps>();
            scroll.Vertical = true;
            scroll.Horizontal = false;
            scroll.ShowVerticalScrollbar = true;
            scroll.Color = s_panel;
            scroll.LayoutElement = new UguiLayoutElement { FlexibleHeight = 1f, MinHeight = 120f };

            var content = UguiBaseProps.__Rent<UguiVerticalLayoutGroupProps>();
            content.Anchors = UguiAnchorPreset.TopStretch;
            content.Spacing = 4f;
            content.PaddingLeft = 8;
            content.PaddingRight = 8;
            content.PaddingTop = 6;
            content.PaddingBottom = 6;
            content.ChildControlWidth = true;
            content.ChildControlHeight = true;
            content.ChildForceExpandHeight = false;
            content.ContentSizeFitter = new UguiContentSizeFitter
            {
                VerticalFit = ContentSizeFitter.FitMode.PreferredSize,
            };

            var rowNodes = Enumerable
                .Range(0, rows)
                .Select(i =>
                {
                    var row = UguiBaseProps.__Rent<UguiTextProps>();
                    row.Text = $"Row {i} of {rows} — {s_flavors[flavor]}";
                    row.FontSize = 16f;
                    row.Color = s_ink;
                    row.Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                    row.LayoutElement = new UguiLayoutElement { MinHeight = 22f };
                    return U.Text(row, $"row-{i}");
                })
                .ToArray();

            return U.VerticalLayoutGroup(
                root,
                null,
                U.Text(header),
                U.Slider(slider),
                U.Toggle(mute, null, U.Text(muteLabel)),
                U.Dropdown(dropdown),
                U.InputField(input),
                U.Button(addRow, null, ButtonLabel("Add row")),
                U.Button(removeRow, null, ButtonLabel("Remove row")),
                U.ScrollRect(scroll, null, U.VerticalLayoutGroup(content, "content", rowNodes))
            );
        }
    }
}
