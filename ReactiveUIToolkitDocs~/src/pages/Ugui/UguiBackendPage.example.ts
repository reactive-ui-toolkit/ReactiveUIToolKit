export const UGUI_CSHARP_COUNTER = `using Ruitk;
using Ruitk.Core;
using Ruitk.Ugui;
using UnityEngine;

// Attach to a GameObject with a UguiRootRenderer, under a Canvas.
// An EventSystem must exist in the scene for interaction to work.
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
}`

export const UGUI_UITKX_COUNTER = `@backend ugui

export VirtualNode CounterPanel() {
  var (count, setCount) = useState(0);

  return (
    <VerticalLayoutGroup spacing={8f} anchors={UguiAnchorPreset.MiddleCenter}>
      <Text text={$"Count: {count}"} />
      <Button onClick={() => setCount(count + 1)}>
        <Text text="Increment" />
      </Button>
    </VerticalLayoutGroup>
  );
}`

export const UGUI_PREFAB_MARKUP = `@backend ugui

export VirtualNode Hud(GameObject healthBarPrefab, float health) {
  return (
    <Panel anchors={UguiAnchorPreset.Stretch}>
      <Prefab source={healthBarPrefab} bind={health} />
    </Panel>
  );
}`

export const UGUI_PREFAB_BINDING = `using Ruitk.Ugui;
using UnityEngine;

// Lives on the prefab root. Receives the bind={...} value on mount and
// again every time it changes -- no manual wiring against the tree.
public sealed class HealthBarBinding : MonoBehaviour, IReactivePrefab
{
    [SerializeField] private UnityEngine.UI.Slider bar;

    public void Bind(object props)
    {
        if (props is float health)
        {
            bar.SetValueWithoutNotify(health);
        }
    }
}`

export const UGUI_ISLANDS = `// In a UI Toolkit tree (V.*): embed a uGUI island.
V.VisualElement(
    null,
    null,
    U.UguiHost(new UguiHostProps
    {
        Content = () => U.Text("Rendered by uGUI"),
        SortingOrder = 10,
    })
);

// In a uGUI tree (U.*): embed a UI Toolkit island.
var uitkHost = UguiBaseProps.__Rent<UguiUitkHostProps>();
uitkHost.Content = () => V.Func(SettingsPanel.Render);
uitkHost.SizeDelta = new Vector2(480f, 320f);
U.UitkHost(uitkHost);`
