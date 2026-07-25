using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Props for the Canvas element — one element, three components exactly
    /// as a designer thinks of "a canvas": Canvas, CanvasScaler,
    /// GraphicRaycaster. Scaler and raycaster props are ignored by Unity on
    /// nested canvases, matching native behavior.
    /// </summary>
    public sealed class UguiCanvasProps : UguiBaseProps
    {
        public RenderMode? RenderMode { get; set; }
        public int? SortingOrder { get; set; }
        public bool? PixelPerfect { get; set; }
        public bool? OverrideSorting { get; set; }
        public Camera WorldCamera { get; set; }
        public float? PlaneDistance { get; set; }

        public CanvasScaler.ScaleMode? UiScaleMode { get; set; }
        public Vector2? ReferenceResolution { get; set; }
        public CanvasScaler.ScreenMatchMode? ScreenMatchMode { get; set; }
        public float? MatchWidthOrHeight { get; set; }
        public float? ScaleFactor { get; set; }

        public GraphicRaycaster.BlockingObjects? BlockingObjects { get; set; }
        public bool? IgnoreReversedGraphics { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiCanvasProps o))
                return false;
            if (RenderMode != o.RenderMode)
                return false;
            if (SortingOrder != o.SortingOrder)
                return false;
            if (PixelPerfect != o.PixelPerfect)
                return false;
            if (OverrideSorting != o.OverrideSorting)
                return false;
            if (!ReferenceEquals(WorldCamera, o.WorldCamera))
                return false;
            if (PlaneDistance != o.PlaneDistance)
                return false;
            if (UiScaleMode != o.UiScaleMode)
                return false;
            if (ReferenceResolution != o.ReferenceResolution)
                return false;
            if (ScreenMatchMode != o.ScreenMatchMode)
                return false;
            if (MatchWidthOrHeight != o.MatchWidthOrHeight)
                return false;
            if (ScaleFactor != o.ScaleFactor)
                return false;
            if (BlockingObjects != o.BlockingObjects)
                return false;
            if (IgnoreReversedGraphics != o.IgnoreReversedGraphics)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            RenderMode = null;
            SortingOrder = null;
            PixelPerfect = null;
            OverrideSorting = null;
            WorldCamera = null;
            PlaneDistance = null;
            UiScaleMode = null;
            ReferenceResolution = null;
            ScreenMatchMode = null;
            MatchWidthOrHeight = null;
            ScaleFactor = null;
            BlockingObjects = null;
            IgnoreReversedGraphics = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiCanvasProps>.Return(this);
        }
    }
}
