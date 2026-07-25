using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Signals;
using ReactiveUITK.Ugui;
using UnityEngine;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
    /// <summary>
    /// The uGUI port of Samples/Components/StressTest: N boxes animated for a
    /// chosen duration with a live average-FPS readout. The bootstrap's
    /// Update drives a time signal while running; every tick re-renders the
    /// whole box field through the reconciler — that sustained full-tree diff
    /// IS the stress. Scene setup: Canvas + EventSystem + a stretched
    /// RectTransform with a UguiRootRenderer and this component.
    /// </summary>
    [RequireComponent(typeof(UguiRootRenderer))]
    public sealed class RuntimeUguiStressTestDemoBootstrap : MonoBehaviour
    {
        // Created in Start, never in field initializers: SignalFactory boots
        // the signals runtime host, which Unity forbids during MonoBehaviour
        // construction (type initializers run in constructor context).
        private static Signal<float> s_time;
        private static Signal<bool> s_running;
        private static Signal<string> s_status;

        private static int s_boxCount = 300;
        private static float s_duration = 10f;

        private float _elapsed;
        private int _frames;

        private void Start()
        {
            s_time ??= SignalFactory.Get<float>("UguiStress.Time", 0f);
            s_running ??= SignalFactory.Get<bool>("UguiStress.Running", false);
            s_status ??= SignalFactory.Get<string>(
                "UguiStress.Status",
                "uGUI Stress Test — Ready"
            );
            GetComponent<UguiRootRenderer>().Render(V.Func(StressTest));
        }

        private void Update()
        {
            if (s_running == null || !s_running.Value)
            {
                return;
            }
            _elapsed += Time.deltaTime;
            _frames++;
            float avgFps = _frames / Mathf.Max(0.0001f, _elapsed);

            if (_elapsed >= s_duration)
            {
                s_running.Set(false);
                s_status.Set(
                    $"DONE — {s_boxCount} boxes | Avg FPS: {avgFps:F1} | "
                        + $"Duration: {_elapsed:F1}s | Frames: {_frames}"
                );
                return;
            }

            s_status.Set(
                $"uGUI Stress — {s_boxCount} boxes | Avg FPS: {avgFps:F1} | "
                    + $"Elapsed: {_elapsed:F1}s / {s_duration:F0}s"
            );
            s_time.Set(_elapsed);
        }

        private void StartRun()
        {
            _elapsed = 0f;
            _frames = 0;
            s_time.Set(0f);
            s_running.Set(true);
        }

        private static VirtualNode HeaderLabel(string text)
        {
            var props = UguiBaseProps.__Rent<UguiTextProps>();
            props.Text = text;
            props.FontSize = 14f;
            props.Alignment = TMPro.TextAlignmentOptions.Midline;
            props.LayoutElement = new UguiLayoutElement { MinWidth = 44f };
            return U.Text(props);
        }

        private static VirtualNode ButtonLabel(string text)
        {
            var props = UguiBaseProps.__Rent<UguiTextProps>();
            props.Text = text;
            props.FontSize = 14f;
            props.Color = new Color(0.13f, 0.13f, 0.15f);
            props.Alignment = TMPro.TextAlignmentOptions.Center;
            props.Anchors = UguiAnchorPreset.Stretch;
            props.OffsetMin = Vector2.zero;
            props.OffsetMax = Vector2.zero;
            return U.Text(props);
        }

        private VirtualNode StressTest(IProps props, IReadOnlyList<VirtualNode> children)
        {
            float t = Hooks.UseSignal(s_time);
            bool running = Hooks.UseSignal(s_running);
            string status = Hooks.UseSignal(s_status);
            var (countText, setCountText) = Hooks.UseState("300");
            var (durationText, setDurationText) = Hooks.UseState("10");

            var root = UguiBaseProps.__Rent<UguiPanelProps>();

            var header = UguiBaseProps.__Rent<UguiHorizontalLayoutGroupProps>();
            header.Anchors = UguiAnchorPreset.TopStretch;
            header.SizeDelta = new Vector2(0f, 44f);
            header.Spacing = 8f;
            header.PaddingLeft = 8;
            header.PaddingRight = 8;
            header.PaddingTop = 8;
            header.ChildControlWidth = true;
            header.ChildControlHeight = true;
            header.ChildForceExpandWidth = false;

            var statusProps = UguiBaseProps.__Rent<UguiTextProps>();
            statusProps.Text = status;
            statusProps.FontSize = 16f;
            statusProps.Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            statusProps.LayoutElement = new UguiLayoutElement { FlexibleWidth = 1f };

            var boxesInput = UguiBaseProps.__Rent<UguiInputFieldProps>();
            boxesInput.Text = countText;
            boxesInput.OnValueChanged = v => setCountText(v);
            boxesInput.LayoutElement = new UguiLayoutElement { MinWidth = 80f, MinHeight = 28f };

            var durationInput = UguiBaseProps.__Rent<UguiInputFieldProps>();
            durationInput.Text = durationText;
            durationInput.OnValueChanged = v => setDurationText(v);
            durationInput.LayoutElement = new UguiLayoutElement { MinWidth = 60f, MinHeight = 28f };

            var start = UguiBaseProps.__Rent<UguiButtonProps>();
            start.Interactable = !running;
            start.OnClick = () =>
            {
                if (running)
                {
                    return;
                }
                if (
                    int.TryParse(countText, out int n) && n > 0 && n <= 10000
                    && float.TryParse(durationText, out float dur) && dur > 0f
                )
                {
                    s_boxCount = n;
                    s_duration = dur;
                    StartRun();
                }
            };
            start.LayoutElement = new UguiLayoutElement { MinWidth = 90f, MinHeight = 28f };

            var area = UguiBaseProps.__Rent<UguiImageProps>();
            area.Anchors = UguiAnchorPreset.Stretch;
            area.OffsetMin = new Vector2(0f, 0f);
            area.OffsetMax = new Vector2(0f, -52f);
            area.Color = new Color(0.09f, 0.1f, 0.13f);

            int boxCount = running ? s_boxCount : 0;
            var boxes = new VirtualNode[boxCount];
            for (int i = 0; i < boxCount; i++)
            {
                var box = UguiBaseProps.__Rent<UguiImageProps>();
                box.Anchors = UguiAnchorPreset.BottomLeft;
                box.SizeDelta = new Vector2(18f, 18f);
                box.AnchoredPosition = new Vector2(
                    Mathf.Abs(Mathf.Sin(t * 0.9f + i * 0.37f)) * (Screen.width - 20f),
                    Mathf.Abs(Mathf.Cos(t * 1.3f + i * 0.11f)) * (Screen.height - 80f)
                );
                box.Color = Color.HSVToRGB(i % 32 / 32f, 0.7f, 1f);
                boxes[i] = U.Image(box, $"box-{i}");
            }

            return U.Panel(
                root,
                null,
                U.HorizontalLayoutGroup(
                    header,
                    "header",
                    U.Text(statusProps),
                    HeaderLabel("Boxes:"),
                    U.InputField(boxesInput),
                    HeaderLabel("Sec:"),
                    U.InputField(durationInput),
                    U.Button(start, null, ButtonLabel(running ? "Running..." : "Start"))
                ),
                U.Image(area, "area", boxes)
            );
        }
    }
}
