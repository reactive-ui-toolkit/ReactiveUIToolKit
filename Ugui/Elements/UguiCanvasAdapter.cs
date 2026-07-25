using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiCanvasAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject(
                "Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            var tag = UguiNodeTag.GetOrAdd(go);
            tag.Control = go.GetComponent<Canvas>();
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            Apply(go, null, props as UguiCanvasProps);
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            Apply(go, prev as UguiCanvasProps, next as UguiCanvasProps);
            base.ApplyTypedDiff(go, prev, next);
        }

        private static void Apply(GameObject go, UguiCanvasProps prev, UguiCanvasProps next)
        {
            if (next == null)
                return;
            bool full = prev == null;

            var tag = UguiNodeTag.Find(go);
            var canvas = tag != null ? tag.Control as Canvas : go.GetComponent<Canvas>();
            if (canvas != null)
            {
                if ((full || next.RenderMode != prev.RenderMode) && next.RenderMode.HasValue)
                    canvas.renderMode = next.RenderMode.Value;
                if ((full || next.SortingOrder != prev.SortingOrder) && next.SortingOrder.HasValue)
                    canvas.sortingOrder = next.SortingOrder.Value;
                if (
                    (full || next.SortingLayerName != prev.SortingLayerName)
                    && next.SortingLayerName != null
                )
                    canvas.sortingLayerName = next.SortingLayerName;
                if ((full || next.TargetDisplay != prev.TargetDisplay) && next.TargetDisplay.HasValue)
                    canvas.targetDisplay = next.TargetDisplay.Value;
                if (
                    (full || next.AdditionalShaderChannels != prev.AdditionalShaderChannels)
                    && next.AdditionalShaderChannels.HasValue
                )
                    canvas.additionalShaderChannels = next.AdditionalShaderChannels.Value;
                if ((full || next.PixelPerfect != prev.PixelPerfect) && next.PixelPerfect.HasValue)
                    canvas.pixelPerfect = next.PixelPerfect.Value;
                if (
                    (full || next.OverrideSorting != prev.OverrideSorting)
                    && next.OverrideSorting.HasValue
                )
                    canvas.overrideSorting = next.OverrideSorting.Value;
                if (full || !ReferenceEquals(next.WorldCamera, prev.WorldCamera))
                    canvas.worldCamera = next.WorldCamera;
                if ((full || next.PlaneDistance != prev.PlaneDistance) && next.PlaneDistance.HasValue)
                    canvas.planeDistance = next.PlaneDistance.Value;
            }

            var scaler = go.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                if ((full || next.UiScaleMode != prev.UiScaleMode) && next.UiScaleMode.HasValue)
                    scaler.uiScaleMode = next.UiScaleMode.Value;
                if (
                    (full || next.ReferenceResolution != prev.ReferenceResolution)
                    && next.ReferenceResolution.HasValue
                )
                    scaler.referenceResolution = next.ReferenceResolution.Value;
                if (
                    (full || next.ScreenMatchMode != prev.ScreenMatchMode)
                    && next.ScreenMatchMode.HasValue
                )
                    scaler.screenMatchMode = next.ScreenMatchMode.Value;
                if (
                    (full || next.MatchWidthOrHeight != prev.MatchWidthOrHeight)
                    && next.MatchWidthOrHeight.HasValue
                )
                    scaler.matchWidthOrHeight = next.MatchWidthOrHeight.Value;
                if ((full || next.ScaleFactor != prev.ScaleFactor) && next.ScaleFactor.HasValue)
                    scaler.scaleFactor = next.ScaleFactor.Value;
            }

            var raycaster = go.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                if (
                    (full || next.BlockingObjects != prev.BlockingObjects)
                    && next.BlockingObjects.HasValue
                )
                    raycaster.blockingObjects = next.BlockingObjects.Value;
                if (
                    (full || next.IgnoreReversedGraphics != prev.IgnoreReversedGraphics)
                    && next.IgnoreReversedGraphics.HasValue
                )
                    raycaster.ignoreReversedGraphics = next.IgnoreReversedGraphics.Value;
            }
        }
    }
}
