using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Element factories for the uGUI backend — the sibling of <c>V</c>.
    /// Tags resolve against the mount's <see cref="UguiElementRegistry"/>;
    /// a uGUI tree is rendered by a <see cref="UguiRootRenderer"/>.
    /// </summary>
    public static class U
    {
        private static VirtualNode Rent(
            string elementTypeName,
            string key,
            UguiBaseProps hostProps,
            VirtualNode[] children
        )
        {
            var v = VirtualNode.__Rent();
            v._nodeType = VirtualNodeType.Element;
            v._elementTypeName = elementTypeName;
            v._key = key;
            v._hostProps = hostProps;
            if (children != null && children.Length > 0)
                v._children = children;
            return v;
        }

        public static VirtualNode Canvas(
            UguiCanvasProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("Canvas", key, props, children);

        public static VirtualNode Panel(
            UguiPanelProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("Panel", key, props, children);

        public static VirtualNode Image(
            UguiImageProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("Image", key, props, children);

        public static VirtualNode RawImage(
            UguiRawImageProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("RawImage", key, props, children);

        public static VirtualNode Text(
            UguiTextProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("Text", key, props, children);

        public static VirtualNode Text(string text, string key = null)
        {
            var props = UguiBaseProps.__Rent<UguiTextProps>();
            props.Text = text ?? string.Empty;
            return Rent("Text", key, props, null);
        }

        public static VirtualNode Button(
            UguiButtonProps props = null,
            string key = null,
            params VirtualNode[] children
        ) => Rent("Button", key, props, children);

        /// <summary>
        /// Portal into any RectTransform in the scene (overlay layers,
        /// user-managed canvases). The uGUI counterpart of
        /// <c>V.Portal(VisualElement, ...)</c>.
        /// </summary>
        public static VirtualNode Portal(
            RectTransform target,
            string key = null,
            params VirtualNode[] children
        )
        {
            var v = VirtualNode.__Rent();
            v._nodeType = VirtualNodeType.Portal;
            v._portalTarget = target != null ? target.gameObject : null;
            v._key = key;
            if (children != null && children.Length > 0)
                v._children = children;
            return v;
        }
    }
}
