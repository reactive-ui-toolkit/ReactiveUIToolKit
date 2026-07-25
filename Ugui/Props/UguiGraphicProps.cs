using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Shared prop surface for elements backed by a <c>Graphic</c>
    /// (Image, RawImage, Text). Names mirror the Inspector.
    /// </summary>
    public abstract class UguiGraphicProps : UguiBaseProps
    {
        public Color? Color { get; set; }
        public Material Material { get; set; }
        public bool? RaycastTarget { get; set; }
        public Vector4? RaycastPadding { get; set; }
        public bool? Maskable { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiGraphicProps o))
                return false;
            if (Color != o.Color)
                return false;
            if (!ReferenceEquals(Material, o.Material))
                return false;
            if (RaycastTarget != o.RaycastTarget)
                return false;
            if (RaycastPadding != o.RaycastPadding)
                return false;
            if (Maskable != o.Maskable)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Color = null;
            Material = null;
            RaycastTarget = null;
            RaycastPadding = null;
            Maskable = null;
            base.__ResetFields();
        }
    }
}
