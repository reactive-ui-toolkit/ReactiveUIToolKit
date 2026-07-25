using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public class UguiImageProps : UguiGraphicProps
    {
        public Sprite Sprite { get; set; }
        public Image.Type? Type { get; set; }
        public Image.FillMethod? FillMethod { get; set; }
        public float? FillAmount { get; set; }
        public int? FillOrigin { get; set; }
        public bool? FillClockwise { get; set; }
        public bool? FillCenter { get; set; }
        public bool? PreserveAspect { get; set; }
        public float? PixelsPerUnitMultiplier { get; set; }
        public bool? UseSpriteMesh { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiImageProps o))
                return false;
            if (!ReferenceEquals(Sprite, o.Sprite))
                return false;
            if (Type != o.Type)
                return false;
            if (FillMethod != o.FillMethod)
                return false;
            if (FillAmount != o.FillAmount)
                return false;
            if (FillOrigin != o.FillOrigin)
                return false;
            if (FillClockwise != o.FillClockwise)
                return false;
            if (FillCenter != o.FillCenter)
                return false;
            if (PreserveAspect != o.PreserveAspect)
                return false;
            if (PixelsPerUnitMultiplier != o.PixelsPerUnitMultiplier)
                return false;
            if (UseSpriteMesh != o.UseSpriteMesh)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Sprite = null;
            Type = null;
            FillMethod = null;
            FillAmount = null;
            FillOrigin = null;
            FillClockwise = null;
            FillCenter = null;
            PreserveAspect = null;
            PixelsPerUnitMultiplier = null;
            UseSpriteMesh = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            if (GetType() == typeof(UguiImageProps))
                Pool<UguiImageProps>.Return(this);
        }
    }
}
