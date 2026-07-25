using System.Collections.Generic;

namespace ReactiveUITK.Ugui
{
    /// <summary>Tag-name → adapter map for the uGUI backend (one per vocabulary).</summary>
    public sealed class UguiElementRegistry
    {
        private readonly Dictionary<string, UguiElementAdapter> _adapters =
            new Dictionary<string, UguiElementAdapter>();

        public void Register(string elementType, UguiElementAdapter adapter)
        {
            if (string.IsNullOrEmpty(elementType) || adapter == null)
                return;
            _adapters[elementType] = adapter;
        }

        public UguiElementAdapter Resolve(string elementType)
        {
            return elementType != null && _adapters.TryGetValue(elementType, out var adapter)
                ? adapter
                : null;
        }
    }

    public static class UguiElementRegistryProvider
    {
        private static UguiElementRegistry s_default;

        public static UguiElementRegistry GetDefaultRegistry()
        {
            if (s_default != null)
                return s_default;

            var registry = new UguiElementRegistry();
            registry.Register("Canvas", new UguiCanvasAdapter());
            registry.Register("Panel", new UguiPanelAdapter());
            registry.Register("Image", new UguiImageAdapter());
            registry.Register("RawImage", new UguiRawImageAdapter());

            var text = new UguiTextAdapter();
            registry.Register("Text", text);
            // V.Text / U.Text plain-text nodes lower to the "Label" element
            // with a {"text": ...} dictionary — same adapter serves both.
            registry.Register("Label", text);

            registry.Register("Button", new UguiButtonAdapter());

            registry.Register("HorizontalLayoutGroup", new UguiHorizontalLayoutGroupAdapter());
            registry.Register("VerticalLayoutGroup", new UguiVerticalLayoutGroupAdapter());
            registry.Register("GridLayoutGroup", new UguiGridLayoutGroupAdapter());

            registry.Register("Toggle", new UguiToggleAdapter());
            registry.Register("ToggleGroup", new UguiToggleGroupAdapter());
            registry.Register("Slider", new UguiSliderAdapter());
            registry.Register("Scrollbar", new UguiScrollbarAdapter());
            registry.Register("ScrollRect", new UguiScrollRectAdapter());
            registry.Register("Dropdown", new UguiDropdownAdapter());
            registry.Register("InputField", new UguiInputFieldAdapter());

            s_default = registry;
            return s_default;
        }
    }
}
