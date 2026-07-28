using UnityEngine.UI;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Shared Image prop application — used by both the Image adapter and the
    /// Button adapter (whose background graphic takes the same prop block).
    /// </summary>
    internal static class UguiImageApplier
    {
        internal static void ApplyDiffOrFull(Image image, UguiImageProps prev, UguiImageProps next)
        {
            if (prev == null)
                ApplyFull(image, next);
            else
                ApplyDiff(image, prev, next);
        }

        internal static void ApplyFull(Image image, UguiImageProps props)
        {
            if (image == null || props == null)
                return;
            if (props.Sprite != null)
                image.sprite = props.Sprite;
            if (props.Type.HasValue)
                image.type = props.Type.Value;
            if (props.FillMethod.HasValue)
                image.fillMethod = props.FillMethod.Value;
            if (props.FillAmount.HasValue)
                image.fillAmount = props.FillAmount.Value;
            if (props.FillOrigin.HasValue)
                image.fillOrigin = props.FillOrigin.Value;
            if (props.FillClockwise.HasValue)
                image.fillClockwise = props.FillClockwise.Value;
            if (props.FillCenter.HasValue)
                image.fillCenter = props.FillCenter.Value;
            if (props.PreserveAspect.HasValue)
                image.preserveAspect = props.PreserveAspect.Value;
            if (props.PixelsPerUnitMultiplier.HasValue)
                image.pixelsPerUnitMultiplier = props.PixelsPerUnitMultiplier.Value;
            if (props.UseSpriteMesh.HasValue)
                image.useSpriteMesh = props.UseSpriteMesh.Value;
            if (props.AlphaHitTestMinimumThreshold.HasValue)
                image.alphaHitTestMinimumThreshold = props.AlphaHitTestMinimumThreshold.Value;
            UguiGraphicApplier.ApplyFull(image, props);
        }

        internal static void ApplyDiff(Image image, UguiImageProps prev, UguiImageProps next)
        {
            if (image == null || next == null)
                return;
            if (prev == null)
            {
                ApplyFull(image, next);
                return;
            }
            if (!ReferenceEquals(next.Sprite, prev.Sprite))
                image.sprite = next.Sprite;
            if (next.Type != prev.Type && next.Type.HasValue)
                image.type = next.Type.Value;
            if (next.FillMethod != prev.FillMethod && next.FillMethod.HasValue)
                image.fillMethod = next.FillMethod.Value;
            if (next.FillAmount != prev.FillAmount && next.FillAmount.HasValue)
                image.fillAmount = next.FillAmount.Value;
            if (next.FillOrigin != prev.FillOrigin && next.FillOrigin.HasValue)
                image.fillOrigin = next.FillOrigin.Value;
            if (next.FillClockwise != prev.FillClockwise && next.FillClockwise.HasValue)
                image.fillClockwise = next.FillClockwise.Value;
            if (next.FillCenter != prev.FillCenter && next.FillCenter.HasValue)
                image.fillCenter = next.FillCenter.Value;
            if (next.PreserveAspect != prev.PreserveAspect && next.PreserveAspect.HasValue)
                image.preserveAspect = next.PreserveAspect.Value;
            if (
                next.PixelsPerUnitMultiplier != prev.PixelsPerUnitMultiplier
                && next.PixelsPerUnitMultiplier.HasValue
            )
                image.pixelsPerUnitMultiplier = next.PixelsPerUnitMultiplier.Value;
            if (next.UseSpriteMesh != prev.UseSpriteMesh && next.UseSpriteMesh.HasValue)
                image.useSpriteMesh = next.UseSpriteMesh.Value;
            if (
                next.AlphaHitTestMinimumThreshold != prev.AlphaHitTestMinimumThreshold
                && next.AlphaHitTestMinimumThreshold.HasValue
            )
                image.alphaHitTestMinimumThreshold = next.AlphaHitTestMinimumThreshold.Value;
            UguiGraphicApplier.ApplyDiff(image, prev, next);
        }
    }
}
